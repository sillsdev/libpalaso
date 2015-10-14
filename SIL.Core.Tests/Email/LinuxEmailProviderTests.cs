using NUnit.Framework;
using SIL.Email;

namespace SIL.Tests.Email
{
	[TestFixture]
	public class LinuxEmailProviderTests
	{
		// Does not change most things.
		[TestCase(@"hello1234 hello", Result =@"hello1234 hello")]
		[TestCase("hello\"hello", Result ="hello\"hello")]
		[TestCase(@"hello`~!@#$%^&*)(][}{/?=+-_|", Result =@"hello`~!@#$%^&*)(][}{/?=+-_|")]
		// Escapes quote and backslash characters.
		[TestCase(@"\", Result =@"\\\\\\\\")]
		[TestCase(@"hello\hello", Result =@"hello\\\\\\\\hello")]
		[TestCase(@"hello\t\n\a\r\0end", Result =@"hello\\\\\\\\t\\\\\\\\n\\\\\\\\a\\\\\\\\r\\\\\\\\0end")]
		[TestCase(@"hello'hello", Result =@"hello\'hello")]
		[TestCase(@"hello'hello'", Result =@"hello\'hello\'")]
		[TestCase("hello'\"hello", Result ="hello\\'\"hello")]
		[TestCase(@"C:\'Data\0end", Result =@"C:\\\\\\\\\'Data\\\\\\\\0end")]
		public string EscapeString(string input)
		{
			return LinuxEmailProvider.EscapeString(input);
		}
	}
}