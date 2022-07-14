using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using InterSystems.Data.IRISClient;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemProvider : AdoProvider
    {
        public InterSystemProvider()
        {
            
        }
        public override IDbConnection Connection
        {
            get
            {
                if (base._DbConnection == null)
                {
                    try
                    {
                        base._DbConnection = new IRISConnection(base.Context.CurrentConnectionConfig.ConnectionString);
                    }
                    catch (Exception ex)
                    {
                        Check.Exception(true, ex.Message);
                    }
                }
                return base._DbConnection;
            }
            set
            {
                base._DbConnection = value;
            }
        }

        public override void BeginTran(string transactionName)
        {
            base.BeginTran();
        }




        /// <summary>
        /// Only SqlServer
        /// </summary>
        /// <param name="iso"></param>
        /// <param name="transactionName"></param>
        public override void BeginTran(IsolationLevel iso, string transactionName)
        {
            base.BeginTran(iso);
        }
        public override IDataAdapter GetAdapter()
        {
            return new InterSystemDataAdapter();
        }
        public override DbCommand GetCommand(string sql, SugarParameter[] parameters)
        {
            IRISCommand sqlCommand = new IRISCommand(sql, (IRISConnection)this.Connection);
            sqlCommand.CommandType = this.CommandType;
            sqlCommand.CommandTimeout = this.CommandTimeOut;
           

            if (this.Transaction != null)
            {
                sqlCommand.Transaction = (IRISTransaction)this.Transaction;
            }
            if (parameters.HasValue())
            {
                IDataParameter[] ipars = ToIDbDataParameter(parameters);
                sqlCommand.Parameters.AddRange((IRISParameter[])ipars);
            }

            CheckConnection();
            return sqlCommand;
        }
        public override void SetCommandToAdapter(IDataAdapter dataAdapter, DbCommand command)
        {
            ((InterSystemDataAdapter)dataAdapter).SelectCommand = (IRISCommand)command;
        }

        private string FormatSQL(string sql ,params SugarParameter[] parameters)
        {
            if(parameters?.HasValue()==false)
                return sql;

            foreach(SugarParameter parameter in parameters)
            {
                if(parameter.HasValue())
                {
                    sql = sql.Replace(parameter.ParameterName, FormateValue(parameter.Value));
                }
            }

            return sql;
        }

        private string FormateValue(object value)
        {
            return FormatValueInSQL.Format(value).Result;
        }

        /// <summary>
        /// ExecuteCommand
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override int ExecuteCommand(string sql, params SugarParameter[] parameters)
        {
            try
            {
                InitParameters(ref sql, parameters);

                //sql = FormatSQL(sql, parameters);
                
                SetConnectionStart(sql);
                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);
                ExecuteBefore(sql, parameters);
                IDbCommand sqlCommand = GetCommand(sql, parameters);
                int count = sqlCommand.ExecuteNonQuery();
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                sqlCommand.Dispose();
                return count;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw ex;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                SetConnectionEnd(sql);
            }
        }

        public override IDataReader GetDataReader(string sql, params SugarParameter[] parameters)
        {
            try
            {
                InitParameters(ref sql, parameters);
                if (IsFormat(parameters))
                    sql = FormatSql(sql);
                SetConnectionStart(sql);
                var isSp = this.CommandType == CommandType.StoredProcedure;
                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);
                ExecuteBefore(sql, parameters);
                IDbCommand sqlCommand = GetCommand(sql, parameters);
                IDataReader sqlDataReader = sqlCommand.ExecuteReader(this.IsAutoClose() ? CommandBehavior.CloseConnection : CommandBehavior.Default);
                if (isSp)
                    DataReaderParameters = sqlCommand.Parameters;
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                SetConnectionEnd(sql);
                return sqlDataReader;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw ex;
            }
        }

        private bool IsFormat(SugarParameter[] parameters)
        {
            return FormatSql != null && parameters != null && parameters.Length > 0;
        }


        /// <summary>
        /// if IRIS return IRISParameter[] pars
        /// if sqlerver return SqlParameter[] pars ...
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override IDataParameter[] ToIDbDataParameter(params SugarParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return null;
            IRISParameter[] result = new IRISParameter[parameters.Length];
            int index = 0;
            var isVarchar = IsVarchar();
            foreach (var parameter in parameters)
            {
                if (parameter.Value == null) parameter.Value = DBNull.Value;
                var sqlParameter = new IRISParameter();
                sqlParameter.ParameterName = parameter.ParameterName;
                sqlParameter.Size = parameter.Size;
                sqlParameter.Value = parameter.Value;
                sqlParameter.DbType = parameter.DbType;
                if (parameter.Direction == 0)
                {
                    parameter.Direction = ParameterDirection.Input;
                }
                sqlParameter.Direction = parameter.Direction;
                //if (sqlParameter.Direction == 0)
                //{
                //    sqlParameter.Direction = ParameterDirection.Input;
                //}
                result[index] = sqlParameter;
                if (sqlParameter.Direction.IsIn(ParameterDirection.Output, ParameterDirection.InputOutput, ParameterDirection.ReturnValue))
                {
                    if (this.OutputParameters == null) this.OutputParameters = new List<IDataParameter>();
                    this.OutputParameters.RemoveAll(it => it.ParameterName == sqlParameter.ParameterName);
                    this.OutputParameters.Add(sqlParameter);
                }
                if (isVarchar && sqlParameter.DbType == System.Data.DbType.String)
                {
                    sqlParameter.DbType = System.Data.DbType.AnsiString;
                }
                else if (parameter.DbType == System.Data.DbType.DateTimeOffset)
                {
                    if (sqlParameter.Value != DBNull.Value)
                        sqlParameter.Value = UtilMethods.ConvertFromDateTimeOffset((DateTimeOffset)sqlParameter.Value);
                    sqlParameter.DbType = System.Data.DbType.DateTime;
                }
                ++index;
            }
            return result;
        }

        private bool IsVarchar()
        {
            if (this.Context.CurrentConnectionConfig.MoreSettings != null && this.Context.CurrentConnectionConfig.MoreSettings.DisableNvarchar)
            {
                return true;
            }
            return false;
        }


        public override object GetScalar(string sql, params SugarParameter[] parameters)
        {
            try
            {
                InitParameters(ref sql, parameters);
                if (IsFormat(parameters))
                    sql = FormatSql(sql);
                if (this.Context.CurrentConnectionConfig?.SqlMiddle?.IsSqlMiddle == true)
                    return this.Context.CurrentConnectionConfig.SqlMiddle.GetScalar(sql, parameters);
                SetConnectionStart(sql);
                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);
                ExecuteBefore(sql, parameters);
                IDbCommand sqlCommand = GetCommand(sql, parameters);
                int sqlCode = sqlCommand.ExecuteNonQuery();
                
                //scalar = (scalar == null ? 0 : scalar);
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);

                object scalar = null;
                if (sqlCode==1)
                {
                    sqlCommand.CommandText = "SELECT LAST_IDENTITY()";
                    scalar = sqlCommand.ExecuteScalar();
                }
                sqlCommand.Dispose();
                return scalar;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw ex;
            }
            finally
            {
                if (this.IsAutoClose()) this.Close();
                SetConnectionEnd(sql);
            }
        }



        #region async
        public async Task CloseAsync()
        {
            if (this.Transaction != null)
            {
                this.Transaction = null;
            }
            if (this.Connection != null && this.Connection.State == ConnectionState.Open)
            {
                await (this.Connection as IRISConnection).CloseAsync();
            }
            if (this.IsMasterSlaveSeparation && this.SlaveConnections.HasValue())
            {
                foreach (var slaveConnection in this.SlaveConnections)
                {
                    if (slaveConnection != null && slaveConnection.State == ConnectionState.Open)
                    {
                        await (slaveConnection as IRISConnection).CloseAsync();
                    }
                }
            }
        }
        public async Task<DbCommand> GetCommandAsync(string sql, SugarParameter[] parameters)
        {
            IRISCommand sqlCommand = new IRISCommand(sql, (IRISConnection)this.Connection);
            sqlCommand.CommandType = this.CommandType;
            sqlCommand.CommandTimeout = this.CommandTimeOut;
            if (this.Transaction != null)
            {
                sqlCommand.Transaction = (IRISTransaction)this.Transaction;
            }
            if (parameters.HasValue())
            {
                IDataParameter[] ipars = ToIDbDataParameter(parameters);
                sqlCommand.Parameters.AddRange((IRISParameter[])ipars);
            }
            if (this.Connection.State != ConnectionState.Open)
            {
                try
                {
                    await (this.Connection as IRISConnection).OpenAsync();
                }
                catch (Exception ex)
                {
                    Check.Exception(true, ex.Message);
                }
            }
            return sqlCommand;
        }
        public override async Task<int> ExecuteCommandAsync(string sql, params SugarParameter[] parameters)
        {
            try
            {
                Async();
                InitParameters(ref sql, parameters);
                if (FormatSql != null)
                    sql = FormatSql(sql);
                SetConnectionStart(sql);
                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);
                ExecuteBefore(sql, parameters);
                var sqlCommand = await GetCommandAsync(sql, parameters);
                int count;
                if (this.CancellationToken == null)
                    count = await sqlCommand.ExecuteNonQueryAsync();
                else
                    count = await sqlCommand.ExecuteNonQueryAsync(this.CancellationToken.Value);
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                sqlCommand.Dispose();
                return count;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw ex;
            }
            finally
            {
                if (this.IsAutoClose())
                {
                    await this.CloseAsync();
                }
                SetConnectionEnd(sql);
            }
        }
        public override async Task<IDataReader> GetDataReaderAsync(string sql, params SugarParameter[] parameters)
        {
            try
            {
                Async();
                InitParameters(ref sql, parameters);
                if (FormatSql != null)
                    sql = FormatSql(sql);
                SetConnectionStart(sql);
                var isSp = this.CommandType == CommandType.StoredProcedure;
                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);
                ExecuteBefore(sql, parameters);
                var sqlCommand = await GetCommandAsync(sql, parameters);
                DbDataReader sqlDataReader;
                if (this.CancellationToken == null)
                    sqlDataReader = await sqlCommand.ExecuteReaderAsync(this.IsAutoClose() ? CommandBehavior.CloseConnection : CommandBehavior.Default);
                else
                    sqlDataReader = await sqlCommand.ExecuteReaderAsync(this.IsAutoClose() ? CommandBehavior.CloseConnection : CommandBehavior.Default, this.CancellationToken.Value);
                if (isSp)
                    DataReaderParameters = sqlCommand.Parameters;
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                SetConnectionEnd(sql);
                if (SugarCompatible.IsFramework || this.Context.CurrentConnectionConfig.DbType != DbType.Sqlite)
                    sqlCommand.Dispose();
                return sqlDataReader;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw ex;
            }
        }
        public override async Task<object> GetScalarAsync(string sql, params SugarParameter[] parameters)
        {
            try
            {
                Async();
                InitParameters(ref sql, parameters);
                if (FormatSql != null)
                    sql = FormatSql(sql);
                SetConnectionStart(sql);
                if (this.ProcessingEventStartingSQL != null)
                    ExecuteProcessingSQL(ref sql, ref parameters);
                ExecuteBefore(sql, parameters);
                var sqlCommand = await GetCommandAsync(sql, parameters);
                object scalar;
                if (CancellationToken == null)
                    scalar = await sqlCommand.ExecuteScalarAsync();
                else
                    scalar = await sqlCommand.ExecuteScalarAsync(this.CancellationToken.Value);
                //scalar = (scalar == null ? 0 : scalar);
                if (this.IsClearParameters)
                    sqlCommand.Parameters.Clear();
                ExecuteAfter(sql, parameters);
                sqlCommand.Dispose();
                return scalar;
            }
            catch (Exception ex)
            {
                CommandType = CommandType.Text;
                if (ErrorEvent != null)
                    ExecuteErrorEvent(sql, parameters, ex);
                throw ex;
            }
            finally
            {
                if (this.IsAutoClose())
                {
                    await this.CloseAsync();
                }
                SetConnectionEnd(sql);
            }
        }
        #endregion
    }
}
