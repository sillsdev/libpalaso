using System.Windows.Forms;

namespace SIL.Windows.Forms.Extensions
{
	public static class KeysExtensions
	{
		/// <summary>
		/// Determines whether the specified key is considered a navigation key.
		/// </summary>
		/// <param name="key">The key (or key-combination) to examine.</param>
		/// <param name="ctrlPressed">If <c>true</c>, the letter <c>Keys.A</c> will be treated as a
		/// navigation key (even if the <paramref name="key"/> value does not explicitly include
		/// <c>Keys.Control</c>). In contexts where Ctrl-A is not a shortcut for Select All or that
		/// behavior is not relevant, pass false or omit this optional parameter.</param>
		/// <remarks>If <paramref name="key"/> represents the combination Ctrl-A (i.e.,
		/// <c>Keys.Control | Keys.A</c>), but <paramref name="ctrlPressed"/> is <c>false</c>,
		/// this method will return <c>false</c>.</remarks>
		public static bool IsNavigationKey(this Keys key, bool ctrlPressed = false)
		{
			switch (key & Keys.KeyCode)
			{
				case Keys.Left:
				case Keys.Up:
				case Keys.Down:
				case Keys.Right:
				case Keys.Home:
				case Keys.End:
				case Keys.PageDown:
				case Keys.PageUp:
					return true;
				case Keys.A:
					return ctrlPressed;
				default:
					return false;
			}
		}
	}

}
