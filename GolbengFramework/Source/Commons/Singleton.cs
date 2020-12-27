using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Golbeng.Framework.Commons
{
	public class Singleton<T> where T : class, new()
	{
		public virtual void OnInitSingleton()
		{}

		private static T _instance = null;
		public static T Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = new T();

					var methodInfo = typeof(T).GetMethod("OnInitSingleton", BindingFlags.Public | BindingFlags.Instance);
					if (methodInfo != null)
						methodInfo.Invoke(_instance, null);
				}

				return _instance;
			}
		}
	}
}
