using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemInsertBuilder : InsertBuilder
    {
        public override string SqlTemplate
        {
            get
            {
                return @"INSERT INTO {0} ({1})  VALUES ({2})";
            }
        }
        public override string SqlTemplateBatch
        {
            get
            {
                return "INSERT {0} ({1})";
            }
        }
        public override string SqlTemplateBatchSelect
        {
            get
            {
                return "{0} AS {1}";
            }
        }
        public override string SqlTemplateBatchUnion
        {
            get
            {
                return "\t\r\nUNION ALL ";
            }
        }




        public override object FormatValue(object value)
        {
            return FormatValueInSQL.Format(value).Result;
        }
    }
}
