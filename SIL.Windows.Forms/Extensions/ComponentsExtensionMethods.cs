// Copyright (c) 2011-2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Extensions
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Extension methods that can be used with components on a form
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public static class ComponentsExtensionMethods
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or creates the named component
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static T Component<T>(this IContainer components, string name)
			where T : class, IComponent, new()
		{
			if (components.Components[name] == null)
				components.Add(new T(), name);
			return components.Components[name] as T;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or creates the named context menu strip, deleting all existing menu items
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static ContextMenuStrip ContextMenuStrip(this IContainer components, string name)
		{
			return ContextMenuStrip(components, name, true);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or creates the named context menu strip, optionally deleting all existing menu
		/// items.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[SuppressMessage("Gendarme.Rules.Correctness", "EnsureLocalDisposalRule",
			Justification = "contextMenu.Items[] returns a reference")]
		public static ContextMenuStrip ContextMenuStrip(this IContainer components, string name,
			bool clear)
		{
			var contextMenu = components.Component<ContextMenuStrip>(name);
			if (clear)
			{
				for (int i = 0; i < contextMenu.Items.Count; i++)
					contextMenu.Items[i].Dispose();
				contextMenu.Items.Clear();
			}
			return contextMenu;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or creates the named context menu, deleting all existing menu items
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static ContextMenu ContextMenu(this IContainer components, string name)
		{
			return ContextMenu(components, name, true);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or creates the named context menu, optionally deleting all existing menu items
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static ContextMenu ContextMenu(this IContainer components, string name,
			bool clear)
		{
			var contextMenu = components.Component<ContextMenu>(name);
			if (clear)
				contextMenu.MenuItems.Clear();
			return contextMenu;
		}
	}
}
