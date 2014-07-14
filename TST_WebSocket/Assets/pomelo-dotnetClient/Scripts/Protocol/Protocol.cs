using System;
using SimpleJSON;
using System.Text;
using UnityEngine;

namespace Pomelo.DotNetClient
{
	public class Protocol
	{
		private MessageProtocol messageProtocol;
		private ProtocolState state;
		private Transporter transporter;
		private HandShakeService handshake;
		private HeartBeatService heartBeatService = null;
		private PomeloClient pc;

		public PomeloClient getPomeloClient()
		{
			return this.pc;
		}

		public Protocol(PomeloClient pc, System.Net.Sockets.Socket socket)
		{
			this.pc = pc;
			this.transporter = new Transporter (socket, this.processMessage);
			this.transporter.onDisconnect = onDisconnect;

			this.handshake = new HandShakeService(this);
			this.state = ProtocolState.start;
		}

		internal void start(JSONNode user, Action<JSONNode> callback)
		{
			this.transporter.start();
			this.handshake.request(user, callback);

			this.state = ProtocolState.handshaking;
		}
		
		//Send notify, do not need id
		internal void send(string route, JSONNode msg)
		{
			send (route, 0, msg);
		}

		//Send request, user request id 
		internal void send(string route, uint id, JSONNode msg)
		{
			if(this.state != ProtocolState.working) return;

			byte[] body = messageProtocol.encode (route, id, msg); 

			send (PackageType.PKG_DATA, body);
		}

		internal void send(PackageType type)
		{
			if(this.state == ProtocolState.closed) return;
			transporter.send(PackageProtocol.encode (type));
		}

		//Send system message, these message do not use messageProtocol
		internal void send(PackageType type, JSONNode msg)
		{
			//This method only used to send system package
			if(type == PackageType .PKG_DATA) return;
			
			byte[] body = Encoding.UTF8.GetBytes(msg.ToString());

			send (type, body);
		}

		//Send message use the transporter
		internal void send(PackageType type, byte[] body){
			if(this.state == ProtocolState.closed) return;

			byte[] pkg = PackageProtocol.encode (type, body);

			transporter.send(pkg);
		}
	
		//Invoke by Transporter, process the message
		internal void processMessage(byte[] bytes)
		{
			Package pkg = PackageProtocol.decode(bytes);

			//Ignore all the message except handshading at handshake stage
			if (pkg.type == PackageType.PKG_HANDSHAKE && this.state == ProtocolState.handshaking) 
			{

				//Ignore all the message except handshading
				JSONNode data = JSONNode.Parse(Encoding.UTF8.GetString(pkg.body));

				processHandshakeData(data);

				this.state = ProtocolState.working;

			}
			else if (pkg.type == PackageType.PKG_HEARTBEAT && this.state == ProtocolState.working)
			{
				this.heartBeatService.resetTimeout();
			}
			else if (pkg.type == PackageType.PKG_DATA && this.state == ProtocolState.working) 
			{
				this.heartBeatService.resetTimeout();
				pc.processMessage(messageProtocol.decode (pkg.body));
			}
			else if (pkg.type == PackageType.PKG_KICK) 
			{
				this.close();
			}
		}
		
		private void processHandshakeData(JSONNode msg)
		{
			Debug.Log (msg.GetType().ToString() + "THE MESSAGE:" + msg.ToString());
			//Handshake error
			if(!msg.ContainsKey("code") || !msg.ContainsKey("sys") || Convert.ToInt32(msg["code"]) != 200)
			{
				throw new Exception("Handshake error! Please check your handshake config.");
			}

			//Set compress data
			JSONNode sys = msg["sys"];

			JSONNode dict = new JSONNode();
			if(sys.ContainsKey("dict")) 
			{
				dict = sys["dict"];
			}

			JSONClass protos = new JSONClass();
			JSONClass serverProtos = new JSONClass();
			JSONClass clientProtos = new JSONClass();

			if(sys.ContainsKey("protos"))
			{
				protos.Add(sys["protos"]);
				serverProtos.Add(protos["server"]);
				clientProtos.Add(protos["client"]);

				//protos = (JSONClass)sys["protos"];
				//serverProtos = (JSONClass)protos["server"];
				//clientProtos = (JSONClass)protos["client"];
			}

			messageProtocol = new MessageProtocol(dict, serverProtos, clientProtos);

			//messageProtocol = new MessageProtocol(msg["sys"]["dict"],msg["protos"]["server"],msg["protos"]["client"]);

			//Init heartbeat service
			int interval = 0;
			if(msg["sys"].ContainsKey("heartbeat"))
			{
				interval = Convert.ToInt32(msg["sys"]["heartbeat"]);
			}
			heartBeatService = new HeartBeatService(interval, this);

			if(interval > 0)
			{
				heartBeatService.start();
			}


			//send ack and change protocol state
			handshake.ack();
			this.state = ProtocolState.working;


			//Invoke handshake callback
			JSONClass user = new JSONClass();
			if(msg.ContainsKey("user")) 
			{
				user.Add(msg["user"]);
			}

			handshake.invokeCallback(user);
		}

		//The socket disconnect
		private void onDisconnect(){
			this.pc.disconnect();
		}

		internal void close(){
			transporter.close();

			if(heartBeatService != null) heartBeatService.stop();

			this.state = ProtocolState.closed;
		}
	}
}

