// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2013, SIL International. All Rights Reserved.
// <copyright from='2013' to='2013' company='SIL International'>
//		Copyright (c) 2013, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
// ---------------------------------------------------------------------------------------------
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Palaso.UI.WindowsForms.Keyboarding.Types
{
	public static class Win32
	{
		#region Imm32.dll

		/// <summary>
		/// These values are used with the ImmGetConversionStatus and ImmSetConversionStatus functions.
		/// </summary>
		[Flags]
		public enum IME_CMODE : uint
		{
			/// <summary>Alphanumeric input mode. This is the default.</summary>
			ALPHANUMERIC = 0x0000,
			/// <summary>Set to 1 if NATIVE mode; 0 if ALPHANUMERIC mode.</summary>
			NATIVE = 0x0001,
			/// <summary>Set to 1 if KATAKANA mode; 0 if HIRAGANA mode.</summary>
			KATAKANA = 0x0002,  // only effect under IME_CMODE_NATIVE
			/// <summary>Set to 1 if full shape mode; 0 if half shape mode.</summary>
			FULLSHAPE = 0x0008,
			/// <summary>Set to 1 if ROMAN input mode; 0 if not.</summary>
			ROMAN = 0x0010,
			/// <summary>Set to 1 if character code input mode; 0 if not.</summary>
			CHARCODE = 0x0020,
			/// <summary>Set to 1 if HANJA convert mode; 0 if not.</summary>
			HANJACONVERT = 0x0040,
			/// <summary>Set to 1 if Soft Keyboard mode; 0 if not.</summary>
			SOFTKBD = 0x0080,
			/// <summary>Set to 1 to prevent processing of conversions by IME; 0 if not.</summary>
			NOCONVERSION = 0x0100,
			/// <summary>Set to 1 if EUDC conversion mode; 0 if not.</summary>
			EUDC = 0x0200,
			/// <summary>Set to 1 if SYMBOL conversion mode; 0 if not.</summary>
			SYMBOL = 0x0400,
			/// <summary>Set to 1 if fixed conversion mode; 0 if not.</summary>
			FIXED = 0x0800,
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Determines if the specified input locale has an IME.
		/// </summary>
		/// <param name="hKL">Input locale identifier.</param>
		/// <returns>Returns a <c>true</c> if the specified locale has an IME, or <c>false</c>
		/// otherwise.</returns>
		/// ------------------------------------------------------------------------------------
		[DllImport("imm32.dll", CharSet = CharSet.Auto)]
		public static extern bool ImmIsIME(HandleRef hKL);

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Retrieve the input context associated with the specified window.
		/// </summary>
		/// <param name="hWnd">The window handle.</param>
		/// <returns>Returns the handle to the input context.</returns>
		/// <remarks>An application should routinely use this function to retrieve the current
		/// input context before attempting to access information in the context.
		/// The application must call ImmReleaseContext when it is finished with the input
		/// context.</remarks>
		/// ------------------------------------------------------------------------------------
		[DllImport("imm32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr ImmGetContext(HandleRef hWnd);

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Releases the input context and unlocks the memory associated in the input context.
		/// An application must call this function for each call to the ImmGetContext function.
		/// </summary>
		/// <param name="hWnd">Handle to the window for which the input context was previously
		/// retrieved.</param>
		/// <param name="hIMC">Handle to the input context.</param>
		/// <returns>Returns <c>true</c> if successful, otherwise <c>false</c>.</returns>
		/// ------------------------------------------------------------------------------------
		[DllImport("imm32.dll", CharSet = CharSet.Auto)]
		public static extern bool ImmReleaseContext(HandleRef hWnd, HandleRef hIMC);

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Retrieves the current conversion status.
		/// </summary>
		/// <param name="context">Handle to the input context for which to retrieve information.
		/// </param>
		/// <param name="conversionMode">A combination of conversion mode values.</param>
		/// <param name="sentenceMode">The sentence mode value.</param>
		/// <returns><c>true</c> if the method succeeded, otherwise <c>false</c>.</returns>
		/// ------------------------------------------------------------------------------------
		[DllImport("imm32.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ImmGetConversionStatus(HandleRef context, out int conversionMode,
			out int sentenceMode);

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the current conversion status.
		/// </summary>
		/// <param name="context">Handle to the input context for which to retrieve information.
		/// </param>
		/// <param name="conversionMode">A combination of conversion mode values.</param>
		/// <param name="sentenceMode">The sentence mode value.</param>
		/// <returns><c>true</c> if the method succeeded, otherwise <c>false</c>.</returns>
		/// ------------------------------------------------------------------------------------
		[DllImport("imm32.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ImmSetConversionStatus(HandleRef context, int conversionMode,
			int sentenceMode);
		#endregion

		#region user32.dll
#if !__MonoCS__
		/// <summary></summary>
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetFocus();
#else
		private static MethodInfo s_getFocus;
		public static IntPtr GetFocus()
		{
			if (s_getFocus == null)
				s_getFocus = XplatUI.GetMethod("GetFocus",
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
					null, Type.EmptyTypes, null);
			return (IntPtr)s_getFocus.Invoke(null, null);

		}
#endif
		#endregion

		#if __MonoCS__
		private static Assembly s_monoWinFormsAssembly;
		private static Assembly MonoWinFormsAssembly
		{
			get
			{
				if (s_monoWinFormsAssembly == null)
				{
#pragma warning disable 0612 // Using Obsolete method LoadWithPartialName.
					s_monoWinFormsAssembly = Assembly.LoadWithPartialName("System.Windows.Forms");
#pragma warning restore 0612
				}
				return s_monoWinFormsAssembly;
			}
		}

		// internal mono WinForms type
		private static Type s_xplatUIType;
		private static Type XplatUI
		{
			get
			{
				if (s_xplatUIType == null)
					s_xplatUIType = MonoWinFormsAssembly.GetType("System.Windows.Forms.XplatUI");
				return s_xplatUIType;
			}
		}
		#endif
	}
}
