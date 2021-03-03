using Golbeng.Framework.Commons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.Networking;
using System.Reflection;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace Golbeng.Framework.Managers
{
	public partial class CResourceManager : Singleton<CResourceManager>
	{
		private static readonly string[] _staticStreamExtension = new[] { ".db", ".json", ".txt" };

		public class CResourceDownloaderAsync : CustomYieldInstruction
		{
			private bool _isComplete = false;

			private List<string> _downloadLabels = new List<string>();
			private List<string> _needDownloadLabels = new List<string>();

			public float Progress { get; private set; } = 0.0f;
			public string ProgressDescription { get; private set; } = "";

			public bool IsDone { get => _isComplete == true ? true : false; }
			public override bool keepWaiting { get => _isComplete == false ? true : false; }

			public CResourceDownloaderAsync(MonoBehaviour dispatcher, IEnumerable<string> labels)
			{
				_isComplete = false;

				_downloadLabels = labels.Distinct().ToList();

				dispatcher.StartCoroutine(Run());
			}

			private IEnumerator Run()
			{
				yield return _IsNeedDownload();

				if(_needDownloadLabels.Count > 0)
				{
					yield return _DownloadResourecs();
				}

				_isComplete = true;
			}

			private IEnumerator _IsNeedDownload()
			{
				Progress = 0.0f;
				ProgressDescription = "UpdateCheck Start";

				long totalDownloadSize = 0;
				List<string> needDownloadLabels = new List<string>();
				foreach (var label in _downloadLabels)
				{
					var downloadSizeHandler = Addressables.GetDownloadSizeAsync(label);

					yield return downloadSizeHandler;

					if (downloadSizeHandler.Result > 0)
					{
						needDownloadLabels.Add(label);
					}

					totalDownloadSize += downloadSizeHandler.Result;
				}

				Progress = 1.0f;
				ProgressDescription = "UpdateCheck Complete";
			}

			private IEnumerator _DownloadResourecs()
			{
				Progress = 0.0f;
				ProgressDescription = "Download Start";

				int totalCount = _needDownloadLabels.Count;

				foreach (var label in _needDownloadLabels)
				{
					var downloadHandler = Addressables.DownloadDependenciesAsync(label);
					while (downloadHandler.IsDone == false)
					{
						var precent = downloadHandler.PercentComplete;

						float progressUnit = precent / 100.0f;
						progressUnit = progressUnit / (float)totalCount;
						Progress += progressUnit;

						yield return null;
					}

					Addressables.Release(downloadHandler);
				}

				Progress = 1.0f;
				ProgressDescription = "Download Complete";
			}
		}
	}
	
	// Get Resources
	public partial class CResourceManager : Singleton<CResourceManager>
	{
		private Dictionary<string, (UnityEngine.Object instance, Type type)> _loadedInstance = new Dictionary<string, (UnityEngine.Object instance, Type type)>();
		private HashSet<string> _resourceInInstances = new HashSet<string>();

		public Coroutine Load(MonoBehaviour dispatcher)
		{
			return dispatcher.StartCoroutine(_LoadCorutine());
		}

		public void Unload()
		{
			foreach (var instance in _loadedInstance)
			{
				GameObject.Destroy(instance.Value.instance);

				// Resource로 부터 로드
				if(_resourceInInstances.Contains(instance.Key))
				{
					Resources.UnloadAsset(instance.Value.instance);
				}

				// Addressable로 부터 로드
				if(_resourceInAddress.Contains(instance.Key))
				{
					Addressables.Release(instance.Value.instance);
				}
			}

			_ReleaseAddressable();
		}

		private void AddResource(string resourcePath, UnityEngine.Object resource, Type type)
		{
			_loadedInstance.Add(resourcePath, (resource, type));
		}

		public T FindResource<T>(string resourcePath) where T : UnityEngine.Object
		{
			var sucecss = _loadedInstance.ContainsKey(resourcePath);
			if(sucecss == false)
			{
				resourcePath = GetAssetResourcePath(resourcePath);
				sucecss = _loadedInstance.ContainsKey(resourcePath);
			}

			if (sucecss == false)
			{
				ManagerProvider.Logger.Warning("CResourceManager", $"FindResource({resourcePath}) not found");
				return null;
			}

			var instance = _loadedInstance[resourcePath];
			if (instance.instance is T)
				return instance.instance as T;

			return null;
		}

		public Stream FindStreamResource(string resourcePath)
		{
			resourcePath = resourcePath.Replace("\\", "/");

			var findResource = FindResource<TextAsset>(resourcePath);
			if (findResource == null)
				return null;

			try
			{
				return new MemoryStream(findResource.bytes);
			}
			catch (Exception e)
			{
				ManagerProvider.Logger.Error("CResourceManager", e.Message);
				return null;
			}
		}

		public string FindAssetFullPath(string resourcePath)
		{
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
			var findResource = FindResource<UnityEngine.Object>(resourcePath);
			if (findResource == null)
				return "";

			var path = AssetDatabase.GetAssetPath(findResource);
			path = Path.Combine(Directory.GetCurrentDirectory(), path);
			return path;
#else
			return "";
#endif
		}

		private string GetAssetResourcePath(string resourcePath)
		{
			resourcePath = resourcePath.Replace("\\", "/");

			int extensionPointIndex = resourcePath.LastIndexOf(".");
			if (extensionPointIndex == -1)
				return resourcePath;

			return resourcePath.Substring(0, extensionPointIndex);
		}
	}


	// load from addressable
	public partial class CResourceManager : Singleton<CResourceManager>
	{
		private IList<string> _reserveAssetRepositoryLabels = new List<string>();
		private HashSet<string> _resourceInAddress = new HashSet<string>();

		public void RegisterFromAssetAddressableLabel(string label)
		{
			_reserveAssetRepositoryLabels.Add(label);
		}

		private void _ReleaseAddressable()
		{
			//Addressables
		}

		public CResourceDownloaderAsync GetResourceDownloaderAsync(MonoBehaviour dispatcher)
		{
			var downloader = new CResourceDownloaderAsync(dispatcher, _reserveAssetRepositoryLabels);
			return downloader;
		}

		public IEnumerator LoadResourceFromAddressable<T>(string label)
		{
			var resourceLocator = Addressables.ResourceLocators.ToList();
			if (resourceLocator.Count == 0)
			{
				ManagerProvider.Logger.Warning("CResourceManager", $"LoadResourceFromAddressable({label}) Locator.Count = 0");
				yield break;
			}


			ResourceLocationMap map = resourceLocator[0] as ResourceLocationMap;
			if (map == null)
			{
				ManagerProvider.Logger.Warning("CResourceManager", $"LoadResourceFromAddressable({label}) ResourceLocationMap is null");
				yield break;
			}

			IList<IResourceLocation> resources = new List<IResourceLocation>();
			map.Locate(label, typeof(T), out resources);

			foreach(var locator in resources)
			{
				var resourceHandler = Addressables.LoadAssetAsync<T>(locator.PrimaryKey);
				yield return resourceHandler;

				AddResource(locator.PrimaryKey, resourceHandler.Result as UnityEngine.Object, typeof(T));

				_resourceInAddress.Add(locator.PrimaryKey);
				//Addressables.Release(resourceHandler);
			}
 		}
	}

	// load from local
	public partial class CResourceManager : Singleton<CResourceManager>
	{
		private List<(string rootPath, Type type)> _reserveResourceRoots = new List<(string rootPath, Type type)>();
		private List<(string rootPath, Type type)> _reserveAssestDatabaseRoots = new List<(string rootPath, Type type)>();
		private List<(string rootPath, Type type)> _reserveAssetBundleRoots = new List<(string rootPath, Type type)>();
		
		public void RegisterFromResourcePath<T>(string path)
		{
			_reserveResourceRoots.Add((path, typeof(T)));
		}

		public void RegisterFromAssetDatabase<T>(string path)
		{
			_reserveAssestDatabaseRoots.Add((path, typeof(T)));
		}

		public void RegisterFromAssetBundle<T>(string path)
		{
			_reserveAssetBundleRoots.Add((path, typeof(T)));
		}
		
		private IEnumerator _LoadCorutine()
		{
			yield return _LoadFromResourceCorutine();
			yield return _LoadFromAssetdatabaseCorutine();
		}

		//
		private IEnumerator _LoadFromResourceCorutine()
		{
			foreach (var resourceRoot in _reserveResourceRoots)
			{
				var loadedResources = Resources.LoadAll(resourceRoot.rootPath, resourceRoot.type);
				foreach (var resource in loadedResources)
				{
					string path = $"{resourceRoot.rootPath}/{resource.name}";

					AddResource(path, resource, resourceRoot.type);

					_resourceInInstances.Add(path);
				}

				yield return null;
			}
		}

		private IEnumerator _LoadFromAssetdatabaseCorutine()
		{
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
			string assestsPath = "Assets/";

			var rawPaths = AssetDatabase.GetAllAssetPaths();
			var paths = rawPaths.Where(r => r.StartsWith(assestsPath));

			foreach (var assetDatabaseRoot in _reserveAssestDatabaseRoots)
			{
				var targetPaths = paths.Where(p => p.Contains(assetDatabaseRoot.rootPath));

				foreach (var targetPath in targetPaths)
				{
					var resource = AssetDatabase.LoadAssetAtPath(targetPath, assetDatabaseRoot.type);
					if (resource == null)
						continue;

					var originPath = AssetDatabase.GetAssetPath(resource);
					var directoryPath = System.IO.Path.GetDirectoryName(originPath);

					directoryPath = directoryPath.Replace("\\", "/");

					string path = $"{directoryPath}/{resource.name}";

					AddResource(path, resource, assetDatabaseRoot.type);
				}

				yield return null;
			}
#else
			yield return null;
#endif
		}
	}
}
