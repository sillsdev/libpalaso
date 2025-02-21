//using Exortech.NetReflector;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.Text;

namespace SIL.Tests.Text
{
	[TestFixture]
	public class MultiTextBaseTests
	{

		[Test]
		public void NullConditions()
		{
			MultiTextBase text = new MultiTextBase();
			Assert.AreSame(string.Empty, text["foo"], "never before heard of alternative should give back an empty string");
			Assert.AreSame(string.Empty, text["foo"], "second time");
			Assert.AreSame(string.Empty, text.GetBestAlternative("fox"));
			text.SetAlternative("zox", "");
			Assert.AreSame(string.Empty, text["zox"]);
			text.SetAlternative("zox", null);
			Assert.AreSame(string.Empty, text["zox"], "should still be empty string after setting to null");
			text.SetAlternative("zox", "something");
			text.SetAlternative("zox", null);
			Assert.AreSame(string.Empty, text["zox"], "should still be empty string after setting something and then back to null");
		}
		[Test]
		public void BasicStuff()
		{
			MultiTextBase text = new MultiTextBase();
			text["foo"] = "alpha";
			Assert.AreSame("alpha", text["foo"]);
			text["foo"] = "beta";
			Assert.AreSame("beta", text["foo"]);
			text["foo"] = "gamma";
			Assert.AreSame("gamma", text["foo"]);
			text["bee"] = "beeeee";
			Assert.AreSame("gamma", text["foo"], "setting a different alternative should not affect this one");
			text["foo"] = null;
			Assert.AreSame(string.Empty, text["foo"]);
		}

		//        [Test, NUnit.Framework.Category("UsesObsoleteExpectedExceptionAttribute"), ExpectedException(typeof(ArgumentOutOfRangeException))]
		//        public void GetIndexerThrowsWhenAltIsMissing()
		//        {
		//            MultiTextBase text = new MultiTextBase();
		//            text["foo"] = "alpha";
		//            string s = text["gee"];
		//        }
		//
		//        [Test, NUnit.Framework.Category("UsesObsoleteExpectedExceptionAttribute"), ExpectedException(typeof(ArgumentOutOfRangeException))]
		//        public void GetExactThrowsWhenAltIsMissing()
		//        {
		//            MultiTextBase text = new MultiTextBase();
		//            text["foo"] = "alpha";
		//            string s = text.GetExactAlternative("gee");
		//        }

		//        [Test]
		//        public void ImplementsIEnumerable()
		//        {
		//            MultiTextBase text = new MultiTextBase();
		//            IEnumerable ienumerable = text;
		//            Assert.IsNotNull(ienumerable);
		//        }

		[Test]
		public void Count()
		{
			MultiTextBase text = new MultiTextBase();
			Assert.AreEqual(0, text.Count);
			text["a"] = "alpha";
			text["b"] = "beta";
			text["g"] = "gamma";
			Assert.AreEqual(3, text.Count);
		}

		[Test]
		public void IterateWithForEach()
		{
			MultiTextBase text = new MultiTextBase();
			text["a"] = "alpha";
			text["b"] = "beta";
			text["g"] = "gamma";
			int i = 0;
			foreach (LanguageForm l in text)
			{
				switch (i)
				{
					case 0:
						Assert.AreEqual("a", l.WritingSystemId);
						Assert.AreEqual("alpha", l.Form);
						break;
					case 1:
						Assert.AreEqual("b", l.WritingSystemId);
						Assert.AreEqual("beta", l.Form);
						break;
					case 2:
						Assert.AreEqual("g", l.WritingSystemId);
						Assert.AreEqual("gamma", l.Form);
						break;
				}
				i++;
			}
		}
		[Test]
		public void GetEnumerator()
		{
			MultiTextBase text = new MultiTextBase();
			IEnumerator ienumerator = text.GetEnumerator();
			Assert.IsNotNull(ienumerator);
		}



		[Test]
		public void MergeWithEmpty()
		{
			MultiTextBase old = new MultiTextBase();
			MultiTextBase newGuy = new MultiTextBase();
			old.MergeIn(newGuy);
			Assert.AreEqual(0, old.Count);

			old = new MultiTextBase();
			old["a"] = "alpha";
			old.MergeIn(newGuy);
			Assert.AreEqual(1, old.Count);
		}

		[Test]
		public void MergeWithOverlap()
		{
			MultiTextBase old = new MultiTextBase();
			old["a"] = "alpha";
			old["b"] = "beta";
			MultiTextBase newGuy = new MultiTextBase();
			newGuy["b"] = "newbeta";
			newGuy["c"] = "charlie";
			old.MergeIn(newGuy);
			Assert.AreEqual(3, old.Count);
			Assert.AreEqual("newbeta", old["b"]);
		}

		[Test]
		public void UsesNextAlternativeWhenMissing()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase["wsWithNullElement"] = null;
			multiTextBase["wsWithEmptyElement"] = "";
			multiTextBase["wsWithContent"] = "hello";
			Assert.AreEqual(String.Empty, multiTextBase.GetExactAlternative("missingWs"));
			Assert.AreEqual(String.Empty, multiTextBase.GetExactAlternative("wsWithEmptyElement"));
			Assert.AreEqual("hello", multiTextBase.GetBestAlternative("missingWs"));
			Assert.AreEqual("hello", multiTextBase.GetBestAlternative("wsWithEmptyElement"));
			Assert.AreEqual("hello*", multiTextBase.GetBestAlternative("wsWithEmptyElement", "*"));
			Assert.AreEqual("hello", multiTextBase.GetBestAlternative("wsWithNullElement"));
			Assert.AreEqual("hello*", multiTextBase.GetBestAlternative("wsWithNullElement", "*"));
			Assert.AreEqual("hello", multiTextBase.GetExactAlternative("wsWithContent"));
			Assert.AreEqual("hello", multiTextBase.GetBestAlternative("wsWithContent"));
			Assert.AreEqual("hello", multiTextBase.GetBestAlternative("wsWithContent", "*"));
		}


		[Test]
		public void SerializeWithXmlSerializer()
		{
			MultiTextBase text = new MultiTextBase();
			text["foo"] = "alpha";
			text["boo"] = "beta";
			var answerXPath = "/TestMultiTextHolder[namespace::xsd='http://www.w3.org/2001/XMLSchema' and namespace::xsi='http://www.w3.org/2001/XMLSchema-instance']/name/form";
			CheckSerializeWithXmlSerializer(text, answerXPath, 2);
		}

		[Test]
		public void SerializeEmptyWithXmlSerializer()
		{
			MultiTextBase text = new MultiTextBase();
			var answerXpath = "/TestMultiTextHolder[namespace::xsd='http://www.w3.org/2001/XMLSchema' and namespace::xsi='http://www.w3.org/2001/XMLSchema-instance']/name";
			CheckSerializeWithXmlSerializer(text, answerXpath, 1);
		}

		private void CheckSerializeWithXmlSerializer(MultiTextBase multiTextBase, string answer, int matches)
		{

			XmlSerializer ser = new XmlSerializer(typeof(TestMultiTextHolder));

			StringWriter writer = new System.IO.StringWriter();
			TestMultiTextHolder holder = new TestMultiTextHolder();
			holder.Name = multiTextBase;
			ser.Serialize(writer, holder);

			var mtxml = writer.GetStringBuilder().ToString();
			Debug.WriteLine(mtxml);
			AssertThatXmlIn.String(mtxml).HasSpecifiedNumberOfMatchesForXpath(answer, matches);
		}

		public class TestMultiTextHolder
		{
			[XmlIgnore]
			public MultiTextBase _name;

			[XmlElement("name")]
				public MultiTextBase Name
			{
				get { return _name; }
				set { _name = value; }
			}
		}

		[Test]
		public void ObjectEquals_DifferentNumberOfForms_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			x["ws2"] = "test";
			MultiTextBase y = new MultiTextBase();
			y["ws"] = "test";
			Assert.IsFalse(x.Equals((object) y));
			Assert.IsFalse(y.Equals((object) x));
		}

		[Test]
		public void ObjectEquals_SameContent_True()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			MultiTextBase y = new MultiTextBase();
			y.MergeIn(x);
			Assert.IsTrue(x.Equals((object) y));
			Assert.IsTrue(y.Equals((object) x));
		}

		[Test]
		public void ObjectEquals_Identity_True()
		{
			MultiTextBase x = new MultiTextBase();
			Assert.IsTrue(x.Equals((object) x));
		}

		[Test]
		public void ObjectEquals_DifferentValues_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			MultiTextBase y = new MultiTextBase();
			y["ws"] = "test1";
			Assert.IsFalse(x.Equals((object) y));
			Assert.IsFalse(y.Equals((object) x));
		}

		[Test]
		public void ObjectEquals_DifferentWritingSystems_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			MultiTextBase y = new MultiTextBase();
			y["ws1"] = "test";
			Assert.IsFalse(x.Equals((object) y));
			Assert.IsFalse(y.Equals((object) x));
		}

		[Test]
		public void ObjectEquals_Null_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			Assert.IsFalse(x.Equals((object) null));
		}

		[Test]
		public void Equals_Null_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			Assert.IsFalse(x.Equals(null));
		}

		[Test]
		public void Equals_DifferentNumberOfForms_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			x["ws2"] = "test";
			MultiTextBase y = new MultiTextBase();
			y["ws"] = "test";
			Assert.IsFalse(x.Equals(y));
			Assert.IsFalse(y.Equals(x));
		}

		[Test]
		public void Equals_SameContent_True()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			MultiTextBase y = new MultiTextBase();
			y.MergeIn(x);
			Assert.IsTrue(x.Equals(y));
			Assert.IsTrue(y.Equals(x));
		}

		[Test]
		public void Equals_Identity_True()
		{
			MultiTextBase x = new MultiTextBase();
			Assert.IsTrue(x.Equals(x));
		}

		[Test]
		public void Equals_DifferentValues_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			MultiTextBase y = new MultiTextBase();
			y["ws"] = "test1";
			Assert.IsFalse(x.Equals(y));
			Assert.IsFalse(y.Equals(x));
		}

		[Test]
		public void Equals_DifferentWritingSystems_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			MultiTextBase y = new MultiTextBase();
			y["ws1"] = "test";
			Assert.IsFalse(x.Equals(y));
			Assert.IsFalse(y.Equals(x));
		}

		[Test]
		public void ContainsEqualForm_SameContent_True()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws1"] = "testing";
			x["ws"] = "test";
			x["ws2"] = "testing";
			LanguageForm form = new LanguageForm();
			form.WritingSystemId = "ws";
			form.Form = "test";
			Assert.IsTrue(x.ContainsEqualForm(form));
		}

		[Test]
		public void ContainsEqualForm_DifferentWritingSystem_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			LanguageForm form = new LanguageForm();
			form.WritingSystemId = "wss";
			form.Form = "test";
			Assert.IsFalse(x.ContainsEqualForm(form));
		}

		[Test]
		public void ContainsEqualForm_DifferentValue_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			LanguageForm form = new LanguageForm();
			form.WritingSystemId = "ws";
			form.Form = "tests";
			Assert.IsFalse(x.ContainsEqualForm(form));
		}

		[Test]
		public void HasFormWithSameContent_Identity_True()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws1"] = "testing";
			x["ws"] = "test";
			x["ws2"] = "testing";
			Assert.IsTrue(x.HasFormWithSameContent(x));

		}
		[Test]
		public void HasFormWithSameContent_SameContent_True()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws1"] = "testing";
			x["ws"] = "test";
			x["ws2"] = "testing";
			MultiTextBase y = new MultiTextBase();
			x["ws1"] = "testin";
			y["ws"] = "test";
			Assert.IsTrue(x.HasFormWithSameContent(y));
			Assert.IsTrue(y.HasFormWithSameContent(x));
		}

		[Test]
		public void HasFormWithSameContent_DifferentWritingSystem_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			MultiTextBase y = new MultiTextBase();
			y["wss"] = "test";
			Assert.IsFalse(x.HasFormWithSameContent(y));
			Assert.IsFalse(y.HasFormWithSameContent(x));
		}

		[Test]
		public void HasFormWithSameContent_DifferentValue_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			MultiTextBase y = new MultiTextBase();
			y["ws"] = "tests";
			Assert.IsFalse(x.HasFormWithSameContent(y));
			Assert.IsFalse(y.HasFormWithSameContent(x));
		}


		[Test]
		public void HasFormWithSameContent_BothEmpty_True()
		{
			MultiTextBase x = new MultiTextBase();
			MultiTextBase y = new MultiTextBase();
			Assert.IsTrue(x.HasFormWithSameContent(y));
			Assert.IsTrue(y.HasFormWithSameContent(x));
		}



		[Test]
		public void SetAnnotation()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase.SetAnnotationOfAlternativeIsStarred("zz", true);
			Assert.AreEqual(String.Empty, multiTextBase.GetExactAlternative("zz"));
			Assert.IsTrue(multiTextBase.GetAnnotationOfAlternativeIsStarred("zz"));
			multiTextBase.SetAnnotationOfAlternativeIsStarred("zz", false);
			Assert.IsFalse(multiTextBase.GetAnnotationOfAlternativeIsStarred("zz"));
		}

		[Test]
		public void ClearingAnnotationOfEmptyAlternativeRemovesTheAlternative()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase.SetAnnotationOfAlternativeIsStarred("zz", true);
			multiTextBase.SetAnnotationOfAlternativeIsStarred("zz", false);
			Assert.IsFalse(multiTextBase.ContainsAlternative("zz"));
		}

		[Test]
		public void ClearingAnnotationOfNonEmptyAlternative()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase.SetAnnotationOfAlternativeIsStarred("zz", true);
			multiTextBase["zz"] = "hello";
			multiTextBase.SetAnnotationOfAlternativeIsStarred("zz", false);
			Assert.IsTrue(multiTextBase.ContainsAlternative("zz"));
		}

		[Test]
		public void EmptyingTextOfFlaggedAlternativeDoesNotDeleteIfFlagged()
		{
			// REVIEW: not clear really what behavior we want here, since user deletes via clearing text
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase["zz"] = "hello";
			multiTextBase.SetAnnotationOfAlternativeIsStarred("zz", true);
			multiTextBase["zz"] = "";
			Assert.IsTrue(multiTextBase.ContainsAlternative("zz"));
		}

		[Test]
		public void AnnotationOfMissingAlternative()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			Assert.IsFalse(multiTextBase.GetAnnotationOfAlternativeIsStarred("zz"));
			Assert.IsFalse(multiTextBase.ContainsAlternative("zz"), "should not cause the creation of the alt");
		}


		[Test]
		public void ContainsEqualForm_DifferentStarred_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			LanguageForm form = new LanguageForm();
			form.WritingSystemId = "ws";
			form.Form = "test";
			form.IsStarred = true;
			Assert.IsFalse(x.ContainsEqualForm(form));
		}


		[Test]
		public void HasFormWithSameContent_DifferentStarred_False()
		{
			MultiTextBase x = new MultiTextBase();
			x["ws"] = "test";
			MultiTextBase y = new MultiTextBase();
			y["ws"] = "test";
			y.SetAnnotationOfAlternativeIsStarred("ws", true);
			Assert.IsFalse(x.HasFormWithSameContent(y));
			Assert.IsFalse(y.HasFormWithSameContent(x));
		}


		[Test]
		public void HasFormWithSameContent_OneEmpty_False()
		{
			MultiTextBase x = new MultiTextBase();
			MultiTextBase y = new MultiTextBase();
			y["ws"] = "test";
			y.SetAnnotationOfAlternativeIsStarred("ws", true);
			Assert.IsFalse(x.HasFormWithSameContent(y));
			Assert.IsFalse(y.HasFormWithSameContent(x));
		}

		[Test]
		public void GetOrderedAndFilteredForms_EmptyIdList_GivesEmptyList()
		{
			MultiTextBase x = new MultiTextBase();
			x["one"] = "test";
			Assert.AreEqual(0, x.GetOrderedAndFilteredForms(new string[] { }).Length);
		}

		[Test]
		public void GetOrderedAndFilteredForms_EmptyMultiText_GivesEmptyList()
		{
			MultiTextBase x = new MultiTextBase();
			LanguageForm[] forms = x.GetOrderedAndFilteredForms(new string[] { "one", "three" });
			Assert.AreEqual(0, forms.Length);
		}

		[Test]
		public void GetOrderedAndFilteredForms_GivesFormsInOrder()
		{
			MultiTextBase x = new MultiTextBase();
			x["one"] = "1";
			x["two"] = "2";
			x["three"] = "3";
			LanguageForm[] forms = x.GetOrderedAndFilteredForms(new string[] {"one", "three","two" });
			Assert.AreEqual(3, forms.Length);
			Assert.AreEqual("1", forms[0].Form);
			Assert.AreEqual("3", forms[1].Form);
			Assert.AreEqual("2", forms[2].Form);
		}

		[Test]
		public void GetOrderedAndFilteredForms_DropsUnlistedForms()
		{
			MultiTextBase x = new MultiTextBase();
			x["one"] = "1";
			x["two"] = "2";
			x["three"] = "3";
			LanguageForm[] forms = x.GetOrderedAndFilteredForms(new string[] { "one", "three" });
			Assert.AreEqual(2, forms.Length);
			Assert.AreEqual("1", forms[0].Form);
			Assert.AreEqual("3", forms[1].Form);
		}

		[Test]
		public void SetAlternative_ThreeDifferentLanguages_LanguageFormsAreSortedByWritingSystem()
		{

			MultiTextBase multiTextBaseToPopulate = new MultiTextBase();
			multiTextBaseToPopulate.SetAlternative("fr", "fr Word3");
			multiTextBaseToPopulate.SetAlternative("de", "de Word1");
			multiTextBaseToPopulate.SetAlternative("en", "en Word2");
			Assert.AreEqual(3, multiTextBaseToPopulate.Forms.Length);
			Assert.AreEqual("de", multiTextBaseToPopulate.Forms[0].WritingSystemId);
			Assert.AreEqual("en", multiTextBaseToPopulate.Forms[1].WritingSystemId);
			Assert.AreEqual("fr", multiTextBaseToPopulate.Forms[2].WritingSystemId);
		}

		[Test]
		public void CompareTo_Null_ReturnsGreater()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase.SetAlternative("de", "Word 1");
			MultiTextBase multiTextBaseToCompare = null;
			Assert.AreEqual(1, multiTextBase.CompareTo(multiTextBaseToCompare));
		}

		[Test]
		public void CompareTo_MultiTextWithFewerForms_ReturnsGreater()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase.SetAlternative("de", "Word 1");
			MultiTextBase multiTextBaseToCompare = new MultiTextBase();
			Assert.AreEqual(1, multiTextBase.CompareTo(multiTextBaseToCompare));
		}

		[Test]
		public void CompareTo_MultiTextWithMoreForms_ReturnsLess()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			MultiTextBase multiTextBaseToCompare = new MultiTextBase();
			multiTextBaseToCompare.SetAlternative("de", "Word 1");
			Assert.AreEqual(-1, multiTextBase.CompareTo(multiTextBaseToCompare));
		}

		[Test]
		public void CompareTo_MultiTextWithNonIdenticalWritingSystemsAndFirstNonidenticalWritingSystemIsAlphabeticallyEarlier_ReturnsGreater()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase.SetAlternative("en", "Word 1");
			MultiTextBase multiTextBaseToCompare = new MultiTextBase();
			multiTextBaseToCompare.SetAlternative("de", "Word 1");
			Assert.AreEqual(1, multiTextBase.CompareTo(multiTextBaseToCompare));
		}

		[Test]
		public void CompareTo_MultiTextWithNonIdenticalWritingSystemsAndFirstNonidenticalWritingSystemIsAlphabeticallyLater_ReturnsLess()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase.SetAlternative("de", "Word 1");
			MultiTextBase multiTextBaseToCompare = new MultiTextBase();
			multiTextBaseToCompare.SetAlternative("en", "Word 1");
			Assert.AreEqual(-1, multiTextBase.CompareTo(multiTextBaseToCompare));
		}

		[Test]
		public void CompareTo_MultiTextWithNonIdenticalFormsAndFirstNonidenticalFormIsAlphabeticallyEarlier_ReturnsGreater()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase.SetAlternative("de", "Word 2");
			MultiTextBase multiTextBaseToCompare = new MultiTextBase();
			multiTextBaseToCompare.SetAlternative("de", "Word 1");
			Assert.AreEqual(1, multiTextBase.CompareTo(multiTextBaseToCompare));
		}

		[Test]
		public void CompareTo_MultiTextWithNonIdenticalFormsAndFirstNonidenticalformIsAlphabeticallyLater_ReturnsLess()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase.SetAlternative("de", "Word 1");
			MultiTextBase multiTextBaseToCompare = new MultiTextBase();
			multiTextBaseToCompare.SetAlternative("de", "Word 2");
			Assert.AreEqual(-1, multiTextBase.CompareTo(multiTextBaseToCompare));
		}

		[Test]
		public void CompareTo_IdenticalMultiText_ReturnsEqual()
		{
			MultiTextBase multiTextBase = new MultiTextBase();
			multiTextBase.SetAlternative("de", "Word 1");
			MultiTextBase multiTextBaseToCompare = new MultiTextBase();
			multiTextBaseToCompare.SetAlternative("de", "Word 1");
			Assert.AreEqual(0, multiTextBase.CompareTo(multiTextBaseToCompare));
		}
	}
}