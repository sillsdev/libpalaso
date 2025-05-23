using System.Windows.Forms;

namespace SIL.Windows.Forms.Extensions
{
	public static class KeysExtensions
	{
		/// <summary>
		/// Determines if the key is a navigation key.
		/// </summary>
		/// <param name="key">The key to examine</param>
		/// <param name="ctrlPressed">If <c>true</c>, the letter `A` will be treated as a
		/// navigation key. In contexts where Ctrl-A is not a shortcut for Select All or
		/// that behavior is not relevant, pass false or omit this optional parameter.</param>
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
