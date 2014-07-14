using System;
using SimpleJSON;


namespace Pomelo.Protobuf
{
	public class Protobuf
	{
		private MsgDecoder decoder;
		private MsgEncoder encoder;
		
		public Protobuf (JSONNode encodeProtos, JSONNode decodeProtos)
		{
			this.encoder = new MsgEncoder(encodeProtos);
			this.decoder = new MsgDecoder(decodeProtos);
		}
		
		public byte[] encode(string route, JSONNode msg)
		{
			return encoder.encode(route, msg);
		}
		
		public JSONNode decode(string route, byte[] buffer){
			return decoder.decode(route, buffer);
		}
	}
}

