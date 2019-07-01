// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;

namespace NUnit.Framework
{
	// TODO: Remove this class when we start using NUnit 3

	/// <summary>
	/// This attribute allows to use the NUnit 3 name while we still use NUnit 2.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class OneTimeSetUpAttribute: TestFixtureSetUpAttribute
	{

	}
}