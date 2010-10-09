using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Palaso.Reporting.Network
{
	public partial class ProxyCredentialsRequestDialog : Form
	{
		public ProxyCredentialsRequestDialog()
		{
			InitializeComponent();
		}



		/// <summary>
		///
		/// </summary>
		/// <param name="doTryStoredCredentials">Call it first with this true, but if that doesn't work, call with this
		/// false so that we ask the user again</param>
		/// <returns></returns>
		public static ICredentials GetCredentials(bool doTryStoredCredentials)
		{
			ProxyCredentialSettings settings = new ProxyCredentialSettings();

			using (var dlg = new ProxyCredentialsRequestDialog())
			{
				try
				{
					dlg._userName.Text = GetClearText(settings.UserName);
					dlg._password.Text = GetClearText(settings.Password);
				}
				catch (Exception)
				{
					//swallow and just give them blanks
				}

				// should we ask the user for info?
				if (!doTryStoredCredentials || string.IsNullOrEmpty(dlg._userName.Text) || string.IsNullOrEmpty(dlg._password.Text))
				{
					if (DialogResult.OK != dlg.ShowDialog(null))
						return null;
					if (string.IsNullOrEmpty(dlg._userName.Text))
						return null;
					if (string.IsNullOrEmpty(dlg._password.Text))
						return null;
				}

				string userName = dlg._userName.Text.Trim();
				if (dlg._remember.Checked)
				{
					settings.UserName = GetEncryptedText(userName);
					settings.Password = GetEncryptedText(dlg._password.Text.Trim());

					settings.Save();
				}

				return new NetworkCredential(userName, dlg._password.Text.Trim());
			}
		}

		private static string GetEncryptedText(string clearText)
		{
			byte[] nameBytes = Encoding.Unicode.GetBytes(clearText);
			byte[] encryptedNameBytes = ProtectedData.Protect(nameBytes, null, DataProtectionScope.CurrentUser);
		   return Convert.ToBase64String(encryptedNameBytes);
		}

		private static string GetClearText(string encryptedString)
		{
		   if (string.IsNullOrEmpty(encryptedString))
			   return string.Empty;
			byte[] encryptedBytes = Convert.FromBase64String(encryptedString);
			byte[] clearBytes = ProtectedData.Unprotect(encryptedBytes, null,
										   DataProtectionScope.CurrentUser);
			return Encoding.Unicode.GetString(clearBytes);
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}


}
