// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using SIL.IO;

namespace SIL.Tests.IO
{
	internal class FileTestEnvironment : IDisposable
	{
		public FileTestEnvironment()
		{
			const string fileContents = "some bogus text\n<element></element>\n<el2 lang='fr' />";
			TempFile = new TempFile(fileContents);
		}

		public TempFile TempFile { get; }

		public void Dispose()
		{
			TempFile.Dispose();
		}
	}
}
