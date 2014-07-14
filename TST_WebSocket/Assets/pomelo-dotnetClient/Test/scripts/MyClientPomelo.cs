using UnityEngine;
using System.Collections;
using Pomelo.DotNetClient;
using System.Collections.Generic;
using SimpleJSON;

public class MyClientPomelo : MonoBehaviour 
{
	public string itsURL;
	public int itsPort;
	public PomeloClient itsClient;

	private string itsLogString = "";

	void OnGUI()
	{
		if (GUILayout.Button ("Connect", GUILayout.Height (90), GUILayout.Width (90))) 
		{
			Connect ();
		}

		if (GUILayout.Button ("Send Message", GUILayout.Height (90), GUILayout.Width (90))) 
		{
			SendMessage();
		}
		/*
		if (GUILayout.Button ("Close", GUILayout.Height (90), GUILayout.Width (90))) 
		{
		}
		*/

		GUILayout.Label(itsLogString,GUILayout.Width(300),GUILayout.Height(300));
	}

	private void Connect()
	{
		itsClient = new PomeloClient(itsURL, itsPort);

		//The user data is the handshake user params
		//JsonObject user = new JsonObject();
		//user.Add(new KeyValuePair<string,object>("user","flo"));
		itsClient.Connect(null,new System.Action<JSONNode>(OnConnection));
	}

	private void OnConnection(JSONNode theJsonClass)
	{
		Log ("Connection Established: "  + theJsonClass.ToString());
	}


	private void SendMessage()
	{
		JSONClass msg = new JSONClass();
		msg["msg"] = "Hello World!";

		itsClient.request("connector.entryHandler.entry", msg,new System.Action<JSONNode>(OnSendMessage));
	}

	private void OnSendMessage(JSONNode theJsonClass)
	{
		Log ("Message Received: "  + theJsonClass.ToString());
	}

	private void Log(string theString)
	{
		itsLogString += theString +  "\n";
		
		Debug.Log (theString);
	}

}
