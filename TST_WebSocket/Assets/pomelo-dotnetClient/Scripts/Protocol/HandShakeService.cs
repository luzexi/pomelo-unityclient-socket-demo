using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SimpleJSON;
using UnityEngine;

namespace Pomelo.DotNetClient
{
	public class HandShakeService
	{
		private Protocol protocol;
		private Action<JSONNode> callback;

		public const string Version = "0.3.0";
		public const string Type = "unity-socket";


		public HandShakeService (Protocol protocol)
		{
			this.protocol = protocol;
		}
		
		public void request(JSONNode user, Action<JSONNode> callback)
		{
			byte[] body = Encoding.UTF8.GetBytes(buildMsg(user).ToString());

			protocol.send(PackageType.PKG_HANDSHAKE, body);

			this.callback = callback;
		}

		internal void invokeCallback(JSONNode data)
		{
			//Invoke the handshake callback
			if(callback != null) 
			{
				callback.Invoke(data);
			}
		}

		public void ack()
		{
			protocol.send(PackageType.PKG_HANDSHAKE_ACK, new byte[0]);
		}

		private JSONNode buildMsg(JSONNode theUser) 
		{
			JSONClass user = new JSONClass();
			if(theUser != null) 
			{
				user["usr"] = theUser["user"];
				user["pwd"] = theUser["pwd"];
			}

			JSONClass msg = new JSONClass();

			//Build sys option
			JSONClass sys = new JSONClass();
			sys["version"] = Version;
			sys["type"] = Type;

			//Build handshake message
			msg ["sys"] = sys;
			msg ["user"] = user;

			return msg;	
		}	
	}
}

