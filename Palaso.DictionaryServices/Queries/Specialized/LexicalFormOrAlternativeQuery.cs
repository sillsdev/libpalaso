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
	public class LexicalFormOrAlternativeQuery:IQuery<LexEntry>
	{
		private WritingSystemDefinition _writingSystemDefinition;

		public LexicalFormOrAlternativeQuery(WritingSystemDefinition wsDef)
		{
			_writingSystemDefinition = wsDef;
		}

		public override IEnumerable<IDictionary<string, object>> GetResults(LexEntry entryToQuery)
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

		public override IEnumerable<SortDefinition> SortDefinitions
		{
			get
			{
				var sortOrder = new SortDefinition[1];
				sortOrder[0] = new SortDefinition("Form", _writingSystemDefinition.Collator);
				return sortOrder;
			}
		}

		public override string UniqueLabel
		{
			get { return "LexicalFormOrAlternativeQuery"; }
		}

		public override bool IsUnpopulated(IDictionary<string, object> entryToCheckAgainst)
		{
			throw new NotImplementedException();
		}
	}
}
