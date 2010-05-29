using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.TestUtilities;
using Palaso.UI.WindowsForms.WritingSystems;
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
				new WritingSystemSetupDialog(folder.Path).ShowDialog();
			}
		}

	}

}