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
				dlg.Update();
				System.Threading.Thread.Sleep(5000);
			}
		}

		[Test, Ignore("by hand only")]
		[STAThread]
		public void ShowSimpleMessageDialog_LongMessageWraps()
		{
			const string longMessage = "my very long message which likes to wrap and wrap and wrap and wrap and wrap and wrap and wrap";
			using (var dlg = new SimpleMessageDialog(longMessage, "my title"))
			{
				dlg.Show();
				dlg.Update();
				System.Threading.Thread.Sleep(5000);
			}
		}

		[Test, Ignore("by hand only")]
		[STAThread]
		public void ShowSimpleMessageDialog_CaptionNotProvided_StillHasCaptionBar()
		{
			using (var dlg = new SimpleMessageDialog("the caption bar is still there, though empty"))
			{
				dlg.Show();
				dlg.Update();
				System.Threading.Thread.Sleep(3000);
			}
			using (var dlg = new SimpleMessageDialog("the caption bar is still there, though empty", null))
			{
				dlg.Show();
				dlg.Update();
				System.Threading.Thread.Sleep(3000);
			}
		}

		[Test, Ignore("by hand only")]
		[STAThread]
		public void ShowSimpleMessageDialog_CaptionBarCanBeHidden()
		{
			using (var dlg = new SimpleMessageDialog("the caption bar can be hidden if truly desired", ""))
			{
				dlg.Show();
				dlg.Update();
				System.Threading.Thread.Sleep(3000);
			}
		}
	}
}
