using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using SIL.Core.ClearShare;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ClearShare.WinFormsUI;
using SIL.Windows.Forms.Widgets.BetterGrid;
using static SIL.Windows.Forms.ClearShare.WinFormsUI.ContributorsListControl;

namespace SIL.Windows.Forms.TestApp
{
	public partial class ContributorsForm : Form
	{
		private ContributorsListControlViewModel _model;

		public ContributorsForm()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (DesignMode)
				return;

			var autoCompleter = new AutoCompleter { Source = _contributorNames };
			_model = new ContributorsListControlViewModel(autoCompleter, () => { });
			_contributorsControl.SetColumnAutoSizeMode(StandardColumns.Name, DataGridViewAutoSizeColumnMode.AllCells);
			_contributorsControl.SetColumnAutoSizeMode(StandardColumns.Role, DataGridViewAutoSizeColumnMode.AllCells);
			_contributorsControl.SetColumnAutoSizeMode(StandardColumns.Comments, DataGridViewAutoSizeColumnMode.Fill);

			// Initialize contributions
			var contribs = new ContributionCollection(new[]
			{
				new Contribution("Fred", new Role("a", "Author", "guy who writes stuff"))
			});
			_model.SetContributionList(contribs);

			// Set column headers
			string[] headerText = { "Name", "Role", "Date", "Comments" };
			for (var i = 0; i < headerText.Length; i++)
				_contributorsControl.SetColumnHeaderText(i, headerText[i]);

			timerToTestNonUiThreadAccess.Start();
		}

		private void UpdateNames(object sender, EventArgs e)
		{
			if (!_contributorsControl.Validate())
				return;

			var contribs = _model.Contributions;
			for (var i = 0; i < _contributorNames.RowCount && i < contribs.Count; i++)
			{
				contribs[i].ContributorName = _contributorNames.Rows[i].Cells[0].Value as string;
			}

			_model.SetContributionList(contribs);
		}

		private KeyValuePair<string, string> HandleValidatingContributor(ContributorsListControl sender,
			Contribution contribution, CancelEventArgs e)
		{
			var kvp = CheckIfContributorIsValid(contribution);
			e.Cancel = !string.IsNullOrEmpty(kvp.Key);
			return kvp;
		}

		private static KeyValuePair<string, string> CheckIfContributorIsValid(Contribution contribution)
		{
			if (contribution != null)
			{
				if (string.IsNullOrEmpty(contribution.ContributorName))
					return new KeyValuePair<string, string>("name", "Enter a name.");

				if (contribution.Role == null)
					return new KeyValuePair<string, string>("role", "Choose a role.");
			}

			return new KeyValuePair<string, string>();
		}

		private class AutoCompleter : IAutoCompleteValueProvider
		{
			public BetterGrid Source { get; set; }

			public IEnumerable<string> GetValuesForKey(string key)
			{
				if (key == "person")
				{
					foreach (DataGridViewRow row in Source.Rows)
					{
						yield return row.Cells[0].Value as string;
					}
				}
			}
		}

		private void timerToTestNonUiThreadAccess_Tick(object sender, EventArgs e)
		{
			timerToTestNonUiThreadAccess.Tick -= timerToTestNonUiThreadAccess_Tick;

			if (!_contributorsControl.InEditMode && !_contributorsControl.InNewContributionRow)
				_contributorsControl.SetColumnAutoSizeMode(StandardColumns.Date, DataGridViewAutoSizeColumnMode.DisplayedCells);
		}
	}
}
