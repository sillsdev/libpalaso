using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using JetBrains.Annotations;
using L10NSharp;
using L10NSharp.Windows.Forms;
using SIL.Code;
using SIL.Core.ClearShare;
using SIL.Reporting;
using SIL.Windows.Forms.Extensions;
using SIL.Windows.Forms.Widgets.BetterGrid;
using static System.String;

namespace SIL.Windows.Forms.ClearShare.WinFormsUI
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Control that displays an editable list of contributors with standard OLAC (Open Language
	/// Archives Community) roles. (See http://www.language-archives.org/REC/role.html).
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public partial class ContributorsListControl : UserControl
	{
		private const string kNameColName = "name";
		private const string kRoleColName = "role";
		private const string kCommentsColName = "comments";
		private const string kDateColName = "date";

		public enum StandardColumns
		{
			Name,
			Role,
			Comments,
			Date,
		}

		public delegate KeyValuePair<string, string> ValidatingContributorHandler(
			ContributorsListControl sender, Contribution contribution, CancelEventArgs e);
		public event ValidatingContributorHandler ValidatingContributor;

		public delegate void ColumnHeaderMouseClickHandler(object sender, DataGridViewCellMouseEventArgs e);
		public event ColumnHeaderMouseClickHandler ColumnHeaderMouseClick;

		private FadingMessageWindow _msgWindow;
		private ContributorsListControlViewModel _model;

		/// ------------------------------------------------------------------------------------
		/// <summary>Default constructor</summary>
		/// <remarks>When using this version of the constructor (e.g., when adding this control
		/// in Designer), you should subsequently call <see cref="Initialize"/> to set the model
		/// (no later than in the <see cref="UserControl.OnLoad"/> event).</remarks>
		/// ------------------------------------------------------------------------------------
		public ContributorsListControl()
		{
			InitializeComponent();
			_grid.DataError += _grid_DataError;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>Constructor that can be used to set the model at object creation time (no
		/// need to call <see cref="Initialize"/>).</summary>
		/// ------------------------------------------------------------------------------------
		public ContributorsListControl(ContributorsListControlViewModel model) : this()
		{
			Initialize(model);
		}

		/// ------------------------------------------------------------------------------------
		private void _grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
		    if (e.Exception != null &&
				e.Context == DataGridViewDataErrorContexts.Commit)
			{
				MessageBox.Show(e.Exception.Message);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>If this object was created using the default constructor (e.g., when added
		/// via Designer), call this to set the model (no later than in the
		/// <see cref="UserControl.OnLoad"/> event).</summary>
		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		public void Initialize(ContributorsListControlViewModel model)
		{
			Debug.Assert(_model == null);

			_model = model;
			_model.NewContributionListAvailable += HandleNewContributionListAvailable;

			// Do not use SafeInvoke here because we want this to work even if called before the
			// handle is created.
			if (InvokeRequired)
				Invoke(new Action(InitializeGrid));
			else
				InitializeGrid();
		}

		/// ------------------------------------------------------------------------------------
		private void InitializeGrid()
		{
			_grid.Font = SystemFonts.MenuFont;

			DataGridViewColumn col = BetterGrid.CreateTextBoxColumn(kNameColName, "Name");
			col.Width = 150;
			_grid.Columns.Add(col);

			col = BetterGrid.CreateDropDownListComboBoxColumn(kRoleColName,
				_model.OlacRoles.Select(r => r.ToString()));
			col.HeaderText = @"Role";
			col.Width = 120;
			_grid.Columns.Add(col);

			_grid.Columns.Add(BetterGrid.CreateCalendarControlColumn(kDateColName, "Date",
				null, CalendarCell.UserAction.CellMouseClick));

			col = BetterGrid.CreateTextBoxColumn(kCommentsColName, "Comments");
			col.Width = 200;
			_grid.Columns.Add(col);

			_grid.AddRemoveRowColumn(null, null,
				null /* TODO: Enhance BetterGrid to be able to show tool tips in non-virtual mode */,
				DeleteRow);

			_grid.AllowUserToAddRows = true;
			_grid.AllowUserToDeleteRows = true;

			_grid.EditingControlShowing += HandleEditingControlShowing;
			_grid.RowValidating += HandleGridRowValidating;
			_grid.MouseClick += HandleGridMouseClick;
			_grid.Leave += HandleGridLeave;
			_grid.Enter += delegate { _grid.SelectionMode = DataGridViewSelectionMode.CellSelect; };
			_grid.RowValidated += HandleGridRowValidated;
			_grid.RowsRemoved += HandleGridRowsRemoved;
			_grid.ColumnHeaderMouseClick += _grid_ColumnHeaderMouseClick;

			_model.ContributorsGridSettings?.InitializeGrid(_grid);

			if (_model.Contributions != null && _model.Contributions.Any())
				HandleNewContributionListAvailable(_model, EventArgs.Empty);
		}

		/// ------------------------------------------------------------------------------------
		// SP-874: Not able to open L10NSharp with Alt-Shift-click
		private void _grid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			ColumnHeaderMouseClick?.Invoke(sender, e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether the extender is currently in design mode.
		/// I have had some problems with the base class' DesignMode property being true
		/// when in design mode. I'm not sure why, but adding a couple more checks fixes the
		/// problem.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private new bool DesignMode => base.DesignMode ||
			GetService(typeof(IDesignerHost)) != null ||
			LicenseManager.UsageMode == LicenseUsageMode.Designtime;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the auto size mode (the mode by which a column may automatically adjust its
		/// width) for the specified column. By default, the columns do not autosize.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Method called after this object has already
		/// been disposed.</exception>
		/// <exception cref="InvalidOperationException">Method called when the model has not been
		/// set (using the <see cref="Initialize"/> method).</exception>
		/// ------------------------------------------------------------------------------------
		public void SetColumnAutoSizeMode(StandardColumns col,
			DataGridViewAutoSizeColumnMode autoSizeMode)
		{
			if (_model == null)
			{
				if (IsDisposed)
					throw new ObjectDisposedException(Name ?? GetType().Name);
				throw new InvalidOperationException(
					$"{nameof(Initialize)} must be called to set the model first.");
			}

			// Do not use SafeInvoke here because we want this to work even if called before the
			// handle is created.
			if (Grid.InvokeRequired)
				Grid.Invoke(new Action(() => SetColumnAutoSizeMode_Internal(col, autoSizeMode)));
			else
				SetColumnAutoSizeMode_Internal(col, autoSizeMode);
		}

		/// ------------------------------------------------------------------------------------
		private void SetColumnAutoSizeMode_Internal(StandardColumns col, DataGridViewAutoSizeColumnMode autoSizeMode)
		{
			var column = col switch
			{
				StandardColumns.Name => Grid.Columns[kNameColName],
				StandardColumns.Role => Grid.Columns[kRoleColName],
				StandardColumns.Comments => Grid.Columns[kCommentsColName],
				StandardColumns.Date => Grid.Columns[kDateColName],
				_ => throw new ArgumentOutOfRangeException(nameof(col), col, null)
			};

			if (column != null)
				column.AutoSizeMode = autoSizeMode;
		}

		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		public bool InEditMode
		{
			get
			{
				if (InvokeRequired)
					return (bool)_grid.Invoke(new Func<bool>(() => _grid.IsCurrentRowDirty));
				return _grid.IsCurrentRowDirty;
			}
		}

		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		public bool InNewContributionRow => _grid.CurrentCellAddress.Y == _grid.NewRowIndex;

		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		public Contribution GetCurrentContribution()
		{
			if (_grid.CurrentCellAddress.Y < 0 || _grid.CurrentCellAddress.Y >= _grid.RowCount)
			{
				// No valid row selected
				return null;
			}
			return GetContributionFromRow(_grid.CurrentCellAddress.Y);
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (!DesignMode)
				_model.ContributorsGridSettings = GridSettings.Create(_grid);

			base.OnHandleDestroyed(e);
		}

		private void HandleNewContributionListAvailable(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				// Since BeginInvoke ensures the call is executed in the UI message loop (at a
				// later time), it might avoid conflicts with UI state updates still in progress.
				_grid.BeginInvoke(new Action(() => HandleNewContributionListAvailable(sender, e)));
				return;
			}

			Guard.AgainstNull(_model.Contributions, nameof(_model.Contributions));

			try
			{
				// Ensure any pending edits are committed or canceled before modifying rows
				if (_grid.IsCurrentCellInEditMode)
					_grid.EndEdit();
				if (_grid.CurrentCell != null && _grid.CurrentRow?.IsNewRow == false)
					_grid.CommitEdit(DataGridViewDataErrorContexts.Commit);

				_grid.RowValidated -= HandleGridRowValidated;
				_grid.RowsRemoved -= HandleGridRowsRemoved;

				_grid.SuspendLayout(); // Improve performance when modifying multiple rows
				try
				{
					_grid.Rows.Clear();

					foreach (var contribution in _model.Contributions)
						_grid.Rows.Add(contribution.ContributorName, contribution.Role.Name, contribution.Date, contribution.Comments);

					if (_grid.Rows.Count > 0)
						_grid.CurrentCell = _grid[0, 0];

					_grid.IsDirty = false;
				}
				finally
				{
					_grid.ResumeLayout(); // Re-enable layout updates

					_grid.RowValidated += HandleGridRowValidated;
					_grid.RowsRemoved += HandleGridRowsRemoved;
				}
			}
			catch (InvalidOperationException ex)
			{
				Logger.WriteError("Error updating contributions list", ex);
				try
				{
					var msg = LocalizationManager.GetString(
						"ContributorsEditorGrid.ErrorUpdatingListMsg", "Error updating list");
					_grid.DrawMessageInCenterOfGrid(Graphics.FromHwnd(_grid.Handle), msg, 0);
				}
				catch (Exception exception)
				{
					Logger.WriteError(exception);
				}
				// REVIEW: Then what? Retry every second until this succeeds???
				// I'm hopeful that this will never happen now that we're using BeginInvoke.
			}
		}

		/// ------------------------------------------------------------------------------------
		private void HandleGridMouseClick(object sender, MouseEventArgs e)
		{
			var hi = _grid.HitTest(e.X, e.Y);

			// Did the user click on a row heading?
			if (e.Button != MouseButtons.Left || hi.ColumnIndex >= 0 ||
				hi.RowIndex < 0 || hi.RowIndex >= _grid.RowCount)
			{
				return;
			}

			void SelectFirstCellInClickedRow() => _grid.CurrentCell = _grid[0, hi.RowIndex];

			// At this point we know the user clicked on a row heading. Now we
			// need to make sure the row they're leaving is in a valid state.
			if (ValidatingContributor != null && _grid.CurrentCellAddress.Y >= 0 &&
				_grid.CurrentCellAddress.Y < _grid.RowCount - 1)
			{
				if (_grid.CurrentCellAddress.Y == _model.Contributions.Count)
					return;

				if (_grid.IsDirty)
				{
					try
					{
						// This actually forces the commit/validation and will fail if the
						// current edit has the contribution in a bogus state.
						SelectFirstCellInClickedRow();
						return;
					}
					catch
					{
						SystemSounds.Beep.Play();
						return;
					}
				}

				var contribution = _model.Contributions.ElementAt(_grid.CurrentCellAddress.Y);
				if (!GetIsValidContribution(contribution))
				{
					SystemSounds.Beep.Play();
					return;
				}
			}

			SelectFirstCellInClickedRow();
		}

		/// ------------------------------------------------------------------------------------
		private bool GetIsValidContribution(Contribution contribution)
		{
			if (ValidatingContributor == null)
				return true;
			var args = new CancelEventArgs(false);
			ValidatingContributor(this, contribution, args);
			return !args.Cancel;
		}

		/// ------------------------------------------------------------------------------------
		private void HandleGridLeave(object sender, EventArgs e)
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

			if (ValidatingContributor == null)
				return;

			var contribution = GetContributionFromRow(e.RowIndex);

			var kvp = ValidatingContributor(this, contribution, e);

			if (!IsNullOrEmpty(kvp.Key))
			{
				_msgWindow ??= new FadingMessageWindow();

				var dataGridViewColumn = _grid.Columns[kvp.Key];
				if (dataGridViewColumn != null)
				{
					int col = dataGridViewColumn.Index;
					var rc = _grid.GetCellDisplayRectangle(col, e.RowIndex, true);
					var pt = new Point(rc.X + (rc.Width / 2), rc.Y + 4);
					this.SafeInvoke(() =>
						{
							_msgWindow.Show(kvp.Value, _grid.PointToScreen(pt));
						}, "Showing fading message with contributor field validation error",
						ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed, true);

					// Invoking here because of "reentrant call to the SetCurrentCellAddressCore" exception.
					// Setting the CurrentCell can trigger validation again.
					BeginInvoke((Action)(() =>_grid.CurrentCell = _grid[col, e.RowIndex]));
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		private void HandleGridRowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			SaveContributions();
		}

		/// ------------------------------------------------------------------------------------
		private void HandleGridRowValidated(object sender, DataGridViewCellEventArgs e)
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
				r.Index != _grid.NewRowIndex).Select(row =>
					GetContributionFromRow(row.Index)).Where(c => c!= null && GetIsValidContribution(c));
		}

		/// ------------------------------------------------------------------------------------
		private Contribution GetContributionFromRow(int rowIndex)
		{
			var row = _grid.Rows[rowIndex];

			var contribution = new Contribution
			{
				ContributorName = row.Cells[kNameColName].Value as string,
				Role = _model.OlacRoles.FirstOrDefault(o => o.Name == row.Cells[kRoleColName].Value as string),
				Comments = row.Cells[kCommentsColName].Value as string
			};

			var dateVal = row.Cells[kDateColName].Value;
			if (dateVal != null)
				contribution.Date = (DateTime)dateVal;

			return contribution;
		}

		/// ------------------------------------------------------------------------------------
		protected void HandleEditingControlShowing(object sender,
			DataGridViewEditingControlShowingEventArgs e)
		{
			if (_grid.CurrentCellAddress.X == 0)
			{
				var txtBox = e.Control as TextBox;
				_grid.Tag = txtBox;
				_grid.CellEndEdit += HandleGridCellEndEdit;
				if (txtBox == null) return;
				txtBox.KeyPress += HandleCellEditBoxKeyPress;
				txtBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
				txtBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
				txtBox.AutoCompleteCustomSource = _model.GetAutoCompleteNames();
			}
			else if (_grid.CurrentCellAddress.X == 1)
			{
				var cboBox = e.Control as ComboBox;
				_grid.Tag = cboBox;
				if (cboBox != null) cboBox.SelectedIndexChanged += HandleRoleValueChanged;
				_grid.CellEndEdit += HandleGridCellEndEdit;
			}
		}

		/// ------------------------------------------------------------------------------------
		private void HandleRoleValueChanged(object sender, EventArgs e)
		{
			_msgWindow?.Close();
		}

		/// ------------------------------------------------------------------------------------
		private void HandleGridCellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			var ctrl = _grid.Tag as Control;

			var txtBox = ctrl as TextBox;
			// SP-793: Text should match case of autocomplete list
			if (e.ColumnIndex == 0 && txtBox != null)
			{
				// is the current text an exact match for the autocomplete list?
				var list = txtBox.AutoCompleteCustomSource.Cast<object>().ToList();
				var found = list.FirstOrDefault(item =>
					string.Equals(item.ToString(), txtBox.Text, StringComparison.CurrentCulture));

				if (found == null)
				{
					// is the current text a match except for case for the autocomplete list?
					found = list.FirstOrDefault(item => string.Equals(item.ToString(),
						txtBox.Text, StringComparison.CurrentCultureIgnoreCase));
					if (found != null)
					{
						txtBox.Text = found.ToString();
						_grid.CurrentCell.Value = txtBox.Text;
					}
				}
			}

			if (txtBox != null)
				txtBox.KeyPress -= HandleCellEditBoxKeyPress;
			else if (ctrl is ComboBox box)
				box.SelectedIndexChanged -= HandleRoleValueChanged;

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

			_msgWindow?.Close();

			_grid.Rows.RemoveAt(rowIndex);
			_grid.CurrentCell = _grid[0, _grid.CurrentCellAddress.Y];
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the caption text on the given column's header cell. By default, the header cells
		/// have English captions.
		/// </summary>
		/// <param name="columnIndex">The index of the column whose header caption is to be set
		/// </param>
		/// <param name="headerText">The (localized) caption text</param>
		/// <remarks>SP-874: Allows for the direct localization of column headers</remarks>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="columnIndex" />
		/// is less than 0 or greater than the number of columns in the control minus 1.
		/// </exception>
		/// <exception cref="ObjectDisposedException">Method called after this object has already
		/// been disposed.</exception>
		/// <exception cref="InvalidOperationException">Method called when the model has not been
		/// set (using the <see cref="Initialize"/> method).</exception>
		/// ------------------------------------------------------------------------------------
		public void SetColumnHeaderText(int columnIndex, string headerText)
		{
			if (_model == null)
			{
				if (IsDisposed)
					throw new ObjectDisposedException(Name ?? GetType().Name);

				throw new InvalidOperationException(
					$"{nameof(Initialize)} must be called to set the model first.");
			}

			if (InvokeRequired)
				BeginInvoke(new Action(() => { _grid.Columns[columnIndex].HeaderText = headerText; }));
			else
				_grid.Columns[columnIndex].HeaderText = headerText;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the <see cref="L10NSharpExtender"/> that can be used to localize the column
		/// headers.
		/// </summary>
		/// <remarks>
		/// SP-874: Allows for localization of column headers via a <see cref="L10NSharpExtender"/>
		/// </remarks>
		/// ------------------------------------------------------------------------------------
		[CLSCompliant (false)]
		[PublicAPI]
		public void SetLocalizationExtender(L10NSharpExtender extender)
		{
			extender.SetLocalizingId(_grid, "ContributorsEditorGrid");
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>Exposes the underlying <see cref="BetterGrid"/> control</summary>
		/// <remarks>
		/// We need to be able to adjust the visual properties to match the hosting program.
		/// If used in code that could run on a thread other than the main UI thread, caller is
		/// responsible for invoking on UI thread if needed.
		/// </remarks>
		/// ------------------------------------------------------------------------------------
		public BetterGrid Grid => _grid;
	}
}
