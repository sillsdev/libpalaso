using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SIL.Archiving.IMDI.Lists
{
	/// <summary>
	/// Generic class to handle items in the IMDI lists
	/// </summary>
	public class IMDIListItem
	{
		/// <summary>
		/// Displayed to the user
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Used in the metadata files
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="text">Displayed to the user</param>
		/// <param name="value">Used in the metadata files</param>
		public IMDIListItem(string text, string value)
		{
			Text = text;
			Value = value;
		}

		public override string ToString() { return Text; }
	}

	/// <summary>
	/// A list of IMDI list items, constructed using the imdi:Entry Nodes from the XML file.
	/// <example>
	/// Example 1:
	/// ----------
	/// var lc = IMDI_Schema.ListConstructor.GetList(ListType.MPILanguages);
	/// comboBox1.Items.AddRange(lc.ToArray());
	/// </example>
	/// <example>
	/// Example 2:
	/// ----------
	/// var lc = IMDI_Schema.ListConstructor.GetList(ListType.MPILanguages);
	/// comboBox1.DataSource = lc;
	/// comboBox1.DisplayMember = "Text";
	/// comboBox1.ValueMember = "Value";
	/// </example>
	/// </summary>
	public class IMDIItemList : List<IMDIListItem>
	{
		/// <summary>
		/// Constructor for derived classes to use
		/// </summary>
		protected IMDIItemList()
		{
			// additional constructor code can go here
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="nodes">A list of the imdi:Entry nodes from the XML file</param>
		public IMDIItemList(XmlNodeList nodes)
		{
			PopulateList(nodes);
		}

		/// ---------------------------------------------------------------------------------------
		protected void PopulateList(XmlNodeList nodes)
		{
			foreach (XmlNode node in nodes)
			{
				if (node.Attributes == null) continue;

				var txt = node.Attributes["Value"].Value; // the "Value" attribute contains the text to show to the user
				var val = node.InnerText;                 // if InnerText is set, it contains the value for the meta-data file

				// most of the time InnerText is empty - use the "Value" attribute for both if it is empty
				AddItem(new IMDIListItem(txt, string.IsNullOrEmpty(val) ? txt : val));
			}
		}

		/// <summary>
		/// Override in inherited classes
		/// </summary>
		/// <param name="item"></param>
		public virtual void AddItem(IMDIListItem item)
		{
			Add(item);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The first item found with the selected Value, or null if not found</returns>
		public IMDIListItem FindByValue(string value)
		{
			return this.FirstOrDefault(i => i.Value == value);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="text"></param>
		/// <returns>The first item found with the selected Text, or null if not found</returns>
		public IMDIListItem FindByText(string text)
		{
			var itm = this.FirstOrDefault(i => i.Text == text);
			return itm;
		}
	}
}
