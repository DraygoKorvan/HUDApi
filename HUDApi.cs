using Sandbox.ModAPI;
using System;
using VRageMath;
using System.Text;
using System.Linq;
namespace Draygo.API
{

	public class HUDTextNI
	{
		public enum TextOrientation : byte
		{
			ltr = 1,
			center = 2,
			rtl = 3
		}
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
		public struct SpaceMessage
		{
			public long id;
			public int ttl;
			public Vector3D pos;
			public Vector3D up;
			public Vector3D left;
			public double scale;
			public string message;
			public TextOrientation TOrientation;
			/// <summary>
			/// Data to transport for Message in 3D space
			/// </summary>
			/// <param name="messageid">Message ID, if set to 0 it will be poplulated with an ID</param>
			/// <param name="timetolive">Time to live in frames</param>
			/// <param name="scale">Scale</param>
			/// <param name="position">World Position</param>
			/// <param name="Up">Up direction of text</param>
			/// <param name="Left">Left Direction of text</param>
			/// <param name="message">Actual message you want to send.&lt;color=colorname&gt; to change the color of the text.</param>
			/// <param name="orientation">left to right (ltr), center, and right to left (rtl) determines how text is laid out relative to pos.</param>
			public SpaceMessage(long messageid, int timetolive, double scale, Vector3D position, Vector3D Up, Vector3D Left, string message, TextOrientation orientation = TextOrientation.ltr )
			{
				id = messageid;
				ttl = timetolive;
				this.scale = scale;
				pos = position;
				up = Up;
				left = Left;
				TOrientation = orientation;
				this.message = message;
			}
		}
		public struct EntityMessage
		{
			public long id;
			public int ttl;
			public long EntityId;
			public Vector3D rel;
			public Vector3D up;
			public Vector3D forward;
			public double scale;
			public string message;
			public TextOrientation TOrientation;
			public Vector2D Max;
			/// <summary>
			/// Data to transport Entity attached message
			/// </summary>
			/// <param name="messageid">Message ID, if set to 0 this will be populated by the Send method</param>
			/// <param name="timetolive">Time to live in frames</param>
			/// <param name="scale">Scale</param>
			/// <param name="EntityId">Entity ID to attach to</param>
			/// <param name="localposition">Position relative to the entity ID</param>
			/// <param name="Up">Up direction relative to the entity</param>
			/// <param name="Forward">Forward direction relative to the entity</param>
			/// <param name="message">Actual message you want to send.&lt;color=colorname&gt; to change the color of the text.</param>
			/// <param name="max_x">maximum in the x direction the text can fill (to the left)  0 is unlimited</param>
			/// <param name="max_y">maximum in the y direction that the text can fill (down) 0 is unlimited</param>
			/// <param name="orientation">left to right (ltr), center, and right to left (rtl) determines how text is laid out relative to the Entity.</param>
			public EntityMessage(long messageid, int timetolive, double scale, long EntityId, Vector3D localposition, Vector3D Up, Vector3D Forward, string message, double max_x = 0, double max_y = 0, TextOrientation orientation = TextOrientation.ltr)
            {
				id = messageid;
				ttl = timetolive;
				this.scale = scale;
				this.EntityId = EntityId;
				rel = localposition;
				up = Up;
				forward = Forward;
				Max = new Vector2D(max_x, max_y);
				TOrientation = orientation;
				this.message = message;
			}
		}
		private bool m_heartbeat = false;
		private long m_modId = 0;
		private long currentId = 1000;
		private readonly ushort HUDAPI_ADVMSG = 54019;
		private readonly ushort HUDAPI_MESSAGE = 54020;
		private readonly ushort HUDAPI_RECEIVE = 54021;
		private readonly ushort MOD_VER = 1;

		/// <summary>
		/// True if HUDApi is installed and initialized. Please wait a few seconds and try again if it isn't ready yet. 
		/// </summary>
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
		/// Gets the next ID for a HUDMessage, starts counting from 1000. Under 1000 is reserved for manual usage. 
		/// </summary>
		/// <returns>ID for a HUDMessage</returns>
		public long GetNextID()
		{
			return currentId++;
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
			var msg = Encode(ref message);
			MyAPIGateway.Multiplayer.SendMessageTo(HUDAPI_MESSAGE, msg, MyAPIGateway.Multiplayer.MyId, true);
			return message;
		}
		/// <summary>
		/// Sends an already constructed SpaceMessage, if Spacemessage has an ID of 0 it will pick the next id. 
		/// </summary>
		/// <param name="message">SpaceMessage being sent or resent</param>
		/// <returns>Returns SpaceMessage with populated ID</returns>
		public SpaceMessage Send(SpaceMessage message)
		{
			var msg = Encode(ref message);
			MyAPIGateway.Multiplayer.SendMessageTo(HUDAPI_ADVMSG, msg, MyAPIGateway.Multiplayer.MyId, true);
			return message;
		}
		/// <summary>
		/// Sends an Entity Attached Message. Entity Messages will stick to the assigned entity. 
		/// </summary>
		/// <param name="message">EntityMessage being sent or resent</param>
		/// <returns>EntityMessage with Populated ID</returns>
		public EntityMessage Send(EntityMessage message)
		{
			var msg = Encode(ref message);
			MyAPIGateway.Multiplayer.SendMessageTo(HUDAPI_ADVMSG, msg, MyAPIGateway.Multiplayer.MyId, true);
			return message;
		}
		/// <summary>
		/// Send already constructed HUDMessage to others. If the HUDMessage has an ID of 0 it will pick the next ID.
		/// </summary>
		/// <param name="message">HUDMessage being sent or resent.</param>
		/// <returns>Returns HUDMessage with populated ID</returns>
		public HUDMessage SendToOthers(HUDMessage message)
		{
			var msg = Encode(ref message);
			MyAPIGateway.Multiplayer.SendMessageToOthers(HUDAPI_MESSAGE, msg, true);
			return message;
		}
		/// <summary>
		/// Sends an already constructed SpaceMessage to others, if Spacemessage has an ID of 0 it will pick the next id. 
		/// </summary>
		/// <param name="message">SpaceMessage being sent or resent</param>
		/// <returns>Returns SpaceMessage with populated ID</returns>
		public SpaceMessage SendToOthers(SpaceMessage message)
		{
			var msg = Encode(ref message);
			MyAPIGateway.Multiplayer.SendMessageToOthers(HUDAPI_ADVMSG, msg, true);
			return message;
		}
		/// <summary>
		/// Sends an Entity Attached Message to others. Entity Messages will stick to the assigned entity. 
		/// </summary>
		/// <param name="message">EntityMessage being sent or resent</param>
		/// <returns>EntityMessage with Populated ID</returns>
		public EntityMessage SendToOthers(EntityMessage message)
		{
			var msg = Encode(ref message);
			MyAPIGateway.Multiplayer.SendMessageToOthers(HUDAPI_ADVMSG, msg, true);
			return message;
		}
		private byte[] Encode(ref HUDMessage message)
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
			return msg;
		}
		private byte[] Encode(ref SpaceMessage message)
		{
			ushort msgtype = 1;
			if (message.id == 0)
			{

				message.id = GetNextID();
			}

			byte[] ver = BitConverter.GetBytes(MOD_VER);
			byte[] type = BitConverter.GetBytes(msgtype);
			byte[] modid = BitConverter.GetBytes(m_modId);
			byte[] mid = BitConverter.GetBytes(message.id);
			byte[] ttl = BitConverter.GetBytes(message.ttl);
			byte[] orient = new byte[1] { (byte)message.TOrientation };
			byte[] pos = Encode(message.pos);
			byte[] up = Encode(message.up);
			byte[] left = Encode(message.left);
			byte[] scale = BitConverter.GetBytes(message.scale);
			byte[] encode = Encoding.UTF8.GetBytes(message.message);
			byte[] msg = new byte[ver.Length + type.Length + modid.Length + mid.Length + ttl.Length + pos.Length + up.Length + left.Length + scale.Length + encode.Length + 1];
			int lth = 0;
			Copy(ref msg, ref ver, ref lth);
			Copy(ref msg, ref type, ref lth);
			Copy(ref msg, ref modid, ref lth);
			Copy(ref msg, ref mid, ref lth);
			Copy(ref msg, ref ttl, ref lth);
			Copy(ref msg, ref orient, ref lth);
			Copy(ref msg, ref pos, ref lth);
			Copy(ref msg, ref up, ref lth);
			Copy(ref msg, ref left, ref lth);
			Copy(ref msg, ref scale, ref lth);
			Copy(ref msg, ref encode, ref lth);
			return msg;
		}



		private byte[] Encode(ref EntityMessage message)
		{
			ushort msgtype = 2;
			if (message.id == 0)
			{

				message.id = GetNextID();
			}

			byte[] ver = BitConverter.GetBytes(MOD_VER);
			byte[] type = BitConverter.GetBytes(msgtype);
			byte[] modid = BitConverter.GetBytes(m_modId);
			byte[] mid = BitConverter.GetBytes(message.id);
			byte[] ttl = BitConverter.GetBytes(message.ttl);
			byte[] orient = new byte[1] { (byte)message.TOrientation };
			byte[] entity = BitConverter.GetBytes(message.EntityId);
			byte[] mx = BitConverter.GetBytes(message.Max.X);
			byte[] my = BitConverter.GetBytes(message.Max.Y);
			byte[] rel = Encode(message.rel);
			byte[] up = Encode(message.up);
			byte[] forward = Encode(message.forward);
			byte[] scale = BitConverter.GetBytes(message.scale);
			byte[] encode = Encoding.UTF8.GetBytes(message.message);
			byte[] msg = new byte[ver.Length + type.Length + modid.Length + mid.Length + ttl.Length + entity.Length + rel.Length + up.Length + forward.Length + scale.Length + encode.Length + 1 + mx.Length + my.Length];

			int lth = 0;
			Copy(ref msg, ref ver, ref lth);
			Copy(ref msg, ref type, ref lth);
			Copy(ref msg, ref modid, ref lth);
			Copy(ref msg, ref mid, ref lth);
			Copy(ref msg, ref ttl, ref lth);
			Copy(ref msg, ref entity, ref lth);
			Copy(ref msg, ref orient, ref lth);
			Copy(ref msg, ref mx, ref lth);
			Copy(ref msg, ref my, ref lth);
			Copy(ref msg, ref rel, ref lth);
			Copy(ref msg, ref up, ref lth);
			Copy(ref msg, ref forward, ref lth);
			Copy(ref msg, ref scale, ref lth);
			Copy(ref msg, ref encode, ref lth);
			return msg;
		}

		
		private byte[] Encode(Vector3D vec)
		{
			byte[] x = BitConverter.GetBytes(vec.X);
			byte[] y = BitConverter.GetBytes(vec.Y);
			byte[] z = BitConverter.GetBytes(vec.Z);
			byte[] retval = new byte[x.Length + y.Length + z.Length];
			x.CopyTo(retval, 0);
			y.CopyTo(retval, x.Length);
			z.CopyTo(retval, x.Length + y.Length);
			return retval;
		}
		private void Copy(ref byte[] message, ref byte[] item, ref int lth)
		{
			item.CopyTo(message, lth);
			lth += item.Length;
		}
		/// <summary>
		/// Call when done.
		/// </summary>
		public void Close()
		{
			MyAPIGateway.Multiplayer.UnregisterMessageHandler(HUDAPI_RECEIVE, callback);//remove
		}
	}
}
