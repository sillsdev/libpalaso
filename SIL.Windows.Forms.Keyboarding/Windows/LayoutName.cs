// Copyright (c) 2013-2024, SIL Global
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
			Profile = default(TfInputProcessorProfile);
		}

		public LayoutName(string layout) : this(layout, layout)
		{
		}

		public LayoutName(string layout, string localizedLayout, TfInputProcessorProfile profile = default(TfInputProcessorProfile))
		{
			Name = layout;
			LocalizedName = localizedLayout;
			Profile = profile;
		}

		public string Name;
		public string LocalizedName;
		public TfInputProcessorProfile Profile;

	}
}