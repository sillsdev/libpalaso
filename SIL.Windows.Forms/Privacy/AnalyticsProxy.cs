using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Win32;
using SIL.Core.Desktop.Privacy;

namespace SIL.Windows.Forms.Privacy
{
	/// <summary>
	/// An analytics implementation that saves settings in the Windows registry.
	/// </summary>
	[PublicAPI]
	public class AnalyticsProxy : IAnalytics
	{
		private const string kRegistryValueName = "Enabled";

		public event EventHandler<AllowTrackingChangedEventArgs> AllowTrackingChanged;

		private string _productRegistryKeyId;
		public string ProductName { get; }

		/// <summary>
		/// Constructs an instance of the AnalyticsProxy class with the specified product name.
		/// </summary>
		/// <param name="productName">
		/// The name of the product (suitable for displaying in the UI). This will be also be used
		/// as part of the registry key path for storing the product-specific analytics-enabled
		/// setting, unless <see cref="ProductRegistryKeyId"/> is overridden.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// The <paramref name="productName"/> was null
		/// </exception>
		public AnalyticsProxy(string productName)
		{
			ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
		}

		/// <summary>
		/// If <see cref="ProductName"/> (the UI name of the product) is not suitable to be used
		/// as part of a registry key path, this property can be overridden to provide an alternate
		/// identifier. This should be unique across products published by the organization.
		/// </summary>
		public virtual string ProductRegistryKeyId => _productRegistryKeyId ?? ProductName;

		public virtual string OrganizationName { get; } = "SIL Global";
		
		public virtual string OrganizationRegistryKeyId { get; } = "SIL";

		private string OrganizationRegistryKeyPath => $@"Software\{OrganizationRegistryKeyId}";

		private string GetKeyPath(string productKeyId)
		{
			var productPart = productKeyId != null ? $@"\{productKeyId}" : string.Empty;
			return $@"{OrganizationRegistryKeyPath}{productPart}\Analytics";
		}

		public bool AllowTracking =>
			ReadAnalyticsEnabledState(ProductRegistryKeyId)  // product-specific
			?? (OrganizationAnalyticsEnabled ?? true); // fall back to global, then default to true

		public bool? OrganizationAnalyticsEnabled => ReadAnalyticsEnabledState(null);

		public void Update(bool allowTracking, bool applyOrganizationWide = false)
		{
			if (applyOrganizationWide)
			{
				WriteAnalyticsEnabledState(null, allowTracking);
				// Since the global value is being set, we must remove the product-specific setting, so that
				// if a *later* decision is made in a different product to change the global setting, it
				// will apply to this product as well (i.e., the product-specific setting won't override it).
				RemoveProductAnalyticsEnabledSetting();
			}
			else
			{
				// The global value is *not* being set. If it was previously set in some other product, it
				// will remain in effect, but the product-specific value will override it for this product.
				WriteAnalyticsEnabledState(ProductRegistryKeyId, allowTracking);
			}

			AllowTrackingChanged?.Invoke(this, new AllowTrackingChangedEventArgs(allowTracking));
		}

		private bool? ReadAnalyticsEnabledState(string productKeyId)
		{
			try
			{
				using var key = Registry.CurrentUser.OpenSubKey(GetKeyPath(productKeyId));

				var value = key?.GetValue(kRegistryValueName);
				if (value == null)
					return null;
				return Convert.ToInt32(value) != 0;
			}
			catch
			{
				return null;
			}
		}

		private void WriteAnalyticsEnabledState(string productKeyId, bool value)
		{
			using var key = Registry.CurrentUser.CreateSubKey(GetKeyPath(productKeyId));
			key?.SetValue(kRegistryValueName, value ? 1 : 0, RegistryValueKind.DWord);
		}

		private void RemoveProductAnalyticsEnabledSetting()
		{
			using var key =
				Registry.CurrentUser.OpenSubKey(GetKeyPath(ProductRegistryKeyId), true);
			if (key != null && key.GetValueNames().Contains(kRegistryValueName))
				key.DeleteValue(kRegistryValueName);
		}
	}
}
