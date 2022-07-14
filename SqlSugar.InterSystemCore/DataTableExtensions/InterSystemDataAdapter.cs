using System;
using System.Collections.Generic;
using System.Text;
using InterSystems.Data.IRISClient;
using System.Data;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemDataAdapter : IDataAdapter
    {
        private IRISCommand _command ;
        private string _sql;
        private IRISConnection _sqlConnection;


        public InterSystemDataAdapter()
        {

        }

        /// <summary>
        /// SqlDataAdapter
        /// </summary>
        /// <param name="command"></param>
        public InterSystemDataAdapter(IRISCommand command)
        {
            this._command = command;
        }

        /// <summary>
        /// SqlDataAdapter
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="_sqlConnection"></param>
        public InterSystemDataAdapter(string sql, IRISConnection _sqlConnection)
        {
            this._sql = sql;
            this._sqlConnection = _sqlConnection;
        }

        /// <summary>
        /// SelectCommand
        /// </summary>
        public IRISCommand SelectCommand
        {
            get
            {
                if (this._command == null)
                {
                    this._command = new IRISCommand(this._sql, this._sqlConnection);
                }
                return this._command;
            }
            set
            {
                this._command = value;
            }
        }

        /// <summary>
        /// Fill
        /// </summary>
        /// <param name="dt"></param>
        public void Fill(DataTable dt)
        {
            if (dt == null)
            {
                dt = new DataTable();
            }
            var columns = dt.Columns;
            var rows = dt.Rows;
            using (IRISDataReader dr = _command.ExecuteReader())
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string name = dr.GetName(i).Trim();
                    if (!columns.Contains(name))
                        columns.Add(new DataColumn(name, dr.GetFieldType(i)));
                    else
                    {
                        columns.Add(new DataColumn(name + i, dr.GetFieldType(i)));
                    }
                }

                while (dr.Read())
                {
                    DataRow daRow = dt.NewRow();
                    for (int i = 0; i < columns.Count; i++)
                    {
                        daRow[columns[i].ColumnName] = dr.GetValue(i);
                    }
                    dt.Rows.Add(daRow);
                }
            }
            dt.AcceptChanges();
        }

        /// <summary>
        /// Fill
        /// </summary>
        /// <param name="ds"></param>
        public void Fill(DataSet ds)
        {
            if (ds == null)
            {
                ds = new DataSet();
            }
            using (IRISDataReader dr = _command.ExecuteReader())
            {
                do
                {
                    var dt = new DataTable();
                    var columns = dt.Columns;
                    var rows = dt.Rows;
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        string name = dr.GetName(i).Trim();
                        if (!columns.Contains(name))
                            columns.Add(new DataColumn(name, dr.GetFieldType(i)));
                        else
                        {
                            columns.Add(new DataColumn(name + i, dr.GetFieldType(i)));
                        }
                    }

                    while (dr.Read())
                    {
                        DataRow daRow = dt.NewRow();
                        for (int i = 0; i < columns.Count; i++)
                        {
                            daRow[columns[i].ColumnName] = dr.GetValue(i);
                        }
                        dt.Rows.Add(daRow);
                    }
                    dt.AcceptChanges();
                    ds.Tables.Add(dt);
                } while (dr.NextResult());
            }
        }
    }
}
