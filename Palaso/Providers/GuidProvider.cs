﻿// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace Palaso.Providers
{
	/// <summary>
	/// Implements a testable GUID provider. Use this class instead of directly calling
	/// <c>Guid.NewGuid()</c>. In your tests you can replace the default Guid provider
	/// with one that gives reproducible results, e.g. ReproducibleGuidProvider.
	/// </summary>
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
