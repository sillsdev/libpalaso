using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Palaso
{
	public partial class WSListDialog : Form
	{
		public WSListDialog()
		{
			InitializeComponent();
			MinimumSize = new Size(ClientSize.Width, 200);
			MaximumSize = new Size(ClientSize.Width, 2000);
		   // Size = new Size(ClientSize.Width, ClientSize.Height);

			AddItem();
			AddItem().Selected=true;
			AddItem();
			AddItem();
			_writingSystemList.LayoutRows();
		}
		private WSListItem AddItem()
		{
			 WSListItem item = new WSListItem();
			this._writingSystemList.AddControlToBottom(item);
			item.Selecting += new EventHandler(item_Selecting);
			return item;
		}


		void item_Selecting(object sender, EventArgs e)
		{
			foreach (WSListItem item in _writingSystemList.Items)
			{
				if (item!= sender && item.Selected)
				{
					item.Selected = false;
					break;
				}
			}
			_writingSystemList.LayoutRows();
		}

		private void controlListBox1_Load(object sender, EventArgs e)
		{
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void WSListDialog_Resize(object sender, EventArgs e)
		{
		  //  this.Width = 300;
		}

		private void WSListDialog_ResizeBegin(object sender, EventArgs e)
		{

		}
	}
}