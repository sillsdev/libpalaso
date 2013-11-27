// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2013, SIL International. All Rights Reserved.
// <copyright from='2013' to='2013' company='SIL International'>
//		Copyright (c) 2013, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Palaso.UI.WindowsForms.ImageGallery;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;
using PalasoUIWindowsForms.TestApp.Properties;

namespace PalasoUIWindowsForms.TestApp
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	///
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public partial class TestAppForm : Form
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Default c'tor
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TestAppForm()
		{
			InitializeComponent();
		}

		private void OnFolderBrowserControlClicked(object sender, EventArgs e)
		{
			var form = new Form();
			var browser = new Palaso.UI.WindowsForms.FolderBrowserControl.FolderBrowserControl();
			browser.Location = new Point(0, 0);
			browser.Width = form.ClientSize.Width;
			browser.Height = form.ClientSize.Height;
			browser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			browser.ShowOnlyMappedDrives = false;
			browser.ShowAddressbar = true;
			form.Controls.Add(browser);
			form.ShowDialog();
		}

		private void OnLookupISOCodeDialogClicked(object sender, EventArgs e)
		{
			var dialog = new LookupISOCodeDialog();
			dialog.ShowDialog();
		}

		private void OnWritingSystemSetupDialogClicked(object sender, EventArgs e)
		{
			string tempPath = Path.GetTempPath() + "WS-Test";
			Directory.CreateDirectory(tempPath);
			KeyboardController.Initialize();
			try
			{
				var wsRepo = LdmlInFolderWritingSystemRepository.Initialize(tempPath, onMigration, onLoadProblem);
				var dialog = new WritingSystemSetupDialog(wsRepo);
				dialog.WritingSystems.LocalKeyboardSettings = Settings.Default.LocalKeyboards;
				dialog.ShowDialog();
				Settings.Default.LocalKeyboards = dialog.WritingSystems.LocalKeyboardSettings;
				Settings.Default.Save();
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				KeyboardController.Shutdown();
			}
		}

		private void OnArtOfReadingClicked(object sender, EventArgs e)
		{
			new ArtOfReadingTestForm().ShowDialog();
		}

		private static void onMigration(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo)
		{
		}

		private static void onLoadProblem(IEnumerable<WritingSystemRepositoryProblem> migrationInfo)
		{
		}
	}
}