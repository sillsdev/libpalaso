using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Palaso.TestUtilities;

namespace PalasoUIWindowsForms.Tests.i8n
{
	[TestFixture]
	class PotIOTests
	{
		class Environment : IDisposable
		{
			private TempFile _potFile;

			public TempFile PotFile
			{
				get { return _potFile; }
			}

			private TempFile GetSimplePoTemplateFile()
			{
				string[] fileContent = new string[2];
				fileContent[0] = "msgid \"This is a string in the PotFile\"";
				fileContent[1] = "msgid \"This is also a string in the PotFile\"";

				var potFile = new TempFile(fileContent);
				return potFile;
			}

			public void Dispose()
			{

			}
		}

		[Test]
		public void GetStringCollection_PotFileHasValidStrings_ReturnsStrings()
		{
			throw new NotImplementedException();
		}

		[Test]
		public void GetStringCollection_PotFileDoesNotHaveaValidString_ReturnsEmpty()
		{
			throw new NotImplementedException();
		}
	}
}
