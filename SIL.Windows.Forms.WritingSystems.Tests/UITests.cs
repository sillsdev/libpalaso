using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems.Tests
{
	[TestFixture]
	public class UITests
	{
		//NB: in Mar 2011, I couldn't get these to run in vs 2010 with resharper, because
		// of the need to be in single apartment thread mode. The app.config is there, requesting
		// it, but it doesn't work. So I had to plug in this CrossThreadTestRunner thing
		[Test]
		[Explicit("By hand only")]
		public void WritingSystemSetupDialog()
		{
			var runner = new CrossThreadTestRunner();
			runner.RunInSTA(
				delegate
				{
					using (var folder = new TemporaryFolder("WS-Test"))
					{
						//var dlg = new WritingSystemSetupDialog(folder.Path,
						//	DummyWritingSystemHandler.onMigration,
						//	DummyWritingSystemHandler.onLoadProblem);
						//that constructor is now obsolete, create repo first
						var repository = LdmlInFolderWritingSystemRepository.Initialize(folder.Path);
						var dlg = new WritingSystemSetupDialog(repository);
						dlg.WritingSystemSuggestor.SuggestVoice = true;
						dlg.ShowDialog();
					}
				});
		}

		[Test]
		[Explicit("By hand only")]
		public void WritingSystemSetupViewWithComboAttached()
		{
			var runner = new CrossThreadTestRunner();
			runner.RunInSTA(
				delegate
				{
					using (var folder = new TemporaryFolder("WS-Test"))
					{
						var f = new Form();
						f.Size = new Size(800, 600);
						var repository = LdmlInFolderWritingSystemRepository.Initialize(folder.Path);
						var model = new WritingSystemSetupModel(repository);
						var v = new WritingSystemSetupView(model);
						var combo = new WSPickerUsingComboBox(model);
						f.Controls.Add(combo);
						f.Controls.Add(v);
						f.ShowDialog();
					}
				});
		}
	}


	public class CrossThreadTestRunner
	{
		private Exception _lastException;

		public void RunInMTA(ThreadStart userDelegate)
		{
			Run(userDelegate, ApartmentState.MTA);
		}

		public void RunInSTA(ThreadStart userDelegate)
		{
			Run(userDelegate, ApartmentState.STA);
		}

		private void Run(ThreadStart userDelegate, ApartmentState apartmentState)
		{
			_lastException = null;

			var thread = new Thread(
				delegate()
				{
					try
					{
						userDelegate.Invoke();
					}
					catch (Exception e)
					{
						_lastException = e;
					}
				});
			thread.SetApartmentState(apartmentState);

			thread.Start();
			thread.Join();

			if (ExceptionWasThrown())
				ThrowExceptionPreservingStack(_lastException);
		}

		private bool ExceptionWasThrown()
		{
			return _lastException != null;
		}

		[ReflectionPermission(SecurityAction.Demand)]
		private static void ThrowExceptionPreservingStack(Exception exception)
		{
			FieldInfo remoteStackTraceString = typeof (Exception).GetField(
				"_remoteStackTraceString",
				BindingFlags.Instance | BindingFlags.NonPublic);
			remoteStackTraceString.SetValue(exception, exception.StackTrace + Environment.NewLine);
			throw exception;
		}
	}
}