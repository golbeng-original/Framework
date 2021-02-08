using CommonPackage.String;
using Golbeng.Framework.Commons;
using Golbeng.Framework.Loader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Golbeng.Framework.Managers
{
	public class CStringManager : Singleton<CStringManager>
	{
		private readonly static string _staticStringDataFileName = "StringDataBundle.json";


		private string _fullPath = null;
		private IFileLoader _fileLoader = null;

		private List<string> _stringDataFilePaths = new List<string>();
		private Dictionary<string, StringData> _container = new Dictionary<string, StringData>();

		public override void OnInitSingleton()
		{
			_fullPath = System.IO.Path.Combine(ManagerProvider.RawStringDataAssestsPath, _staticStringDataFileName);

			// PC, Editor
			if (ManagerProvider.IsEditMode == true || ManagerProvider.CurrentPlatform == ManagerProvider.Platform.Windows)
			{
				_fileLoader = new CPcFileLoader();
				return;
			}

			// android
			if (ManagerProvider.CurrentPlatform == ManagerProvider.Platform.Android)
			{
				_fileLoader = new CAndroidFileLoader();
			}
			// Iphone
			else if (ManagerProvider.CurrentPlatform == ManagerProvider.Platform.IPhone)
			{
				//
			}
		}

		public void RegisterStringDataFileName(string fileName)
		{
			_stringDataFilePaths.Add(fileName);
		}

		public void LoadStringData()
		{
			foreach (var stringDataFilePath in _stringDataFilePaths)
			{
				string fullPath = System.IO.Path.Combine(ManagerProvider.RawStringDataAssestsPath, stringDataFilePath);

				using (var stream = _fileLoader.Load(fullPath))
				{
					if (stream == null)
					{
						ManagerProvider.Logger?.AddLog("stream is null");
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

		public WaitForTask LoadStringDataAsync()
		{
			var waitForTask = new WaitForTask(() =>
			{
				foreach(var stringDataFilePath in _stringDataFilePaths)
				{
					string fullPath = System.IO.Path.Combine(ManagerProvider.RawStringDataAssestsPath, stringDataFilePath);

					using (var stream = _fileLoader.Load(fullPath))
					{
						if (stream == null)
						{
							ManagerProvider.Logger?.AddLog("stream is null");
							return;
						}

						var result = StringDataContainer.Deserialize(stream);

						var loadContainer = result.Container.StringDataSet;

						if(_container.Count > 0)
						{
							foreach(var stringData in loadContainer)
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
			});

			return waitForTask;
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
