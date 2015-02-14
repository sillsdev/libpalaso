using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using SIL.WindowsForms.i18n;

namespace PalsoUIWindowsFormsDesign
{
	/// <summary>
	///   Designer object used to set the Parent property.
	/// </summary>
	internal class LocalizationHelperDesigner : ComponentDesigner
	{
		///   <summary>
		///   Sets the Parent property to "this" -
		///   the Form/UserControl where the component is being dropped.
		///   </summary>
		[Obsolete]
		public override void OnSetComponentDefaults()
		{
			LocalizationHelper rp = (LocalizationHelper)Component;
			rp.Parent = (Control)Component.Site.Container.Components[0];
		}
	}
}
