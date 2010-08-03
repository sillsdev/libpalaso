using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.Text;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Queries
{
	class LexicalFormsWithGlossesQuery:IQuery<LexEntry>
	{
		private WritingSystemDefinition _writingSystemDefinition;

		public LexicalFormsWithGlossesQuery(WritingSystemDefinition wsDef)
		{
			_writingSystemDefinition = wsDef;
		}

		public override IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{
			var fieldsandValuesForRecordTokens = new List<IDictionary<string, object>>();
			int senseNumber = 0;
			foreach (LexSense sense in entryToQuery.Senses)
			{
				foreach (LanguageForm form in sense.Gloss.Forms)
				{
					IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
					string lexicalForm = entryToQuery.LexicalForm[_writingSystemDefinition.Id];
					if (String.IsNullOrEmpty(lexicalForm))
					{
						lexicalForm = null;
					}
					tokenFieldsAndValues.Add("Form", lexicalForm);

					string gloss = form.Form;
					if (String.IsNullOrEmpty(gloss))
					{
						gloss = null;
					}
					tokenFieldsAndValues.Add("Gloss", gloss);

					string glossWritingSystem = form.WritingSystemId;
					if (String.IsNullOrEmpty(glossWritingSystem))
					{
						glossWritingSystem = null;
					}
					tokenFieldsAndValues.Add("GlossWritingSystem", glossWritingSystem);
					tokenFieldsAndValues.Add("SenseNumber", senseNumber);
					fieldsandValuesForRecordTokens.Add(tokenFieldsAndValues);
				}
				senseNumber++;
			}
			return fieldsandValuesForRecordTokens;
		}

		public override IEnumerable<SortDefinition>SortDefinitions
		{
			get
			{
				var sortorder = new SortDefinition[4];
				sortorder[0] = new SortDefinition("Form", _writingSystemDefinition.Collator);
				sortorder[1] = new SortDefinition("Gloss", StringComparer.InvariantCulture);
				sortorder[2] = new SortDefinition("GlossWritingSystem", StringComparer.InvariantCulture);
				sortorder[3] = new SortDefinition("SenseNumber", Comparer<int>.Default);
				return sortorder;
			}
		}

		public override string UniqueLabel
		{
			get { return "LexicalFormsWithGlossesQuery"; }
		}

		public override bool IsUnpopulated(IDictionary<string, object> entryToCheckAgainst)
		{
			throw new NotImplementedException();
		}
	}
}
