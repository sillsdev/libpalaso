using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SIL.Archiving.Generic;
using SIL.Archiving.IMDI;
using SIL.Archiving.IMDI.Lists;
using SIL.Archiving.IMDI.Schema;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	[Category("Archiving")]
	class IMDI30Tests
	{
		[Test]
		public void SessionDate_DateTime_ValidStringProduced()
		{
			var session = new Session();
			var dateIn = DateTime.Today;

			session.SetDate(dateIn);

			var dateOut = session.Date;

			Assert.AreEqual(dateIn.ToString("yyyy-MM-dd"), dateOut, "The date returned was not what was expected.");
		}

		[Test]
		public void SessionDate_YearOnly_ValidStringProduced()
		{
			var session = new Session();
			const int dateIn = 1964;

			session.SetDate(dateIn);

			var dateOut = session.Date;

			Assert.AreEqual(dateIn.ToString(CultureInfo.InvariantCulture), dateOut, "The date returned was not what was expected.");
		}

		[Test]
		public void SessionDate_DateRange_ValidStringProduced()
		{
			var session = new Session();
			const string dateIn = "2013-11-20 to 2013-11-25";

			session.SetDate(dateIn);

			var dateOut = session.Date;

			Assert.AreEqual(dateIn, dateOut, "The date returned was not what was expected.");
		}

		[Test]
		public void ActorType_ArchivingActor_ValidActorType()
		{
			const string motherTongueIso3 = "spa";
			const string primaryLanguageIso3 = "eng";
			const string additionalLanguageIso3 = "fra";

			var actrIn = new ArchivingActor
			{
				Education = "8th grade",
				Name = "John Smith",
				Age = "50 +- 10",
				Gender = "Male",
				BirthDate = 1964,
				Occupation = "Nerf Herder",
				MotherTongueLanguage = new ArchivingLanguage(motherTongueIso3),
				PrimaryLanguage = new ArchivingLanguage(primaryLanguageIso3)
			};

			// additional languages
			actrIn.Iso3Languages.Add(new ArchivingLanguage(additionalLanguageIso3));

			var actrOut = new ActorType(actrIn);

			Assert.AreEqual(actrIn.Name, actrOut.FullName);
			Assert.AreEqual(actrIn.Name, actrOut.Name[0]);
			Assert.AreEqual(actrIn.Gender, actrOut.Sex.Value);
			Assert.AreEqual(actrIn.Occupation, actrOut.Keys.Key.Find(k => k.Name == "Occupation").Value);
			Assert.AreEqual(actrIn.GetBirthDate(), actrOut.BirthDate);
			Assert.AreEqual(actrIn.Age, actrOut.Age);

			// language count
			Assert.AreEqual(3, actrOut.Languages.Language.Count);

			// mother tongue
			string motherTongueCodeFound = null;
			foreach (var lang in actrOut.Languages.Language)
			{
				if (lang.MotherTongue.Value == BooleanEnum.@true)
					motherTongueCodeFound = lang.Id;
			}

			Assert.IsNotNull(motherTongueCodeFound, "Mother tongue not found.");
			Assert.AreEqual(motherTongueIso3, motherTongueCodeFound.Substring(motherTongueCodeFound.Length - 3), "Wrong mother tongue returned.");

			// primary language
			string primaryLanguageCodeFound = null;
			foreach (var lang in actrOut.Languages.Language)
			{
				if (lang.PrimaryLanguage.Value == BooleanEnum.@true)
					primaryLanguageCodeFound = lang.Id;
			}

			Assert.IsNotNull(primaryLanguageCodeFound, "Primary language not found.");
			Assert.AreEqual(primaryLanguageIso3, primaryLanguageCodeFound.Substring(primaryLanguageCodeFound.Length - 3), "Wrong primary language returned.");

			// additional language
			var additionalLanguageFound = false;
			foreach (var lang in actrOut.Languages.Language)
			{
				if (lang.Id.EndsWith(additionalLanguageIso3))
					additionalLanguageFound = true;
			}

			Assert.IsTrue(additionalLanguageFound, "The additional language was not found.");
		}

		[Test]
		public void LocationType_ArchivingLocation_ValidLocationType()
		{
			var locIn = new ArchivingLocation
			{
				Continent = "Asia",
				Country = "China",
				Region = "Great Wall",
				Address = "315 N Main St"
			};

			var locOut = locIn.ToIMDILocationType();

			Assert.AreEqual("Asia", locOut.Continent.Value);
			Assert.AreEqual("China", locOut.Country.Value);
			Assert.AreEqual("Great Wall", locOut.Region[0]);
			Assert.AreEqual("315 N Main St", locOut.Address);
		}

		[Test]
		public void SetContinent_InvalidContinent_ReturnsUnspecified()
		{
			LocationType location = new LocationType();
			location.SetContinent("Narnia");

			Assert.AreEqual("Unspecified", location.Continent.Value);
		}

		[Test]
		public void AddDescription_Add2ForSameLanguage_AddsOnlyTheFirst()
		{
			var desc1 = new LanguageString { Iso3LanguageId = "eng", Value = "First description"};
			var desc2 = new LanguageString { Iso3LanguageId = "eng", Value = "Second description" };

			var obj = new Corpus();
			obj.Description.Add(desc1);
			obj.Description.Add(desc2);

			Assert.AreEqual(1, obj.Description.Count);
			Assert.AreEqual("First description", obj.Description.First().Value);
		}

		[Test]
		public void AddSubjectLanguge_AddDuplicate_DuplicateNotAdded()
		{
			IMDIPackage proj = new IMDIPackage(false, string.Empty);

			proj.ContentIso3Languages.Add(new ArchivingLanguage("fra"));
			proj.ContentIso3Languages.Add(new ArchivingLanguage("spa"));
			proj.ContentIso3Languages.Add(new ArchivingLanguage("spa"));
			proj.ContentIso3Languages.Add(new ArchivingLanguage("fra"));

			Assert.AreEqual(2, proj.ContentIso3Languages.Count);
			Assert.IsTrue(proj.ContentIso3Languages.Contains(new ArchivingLanguage("fra")));
			Assert.IsTrue(proj.ContentIso3Languages.Contains(new ArchivingLanguage("spa")));
		}

		[Test]
		public void AddDocumentLanguge_AddDuplicate_DuplicateNotAdded()
		{
			IMDIPackage proj = new IMDIPackage(false, string.Empty);

			proj.MetadataIso3Languages.Add(new ArchivingLanguage("fra"));
			proj.MetadataIso3Languages.Add(new ArchivingLanguage("spa"));
			proj.MetadataIso3Languages.Add(new ArchivingLanguage("spa"));
			proj.MetadataIso3Languages.Add(new ArchivingLanguage("fra"));

			Assert.AreEqual(2, proj.MetadataIso3Languages.Count);
			Assert.IsTrue(proj.MetadataIso3Languages.Contains(new ArchivingLanguage("fra")));
			Assert.IsTrue(proj.MetadataIso3Languages.Contains(new ArchivingLanguage("spa")));
		}

		[Test]
		public void SessionAddActor_Anonymized_ReturnsAnonymizedNames()
		{
			ArchivingActor actor = new ArchivingActor
			{
				Name = "Actor Name",
				FullName = "Actor Full Name",
				Anonymize = true
			};

			Session session = new Session();
			session.AddActor(actor);

			var imdiActor = session.MDGroup.Actors.Actor[0];

			Assert.AreNotEqual("Actor Name", imdiActor.Name[0]);
			Assert.AreNotEqual("Actor Full Name", imdiActor.FullName);
		}

		[Test]
		public void SessionAddActor_Anonymized_RemovesActorFiles()
		{
			ArchivingActor actor = new ArchivingActor
			{
				Name = "Actor Name",
				FullName = "Actor Full Name",
				Anonymize = true
			};

			actor.Files.Add(new ArchivingFile(System.Reflection.Assembly.GetExecutingAssembly().Location));

			Session session = new Session();
			session.AddActor(actor);

			Assert.AreEqual(0, session.Resources.MediaFile.Count);
			Assert.AreEqual(0, session.Resources.WrittenResource.Count);
		}

		[Test]
		public void FindByISO3Code_InvalidIso3Code_MustBeInList_Throws()
		{
			Assert.Throws<ArgumentException>(() => LanguageList.FindByISO3Code("xyz", true));
		}

		[Test]
		public void FindByISO3Code_InvalidIso3Code_NotMustBeInList_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => LanguageList.FindByISO3Code("xyz", false));
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetList_RemoveNone_ReturnUnknownAndUnspecified()
		{
			var countries = ListConstructor.GetList(ListType.Countries, true, null, ListConstructor.RemoveUnknown.RemoveNone);

			Assert.NotNull(countries.FindByText("Unknown"));
			Assert.NotNull(countries.FindByText("Unspecified"));
			Assert.NotNull(countries.FindByText("Undefined"));
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetList_LeaveUnknown_ReturnUnspecified()
		{
			var countries = ListConstructor.GetList(ListType.Countries, true, null, ListConstructor.RemoveUnknown.LeaveUnknown);

			Assert.NotNull(countries.FindByText("Unknown"));
			Assert.IsNull(countries.FindByText("Unspecified"));
			Assert.IsNull(countries.FindByText("Undefined"));
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetList_RemoveAll_ReturnNone()
		{
			var countries = ListConstructor.GetList(ListType.Countries, true, null, ListConstructor.RemoveUnknown.RemoveAll);

			Assert.IsNull(countries.FindByText("Unknown"));
			Assert.IsNull(countries.FindByText("Unspecified"));
			Assert.IsNull(countries.FindByText("Undefined"));
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetClosedList_RemoveNone_ReturnUnknownAndUnspecified()
		{
			var countries = ListConstructor.GetClosedList(ListType.Countries, true, ListConstructor.RemoveUnknown.RemoveNone);

			Assert.NotNull(countries.FindByText("Unknown"));
			Assert.NotNull(countries.FindByText("Unspecified"));
			Assert.NotNull(countries.FindByText("Undefined"));
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetClosedList_LeaveUnknown_ReturnUnspecified()
		{
			var countries = ListConstructor.GetClosedList(ListType.Countries, true, ListConstructor.RemoveUnknown.LeaveUnknown);

			Assert.NotNull(countries.FindByText("Unknown"));
			Assert.IsNull(countries.FindByText("Unspecified"));
			Assert.IsNull(countries.FindByText("Undefined"));
		}

		[Test]
		[Category("SkipOnTeamCity")]
		public void GetClosedList_RemoveAll_ReturnNone()
		{
			var countries = ListConstructor.GetClosedList(ListType.Countries, true, ListConstructor.RemoveUnknown.RemoveAll);

			Assert.IsNull(countries.FindByText("Unknown"));
			Assert.IsNull(countries.FindByText("Unspecified"));
			Assert.IsNull(countries.FindByText("Undefined"));
		}

		[Test]
		public void SetSessionGenre_ContainsAngleBrackets_LatinOnly()
		{
			var session = new Session();
			session.Genre = "<Unknown>";
			Assert.AreEqual("Unknown", session.Genre);
		}
	}
}
