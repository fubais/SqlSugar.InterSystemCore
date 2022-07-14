using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemInsertable<T> : InsertableProvider<T> where T : class, new()
    {
    }
}
