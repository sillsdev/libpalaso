using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SIL.Windows.Forms.ClearShare;
using SIL.Windows.Forms.ClearShare.WinFormsUI;
using SIL.Windows.Forms.Widgets.BetterGrid;

namespace SIL.Windows.Forms.TestApp
{
	public partial class ContributorsForm : Form
	{
		private TableLayoutPanel _tableLayout;
		private ContributorsListControl _contributorsControl;
		private ContributorsListControlViewModel _model;
		private BetterGrid _contributorNames;

		private class AutoCompleter : IAutoCompleteValueProvider
		{
			public BetterGrid Source {get; set;}

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

		public ContributorsForm()
		{
			var autoCompleter = new AutoCompleter();
			_model = new ContributorsListControlViewModel(autoCompleter, () => { });
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
			autoCompleter.Source = _contributorNames;

			var contribs = new ContributionCollection(new [] { new Contribution("Fred", new Role("a", "Author", "guy who writes stuff")) });
			_model.SetContributionList(contribs);
		}

		private void InitializeComponent()
		{
			SuspendLayout();
			AutoScaleDimensions = new SizeF(6F, 13F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(700, 350);

			_tableLayout = new TableLayoutPanel
			{
				Name = "_tableLayout",
				Dock = DockStyle.Top,
				AutoSize = true,
				BackColor = Color.Transparent,
				ColumnCount = 2,
				RowCount = 2
			};
			_tableLayout.SuspendLayout();
			_tableLayout.ColumnStyles.Add(new ColumnStyle());
			_tableLayout.Location = new Point(0, 0);

			var btnUpdateContributorNames = new Label();
			btnUpdateContributorNames.Text = "Hover over this text to Update Contributors";
			btnUpdateContributorNames.MouseEnter += UpdateNames;
			btnUpdateContributorNames.AutoSize = true;
			btnUpdateContributorNames.Anchor = AnchorStyles.Right;

			_contributorNames = new BetterGrid();
			((ISupportInitialize)_contributorNames).BeginInit();
			_contributorNames.AllowUserToAddRows = false;
			_contributorNames.AllowUserToDeleteRows = false;
			_contributorNames.AllowUserToResizeRows = false;
			_contributorNames.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			_contributorNames.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			_contributorNames.Dock = System.Windows.Forms.DockStyle.Fill;
			_contributorNames.DrawTextBoxEditControlBorder = false;
			_contributorNames.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			_contributorNames.Location = new System.Drawing.Point(1, 1);
			_contributorNames.Name = "_contributorNames";
			_contributorNames.RowHeadersVisible = false;
			var colName = new DataGridViewColumn(new DataGridViewTextBoxCell());
			colName.HeaderText = "Name";
			_contributorNames.Columns.Add(colName);
			_contributorNames.RowCount = 3;
			_contributorNames.Rows[0].Cells[0].Value = "Andrew";
			_contributorNames.Rows[1].Cells[0].Value = "Fred";
			_contributorNames.Rows[2].Cells[0].Value = "Tom";

			_tableLayout.Controls.Add(_contributorsControl, 0, 0);
			_tableLayout.SetColumnSpan(_contributorsControl, 2);
			_tableLayout.Controls.Add(_contributorNames, 0, 1);
			_tableLayout.Controls.Add(btnUpdateContributorNames, 1, 1);
			
			_tableLayout.ColumnStyles[0].SizeType = SizeType.Percent;
			_tableLayout.ColumnStyles.Add(new ColumnStyle { SizeType = SizeType.AutoSize });

			_tableLayout.RowStyles.Add(new RowStyle { SizeType = SizeType.Percent, Height = 50 });
			_tableLayout.RowStyles.Add(new RowStyle { SizeType = SizeType.Percent, Height = 50 });

			_tableLayout.Dock = DockStyle.Fill;

			Controls.Add(_tableLayout);

			Name = "ContributorsForm";
			Text = "Contributors";

			((ISupportInitialize)_contributorNames).EndInit();
			_tableLayout.ResumeLayout(true);
			ResumeLayout(true);
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

		/// ------------------------------------------------------------------------------------
		private void UpdateNames(object sender, EventArgs e)
		{
			var contribs = _model.Contributions;
			for (var i = 0; i < _contributorNames.RowCount; i++)
			{
				if (i < contribs.Count)
				{
					contribs[i].ContributorName = (string)_contributorNames.Rows[i].Cells[0].Value;
				}
			}

			_model.SetContributionList(contribs);
		}
	}
}
