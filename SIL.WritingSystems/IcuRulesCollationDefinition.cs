using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SIL.ObjectModel;

namespace SIL.WritingSystems
{
	public class IcuRulesCollationDefinition : RulesCollationDefinition
	{
		private readonly BulkObservableList<IcuCollationImport> _imports;
		private string _icuRules;

		public IcuRulesCollationDefinition(string type)
			: base(type)
		{
			_imports = new BulkObservableList<IcuCollationImport>();
			SetupCollectionChangeListeners();
		}

		public IcuRulesCollationDefinition(IcuRulesCollationDefinition ircd)
			: base(ircd)
		{
			WritingSystemFactory = ircd.WritingSystemFactory;
			OwningWritingSystemDefinition = ircd.OwningWritingSystemDefinition;
			_imports = new BulkObservableList<IcuCollationImport>(ircd._imports);
			_icuRules = ircd._icuRules;
			SetupCollectionChangeListeners();
		}

		private void SetupCollectionChangeListeners()
		{
			_imports.CollectionChanged += _imports_CollectionChanged;
		}

		private void _imports_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
			ResetCollator();
		}

		public string IcuRules
		{
			get { return _icuRules ?? string.Empty; }
			set
			{
				if (Set(() => IcuRules, ref _icuRules, value))
					ResetCollator();
			}
		}

		public BulkObservableList<IcuCollationImport> Imports
		{
			get { return _imports; }
		}

		public IWritingSystemFactory WritingSystemFactory { get; set; }
		public WritingSystemDefinition OwningWritingSystemDefinition { get; set; }

		public override bool Validate(out string message)
		{
			if (IsValid)
			{
				message = null;
				return true;
			}

			var sb = new StringBuilder();
			foreach (IcuCollationImport import in _imports)
			{
				bool importSuccessful = false;
				WritingSystemDefinition ws = null;
				if (OwningWritingSystemDefinition != null && OwningWritingSystemDefinition.LanguageTag.StartsWith(import.LanguageTag) && OwningWritingSystemDefinition.Collations.Contains(import.Type))
					ws = OwningWritingSystemDefinition;
				else if (WritingSystemFactory != null && (OwningWritingSystemDefinition == null || OwningWritingSystemDefinition.LanguageTag != import.LanguageTag))
					WritingSystemFactory.Create(import.LanguageTag, out ws);
				CollationDefinition cd;
				if (ws != null && ws.Collations.TryGet(import.Type, out cd))
				{
					var rcd = cd as RulesCollationDefinition;
					string importMessage;
					if (rcd != null && rcd.Validate(out importMessage))
					{
						sb.Append(rcd.CollationRules);
						importSuccessful = true;
					}
				}

				if (!importSuccessful)
				{
					message = string.Format("Unable to import the {0} collation rules from {1}.", string.IsNullOrEmpty(import.Type) ? "default" : import.Type, import.LanguageTag);
					return false;
				}
			}

			sb.Append(_icuRules);

			CollationRules = sb.ToString();

			return base.Validate(out message);
		}

		public override bool ValueEquals(CollationDefinition other)
		{
			var ircd = other as IcuRulesCollationDefinition;
			return ircd != null && base.ValueEquals(ircd) && IcuRules == ircd.IcuRules && _imports.SequenceEqual(ircd._imports);
		}

		public override CollationDefinition Clone()
		{
			return new IcuRulesCollationDefinition(this);
		}
	}
}
