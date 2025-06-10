// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Drawing;
using System.Windows.Forms;
using SIL.Windows.Forms.ImageToolbox;

namespace SIL.Windows.Forms.Clipboarding
{
	internal interface IClipboard
	{
		bool ContainsText();
		string GetText();
		string GetText(TextDataFormat format);
		void SetText(string text);
		void SetText(string text, TextDataFormat format);
		bool ContainsImage();

		bool CanGetImage();
		Image GetImage();
		void CopyImageToClipboard(PalasoImage image);
		PalasoImage GetImageFromClipboard();
	}
}