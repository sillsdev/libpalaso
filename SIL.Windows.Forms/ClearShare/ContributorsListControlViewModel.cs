using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SIL.Windows.Forms.Widgets.BetterGrid;

namespace SIL.Windows.Forms.ClearShare
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
			Contributions = list ?? new ContributionCollection();

			NewContributionListAvailable?.Invoke(this, EventArgs.Empty);
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
			list = GetAutoCompleteList(list);
			var autoCompleteValues = new AutoCompleteStringCollection();
			autoCompleteValues.AddRange(list.ToArray());
			return autoCompleteValues;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Remove role from people name if present
		/// </summary>
		/// <param name="list">list of names</param>
		/// <returns>names list without role</returns>
		/// ------------------------------------------------------------------------------------
		private static string[] GetAutoCompleteList(IEnumerable<string> list)
		{
			return new SortedSet<string>(from name in list
				let i = name.IndexOf(" (", StringComparison.Ordinal)
				select i >= 0? name.Substring(0, i) : name).ToArray();
		}
	}
}
