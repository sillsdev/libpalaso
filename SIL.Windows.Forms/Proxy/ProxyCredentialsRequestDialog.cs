using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using SIL.Network;

namespace SIL.Windows.Forms.Proxy
{
	public partial class ProxyCredentialsRequestDialog : Form
	{
		public ProxyCredentialsRequestDialog()
		{
			InitializeComponent();
			Text = Application.ProductName + ": Proxy Credentials";
		}

		/// <summary>
		/// Get proxy creds from a place (not settings) that all palaso apps can share
		/// </summary>
		/// <returns>true if some credentials were found</returns>
		public static bool ReadProxyCredentials(out string user, out string password)
		{
			user = string.Empty;
			password = string.Empty;
			try
			{
				string path = GetPathToProxyCredentialsFile();
				if (!File.Exists(path))
				{
					return false;
				}
				var lines = File.ReadAllLines(path);
				if (lines.Length < 2)
					return false;
				var userParts = lines[0].Split(new string[] {"=="}, 2, StringSplitOptions.RemoveEmptyEntries);
				if (userParts.Length != 2)
					return false;
				var passwordParts = lines[1].Split(new string[] {"=="}, 2, StringSplitOptions.RemoveEmptyEntries);
				if (passwordParts.Length != 2)
					return false;
				user = userParts[1].Trim();
				password = passwordParts[1].Trim();
				return true;
			}

			catch (Exception error)
			{
				Debug.Fail(error.Message);// don't do anything to a non-debug user, though.
			}
			return false;
		}

		public static void SaveProxyCredentials(string user, string password)
		{
			try
			{
				string path = GetPathToProxyCredentialsFile();
				File.WriteAllText(path, string.Format("user=={0}\r\npassword=={1}", user, password));
			}
			catch(Exception error)
			{
				Debug.Fail(error.Message);
			}
		}

		private static string GetPathToProxyCredentialsFile()
		{
			var path = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, "Palaso");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			path = Path.Combine(path, "proxy.txt");
			return path;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="doTryStoredCredentials">Call it first with this true, but if that doesn't work, call with this
		/// false so that we ask the user again</param>
		/// <returns></returns>
		public static ICredentials GetCredentials(bool doTryStoredCredentials)
		{
			using (var dlg = new ProxyCredentialsRequestDialog())
			{
				string userName=string.Empty;
				string password=string.Empty;
				try
				{
					string encryptedUserName;
					string encryptedPassword;
					ReadProxyCredentials(out encryptedUserName, out encryptedPassword);
					userName = RobustNetworkOperation.GetClearText(encryptedUserName);
					password = RobustNetworkOperation.GetClearText(encryptedPassword);
				}
				catch (Exception)
				{
					//swallow and just give them blanks in the dialog
				}

				// should we ask the user for info?
				if (!doTryStoredCredentials || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
				{
					dlg._userName.Text = userName;
					dlg._password.Text = password;
					if (DialogResult.OK != dlg.ShowDialog(null))
						return null;
					if (string.IsNullOrEmpty(dlg._userName.Text))
						return null;
					if (string.IsNullOrEmpty(dlg._password.Text))
						return null;
					 if (dlg._remember.Checked)
					{
						SaveProxyCredentials(GetEncryptedText(userName), GetEncryptedText(password));
					}
				}

				return new NetworkCredential(userName, password);
			}
		}

		private static string GetEncryptedText(string clearText)
		{
			byte[] nameBytes = Encoding.Unicode.GetBytes(clearText);
			byte[] encryptedNameBytes = ProtectedData.Protect(nameBytes, null, DataProtectionScope.CurrentUser);
		   return Convert.ToBase64String(encryptedNameBytes);
		}



		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void _headerLabel_Click(object sender, EventArgs e)
		{

		}
	}


}
