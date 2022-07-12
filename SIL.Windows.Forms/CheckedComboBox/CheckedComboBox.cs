// From https://www.codeproject.com/Articles/31105/A-ComboBox-with-a-CheckedListBox-as-a-Dropdown
// License: https://www.codeproject.com/info/cpol10.aspx

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIL.Reflection;
using SIL.Windows.Forms.Extensions;
using static System.String;

namespace SIL.Windows.Forms.CheckedComboBox
{
	public class CheckedComboBox : ComboBox
	{
		/// <summary>
		/// Internal class to represent the dropdown list of the CheckedComboBox
		/// </summary>
		internal class Dropdown : Form
		{
			private CheckedComboBox _ccbParent;
			private string _oldStrValue = "";
			private bool _dropdownClosed = true;
			// Array holding the checked states of the items. This will be used to reverse any changes if user cancels selection.
			bool[] _checkedStateArr;

			/// <summary>
			/// Custom EventArgs encapsulating value as to whether the combo box value(s) should be
			/// assigned to or not.
			/// </summary>
			internal class CCBoxEventArgs : EventArgs
			{
				public bool AssignValues { get; set; }

				public EventArgs EventArgs { get; set; }

				public CCBoxEventArgs(EventArgs e, bool assignValues)
				{
					EventArgs = e;
					AssignValues = assignValues;
				}
			}

			/// <summary>
			/// A custom CheckedListBox being shown within the dropdown form representing the
			/// dropdown list of the CheckedComboBox.
			/// </summary>
			internal class CustomCheckedListBox : CheckedListBox
			{
				private readonly Func<string> _getSummaryDisplayMember;
				private int _curSelIndex = -1;

				internal CustomCheckedListBox(Func<string> getSummaryDisplayMember)
				{
					_getSummaryDisplayMember = getSummaryDisplayMember;
				}

				protected override void OnItemCheck(ItemCheckEventArgs ice)
				{
					base.OnItemCheck(ice);
				}

				/// <summary>
				/// Intercepts the keyboard input:
				/// [Enter] changes and closes the drop-down
				/// [Delete] clears all checked items
				/// [Shift + Esc] selects all items
				/// </summary>
				/// <param name="e">The Key event arguments</param>
				protected override void OnKeyDown(KeyEventArgs e)
				{
					switch (e.KeyCode)
					{
						case Keys.Enter:
							((Dropdown)Parent).OnDeactivate(new CCBoxEventArgs(null, true));
							e.Handled = true;
							break;
						case Keys.Escape:
							((Dropdown)Parent).OnDeactivate(new CCBoxEventArgs(null, false));
							e.Handled = true;
							break;
						case Keys.Delete:
						{
							for (int i = 0; i < Items.Count; i++)
								SetItemChecked(i, e.Shift);
							e.Handled = true;
							break;
						}
					}

					base.OnKeyDown(e);
				}

				protected override void OnMouseMove(MouseEventArgs e)
				{
					base.OnMouseMove(e);
					int index = IndexFromPoint(e.Location);
					Debug.WriteLine("Mouse over item: " + (index >= 0 ? GetItemText(Items[index]) : "None"));
					if (index >= 0 && index != _curSelIndex)
					{
						_curSelIndex = index;
						SetSelected(index, true);
					}
				}

				public string GetItemSummaryText(object item)
				{
					if (item is CheckedComboBoxItem ccbi)
						return ccbi.SummaryString;

					var summaryDisplayMember = _getSummaryDisplayMember?.Invoke();
					if (summaryDisplayMember != null)
					{
						var value = ReflectionHelper.GetProperty(item, summaryDisplayMember)?.ToString();
						if (value != null)
							return value;
					}
					return GetItemText(item);
				}
			}

			// Keeps track of whether checked item(s) changed, hence the value of the CheckedComboBox as a whole changed.
			// This is simply done via maintaining the old string-representation of the value(s) and the new one and comparing them!
			public bool ValueChanged => _oldStrValue != _ccbParent.Text;

			public CustomCheckedListBox List { get; set; }

			public Dropdown(CheckedComboBox ccbParent)
			{
				_ccbParent = ccbParent;
				InitializeComponent();
				ShowInTaskbar = false;
				// Add a handler to notify our parent of ItemCheck events.
				List.ItemCheck += cclb_ItemCheck;
			}

			// Create a CustomCheckedListBox which fills up the entire form area.
			private void InitializeComponent()
			{
				List = new CustomCheckedListBox(() => _ccbParent.SummaryDisplayMember);
				SuspendLayout();
				// 
				// List
				// 
				List.BorderStyle = System.Windows.Forms.BorderStyle.None;
				List.Dock = System.Windows.Forms.DockStyle.Fill;
				List.FormattingEnabled = true;
				List.Location = new System.Drawing.Point(0, 0);
				List.Name = "List";
				List.Size = new System.Drawing.Size(47, 15);
				List.TabIndex = 0;
				// 
				// Dropdown
				// 
				AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
				AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
				BackColor = System.Drawing.SystemColors.Menu;
				ClientSize = new System.Drawing.Size(47, 16);
				ControlBox = false;
				Controls.Add(List);
				ForeColor = System.Drawing.SystemColors.ControlText;
				FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
				MinimizeBox = false;
				Name = "_ccbParent";
				StartPosition = System.Windows.Forms.FormStartPosition.Manual;
				ResumeLayout(false);
			}

			public string GetCheckedItemsStringValue()
			{
				// Turn into list to prevent a list change from causing an
				// InvalidOperationException:
				var checkedItems = List.CheckedItems.OfType<object>().ToList();
				return Join(_ccbParent.ValueSeparator, checkedItems.Select(
					i => List.GetItemSummaryText(i)));
			}

			/// <summary>
			/// Closes the dropdown portion and enacts any changes according to the specified
			/// boolean parameter.
			/// NOTE: even though the caller might ask for changes to be enacted, this doesn't
			///	   necessarily mean that any changes have occurred as such. Caller should check the
			///    ValueChanged property of the CheckedComboBox (after the dropdown has closed) to
			///    determine any actual value changes.
			/// </summary>
			/// <param name="enactChanges"></param>
			public void CloseDropdown(bool enactChanges)
			{
				if (_dropdownClosed)
					return;

				Debug.WriteLine("CloseDropdown");
				// Perform the actual selection and display of checked items.
				if (enactChanges)
				{
					_ccbParent.SelectedIndex = -1;					
					// Set the text portion equal to the string comprising all checked items (if any, otherwise empty!).
					_ccbParent.Text = GetCheckedItemsStringValue();

				}
				else
				{
					// Caller cancelled selection - need to restore the checked items to their original state.
					for (int i = 0; i < List.Items.Count; i++)
						List.SetItemChecked(i, _checkedStateArr[i]);
				}
				// From now on the dropdown is considered closed. We set the flag here to prevent OnDeactivate() calling
				// this method once again after hiding this window.
				_dropdownClosed = true;
				// See: https://www.codeproject.com/Articles/31105/A-ComboBox-with-a-CheckedListBox-as-a-Dropdown?msg=5394283#xx5394283xx
				// By invoking Hide(), it waits until after the entire event chain is resolved.
				// This prevents the form from losing focus and removes the need to call Focus().
				_ccbParent.BeginInvoke(Hide);

				// Notify CheckedComboBox that its dropdown is closed. (NOTE: it does not matter which parameters we pass to
				// OnDropDownClosed() as long as the argument is CCBoxEventArgs so that the method knows the notification has
				// come from our code and not from the framework).
				_ccbParent.OnDropDownClosed(new CCBoxEventArgs(null, false));
			}

			protected override void OnActivated(EventArgs e)
			{
				Debug.WriteLine("OnActivated");
				base.OnActivated(e);
				_dropdownClosed = false;
				// Assign the old string value to compare with the new value for any changes.
				_oldStrValue = _ccbParent.Text;
				// Make a copy of the checked state of each item, in case caller cancels selection.
				_checkedStateArr = new bool[List.Items.Count];
				for (int i = 0; i < List.Items.Count; i++)
					_checkedStateArr[i] = List.GetItemChecked(i);
			}

			protected override void OnDeactivate(EventArgs e)
			{
				Debug.WriteLine("OnDeactivate");
				base.OnDeactivate(e);
				CloseDropdown(!(e is CCBoxEventArgs ce) || ce.AssignValues);
			}

			private void cclb_ItemCheck(object sender, ItemCheckEventArgs e)
			{
				_ccbParent.ItemCheck?.Invoke(sender, e);
			}

		} // end internal class Dropdown

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private readonly System.ComponentModel.IContainer components = null;
		// A form-derived object representing the drop-down list of the checked combo box.
		private readonly Dropdown _dropdown;
		private string _summaryDisplayMember;

		// The valueSeparator character(s) between the ticked elements as they appear in the 
		// text portion of the CheckedComboBox.
		public string ValueSeparator { get; set; }

		public bool CheckOnClick
		{
			get => _dropdown.List.CheckOnClick;
			set => _dropdown.List.CheckOnClick = value;
		}

		protected override void OnDisplayMemberChanged(EventArgs e)
		{
			base.OnDisplayMemberChanged(e);
			_dropdown.List.DisplayMember = DisplayMember;
		}

		/// <summary>
		/// If items are not of type <see cref="CheckedComboBoxItem"/>, this indicates
		/// which member of the items to use for displaying the items in the summary list.
		/// By default, this will be the same as the base <see cref="ListControl.DisplayMember"/>
		/// </summary>
		public string SummaryDisplayMember
		{
			get => _summaryDisplayMember ?? DisplayMember;
			set => _summaryDisplayMember = value;
		}

		public new CheckedListBox.ObjectCollection Items => _dropdown.List.Items;

		public CheckedListBox.CheckedItemCollection CheckedItems => _dropdown.List.CheckedItems;

		public CheckedListBox.CheckedIndexCollection CheckedIndices => _dropdown.List.CheckedIndices;

		public bool ValueChanged => _dropdown.ValueChanged;

		/// <summary>
		/// Event handler for when an item check state changes.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public event ItemCheckEventHandler ItemCheck;
		
		public CheckedComboBox()
		{
			// We want to do the drawing of the dropdown.
			DrawMode = DrawMode.OwnerDrawVariable;
			// Default value separator.
			ValueSeparator = ", ";
			// This prevents the actual ComboBox dropdown to show, although it's not strickly-speaking necessary.
			// But including this remove a slight flickering just before our dropdown appears (which is caused by
			// the empty-dropdown list of the ComboBox which is displayed for fractions of a second).
			DropDownHeight = 1;			
			// This is the default setting - text portion is editable and user must click the arrow button
			// to see the list portion. Although we don't want to allow the user to edit the text portion
			// the DropDownList style is not being used because for some reason it wouldn't allow the text
			// portion to be programmatically set. Hence we set it as editable but disable keyboard input (see below).
			DropDownStyle = ComboBoxStyle.DropDown;
			_dropdown = new Dropdown(this);
			// CheckOnClick style for the dropdown (NOTE: must be set after dropdown is created).
			CheckOnClick = true;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
				components?.Dispose();
			base.Dispose(disposing);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			Text = _dropdown.GetCheckedItemsStringValue();
		}

		protected override void OnDropDown(EventArgs e)
		{
			base.OnDropDown(e);
			DoDropDown();	
		}

		private void DoDropDown()
		{
			if (!_dropdown.Visible)
			{
				var rect = RectangleToScreen(ClientRectangle);
				_dropdown.Location = new Point(rect.X, rect.Y + Size.Height);
				int count = _dropdown.List.Items.Count;
				if (count > MaxDropDownItems)
					count = MaxDropDownItems;
				else if (count == 0)
					count = 1;

				_dropdown.Size = new Size(DropDownWidth, _dropdown.List.ItemHeight * count + 2);
				_dropdown.Show(this);
			}
		}

		protected override void OnDropDownClosed(EventArgs e)
		{
			// Call the handlers for this event only if the call comes from our code - NOT the framework's!
			// NOTE: that is because the events were being fired in a wrong order, due to the actual dropdown list
			//	   of the ComboBox which lies underneath our dropdown and gets involved every time.
			if (e is Dropdown.CCBoxEventArgs)
				base.OnDropDownClosed(e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				// Signal that the dropdown is "down". This is required so that the behaviour of the dropdown is the same
				// when it is a result of user pressing the Down_Arrow (which we handle and the framework wouldn't know that
				// the list portion is down unless we tell it so).
				// NOTE: all that so the DropDownClosed event fires correctly!				
				OnDropDown(null);
			}
			// Make sure that certain keys or combinations are not blocked.
			e.Handled = !e.Alt && e.KeyCode != Keys.Tab &&
				!(e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Home || e.KeyCode == Keys.End);

			base.OnKeyDown(e);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			e.Handled = true;
			base.OnKeyPress(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			DroppedDown = false;
		}

		public bool GetItemChecked(int index)
		{
			if (index < 0 || index > Items.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			return _dropdown.List.GetItemChecked(index);
		}

		public void SetItemChecked(int index, bool isChecked)
		{
			if (index < 0 || index > Items.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			_dropdown.List.SetItemChecked(index, isChecked);
			// Need to update the Text.
			Text = _dropdown.GetCheckedItemsStringValue();
		}

		public CheckState GetItemCheckState(int index)
		{
			if (index < 0 || index > Items.Count)
				throw new ArgumentOutOfRangeException(nameof(index));
			return _dropdown.List.GetItemCheckState(index);
		}

		public void SetItemCheckState(int index, CheckState state)
		{
			if (index < 0 || index > Items.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			_dropdown.List.SetItemCheckState(index, state);
			// Need to update the Text.
			Text = _dropdown.GetCheckedItemsStringValue();
		}

	}
}
