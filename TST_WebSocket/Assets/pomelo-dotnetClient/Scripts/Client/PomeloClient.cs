using System.Collections;
using SimpleJSON;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Pomelo.DotNetClient
{
	public class PomeloClient : IDisposable
	{
		public const string EVENT_DISCONNECT = "disconnect";

		private EventManager eventManager;
		private Socket socket;
		private Protocol protocol;
		private bool disposed = false;
		private uint reqId = 1;
		
		public PomeloClient(string host, int port) 
		{
			this.eventManager = new EventManager();
			InitClient(host, port);
			this.protocol = new Protocol(this, socket);
		}

		private void InitClient(string host, int port) 
		{
			bool aSecurityCheck = Security.PrefetchSocketPolicy(host, 843);

			Debug.Log ("Security Check:" + aSecurityCheck);

			this.socket=new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
	        IPEndPoint ie=new IPEndPoint(IPAddress.Parse(host), port);
	        
			try 
			{
	            this.socket.Connect(ie);
	        }
	        catch (SocketException e) 
			{
	            Console.WriteLine(String.Format("unable to connect to server: {0}", e.ToString()));
	            return;
	        }
		}
		
		public void Connect()
		{
			protocol.start(null, null);
		}
		
		public void Connect(JSONClass user)
		{
			protocol.start(user, null);
		}
		
		public void Connect(Action<JSONNode> handshakeCallback)
		{
			protocol.start(null, handshakeCallback);
		}
		
		public bool Connect(JSONNode user, Action<JSONNode> handshakeCallback)
		{
			try
			{
				protocol.start(user, handshakeCallback);
				return true;
			}
			catch(Exception e)
			{
				Debug.LogError(e.Message);
			}

			return false;
		}

		public void request(string route, Action<JSONNode> action)
		{
			this.request(route, new JSONClass(), action);
		}
		
		public void request(string route, JSONNode msg, Action<JSONNode> action)
		{
			this.eventManager.AddCallBack(reqId, action);
			protocol.send (route, reqId, msg);

			reqId++;
		}
		
		public void notify(string route, JSONNode msg)
		{
			protocol.send (route, msg);
		}
		
		public void on(string eventName, Action<JSONNode> action)
		{
			eventManager.AddOnEvent(eventName, action);
		}

		internal void processMessage(Message msg)
		{
			if(msg.type == MessageType.MSG_RESPONSE)
			{
				eventManager.InvokeCallBack(msg.id, msg.data);
			}
			else if(msg.type == MessageType.MSG_PUSH)
			{
				eventManager.InvokeOnEvent(msg.route, msg.data);
			}
		}

		public void disconnect(){
			Dispose ();
		}

		public void Dispose() 
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// The bulk of the clean-up code 
		protected virtual void Dispose(bool disposing) 
		{
			if(this.disposed)
			{
				return;
			}

			if (disposing) 
			{
				// free managed resources
				this.protocol.close();
				this.socket.Shutdown(SocketShutdown.Both);
				this.socket.Close();
				this.disposed = true;

				//Call disconnect callback
				eventManager.InvokeOnEvent(EVENT_DISCONNECT, null);
			}
		}
	}
}

