// from https://www.codeproject.com/Articles/17561/Programmatically-adding-attachments-to-emails-in-C
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using SIL.PlatformUtilities;

namespace SIL.Email
{
	internal class MAPI
	{
		private bool _useUnicode = Environment.OSVersion.Version >= new Version(6, 2);

		public bool AddRecipientTo(string email)
		{
			return AddRecipient(email, HowTo.MAPI_TO);
		}

		public bool AddRecipientCc(string email)
		{
			return AddRecipient(email, HowTo.MAPI_CC);
		}

		public bool AddRecipientBcc(string email)
		{
			return AddRecipient(email, HowTo.MAPI_BCC);
		}

		public void AddAttachment(string strAttachmentFileName)
		{
			m_attachments.Add(strAttachmentFileName);
		}

		public bool SendMailPopup(string strSubject, string strBody)
		{
			return SendMail(strSubject, strBody, MAPI_LOGON_UI | MAPI_DIALOG);
		}

		public bool SendMailDirect(string strSubject, string strBody)
		{
			return SendMail(strSubject, strBody, MAPI_LOGON_UI);
		}


		[DllImport("MAPI32.DLL", CharSet = CharSet.Ansi)]
		private static extern int MAPISendMail(IntPtr sess, IntPtr hwnd, MapiMessage message, int flg, int rsv);

		[DllImport("MAPI32.DLL", CharSet = CharSet.Unicode)]
		private static extern int MAPISendMailW(IntPtr sess, IntPtr hwnd, MapiMessageW message, int flg, int rsv);

		/// <summary>
		///
		/// </summary>
		/// <param name="strSubject"></param>
		/// <param name="strBody"></param>
		/// <param name="how"></param>
		/// <returns>true if successful</returns>
		private bool SendMail(string strSubject, string strBody, int how)
		{
			var msg = new MapiMessage {
				subject = strSubject,
				noteText = strBody
			};

			msg.recips = GetRecipients(out msg.recipCount);
			msg.files = GetAttachments(out msg.fileCount);

			m_lastError = _useUnicode
				? MAPISendMailW(new IntPtr(0), new IntPtr(0), new MapiMessageW(msg), how, 0)
				: MAPISendMail(new IntPtr(0), new IntPtr(0), msg, how, 0);
			//            if (m_lastError > 1)
			//                MessageBox.Show("MAPISendMail failed! " + GetLastError(), "MAPISendMail");
			//todo if(m_lastError==25)
			//bad recipient
			var success = m_lastError == 0; // m_lastError gets reset by Cleanup()

			Cleanup(ref msg);
			return success;//NB: doesn't seem to catch user "denial" using outlook's warning dialog
		}

		private bool AddRecipient(string email, HowTo howTo)
		{
			var recipient = new MapiRecipDesc {
				recipClass = (int) howTo,
				name = email
			};

			// Note: For Outlook Express it would be better to also set recipient.address so that it
			// shows the email address in the confirmation dialog, but this messes up things in
			// Outlook and Windows Mail.
			m_recipients.Add(recipient);

			return true;
		}

		private IntPtr GetRecipients(out int recipCount)
		{
			recipCount = 0;
			if (m_recipients.Count == 0)
				return IntPtr.Zero;

			var size = Marshal.SizeOf(_useUnicode ? typeof(MapiRecipDescW) : typeof
			(MapiRecipDesc));
			var intPtr = Marshal.AllocHGlobal(m_recipients.Count * size);

			var ptr = intPtr;
			foreach (var mapiDesc in m_recipients)
			{
				if (_useUnicode)
					Marshal.StructureToPtr(new MapiRecipDescW(mapiDesc), ptr, false);
				else
					Marshal.StructureToPtr(mapiDesc, ptr, false);
				IntPtr.Add(ptr, size);
			}

			recipCount = m_recipients.Count;
			return intPtr;
		}

		private IntPtr GetAttachments(out int fileCount)
		{
			fileCount = 0;
			if (m_attachments == null)
				return IntPtr.Zero;

			if ((m_attachments.Count <= 0) || (m_attachments.Count > maxAttachments))
				return IntPtr.Zero;

			var size = Marshal.SizeOf(_useUnicode ? typeof(MapiFileDescW) : typeof(MapiFileDesc));
			var intPtr = Marshal.AllocHGlobal(m_attachments.Count * size);

			var mapiFileDesc = new MapiFileDesc { position = -1 };
			var ptr = intPtr;

			foreach (var strAttachment in m_attachments)
			{
				mapiFileDesc.name = Path.GetFileName(strAttachment);
				mapiFileDesc.path = strAttachment;
				if (_useUnicode)
					Marshal.StructureToPtr(new MapiFileDescW(mapiFileDesc), ptr, false);
				else
					Marshal.StructureToPtr(mapiFileDesc, ptr, false);
				IntPtr.Add(ptr, size);
			}

			fileCount = m_attachments.Count;
			return intPtr;
		}

		private void Cleanup(ref MapiMessage msg)
		{
			FreeStruct(_useUnicode ? typeof(MapiRecipDescW) : typeof(MapiRecipDesc),
				msg.recips, msg.recipCount);
			FreeStruct(_useUnicode ? typeof(MapiFileDescW) : typeof(MapiFileDesc),
				msg.files, msg.fileCount);

			m_recipients.Clear();
			m_attachments.Clear();
			m_lastError = 0;

			void FreeStruct(Type type, IntPtr structPtr, int count)
			{
				var size = Marshal.SizeOf(type);

				if (structPtr == IntPtr.Zero)
					return;

				var ptr = structPtr;
				for (var i = 0; i < count; i++)
				{
					Marshal.DestroyStructure(ptr, type);
					IntPtr.Add(ptr, size);
				}
				Marshal.FreeHGlobal(structPtr);
			}
		}

		public string GetLastError()
		{
			if (m_lastError >= 0 && m_lastError <= 26)
				return Errors[m_lastError];
			return "MAPI error [" + m_lastError + "]";
		}

		readonly string[] Errors = new[]
		{
			"OK [0]", "User abort [1]", "General MAPI failure [2]", "MAPI login failure [3]",
			"Disk full [4]", "Insufficient memory [5]", "Access denied [6]", "-unknown- [7]",
			"Too many sessions [8]", "Too many files were specified [9]", "Too many recipients were specified [10]", "A specified attachment was not found [11]",
			"Attachment open failure [12]", "Attachment write failure [13]", "Unknown recipient [14]", "Bad recipient type [15]",
			"No messages [16]", "Invalid message [17]", "Text too large [18]", "Invalid session [19]",
			"Type not supported [20]", "A recipient was specified ambiguously [21]", "Message in use [22]", "Network failure [23]",
			"Invalid edit fields [24]", "Invalid recipients [25]", "Not supported [26]"
		};

		readonly List<MapiRecipDesc> m_recipients = new List<MapiRecipDesc>();
		readonly List<string> m_attachments = new List<string>();
		int m_lastError;

		const int MAPI_LOGON_UI = 0x00000001;
		const int MAPI_DIALOG = 0x00000008;
		const int maxAttachments = 20;

		enum HowTo
		{
			MAPI_ORIG,
			MAPI_TO,
			MAPI_CC,
			MAPI_BCC
		};
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class MapiMessage
	{
		public int reserved;
		public string subject;
		public string noteText;
		public string messageType;
		public string dateReceived;
		public string conversationID;
		public int flags;
		public IntPtr originator;
		public int recipCount;
		public IntPtr recips;
		public int fileCount;
		public IntPtr files;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal class MapiMessageW
	{
		public int    reserved;
		public string subject;
		public string noteText;
		public string messageType;
		public string dateReceived;
		public string conversationID;
		public int    flags;
		public IntPtr originator;
		public int    recipCount;
		public IntPtr recips;
		public int    fileCount;
		public IntPtr files;

		public MapiMessageW(MapiMessage message)
		{
			reserved = message.reserved;
			subject = message.subject;
			noteText = message.noteText;
			messageType = message.messageType;
			dateReceived = message.dateReceived;
			conversationID = message.conversationID;
			flags = message.flags;
			originator = message.originator;
			recipCount = message.recipCount;
			recips = message.recips;
			fileCount = message.fileCount;
			files = message.files;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class MapiFileDesc
	{
		public int reserved;
		public int flags;
		public int position;
		public string path;
		public string name;
		public IntPtr type;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal class MapiFileDescW
	{
		public int    reserved;
		public int    flags;
		public int    position;
		public string path;
		public string name;
		public IntPtr type;

		public MapiFileDescW(MapiFileDesc fileDesc)
		{
			reserved = fileDesc.reserved;
			flags = fileDesc.flags;
			position = fileDesc.position;
			path = fileDesc.path;
			name = fileDesc.name;
			type = fileDesc.type;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class MapiRecipDesc
	{
		public int reserved;
		public int recipClass;
		public string name;
		public string address;
		public int eIDSize;
		public IntPtr entryID;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal class MapiRecipDescW
	{
		public int    reserved;
		public int    recipClass;
		public string name;
		public string address;
		public int    eIDSize;
		public IntPtr entryID;

		public MapiRecipDescW(MapiRecipDesc recipDesc)
		{
			reserved = recipDesc.reserved;
			recipClass = recipDesc.recipClass;
			name = recipDesc.name;
			address = recipDesc.address;
			eIDSize = recipDesc.eIDSize;
			entryID = recipDesc.entryID;
		}
	}
}
