using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ClearShare.WinFormsUI;

namespace SIL.Windows.Forms.TestApp
{
	public partial class ContributorsForm : Form
	{
		private TableLayoutPanel _tableLayout;
		private ContributorsListControl _contributorsControl;
		private ContributorsListControlViewModel _model;

		private class AutoCompleter : IAutoCompleteValueProvider
		{
			public IEnumerable<string> GetValuesForKey(string key)
			{
				if (key == "person")
				{
					yield return "Andrew";
					yield return "Fred";
					yield return "Tom";
				}
			}
		}

		public ContributorsForm()
		{
			_model = new ContributorsListControlViewModel(new AutoCompleter(), () => { });
			var dataGridView = new DataGridView();

			_contributorsControl = new ContributorsListControl(_model);
			_contributorsControl.Dock = DockStyle.Fill;
			_contributorsControl.Location = new System.Drawing.Point(0, 0);
			_contributorsControl.Name = "_contributorsControl";

			_contributorsControl.ValidatingContributor += HandleValidatingContributor;

			// set the column header text
			string[] headerText =
			{
				"Name",
				"Role",
				"Date",
				"Comments"
			};

			for (var i = 0; i < headerText.Length; i++)
				_contributorsControl.SetColumnHeaderText(i, headerText[i]);

			InitializeComponent();

			var contribs = new ContributionCollection(new [] { new Contribution("Fred", new Role("a", "Author", "guy who writes stuff")) });
			_model.SetContributionList(contribs);
		}

		private void InitializeComponent()
		{
			SuspendLayout();
			AutoScaleDimensions = new SizeF(6F, 13F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(700, 350);
			Controls.Add(_contributorsControl);
			Name = "ContributorsForm";
			Text = "Contributors";

			ResumeLayout(false);
		}

		/// ------------------------------------------------------------------------------------
		private KeyValuePair<string, string> HandleValidatingContributor(ContributorsListControl sender,
			Contribution contribution, CancelEventArgs e)
		{
			var kvp = CheckIfContributorIsValid(contribution);
			e.Cancel = !string.IsNullOrEmpty(kvp.Key);
			return kvp;
		}

		/// ------------------------------------------------------------------------------------
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
	}
}
