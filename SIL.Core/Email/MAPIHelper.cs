//from http://www.codeproject.com/cs/internet/SendFileToNET.asp
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;

namespace SIL.Email
{
	internal class MAPI
	{
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


		[DllImport("MAPI32.DLL")]
		private static extern int MAPISendMail(IntPtr sess, IntPtr hwnd, MapiMessage message, int flg, int rsv);

		/// <summary>
		///
		/// </summary>
		/// <param name="strSubject"></param>
		/// <param name="strBody"></param>
		/// <param name="how"></param>
		/// <returns>true if successful</returns>
		private bool SendMail(string strSubject, string strBody, int how)
		{
			MapiMessage msg = new MapiMessage();
			msg.subject = strSubject;
			msg.noteText = strBody;

			msg.recips = GetRecipients(out msg.recipCount);
			msg.files = GetAttachments(out msg.fileCount);

			m_lastError = MAPISendMail(new IntPtr(0), new IntPtr(0), msg, how, 0);
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
			MapiRecipDesc recipient = new MapiRecipDesc();

			recipient.recipClass = (int)howTo;
			recipient.name = email;
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

			int size = Marshal.SizeOf(typeof(MapiRecipDesc));
			IntPtr intPtr = Marshal.AllocHGlobal(m_recipients.Count * size);

			var ptr = intPtr;
			foreach (var mapiDesc in m_recipients)
			{
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

			int size = Marshal.SizeOf(typeof(MapiFileDesc));
			IntPtr intPtr = Marshal.AllocHGlobal(m_attachments.Count * size);

			var mapiFileDesc = new MapiFileDesc();
			mapiFileDesc.position = -1;
			var ptr = intPtr;

			foreach (var strAttachment in m_attachments)
			{
				mapiFileDesc.name = Path.GetFileName(strAttachment);
				mapiFileDesc.path = strAttachment;
				Marshal.StructureToPtr(mapiFileDesc, ptr, false);
				IntPtr.Add(ptr, size);
			}

			fileCount = m_attachments.Count;
			return intPtr;
		}

		private void Cleanup(ref MapiMessage msg)
		{
			int size = Marshal.SizeOf(typeof(MapiRecipDesc));
			IntPtr ptr;

			if (msg.recips != IntPtr.Zero)
			{
				ptr = msg.recips;
				for (int i = 0; i < msg.recipCount; i++)
				{
					Marshal.DestroyStructure(ptr, typeof(MapiRecipDesc));
					IntPtr.Add(ptr, size);
				}
				Marshal.FreeHGlobal(msg.recips);
			}

			if (msg.files != IntPtr.Zero)
			{
				size = Marshal.SizeOf(typeof(MapiFileDesc));

				ptr = msg.files;
				for (int i = 0; i < msg.fileCount; i++)
				{
					Marshal.DestroyStructure(ptr, typeof(MapiFileDesc));
					IntPtr.Add(ptr, size);
				}
				Marshal.FreeHGlobal(msg.files);
			}

			m_recipients.Clear();
			m_attachments.Clear();
			m_lastError = 0;
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
}
