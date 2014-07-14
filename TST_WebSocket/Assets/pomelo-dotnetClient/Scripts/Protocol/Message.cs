using System;
using SimpleJSON;

namespace Pomelo.DotNetClient
{
	public class Message
	{
		public MessageType type;
		public string route;
		public uint id;
		public JSONNode data;

		public Message (MessageType type, uint id, string route, JSONNode data)
		{
			this.type = type;
			this.id = id;
			this.route = route;
			this.data = data;
		}
	}
}

