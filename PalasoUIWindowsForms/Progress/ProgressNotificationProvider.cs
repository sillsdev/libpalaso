using System;
using System.ComponentModel;
using System.Windows.Forms;
using Palaso.Progress;


namespace Palaso.UI.WindowsForms.Progress
{
	public delegate T ActionThatReportsProgress<T>(ProgressState progressState);

	public interface IProgressNotificationProvider
	{
		T Go<T>(string taskDescription, ActionThatReportsProgress<T> thingToDo);
	}

	public class DialogProgressNotificationProvider : IProgressNotificationProvider
	{
		public DialogProgressNotificationProvider()
		{
		}

		public T Go<T>(string taskDescription, ActionThatReportsProgress<T> thingToDo)
		{
			T result = default(T);
			using (ProgressDialog dlg = new ProgressDialog())
			{
				dlg.Overview = taskDescription;
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += delegate(object sender, DoWorkEventArgs args)
									 {
										 ProgressState progressState = (ProgressState)args.Argument;
										 try
										 {
											 result = thingToDo(progressState);
										 }
										 catch (Exception error)
										 {
											 args.Cancel = true;//review
											 args.Result = error;
											 progressState.ExceptionThatWasEncountered = error;
										 }
									 };
				dlg.BackgroundWorker = worker;
				dlg.CanCancel = false;

				dlg.ShowDialog();
				if (dlg.DialogResult != DialogResult.OK)
				{
					Exception err = dlg.ProgressStateResult.ExceptionThatWasEncountered;

					if (dlg.ProgressStateResult.State ==
						ProgressState.StateValue.StoppedWithError)
					{
						throw new ApplicationException("Failure while "+taskDescription, err);
					}
					else if (err != null)
					{
						throw err;
					}
				}

			}
			return result;
		}
	}
}