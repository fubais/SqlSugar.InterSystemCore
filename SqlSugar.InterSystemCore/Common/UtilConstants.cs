using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace SqlSugar.InterSystemCore
{
    internal static class UtilConstants
    {
        internal static Type IntType = typeof(int);
        internal static Type LongType = typeof(long);
        internal static Type BoolType = typeof(bool);
        internal static Type BoolTypeNull = typeof(bool?);
        internal static Type ByteType = typeof(Byte);
        internal static Type ObjType = typeof(object);
        internal static Type DobType = typeof(double);
        internal static Type FloatType = typeof(float);
        internal static Type ShortType = typeof(short);
        internal static Type DecType = typeof(decimal);
        internal static Type StringType = typeof(string);
        internal static Type DateType = typeof(DateTime);
        internal static Type TimeSpanType = typeof(TimeSpan);
        internal static Type ByteArrayType = typeof(byte[]);
        internal static Type ModelType = typeof(ModelContext);
        internal static Type DynamicType = typeof(ExpandoObject);
        internal static Type StreamType = typeof(System.IO.Stream);
        internal static Type MemoryStreamType = typeof(System.IO.MemoryStream);
        public static Type SugarType = typeof(SqlSugarProvider);


    }
}
