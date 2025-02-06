// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace SIL.Providers
{
	/// <summary>
	/// Implements a testable GUID provider. Use this class instead of directly calling
	/// <c>Guid.NewGuid()</c>. In your tests you can replace the default Guid provider
	/// with one that gives reproducible results, e.g. ReproducibleGuidProvider.
	/// </summary>
	/// <remarks>Usage:
	/// Instead of <c>Guid.NewGuid()</c> you write <c>GuidProvider.Current.NewGuid()</c>
	/// </remarks>
	public abstract class GuidProvider
		: BaseProvider<GuidProvider, GuidProvider.DefaultGuidProvider>
	{
		#region DefaultGuidProvider
		public class DefaultGuidProvider: GuidProvider
		{
			public override Guid NewGuid()
			{
				return Guid.NewGuid();
			}
		}
		#endregion

		public abstract Guid NewGuid();
	}
}
