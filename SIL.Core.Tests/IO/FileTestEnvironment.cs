using SIL.IO;
using System;

namespace SIL.Tests.IO
{
	internal class FileTestEnvironment : IDisposable
	{
		public FileTestEnvironment()
		{
			string fileContents = "some bogus text\n<element></element>\n<el2 lang='fr' />";
			TempFile = new TempFile(fileContents);
		}

		public TempFile TempFile { get; }

		public void Dispose()
		{
			TempFile.Dispose();
		}
	}
}
