using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using Palaso.Code;
using Palaso.UI.WindowsForms.Widgets.Grid;

namespace Palaso.UI.WindowsForms.ClearShare.WinFormsUI
{
	/// ----------------------------------------------------------------------------------------
	public partial class ContributorsListControl : UserControl
	{
		public delegate KeyValuePair<string, string> ValidatingContributorHandler(
			ContributorsListControl sender, Contribution contribution, CancelEventArgs e);

		public event ValidatingContributorHandler ValidatingContributor;

		private FadingMessageWindow _msgWindow;
		private readonly ContributorsListControlViewModel _model;

		/// ------------------------------------------------------------------------------------
		public ContributorsListControl()
		{
			InitializeComponent();
		}

		/// ------------------------------------------------------------------------------------
		public ContributorsListControl(ContributorsListControlViewModel model) : this()
		{
			_model = model;
			_model.NewContributionListAvailable += HandleNewContributionListAvailable;
			Initialize();
		}

		/// ------------------------------------------------------------------------------------
		private void Initialize()
		{
			_grid.Font = SystemFonts.MenuFont;

			// TODO: Localize column headings

			DataGridViewColumn col = BetterGrid.CreateTextBoxColumn("name", "Name");
			col.Width = 150;
			_grid.Columns.Add(col);

			col = BetterGrid.CreateDropDownListComboBoxColumn("role",
				_model.OlacRoles.Select(r => r.ToString()));
			col.HeaderText = "Role";
			col.Width = 120;
			_grid.Columns.Add(col);

			_grid.Columns.Add(BetterGrid.CreateCalendarControlColumn("date", "Date"));

			col = BetterGrid.CreateTextBoxColumn("comments", "Comments");
			col.Width = 200;
			_grid.Columns.Add(col);

			_grid.AddRemoveRowColumn(null, null,
				null /* TODO: Enhance BetterGrid to be able to show tool tips in non-virtual mode */,
				rowIndex => DeleteRow(rowIndex));

			_grid.AllowUserToAddRows = true;
			_grid.AllowUserToDeleteRows = true;

			_grid.EditingControlShowing += HandleEditingControlShowing;
			_grid.RowValidating += HandleGridRowValidating;
			_grid.MouseClick += HandleGridMouseClick;
			_grid.Leave += HandleGridLeave;
			_grid.Enter += delegate { _grid.SelectionMode = DataGridViewSelectionMode.CellSelect; };
			_grid.RowValidated += HandleGridRowValidated;
			_grid.RowsRemoved += HandleGridRowsRemoved;

			if (_model.ContributorsGridSettings != null)
				_model.ContributorsGridSettings.InitializeGrid(_grid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether or not the extender is currently in design mode.
		/// I have had some problems with the base class' DesignMode property being true
		/// when in design mode. I'm not sure why, but adding a couple more checks fixes the
		/// problem.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private new bool DesignMode
		{
			get
			{
				return (base.DesignMode || GetService(typeof(IDesignerHost)) != null) ||
					(LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			}
		}

		/// ------------------------------------------------------------------------------------
		public bool InEditMode
		{
			get { return _grid.IsCurrentRowDirty; }
		}

		/// ------------------------------------------------------------------------------------
		public bool InNewContributionRow
		{
			get { return (_grid.CurrentCellAddress.Y == _grid.NewRowIndex); }
		}

		/// ------------------------------------------------------------------------------------
		public Contribution GetCurrentContribution()
		{
			return GetContributionFromRow(_grid.CurrentCellAddress.Y);
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (!DesignMode)
				_model.ContributorsGridSettings = GridSettings.Create(_grid);

			base.OnHandleDestroyed(e);
		}

		/// ------------------------------------------------------------------------------------
		void HandleNewContributionListAvailable(object sender, EventArgs e)
		{
			Guard.AgainstNull(_model.Contributions, "Contributions");

			_grid.RowValidated -= HandleGridRowValidated;
			_grid.RowsRemoved -= HandleGridRowsRemoved;
			_grid.Rows.Clear();

			foreach (var contribution in _model.Contributions)
				_grid.Rows.Add(contribution.ContributorName, contribution.Role.Name, contribution.Date, contribution.Comments);

			_grid.CurrentCell = _grid[0, 0];
			_grid.IsDirty = false;

			_grid.RowValidated += HandleGridRowValidated;
			_grid.RowsRemoved += HandleGridRowsRemoved;
		}

		/// ------------------------------------------------------------------------------------
		void HandleGridMouseClick(object sender, MouseEventArgs e)
		{
			var hi = _grid.HitTest(e.X, e.Y);

			// Did the user click on a row heading?
			if (e.Button != MouseButtons.Left || hi.ColumnIndex >= 0 ||
				hi.RowIndex < 0 || hi.RowIndex >= _grid.RowCount)
			{
				return;
			}

			// At this point we know the user clicked on a row heading. Now we
			// need to make sure the row they're leaving is in a valid state.
			if (ValidatingContributor != null && _grid.CurrentCellAddress.Y >= 0 &&
				_grid.CurrentCellAddress.Y < _grid.RowCount - 1)
			{
				if (_grid.CurrentCellAddress.Y == _model.Contributions.Count())
					return;

				var contribution = _model.Contributions.ElementAt(_grid.CurrentCellAddress.Y);
				var args = new CancelEventArgs(false);
				ValidatingContributor(this, contribution, args);
				if (args.Cancel)
				{
					SystemSounds.Beep.Play();
					return;
				}
			}

			// Make the first cell current in the row the user clicked.
			_grid.CurrentCell = _grid[0, hi.RowIndex];
		}

		/// ------------------------------------------------------------------------------------
		void HandleGridLeave(object sender, EventArgs e)
		{
			_grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

			if (_grid.CurrentRow != null)
				_grid.CurrentRow.Selected = true;
		}

		/// ------------------------------------------------------------------------------------
		private void HandleGridRowValidating(object sender, DataGridViewCellCancelEventArgs e)
		{
			if (e.RowIndex == _grid.RowCount - 1 || !_grid.IsCurrentRowDirty)
				return;

			_grid.IsDirty = true;

			var contribution = GetContributionFromRow(e.RowIndex);

			if (ValidatingContributor == null)
				return;

			var args = new CancelEventArgs(e.Cancel);
			var kvp = ValidatingContributor(this, contribution, args);
			e.Cancel = args.Cancel;

			if (!string.IsNullOrEmpty(kvp.Key))
			{
				if (_msgWindow == null)
					_msgWindow = new FadingMessageWindow();

				int col = _grid.Columns[kvp.Key].Index;
				var rc = _grid.GetCellDisplayRectangle(col, e.RowIndex, true);
				var pt = new Point(rc.X + (rc.Width / 2), rc.Y + 4);
				_msgWindow.Show(kvp.Value, _grid.PointToScreen(pt));
				_grid.CurrentCell = _grid[col, e.RowIndex];
			}
		}

		/// ------------------------------------------------------------------------------------
		void HandleGridRowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			SaveContributions();
		}

		/// ------------------------------------------------------------------------------------
		void HandleGridRowValidated(object sender, DataGridViewCellEventArgs e)
		{
			SaveContributions();
		}

		/// ------------------------------------------------------------------------------------
		private void SaveContributions()
		{
			if (_grid.IsDirty)
			{
				_model.SaveContributionList(new ContributionCollection(GetContributionCollectionFromGrid()));
				_grid.IsDirty = false;
			}
		}

		/// ------------------------------------------------------------------------------------
		private IEnumerable<Contribution> GetContributionCollectionFromGrid()
		{
			return _grid.GetRows().Where(r =>
				r.Index != _grid.NewRowIndex).Select(row => GetContributionFromRow(row.Index)).Where(c => c!= null);
		}

		/// ------------------------------------------------------------------------------------
		private Contribution GetContributionFromRow(int rowIndex)
		{
			var row = _grid.Rows[rowIndex];

			var contribution = new Contribution
			{
				ContributorName = row.Cells["name"].Value as string,
				Role = _model.OlacRoles.FirstOrDefault(o => o.Name == row.Cells["role"].Value as string),
				Comments = row.Cells["comments"].Value as string
			};

			if (row.Cells["date"].Value != null)
				contribution.Date = (DateTime)row.Cells["date"].Value;

			return contribution;
		}

		/// ------------------------------------------------------------------------------------
		protected void HandleEditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
		{
			if (_grid.CurrentCellAddress.X == 0)
			{
				var txtBox = e.Control as TextBox;
				_grid.Tag = txtBox;
				_grid.CellEndEdit += HandleGridCellEndEdit;
				txtBox.KeyPress += HandleCellEditBoxKeyPress;
				txtBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
				txtBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
				txtBox.AutoCompleteCustomSource = _model.GetAutoCompleteNames();
			}
			else if (_grid.CurrentCellAddress.X == 1)
			{
				var cboBox = e.Control as ComboBox;
				_grid.Tag = cboBox;
				cboBox.SelectedIndexChanged += HandleRoleValueChanged;
				_grid.CellEndEdit += HandleGridCellEndEdit;
			}
		}

		/// ------------------------------------------------------------------------------------
		void HandleRoleValueChanged(object sender, EventArgs e)
		{
			if (_msgWindow != null)
				_msgWindow.Close();
		}

		/// ------------------------------------------------------------------------------------
		void HandleGridCellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			var ctrl = _grid.Tag as Control;

			if (ctrl is TextBox)
				ctrl.KeyPress -= HandleCellEditBoxKeyPress;
			else if (ctrl is ComboBox)
				((ComboBox)ctrl).SelectedIndexChanged -= HandleRoleValueChanged;

			_grid.CellEndEdit -= HandleGridCellEndEdit;
			_grid.Tag = null;
		}

		/// ------------------------------------------------------------------------------------
		private void HandleCellEditBoxKeyPress(object sender, KeyPressEventArgs e)
		{
			// Prevent characters that are invalid as xml tags. There's probably more,
			// but this will do for now.
			if ("<>{}()[]/'\"\\.,;:?|!@#$%^&*=+`~".IndexOf(e.KeyChar) >= 0)
			{
				e.KeyChar = (char)0;
				e.Handled = true;
				SystemSounds.Beep.Play();
			}
		}

		/// ------------------------------------------------------------------------------------
		private void DeleteRow(int rowIndex)
		{
			if (_grid.IsCurrentCellInEditMode)
				_grid.EndEdit(DataGridViewDataErrorContexts.RowDeletion);

			_grid.Rows.RemoveAt(rowIndex);
			_grid.CurrentCell = _grid[0, _grid.CurrentCellAddress.Y];
		}
	}
}
