using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
			if(_dialog!=null)
			{
				_dialog.Dispose();
			}
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
			WorkArguments args  = new WorkArguments();
			args.dummy = "testing";

			_dialog.InitialProgressState.Arguments = args;
			Assert.AreNotEqual("testing", _argumentReceivedFromProgressState);
			_dialog.ShowDialog();
			Assert.AreEqual("testing", _argumentReceivedFromProgressState);
			Assert.AreEqual(DialogResult.OK, _dialog.DialogResult);
		}

		/// <summary>
		/// Though the result here are great, I'm afraid this may look worse on a single-core machine
		/// </summary>
		[Test]
		public void DontBogDownWhenBombardedWithProgressUpdates()
		{
			Debug.WriteLine("Priming");
			int toDo = 99000;
			MeasureProgressUpdateCost(true, toDo);

			Debug.WriteLine("Without Progress");
			long noProgressMilliseconds = MeasureProgressUpdateCost(false, toDo);

			Debug.WriteLine("With Progress");
			long withProgressMilliseconds = MeasureProgressUpdateCost(true, toDo);

			Assert.Less((int)(withProgressMilliseconds - noProgressMilliseconds), 1000, "Should not have more than a one second overhead.");
		}

		[Test]
		public void FreezeBugRegression()
		{
			//this used to have this behavior:  this line would run fine and fast
			MeasureProgressUpdateCost(true, 10000);
			//this would never actually execute the worker at all!
			MeasureProgressUpdateCost(false, 1);
			//It didn't matter if the second guy planned to show progress, or what he planned to do.
			//The progress dialog would just sit there, constantly upping its estimate for when this
			//turkey was going to be done, never hearing back from it.

			//this was, um, "solved" by making sure that only actually advances of at least one
			//percentage point were passed on to the System.ComponentModel.Component.BackgroundWorker.ReportProgress() method.
		}

		private long MeasureProgressUpdateCost(bool doMakeProgressCalls, int iterationsToDo)
		{
			_dialog = new ProgressDialog();
			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += OnDoSomeWork;
			_dialog.BackgroundWorker = worker;
			WorkArguments args = new WorkArguments();
			args.doMakeProgressCalls = doMakeProgressCalls;
			args.secondsToUseUp = 0;
			args.iterationsToDo = iterationsToDo;
			_dialog.InitialProgressState.Arguments = args;
			Stopwatch w = new Stopwatch();
			w.Start();
			_dialog.ShowDialog();
			_dialog.Close();
			_dialog.Dispose();
			_dialog = null;
			w.Stop();
			worker.Dispose();
			Debug.WriteLine("Took "+ w.Elapsed.ToString());
			return w.ElapsedMilliseconds;
		}

		internal class WorkArguments
		{
			public int secondsToUseUp = 1;
			public string dummy;
			public bool doMakeProgressCalls = false;
			public int iterationsToDo = 9000;
		}

		private void OnProgressStateLog(object sender, ProgressState.LogEvent e)
		{
			_logBuilder.AppendLine(e.message);
		}

		private void OnDoSomeWork(object sender, DoWorkEventArgs e)
		{
			ProgressState state = (ProgressState) e.Argument;
			WorkArguments args = state.Arguments as WorkArguments;
			if (args == null)
			{
				args = new WorkArguments(); //use defaults
			}
			state.TotalNumberOfSteps = 10000; //we actually have no idea, but let it be large
			try
			{
				_argumentReceivedFromProgressState = args.dummy;
				if (args.secondsToUseUp > 0)
				{
					DateTime end = DateTime.Now.AddSeconds(args.secondsToUseUp);
					while (DateTime.Now < end)
					{
						state.WriteToLog(_countForWork.ToString());
						Thread.Sleep(10);
						if (args.doMakeProgressCalls)
						{
							state.NumberOfStepsCompleted++;
						}
					}
				}
				else  //seeing how long this takes
				{
					state.TotalNumberOfSteps = args.iterationsToDo;
					double a = 0;
					for (int i = 0; i < state.TotalNumberOfSteps; i++)
					{
					   //adding this makes freeze not happen Thread.Sleep(1);
						a += Math.Sqrt(987987+i);
						if (args.doMakeProgressCalls)
						{
							state.NumberOfStepsCompleted++;
						}
					}
				   Debug.WriteLine(a);
				}
				e.Result = "all done";
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
