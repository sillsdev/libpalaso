// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace Palaso.Providers
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
				throw new ArgumentNullException("provider");

			Current = provider;
		}
	}
}
