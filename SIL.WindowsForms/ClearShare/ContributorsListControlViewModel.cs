using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SIL.WindowsForms.Widgets.BetterGrid;

namespace SIL.WindowsForms.ClearShare
{
	public class ContributorsListControlViewModel
	{
		public event EventHandler NewContributionListAvailable;

		private readonly OlacSystem _olacSystem = new OlacSystem();
		private readonly IAutoCompleteValueProvider _autoCompleteProvider;
		private readonly Action _saveAction;

		public ContributionCollection Contributions { get; protected set; }

		/// ------------------------------------------------------------------------------------
		public ContributorsListControlViewModel(IAutoCompleteValueProvider autoCompleteProvider,
			Action saveAction)
		{
			_autoCompleteProvider = autoCompleteProvider;
			_saveAction = saveAction;
		}

		/// ------------------------------------------------------------------------------------
		public IEnumerable<Role> OlacRoles
		{
			get { return _olacSystem.GetRoles(); }
		}

		/// ------------------------------------------------------------------------------------
		public GridSettings ContributorsGridSettings
		{
			get;

			//TODO this get/set might not be the right way... in any case on set, we need to save
			set;
		}

		/// ------------------------------------------------------------------------------------
		public void SetContributionList(ContributionCollection list)
		{
			Contributions = (list ?? new ContributionCollection());

			if (NewContributionListAvailable != null)
				NewContributionListAvailable(this, EventArgs.Empty);
		}

		/// ------------------------------------------------------------------------------------
		internal void SaveContributionList(ContributionCollection list)
		{
			Contributions = (list ?? new ContributionCollection());

			if (_saveAction != null)
				_saveAction();
		}

		/// ------------------------------------------------------------------------------------
		public AutoCompleteStringCollection GetAutoCompleteNames()
		{
			var list = (_autoCompleteProvider != null ?
				_autoCompleteProvider.GetValuesForKey("person") : new List<string>(0));

			var autoCompleteValues = new AutoCompleteStringCollection();
			autoCompleteValues.AddRange(list.ToArray());
			return autoCompleteValues;
		}
	}
}
