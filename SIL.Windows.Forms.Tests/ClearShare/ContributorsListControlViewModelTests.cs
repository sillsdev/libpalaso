using System.Linq;
using Moq;
using NUnit.Framework;
using SIL.Windows.Forms.ClearShare;

namespace SIL.Windows.Forms.Tests.ClearShare
{
	[TestFixture]
	public class ContributorsListControlViewModelTests
	{
		private ContributorsListControlViewModel _model;
		private ContributionCollection _contributions;

		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void TestSetup()
		{
			var system = new OlacSystem();

			_model = new ContributorsListControlViewModel(null, null);
			_contributions = new ContributionCollection(new[]
			{
				new Contribution("Leroy", system.GetRoles().ElementAt(0)),
				new Contribution("Jed", system.GetRoles().ElementAt(1)),
				new Contribution("Art", system.GetRoles().ElementAt(2))
			});

			_model.SetContributionList(_contributions);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetAutoCompleteNames_NullGatherer_ReturnsEmptyList()
		{
			Assert.AreEqual(0, _model.GetAutoCompleteNames().Count);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetAutoCompleteNames_HasGatherer_ReturnsNames()
		{
			var gatherer = new Mock<IAutoCompleteValueProvider>();
			gatherer.Setup(g => g.GetValuesForKey("person")).Returns(new[] { "jimmy", "tommy" });
			_model = new ContributorsListControlViewModel(gatherer.Object, null);

			var names = _model.GetAutoCompleteNames();
			Assert.AreEqual(2, names.Count);
			Assert.IsTrue(names.Contains("jimmy"));
			Assert.IsTrue(names.Contains("tommy"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetAutoCompleteNames_HasGatherer_ReturnsValidNames()
		{
			var gatherer = new Mock<IAutoCompleteValueProvider>();
			gatherer.Setup(g => g.GetValuesForKey("person")).Returns(new[] { "jimmy (Author)", "tommy", "jimmy (transcriber)" });
			_model = new ContributorsListControlViewModel(gatherer.Object, null);

			var names = _model.GetAutoCompleteNames();
			Assert.AreEqual(2, names.Count);
			Assert.IsFalse(names.Contains("jimmy (Author)"));
			Assert.IsTrue(names.Contains("jimmy"));
			Assert.IsTrue(names.Contains("tommy"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributionList_SetListToNull_YieldsEmptyList()
		{
			_model.SetContributionList(null);
			Assert.AreEqual(0, _model.Contributions.Count());
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributionList_SetToValidList_YieldsCorrectList()
		{
			Assert.AreEqual(3, _model.Contributions.Count());
			Assert.AreEqual("Leroy", _model.Contributions.ElementAt(0).ContributorName);
			Assert.AreEqual("Jed", _model.Contributions.ElementAt(1).ContributorName);
			Assert.AreEqual("Art", _model.Contributions.ElementAt(2).ContributorName);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributionList_Called_FiresEvent()
		{
			bool eventFired = false;
			_model.NewContributionListAvailable += ((o, a) => eventFired = true);
			_model.SetContributionList(null);
			Assert.IsTrue(eventFired);
		}
	}
}
