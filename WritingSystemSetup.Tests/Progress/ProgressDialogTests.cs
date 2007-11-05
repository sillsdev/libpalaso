using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.Progress;
using Palaso.UI.WindowsForms.Progress;

namespace PalasoUIWindowsForms.Tests
{
	[TestFixture]
	public class ProgressDialogTests
	{
		private int _countForWork;
		private ProgressDialog _dialog;
		private StringBuilder _logBuilder;
		private bool _commandFinishedCalled;


		[SetUp]
		public void Setup()
		{
			_dialog = new ProgressDialog();
			_logBuilder = new StringBuilder();
			_dialog.Text = "Unit Test";
			_dialog.Overview = "Pretending to do some work...";
			_countForWork = 0;
		}

		[TearDown]
		public void TearDown()
		{
			_dialog.Dispose();
		}

		[Test]
		public void TypicalUsage()
		{
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += OnDoSomeWork;
			_dialog.BackgroundWorker = worker;
			_dialog.CanCancel = true;
			_dialog.ShowDialog();
		  //  if (_dialog.ProgressStateResult.ExceptionThatWasEncountered != null)
			Assert.AreEqual(DialogResult.OK, _dialog.DialogResult);
		}

	   /* [Test]
		public void UseConsoleState()
		{
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += OnDoSomeWork;
			_dialog.BackgroundWorker = worker;
			_dialog.CanCancel = true;
			ProgressState _progress = new ConsoleProgress();// ProgressState(_progressHandler);
			_progress.Log += new EventHandler<ProgressState.LogEvent>(OnProgressStateLog);

			_dialog.ShowDialog();
			Assert.AreEqual(DialogResult.OK, _dialog.DialogResult);
			Assert.AreEqual("hello", _logBuilder.ToString());
		}*/


		private void OnProgressStateLog(object sender, ProgressState.LogEvent e)
		{
			_logBuilder.AppendLine(e.message);
		}

		private void OnDoSomeWork(object sender, DoWorkEventArgs e)
		{
			ProgressState state = (ProgressState) e.Argument;
			DateTime end = DateTime.Now.AddSeconds(1);
			while(DateTime.Now < end)
			{
				state.WriteToLog(_countForWork.ToString());
				Thread.Sleep(100);

			}
			_countForWork++;
			if (_countForWork > (int)100)
			{
				e.Result = "all done";
			}
		}
	}
}
