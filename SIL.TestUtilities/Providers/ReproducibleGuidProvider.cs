// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using JetBrains.Annotations;
using SIL.Providers;

namespace SIL.TestUtilities.Providers
{
	/// <summary>
	/// A provider to get reproducible GUIDs. This is helpful in unit tests.
	/// </summary>
	/// <remarks>Usage:
	/// <code>
	/// GuidProvider.SetProvider(new ReproducibleGuidProvider("ea5cddc9-4d17-4bb7-b0fd-714d5c9d7d{0:00}"));
	/// ... tests ...
	/// GuidProvider.ResetToDefault();
	/// </code>
	/// </remarks>
	[PublicAPI]
	public class ReproducibleGuidProvider: GuidProvider
	{
		private          int    _count;
		private readonly string _guidTemplate;

		/// <summary>
		/// Initializes a new instance of the ReproducibleGuidProvider class.
		/// </summary>
		/// <param name="guidTemplate">A template GUID that can contain a placeholder. This
		/// template will be processed with <c>string.Format()</c>.</param>
		public ReproducibleGuidProvider(string guidTemplate = "00000000-0000-0000-0000-0000000000{0:00}")
		{
			_guidTemplate = guidTemplate;
		}

		public override Guid NewGuid() => new Guid(string.Format(_guidTemplate, _count++));
	}
}
