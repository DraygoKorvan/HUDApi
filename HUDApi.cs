using Sandbox.ModAPI;
using System;
using VRageMath;
using System.Text;

namespace Draygo.API
{
	
	public class HUDTextNI 
	{
		public struct HUDMessage
		{
			public long id;
			public int ttl;
			public Vector2D Origin;
			public string message;

			/// <summary>
			/// Data to transport for HUD Text Display
			/// </summary>
			/// <param name="messageid">Message id is automatically generated if set to 0. Resend a message with the same ID to overwrite previously sent messages</param>
			/// <param name="timetolive">How many frames a message will live</param>
			/// <param name="origin">Vector 2D, middle of screen is (0,0) up is positive, down is negative, right is positive, left is negative.</param>
			/// <param name="message">Actual message you want to send.&lt;color=colorname&gt; to change the color of the text.</param>
			public HUDMessage(long messageid, int timetolive, Vector2D origin, string message)
			{
				id = messageid;
				ttl = timetolive;
				Origin = origin;
				this.message = message;
			}
		}

		private bool m_heartbeat = false;
		private long m_modId = 0;
		private long currentId = 0;
		private readonly ushort HUDAPI_MESSAGE = 54020;
		private readonly ushort HUDAPI_RECEIVE = 54021;

		public bool Heartbeat
		{
			get
			{
				return m_heartbeat;
			}

			private set
			{
				m_heartbeat = value;
			}
		}

		/// <summary>
		/// You must specify a modId to avoid conflicts with other mods. Just pick a random number, it probably will be fine ;) Please call .Close() during the cleanup of your mod.
		/// </summary>
		/// <param name="modId">ID of your mod, it is recommended you choose a unique one for each mod.</param>
		public HUDTextNI(long modId)
		{
			m_modId = modId;
			MyAPIGateway.Multiplayer.RegisterMessageHandler(HUDAPI_RECEIVE, callback);
		}

		private void callback(byte[] obj)
		{
			m_heartbeat = true;
			return;
		}
		/// <summary>
		/// Gets the next ID for a HUDMessage
		/// </summary>
		/// <returns>ID for a HUDMessage</returns>
		public long GetNextID()
		{
			return ++currentId;
		}
		/// <summary>
		/// Creates and Sends a HUDMessage
		/// </summary>
		/// <param name="timetolive">Time in frames until HUD element expires, not recommended to set this to 1 as it can lead to flicker.</param>
		/// <param name="origin">Vector2D between 1,1 and -1,-1. 0,0 is the middle of the screen.</param>
		/// <param name="message">Actual message you want to send.&lt;color=colorname&gt; to change the color of the text.</param>
		/// <returns>HUDMessage populated with a new ID that can be resent.</returns>
		public HUDMessage CreateAndSend(int timetolive, Vector2D origin, string message)
		{
			return CreateAndSend(GetNextID(), timetolive, origin, message);
		}
		/// <summary>
		/// Creates and Sends a HUDMessage
		/// </summary>
		/// <param name="id">ID of the HUDMessage, if 0 it will choose the next ID.</param>
		/// <param name="timetolive">Time in frames until HUD element expires, not recommended to set this to 1 as it can lead to flicker.</param>
		/// <param name="origin">Vector2D between 1,1 and -1,-1.</param>
		/// <param name="message">Actual message you want to send.&lt;color=colorname&gt; to change the color of the text.</param>
		/// <returns>HUDMessage that can be resent.</returns>
		public HUDMessage CreateAndSend(long id, int timetolive, Vector2D origin, string message)
		{
			HUDMessage Hmessage = new HUDMessage(id, timetolive, origin, message);
			return Send(Hmessage);
		}

		/// <summary>
		/// Sends an already constructed HUDMessage, if HUDMessage has an ID of 0 it will pick the next ID.
		/// </summary>
		/// <param name="message">HUDMessage being sent or resent.</param>
		/// <returns>Returns HUDMessage with populated ID</returns>
		public HUDMessage Send(HUDMessage message)
		{
			if (message.id == 0)
			{

				message.id = GetNextID();
            }
				

			byte[] modid = BitConverter.GetBytes(m_modId);
			byte[] mid = BitConverter.GetBytes(message.id);
			byte[] ttl = BitConverter.GetBytes(message.ttl);
			byte[] vx = BitConverter.GetBytes(message.Origin.X);
			byte[] vy = BitConverter.GetBytes(message.Origin.Y);
			byte[] encode = Encoding.UTF8.GetBytes(message.message);
			byte[] msg = new byte[modid.Length + mid.Length + ttl.Length + vx.Length + vy.Length + encode.Length];
			modid.CopyTo(msg, 0);
			mid.CopyTo(msg, modid.Length);
			ttl.CopyTo(msg, modid.Length + mid.Length);
			vx.CopyTo(msg, modid.Length + mid.Length + ttl.Length);
			vy.CopyTo(msg, modid.Length + mid.Length + ttl.Length + vx.Length);
			encode.CopyTo(msg, modid.Length + mid.Length + ttl.Length + vx.Length + vy.Length);
			MyAPIGateway.Multiplayer.SendMessageTo(HUDAPI_MESSAGE, msg, MyAPIGateway.Multiplayer.MyId, true);
            return message;
		}

		public void Close()
		{
			MyAPIGateway.Multiplayer.UnregisterMessageHandler(HUDAPI_RECEIVE, callback);//remove
		}
	}
}
