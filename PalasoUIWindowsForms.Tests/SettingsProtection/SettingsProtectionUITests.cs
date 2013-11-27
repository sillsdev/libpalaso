
using System;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;


namespace  PalasoUIWindowsForms.Tests.SettingsProtection
{
	[TestFixture]
	public class SettingsProtectionUITests
	{

		[Test, Ignore("By hand only")]
		public void LaunchDemoDialog()
		{
			using (var dlg = new DialogWithLinkToSettings())
			{
				dlg.ShowDialog();
			}
		}

	}

}