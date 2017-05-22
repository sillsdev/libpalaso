using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SIL.Reflection;

namespace SIL.Windows.Forms.AppColorPalette
{
	[ProvideProperty("UsePaletteColors", typeof(IComponent))]
	[ProvideProperty("ForeColor", typeof(IComponent))]
	[ProvideProperty("BackColor", typeof(IComponent))]
	[ProvideProperty("LinkColor", typeof(LinkLabel))]
	[ProvideProperty("ActiveLinkColor", typeof(LinkLabel))]
	[ProvideProperty("DisabledLinkColor", typeof(LinkLabel))]
	[ProvideProperty("VisitedLinkColor", typeof(LinkLabel))]
	[ProvideProperty("FlatAppearanceBorderColor", typeof(ButtonBase))]
	[ProvideProperty("FlatAppearanceMouseDownBackColor", typeof(ButtonBase))]
	[ProvideProperty("FlatAppearanceMouseOverBackColor", typeof(ButtonBase))]
	public abstract class AppColorPaletteExtender<TPaletteColors> : Component, IExtenderProvider, ISupportInitialize where TPaletteColors : struct, IConvertible
	{
		public enum ColorProperties
		{
			ForeColor,
			BackColor,
			LinkColor,
			ActiveLinkColor, 
			DisabledLinkColor,
			VisitedLinkColor,
			BorderColor,
			MouseDownBackColor,
			MouseOverBackColor,
		}

		private Container _components = null;
		private readonly Dictionary<object, ColorOverrideInfo> _extendedCtrls;
		private bool _initialized;

		private class ColorOverrideInfo
		{
			public ColorOverrideInfo(bool usePaletteColors)
			{
				UsePaletteColors = usePaletteColors;
			}

			public bool UsePaletteColors { get; set; }
			public TPaletteColors ForeColor { get; set; }
			public TPaletteColors BackColor { get; set; }
			public TPaletteColors LinkColor { get; set; }
			public TPaletteColors ActiveLinkColor { get; set; }
			public TPaletteColors DisabledLinkColor { get; set; }
			public TPaletteColors VisitedLinkColor { get; set; }
			public TPaletteColors FlatAppearanceBorderColor { get; set; }
			public TPaletteColors FlatAppearanceMouseDownBackColor { get; set; }
			public TPaletteColors FlatAppearanceMouseOverBackColor { get; set; }
		}

		public abstract Color GetColor(TPaletteColors paletteColor);

		protected virtual bool UsePaletteColorsForComponent(IComponent component)
		{
			return true;
		}

		protected virtual TPaletteColors GetDefaultPaletteColor(ColorProperties colorProperty)
		{
			return default(TPaletteColors);
		}

		protected AppColorPaletteExtender()
		{
			// Required for Windows.Forms Class Composition Designer support
			_components = new Container();
			_extendedCtrls = new Dictionary<object, ColorOverrideInfo>();
		}

		protected AppColorPaletteExtender(IContainer container) : this()
		{
			container.Add(this);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether to use colors from the application color palette for
		/// the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("App Palette Properties")]
		public bool GetUsePaletteColors(IComponent component)
		{
			var overrideInfo = GetComponentInfo(component);
			return overrideInfo.UsePaletteColors;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets a value indicating whether to use colors from the application color palette for
		/// the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetUsePaletteColors(IComponent component, bool usePaletteColors)
		{
			var overrideInfo = GetComponentInfo(component);
			overrideInfo.UsePaletteColors = usePaletteColors;
			if (_initialized)
			{
				if (usePaletteColors)
					ApplyColorChange(component, overrideInfo);
				else
				{
					var control = component as Control;
					if (control != null)
					{
						control.ResetForeColor();
						control.ResetBackColor();
						var linkLabel = control as LinkLabel;
						if (linkLabel != null)
						{
							linkLabel.LinkColor = Color.Empty;
							linkLabel.ActiveLinkColor = Color.Empty;
							linkLabel.DisabledLinkColor = Color.Empty;
							linkLabel.VisitedLinkColor = Color.Empty;
						}
						else
						{
							var button = control as ButtonBase;
							if (button != null)
							{
								button.FlatAppearance.BorderColor = Color.Empty;
								button.FlatAppearance.MouseDownBackColor = Color.Empty;
								button.FlatAppearance.MouseOverBackColor = Color.Empty;
							}
						}
					}
					else
					{
						var item = component as ToolStripItem;
						if (item != null)
						{
							item.ResetForeColor();
							item.ResetBackColor();
						}
					}
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the fore color of the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("App Palette Properties")]
		public TPaletteColors GetForeColor(IComponent component)
		{
			var overrideInfo = GetComponentInfo(component);
			// By default, we do not override the fore color for forms because it will affect the
			// fore color of buttons or other controls that are using visual styles.
			if (overrideInfo.ForeColor.Equals(default(TPaletteColors)) && (!(component is ScrollableControl) || component is UserControl))
				overrideInfo.ForeColor = GetDefaultPaletteColor(ColorProperties.ForeColor);
			return overrideInfo.ForeColor;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the fore color of the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetForeColor(IComponent component, TPaletteColors color)
		{
			var overrideInfo = GetComponentInfo(component);
			overrideInfo.ForeColor = color;
			if (_initialized)
				ApplyColorChange(component, overrideInfo);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the back color of the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("App Palette Properties")]
		public TPaletteColors GetBackColor(IComponent component)
		{
			var overrideInfo = GetComponentInfo(component);
			if (overrideInfo.BackColor.Equals(default(TPaletteColors)))
				overrideInfo.BackColor = GetDefaultPaletteColor(ColorProperties.BackColor);
			return overrideInfo.BackColor;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the back color of the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetBackColor(IComponent component, TPaletteColors color)
		{
			var overrideInfo = GetComponentInfo(component);
			overrideInfo.BackColor = color;
			if (_initialized)
				ApplyColorChange(component, overrideInfo);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the link color of the specified link label.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("App Palette Properties")]
		public TPaletteColors GetLinkColor(LinkLabel label)
		{
			var overrideInfo = GetComponentInfo(label);
			if (overrideInfo.LinkColor.Equals(default(TPaletteColors)))
				overrideInfo.LinkColor = GetDefaultPaletteColor(ColorProperties.LinkColor);
			return overrideInfo.LinkColor;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the link color of the specified link label.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetLinkColor(LinkLabel label, TPaletteColors color)
		{
			var overrideInfo = GetComponentInfo(label);
			overrideInfo.LinkColor = color;
			if (_initialized)
				ApplyColorChange(label, overrideInfo);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the active link color of the specified link label.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("App Palette Properties")]
		public TPaletteColors GetActiveLinkColor(LinkLabel label)
		{
			var overrideInfo = GetComponentInfo(label);
			if (overrideInfo.ActiveLinkColor.Equals(default(TPaletteColors)))
				overrideInfo.ActiveLinkColor = GetDefaultPaletteColor(ColorProperties.ActiveLinkColor);
			return overrideInfo.ActiveLinkColor;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the active link color of the specified link label.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetActiveLinkColor(LinkLabel label, TPaletteColors color)
		{
			var overrideInfo = GetComponentInfo(label);
			overrideInfo.ActiveLinkColor = color;
			if (_initialized)
				ApplyColorChange(label, overrideInfo);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the disabled link color of the specified link label.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("App Palette Properties")]
		public TPaletteColors GetDisabledLinkColor(LinkLabel label)
		{
			var overrideInfo = GetComponentInfo(label);
			if (overrideInfo.DisabledLinkColor.Equals(default(TPaletteColors)))
				overrideInfo.DisabledLinkColor = GetDefaultPaletteColor(ColorProperties.DisabledLinkColor);
			return overrideInfo.DisabledLinkColor;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the disabled link color of the specified link label.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetDisabledLinkColor(LinkLabel label, TPaletteColors color)
		{
			var overrideInfo = GetComponentInfo(label);
			overrideInfo.DisabledLinkColor = color;
			if (_initialized)
				ApplyColorChange(label, overrideInfo);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the visited link color of the specified link label.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("App Palette Properties")]
		public TPaletteColors GetVisitedLinkColor(LinkLabel label)
		{
			var overrideInfo = GetComponentInfo(label);
			if (overrideInfo.VisitedLinkColor.Equals(default(TPaletteColors)))
				overrideInfo.VisitedLinkColor = GetDefaultPaletteColor(ColorProperties.VisitedLinkColor);
			return overrideInfo.VisitedLinkColor;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the visited link color of the specified link label.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetVisitedLinkColor(LinkLabel label, TPaletteColors color)
		{
			var overrideInfo = GetComponentInfo(label);
			overrideInfo.VisitedLinkColor = color;
			if (_initialized)
				ApplyColorChange(label, overrideInfo);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the border color of a button that uses "flat" appearance (ignored if button
		/// uses "VisualStyles").
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Localizable(false)]
		[Category("App Palette Properties")]
		public TPaletteColors GetFlatAppearanceBorderColor(ButtonBase button)
		{
			var overrideInfo = GetComponentInfo(button);
			if (overrideInfo.FlatAppearanceBorderColor.Equals(default(TPaletteColors)))
				overrideInfo.FlatAppearanceBorderColor = GetDefaultPaletteColor(ColorProperties.BorderColor);
			return overrideInfo.FlatAppearanceBorderColor;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the border color of a button that uses "flat" appearance (ignored if button
		/// uses "VisualStyles").
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetFlatAppearanceBorderColor(ButtonBase button, TPaletteColors color)
		{
			var overrideInfo = GetComponentInfo(button);
			overrideInfo.FlatAppearanceBorderColor = color;
			if (_initialized)
				ApplyColorChange(button, overrideInfo);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the color object information for the specified component.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private ColorOverrideInfo GetComponentInfo(IComponent component)
		{
			ColorOverrideInfo coi;
			if (!_extendedCtrls.TryGetValue(component, out coi))
				_extendedCtrls[component] = coi = new ColorOverrideInfo(UsePaletteColorsForComponent(component));
			return coi;
		}

		public bool CanExtend(object extendee)
		{
			return (extendee is Control || extendee is ToolStripItem);
		}

		public void BeginInit()
		{
			_initialized = false;
		}

		public void EndInit()
		{
			foreach (var componentOverrideInfo in _extendedCtrls)
			{
				ApplyColorChange(componentOverrideInfo.Key, componentOverrideInfo.Value);
			}
			_initialized = true;
		}

		private void ApplyColorChange(object component, ColorOverrideInfo info)
		{
			var control = component as Control;
			if (control != null)
			{
				bool resetUseVisualStyleBackColor = false;
				try
				{
					var useVisualStyleBackColorProperty = ReflectionHelper.GetProperty(control, "UseVisualStyleBackColor");
					if (useVisualStyleBackColorProperty != null)
						resetUseVisualStyleBackColor = (bool)useVisualStyleBackColorProperty;
				}
				catch (InvalidCastException)
				{
				}
				catch (MissingMethodException)
				{
				}
				if (info.UsePaletteColors)
				{
					if (!info.ForeColor.Equals(default(TPaletteColors)))
						control.ForeColor = GetColor(info.ForeColor);
					if (!info.BackColor.Equals(default(TPaletteColors)))
						control.BackColor = GetColor(info.BackColor);
					var linkLabel = control as LinkLabel;
					if (linkLabel != null)
					{
						if (!info.LinkColor.Equals(default(TPaletteColors)))
							linkLabel.LinkColor = GetColor(info.LinkColor);
						if (!info.ActiveLinkColor.Equals(default(TPaletteColors)))
							linkLabel.ActiveLinkColor = GetColor(info.ActiveLinkColor);
						if (!info.DisabledLinkColor.Equals(default(TPaletteColors)))
							linkLabel.DisabledLinkColor = GetColor(info.DisabledLinkColor);
						if (!info.VisitedLinkColor.Equals(default(TPaletteColors)))
							linkLabel.VisitedLinkColor = GetColor(info.VisitedLinkColor);
					}
					else
					{
						var button = control as ButtonBase;
						if (button != null)
						{
							if (!info.FlatAppearanceBorderColor.Equals(default(TPaletteColors)))
								button.FlatAppearance.BorderColor = GetColor(info.FlatAppearanceBorderColor);
							if (!info.FlatAppearanceMouseDownBackColor.Equals(default(TPaletteColors)))
								button.FlatAppearance.MouseDownBackColor = GetColor(info.FlatAppearanceMouseDownBackColor);
							if (!info.FlatAppearanceMouseOverBackColor.Equals(default(TPaletteColors)))
								button.FlatAppearance.MouseOverBackColor = GetColor(info.FlatAppearanceMouseOverBackColor);
						}
					}
				}
				if (resetUseVisualStyleBackColor)
					ReflectionHelper.SetProperty(control, "UseVisualStyleBackColor",  true);
			}
			else
			{
				var item = component as ToolStripItem;
				if (item != null)
				{
					if (info.UsePaletteColors)
					{
						if (!info.ForeColor.Equals(default(TPaletteColors)))
							item.ForeColor = GetColor(info.ForeColor);
						if (!info.BackColor.Equals(default(TPaletteColors)))
							item.BackColor = GetColor(info.BackColor);
					}
				}
			}
		}

		private void InitializeComponent()
		{

		}
	}
}
