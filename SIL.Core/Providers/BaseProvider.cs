// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace SIL.Providers
{
	public abstract class BaseProvider<T, TDefault> where TDefault : T, new()
	{
		static BaseProvider()
		{
			Current = new TDefault();
		}

		public static T Current { get; private set; }

		public static void ResetToDefault()
		{
			Current = new TDefault();
		}

		public static void SetProvider(T provider)
		{
			if (ReferenceEquals(provider, null))
				throw new ArgumentNullException(nameof(provider));

			Current = provider;
		}
	}
}
