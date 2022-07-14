using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemDbMaintenance : DbMaintenanceProvider
    {
        #region DML
        protected override string GetDataBaseSql
        {
            get
            {
                throw new NotSupportedException("未发现InterSystem提供该支持在GetDataBaseSql");
            }
        }

        protected override string GetColumnInfosByTableNameSql
        {
            get
            {
                return @"SELECT 
                            0 as TableId,
                            parent AS TableName,
                            Name AS DbColumnName,
                            Type AS DataType,
                            0 AS Length,
                            '' AS DefaultValue,
                            Description AS ColumnDescription,
                            convert(BIT,0) AS IsPrimaryKey,
                            _Identity AS IsIdentity,
                            convert(BIT,0) AS IsNullable,
                            0 AS Scale,
                            0 AS DecimalDigits,
                            convert(BIT,0) AS IsUnsigned 
                             FROM %Dictionary.PropertyDefinition WHERE parent ='User.RoomTest' ORDER BY SqlColumnNumber";
            }
        }
        protected override string GetTableInfoListSql
        {
            get
            {
                return "select Name,Description from %dictionary.compiledclass where ID LIKE 'User.%' AND Ancestry IS NOT NULL";
            }
        }
        protected override string GetViewInfoListSql
        {
            get
            {
                throw new NotSupportedException("未发现InterSystem提供该支持在GetViewInfoListSql");
            }
        }
        #endregion

        #region DDL
        protected override string CreateDataBaseSql
        {
            get
            {
                return "CREATE DATABASE {0} ";
            }
        }
        protected override string AddPrimaryKeySql
        {
            get
            {
                return "ALTER TABLE {0} ADD PRIMARY KEY({2}) /*{1}*/";
            }
        }
        protected override string AddColumnToTableSql
        {
            get
            {
                return "ALTER TABLE {0} ADD {1} {2}{3} {4} {5} {6}";
            }
        }
        protected override string AlterColumnToTableSql
        {
            get
            {
                return "ALTER TABLE {0} ALTER COLUMN {1} {2}{3} {4} {5} {6}";
            }
        }
        protected override string BackupDataBaseSql
        {
            get
            {
                throw new NotSupportedException("未发现InterSystem提供该支持在BackupDataBaseSql");
            }
        }
        protected override string CreateTableSql
        {
            get
            {
                return "CREATE TABLE SQLUser.{0}(\r\n{1})";
            }
        }
        protected override string CreateTableColumn
        {
            get
            {
                return "{0} {1}{2} {3} {4} {5}";
            }
        }
        protected override string TruncateTableSql
        {
            get
            {
                return "TRUNCATE TABLE {0}";
            }
        }
        protected override string BackupTableSql
        {
            get
            {
                throw new NotSupportedException("未发现InterSystem提供该支持在BackupTableSql");
                return "Create table SQLUser.{1} (Select top {0} * from {2})";
            }
        }
        protected override string DropTableSql
        {
            get
            {
                return "DROP TABLE {0}";
            }
        }
        protected override string DropColumnToTableSql
        {
            get
            {
                return "ALTER TABLE {0} DROP COLUMN {1}";
            }
        }
        protected override string DropConstraintSql
        {
            get
            {
                return "ALTER TABLE {0} drop primary key;";
            }
        }
        protected override string RenameColumnSql
        {
            get
            {
                return "alter table {0} change  column {1} {2}";
            }
        }
        #endregion

        #region Check
        protected override string CheckSystemTablePermissionsSql
        {
            get
            {
                return "select * from %dictionary.compiledclass where ID LIKE 'User.%' AND Ancestry IS NOT NULL";
            }
        }
        #endregion

        #region Scattered
        protected override string CreateTableNull
        {
            get
            {
                return "DEFAULT NULL";
            }
        }
        protected override string CreateTableNotNull
        {
            get
            {
                return "NOT NULL";
            }
        }
        protected override string CreateTablePirmaryKey
        {
            get
            {
                return "PRIMARY KEY";
            }
        }
        protected override string CreateTableIdentity
        {
            get
            {
                return "IDENTITY(1,1)";
            }
        }

        protected override string AddColumnRemarkSql
        {
            get
            {
                throw new NotSupportedException("未发现InterSystem提供该支持在AddColumnRemarkSql");
            }
        }

        protected override string DeleteColumnRemarkSql
        {
            get
            {
                throw new NotSupportedException("未发现InterSystem提供该支持在DeleteColumnRemarkSql");
            }
        }

        protected override string IsAnyColumnRemarkSql
        {
            get
            {
                throw new NotSupportedException("未发现InterSystem提供该支持在IsAnyColumnRemarkSql");
            }
        }

        protected override string AddTableRemarkSql
        {
            get
            {
                throw new NotSupportedException("未发现InterSystem提供该支持在AddTableRemarkSql");
                return "ALTER TABLE {0} COMMENT='{1}';";
            }
        }

        protected override string DeleteTableRemarkSql
        {
            get
            {
                return "ALTER TABLE {0} COMMENT='';";
            }
        }

        protected override string IsAnyTableRemarkSql
        {
            get
            {
                throw new NotSupportedException("未发现InterSystem提供该支持在IsAnyTableRemarkSql");
            }
        }

        protected override string RenameTableSql
        {
            get
            {
                return "alter table SQLUser.{0} rename SQLUser.{1}";
            }
        }

        protected override string CreateIndexSql
        {
            get
            {
                return "CREATE {3} INDEX Index_{0}_{2} ON {0} ({1})";
            }
        }

        protected override string AddDefaultValueSql
        {
            get
            {
                return "ALTER TABLE {0} ALTER COLUMN {1} SET DEFAULT '{2}'";
            }
        }
        protected override string IsAnyIndexSql
        {
            get
            {
                throw new NotSupportedException("未发现InterSystem提供该支持在IsAnyIndexSql");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///by current connection string
        /// </summary>
        /// <param name="databaseDirectory"></param>
        /// <returns></returns>
        public override bool CreateDatabase(string databaseName, string databaseDirectory = null)
        {
            throw new NotSupportedException("未发现InterSystem提供该支持在CreateDatabase");

            return true;
        }
        public override bool AddTableRemark(string tableName, string description)
        {
            throw new NotSupportedException("未发现InterSystem提供该支持在AddTableRemark");

            return true;
        }
        public override bool CreateTable(string tableName, List<DbColumnInfo> columns, bool isCreatePrimaryKey = true)
        {
            if (columns.HasValue())
            {
                foreach (var item in columns)
                {
                    if (item.DbColumnName.Equals("GUID", StringComparison.CurrentCultureIgnoreCase) && item.Length == 0)
                    {
                        item.Length = 10;
                    }
                }
            }
            string sql = GetCreateTableSql(tableName, columns);
            string primaryKeyInfo = null;
            if (columns.Any(it => it.IsPrimarykey) && isCreatePrimaryKey)
            {
                primaryKeyInfo = string.Format(", Primary key({0})", string.Join(",", columns.Where(it => it.IsPrimarykey).Select(it => this.SqlBuilder.GetTranslationColumnName(it.DbColumnName))));

            }
            sql = sql.Replace("$PrimaryKey", primaryKeyInfo);
            this.Context.Ado.ExecuteCommand(sql);

            return true;
        }
        
        public override bool AddRemark(EntityInfo entity)
        {

            return true;
        }
        protected override string GetCreateTableSql(string tableName, List<DbColumnInfo> columns)
        {
            List<string> columnArray = new List<string>();
            Check.Exception(columns.IsNullOrEmpty(), "No columns found ");
            foreach (var item in columns)
            {
                string columnName = item.DbColumnName;
                string dataSize = "";
                dataSize = GetSize(item);
                string dataType = item.DataType;
                string nullType = item.IsNullable ? this.CreateTableNull : CreateTableNotNull;
                string primaryKey = null;
                string identity = item.IsIdentity ? this.CreateTableIdentity : null;
                string addItem = string.Format(this.CreateTableColumn, this.SqlBuilder.GetTranslationColumnName(columnName), dataType, dataSize, nullType, primaryKey, identity);
                columnArray.Add(addItem);
            }
            string tableString = string.Format(this.CreateTableSql, this.SqlBuilder.GetTranslationTableName(tableName), string.Join(",\r\n", columnArray));
            return tableString;
        }

        protected override string GetSize(DbColumnInfo item)
        {
            string dataSize = null;
            var isMax = item.Length > 4000 || item.Length == -1;
            if (isMax)
            {
                dataSize = "";
                item.DataType = "LONGVAERCHER";
            }
            else if (item.Length > 0 && item.DecimalDigits == 0)
            {
                dataSize = item.Length > 0 ? string.Format("({0})", item.Length) : null;
            }
            else if (item.Length == 0 && item.DecimalDigits > 0)
            {
                item.Length = 10;
                dataSize = string.Format("({0},{1})", item.Length, item.DecimalDigits);
            }
            else if (item.Length > 0 && item.DecimalDigits > 0)
            {
                dataSize = item.Length > 0 ? string.Format("({0},{1})", item.Length, item.DecimalDigits) : null;
            }
            return dataSize;
        }

        public override bool RenameColumn(string tableName, string oldColumnName, string newColumnName)
        {

            throw new NotSupportedException("未发现InterSystem提供该支持在RenameColumn");

        }
        public override bool AddDefaultValue(string tableName, string columnName, string defaultValue)
        {
            throw new NotSupportedException("未发现InterSystem提供该支持AddDefaultValue");
        }
        public override bool IsAnyConstraint(string constraintName)
        {
            throw new NotSupportedException("未发现InterSystem提供该支持IsAnyConstraint");
        }
        public override bool BackupDataBase(string databaseName, string fullFileName)
        {
            throw new NotSupportedException("未发现InterSystem提供该支持BackupDataBase");
        }

        #endregion
    }
}
