using System;
using System.Collections.Generic;
using LiftIO.Parsing;
using LiftIO.Validation;
using Palaso.Data;
using Palaso.Lift;
using Palaso.Lift.Model;
using Palaso.Lift.Options;
using Palaso.Progress;
using Palaso.Reporting;

namespace WeSay.LexicalModel
{
	public class WeSayLiftReader : ILiftReader<LexEntry>
	{
		private readonly ProgressState _progressState;
		private readonly OptionsList _semanticDomainsList; // Review: how is this used in LexEntryFromLiftBuilder.

		/// <summary>
		/// the lift format puts all options in "<trait/>", so we need this to know when we have an optioncollection and when it's a single option field</param>
		/// </summary>
		private readonly IEnumerable<string> _idsOfSingleOptionFields;

		 public WeSayLiftReader(ProgressState progressState, OptionsList semanticDomainsList, IEnumerable<string>  namesOfSingleOptionFields)
		{
			_progressState = progressState;
			_semanticDomainsList = semanticDomainsList;
			_idsOfSingleOptionFields = namesOfSingleOptionFields;
		}

		public void Read(string filePath, MemoryDataMapper<LexEntry> dataMapper)
		{
			const string status = "Loading entries";
			Logger.WriteEvent(status);
			_progressState.StatusLabel = status;

			using (LexEntryFromLiftBuilder builder = new LexEntryFromLiftBuilder(dataMapper, _semanticDomainsList))
			{
				builder.ExpectedOptionTraits = _idsOfSingleOptionFields;

				LiftParser<PalasoDataObject, LexEntry, LexSense, LexExampleSentence> parser =
					new LiftParser<PalasoDataObject, LexEntry, LexSense, LexExampleSentence>(
						builder);

				parser.SetTotalNumberSteps += parser_SetTotalNumberSteps;
				parser.SetStepsCompleted += parser_SetStepsCompleted;

				parser.ParsingWarning += parser_ParsingWarning;

				try
				{
					parser.ReadLiftFile(filePath);
					if (_progressState.ExceptionThatWasEncountered != null)
					{
						throw _progressState.ExceptionThatWasEncountered;
					}
				}
				//                        catch (LiftFormatException)
				//                        {
				//                            throw;
				//                        }
				catch (Exception)
				{
					//our parser failed.  Hopefully, because of bad lift. Validate it now  to
					//see if that's the problem.
					Validator.CheckLiftWithPossibleThrow(filePath);

					//if it got past that, ok, send along the error the parser encountered.
					throw;
				}
			}
		}

		private void parser_ParsingWarning(object sender,
										   LiftParser
												   <PalasoDataObject, LexEntry, LexSense,
												   LexExampleSentence>.ErrorArgs e)
		{
			_progressState.ExceptionThatWasEncountered = e.Exception;
		}

		private void parser_SetStepsCompleted(object sender,
											  LiftParser
													  <PalasoDataObject, LexEntry, LexSense,
													  LexExampleSentence>.ProgressEventArgs e)
		{
			_progressState.NumberOfStepsCompleted = e.Progress;
			e.Cancel = _progressState.Cancel;
		}

		private void parser_SetTotalNumberSteps(object sender,
												LiftParser
														<PalasoDataObject, LexEntry, LexSense,
														LexExampleSentence>.StepsArgs e)
		{
			_progressState.TotalNumberOfSteps = e.Steps;
		}

		#region IDisposable Members

#if DEBUG
		~WeSayLiftReader()
		{
			if (!_disposed)
			{
				throw new ApplicationException("Disposed not explicitly called on WeSayLiftReader.");
			}
		}
#endif

		[CLSCompliantAttribute(false)]
		protected bool _disposed;

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// dispose-only, i.e. non-finalizable logic
				}

				// shared (dispose and finalizable) cleanup logic
				_disposed = true;
			}
		}

		protected void VerifyNotDisposed()
		{
			if (!_disposed)
			{
				throw new ObjectDisposedException("WeSayLiftReader");
			}
		}

		#endregion


	}
}