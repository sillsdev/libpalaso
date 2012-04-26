using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Palaso.UI.WindowsForms.Widgets.BetterGrid
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Object for persisting and loading settings for a grid in settings files (i.e. files
	/// created/managed using SettingsProvider).
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[XmlType("gridSettings")]
	public class GridSettings
	{
		/// ------------------------------------------------------------------------------------
		[XmlElement("sortedColumn")]
		public string SortedColumn { get; set; }
		/// ------------------------------------------------------------------------------------
		[XmlElement("sortDirection")]
		public string SortDirection { get; set; }
		/// ------------------------------------------------------------------------------------
		[XmlElement("columnHeaderHeight")]
		public int ColumnHeaderHeight { get; set; }
		/// ------------------------------------------------------------------------------------
		[XmlElement("dpi")]
		public float DPI { get; set; }
		/// ------------------------------------------------------------------------------------
		[XmlArray("columns"), XmlArrayItem("col")]
		public GridColumnSettings[] Columns { get; set; }

		private readonly float m_currDpi;

		/// ------------------------------------------------------------------------------------
		public GridSettings()
		{
			ColumnHeaderHeight = -1;
			Columns = new GridColumnSettings[] { };
			using (Form frm = new Form())
			using (Graphics g = frm.CreateGraphics())
				m_currDpi = DPI = g.DpiX;
		}

		/// ------------------------------------------------------------------------------------
		public SortOrder SortOrder
		{
			get
			{
				if (SortDirection == SortOrder.Ascending.ToString())
					return SortOrder.Ascending;

				if (SortDirection == SortOrder.Descending.ToString())
					return SortOrder.Descending;

				return SortOrder.None;
			}
		}

		/// ------------------------------------------------------------------------------------
		public static GridSettings Create(DataGridView grid)
		{
			var gridSettings = new GridSettings();

			var sortCol = grid.Columns.Cast<DataGridViewColumn>()
				.FirstOrDefault(c => c.HeaderCell.SortGlyphDirection != SortOrder.None);

			if (sortCol != null)
			{
				gridSettings.SortedColumn = sortCol.Name;
				gridSettings.SortDirection = sortCol.HeaderCell.SortGlyphDirection.ToString();
			}

			gridSettings.ColumnHeaderHeight = grid.ColumnHeadersHeight;

			gridSettings.Columns = (from c in grid.Columns.Cast<DataGridViewColumn>()
									select new GridColumnSettings { Id = c.Name,
										Width = c.Width, Visible = c.Visible,
										DisplayIndex = c.DisplayIndex }).ToArray();

			return gridSettings;
		}

		/// ------------------------------------------------------------------------------------
		public void InitializeGrid(DataGridView grid)
		{
			foreach (var col in Columns)
			{
				if (!grid.Columns.Contains(col.Id))
					continue;

				grid.Columns[col.Id].Visible = col.Visible;

				if (col.Width >= 0)
					grid.Columns[col.Id].Width = col.Width;

				if (col.DisplayIndex < 0)
					grid.Columns[col.Id].DisplayIndex = 0;
				else if (col.DisplayIndex >= grid.ColumnCount)
					grid.Columns[col.Id].DisplayIndex = grid.ColumnCount - 1;
				else
					grid.Columns[col.Id].DisplayIndex = col.DisplayIndex;
			}

			// If the column header height or the former dpi settings are different,
			// then auto. calculate the height of the column headings.
			if (ColumnHeaderHeight <= 0 || DPI != m_currDpi)
				grid.AutoResizeColumnHeadersHeight();
			else
				grid.ColumnHeadersHeight = ColumnHeaderHeight;
		}
	}

	/// ----------------------------------------------------------------------------------------
	public class GridColumnSettings
	{
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("id")]
		public string Id { get; set; }
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("visible")]
		public bool Visible { get; set; }
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("width")]
		public int Width { get; set; }
		/// ------------------------------------------------------------------------------------
		[XmlAttribute("displayIndex")]
		public int DisplayIndex { get; set; }

		/// ------------------------------------------------------------------------------------
		internal GridColumnSettings()
		{
			Visible = true;
			Width = -1;
			DisplayIndex = -1;
		}
	}
}
