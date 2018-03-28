// Copyright (c) 2013-2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
namespace SIL.Windows.Forms.Keyboarding.Windows
{
	/// <summary/>
	internal class LayoutName
	{
		public LayoutName()
		{
			Name = string.Empty;
			LocalizedName = string.Empty;
		}

		public LayoutName(string layout) : this(layout, layout)
		{
		}

		public LayoutName(string layout, string localizedLayout)
		{
			Name = layout;
			LocalizedName = localizedLayout;
		}

		public string Name;
		public string LocalizedName;
	}
}