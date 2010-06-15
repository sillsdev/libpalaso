using System;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms
{
	/// <summary>
	/// Convenience extensions for controls
	/// </summary>
	public static class ControlExtensions
	{
		/// <summary>
		/// If the method might be in a different thread, this will do an invoke,
		/// otherwise it just invokes the action
		/// </summary>
		/// <example>InvokeIfRequired(()=>BackgroundColor=Color.Blue);</example>
		/// <example>((Control)this).InvokeIfRequired(()=>SetChoices(languages));</example>
		public static void InvokeIfRequired(this Control control, Action action)
		{
			if (control.InvokeRequired)
			{
				control.Invoke(action);
			}
			else
			{
				action();
			}
		}
	}
}
