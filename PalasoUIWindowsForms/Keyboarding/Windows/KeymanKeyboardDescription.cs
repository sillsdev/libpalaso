// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
#if !__MonoCS__
using System.Diagnostics.CodeAnalysis;
using Palaso.UI.WindowsForms.Keyboarding.InternalInterfaces;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.Keyboarding.Windows
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Keyboard description for a Keyman keyboard not associated with a windows language
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[SuppressMessage("Gendarme.Rules.Design", "TypesWithNativeFieldsShouldBeDisposableRule",
		Justification = "WindowHandle is a reference to a control")]
	internal class KeymanKeyboardDescription : KeyboardDescription
	{
		public bool IsKeyman6 { get; private set; }

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="T:Palaso.UI.WindowsForms.Keyboard.Windows.KeymanKeyboardDescription"/> class.
		/// </summary>
		public KeymanKeyboardDescription(string layout, bool isKeyman6, IKeyboardAdaptor engine)
			: base(engine, KeyboardType.OtherIm)
		{
			InternalName = layout;
			Layout = layout;
			IsKeyman6 = isKeyman6;
		}

		internal KeymanKeyboardDescription(KeymanKeyboardDescription other): base(other)
		{
			IsKeyman6 = other.IsKeyman6;
		}

		public override IKeyboardDefinition Clone()
		{
			return new KeymanKeyboardDescription(this);
		}
	}
}
#endif
