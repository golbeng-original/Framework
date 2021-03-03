using Golbeng.Framework.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Golbeng.Framework.Commons
{

	public class ManagerProvider
	{
		public enum Platform
		{ 
			None,
			Android,
			IPhone,
			Windows,
		};

		public class HttpConfigure
		{
			public string ConnectUrl { get; set; }
			public string SendMethod { get; set; }
		}


		public class WebSocketConfigure
		{ 
			public string ConnectUrl { get; set; }
			public string SendMethod { get; set; }
			public string ReceiveMethod { get; set; }
		}

		private static readonly string _staticReleativeConfigurePath = "Data/Config";
		private static readonly string _staticReleativeTablePath = "Data/Table";
		private static readonly string _staticReleativeStringPath = "Data/String";

		public static Platform CurrentPlatform { get; set; } = Platform.None;

		public static bool IsEditMode { get; set; } = false;

		public static string StreamingAssestsPath { get; set; } = "";

		public static string PersistentDataPath { get; set; } = "";

		public static string LoadConfigureAssestsPath { get => System.IO.Path.Combine(PersistentDataPath, _staticReleativeConfigurePath); }
		public static string RawConfigureAssestsPath { get => System.IO.Path.Combine(StreamingAssestsPath, _staticReleativeConfigurePath); }

		public static string LoadTableAssestsPath { get => System.IO.Path.Combine(PersistentDataPath, _staticReleativeTablePath); }
		public static string RawTableAssestsPath { get => System.IO.Path.Combine(StreamingAssestsPath, _staticReleativeTablePath); }
		public static string LoadStringDataAssestsPath { get => System.IO.Path.Combine(PersistentDataPath, _staticReleativeStringPath); }
		public static string RawStringDataAssestsPath { get => System.IO.Path.Combine(StreamingAssestsPath, _staticReleativeStringPath); }

		public static string ConfigureAssetPath { get => "Assets/_Data/Config"; }
		public static string StringAssetPath { get => "Assets/_Data/String"; }
		public static string TableAssetPath { get => "Assets/_Data/Table"; }

		public static HttpConfigure HttpConfig { get; private set; } = new HttpConfigure();

		public static WebSocketConfigure SignalRConfig { get; private set; } = new WebSocketConfigure();

		private static ILogger _logger = new CDefaultLogger();
		public static ILogger Logger 
		{
			get => _logger;
			set
			{
				if(_logger.GetType() == value.GetType())
					return;

				if (_logger != null)
					_logger.Dispose();

				_logger = value;
			}
		} 
	}
}
