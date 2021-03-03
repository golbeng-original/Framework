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
		protected virtual void OnInitSingleton()
		{}

		private static T _instance = null;
		public static T Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = new T();

					Singleton<T> singletonInstance = _instance as Singleton<T>;
					if(singletonInstance != null)
						singletonInstance.OnInitSingleton();
				}

				return _instance;
			}
		}
	}
}
