using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemCodeFirst : CodeFirstProvider
    {
        protected override void ConvertColumns(List<DbColumnInfo> dbColumns)
        {
            foreach (var item in dbColumns)
            {
                if (item.DataType == "DateTime")
                {
                    item.Length = 0;
                }
            }
        }

    }
}
