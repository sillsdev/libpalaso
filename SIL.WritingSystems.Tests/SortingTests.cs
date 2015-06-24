using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class SortingTests
	{
		[Test]
		public void IcuCollator_CBA_ABC()
		{
			var list = new List<string>();
			list.Add("c");
			list.Add("b");
			list.Add("a");
			list.Sort(new IcuRulesCollator(String.Empty));
			Assert.That(list[0], Is.EqualTo("a"));
			Assert.That(list[1], Is.EqualTo("b"));
			Assert.That(list[2], Is.EqualTo("c"));
		}

		[Test]
		public void System_CBA_ABC()
		{
			var list = new List<string>();
			list.Add("c");
			list.Add("b");
			list.Add("a");
			list.Sort(new SystemCollator(null));
			Assert.That(list[0], Is.EqualTo("a"));
			Assert.That(list[1], Is.EqualTo("b"));
			Assert.That(list[2], Is.EqualTo("c"));
		}

		[Test]
		public void IcuCollator_WithDashCBA_ABCDash()
		{
//            string rules = String.Empty; // This will fail
			string rules = "&[last primary ignorable] <<< '-' <<< ' '";
			var list = new List<string>();
			list.Add("-c");
			list.Add("c");
			list.Add("b");
			list.Add("a");
			list.Sort(new IcuRulesCollator(rules));
			Assert.That(list[0], Is.EqualTo("a"));
			Assert.That(list[1], Is.EqualTo("b"));
			Assert.That(list[2], Is.EqualTo("c"));
			Assert.That(list[3], Is.EqualTo("-c"));
		}

		[Test]
		public void ICUCollator_WithUnicodeEscapes()
		{
			string rules = "&b<\\u0061"; //b comes before 'a' where 'a' is a unicode escape
			var list = new List<string>();
			list.Add("a");
			list.Add("c");
			list.Add("b");
			list.Sort(new IcuRulesCollator(rules));
			Assert.That(list[0], Is.EqualTo("b"));
			Assert.That(list[1], Is.EqualTo("a"));
			Assert.That(list[2], Is.EqualTo("c"));
		}

		[Test]
		public void System_WithDashCBA_ABCDash()
		{
			var list = new List<string>();
			list.Add("-c");
			list.Add("c");
			list.Add("b");
			list.Add("a");
			list.Sort(new SystemCollator(null));
			Assert.That(list[0], Is.EqualTo("a"));
			Assert.That(list[1], Is.EqualTo("b"));
			Assert.That(list[2], Is.EqualTo("c"));
			Assert.That(list[3], Is.EqualTo("-c"));
		}
	}
}
