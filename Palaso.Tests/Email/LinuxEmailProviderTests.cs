using System;
using NUnit.Framework;
using Palaso.Email;

namespace Palaso.Tests.Email
{
	[TestFixture]
	public class LinuxEmailProviderTests
	{
		[Test]
		public void EscapeString()
		{
			string input;

			// Does not change most things.
			input = @"hello1234 hello";
			Assert.That(LinuxEmailProvider.EscapeString(input), Is.EqualTo(input));
			input = "hello\"hello";
			Assert.That(LinuxEmailProvider.EscapeString(input), Is.EqualTo(input));
			input = @"hello`~!@#$%^&*)(][}{/?=+-_|";
			Assert.That(LinuxEmailProvider.EscapeString(input), Is.EqualTo(input));

			// Escapes quote and backslash characters.
			Assert.That(LinuxEmailProvider.EscapeString(@"\"), Is.EqualTo(@"\\\\\\\\"));
			Assert.That(LinuxEmailProvider.EscapeString(@"hello\hello"), Is.EqualTo(@"hello\\\\\\\\hello"));
			Assert.That(LinuxEmailProvider.EscapeString(@"hello\t\n\a\r\0end"), Is.EqualTo(@"hello\\\\\\\\t\\\\\\\\n\\\\\\\\a\\\\\\\\r\\\\\\\\0end"));
			Assert.That(LinuxEmailProvider.EscapeString(@"hello'hello"), Is.EqualTo(@"hello\'hello"));
			Assert.That(LinuxEmailProvider.EscapeString(@"hello'hello'"), Is.EqualTo(@"hello\'hello\'"));
			Assert.That(LinuxEmailProvider.EscapeString("hello'\"hello"), Is.EqualTo("hello\\'\"hello"));
			Assert.That(LinuxEmailProvider.EscapeString(@"C:\'Data\0end"), Is.EqualTo(@"C:\\\\\\\\\'Data\\\\\\\\0end"));
		}
	}
}