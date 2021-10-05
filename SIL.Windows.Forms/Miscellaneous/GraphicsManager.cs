// Copyright (c) 2021 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using JetBrains.Annotations;

namespace SIL.Windows.Forms.Miscellaneous
{
	[PublicAPI]
	public static class GraphicsManager
	{
		public enum GtkVersion
		{
			Gtk2,
			Gtk3
		}

		public const GtkVersion GTK2 = GtkVersion.Gtk2;
		public const GtkVersion GTK3 = GtkVersion.Gtk3;

		// NOTE: just selecting GTK3 usually isn't enough to make things work. System.Windows.Forms
		// still uses GTK2 and runs a GTK2 message loop. To do anything with GTK3 (e.g.
		// copy to/from the clipboard) you'll have to make sure that a GTK3 message loop is
		// running.

		public static GtkVersion GtkVersionInUse { get; set; } = GTK2;
	}
}
