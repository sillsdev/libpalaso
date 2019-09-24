// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Reflection;

namespace NUnit.Framework.Interfaces
{
	public class ITest: TestDetails
	{
		public ITest(object fixture, MethodInfo method, string fullName, string type, bool isSuite)
			: base(fixture, method, fullName, type, isSuite)
		{
		}
	}
}