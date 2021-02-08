using Golbeng.Framework.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Golbeng.Framework.Managers
{
	public partial class CObjectGenerateManager<TKey> : Singleton<CObjectGenerateManager<TKey>>
	{
		public delegate void GenerateHandler(GameObject gameObject);
		public delegate void DestoryHandler(GameObject gameObject);

		public class CObjectPool
		{
			// Prefab, List<생성 오브젝트들>
			private Dictionary<GameObject, List<GameObject>> _generatePool = new Dictionary<GameObject, List<GameObject>>();
			// <GameObject, PrefabObject> GameObject 기준으로 어떤 Prefab과 연결되었는가 정보
			private Dictionary<GameObject, GameObject> _prefabsMapping = new Dictionary<GameObject, GameObject>();
		
			private List<GameObject> _IdleObjectPool = new List<GameObject>();
			private HashSet<GameObject> _activeObjectPool = new HashSet<GameObject>();

			public GameObject ParentGameObject { get; private set; }

			public int ActiveCount { get => _activeObjectPool.Count; }
			public int PoolCount { get => _generatePool.Sum(kv => kv.Value.Count); }

			public event GenerateHandler GenerateHandler;
			public event DestoryHandler DestoryHandler;

			public CObjectPool(GameObject parentGameObject)
			{
				ParentGameObject = parentGameObject;
			}

			public void Clear()
			{
				foreach (var pair in _generatePool)
				{
					foreach(var generateObj in pair.Value)
					{
						generateObj.SetActive(false);
						GameObject.Destroy(generateObj);
					}
				}

				_generatePool.Clear();
				_IdleObjectPool.Clear();
				_activeObjectPool.Clear();
			}

			public GameObject GenerateObject(GameObject prefabObject, Vector3? generatePosition = null)
			{
				if (prefabObject == null)
					return null;

				GameObject generateObject = null;
				if (_IdleObjectPool.Count > 0)
				{
					foreach(var idle in _IdleObjectPool)
					{
						if (_prefabsMapping.ContainsKey(idle) == false)
							continue;

						var findPrefab = _prefabsMapping[idle];
						if (findPrefab != prefabObject)
							continue;

						generateObject = idle;
						_IdleObjectPool.Remove(idle);
						break;
					}
				}
				
				if(generateObject == null)
				{
					if (ParentGameObject != null)
						generateObject = GameObject.Instantiate(prefabObject, ParentGameObject.transform);
					else
						generateObject = GameObject.Instantiate(prefabObject);

					if(_generatePool.ContainsKey(prefabObject) == false)
						_generatePool.Add(prefabObject, new List<GameObject>());

					_generatePool[prefabObject].Add(generateObject);
					_prefabsMapping.Add(generateObject, prefabObject);
				}

				_activeObjectPool.Add(generateObject);

				Vector3 position = generatePosition ?? Vector3.zero;

				generateObject.transform.position = position;
				generateObject.SetActive(true);

				GenerateHandler?.Invoke(generateObject);

				return generateObject;
			}

			public bool DestoryObject(GameObject gameObject, bool terminate = false)
			{
				if (_activeObjectPool.Contains(gameObject) == false)
					return false;

				DestoryHandler?.Invoke(gameObject);

				_activeObjectPool.Remove(gameObject);

				gameObject.SetActive(false);

				if(terminate == true)
				{
					if(_prefabsMapping.ContainsKey(gameObject) == true)
					{
						var prefab = _prefabsMapping[gameObject];
						if(_generatePool.ContainsKey(prefab) == true)
						{
							_generatePool[prefab].Remove(gameObject);
						}

						_prefabsMapping.Remove(gameObject);
					}

					GameObject.Destroy(gameObject);
				}
				else
				{
					_IdleObjectPool.Add(gameObject);
				}

				return true;
			}

			public GameObject FindObject(int instanceID)
			{
				var findGameObject = _activeObjectPool.Where(g => g.GetInstanceID() == instanceID).FirstOrDefault();
				return findGameObject;
			}
		}
	}

	public partial class CObjectGenerateManager<TKey> : Singleton<CObjectGenerateManager<TKey>>
	{
		private Dictionary<TKey, CObjectPool> _objectPool = new Dictionary<TKey, CObjectPool>();

		public CObjectPool RegisterObjectPool(TKey key, GameObject parentObject)
		{
			if(_objectPool.ContainsKey(key) == true)
				throw new Exception("Duplicate ObjectPool Type");

			var objectPool = new CObjectPool(parentObject);
			_objectPool.Add(key, objectPool);

			return objectPool;
		}

		public void UnregisterObjectPool(TKey key)
		{
			if (_objectPool.ContainsKey(key) == false)
				return;

			_objectPool[key].Clear();
			_objectPool.Remove(key);
		}

		public bool IsRegistertObjectPool(TKey key)
		{
			return _objectPool.ContainsKey(key);
		}

		public CObjectPool GetObjectPool(TKey key)
		{
			if (_objectPool.ContainsKey(key) == false)
				return null;

			return _objectPool[key];
		}

		public void ClearObjectPool()
		{
			foreach(var objectPool in _objectPool)
			{
				objectPool.Value.Clear();
			}
		}

		public GameObject GenerateGameObject(TKey key, GameObject prefabObject, Vector3? generatePosition = null)
		{
			if (_objectPool.ContainsKey(key) == false)
				return null;

			return _objectPool[key].GenerateObject(prefabObject, generatePosition);
		}

		public T GenerateGameObject<T>(TKey key, GameObject prefabObject, Vector3? generatePosition = null) where T: class
		{
			if (_objectPool.ContainsKey(key) == false)
				return default(T);

			var generateGameObject = _objectPool[key].GenerateObject(prefabObject, generatePosition);
			if (generateGameObject == null)
				return default(T);

			T component = null;
			if(generateGameObject.TryGetComponent<T>(out component) == false)
			{
				_objectPool[key].DestoryObject(generateGameObject, true);
				return default(T);
			}

			return component;
		}

		public void DestoryGameObject(TKey key, GameObject gameObejct)
		{
			if (_objectPool.ContainsKey(key) == false)
				return;

			_objectPool[key].DestoryObject(gameObejct);
		}

		public void DestoryGameObject(GameObject gameObejct)
		{
			foreach(var pool in _objectPool.Values)
			{
				if (pool.DestoryObject(gameObejct) == true)
					break;
			}
		}

		public (TKey key, GameObject gameObject)? FindGameObject(int instanceID)
		{
			foreach (var kv in _objectPool)
			{
				var findGameObject = kv.Value.FindObject(instanceID);
				if(findGameObject != null)
				{
					return (kv.Key, findGameObject);
				}
			}

			return null;
		}

		public int GetCount(TKey key)
		{
			if (_objectPool.ContainsKey(key) == false)
				return 0;

			return _objectPool[key].ActiveCount;
		}
	}
}
