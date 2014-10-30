using System;
using NUnit.Framework;
using Palaso.UI.WindowsForms;

namespace PalasoUIWindowsForms.Tests
{
	class SimpleMessageDialogTests
	{
		[Test, Ignore("by hand only")]
		[STAThread]
		public void ShowSimpleMessageDialog()
		{
			using (var dlg = new SimpleMessageDialog("my message", "my title"))
			{
				dlg.Show();
				System.Threading.Thread.Sleep(5000);
			}
		}

		[Test, Ignore("by hand only")]
		[STAThread]
		public void ShowSimpleMessageDialog_TitleNotProvided_StillHasTitleBar()
		{
			using (var dlg = new SimpleMessageDialog("my message"))
			{
				dlg.Show();
				System.Threading.Thread.Sleep(5000);
			}
		}
	}
}
