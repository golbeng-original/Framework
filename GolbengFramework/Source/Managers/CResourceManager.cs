using Golbeng.Framework.Commons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.Managers
{
	public class CResourceManage : Singleton<CResourceManage>
	{
		private List<(string rootPath, Type type)> _registerResourceRoot = new List<(string rootPath, Type type)>();
		private Dictionary<string, (UnityEngine.Object instance, Type type)> _loadedInstance = new Dictionary<string, (UnityEngine.Object instance, Type type)>();

		public void RegisterIconRootPath<T>(string path)
		{
			_registerResourceRoot.Add((path, typeof(T)));
		}

		public Coroutine Load(MonoBehaviour dispatcher)
		{
			return dispatcher.StartCoroutine(_LoadCorutine());
		}

		public void Unload()
		{
			foreach(var sprite in _loadedInstance.Values)
			{
				GameObject.Destroy(sprite.instance);
				Resources.UnloadAsset(sprite.instance);
			}
		}
		public T FindResouce<T>(string resourcePath) where T : UnityEngine.Object
		{
			if (_loadedInstance.ContainsKey(resourcePath) == false)
				return null;

			var instance = _loadedInstance[resourcePath];
			if (instance.type != typeof(T))
				return null;

			return instance.instance as T;
		}

		private IEnumerator _LoadCorutine()
		{
			foreach(var resourceRoot in _registerResourceRoot)
			{
				var loadedResources = Resources.LoadAll(resourceRoot.rootPath, resourceRoot.type);
				foreach(var resource in loadedResources)
				{
					string path = $"{resourceRoot.rootPath}/{resource.name}";
					_loadedInstance.Add(path, (resource, resourceRoot.type));
				}
			}

			yield return null;
		}
	}
}
