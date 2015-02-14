using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using SIL.Progress;
using SIL.Reporting;

namespace SIL.WindowsForms.Progress
{
	/// <summary>
	/// Does an xslt transform with a cancellable progress dialog.
	/// The XSLT should have a <xsl:message/> to trigger each step
	/// </summary>
	public class TransformWithProgressDialog
	{
		private readonly XslTransformWithProgress _xslTransformer;
		private string _taskMessage;

		public delegate void FileManipulationMethod(object sender, DoWorkEventArgs e);

		public TransformWithProgressDialog(string pathToLift, string outputPath, Stream xsltStream, string xpathToCountSteps)
		{
			_xslTransformer = new XslTransformWithProgress(pathToLift, outputPath, xsltStream, xpathToCountSteps);
		}

		public TransformWithProgressDialog(XslTransformWithProgress xslTransformer)
		{
			_xslTransformer = xslTransformer;
		}

		public void AddArgument(string name, string value)
		{
			_xslTransformer.AddArgument(name, value);
		}

		public string TaskMessage
		{
			get { return _taskMessage; }
			set { _taskMessage = value; }
		}

		/// <summary>
		///
		/// </summary>
		/// <returns>true if succeeded</returns>
		public bool Transform(bool failureWouldBeFatal)
		{
			if (!DoTransformWithProgressDialog(failureWouldBeFatal))
				{
					try
					{
						if (File.Exists(_xslTransformer.OutputFilePath))
						{
							File.Delete(_xslTransformer.OutputFilePath);
						}
					}
					catch (Exception err )
					{
						Debug.Fail(err.Message);
					}
					return false;
				}
			return true;
		}


		/// <summary>
		///
		/// </summary>
		/// <returns>false if not successful or cancelled</returns>
		private bool DoTransformWithProgressDialog(bool failureWouldBeFatal)
		{
			using (ProgressDialog dlg = new ProgressDialog())
			{
				dlg.Overview =string.Format("{0}", _taskMessage);
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += OnDoTransformWork;
				dlg.BackgroundWorker = worker;
				dlg.CanCancel = true;
				//dlg.CancelRequested += new EventHandler(OnCancelRequested);
				dlg.ShowDialog();
				if (dlg.ProgressStateResult!=null && dlg.ProgressStateResult.ExceptionThatWasEncountered != null)
				{
					if(failureWouldBeFatal)
						ErrorReport.ReportFatalException(dlg.ProgressStateResult.ExceptionThatWasEncountered);
				   else
					{
						ErrorReport.ReportNonFatalException(dlg.ProgressStateResult.ExceptionThatWasEncountered);
					}
					return false;
				}
				return !dlg.ProgressState.Cancel;
			}
		}

		/// <summary>
		/// this runs in a worker thread
		/// </summary>
		private void OnDoTransformWork(object sender, DoWorkEventArgs args)
		{
			_xslTransformer.Transform((ProgressState)args.Argument);
		}
	}
}