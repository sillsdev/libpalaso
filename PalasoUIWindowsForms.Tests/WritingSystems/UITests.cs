using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.TestUtilities;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.UI.WindowsForms.WritingSystems.WSTree;
using Palaso.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems
{
	[TestFixture]
	public class UITests
	{

		[Test, Ignore("By hand only")]
		public void WritingSystemSetupDialog()
		{
			using (var folder = new TemporaryFolder("WS-Test"))
			{
				var dlg = new WritingSystemSetupDialog(folder.Path);
				dlg.WritingSystemSuggestor.SuggestVoice = true;
				dlg.ShowDialog();
			}
		}

		[Test, Ignore("By hand only")]
		public void WritingSystemSetupViewWithComboAttached()
		{
			using (var folder = new TemporaryFolder("WS-Test"))
			{
				var f = new Form();
				f.Size=new Size(800,600);
				var model = new WritingSystemSetupModel(new LdmlInFolderWritingSystemRepository(folder.Path));
				var v = new WritingSystemSetupView(model);
				var combo = new WSPickerUsingComboBox(model);
				f.Controls.Add(combo);
				f.Controls.Add(v);
				f.ShowDialog();
			}
		}
	}

}