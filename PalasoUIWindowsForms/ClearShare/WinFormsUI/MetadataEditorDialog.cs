using System;
using System.Reflection;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.ClearShare.WinFormsUI
{
	public partial class MetadataEditorDialog : Form
	{
		private readonly Metadata _originalMetaData;
		private Metadata _returnMetaData;

		public MetadataEditorDialog(Metadata originalMetaData)
		{
			_originalMetaData = originalMetaData;
			InitializeComponent();
			metdataEditorControl1.Metadata = _returnMetaData = originalMetaData.DeepCopy();
		}

		public Metadata Metadata
		{
			get { return _returnMetaData; }
		}

		private void _cancelButton_Click(object sender, EventArgs e)
		{
			_returnMetaData = _originalMetaData;
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void _okButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}


		private void _minimallyCompleteCheckTimer_Tick(object sender, EventArgs e)
		{
			_okButton.Enabled = metdataEditorControl1.Metadata.IsMinimallyComplete;
		}
	}
}
/*    public static T CloneObject<T>(T source)
		{
			//grab the type and create a new instance of that type
			Type sourceType = source.GetType();
			T target = (T) Activator.CreateInstance(source.GetType(), null);
			//T target = Activator.CreateInstance<T>();

			//grab the properties
			PropertyInfo[] PropertyInfo =
				sourceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			//iterate over the properties and if it has a 'set' method assign it from the source TO the target
			foreach (PropertyInfo item in PropertyInfo)
			{
				if (item.CanWrite)
				{
					//value types can simply be 'set'
					if (item.PropertyType.IsValueType || item.PropertyType.IsEnum ||
						item.PropertyType.Equals(typeof (System.String)))
					{
						item.SetValue(target, item.GetValue(source, null), null);
					}
						//object/complex types need to recursively call this method until the end of the tree is reached
					else
					{
						object propertyValue = item.GetValue(source, null);
						if (propertyValue == null)
						{
							item.SetValue(target, null, null);
						}
						else
						{
							Type z = item.PropertyType;
							item.SetValue(target, CloneObject(propertyValue), null);
							//item.SetValue(target, CloneObject<LicenseInfo>(propertyValue as LicenseInfo), null);
						}
					}
				}
			}
			//return the new item
			return target;
		}*/