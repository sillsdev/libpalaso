using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text;

using NUnit.Framework;

using Palaso.WritingSystems;
using Palaso.UI.WindowsForms.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems
{

	[TestFixture]
	public class LookupIsoCodeModelTests
	{
		private LookupIsoCodeModel _model;

		[SetUp]
		public void Setup()
		{
			_model = new LookupIsoCodeModel();
		}

		[Test]
		public void GetMatchingWritingSystems_NoMatches_Empty()
		{
			Assert.AreEqual(0, _model.GetMatchingWritingSystems("^^^").Count());
		}

		[Test]
		public void GetMatchingWritingSystems_EmptyString_GetWholeList()
		{
			Assert.Greater(_model.GetMatchingWritingSystems(string.Empty).Count(), 7000);
		}


		[Test]
		public void GetMatchingWritingSystems_UpperCaseGerman_ReturnsGerman()
		{
			var matchingWritingSystems = _model.GetMatchingWritingSystems("German").ToArray();
			Assert.AreEqual("de",matchingWritingSystems[0].Code);
		}

		[Test]
		public void GetMatchingWritingSystems_lowerCaseGerman_ReturnsGerman()
		{
			var matchingWritingSystems = _model.GetMatchingWritingSystems("german").ToArray();
			Assert.AreEqual("de", matchingWritingSystems[0].Code);
		}
	}
}