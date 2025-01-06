using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using SIL.Windows.Forms.Properties;

namespace SIL.Windows.Forms.Widgets
{
	public enum SilLogoVariant
	{
		Random,
		[PublicAPI]
		Abbysinica,
		[PublicAPI]
		AndikaV1,
		[PublicAPI]
		AndikaV2,
		[PublicAPI]
		Annapurna,
		[PublicAPI]
		TaiHeritage,
	}

	public static class SilResources
	{

		private static readonly (string Name, Bitmap Logo)[] s_allLogos;

		static SilResources()
		{
			s_allLogos = new[]
			{
				("Abbysinica", Resources.SIL_Glyph_Logo_Color___Abbysinica_RGB),
				("AndikaV1", Resources.SIL_Glyph_Logo_Color___Andika_v1_RGB),
				("AndikaV2", Resources.SIL_Glyph_Logo_Color___Andika_v2_RGB),
				("Annapurna", Resources.SIL_Glyph_Logo_Color___Annapurna_RGB),
				("TaiHeritage", Resources.SIL_Glyph_Logo_Color___Tai_Heritage_Pro_RGB)
			};
		}

		[PublicAPI]
		public static IEnumerable<(string Name, Bitmap Logo)> AllLogoVariants => s_allLogos;

		[PublicAPI]
		public static Bitmap GetLogo(SilLogoVariant variant) =>
			variant == SilLogoVariant.Random ? SilLogoRandom : s_allLogos[(int)variant].Logo;

		[PublicAPI]
		public static Bitmap SilLogoRandom => s_allLogos[new Random().Next(s_allLogos.Length)].Logo;
	}
}