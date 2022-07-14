using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemFastBuilder : FastBuilder, IFastBuilder
    {
        public Task<int> ExecuteBulkCopyAsync(DataTable dt)
        {
            throw new NotImplementedException("InterSystemFastBuilder_InterSystemFastBuilder");
        }
    }
}
