using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemBuilder : SqlBuilderProvider
    {
        public override string SqlTranslationLeft { get { return "\r\n"; } }
        public override string SqlTranslationRight { get { return ""; } }


        public override string SqlDateNow
        {
            get
            {
                return "SELECT now()";
            }
        }

    }
}
