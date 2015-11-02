using System;
using System.IO;
using SIL.Lift.Validation;
using SIL.Progress;
using SIL.Reporting;

namespace SIL.Lift.Migration
{
	public class LiftPreparer
	{
		private readonly string _liftFilePath;

		public LiftPreparer(string liftFilePath)
		{
			_liftFilePath = liftFilePath;
		}

#if notUsedSinceNov2008
		private bool PopulateDefinitionsWithUI()
		{
			using (ProgressDialog dlg = new ProgressDialog())
			{
				dlg.Overview = "Please wait while WeSay preprocesses your LIFT file.";
				BackgroundWorker preprocessWorker = new BackgroundWorker();
				preprocessWorker.DoWork += DoPopulateDefinitionsWork;
				dlg.BackgroundWorker = preprocessWorker;
				dlg.CanCancel = true;
				dlg.ShowDialog();
				if (dlg.ProgressStateResult.ExceptionThatWasEncountered != null)
				{
					ErrorReport.NotifyUserOfProblem(
							String.Format(
									"WeSay encountered an error while preprocessing the file '{0}'.  Error was: {1}",
									_liftFilePath,
									dlg.ProgressStateResult.ExceptionThatWasEncountered.Message));
				}
				return (dlg.DialogResult == DialogResult.OK);
			}
		}

		private void DoPopulateDefinitionsWork(object sender, DoWorkEventArgs args)
		{
			PopulateDefinitions((ProgressState) args.Argument);
		}

		public void PopulateDefinitions(ProgressState state)
		{
			state.StatusLabel = "Updating Lift File...";
			try
			{
				string pathToLift = _liftFilePath;
				string temp1 = Utilities.ProcessLiftForLaterMerging(pathToLift);
				//    int liftProducerVersion = GetLiftProducerVersion(pathToLift);

				string outputPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.NewLineOnAttributes = true;

				using (
						Stream xsltStream =
								Assembly.GetExecutingAssembly().GetManifestResourceStream(
										"WeSay.LexicalModel.Migration.populateDefinitionFromGloss.xslt")
						)
				{
					XslTransformWithProgress transformer = new XslTransformWithProgress(
							temp1, outputPath, xsltStream, "//sense");
					state.StatusLabel = "Populating Definitions from Glosses";
					transformer.Transform(state);
				}

				MoveTempOverRealAndBackup(pathToLift, outputPath);
			}
			catch (Exception error)
			{
				state.ExceptionThatWasEncountered = error;
				state.State = ProgressState.StateValue.StoppedWithError;
				throw;
				// this will put the exception in the e.Error arg of the RunWorkerCompletedEventArgs
			}
		}

		//        private int GetLiftProducerVersion(string pathToLift)
		//        {
		//            string s = FindFirstInstanceOfPatternInFile(pathToLift, "producer=\"()\"");
		//        }
#endif
		//private static string FindFirstInstanceOfPatternInFile(string inputPath, string pattern)
		//{
		//    Regex regex = new Regex(pattern);
		//    using (StreamReader reader = File.OpenText(inputPath))
		//    {
		//        while (!reader.EndOfStream)
		//        {
		//            Match m = regex.Match(reader.ReadLine());
		//            if (m != null)
		//            {
		//                return m.Value;
		//            }
		//        }
		//        reader.Close();
		//    }
		//    return string.Empty;
		//}

		private static void MoveTempOverRealAndBackup(string existingPath, string newFilePath)
		{
			string backupName = existingPath + ".old";

			try
			{
				if (File.Exists(backupName))
				{
					File.Delete(backupName);
				}

				File.Move(existingPath, backupName);
			}
			catch
			{
				Logger.WriteEvent(String.Format("Couldn't write out to {0} ", backupName));
			}

			File.Copy(newFilePath, existingPath);
			File.Delete(newFilePath);
		}

		public bool IsMigrationNeeded()
		{
			return Migrator.IsMigrationNeeded(_liftFilePath);
		}

#if LiftPreparerHasUi
		/// <summary>
		///
		/// </summary>
		/// <returns>true if everything is ok, false if something went wrong</returns>
		public bool MigrateIfNeeded()
		{
			if (Migrator.IsMigrationNeeded(_liftFilePath))
			{
				using (ProgressDialog dlg = new ProgressDialog())
				{
					dlg.Overview =
							"Please wait while WeSay migrates your lift database to the required version.";
					BackgroundWorker migrationWorker = new BackgroundWorker();
					migrationWorker.DoWork += DoMigrateLiftFile;
					dlg.BackgroundWorker = migrationWorker;
					dlg.CanCancel = false;

					dlg.ShowDialog();
					if (dlg.DialogResult != DialogResult.OK)
					{
						Exception err = dlg.ProgressStateResult.ExceptionThatWasEncountered;
						if (err != null)
						{
							ErrorReport.ReportFatalException(err);
						}
						else if (dlg.ProgressStateResult.State ==
								 ProgressState.StateValue.StoppedWithError)
						{
							ErrorReport.ReportNonFatalMessageWithStackTrace(
									"Failed. " + dlg.ProgressStateResult.LogString);
						}
						return false;
					}
				}
			}
			return true;
		}

		private void DoMigrateLiftFile(object obj, DoWorkEventArgs args)
		{
			MigrateLiftFile((ProgressState) args.Argument);
		}
#endif

		public void MigrateLiftFile(ProgressState state)
		{
			try
			{
				string oldVersion = Validator.GetLiftVersion(_liftFilePath);
				string status = String.Format("Migrating from {0} to {1}",
											  oldVersion,
											  Validator.LiftVersion);
				Logger.WriteEvent(status);
				state.StatusLabel = status;
				string migratedFile = Migrator.MigrateToLatestVersion(_liftFilePath);
				//this was hard on test, as it would fail if the file ended in ".tmp"
				//      string nameForOldFile = _liftFilePath.Replace(".lift", "." + oldVersion + ".lift");
				var extension = Path.GetExtension(_liftFilePath);
				string nameForOldFile = _liftFilePath.Replace(extension, "." + oldVersion + extension);

				if (File.Exists(nameForOldFile))
						// like, if we tried to convert it before and for some reason want to do it again
				{
					File.Delete(nameForOldFile);
				}
				File.Move(_liftFilePath, nameForOldFile);
				File.Move(migratedFile, _liftFilePath);

				//review: CJP asks I'm not sure why this is required to be passed back via results. ???
				//args.Result = args.Argument as ProgressState;
			}
			catch (Exception e)
			{
				//currently, error reporter can choke because this is
				//being called from a non sta thread.
				//so let's leave it to the progress dialog to report the error
				//                Reporting.ErrorReporter.ReportException(e,null, false);
				state.ExceptionThatWasEncountered = e;
				state.WriteToLog(e.Message);
				state.State = ProgressState.StateValue.StoppedWithError;
			}
		}

		//private bool BringCachesUpToDate()
		//{
		//    Debug.Assert(!string.IsNullOrEmpty(_liftFilePath));
		//    CacheBuilder builder = CacheManager.GetCacheBuilderIfNeeded(_project);

		//    if (builder == null)
		//    {
		//        return true;
		//    }

		//    //ProgressState progressState = new WeSay.Foundation.ConsoleProgress();//new ProgressState(progressDialogHandler);
		//    using (ProgressDialog dlg = new ProgressDialog())
		//    {
		//        if (!PopulateDefinitionsWithUI())
		//        {
		//            return false;
		//        }

		//        dlg.Overview =
		//                "Please wait while WeSay updates its caches to match the new or modified LIFT file.";
		//        BackgroundWorker cacheBuildingWork = new BackgroundWorker();
		//        cacheBuildingWork.DoWork += builder.OnDoWork;
		//        dlg.BackgroundWorker = cacheBuildingWork;
		//        dlg.CanCancel = true;
		//        dlg.ShowDialog();
		//        if (dlg.DialogResult != DialogResult.OK)
		//        {
		//            Exception err = dlg.ProgressStateResult.ExceptionThatWasEncountered;
		//            if (err != null)
		//            {
		//                if (err is LiftFormatException)
		//                {
		//                    ErrorReport.NotifyUserOfProblem(
		//                            "WeSay had problems with the content of the dictionary file.\r\n\r\n" +
		//                            err.Message);
		//                }
		//                else
		//                {
		//                    ErrorReport.ReportException(err, null, false);
		//                }
		//            }
		//            else if (dlg.ProgressStateResult.State ==
		//                     ProgressState.StateValue.StoppedWithError)
		//            {
		//                ErrorReport.NotifyUserOfProblem(
		//                        "Could not build caches. " + dlg.ProgressStateResult.LogString,
		//                        null,
		//                        false);
		//            }
		//            return false;
		//        }
		//        _repository.BackendLiftIsFreshNow();
		//    }
		//    return true;
		//}
	}
}