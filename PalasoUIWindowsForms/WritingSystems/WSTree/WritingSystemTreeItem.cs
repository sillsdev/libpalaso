using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public  class WritingSystemTreeItem
	{
		protected readonly Action<WritingSystemTreeItem> _clickAction;
		protected static Font kLabelFont=new Font(SystemFonts.MessageBoxFont.Name, 8);
		protected static Font kHeaderFont=new Font(SystemFonts.MessageBoxFont.Name, 8);
		public WritingSystemTreeItem(string text, Action<WritingSystemTreeItem> clickAction)
		{
			Children = new List<WritingSystemTreeItem>();
			_clickAction = clickAction;
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
			if (_clickAction != null)
			{
				_clickAction(this);
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
	public class WritingSystemDefinitionTreeItem : WritingSystemTreeItem
	{
		public WritingSystemDefinition Definition { get; set; }
		protected static Font kExistingItemFont = new Font(SystemFonts.MessageBoxFont.Name, 11);

		public WritingSystemDefinitionTreeItem(WritingSystemDefinition definition, Action<WritingSystemTreeItem> clickAction)
			: base(definition.ListLabel, clickAction)
		{
			Definition = definition;
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

		public WritingSystemCreationTreeItem(WritingSystemDefinition definition, Action<WritingSystemTreeItem> clickAction)
			: base(definition, clickAction)
		{
			Text = "Add " + definition.ListLabel;
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
}