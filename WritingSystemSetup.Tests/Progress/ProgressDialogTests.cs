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
		private object _argumentReceivedFromProgressState;


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


		[Test]
		public void NewDialogHasNonNullStates()
		{
			//avoids the client getting null errors if he checks this when there
			//has yet to be a callback from the worker

			_dialog.BackgroundWorker = new BackgroundWorker();
			Assert.IsNotNull(_dialog.InitialProgressState);
			Assert.IsNotNull(_dialog.ProgressStateResult);
		}

		[Test]
		public void SendArgumentsToWorker()
		{
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += OnDoSomeWork;
			_dialog.BackgroundWorker = worker;
			_dialog.InitialProgressState.Arguments = "testing";
			Assert.AreNotEqual("testing", _argumentReceivedFromProgressState);
			_dialog.ShowDialog();
			Assert.AreEqual("testing", _argumentReceivedFromProgressState);
			Assert.AreEqual(DialogResult.OK, _dialog.DialogResult);
		}



		private void OnProgressStateLog(object sender, ProgressState.LogEvent e)
		{
			_logBuilder.AppendLine(e.message);
		}

		private void OnDoSomeWork(object sender, DoWorkEventArgs e)
		{
			ProgressState state = (ProgressState)e.Argument;
			try
			{
				_argumentReceivedFromProgressState = state.Arguments;
				DateTime end = DateTime.Now.AddSeconds(1);
				while (DateTime.Now < end)
				{
					state.WriteToLog(_countForWork.ToString());
					Thread.Sleep(100);
				}
				_countForWork++;
				if (_countForWork > (int) 100)
				{
					e.Result = "all done";
				}
			}
			catch (Exception err)
			{
				//currently, error reporter can choke because this is
				//being called from a non sta thread.
				//so let's leave it to the progress dialog to report the error
				//                Reporting.ErrorReporter.ReportException(e,null, false);
				state.ExceptionThatWasEncountered = err;
				state.WriteToLog(err.Message);
				state.State = ProgressState.StateValue.StoppedWithError;
			}
		}
	}
}
