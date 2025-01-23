// Based on code copied with permission from The Code Project:
// http://www.codeproject.com/Articles/14570/A-Windows-Explorer-in-a-user-control

#region Original Copyright and EULA notices

/* *************************************************

Original Programmer: Rajesh Lal(connectrajesh@hotmail.com)
Date: 06/25/06
Company Info: www.irajesh.com
Explorer Tree
----------------------------------------------------------------
Original copyright (C) Rajesh Lal, 2006-2007
Email connectrajesh@hotmail.com

Author Quartz (Rajesh Lal - connectrajesh@hotmail.com)

The program is FREE for any personal or non-commercial usage. In the case of
modifying and/or redistributing the code it's obligate to retain
the original copyright notice.  It is not allowed to create your
own commercial products on the basis of the program or any parts
of it.

Please, contact me for purchase/ modification and redistribution with, remarks, ideas, etc.
also check the EULA.txt for User agreement
www.irajesh.com

----------------------------------------------------------------
Original End User License Agreement:

General Source Code - User Licence

The following agreement applies to a software product "FolderBrowserControl"

This is a legal agreement (hereafter "Agreement") between you, either an
individual or an entity, as the buyer of the source code (hereafter
"Recipient") and Rajesh Lal the author of the product. (hereafter "Author").

NON-DISCLOSURE AND LICENCE AGREEMENT FOR FolderBrowserControl

1. GRANT OF LICENCE.

Author grants Recipient a limited, non-exclusive, nontransferable, royalty-free
licence to use the Product and its source code.

Recipient may not sell, rent, transfer or disclose source code of the software
product or derivative work of the software product to the third party without
written permission from the Author.

Recipient may not use source code for development commercially competitive product
to Author's Work.

Recipient shall not use Author's name, logo or trademarks to market Recipient's
resulting software components or applications without express written consent
from the Author.

2. TERM OF AGREEMENT.

The term of this Agreement shall commence at the date Recipient purchases the
Product.

3. MAINTENANCE.

The Author is not obligated to provide maintenance or technical support to Recipient
for the Product licensed under this Agreement.

4. DISCLAIMER OF WARRANTIES.

To the maximum extent permitted by applicable law, the Product is provided
"as is" and without warranties of any kind, whether express or implied,
including but not limited to the implied warranties of merchantability or
fitness for a particular purpose. The entire risk arising out of the use or
installation of the Product, if any, remains with Recipient.

5. EXCLUSION OF INCIDENTAL, CONSEQUENTIAL AND CERTAIN OTHER DAMAGES.

To the maximum extent permitted by applicable law, in no event shall
Author be liable for any special, incidental, indirect, consequential or
punitive damages whatsoever arising out of or in any way related to the use of
or the inability to use the Product.

6. LIMITATION OF LIABILITY AND REMEDIES.

To the maximum extent permitted by applicable law, any liability of the Author will
be limited exclusively to a refund of the purchase price.


7. GOVERNING LAW.

This Agreement shall be construed and controlled by the laws of the
United States. Exclusive jurisdiction and venue for all matters relating to
this Agreement shall be in courts located in the United States. The Recipient
consents to such jurisdiction and venue.

8. ENTIRE AGREEMENT.

This Agreement constitutes the complete and exclusive agreement between the Author
and Recipient with respect to the subject matter hereof and supersedes all
prior oral or written understandings, communications or agreements not
specifically incorporated herein. This Agreement may not be modified except in
a writing duly signed by an authorised representative of the Author and Recipient.

Should you have any questions concerning this Agreement, please contact
the Author.

**************************************************/

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.FolderBrowserControl
{
	/// <summary>
	/// A form control for browsing folders on all disk drives and network nodes.
	/// </summary>
	[ToolboxBitmap(typeof (FolderBrowserControl), "tree.gif"), DefaultEvent("PathChanged")]
	public class FolderBrowserControl : UserControl
	{
		#region Control declarations

		private TreeView _folderTreeView; // tree represenation of all folders
		private TextBox _folderPathTextBox; // text box containing current folder path

		private ImageList _folderTreeViewImages; // collection of icons for folder treeview

		private Button _refreshTreeButton; // button to redraw folder tree
		private Button _goButton; // button to "enter" path in address bar
		private Button _homeButton; // button to navigate directly to folder of this .exe
		private Button _backButton; // button to navigate directly to previous folder
		private Button _nextButton; // button to navigate to next folder
		private Button _upButton; // button to navigate to current folder parent
		private Button _addShortcutButton; // button to add a shortcut to a frequently-used folder
		private Button _infoButton; // (unused) button to bring up info
		private GroupBox _toolBarGroupBox; // groupbox container for all the buttons listed above

		private ToolTip _toolTip; // magic container for all tooltips

		private ContextMenuStrip _shortcutContextMenu; // a context menu that works on user's shortcut folders
		private ToolStripMenuItem _shortCutRemovalMenuItem; // context menu item to remove a user's shortcut folder

		private IContainer components; // silly thing that .NET seems to like

		#endregion

		#region Boolean properties to control which sub-controls and special folders the user can see

		// Flag to indicate if the My Documents folder should be available:
		private bool _showMyDocuments = true;

		public bool ShowMyDocuments
		{
			get { return _showMyDocuments; }
			set
			{
				_showMyDocuments = value;
				Refresh();
			}
		}

		// Flag to indicate if the My Favorites folder should be available:
		private bool _showMyFavorites = true;

		public bool ShowMyFavorites
		{
			get { return _showMyFavorites; }
			set
			{
				_showMyFavorites = value;
				Refresh();
			}
		}

		// Flag to indicate if the My Network folder should be available:
		private bool _showMyNetwork = true;

		public bool ShowMyNetwork
		{
			get { return _showMyNetwork; }
			set
			{
				_showMyNetwork = value;
				Refresh();
			}
		}

		// Flag to indicate if drives A: through Z: should only appear if they are mapped drives:
		private bool _showOnlyMappedDrives;

		public bool ShowOnlyMappedDrives
		{
			get { return _showOnlyMappedDrives; }
			set
			{
				_showOnlyMappedDrives = value;
				Refresh();
			}
		}

		// Flag to indicate if the Address Bar should be visible:
		public bool ShowAddressbar { get; set; }

		// Flag to indicate if the Tool Bar buttons should be available:
		public bool ShowToolbar { get; set; }

		// Flag to indicate if the Go button should be available:
		public bool ShowGoButton { get; set; }

		#endregion

		#region General properties

		[
			Category("Appearance"),
			Description("Text of current folder path")
		]
		public string SelectedPath { get; set; }

		#endregion

		#region Custom event declarations

		// Define an event for when the user has selected a different folder path, to be used by consumer of this control:
		public delegate void PathChangedEventHandler(object sender, EventArgs e);
		private PathChangedEventHandler _pathChangedEvent = delegate { };
		public event PathChangedEventHandler PathChanged
		{
			add { _pathChangedEvent = (PathChangedEventHandler) Delegate.Combine(_pathChangedEvent, value); }
			remove { _pathChangedEvent = (PathChangedEventHandler) Delegate.Remove(_pathChangedEvent, value); }
		}

		// Define an event for when the selection history needs to be updated:
		private delegate void PathSelectionHistoryEventHandler(string path);
		private event PathSelectionHistoryEventHandler HistoryChangeEventHandler = delegate { };

		#endregion

		private TreeNode _treeNodeMyComputer; // special node in folder tree
		private TreeNode _treeNodeRootNode; // placeholder for root node in folder tree

		private List<string> _selectionHistory; // List of folders the user has selected
		private int _selectionHistoryIndex; // Position in history list when going back or forth.

		#region Dummy treeview-node name declarations

		private const string DummyMicrosoftWindowsNetworkName = "DummyMicrosoftWindowsNetworkName";
		private const string DummyMyComputerChildName = "DummyMyComputerChildName";
		private const string DummyEntireNetworkChildName = "DummyEntireNetworkChildName";

		#endregion

		#region Special folder name declarations

		private const string DesktopFolderName = "Desktop";
		private const string MicrosoftWindowsNetworkFolderName = "Microsoft Windows Network";
		private const string MyComputerFolderName = "My Computer";
		private const string MyDocumentsFolderName = "My Documents";
		private const string MyNetworkPlacesFolderName = "My Network Places";
		private const string EntireNetworkFolderName = "Entire Network";
		private const string MyFavoritesFolderName = "My Favorites";

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public FolderBrowserControl()
		{
			if (!Platform.IsWindows)
				throw new NotSupportedException("The FolderBrowserControl dialog is currently only supported on Windows");

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// Add in some event handlers that we want to be able to remove and re-add at will,
			// so we have had to stop the Visual Studio Designer from managing them:
			_folderPathTextBox.TextChanged += OnFolderPathTextChanged;
			_folderTreeView.AfterSelect += OnFolderTreeViewAfterSelect;

			ClearHistoryList();

			// The Go Button is largely redundant, since new functionality responds in real time to textbox changes:
			ShowGoButton = false;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FolderBrowserControl));
			this._refreshTreeButton = new System.Windows.Forms.Button();
			this._folderPathTextBox = new System.Windows.Forms.TextBox();
			this._goButton = new System.Windows.Forms.Button();
			this._folderTreeView = new System.Windows.Forms.TreeView();
			this._folderTreeViewImages = new System.Windows.Forms.ImageList(this.components);
			this._homeButton = new System.Windows.Forms.Button();
			this._backButton = new System.Windows.Forms.Button();
			this._nextButton = new System.Windows.Forms.Button();
			this._upButton = new System.Windows.Forms.Button();
			this._toolTip = new System.Windows.Forms.ToolTip(this.components);
			this._addShortcutButton = new System.Windows.Forms.Button();
			this._infoButton = new System.Windows.Forms.Button();
			this._shortcutContextMenu = new System.Windows.Forms.ContextMenuStrip();
			this._shortCutRemovalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._toolBarGroupBox = new System.Windows.Forms.GroupBox();
			this._toolBarGroupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// _refreshTreeButton
			//
			this._refreshTreeButton.BackColor = System.Drawing.Color.White;
			this._refreshTreeButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this._refreshTreeButton.FlatAppearance.BorderSize = 0;
			this._refreshTreeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._refreshTreeButton.ForeColor = System.Drawing.Color.Transparent;
			this._refreshTreeButton.Image = ((System.Drawing.Image)(resources.GetObject("_refreshTreeButton.Image")));
			this._refreshTreeButton.Location = new System.Drawing.Point(88, 0);
			this._refreshTreeButton.Margin = new System.Windows.Forms.Padding(0);
			this._refreshTreeButton.Name = "_refreshTreeButton";
			this._refreshTreeButton.Size = new System.Drawing.Size(20, 20);
			this._refreshTreeButton.TabIndex = 62;
			this._toolTip.SetToolTip(this._refreshTreeButton, "Refresh Explorer Tree");
			this._refreshTreeButton.UseVisualStyleBackColor = false;
			this._refreshTreeButton.Click += new System.EventHandler(this.OnRefreshButtonClick);
			//
			// _folderPathTextBox
			//
			this._folderPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._folderPathTextBox.Location = new System.Drawing.Point(0, 21);
			this._folderPathTextBox.Name = "_folderPathTextBox";
			this._folderPathTextBox.Size = new System.Drawing.Size(220, 20);
			this._folderPathTextBox.TabIndex = 1;
			this._toolTip.SetToolTip(this._folderPathTextBox, "Current directory");
			this._folderPathTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnTextBoxFolderPathKeyUp);
			//
			// _goButton
			//
			this._goButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._goButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this._goButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._goButton.ForeColor = System.Drawing.Color.White;
			this._goButton.Image = ((System.Drawing.Image)(resources.GetObject("_goButton.Image")));
			this._goButton.Location = new System.Drawing.Point(216, 21);
			this._goButton.Margin = new System.Windows.Forms.Padding(0);
			this._goButton.Name = "_goButton";
			this._goButton.Size = new System.Drawing.Size(20, 20);
			this._goButton.TabIndex = 60;
			this._toolTip.SetToolTip(this._goButton, "Go to the directory");
			this._goButton.Click += new System.EventHandler(this.OnGoButtonClick);
			//
			// _folderTreeView
			//
			this._folderTreeView.AllowDrop = true;
			this._folderTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._folderTreeView.BackColor = System.Drawing.Color.White;
			this._folderTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
			this._folderTreeView.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._folderTreeView.ImageIndex = 0;
			this._folderTreeView.ImageList = this._folderTreeViewImages;
			this._folderTreeView.Location = new System.Drawing.Point(0, 42);
			this._folderTreeView.Name = "_folderTreeView";
			this._folderTreeView.SelectedImageIndex = 2;
			this._folderTreeView.ShowLines = false;
			this._folderTreeView.ShowRootLines = false;
			this._folderTreeView.Size = new System.Drawing.Size(240, 294);
			this._folderTreeView.TabIndex = 59;
			this._folderTreeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.OnFolderTreeViewAfterExpand);
			this._folderTreeView.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.OnFolderTreeViewDrawNode);
			this._folderTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnFolderTreeViewBeforeSelect);
			this._folderTreeView.DoubleClick += new System.EventHandler(this.OnFolderTreeViewDoubleClick);
			this._folderTreeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnFolderTreeViewMouseUp);
			//
			// _folderTreeViewImages
			//
			this._folderTreeViewImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this._folderTreeViewImages.ImageSize = new System.Drawing.Size(16, 16);
			this._folderTreeViewImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this._folderTreeViewImages.TransparentColor = System.Drawing.Color.Transparent;
			//
			// _homeButton
			//
			this._homeButton.BackColor = System.Drawing.Color.White;
			this._homeButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this._homeButton.FlatAppearance.BorderSize = 0;
			this._homeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._homeButton.ForeColor = System.Drawing.Color.Transparent;
			this._homeButton.Image = ((System.Drawing.Image)(resources.GetObject("_homeButton.Image")));
			this._homeButton.Location = new System.Drawing.Point(110, 0);
			this._homeButton.Margin = new System.Windows.Forms.Padding(0);
			this._homeButton.Name = "_homeButton";
			this._homeButton.Size = new System.Drawing.Size(20, 20);
			this._homeButton.TabIndex = 63;
			this._toolTip.SetToolTip(this._homeButton, "Application Directory");
			this._homeButton.UseVisualStyleBackColor = false;
			this._homeButton.Click += new System.EventHandler(this.OnHomeButtonClick);
			//
			// _backButton
			//
			this._backButton.BackColor = System.Drawing.Color.White;
			this._backButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this._backButton.FlatAppearance.BorderSize = 0;
			this._backButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._backButton.ForeColor = System.Drawing.Color.Transparent;
			this._backButton.Image = ((System.Drawing.Image)(resources.GetObject("_backButton.Image")));
			this._backButton.Location = new System.Drawing.Point(22, 0);
			this._backButton.Margin = new System.Windows.Forms.Padding(0);
			this._backButton.Name = "_backButton";
			this._backButton.Size = new System.Drawing.Size(20, 20);
			this._backButton.TabIndex = 64;
			this._toolTip.SetToolTip(this._backButton, "Backward");
			this._backButton.UseVisualStyleBackColor = false;
			this._backButton.Click += new System.EventHandler(this.OnBackButtonClick);
			//
			// _nextButton
			//
			this._nextButton.BackColor = System.Drawing.Color.White;
			this._nextButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this._nextButton.FlatAppearance.BorderSize = 0;
			this._nextButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._nextButton.ForeColor = System.Drawing.Color.Transparent;
			this._nextButton.Image = ((System.Drawing.Image)(resources.GetObject("_nextButton.Image")));
			this._nextButton.Location = new System.Drawing.Point(44, 0);
			this._nextButton.Margin = new System.Windows.Forms.Padding(0);
			this._nextButton.Name = "_nextButton";
			this._nextButton.Size = new System.Drawing.Size(20, 20);
			this._nextButton.TabIndex = 65;
			this._toolTip.SetToolTip(this._nextButton, "Forward");
			this._nextButton.UseVisualStyleBackColor = false;
			this._nextButton.Click += new System.EventHandler(this.OnNextButtonClick);
			//
			// _upButton
			//
			this._upButton.BackColor = System.Drawing.Color.White;
			this._upButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this._upButton.FlatAppearance.BorderSize = 0;
			this._upButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._upButton.ForeColor = System.Drawing.Color.Transparent;
			this._upButton.Image = ((System.Drawing.Image)(resources.GetObject("_upButton.Image")));
			this._upButton.Location = new System.Drawing.Point(66, 0);
			this._upButton.Margin = new System.Windows.Forms.Padding(0);
			this._upButton.Name = "_upButton";
			this._upButton.Size = new System.Drawing.Size(20, 20);
			this._upButton.TabIndex = 67;
			this._toolTip.SetToolTip(this._upButton, "Parent Directory");
			this._upButton.UseVisualStyleBackColor = false;
			this._upButton.Click += new System.EventHandler(this.OnUpButtonClick);
			//
			// _addShortcutButton
			//
			this._addShortcutButton.BackColor = System.Drawing.Color.White;
			this._addShortcutButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this._addShortcutButton.FlatAppearance.BorderSize = 0;
			this._addShortcutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._addShortcutButton.ForeColor = System.Drawing.Color.Transparent;
			this._addShortcutButton.Image = ((System.Drawing.Image)(resources.GetObject("_addShortcutButton.Image")));
			this._addShortcutButton.Location = new System.Drawing.Point(0, 0);
			this._addShortcutButton.Margin = new System.Windows.Forms.Padding(0);
			this._addShortcutButton.Name = "_addShortcutButton";
			this._addShortcutButton.Size = new System.Drawing.Size(20, 20);
			this._addShortcutButton.TabIndex = 70;
			this._toolTip.SetToolTip(this._addShortcutButton, "Add a shortcut to the currently-selected folder in the frequently-used folders section");
			this._addShortcutButton.UseVisualStyleBackColor = false;
			this._addShortcutButton.Click += new System.EventHandler(this.OnAddButtonClick);
			//
			// _infoButton
			//
			this._infoButton.BackColor = System.Drawing.Color.White;
			this._infoButton.Cursor = System.Windows.Forms.Cursors.Hand;
			this._infoButton.FlatAppearance.BorderSize = 0;
			this._infoButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._infoButton.ForeColor = System.Drawing.Color.Transparent;
			this._infoButton.Image = ((System.Drawing.Image)(resources.GetObject("_infoButton.Image")));
			this._infoButton.Location = new System.Drawing.Point(132, 0);
			this._infoButton.Margin = new System.Windows.Forms.Padding(0);
			this._infoButton.Name = "_infoButton";
			this._infoButton.Size = new System.Drawing.Size(20, 20);
			this._infoButton.TabIndex = 71;
			this._toolTip.SetToolTip(this._infoButton, "About folder browser control");
			this._infoButton.UseVisualStyleBackColor = false;
			this._infoButton.Visible = false;
			this._infoButton.Click += new System.EventHandler(this.OnInfoButtonClick);
			//
			// _shortcutContextMenu
			//
			this._shortcutContextMenu.Items.Insert(0, this._shortCutRemovalMenuItem);
			//
			// _shortCutRemovalMenuItem
			//
			this._shortCutRemovalMenuItem.Text = "Remove Shortcut";
			this._shortCutRemovalMenuItem.Click += new System.EventHandler(this.OnShortCutRemovalMenuItemClick);
			//
			// _toolBarGroupBox
			//
			this._toolBarGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._toolBarGroupBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this._toolBarGroupBox.Controls.Add(this._infoButton);
			this._toolBarGroupBox.Controls.Add(this._refreshTreeButton);
			this._toolBarGroupBox.Controls.Add(this._homeButton);
			this._toolBarGroupBox.Controls.Add(this._backButton);
			this._toolBarGroupBox.Controls.Add(this._nextButton);
			this._toolBarGroupBox.Controls.Add(this._upButton);
			this._toolBarGroupBox.Controls.Add(this._addShortcutButton);
			this._toolBarGroupBox.ForeColor = System.Drawing.SystemColors.Window;
			this._toolBarGroupBox.Location = new System.Drawing.Point(0, 0);
			this._toolBarGroupBox.Margin = new System.Windows.Forms.Padding(0);
			this._toolBarGroupBox.Name = "_toolBarGroupBox";
			this._toolBarGroupBox.Padding = new System.Windows.Forms.Padding(0);
			this._toolBarGroupBox.Size = new System.Drawing.Size(240, 20);
			this._toolBarGroupBox.TabIndex = 71;
			this._toolBarGroupBox.TabStop = false;
			this._toolBarGroupBox.Paint += new System.Windows.Forms.PaintEventHandler(this.OnGroupBoxPaint);
			//
			// FolderBrowserControl
			//
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this._goButton);
			this.Controls.Add(this._folderPathTextBox);
			this.Controls.Add(this._folderTreeView);
			this.Controls.Add(this._toolBarGroupBox);
			this.Name = "FolderBrowserControl";
			this.Size = new System.Drawing.Size(240, 336);
			this.Load += new System.EventHandler(this.OnFolderBrowserControlLoad);
			this._toolBarGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#region Public methods

		/// <summary>
		/// Wipes folder treeview and user's selection history. then builds a fresh folder tree.
		/// </summary>
		public void ResetTree()
		{
			ClearHistoryList();

			FillTreeViewWithFolders();
			SetCurrentPath(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
		}

		/// <summary>
		/// Redisplays entire control, typically because an outside entity has tweaked the properties to
		/// say what should be visible.
		/// </summary>
		public void ResetControl()
		{
			Cursor.Current = Cursors.WaitCursor;

			// Redo the buttons, toolbar, etc:
			DisplaySelectedControls();

			// Redo the folder treeview:
			ResetTree();

			ClearHistoryList();

			Cursor.Current = Cursors.Default;
			ExpandMyComputerNode();
		}

		public void SetInitialPath(string path)
		{
			ClearHistoryList();
			SetCurrentPath(path);
			HistoryChangeEventHandler(SelectedPath);
		}

		/// <summary>
		/// Makes sure the correct buttons, toolbars etc are displayed, and adjusts positions
		/// of visible controls for best fit.
		/// </summary>
		public void DisplaySelectedControls()
		{
			if ((!ShowAddressbar) && (!ShowToolbar))
			{
				_folderTreeView.Top = 0;
				_folderPathTextBox.Visible = false;
				_goButton.Visible = false;
				_toolBarGroupBox.Visible = false;
				_folderTreeView.Height = Height;
			}
			else
			{
				if (ShowToolbar && (!ShowAddressbar))
				{
					_folderTreeView.Top = 21;
					_folderPathTextBox.Visible = false;
					_goButton.Visible = false;
					_folderTreeView.Height = Height - 21;
					_toolBarGroupBox.Visible = true;
				}
				else if (ShowAddressbar && (!ShowToolbar))
				{
					_folderTreeView.Top = 21;
					_folderPathTextBox.Top = 0;
					_goButton.Top = 0;
					_folderPathTextBox.Visible = true;
					_folderPathTextBox.Width = ShowGoButton ? _folderTreeView.Width - 20 : _folderTreeView.Width;
					_goButton.Visible = ShowGoButton;
					_folderTreeView.Height = Height - 21;
					_toolBarGroupBox.Visible = false;
				}
				else
				{
					_folderTreeView.Top = 42;
					_folderPathTextBox.Visible = true;
					_goButton.Visible = ShowGoButton;
					_folderPathTextBox.Top = 21;
					_folderPathTextBox.Width = ShowGoButton ? _folderTreeView.Width - 20 : _folderTreeView.Width;
					_goButton.Top = 21;
					_toolBarGroupBox.Visible = true;
					_folderTreeView.Height = Height - 42;
				}
			}
		}

		#endregion

		/// <summary>
		/// Sets the folder path text both, after doing a few checks.
		/// </summary>
		/// <param name="strPath"></param>
		private void SetCurrentPath(string strPath)
		{
			if (strPath == null)
				strPath = "";

			SelectedPath = strPath;

			if (strPath.Length == 0)
			{
				_folderPathTextBox.Text = "";
				SelectFolderTreeNodeManually(_treeNodeRootNode);
				_treeNodeRootNode.Expand();
			}
			else
			{
				// Set the proposed folder if it exists, otherwise use user's personal folder path:
				_folderPathTextBox.Text = Directory.Exists(strPath)
											? strPath
											: Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			}
		}

		/// <summary>
		/// Loads the topmost nodes of the folder tree view with folders.
		/// The lower layers are not filled until user clicks "+" buttons
		/// to reveal them. Very efficient.
		/// </summary>
		private void FillTreeViewWithFolders()
		{
			// Start with a clean slate:
			_folderTreeView.Nodes.Clear();

			// Desktop node:
			var treeNodeDesktop = new TreeNode
									{
										Tag = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
										Text = DesktopFolderName,
										ImageIndex = 10,
										SelectedImageIndex = 10
									};

			_folderTreeView.Nodes.Add(treeNodeDesktop);
			_treeNodeRootNode = treeNodeDesktop;

			if (ShowMyDocuments)
			{
				// Add My Documents and Desktop folder node:
				var treeNodeMyDocuments = new TreeNode
											{
												Tag = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
												Text = MyDocumentsFolderName,
												ImageIndex = 9,
												SelectedImageIndex = 9
											};
				treeNodeDesktop.Nodes.Add(treeNodeMyDocuments);
				AddChildFolders(treeNodeMyDocuments);
			}

			// My Computer node:
			_treeNodeMyComputer = new TreeNode
									{
										Tag = MyComputerFolderName,
										Text = MyComputerFolderName,
										ImageIndex = 12,
										SelectedImageIndex = 12
									};
			treeNodeDesktop.Nodes.Add(_treeNodeMyComputer);

			// Dummy child node for My Computer (to make "+" box appear):
			var treeNodePlaceholder = new TreeNode
										{
											Tag = DummyMyComputerChildName,
											Text = DummyMyComputerChildName,
											ImageIndex = 12,
											SelectedImageIndex = 12
										};
			_treeNodeMyComputer.Nodes.Add(treeNodePlaceholder);

			if (ShowMyNetwork)
			{
				// My Network Places node:
				var treeNodeMyNetwork = new TreeNode
											{
												Tag = MyNetworkPlacesFolderName,
												Text = MyNetworkPlacesFolderName,
												ImageIndex = 13,
												SelectedImageIndex = 13
											};
				treeNodeDesktop.Nodes.Add(treeNodeMyNetwork);

				var treeNodeEntireNetwork = new TreeNode
												{
													Tag = EntireNetworkFolderName,
													Text = EntireNetworkFolderName,
													ImageIndex = 14,
													SelectedImageIndex = 14
												};
				treeNodeMyNetwork.Nodes.Add(treeNodeEntireNetwork);

				// Dummy child node for Entire Network (to make "+" box appear):
				var treeNodeNetwork = new TreeNode
										{
											Tag = DummyEntireNetworkChildName,
											Text = DummyEntireNetworkChildName,
											ImageIndex = 15,
											SelectedImageIndex = 15
										};
				treeNodeEntireNetwork.Nodes.Add(treeNodeNetwork);
			}

			if (ShowMyFavorites)
			{
				var treeNodeMyFavorites = new TreeNode
											{
												Tag = Environment.GetFolderPath(Environment.SpecialFolder.Favorites),
												Text = MyFavoritesFolderName,
												ImageIndex = 26,
												SelectedImageIndex = 26
											};
				treeNodeDesktop.Nodes.Add(treeNodeMyFavorites);
				AddChildFolders(treeNodeMyFavorites);
			}

			// Fill the next levels in the Desktop node:
			AddChildAndGrandchildFolders(treeNodeDesktop);
		}

		/// <summary>
		/// Fills in the child and grandchild levels of subfolders of the given folder.
		/// </summary>
		/// <param name="currentFolder">A folder somewhere in the folder treeview</param>
		private static void AddChildAndGrandchildFolders(TreeNode currentFolder)
		{
			// This could take a while:
			Cursor.Current = Cursors.WaitCursor;

			// Get first level of subfolders (direct children):
			AddChildFolders(currentFolder);

			// Get grandchildren of current folder so user can see there are
			// more folders underneath current folder:
			foreach (TreeNode node in currentFolder.Nodes)
			{
				if (currentFolder.Text == DesktopFolderName)
				{
					if (!IsSpecialNode(node))
					{
						AddChildFolders(node);
					}
				}
				else
				{
					AddChildFolders(node);
				}
			}

			Cursor.Current = Cursors.Default;
		}

		/// <summary>
		/// This method will check if a given treenode is one of the specially handled ones
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private static bool IsSpecialNode(TreeNode node)
		{
			return (node.Text == MyDocumentsFolderName)
				   || (node.Text == MyComputerFolderName)
				   || (node.Text == MicrosoftWindowsNetworkFolderName)
				   || (node.Text == MyNetworkPlacesFolderName)
				   || (node.Text == EntireNetworkFolderName)
				   || (node.Parent != null && node.Parent.Text == MicrosoftWindowsNetworkFolderName)
				   || (node.Tag.ToString().Contains("\\\\") && node.Tag.ToString().LastIndexOf('\\') <= 1); //can't use \\computername to find shares, they must be clicked on directly.
		}

		/// <summary>
		/// Fills in the child level of subfolders of the given folder.
		/// </summary>
		/// <param name="currentFolder">A folder somewhere in the folder treeview</param>
		private static void AddChildFolders(TreeNode currentFolder)
		{
			try
			{
				if(!IsSpecialNode(currentFolder) && !string.IsNullOrEmpty(currentFolder.Tag as string))
				{
					var folderList = Directory.GetDirectories(currentFolder.Tag.ToString());

					// Rudimentary check to see if we've already processed this folder node:
					if (folderList.Length == currentFolder.Nodes.Count)
						return;

					Array.Sort(folderList);

					// Add to the currentFolder's children each folder's path from folderList:
					foreach (var path in folderList)
					{
						var testDup = currentFolder.Nodes.Find(path, false);
						if(testDup.Length == 0)
						{
							var node = new TreeNode { Name = path, Tag = path, Text = Path.GetFileName(path) ?? "", ImageIndex = 1 };
							currentFolder.Nodes.Add(node);
						}
					}
				}
			}
			catch (UnauthorizedAccessException)
			{
				return; // We don't care if we can't read a folder; we'll just go on to the next one.
			}
		}

		#region Methods for filling in special-folder nodes in treeview

		/// <summary>
		/// Adds servers etc to the treeview node previously identified as a child of the Microsoft Windows Network node.
		/// </summary>
		/// <param name="microsoftWindowsNetworkChildNode"></param>
		private static void ExpandMicrosoftWindowsNetworkChildNode(TreeNode microsoftWindowsNetworkChildNode)
		{
			// Check if the first child node is our dummy placeholder:
			if (microsoftWindowsNetworkChildNode.FirstNode.Text == DummyMicrosoftWindowsNetworkName)
			{
				// Remove dummy and add real data:
				microsoftWindowsNetworkChildNode.FirstNode.Remove();

				var serverPath = microsoftWindowsNetworkChildNode.Text;

				var servers = new ServerEnum(ServerEnum.ResourceScope.RESOURCE_GLOBALNET,
					ServerEnum.ResourceType.RESOURCETYPE_DISK, ServerEnum.ResourceUsage.RESOURCEUSAGE_ALL,
					ServerEnum.ResourceDisplayType.RESOURCEDISPLAYTYPE_SERVER, serverPath);

				foreach (string server in servers)
				{
					if (server.EndsWith("-share"))
						continue;

					var serverNode = new TreeNode
										{
											Tag = server,
											Text = server.Substring(2),
											ImageIndex = 12,
											SelectedImageIndex = 12
										};
					microsoftWindowsNetworkChildNode.Nodes.Add(serverNode);

					// Look for child (share) servers:
					foreach (var shareServer in servers.Cast<string>().Where(shareServer => shareServer.EndsWith("-share")))
					{
						if (server.Length <= shareServer.Length)
						{
							try
							{
								if (shareServer.StartsWith(server + Path.DirectorySeparatorChar))
								{
									var childServerNode = new TreeNode
															{
																Tag = shareServer.Substring(0, shareServer.Length - 6),  // Server name minus "-share"
																Text = shareServer.Substring(server.Length + 1, shareServer.Length - server.Length - 7),
																ImageIndex = 28,
																SelectedImageIndex = 28
															};
									serverNode.Nodes.Add(childServerNode);
								}
							}
							catch (ArgumentOutOfRangeException)
							{
							}
						}
					} // Next child server
				} // Next server
			} //End if first child node is our dummy placeholder
		}

		/// <summary>
		/// Adds servers etc to the treeview node previously identified as the Entire Network node.
		/// </summary>
		/// <param name="entireNetworkNode"></param>
		private static void ExpandEntireNetworkNode(TreeNode entireNetworkNode)
		{
			// First test that we haven't already expanded this node, by seeing if the first node
			// is our dummy placeholder:
			if (entireNetworkNode.FirstNode.Text != DummyEntireNetworkChildName)
				return;

			entireNetworkNode.FirstNode.Remove();

			var servers = new ServerEnum(ServerEnum.ResourceScope.RESOURCE_GLOBALNET,
				ServerEnum.ResourceType.RESOURCETYPE_DISK, ServerEnum.ResourceUsage.RESOURCEUSAGE_ALL,
				ServerEnum.ResourceDisplayType.RESOURCEDISPLAYTYPE_NETWORK, "");

			foreach (string server in servers)
			{
				var serverNameBeforeBar = server.Substring(0, server.IndexOf("|", 1));

				if (server.IndexOf("NETWORK", 1) > 0)
				{
					var networkNode = new TreeNode
										{
											Tag = serverNameBeforeBar,
											Text = serverNameBeforeBar,
											ImageIndex = 15,
											SelectedImageIndex = 15
										};
					entireNetworkNode.Nodes.Add(networkNode);
				}
				else
				{
					var myNetworkNode = new TreeNode
											{
												Tag = serverNameBeforeBar,
												Text = serverNameBeforeBar,
												ImageIndex = 16,
												SelectedImageIndex = 16
											};
					entireNetworkNode.LastNode.Nodes.Add(myNetworkNode);

					// Add dummy placeholder node for Micorosoft Windows Network (to make "+" box appear):
					var dummyNode = new TreeNode
										{
											Tag = DummyMicrosoftWindowsNetworkName,
											Text = DummyMicrosoftWindowsNetworkName,
											ImageIndex = 12,
											SelectedImageIndex = 12
										};
					myNetworkNode.Nodes.Add(dummyNode);
				}
			}
		}

		/// <summary>
		/// Adds drives and folders to the folder treeview node previously identified as the My Computer node.
		/// </summary>
		private void ExpandMyComputerNode()
		{
			// Check that the node has not already been expanded. The initial unexpanded state is
			// that there is only one child, which has the Tag and Text equal to DummyMyComputerChildName:
			var treeNodeCount = _treeNodeMyComputer.GetNodeCount(true);
			if (treeNodeCount >= 2)
				return;
			if (treeNodeCount > 0)
			{
				if (_treeNodeMyComputer.FirstNode.Tag.ToString() != DummyMyComputerChildName
					|| _treeNodeMyComputer.FirstNode.Text != DummyMyComputerChildName)
				{
					return;
				}

				// Remove dummy placeholder (first child):
				_treeNodeMyComputer.FirstNode.Remove();
			}

			// Iterate of all logical drives on user's computer:
			var logicalDrives = Environment.GetLogicalDrives();

			foreach (var drive in logicalDrives)
			{
				var driveType = Win32.GetDriveType(drive);

				if (_showOnlyMappedDrives)
				{
					if (driveType != 4) // DRIVE_REMOTE
						continue;
				}

				// Make a new node for each drive:
				var logicalDriveNode = new TreeNode {Tag = drive, Text = drive};

				// Determine which icon to display, according to drive type:
				switch (driveType)
				{
					case 2: // DRIVE_REMOVABLE
						logicalDriveNode.ImageIndex = 17;
						logicalDriveNode.SelectedImageIndex = 17;
						break;
					case 3: // DRIVE_FIXED
						logicalDriveNode.ImageIndex = 0;
						logicalDriveNode.SelectedImageIndex = 0;
						break;
					case 4: // DRIVE_REMOTE
						logicalDriveNode.ImageIndex = 8;
						logicalDriveNode.SelectedImageIndex = 8;
						break;
					case 5: // DRIVE_CDROM
						logicalDriveNode.ImageIndex = 7;
						logicalDriveNode.SelectedImageIndex = 7;
						break;
					default:
						logicalDriveNode.ImageIndex = 0;
						logicalDriveNode.SelectedImageIndex = 0;
						break;
				}

				// Add in the new drive's node:
				_treeNodeMyComputer.Nodes.Add(logicalDriveNode);

				// Add child folders into tree:
				if (Directory.Exists(drive))
				{
					try
					{
						foreach (var folderPath in Directory.GetDirectories(drive))
						{
							var node = new TreeNode {Tag = folderPath, Text = Path.GetFileName(folderPath) ?? "", ImageIndex = 1};
							logicalDriveNode.Nodes.Add(node);
						}
					}
					catch (UnauthorizedAccessException)
					{
						// We don't care if we can't read a folder; we'll just go on to the next one.
					}
				}
			}
			_treeNodeMyComputer.Expand();
		}

		#endregion

		/// <summary>
		/// Attempts to expand the folder tree to reveal the last folder element in the textbox path.
		/// Assumes the textbox path is under My Computer.
		/// </summary>
		/// <param name="focusTree">Whether or not treeview should gain focus</param>
		private void ExpandTreeToMatchTextbox(bool focusTree)
		{
			Cursor.Current = Cursors.WaitCursor;

			// Make sure we see at least the initial data under the My Computer node:
			ExpandMyComputerNode();

			// Expand the tree all the way down the path in the textbox:
			ExpandTreeFromPath(_treeNodeMyComputer, _folderPathTextBox.Text, focusTree);

			Cursor.Current = Cursors.Default;
		}

		/// <summary>
		/// Recursively expands folder tree to reveal entire given Path.
		/// </summary>
		/// <param name="lastExpandedFolderNode">The last folder node to have been expanded so far</param>
		/// <param name="path">Full text path that is to be expanded in tree</param>
		/// <param name="focusTree">Whether or not treeview should gain focus</param>
		private void ExpandTreeFromPath(TreeNode lastExpandedFolderNode, string path, bool focusTree)
		{
			// Windows is case-insensitive in folder names, so to compensate for user case-laziness,
			// we will work entirely in lower case:
			var usefulPath = Platform.IsWindows ? path.ToLower() : path;

			// Search all subfolder nodes for one which gets us further along the lowerCasePath:
			foreach (TreeNode subfolderNode in lastExpandedFolderNode.Nodes)
			{
				// Get full path of current subfolder:
				var subfolderPath = Platform.IsWindows
					? subfolderNode.Tag.ToString().ToLower()
					: subfolderNode.Tag.ToString();

				// If we haven't gotten all the way along lowerCasePath, make a copy of
				// subfolderPath with a trailing backslash:
				string subfolderPathBackslash = subfolderPath;
				if (!subfolderPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					if (usefulPath.Length > subfolderPathBackslash.Length)
						subfolderPathBackslash = subfolderPath + Path.DirectorySeparatorChar;
				}

				// See if the current path is matched by the current subfolder (as far as it goes):
				if (usefulPath.StartsWith(subfolderPathBackslash))
				{
					subfolderNode.Expand();
					if (focusTree)
						subfolderNode.TreeView.Focus();

					// If the current subfolder has any child folders, recurse to see if any of
					// them need expanding:
					if (subfolderNode.Nodes.Count >= 1)
					{
						ExpandTreeFromPath(subfolderNode, usefulPath, focusTree);
						return;
					}
					// If we've gone all the way through the given path, then we're done:
					if (usefulPath == subfolderPath)
					{
						// Base case:
						SelectFolderTreeNodeManually(subfolderNode);
						return;
					}

					// Sanity check: have we already gone too far?
					if (subfolderPathBackslash.StartsWith(usefulPath))
					{
						// Base case:
						return;
					}
				}
			}

			// Manually select the last node we expanded:
			SelectFolderTreeNodeManually(lastExpandedFolderNode);

			// Base case:
			return;
		}

		/// <summary>
		/// Call this method to select a node in the Folder tree when you don't want
		/// any events fired as a consequence.
		/// </summary>
		/// <param name="node">The _folderTreeView node to select</param>
		private void SelectFolderTreeNodeManually(TreeNode node)
		{
			// Prevent the TreeView event handler firing when we manually select a node:
			_folderTreeView.AfterSelect -= OnFolderTreeViewAfterSelect;

			// Select the node:
			_folderTreeView.SelectedNode = node;

			// Reinstate the TreeView event handler:
			_folderTreeView.AfterSelect += OnFolderTreeViewAfterSelect;
		}

		/// <summary>
		/// Call this method to set the folder path textbox when you don't want any
		/// events fired as a consequence.
		/// </summary>
		/// <param name="path">The path string to put in the textbox</param>
		void SetFolderPathTextBoxManually(string path)
		{
			// Prevent the textbox event handler firing:
			_folderPathTextBox.TextChanged -= OnFolderPathTextChanged;

			_folderPathTextBox.Text = path;

			// Reinstate the textbox event handler:
			_folderPathTextBox.TextChanged += OnFolderPathTextChanged;

		}

		/// <summary>
		/// Adds the given path to the root node in the folder treeview, so it looks
		/// like a shortcut.
		/// </summary>
		/// <param name="name">Name of folder</param>
		/// <param name="path">Full path of folder</param>
		private void AddFolderShortcut(string name, string path)
		{
			// Special image with index 18 only used in shortcuts:
			var shortcutNode = new TreeNode {Tag = path, Text = name, ImageIndex = 18, SelectedImageIndex = 18};

			_treeNodeRootNode.Nodes.Add(shortcutNode);

			AddChildFolders(shortcutNode);

			shortcutNode.TreeView.SelectedNode = shortcutNode;
			shortcutNode.EnsureVisible();
			shortcutNode.TreeView.Focus();
		}

		#region General control event handlers

		/// <summary>
		/// Response to FolderBrowserControl Load event, occurs before the control becomes visible for the first time.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFolderBrowserControlLoad(object sender, EventArgs e)
		{
			// Load up the tree view:
			FillTreeViewWithFolders();

			ClearHistoryList();
			HistoryChangeEventHandler += AddToSelectionHistoryList;

			// Set the text in the folder text box:
			SetCurrentPath(SelectedPath);

			// Make sure correct buttons and other controls are displayed:
			DisplaySelectedControls();
		}

		/// <summary>
		/// Event Handler for when Shortcut context menu item "Remove Shortcut" is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnShortCutRemovalMenuItemClick(object sender, EventArgs e)
		{
			// Check that we're really on a shortcut by testing the image index for our special number:
			if (_folderTreeView.SelectedNode.ImageIndex == 18)
				_folderTreeView.SelectedNode.Remove();
		}

		/// <summary>
		/// Hack to remove irritating inner border from GroupBoxes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="p"></param>
		private void OnGroupBoxPaint(object sender, PaintEventArgs p)
		{
			var groupBox = (GroupBox)sender;
			p.Graphics.Clear(groupBox.BackColor);
		}

		#endregion

		#region Folder path textbox event handlers

		/// <summary>
		/// Event handler for when the text changes in the Folder Path textbox.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFolderPathTextChanged(object sender, EventArgs e)
		{
			// Check that the proposed folder exists, then set the new selection:
			if (Directory.Exists(_folderPathTextBox.Text))
			{
				SelectedPath = _folderPathTextBox.Text;

				// Make sure the typed-in (or pasted) folder is selected and visible in the treeview.
				// Pass in "false" to make sure the textbox does not lose focus to the treeview:
				ExpandTreeToMatchTextbox(false);

				// Raise our custom event, so the consumer of this control can respond:
				_pathChangedEvent(this, EventArgs.Empty);

				HistoryChangeEventHandler(SelectedPath);
			}
		}

		/// <summary>
		/// Event handler for when a keyboard key is released in the Folder Path textbox.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTextBoxFolderPathKeyUp(object sender, KeyEventArgs e)
		{
			// If the user just released the Enter key, show the textbox path's location in the tree
			if (e.KeyValue == 13)
			{
				ExpandTreeToMatchTextbox(true);
				_folderPathTextBox.Focus();
			}
		}

		#endregion

		#region TreeView event handlers

		/// <summary>
		/// Handles AfterSelect event in folder tree view. Occurs after a tree node is selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFolderTreeViewAfterSelect(object sender, TreeViewEventArgs e)
		{
			var selectedNode = e.Node;

			// Set the path textbox text to match the selection, unless it is one of the fancy folders:
			if (!IsSpecialNode(selectedNode))
			{
				SetFolderPathTextBoxManually(selectedNode.Tag.ToString());
				SelectedPath = _folderPathTextBox.Text;
				HistoryChangeEventHandler(selectedNode.Tag.ToString());

				// Raise our custom event, so the consumer of this control can respond:
				_pathChangedEvent(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Handles AfterExpand event in folder tree view. Occurs after the tree node is expanded.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFolderTreeViewAfterExpand(object sender, TreeViewEventArgs e)
		{
			// This could take some time:
			Cursor.Current = Cursors.WaitCursor;

			var expandedNode = e.Node;

			// If there is a colon in the expanded node's folder name, then it is a regular folder on a (mapped) drive:
			if (expandedNode.Text.Contains(":"))
			{
				AddChildAndGrandchildFolders(expandedNode);
			}
			else
			{
				// Check if microsoftWindowsNetworkChildNode is one of several special folders:
				switch(expandedNode.Text)
				{
					case MyComputerFolderName:
						if (expandedNode.GetNodeCount(true) < 2)
						{
							ExpandMyComputerNode();
						}
						break;
					case EntireNetworkFolderName:
						ExpandEntireNetworkNode(expandedNode);
						break;
					default:
						if (expandedNode.Parent != null && expandedNode.Parent.Text == MicrosoftWindowsNetworkFolderName)
						{
							ExpandMicrosoftWindowsNetworkChildNode(expandedNode);
						}
						else
						{
							AddChildAndGrandchildFolders(expandedNode);
						}
						break;
				}
			}
			Cursor.Current = Cursors.Default;
		}

		/// <summary>
		/// Handles DoubleClick event in folder tree view. Occurs when the control is double-clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFolderTreeViewDoubleClick(object sender, EventArgs e)
		{
			if (!_folderTreeView.SelectedNode.IsExpanded)
				_folderTreeView.SelectedNode.Collapse();
			else
			{
				AddChildAndGrandchildFolders(_folderTreeView.SelectedNode);
			}
		}

		/// <summary>
		/// Handles MouseUp event in folder tree view. Adds folder selection to selection history.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFolderTreeViewMouseUp(object sender, MouseEventArgs e)
		{
			// If user is on a custom folder shortcut, then releasing the right button brings up
			// the shortcuts context menu (but only if folder already selected):
			if (_folderTreeView.SelectedNode != null)
			{
				// 18 is a special image index only used in shortcuts:
				if ((_folderTreeView.SelectedNode.ImageIndex == 18) && (e.Button == MouseButtons.Right))
					_shortcutContextMenu.Show(_folderTreeView, new Point(e.X, e.Y));
			}
		}

		/// <summary>
		/// Event handler for when a node is about to be selected in our Folders treeview.
		/// We need this so we can highlight the selected node even when the tree
		/// is not in focus.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFolderTreeViewBeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			if (_folderTreeView.SelectedNode != null)
				_folderTreeView.SelectedNode.ForeColor = Color.Black;
			e.Node.ForeColor = Color.Blue;
		}

		/// <summary>
		/// Event handler for when a node is to be drawn in our Folders treeview.
		/// We need this so we can highlight the selected node even when the tree
		/// is not in focus.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFolderTreeViewDrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			if (((e.State & TreeNodeStates.Selected) != 0) && (!_folderTreeView.Focused))
				e.Node.ForeColor = Color.Blue;
			else
				e.DrawDefault = true;
		}

		#endregion

		#region Button event handlers

		/// <summary>
		/// Event handler for when Go button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnGoButtonClick(object sender, EventArgs e)
		{
			ExpandTreeToMatchTextbox(true);
		}

		/// <summary>
		/// Handler for Click event of Refresh button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRefreshButtonClick(object sender, EventArgs e)
		{
			ResetControl();
		}

		/// <summary>
		/// Event handler for when Home button is clicked.
		/// Changes folder selection to user's personal folder folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnHomeButtonClick(object sender, EventArgs e)
		{
			Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			ExpandMyComputerNode();

			// Expand folder tree to reveal selected folder:
			ExpandTreeToMatchTextbox(true);
		}

		/// <summary>
		/// Event handler for when Next button is clicked.
		/// Changes folder selection to next folder in selection history.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnNextButtonClick(object sender, EventArgs e)
		{
			HistoryChangeEventHandler -= AddToSelectionHistoryList;

			// Remember the current path selection:
			string storedPath = _folderPathTextBox.Text;

			// Move to next path in selection history:
			var nextPath = GetHistoryListNextEntry();
			if (nextPath != null)
				_folderPathTextBox.Text = nextPath;

			// If the path actually changed, then expand the folder tree to the new path:
			if (storedPath != _folderPathTextBox.Text)
				ExpandTreeToMatchTextbox(true);

			HistoryChangeEventHandler += AddToSelectionHistoryList;
		}

		/// <summary>
		/// Event handler for when Back button is clicked.
		/// Changes folder selection to next folder in selection history.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnBackButtonClick(object sender, EventArgs e)
		{
			HistoryChangeEventHandler -= AddToSelectionHistoryList;

			// Remember the current path selection:
			string storedPath = _folderPathTextBox.Text;

			// Move to previous path in selection history:
			var previousPath = GetHistoryListPreviousEntry();
			if (previousPath != null)
				_folderPathTextBox.Text = previousPath;

			// If the path actually changed, then expand the folder tree to the new path:
			if (storedPath != _folderPathTextBox.Text)
				ExpandTreeToMatchTextbox(true);

			HistoryChangeEventHandler += AddToSelectionHistoryList;
		}

		/// <summary>
		/// Event handler for when Up button is clicked.
		/// Changes folder selection to parent folder of current selection.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUpButtonClick(object sender, EventArgs e)
		{
			// Get current selected folder's path:
			var currentFolder = new DirectoryInfo(_folderPathTextBox.Text);

			// Get current selected folder's parent:
			var parentFolder = currentFolder.Parent;

			if (parentFolder == null)
				return;

			if (!parentFolder.Exists)
				return;

			// Change selection to parent folder:
			_folderPathTextBox.Text = parentFolder.FullName;
			ExpandTreeToMatchTextbox(true);
		}

		/// <summary>
		/// Event handler for when Add button is clicked.
		/// Adds a user-defined shortcut to a folder which user selects from tradational
		/// Folder Browser dialog. (Yuck!)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAddButtonClick(object sender, EventArgs e)
		{
			if (Directory.Exists(SelectedPath))
			{
				var folderName = Path.GetFileName(SelectedPath);
				AddFolderShortcut(folderName, SelectedPath);
			}
		}

		/// <summary>
		/// Event handler for when the Info button is pressed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInfoButtonClick(object sender, EventArgs e)
		{
			// We don't currently support this.
		}

		#endregion

		#region Methods to deal with user's selection history

		/// <summary>
		/// Create a new path-selection history list.
		/// </summary>
		private void ClearHistoryList()
		{
			_selectionHistory = new List<string>();
			_selectionHistoryIndex = -1;
		}

		/// <summary>
		/// Returns the user's previously-selected path.
		/// </summary>
		/// <returns></returns>
		private string GetHistoryListPreviousEntry()
		{
			if (_selectionHistoryIndex > 0)
			{
				_selectionHistoryIndex--;
				return _selectionHistory[_selectionHistoryIndex];
			}
			return null;
		}

		/// <summary>
		/// Returns the next path that the user navigated back from.
		/// </summary>
		/// <returns></returns>
		private string GetHistoryListNextEntry()
		{
			if (_selectionHistoryIndex < _selectionHistory.Count - 1)
			{
				_selectionHistoryIndex++;
				return _selectionHistory[_selectionHistoryIndex];
			}
			return null;
		}

		/// <summary>
		/// Adds a given path to the selected-path history list.
		/// </summary>
		/// <param name="path"></param>
		private void AddToSelectionHistoryList(string path)
		{
			// To avoid the possibility of adding a path ending with a backslash when the
			// current history entry is the same but without a backslash, we will remove trailing
			// backslashes first:
			while (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
				path = path.Substring(0, path.Length - 1);

			// Don't add a path if it is the same (case-insensitive) as the current entry in the history list:
			// TODO: case-insentive search won't do for Linux (but case-sensitive search won't do for Windows).
			if (_selectionHistoryIndex >= 0 && _selectionHistoryIndex < _selectionHistory.Count)
				if (string.Compare(_selectionHistory[_selectionHistoryIndex], path, true) == 0)
					return;

			// Guard against an empty list or when we're pointing at the last item:
			if (_selectionHistoryIndex < _selectionHistory.Count - 1)
				_selectionHistory.RemoveRange(_selectionHistoryIndex + 1, _selectionHistory.Count - _selectionHistoryIndex - 1);

			_selectionHistory.Add(path);

			_selectionHistoryIndex = _selectionHistory.Count - 1;
		}

		#endregion
	}

	#region Windows shell functions

	public class Win32
	{
		[DllImport("kernel32.dll")]
		[CLSCompliant(false)]
		public static extern uint GetDriveType(string lpRootPathName);
	}

	#endregion

	#region Class to deal with enumeration of network servers

	/// <summary>
	/// Class to deal with enumeration of network servers.
	/// </summary>
	public class ServerEnum : IEnumerable
	{
		// ReSharper disable InconsistentNaming
		enum ErrorCodes
		{
			NO_ERROR = 0,
			ERROR_NO_MORE_ITEMS = 259
		};

		public enum ResourceScope
		{
			RESOURCE_CONNECTED = 1,
			RESOURCE_GLOBALNET,
			RESOURCE_REMEMBERED,
			RESOURCE_RECENT,
			RESOURCE_CONTEXT
		};

		public enum ResourceType
		{
			RESOURCETYPE_ANY,
			RESOURCETYPE_DISK,
			RESOURCETYPE_PRINT,
			RESOURCETYPE_RESERVED
		};

		[Flags]
		public enum ResourceUsage
		{
			RESOURCEUSAGE_CONNECTABLE = 0x00000001,
			RESOURCEUSAGE_CONTAINER = 0x00000002,
			RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
			RESOURCEUSAGE_SIBLING = 0x00000008,
			RESOURCEUSAGE_ATTACHED = 0x00000010,
			RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
		};

		public enum ResourceDisplayType
		{
			RESOURCEDISPLAYTYPE_GENERIC,
			RESOURCEDISPLAYTYPE_DOMAIN,
			RESOURCEDISPLAYTYPE_SERVER,
			RESOURCEDISPLAYTYPE_SHARE,
			RESOURCEDISPLAYTYPE_FILE,
			RESOURCEDISPLAYTYPE_GROUP,
			RESOURCEDISPLAYTYPE_NETWORK,
			RESOURCEDISPLAYTYPE_ROOT,
			RESOURCEDISPLAYTYPE_SHAREADMIN,
			RESOURCEDISPLAYTYPE_DIRECTORY,
			RESOURCEDISPLAYTYPE_TREE,
			RESOURCEDISPLAYTYPE_NDSCONTAINER
		};

		// ReSharper restore InconsistentNaming

// ReSharper disable ConvertToConstant.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable RedundantDefaultFieldInitializer
#pragma warning disable 169
		[StructLayout(LayoutKind.Sequential)]
		private class NetResource
		{
			public ResourceScope dwScope = 0;
			public ResourceType dwType = 0;
			public ResourceDisplayType dwDisplayType = 0;
			public ResourceUsage dwUsage = 0;
			public string lpLocalName = null;
			public string lpRemoteName = null;
			public string lpComment = null;
			public string lpProvider = null;
		};
#pragma warning restore 169
// ReSharper restore RedundantDefaultFieldInitializer
// ReSharper restore FieldCanBeMadeReadOnly.Local
// ReSharper restore ConvertToConstant.Local

		private readonly ArrayList _aData = new ArrayList();

		public int Count
		{
			get { return _aData.Count; }
		}

		[DllImport("Mpr.dll", EntryPoint = "WNetOpenEnumA", CallingConvention = CallingConvention.Winapi)]
		private static extern ErrorCodes WNetOpenEnum(ResourceScope dwScope, ResourceType dwType,
			ResourceUsage dwUsage, NetResource p, out IntPtr lphEnum);

		[DllImport("Mpr.dll", EntryPoint = "WNetCloseEnum", CallingConvention = CallingConvention.Winapi)]
		private static extern ErrorCodes WNetCloseEnum(IntPtr hEnum);

		[DllImport("Mpr.dll", EntryPoint = "WNetEnumResourceA", CallingConvention = CallingConvention.Winapi)]
		private static extern ErrorCodes WNetEnumResource(IntPtr hEnum, ref uint lpcCount, IntPtr buffer,
			ref uint lpBufferSize);

		private void EnumerateServers(NetResource pRsrc, ResourceScope scope, ResourceType type,
			ResourceUsage usage, ResourceDisplayType displayType, string kPath)
		{
			uint bufferSize = 16384;
			var buffer = Marshal.AllocHGlobal((int)bufferSize);
			IntPtr handle;
			uint cEntries = 1;
			var serverenum = false;

			var result = WNetOpenEnum(scope, type, usage, pRsrc, out handle);

			if (result == ErrorCodes.NO_ERROR)
			{
				do
				{
					result = WNetEnumResource(handle, ref cEntries, buffer, ref	bufferSize);

					if ((result == ErrorCodes.NO_ERROR))
					{
						Marshal.PtrToStructure(buffer, pRsrc);

						if (kPath == "")
						{
							if ((pRsrc.dwDisplayType == displayType) || (pRsrc.dwDisplayType == ResourceDisplayType.RESOURCEDISPLAYTYPE_DOMAIN))
								_aData.Add(pRsrc.lpRemoteName + "|" + pRsrc.dwDisplayType);

							if ((pRsrc.dwUsage & ResourceUsage.RESOURCEUSAGE_CONTAINER) == ResourceUsage.RESOURCEUSAGE_CONTAINER)
								if ((pRsrc.dwDisplayType == displayType))
									EnumerateServers(pRsrc, scope, type, usage, displayType, kPath);
						}
						else
						{
							if (pRsrc.dwDisplayType == displayType)
							{
								_aData.Add(pRsrc.lpRemoteName);
								EnumerateServers(pRsrc, scope, type, usage, displayType, kPath);
								serverenum = true;
							}
							if (!serverenum)
							{
								if (pRsrc.dwDisplayType == ResourceDisplayType.RESOURCEDISPLAYTYPE_SHARE)
									_aData.Add(pRsrc.lpRemoteName + "-share");
							}
							else
								serverenum = false;

							if ((kPath.IndexOf(pRsrc.lpRemoteName) >= 0) || (String.Compare(pRsrc.lpRemoteName, "Microsoft Windows Network") == 0))
								EnumerateServers(pRsrc, scope, type, usage, displayType, kPath);
						}
					}
					else if (result != ErrorCodes.ERROR_NO_MORE_ITEMS)
						break;
				} while (result != ErrorCodes.ERROR_NO_MORE_ITEMS);

				WNetCloseEnum(handle);
			}
			Marshal.FreeHGlobal(buffer);
		}

		public ServerEnum(ResourceScope scope, ResourceType type, ResourceUsage usage, ResourceDisplayType displayType, string kPath)
		{
			var netRoot = new NetResource();
			EnumerateServers(netRoot, scope, type, usage, displayType, kPath);
		}

		public IEnumerator GetEnumerator()
		{
			return _aData.GetEnumerator();
		}
	}

	#endregion
}
