using Golbeng.Framework.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Golbeng.Framework.Managers
{
	public class CDynamicSingletonManager : Singleton<CDynamicSingletonManager>
	{
		private Dictionary<Type, object> _dynamicSingletonPool = new Dictionary<Type, object>();

		public T AllocDynamicSingleton<T>() where T : class, new()
		{
			var type = typeof(T);
			if (_dynamicSingletonPool.ContainsKey(type) == false)
				_dynamicSingletonPool.Add(type, new T());

			return _dynamicSingletonPool[type] as T;
		}

		public void ReleaseDynamicSingleton<T>() where T : class
		{
			var type = typeof(T);
			if (_dynamicSingletonPool.ContainsKey(type) == false)
				return;

			_dynamicSingletonPool.Remove(type);
		}

		public T GetDynamicSingletone<T>() where T : class, new()
		{
			return AllocDynamicSingleton<T>();
		}
	}
}
