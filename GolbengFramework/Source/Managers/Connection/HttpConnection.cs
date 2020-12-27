using BestHTTP;
using BestHTTP.Forms;
using CommonPackage.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using pb = Google.Protobuf;

namespace Golbeng.Framework.Managers.Connection
{
	public class HttpConnection
	{
		private string _connectUrl = "";

		private Queue<object> _receivePacketQueue = new Queue<object>();

		public HttpConnection(string connectUrl, string sendMethod)
		{
			_connectUrl = $"{connectUrl}/{sendMethod}";
		}

		public void Send<T>(T packet) where T : pb.IMessage<T>
		{
			var serializePacket = PacketConverter.SerializePacketDescriptionJson(packet);

			var request = new HTTPRequest(new Uri(_connectUrl),
										methodType: HTTPMethods.Post,
										isKeepAlive: false,
										disableCache: true,
										callback: OnReceive);

			
			request.SetHeader("Content-Type", "application/json");
			request.RawData = Encoding.UTF8.GetBytes(serializePacket);

			var req = request.Send();
		}

		private void OnReceive(HTTPRequest req, HTTPResponse res)
		{
			if (res == null)
				return;

			if(res.StatusCode != 200)
				return;

			var packet = res.DataAsText;

			var receivePacket = PacketConverter.DeserializePacketFromJson(packet);
			if (receivePacket == null)
				return;

			_receivePacketQueue.Enqueue(receivePacket);
		}

		public IEnumerable<object> PopPackets()
		{
			while(_receivePacketQueue.Count > 0)
			{
				yield return _receivePacketQueue.Dequeue();
			}
		}
	}
}
