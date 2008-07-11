using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

using Palaso.WritingSystems;
using Palaso.UI.WindowsForms.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems
{
	class PickerPMTests
	{
		PickerPM _model;
		string _testFilePath;

		[SetUp]
		public void Setup()
		{
			_model = new PickerPM();
			_testFilePath = Path.GetTempFileName();
			IWritingSystemStore writingSystemStore = new LdmlInXmlWritingSystemStore();
			_model.WritingSystemStore = writingSystemStore;
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(_testFilePath);
		}

		[Test]
		public void EnumerateWritingSystems_HasMoreThanOne()
		{
			IEnumerable<WritingSystemDefinition> ws = _model.WritingSystemsDefinitions;
			IEnumerator<WritingSystemDefinition> it = ws.GetEnumerator();
			it.MoveNext();
			Console.WriteLine(String.Format("Current writingsystem {0}", it.Current.DisplayLabel));
			Assert.IsNotNull(it.Current);
		}

	}
}
