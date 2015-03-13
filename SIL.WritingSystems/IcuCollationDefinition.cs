using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SIL.ObjectModel;

namespace SIL.WritingSystems
{
	public class IcuCollationDefinition : CollationDefinition
	{
		private readonly BulkObservableList<IcuCollationImport> _imports;
		private string _icuRules;

		public IcuCollationDefinition(string type)
			: base(type)
		{
			_imports = new BulkObservableList<IcuCollationImport>();
			IsValid = true;
			SetupCollectionChangeListeners();
		}

		public IcuCollationDefinition(IcuCollationDefinition icd)
			: base(icd)
		{
			WritingSystemFactory = icd.WritingSystemFactory;
			_imports = new BulkObservableList<IcuCollationImport>(icd._imports);
			_icuRules = icd._icuRules;
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
				if (WritingSystemFactory == null)
				{
					message = string.Format("Unable to import the {0} collation rules from {1}.", string.IsNullOrEmpty(import.Type) ? "default" : import.Type, import.IetfLanguageTag);
					return false;
				}

				WritingSystemDefinition ws = WritingSystemFactory.Create(import.IetfLanguageTag);
				CollationDefinition cd;
				if (string.IsNullOrEmpty(import.Type))
					cd = ws.DefaultCollation;
				else if (!ws.Collations.TryGet(import.Type, out cd))
					cd = null;
				if (cd != null && cd.IsValid)
				{
					sb.Append(cd.CollationRules);
				}
				else
				{
					message = string.Format("Unable to import the {0} collation rules from {1}.", string.IsNullOrEmpty(import.Type) ? "default" : import.Type, import.IetfLanguageTag);
					return false;
				}
			}

			sb.Append(_icuRules);

			CollationRules = sb.ToString();

			return base.Validate(out message);
		}

		public override bool ValueEquals(CollationDefinition other)
		{
			var icd = other as IcuCollationDefinition;
			return icd != null && base.ValueEquals(icd) && IcuRules == icd.IcuRules && _imports.SequenceEqual(icd._imports);
		}

		public override CollationDefinition Clone()
		{
			return new IcuCollationDefinition(this);
		}
	}
}
