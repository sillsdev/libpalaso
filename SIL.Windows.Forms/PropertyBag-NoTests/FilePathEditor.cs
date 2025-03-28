using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace SIL.Windows.Forms
{
	public class PathForPropertyGrid
	{
		public string Path;
		public string Filter = "All | *.*";

		public PathForPropertyGrid(string path, string filter)
		{
			Path = path;
			Filter = filter;
		}

		public override string ToString()
		{
			return GetTruncatedPath(Path);
		}

		public PathForPropertyGrid Clone()
		{
			return new PathForPropertyGrid(Path, Filter);
		}

		public string GetTruncatedPath(string path)
		{
			System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);
			if (dir != null)
			{
				string directParent = string.Empty;
				string parentParent = string.Empty;
				string ellipsis = "...";
				System.IO.DirectoryInfo p = dir.Parent;
				if ((p != null) && (p.Name != dir.Root.Name))
				{
					directParent = p.Name + @"\";
					System.IO.DirectoryInfo pp = p.Parent;
					if ((pp != null) && (pp.Name != dir.Root.Name))
					{
						parentParent = pp.Name + @"\";
					}
					else
					{
						ellipsis = string.Empty;
					}
				}
				else
				{
					ellipsis = string.Empty;
				}
				return dir.Root.Name + ellipsis + parentParent + directParent + dir.Name;
			}
			return path;
		}
	}
	public class FilePathEditor : UITypeEditor
	{

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			PathForPropertyGrid pathWrapper = value as PathForPropertyGrid;

			System.Windows.Forms.OpenFileDialog dialog = new OpenFileDialog();
			dialog.FileName = pathWrapper.Path;

			dialog.Filter = pathWrapper.Filter;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				//have to make a new object, or the propertyGrid will think we didn't change anything
				PathForPropertyGrid p = pathWrapper.Clone();
				p.Path = dialog.FileName;
				return p;
			}
			else
			{
				return value;
			}
		}


	}

}
