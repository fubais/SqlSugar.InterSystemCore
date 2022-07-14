using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemDbBind : DbBindProvider
    {
        public override List<KeyValuePair<string, CSharpDataType>> MappingTypes { get;  } = new List<KeyValuePair<string, CSharpDataType>>() {

            new KeyValuePair<string, CSharpDataType>("BIT",CSharpDataType.@bool),//%Library.Boolean
            new KeyValuePair<string, CSharpDataType>("TINYINT",CSharpDataType.@byte),
            new KeyValuePair<string, CSharpDataType>("SMALLINT",CSharpDataType.@short),
            new KeyValuePair<string, CSharpDataType>("INTEGER",CSharpDataType.@int),
            new KeyValuePair<string, CSharpDataType>("BIGINT",CSharpDataType.@long),

            new KeyValuePair<string, CSharpDataType>("FLOAT",CSharpDataType.@float),//%Library.Float
            new KeyValuePair<string, CSharpDataType>("DOUBLE",CSharpDataType.@double),//%Library.Double
            new KeyValuePair<string, CSharpDataType>("DECIMAL",CSharpDataType.@decimal),

            new KeyValuePair<string, CSharpDataType>("VARCHAR",CSharpDataType.@string),//%Library.String
            new KeyValuePair<string, CSharpDataType>("CHAR",CSharpDataType.@string),//%Library.String(MAXLEN=1)


            new KeyValuePair<string, CSharpDataType>("DATETIME",CSharpDataType.DateTime),//%Library.TimeStamp
            new KeyValuePair<string, CSharpDataType>("DATE",CSharpDataType.DateTime),//%Library.Date
            new KeyValuePair<string, CSharpDataType>("TIME",CSharpDataType.TimeSpan),//%Library.Time

            new KeyValuePair<string, CSharpDataType>("LONGVARBINARY",CSharpDataType.byteArray),//%Stream.GlobalBinary
            new KeyValuePair<string, CSharpDataType>("LONGVARCHAR",CSharpDataType.@string),//%Stream.GlobalBinary
        };

        public override string GetDbTypeName(string csharpTypeName)
        {
            if (csharpTypeName == UtilConstants.ByteArrayType.Name)
                csharpTypeName = "byteArray";
            if (csharpTypeName.ToLower() == "int32")
                csharpTypeName = "int";
            if (csharpTypeName.ToLower() == "int16")
                csharpTypeName = "short";
            if (csharpTypeName.ToLower() == "int64")
                csharpTypeName = "long";
            if (csharpTypeName.ToLower().IsIn("boolean", "bool"))
                csharpTypeName = "bool";
            if (csharpTypeName == "DateTimeOffset")
                csharpTypeName = "DateTime";
            //
            if (csharpTypeName == "Single")
                csharpTypeName = "float";

            var mappings = this.MappingTypes.Where(it => it.Value.ToString().Equals(csharpTypeName, StringComparison.CurrentCultureIgnoreCase)).ToList();
            if (mappings != null && mappings.Count > 0)
                return mappings.First().Key;
            else
                return "varchar";


        }

    }
}
