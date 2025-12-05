using System;
using System.Diagnostics;
using System.Windows.Forms;
using L10NSharp;
using SIL.Core.ClearShare;
using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.ClearShare.WinFormsUI
{
	/// <summary>
	/// This control is just for displaying metadata in a compact way, not editing it.
	/// </summary>
	public partial class MetadataDisplayControl : UserControl
	{
		public MetadataDisplayControl()
		{
			InitializeComponent();
		}

		public void SetMetadata(Metadata metaData)
		{
			_table.SuspendLayout();
			_table.Controls.Clear();
			_table.RowCount = 0;
			_table.RowStyles.Clear();
			if (!string.IsNullOrEmpty(metaData.Creator))
			{
				AddRow(string.Format("Creator: {0}".Localize("MetadataDisplay.Creator"), metaData.Creator));
			}
			if (!string.IsNullOrEmpty(metaData.CollectionName))
			{
				if (!string.IsNullOrEmpty(metaData.CollectionUri))
				{
					AddHyperLink(metaData.CollectionName, metaData.CollectionUri,2);
				}
				else
				{
					AddRow(metaData.CollectionName);
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(metaData.CollectionUri))
				{
					AddHyperLink(metaData.CollectionUri, metaData.CollectionUri,2);
				}
			}

			if (!string.IsNullOrEmpty(metaData.CopyrightNotice))
			{
				AddRow(metaData.ShortCopyrightNotice);
			}
			if (!string.IsNullOrEmpty(metaData.AttributionUrl))
			{
				AddHyperLink(metaData.AttributionUrl.Replace("http://",""), metaData.AttributionUrl,2);
			}
			if (metaData.License!=null)
			{
				if (metaData.License is NullLicense)
				{
					if (metaData.IsLicenseNotSet)
						AddRow("No license specified".Localize("MetadataDisplay.NoLicense"));
					else
						AddRow("All rights reserved".Localize("MetadataDisplay.AllRightsReserved"));
				}
				else
				{
					System.Drawing.Image licenseImage = null;
					// Licenses constructed from SIL.Core.Clearshare will not have images,
					// and will not be of type ILicenseWithImage.
					if (metaData.License is ILicenseWithImage)
						licenseImage = ((ILicenseWithImage)metaData.License).GetImage();
					PictureBox pictureBox = null;
					if (licenseImage != null)
					{
						pictureBox = new PictureBox();
						pictureBox.Size = new System.Drawing.Size(124, 40);
						pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
						pictureBox.Image = licenseImage;
						_table.Controls.Add(pictureBox);
					}
					if (!string.IsNullOrEmpty(metaData.License.RightsStatement))
					{
						AddRow(metaData.License.RightsStatement);
					}
					if (!string.IsNullOrEmpty(metaData.License.Url))
					{
						//AddHyperLink(LocalizationManager.GetString("License Info", metaData.License.Url, 1);
						AddHyperLink("License Info".Localize("MetadataDisplay.LicenseInfo"), metaData.License.Url, 1);
					}
					else if (pictureBox!=null)
					{
						_table.SetColumnSpan(pictureBox, 2);
					}
					_table.RowCount++;
				}
			}
			_table.ResumeLayout();
		}
		private void AddHyperLink(string label, string url, int columns)
		{
			using (var g = this.CreateGraphics())
			{
				var w = this.Width - 10;
				var linkLabel = new LinkLabel() { Padding = Padding.Empty, Text = label };

				// Have link label automatically determine its height, capping width at w.
				linkLabel.MaximumSize = new System.Drawing.Size(w, 0);
				linkLabel.AutoSize = true;

				linkLabel.Click += new EventHandler((x, y) => SIL.Program.Process.SafeStart(url));
				_table.Controls.Add(linkLabel);
				_table.SetColumnSpan(linkLabel, columns);
			   _table.RowCount++;
			}
		}

		public void AddRow(string label, Control control)
		{
			_table.Controls.Add(new Label() { Text = label });
			_table.Controls.Add(control);
			_table.RowCount++;
		}
		public void AddRow(string label)
		{
			var control = new BetterLabel() {  Width=this.Width, Text = label};//BetterLabel will automatically determine its height
			_table.Controls.Add(control);
			_table.SetColumnSpan(control,2);
			_table.RowCount++;
		}

//        public void LayoutRows()
//        {
//            foreach (Control c in _table.Controls)
//            {
//                c.TabIndex = _table.Controls.GetChildIndex(c);
//            }
//
//            float h = 0;
//            _table.RowStyles.Clear();
//            for (int r = 0; r < _table.RowCount; r++)
//            {
//                Control c = _table.GetControlFromPosition(0, r);
//                if (c != null)// null happens at design time
//                {
//                    RowStyle style = new RowStyle(SizeType.Absolute, c.Height + _table.Margin.Vertical);
//                    _table.RowStyles.Add(style);
//                    h += style.Height;
//                }
//            }
//            _table.Height = (int)h;
//            //_table.Invalidate();
//            _table.Refresh();
//        }

		private void UpdateDisplay()
		{

		}

		private void MetadataDisplayControl_Load(object sender, EventArgs e)
		{
			UpdateDisplay();
		}
	}
}
