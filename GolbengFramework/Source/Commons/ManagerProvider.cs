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


		private static readonly string _staticReleativeTablePath = "Data/Table";

		public static Platform CurrentPlatform { get; set; } = Platform.None;

		public static bool IsEditMode { get; set; } = false;

		public static string StreamingAssestsPath { get; set; } = "";

		public static string PersistentDataPath { get; set; } = "";

		public static string LoadTableAssestsPath { get => System.IO.Path.Combine(PersistentDataPath, _staticReleativeTablePath); }

		public static string RawTableAssestsPath { get => System.IO.Path.Combine(StreamingAssestsPath, _staticReleativeTablePath); }
		public static HttpConfigure HttpConfig { get; private set; } = new HttpConfigure();

		public static WebSocketConfigure SignalRConfig { get; private set; } = new WebSocketConfigure();

		public static ILogger Logger { get; set; } = new BaseLogger();
	}
}
