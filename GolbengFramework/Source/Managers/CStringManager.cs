﻿using CommonPackage.String;
using Golbeng.Framework.Commons;
using Golbeng.Framework.Loader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Golbeng.Framework.Managers
{
	public class CStringManager : Singleton<CStringManager>
	{
		private List<string> _stringDataFilePaths = new List<string>();
		private Dictionary<string, StringData> _container = new Dictionary<string, StringData>();

		public void RegisterStringDataFileName(string fileName)
		{
			_stringDataFilePaths.Add(fileName);
		}

		public void LoadStringData()
		{
			foreach (var stringDataFilePath in _stringDataFilePaths)
			{
				string fullPath = System.IO.Path.Combine(ManagerProvider.StringAssetPath, stringDataFilePath);

				using (var stream = CResourceManager.Instance.FindStreamResource(fullPath))
				{
					if (stream == null)
					{
						ManagerProvider.Logger?.Warning("StringManager", "stream is null");
						return;
					}

					var result = StringDataContainer.Deserialize(stream);

					var loadContainer = result.Container.StringDataSet;

					if (_container.Count > 0)
					{
						foreach (var stringData in loadContainer)
						{
							string key = stringData.Key.ToLower();

							if (_container.ContainsKey(key) == false)
								continue;

							_container.Add(key, stringData);
						}
					}
					else
					{
						_container = loadContainer.ToDictionary(s =>
						{
							return s.Key.ToLower();
						});
					}
				}
			}
		}

		public WaitForCoroutineTasks LoadStringDataAsync(MonoBehaviour dispatcher)
		{
			var waitForCoroutineTask = new WaitForCoroutineTasks(dispatcher);
			waitForCoroutineTask.RegisterTask(() =>
			{
				foreach (var stringDataFilePath in _stringDataFilePaths)
				{
					string fullPath = System.IO.Path.Combine(ManagerProvider.StringAssetPath, stringDataFilePath);

					using (var stream = CResourceManager.Instance.FindStreamResource(fullPath))
					{
						if (stream == null)
						{
							ManagerProvider.Logger.Warning("StringManager", "stream is null");
							return;
						}

						try
						{
							var result = StringDataContainer.Deserialize(stream);

							var loadContainer = result.Container.StringDataSet;

							if (_container.Count > 0)
							{
								foreach (var stringData in loadContainer)
								{
									string key = stringData.Key.ToLower();

									if (_container.ContainsKey(key) == false)
										continue;

									_container.Add(key, stringData);
								}
							}
							else
							{
								_container = loadContainer.ToDictionary(s =>
								{
									return s.Key.ToLower();
								});
							}
						}
						catch (Exception e)
						{
							ManagerProvider.Logger.Exception("StringManager", "LoadStringDataAsync Exception", e);
						}
					}
				}
			});

			waitForCoroutineTask.Run();

			return waitForCoroutineTask;
		}

		public IEnumerable<StringData> GetStringDatas()
		{
			foreach(var stringData in _container.Values)
			{
				yield return stringData;
			}
		}

		public string GetStringData(string key)
		{
			var rawData = GetStringDataRaw(key);
			if (rawData == null)
				return "";

			return rawData.Data;
		}

		public StringData GetStringDataRaw(string key)
		{
			key = key.ToLower();
			if (_container.ContainsKey(key) == false)
				return null;

			return _container[key];
		}
	}
}
