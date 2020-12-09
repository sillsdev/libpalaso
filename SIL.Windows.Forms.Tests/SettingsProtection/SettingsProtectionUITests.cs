using NUnit.Framework;

namespace  SIL.Windows.Forms.Tests.SettingsProtection
{
	[TestFixture]
	public class SettingsProtectionUITests
	{

		[Test]
		[Explicit("By hand only")]
		public void LaunchDemoDialog()
		{
			using (var dlg = new DialogWithLinkToSettings())
			{
				dlg.ShowDialog();
			}
		}

	}

}