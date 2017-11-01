using System;
using System.Collections.Generic;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.Progress;
using Palaso.Text;

namespace Palaso.DictionaryServices.Lift
{
	public class LiftDataMapper : LiftDataMapper<LexEntry>
	{
		private class LiftReaderWriterProvider : ILiftReaderWriterProvider<LexEntry>
		{
			private readonly ProgressState _progressState;
			private readonly OptionsList _semanticDomainsList;
			private readonly IEnumerable<string> _idsOfSingleOptionFields;

			public LiftReaderWriterProvider(ProgressState progressState, OptionsList semanticDomainsList, IEnumerable<string> idsOfSingleOptionFields)
			{
				_progressState = progressState;
				_semanticDomainsList = semanticDomainsList;
				_idsOfSingleOptionFields = idsOfSingleOptionFields;
			}

			public ILiftWriter<LexEntry> CreateWriter(string liftFilePath)
			{
				return new LiftWriter(liftFilePath, LiftWriter.ByteOrderStyle.BOM);
			}

			public virtual ILiftReader<LexEntry> CreateReader()
			{
				return InnerCreateReader();
			}

			protected ILiftReader<LexEntry> InnerCreateReader()
			{
				return new LiftReader(_progressState, _semanticDomainsList, _idsOfSingleOptionFields);
			}
		}

		public LiftDataMapper(string filePath, OptionsList semanticDomainsList, IEnumerable<string> idsOfSingleOptionFields, ProgressState progressState)
			: base(filePath, progressState, new LiftReaderWriterProvider(progressState, semanticDomainsList, idsOfSingleOptionFields))
		{
		}



		/// <summary>
		/// unit tests only
		/// </summary>
		/// <param name="filePath"></param>
		public LiftDataMapper(string filePath)
			: this(filePath, new OptionsList(), new string[] { }, new ProgressState())
		{
		}
	}

	/// <summary>
	/// this subclass is for adding wesay-specific policies, i.e. auto-populating definitions from glosses
	/// </summary>
	public class WeSayLiftDataMapper : LiftDataMapper
	{
		private bool _glossMeaningField;
		public WeSayLiftDataMapper(string filePath, OptionsList semanticDomainsList, IEnumerable<string> idsOfSingleOptionFields, ProgressState progressState, bool glossMeaningField)
			: base(filePath,semanticDomainsList,idsOfSingleOptionFields,progressState)
		{
			_glossMeaningField = glossMeaningField;
			Init();
		}

		protected override void CustomizeReader(ILiftReader<LexEntry> reader)
		{
			reader.AfterEntryRead += PostProcessSenses;
			base.CustomizeReader(reader);
		}


		/// <summary>
		/// We do this because in linguist tools, there is a distinction that we don't want to
		/// normally make in WeSay.  There, "gloss" is the first pass at a definition, but its
		/// really just for interlinearlization.  That isn't a distinction we want our user
		/// to bother with.
		/// </summary>
		private void PostProcessSenses(object entryObj, EventArgs eventArgs)
		{
			var entry = (LexEntry) entryObj;
			foreach (LexSense sense in entry.Senses)
			{
				if (!_glossMeaningField)
				{
					CopyOverGlossesIfDefinitionsMissing(sense);
				}
				FixUpOldLiteralMeaningMistake(entry, sense);
			}
		}


		private void CopyOverGlossesIfDefinitionsMissing(LexSense sense)
		{
			foreach (LanguageForm form in sense.Gloss.Forms)
			{
				if (!sense.Definition.ContainsAlternative(form.WritingSystemId))
				{
					sense.Definition.SetAlternative(form.WritingSystemId, form.Form);
				}
			}
		}

		/// <summary>
		/// we initially, mistakenly put literal meaning on sense. This moves
		/// it and switches to newer naming style.
		/// </summary>
		internal void FixUpOldLiteralMeaningMistake(LexEntry entry, LexSense sense)
		{
			KeyValuePair<string, IPalasoDataObjectProperty> prop = sense.Properties.Find(p => p.Key == "LiteralMeaning");
			if (prop.Key != null)
			{
				sense.Properties.Remove(prop);
				//change the label and move it up to lex entry
				var newGuy = new KeyValuePair<string, IPalasoDataObjectProperty>("literal-meaning", prop.Value);
				entry.Properties.Add(newGuy);
			}
		}
	}
}