using BestHTTP;
using CommonPackage.Packet;
using Golbeng.Framework.Commons;
using Golbeng.Framework.Managers.Connection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using pb = Google.Protobuf;

namespace Golbeng.Framework.Managers
{
	public class CPacketManager : Singleton<CPacketManager>
	{
		public enum SendType
		{
			HTTP,
			SIRNALR
		}

		private ILogger _logger;

		private Queue<object> _recivedPackets = new Queue<object>();

		private HttpConnection _httpConnection = null;
		private SignalRConnection _signalRConnection = null;

		public bool IsInitialize { get; private set; } = false;

		public override void OnInitSingleton()
		{
			var defaultLogger = new BestHTTP.Logger.DefaultLogger();
			defaultLogger.Level = BestHTTP.Logger.Loglevels.Error;
			defaultLogger.Output = new BestHTTP.Logger.UnityOutput();

			//HTTPManager.KeepAliveDefaultValue = true;
			HTTPManager.Logger = defaultLogger;

			HTTPManager.HTTP2Settings.MaxConcurrentStreams = 256;
			HTTPManager.HTTP2Settings.InitialStreamWindowSize = 10 * 1024 * 1024;
			HTTPManager.HTTP2Settings.InitialConnectionWindowSize = HTTPManager.HTTP2Settings.MaxConcurrentStreams * 1024 * 1024;
			HTTPManager.HTTP2Settings.MaxFrameSize = 1 * 1024 * 1024;
			HTTPManager.HTTP2Settings.MaxIdleTime = TimeSpan.FromSeconds(120);

			_logger = ManagerProvider.Logger;

			_httpConnection = new HttpConnection(ManagerProvider.HttpConfig.ConnectUrl, ManagerProvider.HttpConfig.SendMethod);

			_signalRConnection = new SignalRConnection(ManagerProvider.SignalRConfig.ConnectUrl,
														ManagerProvider.SignalRConfig.SendMethod,
														ManagerProvider.SignalRConfig.ReceiveMethod);
		}

		public void Connect()
		{
			_signalRConnection.Connect();
		}

		public void Send<T>(T packet, SendType sendType = SendType.HTTP) where T : pb.IMessage<T>
		{
			switch (sendType)
			{
				case SendType.HTTP:
					_httpConnection.Send(packet);
					break;
				case SendType.SIRNALR:
					_signalRConnection.Send(packet);
					break;
			}
		}

		public IEnumerable<object> PopPackets()
		{
			foreach (var recive in _httpConnection.PopPackets())
			{
				yield return recive;
			}

			foreach (var recive in _signalRConnection.PopPackets())
			{
				yield return recive;
			}
		}
	}
}
