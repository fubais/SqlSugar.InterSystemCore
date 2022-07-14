using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSugar.InterSystemCore
{
    public class InterSystemUpdateable<T> : UpdateableProvider<T> where T : class, new()
    {
    }
}
