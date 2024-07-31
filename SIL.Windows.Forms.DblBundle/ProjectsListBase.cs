using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using L10NSharp;
using SIL.DblBundle;
using SIL.DblBundle.Text;
using SIL.Windows.Forms.PortableSettingsProvider;

namespace SIL.Windows.Forms.DblBundle
{
//	[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ProjectsListBase<DblMetadataBase<DblMetadataLanguage>, DblMetadataLanguage>, UserControl>))]
	public abstract partial class ProjectsListBase<TM, TL> : UserControl
		where TM : DblMetadataBase<TL>
		where TL : DblMetadataLanguage, new()
	{
		/// <summary>Event when selected project is changed</summary>
		public event EventHandler SelectedProjectChanged;
		/// <summary>Event after list of project is loaded</summary>
		public event EventHandler ListLoaded;
		/// <summary>Event when the column width changes</summary>
		public event EventHandler<DataGridViewColumnEventArgs> ColumnWidthChanged;
		public event EventHandler<DataGridViewColumnEventArgs> ColumnDisplayIndexChanged;
		public event EventHandler ProjectListSorted;

		private string m_selectedProject;
		private string m_filterIcuLocale;
		private string m_filterBundleId;
		private bool m_includeHiddenProjects;
		private bool m_hiddenProjectsExist;
		private bool m_gridInitializedFromSettings;
		private readonly List<string> m_readOnlyProjects = new List<string>();
		private bool m_projectSelected;  // The value of this boolean is only reliable if m_sorting is true.
		private bool m_sorting;

		protected ProjectsListBase()
		{
			InitializeComponent();
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string SelectedProject
		{
			get => m_selectedProject;
			set
			{
				m_selectedProject = value;
				if (m_selectedProject == null)
					return;
				for (int iRow = 0; iRow < m_list.RowCount; iRow++)
				{
					if (m_selectedProject.Equals(m_list.Rows[iRow].Cells[colProjectPathOrId.Index].Value))
					{
						m_list.Rows[iRow].Selected = true;
						break;
					}
				}
			}
		}

		[PublicAPI]
		public void AddReadOnlyProject(string projectFilePath)
		{
			m_readOnlyProjects.Add(projectFilePath);
		}

		public virtual bool IncludeHiddenProjects
		{
			get => m_includeHiddenProjects;
			set
			{
				m_includeHiddenProjects = value;
				ReloadExistingProjects();
			}
		}

		protected virtual DataGridViewColumn InactiveColumn => null;

		[PublicAPI]
		protected void OverrideColumnHeaderText(int columnIndex, string displayName)
		{
			m_list.Columns[columnIndex].HeaderText = displayName;
		}

		[PublicAPI]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool HiddenProjectsExist => m_hiddenProjectsExist;

		[PublicAPI]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GridSettings GridSettings
		{
			get => GridSettings.Create(m_list);
			set
			{
				if (value == null)
					return;

				// InitializeGrid might cause the list to be sorted, but we don't want it to result in
				// selecting a row if none was selected initially.
				PrepareToSort();
				value.InitializeGrid(m_list);
				m_sorting = false; // Clear this in case InitializeGrid didn't result in a call to Sort.
				m_gridInitializedFromSettings = true;
			}
		}

		private void PrepareToSort()
		{
			Debug.Assert(!m_sorting, "PrepareToSort should only be called once before Sort is called. If Sort is not called, the m_sorting flag needs to be cleared. If " +
				"a sort is in progress, m_list.SelectedRows is not reliable because Sort will select a row if none is selected.");
			m_sorting = true;
			m_projectSelected = m_list.SelectedRows.Count > 0;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (DesignMode)
				return;

			LoadExistingProjects();

			// The very first time, we want the columns to autosize, then use the user settings
			if (!m_gridInitializedFromSettings)
			{
				foreach (DataGridViewColumn col in m_list.Columns)
					col.AutoSizeMode = col == FillColumn ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.AllCells;

				for (int i = 0; i < m_list.Columns.Count; i++)
				{
					// don't change the AutoSizeMode of the fill column
					if (m_list.Columns[i] == FillColumn)
						continue;
					int colWidth = m_list.Columns[i].Width;
					m_list.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
					m_list.Columns[i].Width = colWidth;
				}
			}
		}

		protected virtual DataGridViewColumn FillColumn => colRecordingProjectName;

		/// <summary>
		/// Enumeration of folders where existing project files are stored
		/// </summary>
		protected abstract IEnumerable<string> AllProjectFolders { get; }

		protected abstract string ProjectFileExtension { get; }

		protected virtual IEnumerable<Tuple<string, IProjectInfo>> Projects
		{
			get
			{
				foreach (var recordingProjectFolder in AllProjectFolders)
				{
					var path = Directory.GetFiles(recordingProjectFolder, "*" + ProjectFileExtension)
						.OrderBy(filename => filename).FirstOrDefault();
					if (path != null)
					{
						var projectInfo = GetProjectInfo(path);
						if (projectInfo != null)
							yield return new Tuple<string, IProjectInfo>(path, projectInfo);
					}
				}
			}
		}

		protected virtual IProjectInfo GetProjectInfo(string path)
		{
			var metadata = DblMetadataBase<TL>.Load<TM>(path, out var exception);
			return exception == null ? metadata : null;
		}

		protected virtual string GetRecordingProjectName(Tuple<string, IProjectInfo> project)
		{
			return project.Item2.Name;
		}

		protected virtual IEnumerable<object> GetAdditionalRowData(IProjectInfo projectInfo)
		{
			return new object[0];
		}

		protected virtual bool IsInactive(IProjectInfo project)
		{
			return false;
		}

		public void ReloadExistingProjects()
		{
			if (IsHandleCreated)
				LoadExistingProjects();
		}

		private void LoadExistingProjects()
		{
			m_list.SelectionChanged -= HandleSelectionChanged;
			m_list.CellValueChanged -= HandleCellValueChanged;
			m_list.CellValidating -= HandleCellValidating;

			m_list.Rows.Clear();
			m_hiddenProjectsExist = false;
			bool selectedProjectWasSelected = false;
			foreach (var project in Projects)
			{
				if (IsInactive(project.Item2))
				{
					m_hiddenProjectsExist = true;
					if (!IncludeHiddenProjects)
						continue;
				}

				if ((m_filterIcuLocale != null && m_filterIcuLocale != project.Item2.Language.Iso) ||
					(m_filterBundleId != null && m_filterBundleId != project.Item2.Id))
					continue;

				List<object> rowData = new List<object>
				{
					project.Item1,
					project.Item2.Language,
					GetRecordingProjectName(project)
				};
				rowData.AddRange(GetAdditionalRowData(project.Item2));
				int iRow = m_list.Rows.Add(rowData.ToArray());

				if (SelectedProject == project.Item1)
				{
					m_list.Rows[iRow].Selected = true;
					selectedProjectWasSelected = true;
				}
			}

			PrepareToSort();

			m_list.Sort(m_list.SortedColumn ?? colLanguage,
				m_list.SortOrder == SortOrder.Descending ? ListSortDirection.Descending : ListSortDirection.Ascending);

			if (SelectedProject == null || !selectedProjectWasSelected)
			{
				m_list.ClearSelection();
				if (SelectedProject != null)
				{
					SelectedProject = null;
					SelectedProjectChanged?.Invoke(this, EventArgs.Empty);
				}
			}
			else
				m_list.FirstDisplayedScrollingRowIndex = m_list.SelectedRows[0].Index;

			m_list.SelectionChanged += HandleSelectionChanged;
			m_list.CellValueChanged += HandleCellValueChanged;
			m_list.CellValidating += HandleCellValidating;

			ListLoaded?.Invoke(this, EventArgs.Empty);
		}

		public void SetFilter(string icuLocale, string bundleId)
		{
			m_filterIcuLocale = icuLocale;
			m_filterBundleId = bundleId;
			ReloadExistingProjects();
		}

		private void HandleSelectionChanged(object sender, EventArgs e)
		{
			// If the selection changed as a result of sorting (i.e., a row got selected because none was
			// selected previously), HandleProjectListSorted will clear the selection (which will result
			// in this handler getting called a second time). The net effect should be no change, so don't
			// do anything.
			if (m_sorting)
				return;

			if (DesignMode || m_list.SelectedRows.Count < 1 || m_list.SelectedRows[0].Index < 0)
				SelectedProject = null;
			else
				SelectedProject = m_list.SelectedRows[0].Cells[colProjectPathOrId.Index].Value as string;

			SelectedProjectChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// See https://stackoverflow.com/questions/1407195/prevent-datagridview-selecting-a-row-when-sorted-if-none-was-previously-selected/1407261#1407261
		/// </summary>
		private void HandleProjectListSorted(object sender, EventArgs e)
		{
			if (m_sorting)
			{
				if (!m_projectSelected)
					m_list.ClearSelection();
				m_sorting = false;

				if (ProjectListSorted != null)
					ProjectListSorted(this, e);
			}
			else
				Debug.Fail("PrepareToSort should have been called before sorting.");
		}

		private void HandleCellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			// ignore double-click of header cells
			if (e.RowIndex < 0)
			{
				m_sorting = false;
				return;
			}

			OnDoubleClick(EventArgs.Empty);
		}

		private void HandleListCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
		{
			// clicking on the header row
			// also make sure we're not already set for sorting -- can happen if header is double-clicked
			if (e.Button == MouseButtons.Left && e.RowIndex == -1 && !m_sorting)
				PrepareToSort();
		}

		protected virtual void SetHiddenFlag(bool inactive)
		{
			// no-op
		}

		private void HandleCellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex == -1)
				return; // Heading Text changed
			if (InactiveColumn == null || e.ColumnIndex != InactiveColumn.Index)
				throw new InvalidOperationException("Unexpected change in read-only column!");

			var row = m_list.Rows[e.RowIndex];
			SetHiddenFlag((bool)row.Cells[e.ColumnIndex].Value);
		}

		private void HandleCellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (InactiveColumn == null || e.ColumnIndex != InactiveColumn.Index)
				return;

			if ((bool)e.FormattedValue && m_readOnlyProjects.Contains(m_list.Rows[e.RowIndex].Cells[colProjectPathOrId.Index].Value))
			{
				string title = LocalizationManager.GetString("Project.CannotRemoveCaption", "Cannot Remove from List");
				string msg = LocalizationManager.GetString("Project.CannotRemove", "Cannot remove the selected project because it is currently open");
				MessageBox.Show(msg, title);
				m_list.RefreshEdit();
				e.Cancel = true;
			}
		}

		private void HandleColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
		{
			ColumnWidthChanged?.Invoke(this, e);
		}

		private void HandleColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs e)
		{
			ColumnDisplayIndexChanged?.Invoke(this, e);
		}
	}
}
