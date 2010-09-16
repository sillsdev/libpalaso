using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Text;

namespace Palaso.DictionaryServices.Queries
{
	public class PalasoDataObjectMultiTextQuery : IQuery<PalasoDataObject>
	{
		private string _fieldLabel;
		private string _writingsystemId;

		public PalasoDataObjectMultiTextQuery(string fieldlabel, string writingSystemId)
		{
			_fieldLabel = fieldlabel;
			_writingsystemId = writingSystemId;
		}

		public override IEnumerable<IDictionary<string, object>> GetResults(PalasoDataObject item)
		{
			var fieldsandValuesFromProperties = new List<IDictionary<string, object>>();
			foreach (KeyValuePair<string, object> fieldLabelAndFieldPair in item.Properties)
			{
				if (fieldLabelAndFieldPair.Key == _fieldLabel)
				{
					bool fieldIsAMultiText = fieldLabelAndFieldPair.Value.GetType() == typeof(MultiText);
					if (fieldIsAMultiText)
					{
						MultiText multiText = (MultiText)fieldLabelAndFieldPair.Value;
						IDictionary<string, object> newFieldsAndValues = new Dictionary<string, object>();
						newFieldsAndValues.Add(_fieldLabel, multiText[_writingsystemId]);
						newFieldsAndValues.Add("WritingSystem", _writingsystemId);
						newFieldsAndValues.Add("Guid", item.Guid);
						int guidLevel = 1;
						while(item.Parent!=null)
						{
							string newFieldLabel = guidLevel.ToString() + "Guid";
							newFieldsAndValues.Add(newFieldLabel, item.Parent.Guid);
							item = item.Parent;
						}
						fieldsandValuesFromProperties.Add(newFieldsAndValues);
					}
					else
					{
						throw new InvalidOperationException(String.Format("The field associated with field label '{0}' is not of type 'MultiText'.", _fieldLabel, fieldLabelAndFieldPair.Value.GetType()));
					}
				}
			}
			return fieldsandValuesFromProperties;
		}

		public override IEnumerable<SortDefinition> SortDefinitions
		{
			get
			{
				var sortorder = new SortDefinition[2];
				sortorder[0] = new SortDefinition(_fieldLabel, Comparer<string>.Default);
				sortorder[1] = new SortDefinition("WritingSystem", Comparer<string>.Default);
				return sortorder;
			}
		}

		public override string UniqueLabel
		{
			get{ return "PalasoDataObjectMultiTextQuery" + _fieldLabel + _writingsystemId;}
		}

		public override bool IsUnpopulated(IDictionary<string, object> resultToCheck)
		{
			throw new NotImplementedException();
		}
	}
}
