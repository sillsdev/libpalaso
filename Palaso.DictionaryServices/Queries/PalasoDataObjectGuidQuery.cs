using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.Lift;

namespace Palaso.DictionaryServices.Queries
{
	public class PalasoDataObjectGuidQuery:IQuery<PalasoDataObject>
	{
		public override IEnumerable<IDictionary<string, object>> GetResults(PalasoDataObject item)
		{
			var fieldsandValuesFromProperties = new List<IDictionary<string, object>>();
			Dictionary<string, object> fieldAndValue = new Dictionary<string, object>();
			fieldAndValue.Add("Guid", item.Guid);
			fieldsandValuesFromProperties.Add(fieldAndValue);
			return fieldsandValuesFromProperties;
		}

		public override IEnumerable<SortDefinition> SortDefinitions
		{
			get
			{
				var sortorder = new SortDefinition[1];
				sortorder[0] = new SortDefinition("Guid", Comparer<Guid>.Default);
				return sortorder;
			}
		}

		public override string UniqueLabel
		{
			get { return "GuidQuery"; }
		}

		public override bool IsUnpopulated(IDictionary<string, object> resultToCheck)
		{
			throw new NotImplementedException();
		}
	}
}
