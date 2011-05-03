using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Palaso.ClearShare;
using System.Linq;

namespace PalasoUIWindowsForms.Tests.ClearShare
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
		public void GetCanDeleteContribution_NullList_ReturnsFalse()
		{
			_model.SetContributionList(null);
			Assert.IsFalse(_model.GetCanDeleteContribution(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetCanDeleteContribution_EmptyList_ReturnsFalse()
		{
			_model.SetContributionList(new ContributionCollection());
			Assert.AreEqual(0, _model.Contributions.Count());
			Assert.IsFalse(_model.GetCanDeleteContribution(0));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetCanDeleteContribution_IndexTooSmall_ReturnsFalse()
		{
			Assert.IsFalse(_model.GetCanDeleteContribution(-1));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetCanDeleteContribution_IndexTooBig_ReturnsFalse()
		{
			Assert.IsFalse(_model.GetCanDeleteContribution(3));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetCanDeleteContribution_ValidIndex_ReturnsTrue()
		{
			Assert.IsTrue(_model.GetCanDeleteContribution(0));
			Assert.IsTrue(_model.GetCanDeleteContribution(1));
			Assert.IsTrue(_model.GetCanDeleteContribution(2));
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

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionCopy_IndexTooSmall_ReturnsNull()
		{
			Assert.IsNull(_model.GetContributionCopy(-1));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionCopy_IndexTooBig_ReturnsNull()
		{
			Assert.IsNull(_model.GetContributionCopy(3));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionCopy_ValidIndex_ReturnsCopy()
		{
			var copy = _model.GetContributionCopy(1);
			Assert.AreEqual(_model.Contributions.ElementAt(1).ContributorName, copy.ContributorName);
			Assert.AreEqual(_model.Contributions.ElementAt(1).Role.Code, copy.Role.Code);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionCopy_IndexOneGreaterThanCollectionCount_ReturnsCopyOfTempContributor()
		{
			var temp = _model.CreateTempContribution();
			temp.ContributorName = "Fosdick";
			var copy = _model.GetContributionCopy(_model.Contributions.Count);

			Assert.AreNotSame(temp, copy);
			Assert.AreEqual("Fosdick", copy.ContributorName);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionValue_IndexTooSmall_ReturnsNull()
		{
			Assert.IsNull(_model.GetContributionValue(-1, "name"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionValue_IndexTooBig_ReturnsNull()
		{
			Assert.IsNull(_model.GetContributionValue(3, "name"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionValue_ValidIndexNullValueName_ReturnsNull()
		{
			Assert.IsNull(_model.GetContributionValue(1, null));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionValue_ValidIndexAndValueName_ReturnsValue()
		{
			Assert.AreEqual("Jed", _model.GetContributionValue(1, "name"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionValue_IndexEqualsCollectionCount_ReturnsTempContributionValue()
		{
			var temp = _model.CreateTempContribution();
			temp.ContributorName = "Arturo";
			temp.Comments = "is skinny";
			Assert.AreEqual("Arturo", _model.GetContributionValue(_model.Contributions.Count, "name"));
			Assert.AreEqual("is skinny", _model.GetContributionValue(_model.Contributions.Count, "comments"));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributionValue_IndexTooSmall_DoesNothing()
		{
			_model.SetContributionValue(-1, "name", "Dusty");
			Assert.AreEqual("Leroy", _model.Contributions.ElementAt(0).ContributorName);
			Assert.AreEqual("Jed", _model.Contributions.ElementAt(1).ContributorName);
			Assert.AreEqual("Art", _model.Contributions.ElementAt(2).ContributorName);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributionValue_IndexTooBig_DoesNothing()
		{
			_model.SetContributionValue(5, "name", "Dusty");
			Assert.AreEqual("Leroy", _model.Contributions.ElementAt(0).ContributorName);
			Assert.AreEqual("Jed", _model.Contributions.ElementAt(1).ContributorName);
			Assert.AreEqual("Art", _model.Contributions.ElementAt(2).ContributorName);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributionValue_ValidIndexNullValueName_DoesNothing()
		{
			_model.SetContributionValue(1, null, "Dusty");
			Assert.AreEqual("Leroy", _model.Contributions.ElementAt(0).ContributorName);
			Assert.AreEqual("Jed", _model.Contributions.ElementAt(1).ContributorName);
			Assert.AreEqual("Art", _model.Contributions.ElementAt(2).ContributorName);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributionValue_ValidIndexAndValueName_SetsValue()
		{
			_model.SetContributionValue(1, "name", "Dusty");
			Assert.AreEqual("Leroy", _model.Contributions.ElementAt(0).ContributorName);
			Assert.AreEqual("Dusty", _model.Contributions.ElementAt(1).ContributorName);
			Assert.AreEqual("Art", _model.Contributions.ElementAt(2).ContributorName);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributionValue_ChangeRole_ChangesRole()
		{
			Assert.AreEqual(_model.OlacRoles.ElementAt(2).Code, _model.Contributions.ElementAt(2).Role.Code);
			_model.SetContributionValue(2, "role", "Editor");
			Assert.AreEqual("editor", _model.Contributions.ElementAt(2).Role.Code);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void SetContributionValue_IndexEqualsCollectionCount_ChangesTempContribution()
		{
			var temp = _model.CreateTempContribution();
			Assert.IsNull(temp.ContributorName);
			Assert.IsNull(temp.Role);
			_model.SetContributionValue(_model.Contributions.Count, "name", "Arturo");
			_model.SetContributionValue(_model.Contributions.Count, "role", "Editor");
			Assert.AreEqual("Arturo", temp.ContributorName);
			Assert.AreEqual("editor", temp.Role.Code);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void DeleteContribution_EmptyList_DoesNothing()
		{
			_model.SetContributionList(null);
			Assert.AreEqual(0, _model.Contributions.Count());
			_model.DeleteContribution(1);
			Assert.AreEqual(0, _model.Contributions.Count());
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void DeleteContribution_IndexTooSmall_DoesNothing()
		{
			Assert.AreEqual(3, _model.Contributions.Count());
			_model.DeleteContribution(-1);
			Assert.AreEqual(3, _model.Contributions.Count());
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void DeleteContribution_IndexTooBig_DoesNothing()
		{
			Assert.AreEqual(3, _model.Contributions.Count());
			_model.DeleteContribution(3);
			Assert.AreEqual(3, _model.Contributions.Count());
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void DeleteContribution_ValidIndex_DeletesContributor()
		{
			Assert.AreEqual(3, _model.Contributions.Count());
			_model.DeleteContribution(1);
			Assert.AreEqual(2, _model.Contributions.Count());
			Assert.AreEqual("Leroy", _model.Contributions.ElementAt(0).ContributorName);
			Assert.AreEqual("Art", _model.Contributions.ElementAt(1).ContributorName);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionAt_IndexTooSmall_ReturnsNull()
		{
			Assert.IsNull(_model.GetContributionAt(-1));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionAt_IndexTooBig_ReturnsNull()
		{
			Assert.IsNull(_model.GetContributionAt(3));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionAt_ValidIndex_ReturnsCorrectContributor()
		{
			Assert.AreEqual("Leroy", _model.GetContributionAt(0).ContributorName);
			Assert.AreEqual("Jed", _model.GetContributionAt(1).ContributorName);
			Assert.AreEqual("Art", _model.GetContributionAt(2).ContributorName);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionAt_WhenTempExistsIndexOneGreaterThanCollectionCount_ReturnsTempContribution()
		{
			var temp =_model.CreateTempContribution();
			Assert.AreEqual(temp, _model.GetContributionAt(3));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void GetContributionAt_WhenTempExistsIndexTooBig_ReturnsNull()
		{
			_model.CreateTempContribution();
			Assert.IsNull(_model.GetContributionAt(4));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CreateTempContribution_Called_ReturnsContribution()
		{
			Assert.IsNotNull(_model.CreateTempContribution());
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CreateTempContribution_Called_ReturnsContributionNotInCollection()
		{
			var temp = _model.CreateTempContribution();
			Assert.IsFalse(_model.Contributions.Contains(temp));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CommitTempContributionIfExists_TempDoesNotExist_DoesNothing()
		{
			Assert.AreEqual(3, _model.Contributions.Count);
			_model.CommitTempContributionIfExists();
			Assert.AreEqual(3, _model.Contributions.Count);
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void CommitTempContributionIfExists_TempExists_AddsToCollection()
		{
			var temp = _model.CreateTempContribution();
			Assert.IsFalse(_model.Contributions.Contains(temp));
			_model.CommitTempContributionIfExists();
			Assert.IsTrue(_model.Contributions.Contains(temp));
		}

		/// ------------------------------------------------------------------------------------
		[Test]
		public void DiscardTempContribution_TempExists_CommitAfterDiscard_DoesNothing()
		{
			Assert.AreEqual(3, _model.Contributions.Count);
			_model.CreateTempContribution();
			_model.DiscardTempContribution();
			_model.CommitTempContributionIfExists();
			Assert.AreEqual(3, _model.Contributions.Count);
		}
	}
}
