// Copyright (c) 2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using IBusDotNet;

namespace SIL.Windows.Forms.Keyboarding.Linux
{
	internal class XkbIbusEngineDesc: IBusEngineDesc
	{
		public string Name { get; set; }
		public string LongName { get; set; }
		public string Description { get; set; }
		public string Language { get; set; }
		public string License { get; set; }
		public string Author { get; set; }
		public string Icon { get; set; }
		public string Layout { get; set; }
		public string Hotkeys { get; set; }
		public uint Rank { get; set; }
		public string Symbol { get; set; }
		public string Setup { get; set; }
		public string LayoutVariant { get; set; }
		public string LayoutOption { get; set; }
		public string Version { get; set; }
		public string TextDomain { get; set; }
	}
}