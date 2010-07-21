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
	class LexicalFormOrAlternativeQuery:IQuery<LexEntry>
	{
		private WritingSystemDefinition _writingSystemDefinition;

		public LexicalFormOrAlternativeQuery(WritingSystemDefinition wsDef)
		{
			_writingSystemDefinition = wsDef;
		}

		public IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
		{
			IDictionary<string, object> tokenFieldsAndValues = new Dictionary<string, object>();
			string lexicalform = entryToQuery.LexicalForm[_writingSystemDefinition.Id];
			string writingSystemOfForm = _writingSystemDefinition.Id;
			if (lexicalform == "")
			{
				lexicalform = entryToQuery.LexicalForm.GetBestAlternative(_writingSystemDefinition.Id);
				foreach (LanguageForm form in entryToQuery.LexicalForm.Forms)
				{
					if (form.Form == lexicalform)
					{
						writingSystemOfForm = form.WritingSystemId;
					}
				}
				if (lexicalform == "")
				{
					lexicalform = null;
				}
			}
			tokenFieldsAndValues.Add("Form", lexicalform);
			tokenFieldsAndValues.Add("WritingSystem", writingSystemOfForm);
			return new[] { tokenFieldsAndValues };
		}

		public SortDefinition[] SortDefinitions
		{
			get
			{
				var sortOrder = new SortDefinition[1];
				sortOrder[0] = new SortDefinition("Form", _writingSystemDefinition.Collator);
				return sortOrder;
			}
		}

		public string UniqueLabel
		{
			get { return "LexicalFormOrAlternativeQuery"; }
		}
	}
}
