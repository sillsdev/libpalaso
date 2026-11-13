using System;
using System.Drawing;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Widgets
{
	/// <summary>
	/// A Button that, when its text doesn't fit within MaximumSize.Width, wraps onto
	/// additional lines and grows taller instead of growing wider or clipping. Unlike
	/// Label, Button's own AutoSize ignores MaximumSize when deciding how big it needs to
	/// be: it just measures the text as a single line and grows to fit, so a long string
	/// (for example, after localization) pushes the button wider instead of wrapping.
	/// This class takes over sizing to fix that.
	/// </summary>
	public class WrappingButton : Button
	{
		private bool _originalSizeCaptured;
		private bool _wasAutoSized;
		private Size _originalSize;
		private bool _isResizing;

		public override string Text
		{
			get => base.Text;
			set
			{
				base.Text = value;
				ResizeToFitText();
			}
		}

		public override Font Font
		{
			get => base.Font;
			set
			{
				base.Font = value;
				ResizeToFitText();
			}
		}

		public override Size MaximumSize
		{
			get => base.MaximumSize;
			set
			{
				base.MaximumSize = value;
				ResizeToFitText();
			}
		}

		public override Size MinimumSize
		{
			get => base.MinimumSize;
			set
			{
				base.MinimumSize = value;
				ResizeToFitText();
			}
		}

		/// <summary>
		/// Recomputes the button's size for its current text, wrapping onto additional
		/// lines (growing taller) instead of growing wider or clipping if the text doesn't
		/// fit within MaximumSize.Width. Called automatically whenever Text, Font,
		/// MaximumSize, or MinimumSize changes. Does nothing if MaximumSize.Width hasn't been
		/// set (0 means "no maximum").
		/// </summary>
		public void ResizeToFitText()
		{
			// MaximumSize is overridden above, and we temporarily clear/restore it below to
			// get GetPreferredSize's unclamped measurement - each of those writes would
			// otherwise re-enter this method and repeat the same clear/restore, recursing
			// forever. Make re-entrant calls (and any other re-entrant trigger, e.g. Text or
			// Font changing as a side effect of resizing) no-ops.
			if (_isResizing)
				return;
			_isResizing = true;
			try
			{
				ResizeToFitTextCore();
			}
			finally
			{
				_isResizing = false;
			}
		}

		private void ResizeToFitTextCore()
		{
			if (!_originalSizeCaptured)
			{
				// AutoSize defaults to GrowOnly: a button that was auto-sized should never
				// shrink below the size it would naturally have had. Capture that size (and
				// whether AutoSize was even on) before we take over, the first time we're
				// asked to resize - by then, Designer-generated code will already have set
				// both, so this reflects its original intent.
				_wasAutoSized = AutoSize;
				_originalSize = Size;
				AutoSize = false;
				_originalSizeCaptured = true;
			}

			var maxWidth = MaximumSize.Width;
			if (maxWidth <= 0)
				return;

			// The button's own preferred-size calculation already knows this control's/
			// theme's border and padding correctly for single-line text, so there's no need
			// to hard-code any padding/border "chrome" value for that case. GetPreferredSize
			// clamps its result to MaximumSize internally, though, so it can never actually
			// report a width wider than our cap; temporarily lift the cap to learn the
			// button's true, unclamped single-line size, which is what tells us whether it
			// would overflow and needs to wrap.
			var savedMaxSize = MaximumSize;
			MaximumSize = Size.Empty;
			var preferredSize = GetPreferredSize(Size.Empty);
			MaximumSize = savedMaxSize;

			Size neededSize;
			if (preferredSize.Width <= maxWidth)
			{
				// Fits on one line within the max width: this is exactly what AutoSize
				// would have produced.
				neededSize = preferredSize;
			}
			else
			{
				// Doesn't fit on one line: derive the chrome for the current text (the
				// difference between the button's own preferred size and the text's raw,
				// unwrapped measurement) instead of hard-coding it, then apply that same
				// chrome to a word-wrapped measurement constrained to the max width, so the
				// button grows taller instead of wider.
				var rawTextSize = TextRenderer.MeasureText(Text, Font);
				var chromeWidth = preferredSize.Width - rawTextSize.Width;
				var chromeHeight = preferredSize.Height - rawTextSize.Height;
				var wrappedTextSize = TextRenderer.MeasureText(Text, Font,
					new Size(maxWidth - chromeWidth, int.MaxValue), TextFormatFlags.WordBreak);
				neededSize = new Size(wrappedTextSize.Width + chromeWidth,
					wrappedTextSize.Height + chromeHeight);
			}

			// Always respect the explicit MinimumSize floor and the max width ceiling, on
			// top of the GrowOnly floor captured above.
			var minWidth = Math.Max(MinimumSize.Width, _wasAutoSized ? _originalSize.Width : 0);
			var minHeight = Math.Max(MinimumSize.Height, _wasAutoSized ? _originalSize.Height : 0);
			Size = new Size(
				Math.Min(maxWidth, Math.Max(minWidth, neededSize.Width)),
				Math.Max(minHeight, neededSize.Height));
		}
	}
}
