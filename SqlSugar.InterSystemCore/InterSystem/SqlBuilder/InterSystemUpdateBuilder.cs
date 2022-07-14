using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemUpdateBuilder: UpdateBuilder
    {
        public override string SqlTemplateBatch => "UPDATE {1} AS S SET {0} FROM {1} S {2} INNER JOIN ";


        public override object FormatValue(object value)
        {
            return FormatValueInSQL.Format(value).Result;
        }

    }
}
