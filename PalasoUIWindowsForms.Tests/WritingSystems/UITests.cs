using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.TestUtilities;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.UI.WindowsForms.WritingSystems.WSTree;
using Palaso.WritingSystems;
using Palaso.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration;

namespace PalasoUIWindowsForms.Tests.WritingSystems
{
	[TestFixture]
	public class UITests
	{

		//NB: in Mar 2011, I couldn't get these to run in vs 2010 with resharper, because
		// of the need to be in single apartment thread mode. The app.config is there, requesting
		// it, but it doesn't work. So I had to plug in this CrossThreadTestRunner thing

		[Test, Ignore("By hand only")]
		public void WritingSystemSetupDialog()
		{
			CrossThreadTestRunner runner = new CrossThreadTestRunner();
			runner.RunInSTA(
				delegate
				{
					using (var folder = new TemporaryFolder("WS-Test"))
					{
						var dlg = new WritingSystemSetupDialog(folder.Path, DummyMigratorCallback.onMigration);
						dlg.WritingSystemSuggestor.SuggestVoice = true;
						dlg.ShowDialog();
					}
				});
		}

		[Test, Ignore("By hand only")]
		public void WritingSystemSetupViewWithComboAttached()
		{
			CrossThreadTestRunner runner = new CrossThreadTestRunner();
			runner.RunInSTA(
				delegate
				{
					using (var folder = new TemporaryFolder("WS-Test"))
					{
						var f = new Form();
						f.Size = new Size(800, 600);
						var repository = LdmlInFolderWritingSystemRepository.Initialize(
							DummyMigratorCallback.onMigration,
							folder.Path
						);
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
	private Exception lastException;

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
	  lastException = null;

	  Thread thread = new Thread(
		delegate()
		{
		  try
		  {
			userDelegate.Invoke();
		  }
		  catch (Exception e)
		  {
			lastException = e;
		  }
		});
	  thread.SetApartmentState(apartmentState);

	  thread.Start();
	  thread.Join();

	  if (ExceptionWasThrown())
		ThrowExceptionPreservingStack(lastException);
	}

	private bool ExceptionWasThrown()
	{
	  return lastException != null;
	}

	[ReflectionPermission(SecurityAction.Demand)]
	private static void ThrowExceptionPreservingStack(Exception exception)
	{
	  FieldInfo remoteStackTraceString = typeof(Exception).GetField(
		"_remoteStackTraceString",
		BindingFlags.Instance | BindingFlags.NonPublic);
	  remoteStackTraceString.SetValue(exception, exception.StackTrace + Environment.NewLine);
	  throw exception;
	}
  }

  internal class DummyMigratorCallback
  {
	  public static void onMigration(IEnumerable<LdmlVersion0MigrationStrategy.MigrationInfo> migrationInfo)
	  {
	  }
  }

}