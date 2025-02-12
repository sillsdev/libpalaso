using System;
using System.Collections.Generic;
using SIL.Data;
using SIL.DictionaryServices.Model;
using SIL.Lift;
using SIL.Lift.Options;
using SIL.Lift.Parsing;
using SIL.Lift.Validation;
using SIL.Progress;
using SIL.Reporting;

namespace SIL.DictionaryServices.Lift
{
	public class LiftReader : ILiftReader<LexEntry>
	{
		private readonly ProgressState _progressState;
		private readonly OptionsList _semanticDomainsList; // Review: how is this used in LexEntryFromLiftBuilder.

		/// <summary>
		/// the lift format puts all options in "&lt;trait/&gt;", so we need this to know when we have an option collection and when it's a single option field
		/// </summary>
		private readonly IEnumerable<string> _idsOfSingleOptionFields;

		 public LiftReader(ProgressState progressState, OptionsList semanticDomainsList, IEnumerable<string>  namesOfSingleOptionFields)
		{
			_progressState = progressState;
			_semanticDomainsList = semanticDomainsList;
			_idsOfSingleOptionFields = namesOfSingleOptionFields;
		}


		/// <summary>
		/// Subscribe to this event in order to do something (or do something to an entry) as soon as it has been parsed in.
		/// WeSay uses this to populate definitions from glosses.
		/// </summary>
		public event EventHandler AfterEntryRead;

	    public void Read(string filePath, MemoryDataMapper<LexEntry> dataMapper)
		{
			const string status = "Loading entries";
			Logger.WriteEvent(status);
			_progressState.StatusLabel = status;

			using (LexEntryFromLiftBuilder builder = new LexEntryFromLiftBuilder(dataMapper, _semanticDomainsList))
			{
				builder.AfterEntryRead += new EventHandler<LexEntryFromLiftBuilder.EntryEvent>(OnAfterEntryRead);
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
					_progressState.StatusLabel = "Looking for error in file...";

					//our parser failed.  Hopefully, because of bad lift. Validate it now  to
					//see if that's the problem.
					Validator.CheckLiftWithPossibleThrow(filePath);

					//if it got past that, ok, send along the error the parser encountered.
					throw;
				}
			}
		}

		/// <summary>
		/// this just passes on the event to our client, who can't directly access the LexEntryFromLiftBuilder
		/// </summary>
		private void OnAfterEntryRead(object sender, LexEntryFromLiftBuilder.EntryEvent e)
		{
			if(AfterEntryRead !=null)
			{
				AfterEntryRead.Invoke(sender, e);
			}
		}

		private void parser_ParsingWarning(
			object sender,
			LiftParser<PalasoDataObject, LexEntry, LexSense, LexExampleSentence>.ErrorArgs e
		)
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
		~LiftReader()
		{
			if (!Disposed)
			{
				throw new ApplicationException("Disposed not explicitly called on LiftReader.");
			}
		}
#endif

		protected bool Disposed { get; set; }

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				if (disposing)
				{
					// dispose-only, i.e. non-finalizable logic
				}

				// shared (dispose and finalizable) cleanup logic
				Disposed = true;
			}
		}

		protected void VerifyNotDisposed()
		{
			if (!Disposed)
			{
				throw new ObjectDisposedException("WeSayLiftReader");
			}
		}
		#endregion
	}
}