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
	public class CSpriteResourceManager : Singleton<CSpriteResourceManager>
	{
		private List<string> _registerRootPaths = new List<string>();
		private Dictionary<string, Sprite> _loadedSprite = new Dictionary<string, Sprite>();

		public bool IsInitialize { get; private set; } = false;

		public void RegisterIconRootPath(string path)
		{
			_registerRootPaths.Add(path);
		}

		public Coroutine Load(MonoBehaviour dispatcher)
		{
			return dispatcher.StartCoroutine(_LoadCorutine());
		}

		public void Unload()
		{
			foreach(var sprite in _loadedSprite.Values)
			{
				GameObject.Destroy(sprite);
				Resources.UnloadAsset(sprite);
			}
		}
		public Sprite FindSrpite(string resourcePath)
		{
			if (_loadedSprite.ContainsKey(resourcePath) == false)
				return null;

			return _loadedSprite[resourcePath];
		}

		private IEnumerator _LoadCorutine()
		{
			foreach(var rootPath in _registerRootPaths)
			{
				var loadedSprites = Resources.LoadAll<Sprite>(rootPath);
				foreach(var sprite in loadedSprites)
				{
					string path = $"{rootPath}/{sprite.name}";
					_loadedSprite.Add(path, sprite);
				}
			}

			IsInitialize = true;

			yield return null;
		}
	}
}
