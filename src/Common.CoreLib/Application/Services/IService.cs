using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Services
{
    public interface IService<T> where T : notnull, IService<T>
    {
        static T Instance => DI.Get<T>();
    }
}
