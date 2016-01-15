using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SIL.Windows.Forms.AppColorPalette
{
	[ProvideProperty("ForeColor", typeof(IComponent))]
	public class AppColorPaletteExtender : Component, IExtenderProvider, ISupportInitialize
	{
		private Container _components = null;
		private readonly Dictionary<object, ColorOverrideInfo> _extendedCtrls;
		private readonly Dictionary<PaletteColors, Color> _appColorPalette = new Dictionary<PaletteColors, Color> {{PaletteColors.Color1, Color.AliceBlue}, {PaletteColors.Color2, Color.BlueViolet}};

		public enum PaletteColors
		{
			Color1,
			Color2,
		}

		private class ColorOverrideInfo
		{
			public PaletteColors ForeColor { get; set; }
		}

		public virtual Dictionary<PaletteColors, Color> AppColorPalette
		{
			get { return _appColorPalette; }
		}

		public AppColorPaletteExtender()
		{
			// Required for Windows.Forms Class Composition Designer support
			_components = new Container();

			_extendedCtrls = new Dictionary<object, ColorOverrideInfo>();
		}

		public AppColorPaletteExtender(IContainer container) : this()
		{
			container.Add(this);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the fore color of the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("App Palette Properties")]
		[DefaultValue(PaletteColors.Color1)]
		public PaletteColors GetForeColor(IComponent component)
		{
			return GetComponentInfo(component).ForeColor;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the fore color of the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetForeColor(IComponent component, PaletteColors color)
		{
			GetComponentInfo(component).ForeColor = color;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the color object information for the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private ColorOverrideInfo GetComponentInfo(IComponent component)
		{
			ColorOverrideInfo coi;
			if (_extendedCtrls.TryGetValue(component, out coi))
				return coi;

			coi = new ColorOverrideInfo();
			_extendedCtrls[component] = coi;
			return coi;
		}

		public bool CanExtend(object extendee)
		{
			return (extendee is Control || extendee is ToolStripItem);
		}

		public void BeginInit()
		{
		}

		public void EndInit()
		{
			try
			{
				foreach (var colorOverrideInfo in _extendedCtrls)
				{
					var control = colorOverrideInfo.Key as Control;
					if (control != null)
					{
						control.ForeColor = AppColorPalette[colorOverrideInfo.Value.ForeColor];
					}
					else
					{
						var item = colorOverrideInfo.Key as ToolStripItem;
						if (item != null)
						{
							item.ForeColor = AppColorPalette[colorOverrideInfo.Value.ForeColor];
						}
					}
				}
			}
			catch (Exception)
			{
#if DEBUG
				throw;
#endif
			}
		}
	}
}
