using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SIL.Core.ClearShare;
using SIL.Reporting;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ClearShare.WinFormsUI;
using SIL.Windows.Forms.Widgets.BetterGrid;
using static SIL.Windows.Forms.ClearShare.WinFormsUI.ContributorsListControl;

namespace SIL.Windows.Forms.TestApp
{
	public partial class ContributorsForm : Form
	{
		private ContributorsListControlViewModel _model;
		private readonly Role _authorRole = new Role("a", "Author", "someone who writes stuff");
		private bool _doneTestingNonUiThreadAccess;

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

			// Initialize contributions
			_model.SetContributionList(new ContributionCollection(new[]
			{
				new Contribution("Fred", _authorRole)
			}));
			_contributorsControl.Initialize(_model);

			_contributorsControl.SetColumnAutoSizeMode(StandardColumns.Name, DataGridViewAutoSizeColumnMode.AllCells);
			_contributorsControl.SetColumnAutoSizeMode(StandardColumns.Role, DataGridViewAutoSizeColumnMode.AllCells);
			_contributorsControl.SetColumnAutoSizeMode(StandardColumns.Comments, DataGridViewAutoSizeColumnMode.Fill);

			// Set column headers
			string[] headerText = { "Name", "Role", "Date", "Comments" };
			for (var i = 0; i < headerText.Length; i++)
				_contributorsControl.SetColumnHeaderText(i, headerText[i]);

			StartBackgroundTaskToTestNonUiThreadAccess();
		}

		private async void StartBackgroundTaskToTestNonUiThreadAccess()
		{
			while (!_doneTestingNonUiThreadAccess)
			{
				await Task.Delay(1500); // Space out additions of new contributors
				await Task.Run(TestNonUiThreadAccessOnContributorsControl);
			}
		}

		private void UpdateNames(object sender, EventArgs e)
		{
			if (!_contributorsControl.Validate())
				return;

			var contributions = _model.Contributions;
			for (var i = 0; i < _contributorNames.RowCount && i < contributions.Count; i++)
			{
				contributions[i].ContributorName =
					_contributorNames.Rows[i].Cells[0].Value as string;
			}

			_model.SetContributionList(contributions);
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

		private void TestNonUiThreadAccessOnContributorsControl()
		{
			try
			{
				if (_contributorsControl.InEditMode || _contributorsControl.InNewContributionRow)
				{
					_contributorsControl.SetColumnAutoSizeMode(StandardColumns.Date,
						_model.Contributions.Count % 2 == 0 ?
							DataGridViewAutoSizeColumnMode.DisplayedCells :
							DataGridViewAutoSizeColumnMode.ColumnHeader);
					return;
				}
			}
			catch (ObjectDisposedException)
			{
				Logger.WriteEvent("User must have closed the form before we finished. " +
					"That's fine. But if you think we're going to quit, you're mistaken...");
			}

			var newList = _model.Contributions.ToList();
			string newName;
			switch (newList.Count)
			{
				case 1:
					newName = "Marko";
					break;
				case 2:
					newName = "Ralph";
					break;
				case 3:
					newName = "Hank";
					break;
				case 4:
					newName = "Fredrick";
					break;
				case 5:
					newName = "Timoteo";
					try
					{
						_contributorsControl.SetColumnHeaderText(0, "Nombre");
						_contributorsControl.SetColumnHeaderText(1,
							_contributorsControl.GetCurrentContribution()?.ContributorName ??
							"Rol");
						_contributorsControl.SetColumnHeaderText(2, "Fecha");
					}
					catch (ObjectDisposedException)
					{
						Logger.WriteEvent("And yet we keep going...");
					}

					break;
				case 6:
					newName = "Saul";
					break;
				case 7:
					newName = "Gumby";
					break;
				case 8:
					newName = "Serge";
					break;
				case 9:
					newName = "Linda";
					break;
				default:
					Invoke(new Action(() =>
					{
						_contributorsControl.Grid.DrawMessageInCenterOfGrid(
							Graphics.FromHwnd(_contributorsControl.Grid.Handle), "All done blasting!", 0);
					}));
					_doneTestingNonUiThreadAccess = true;
					return;
			}

			newList.Add(new Contribution(newName, _authorRole));

			_model.SetContributionList(new ContributionCollection(newList));
		}
	}
}
