using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems.WSTree
{
	public  class WritingSystemTreeItem
	{
		protected readonly Action<WritingSystemTreeItem> ClickAction;
		protected static Font kLabelFont=new Font(SystemFonts.MessageBoxFont.Name, 8);
		protected static Font kHeaderFont=new Font(SystemFonts.MessageBoxFont.Name, 8);
		public WritingSystemTreeItem(string text, Action<WritingSystemTreeItem> clickAction)
		{
			Children = new List<WritingSystemTreeItem>();
			ClickAction = clickAction;
			Text=text;
		}

		public bool Selected { get; set; }
		public string Text { get; set; }

		public List<WritingSystemTreeItem> Children { get; set; }


		public TreeNode MakeTreeNode()
		{
			var node = new TreeNode(Text, Children.Select(n => n.MakeTreeNode()).ToArray());
			node.Tag=this;
			node.ForeColor = ForeColor;
			node.NodeFont = this.Font;
			return node;
		}

		protected virtual Color ForeColor
		{
			get { return System.Drawing.Color.Black; }
		}
		protected virtual Font Font
		{
			get { return kHeaderFont; }
		}

		public virtual bool CanSelect
		{
			get { return false; }
		}

		public virtual void Clicked()
		{
			if (ClickAction != null)
			{
				ClickAction(this);
			}
		}

	}


	public class NullTreeItem : WritingSystemTreeItem
	{
		public NullTreeItem()
			: base(string.Empty, new Action<WritingSystemTreeItem>(x => { }))
		{
		}


	}


	/// <summary>
	/// this is used when it would be confusing to make one of the WS's primary above the others.
	/// related to http://projects.palaso.org/issues/show/482
	/// </summary>
	public class GroupTreeItem : WritingSystemTreeItem
	{
		protected static Font kFont = new Font(SystemFonts.MessageBoxFont.Name, 11);

		public GroupTreeItem(string name)
			: base(name, new Action<WritingSystemTreeItem>(x => { }))
		{
		}

		protected override Font Font
		{
			get
			{
				return kFont;
			}
		}

		protected override Color ForeColor
		{
			get
			{
				return Color.DarkGray;
			}
		}


	}

	public class WritingSystemDefinitionTreeItem : WritingSystemTreeItem
	{
		public WritingSystemDefinition Definition { get; set; }
		protected static Font kExistingItemFont = new Font(SystemFonts.MessageBoxFont.Name, 11);

		public WritingSystemDefinitionTreeItem(WritingSystemDefinition definition, Action<WritingSystemTreeItem> clickAction)
			: base(definition.ListLabel, clickAction)
		{
			Definition = definition;
		}

		protected WritingSystemDefinitionTreeItem(Action<WritingSystemTreeItem> clickAction)
			: base("label", clickAction)
		{
		}

		protected override Font Font
		{
			get { return kExistingItemFont; }
		}
		public override bool CanSelect
		{
			get { return true; }
		}
	}

	public class WritingSystemCreationTreeItem : WritingSystemDefinitionTreeItem
	{
		private readonly IWritingSystemDefinitionSuggestion _suggestion;

		public WritingSystemCreationTreeItem(IWritingSystemDefinitionSuggestion suggestion, Action<WritingSystemTreeItem> clickAction)
			: base(clickAction)
		{
			_suggestion = suggestion;
			Text = "Add " + suggestion.Label;
		}

		protected override Color ForeColor
		{
			get { return System.Drawing.Color.DarkBlue; }
		}

		public override bool CanSelect
		{
			get { return true; }
		}

		protected override Font Font
		{
			get { return kLabelFont; }
		}

		public WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			return _suggestion.ShowDialogIfNeededAndGetDefinition();
		}
	}

	public class WritingSystemCreateUnknownTreeItem : WritingSystemTreeItem
	{

		public WritingSystemCreateUnknownTreeItem(Action<WritingSystemTreeItem> clickAction)
			: base("Add Language", clickAction)
		{
		}

		protected override Color ForeColor
		{
			get { return System.Drawing.Color.DarkBlue; }
		}

		protected override Font Font
		{
			get { return kLabelFont; }
		}
		public override bool CanSelect
		{
			get { return true; }
		}
	}

	public class WritingSystemRenameUnlistedLanguageTreeItem : WritingSystemTreeItem
	{
		public WritingSystemRenameUnlistedLanguageTreeItem(Action<WritingSystemTreeItem> clickAction)
			: base("Change to Listed Language", clickAction)
		{
		}

		protected override Color ForeColor
		{
			get { return System.Drawing.Color.DarkBlue; }
		}

		protected override Font Font
		{
			get { return kLabelFont; }
		}
		public override bool CanSelect
		{
			get { return true; }
		}
	}
}