using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Golbeng.Framework.Commons;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Golbeng.Framework.Manager
{
	using ConfigureUnit = Dictionary<string, object>;
	

	public class CConfigureManager : Singleton<CConfigureManager>
	{
		private class CCOnfigureValue
		{
			private Dictionary<string, object> _configureValue = new Dictionary<string, object>();

			public CCOnfigureValue(Type valueType)
			{
				ValueType = valueType;
			}

			public void UpdateValue(object value, string versionKey = "default")
			{
				_configureValue.Add(versionKey.ToLower(), value);
			}

			public object DefaultValue
			{
				get => _configureValue.ContainsKey("default") ? _configureValue["default"] : null;
			}

			public object this[string versionKey]
			{
				get
				{
					var lowVersionKey = versionKey.ToLower();
					if (_configureValue.ContainsKey(lowVersionKey) == true)
						return _configureValue[lowVersionKey];

					return DefaultValue;
				}
			}
		
			public Type ValueType { get; }
		}

		private Dictionary<string, Dictionary<string, CCOnfigureValue>> _configureValues = new Dictionary<string, Dictionary<string, CCOnfigureValue>>();
		////////////////////////////////////////////////////////////////////////////
		public string Enviroment { get; set; } = "default";
		
		public IEnumerable<string> FirstKeys
		{
			get
			{
				return _configureValues.Keys;
			}
		}

		public IEnumerable<string> SecondKeys(string firstKey)
		{
			firstKey = firstKey.ToLower();
			if (_configureValues.ContainsKey(firstKey) == false)
				yield break;

			foreach(var key in _configureValues[firstKey].Keys)
			{
				yield return key;
			}
		}

		public T GetConfigureValue<T>(string firstKey, string secondKey)
		{
			var configureValue = FindConfigureValue(firstKey, secondKey);
			if (configureValue == null)
				throw new Exception($"{firstKey}.{secondKey} is not exist");

			if (typeof(T) != configureValue.ValueType)
				throw new Exception($"{firstKey}.{secondKey} type is not same");

			if (string.IsNullOrEmpty(Enviroment) == true)
				return (T)configureValue.DefaultValue;

			return (T)configureValue[Enviroment];
		}

		public object GetConfigureValue(string firstKey, string secondKey)
		{
			var configureValue = FindConfigureValue(firstKey, secondKey);
			if (configureValue == null)
				throw new Exception($"{firstKey}.{secondKey} is not exist");

			if (string.IsNullOrEmpty(Enviroment) == true)
				return configureValue.DefaultValue;

			return configureValue[Enviroment];
		}

		public Type GetConfigureType(string firstKey, string secondKey)
		{
			var configureValue = FindConfigureValue(firstKey, secondKey);
			if(configureValue == null)
				throw new Exception($"{firstKey}.{secondKey} is not exist");

			return configureValue.ValueType;
		}

		private CCOnfigureValue FindConfigureValue(string firstKey, string secondKey)
		{
			firstKey = firstKey.ToLower();
			secondKey = secondKey.ToLower();

			if (_configureValues.ContainsKey(firstKey) == false)
				return null;

			if (_configureValues[firstKey].ContainsKey(secondKey) == false)
				return null;

			return _configureValues[firstKey][secondKey];
		}

		public WaitForTask Initialize(string fileName)
		{
			var waitForTask = new WaitForTask(async () =>
			{
				var jsonContext = await LoadConfigure(fileName);

				if (string.IsNullOrEmpty(jsonContext) == true)
					return;

				JObject rawRoot = JObject.Parse(jsonContext);

				foreach (var rawBundle in rawRoot)
				{
					if (_configureValues.ContainsKey(rawBundle.Key) == true)
						continue;

					var bundle = ParseBundle(rawBundle.Value);

					_configureValues.Add(rawBundle.Key.ToLower(), bundle);
				}
			});

			return waitForTask;
		}

		private Dictionary<string, CCOnfigureValue> ParseBundle(JToken rawBundle)
		{
			Dictionary<string, CCOnfigureValue> bundle = new Dictionary<string, CCOnfigureValue>();

			foreach(JProperty unit in rawBundle)
			{
				CCOnfigureValue configureValue = null;
				// 세부 설정이 있다..
				if (unit.Value.Type == JTokenType.Object)
					configureValue = GetConfigureValueSet(unit);
				else
					configureValue = GetConfigureValue(unit);

				if(configureValue == null)
				{
					Debug.LogError($"[ConfigureManager] {unit.Name} config value is empty");
					continue;
				}

				bundle.Add(unit.Name.ToLower(), configureValue);
			}

			return bundle;
		}

		private CCOnfigureValue GetConfigureValueSet(JProperty property)
		{
			List<(Type type, string name, object value)> setValueList = new List<(Type type, string name, object value)>();

			foreach (JProperty valueUnit in property.Value)
			{
				var parseResult = ParseConfigureValue(valueUnit.Value);
				if(parseResult.value == null)
				{
					Debug.LogError($"[ConfigureManager] {valueUnit.Name} is ParseConfigureValue error");
					continue;
				}

				setValueList.Add((parseResult.type, valueUnit.Name, parseResult.value));
			}

			// 정상적인지 체크 
			if(setValueList.Count == 0)
			{
				Debug.LogError($"[ConfigureManager] {property.Name} is empty");
				return null;
			}
			
			// default 포함?
			bool existDefault = setValueList.Where(v => v.name.Equals("default", StringComparison.OrdinalIgnoreCase)).Any();
			if(existDefault == false)
			{
				Debug.LogError($"[ConfigureManager] {property.Name} is not contain \"default\"");
				return null;
			}

			// 각각 타입이 같은가??
			var smapleType = setValueList.First().type;
			if (setValueList.TrueForAll(v => v.type == smapleType) == false)
			{
				Debug.LogError($"[ConfigureManager] {property.Name} not same each other");
				return null;
			}

			CCOnfigureValue configureValue = new CCOnfigureValue(smapleType);
			setValueList.ForEach(v =>
			{
				configureValue.UpdateValue(v.value, v.name.ToLower());
			});

			return configureValue;
		}

		private CCOnfigureValue GetConfigureValue(JProperty property)
		{
			var result = ParseConfigureValue(property.Value);
			if (result.value == null)
				return null;

			var configureValue =  new CCOnfigureValue(result.type);
			configureValue.UpdateValue(result.value);

			return configureValue;
		}

		private (Type type, object value) ParseConfigureValue(JToken jToken)
		{
			switch (jToken.Type)
			{
				case JTokenType.Integer:
					return (typeof(int), jToken.ToObject<int>());
				case JTokenType.Float:
					return (typeof(float), jToken.ToObject<float>());
				case JTokenType.Boolean:
					return (typeof(bool), jToken.ToObject<bool>());
				default:
					Debug.LogError("not allow Configure Value");
					return (typeof(Nullable), null);
			}
		}

		private async Task<string> LoadConfigure(string fileName)
		{
			if(ManagerProvider.CurrentPlatform == ManagerProvider.Platform.Windows)
			{
				return await _LoadConfigureWindow(fileName);
			}
			else if(ManagerProvider.CurrentPlatform == ManagerProvider.Platform.Android)
			{
				return await _LaodConfigureAndroid(fileName);
			}

			return "";
		}

		private async Task<string> _LoadConfigureWindow(string fileName)
		{
			string streamingAssestsPath = Path.Combine(ManagerProvider.RawConfigureAssestsPath, fileName);
			if (File.Exists(streamingAssestsPath) == false)
				return "";

			var jsonValue = "";
			using (StreamReader sr = new StreamReader(streamingAssestsPath, Encoding.UTF8))
			{
				jsonValue = await sr.ReadToEndAsync();
			}

			return jsonValue;
		}

		private async Task<string> _LaodConfigureAndroid(string fileName)
		{
			string jsonValue = "";

			string streamingAssestsPath = Path.Combine(ManagerProvider.RawConfigureAssestsPath, fileName);
			using (var unityWebRequest = UnityWebRequest.Get(streamingAssestsPath))
			{
				unityWebRequest.timeout = 1000;
				await Task.Run(() =>
				{
					var request = unityWebRequest.SendWebRequest();

					while (request.isDone == false) { }
				});

				jsonValue = Encoding.UTF8.GetString(unityWebRequest.downloadHandler.data);
			}

			return jsonValue;
		}

	}
}
