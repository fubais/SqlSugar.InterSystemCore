using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SqlSugar.InterSystemCore
{
    internal class FormatValueInSQL
    {

        public static async Task<string> Format(object value)
        {
            return await Task.Run<string>(() => {
                if (value == null)
                {
                    return "NULL";
                }

                var type = UtilMethods.GetUnderType(value.GetType());

                if (type == UtilConstants.DateType)
                {
                    var date = value.ObjToDate();
                    if (date < UtilMethods.GetMinDate())
                    {
                        date = UtilMethods.GetMinDate();
                    }
                    return "'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                }
                else if (type == UtilConstants.StreamType || type == UtilConstants.MemoryStreamType)
                {
                    var stream = value as Stream;
                    var saveStream = new MemoryStream();
                    stream?.CopyTo(saveStream);
                    return "'" + "0x" + BitConverter.ToString(saveStream.ToArray()).Replace("-", "") + "'";
                }
                else if (type == UtilConstants.ByteArrayType)
                {
                    return "'" + "0x" + BitConverter.ToString((byte[])value).Replace("-", "") + "'";
                }
                else if (type.IsEnum())
                {
                    return Convert.ToInt64(value).ToString();
                }
                else if (type == UtilConstants.BoolType)
                {
                    return value.ObjToBool() ? "1" : "0";
                }
                else if (type == UtilConstants.StringType || type == UtilConstants.ObjType)
                {
                    return "'" + value.ToString().ToSqlFilter() + "'";
                }
                else
                {
                    return value.ToString();
                }
            });
        }
    }
}
