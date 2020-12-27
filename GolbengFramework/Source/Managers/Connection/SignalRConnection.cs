using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using CommonPackage.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pb = Google.Protobuf;

namespace Golbeng.Framework.Managers.Connection
{
	public class SignalRConnection
	{
		private string _connectUrl = "";
		private string _sendMethod = "";
		private string _receiveMethod = "";

		private Queue<object> _receivePacketQueue = new Queue<object>();

		private HubConnection _hub = null;

		public bool IsConnected { get => _hub.State == ConnectionStates.Connected; }

		public SignalRConnection(string connectUrl, string sendMethod, string ReceiveMethod)
		{
			_connectUrl = connectUrl;
			_sendMethod = sendMethod;
			_receiveMethod = ReceiveMethod;

			HubOptions options = new HubOptions();
			_hub = new HubConnection(new Uri(_connectUrl), new JsonProtocol(new LitJsonEncoder()));

			_hub.OnConnected += OnConnected;
			_hub.OnError += OnError;
			_hub.OnClosed += OnClosed;
			_hub.OnReconnecting += (hub, value) =>
			{
				var h = hub;
			};

			_hub.On<PacketDescription>(_receiveMethod, OnReceive);
		}

		public void Connect()
		{
			_hub.StartConnect();
		}

		public void Disconnect()
		{
			if (IsConnected == false)
				return;

			_hub.CloseAsync();
		}

		public async void Send<T>(T packet) where T : pb.IMessage<T>
		{
			if (IsConnected == false)
				return;

			var packetDescription = PacketConverter.SerializePacketDescription(packet);

			await _hub.SendAsync(_sendMethod, packetDescription);
		}
		private void OnReceive(PacketDescription packetDescription)
		{
			var receivePacket = PacketConverter.DeserializePacket(packetDescription);
			if (receivePacket == null)
				return;

			_receivePacketQueue.Enqueue(receivePacket);
		}

		private void OnConnected(HubConnection hub)
		{
			var h = hub;
		}

		private void OnError(HubConnection hub, string error)
		{
			var h = hub;
		}

		private void OnClosed(HubConnection hub)
		{
			var h = hub;
		}

		public IEnumerable<object> PopPackets()
		{
			while (_receivePacketQueue.Count > 0)
			{
				yield return _receivePacketQueue.Dequeue();
			}
		}
	}
}
