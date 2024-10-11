using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace SIL.Progress
{
	/// <summary>
	/// Does an xslt transform with a cancellable progress dialog.
	/// The XSLT should have a &lt;xsl:message/&gt; to trigger each step
	/// </summary>
	public class XslTransformWithProgress
	{
		private readonly string _inputFilePath;
		private readonly Stream _xsltStream;
		private readonly string _xpathToCountSteps;
		private XsltArgumentList _xsltArguments;

		private ProgressState _progressState;

		public XslTransformWithProgress(string inputFilePath, string outputFilePath, Stream xsltStream,
			string xpathToCountSteps)
		{
			_inputFilePath = inputFilePath;
			OutputFilePath = outputFilePath;
			_xsltStream = xsltStream;
			_xpathToCountSteps = xpathToCountSteps;
			_progressState = null;
		}

		public void AddArgument(string name, string value)
		{
			if (_xsltArguments == null)
			{
				_xsltArguments = new XsltArgumentList();
			}
			_xsltArguments.AddParam(name, string.Empty, value);
		}

		public string OutputFilePath { get; }

		public void Transform(ProgressState progressState)
		{
			_progressState = progressState;
			using (Stream outputStream = File.Create(OutputFilePath))
			{
				var inputDocument = new XmlDocument { PreserveWhitespace = true };
				inputDocument.Load(_inputFilePath);

				var transform = new XslCompiledTransform();
				try
				{
					//all this just to allow a DTD statement in the source xslt
					var readerSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse };

					_progressState.StatusLabel = "Preparing...";
					using (Stream stream = _xsltStream)
					{
						using (XmlReader xsltReader = XmlReader.Create(stream, readerSettings))
						{
							XsltSettings settings = new XsltSettings(true, true);
							transform.Load(xsltReader, settings, new XmlUrlResolver());
							xsltReader.Close();
						}

						stream.Close();
					}

					_progressState.StatusLabel = "Processing...";
					int entriesCount = 1;
					XmlNodeList nodeCountList = inputDocument.SelectNodes(_xpathToCountSteps);
					if (nodeCountList != null)
					{
						entriesCount = nodeCountList.Count;
					}

					_progressState.TotalNumberOfSteps = entriesCount;
					if (_xsltArguments == null)
					{
						_xsltArguments = new XsltArgumentList();
					}

					_xsltArguments.XsltMessageEncountered += OnXsltMessageEncountered;
					transform.Transform(inputDocument, _xsltArguments, outputStream);

					outputStream.Close(); //let the next guy get at the file
					Debug.Assert(_progressState.NumberOfStepsCompleted <= entriesCount,
						"Should use up more than we reserved for ourselves");
					_progressState.NumberOfStepsCompleted = entriesCount;
					_progressState.State = ProgressState.StateValue.Finished;
				}
				catch (CancelingException) //not an error
				{
					_progressState.State = ProgressState.StateValue.Finished;
				}
				catch (Exception err)
				{
					//currently, error reporter can choke because this is
					//being called from a non sta thread.
					//so let's leave it to the progress dialog to report the error
					//                Reporting.ErrorReporter.ReportException(args,null, false);
					_progressState.ExceptionThatWasEncountered = err;
					_progressState.WriteToLog(err.Message);
					_progressState.State = ProgressState.StateValue.StoppedWithError;
				}
#if NET462
				finally
				{
					_progressState.StatusLabel = "Cleaning up...";
					transform.TemporaryFiles?.Delete();
				}
#endif
			}
		}

		void OnXsltMessageEncountered(object sender, XsltMessageEncounteredEventArgs e)
		{
			_progressState.NumberOfStepsCompleted++;
			if (_progressState.Cancel)
			{
				throw new CancelingException();
			}
		}

		/// <summary>
		/// used to break us out of the xslt transformer if the user cancels
		/// </summary>
		private class CancelingException : ApplicationException
		{
		}
	}
}