using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using SIL.Lift.Parsing;
using SIL.TestUtilities;
using SIL.Text;

namespace SIL.Lift.Tests
{

	[TestFixture]
	public class MultiTextAsPalasoDataObjectPropertyCloneableTests : CloneableTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewCloneable()
		{
			return new MultiText();
		}

		public override string ExceptionList =>
			//PropertyChanged: No good way to clone eventhandlers
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			"|Parent|PropertyChanged|";

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
							 {
								 new ValuesToSet(new List<string>{"to", "be"}, new List<string>{"!","to","be"}),
								 new ValuesToSet(new []{new LanguageForm("en", "en_form", null)}, new []{new LanguageForm("de", "de_form", null)})
							 };
			}
		}
	}

	// ReSharper disable InconsistentNaming
	[TestFixture]
	public class MultiTextTests
	{
		private bool _gotHandlerNotice;

		[SetUp]
		public void Setup() {}

		[Test]
		public void Notification()
		{
			_gotHandlerNotice = false;
			MultiText text = new MultiText();
			text.PropertyChanged += propertyChangedHandler;
			text.SetAlternative("zox", "");
			Assert.IsTrue(_gotHandlerNotice);
		}

		private void propertyChangedHandler(object sender, PropertyChangedEventArgs e)
		{
			_gotHandlerNotice = true;
		}

		[Test]
		public void MergedGuyHasCorrectParentsOnForms()
		{
			MultiText x = new MultiText();
			x["a"] = "alpha";
			MultiText y = new MultiText();
			y["b"] = "beta";
			x.MergeIn(y);
			Assert.AreSame(y, y.Find("b").Parent);
			Assert.AreSame(x, x.Find("b").Parent);
		}


		/// <summary>
		/// RFC5646 calls for case insensitivity
		/// </summary>
		[Test]
		public void Find_QueryHasDIfferentCase_StillFinds()
		{
			MultiText x = new MultiText();
			x["aBc"] = "alpha";
			Assert.AreSame("alpha", x.Find("AbC").Form);
			Assert.AreSame(x.Find("aBc"), x.Find("AbC"));
		}

		[Test]
		public void Create_LiftMultiTextWithSpansInForms_ReturnsMultiTextWithSpansInForms()
		{
			LiftString liftStringToConvert = new LiftString("Text to Mark Up!");
			LiftSpan span1 = new LiftSpan(0, 4, "", "", "");
			liftStringToConvert.Spans.Add(span1);

			LiftMultiText liftMultiTextWithSpansInForms = new LiftMultiText();
			liftMultiTextWithSpansInForms.Add("de",liftStringToConvert);

			MultiText multiText = MultiText.Create(liftMultiTextWithSpansInForms);
			Assert.AreEqual("<span>Text</span> to Mark Up!", multiText["de"]);
		}

		[Test]
		public void Create_LiftMultiTextWithOutSpansInForms_ReturnsMultiTextWithOutSpansInForms()
		{
			LiftString liftStringToConvert = new LiftString("No spans here!");

			LiftMultiText liftMultiTextWithSpansInForms = new LiftMultiText();
			liftMultiTextWithSpansInForms.Add("de", liftStringToConvert);

			MultiText multiText = MultiText.Create(liftMultiTextWithSpansInForms);
			Assert.AreEqual("No spans here!", multiText["de"]);
		}

		[Test]
		public void Create_Null_Throws()
		{
			Assert.Throws<ArgumentNullException>(() =>
				MultiText.Create((LiftMultiText)null));
		}

		[Test]
		public void GetFormWithSpans_WritingSystemLinkedToFormWithSpans_ReturnsFormWithSpans()
		{
			LiftString liftStringToConvert = new LiftString("Text to Mark Up!");
			LiftSpan span1 = new LiftSpan(0, 4, "", "", "");
			liftStringToConvert.Spans.Add(span1);

			LiftMultiText liftMultiTextWithSpansInForms = new LiftMultiText();
			liftMultiTextWithSpansInForms.Add("de", liftStringToConvert);

			MultiText multiText = MultiText.Create(liftMultiTextWithSpansInForms);
			Assert.AreEqual("<span>Text</span> to Mark Up!", multiText["de"]);
		}

		[Test]
		public void GetFormWithSpans_WritingSystemLinkedToFormWithOutSpans_ReturnsFormWithOutSpans()
		{
			LiftString liftStringToConvert = new LiftString("No spans here!");

			LiftMultiText liftMultiTextWithSpansInForms = new LiftMultiText();
			liftMultiTextWithSpansInForms.Add("de", liftStringToConvert);

			MultiText multiText = MultiText.Create(liftMultiTextWithSpansInForms);
			Assert.AreEqual("No spans here!", multiText["de"]);
		}

		[Test]
		public void ConvertLiftStringToSimpleStringWithMarkers_TextWithNoSpan()
		{
			LiftString liftStringToConvert = new LiftString("No Markers Here!");
			string convertedString = MultiText.ConvertLiftStringToSimpleStringWithMarkers(liftStringToConvert);
			Assert.AreEqual("No Markers Here!", convertedString);
		}

		[Test]
		public void ConvertLiftStringToSimpleStringWithMarkers_TextWithOneSpan()
		{
			LiftString liftStringToConvert = new LiftString("Text to Mark Up!");
			LiftSpan span1 = new LiftSpan(0, 4, "", "", "");
			liftStringToConvert.Spans.Add(span1);
			string convertedString = MultiText.ConvertLiftStringToSimpleStringWithMarkers(liftStringToConvert);
			Assert.AreEqual("<span>Text</span> to Mark Up!", convertedString);
		}

		[Test]
		public void ConvertLiftStringToSimpleStringWithMarkers_TextWithTwoSpans()
		{
			LiftString liftStringToConvert = new LiftString("Text to Mark Up!");
			LiftSpan span1 = new LiftSpan(0, 4, "", "", "");
			LiftSpan span2 = new LiftSpan(8, 4, "", "", "");
			liftStringToConvert.Spans.Add(span1);
			liftStringToConvert.Spans.Add(span2);

			string convertedString = MultiText.ConvertLiftStringToSimpleStringWithMarkers(liftStringToConvert);
			Assert.AreEqual("<span>Text</span> to <span>Mark</span> Up!", convertedString);
		}

		[Test]
		public void ConvertLiftStringToSimpleStringWithMarkers_TextWithNestedSpans()
		{
			LiftString liftStringToConvert = new LiftString("Text to Mark Up!");
			LiftSpan span1 = new LiftSpan(0, 12, "", "", "");
			LiftSpan span2 = new LiftSpan(5, 2, "", "", "");
			liftStringToConvert.Spans.Add(span1);
			liftStringToConvert.Spans.Add(span2);

			string convertedString = MultiText.ConvertLiftStringToSimpleStringWithMarkers(liftStringToConvert);
			Assert.AreEqual("<span>Text <span>to</span> Mark</span> Up!", convertedString);
		}

		[Test]
		public void ConvertLiftStringToSimpleStringWithMarkers_TextWithTwoSpansAtSamePosition()
		{
			LiftString liftStringToConvert = new LiftString("Text to Mark Up!");
			LiftSpan span1 = new LiftSpan(0, 4, "", "", "");
			LiftSpan span2 = new LiftSpan(0, 4, "", "", "");
			liftStringToConvert.Spans.Add(span1);
			liftStringToConvert.Spans.Add(span2);

			string convertedString = MultiText.ConvertLiftStringToSimpleStringWithMarkers(liftStringToConvert);
			Assert.AreEqual("<span><span>Text</span></span> to Mark Up!", convertedString);
		}

		[Test]
		public void ConvertLiftStringToSimpleStringWithMarkers_TextWithOneSpanEndingWhereASecondStarts_CloseTagOfFirstAppearsBeforeStartingTagOfSecond()
		{
			LiftString liftStringToConvert = new LiftString("Text to Mark Up!");
			LiftSpan span1 = new LiftSpan(0, 4, "", "", "");
			LiftSpan span2 = new LiftSpan(4, 3, "", "", "");
			liftStringToConvert.Spans.Add(span1);
			liftStringToConvert.Spans.Add(span2);

			string convertedString = MultiText.ConvertLiftStringToSimpleStringWithMarkers(liftStringToConvert);
			Assert.AreEqual("<span>Text</span><span> to</span> Mark Up!", convertedString);
		}

		[Test]
		public void ConvertLiftStringToSimpleStringWithMarkers_SpanWithLang_ReturnsMarkerWithLang()
		{
			LiftString liftStringToConvert = new LiftString("Text to Mark Up!");
			LiftSpan span1 = new LiftSpan(0, 4, "language", "", "");
			liftStringToConvert.Spans.Add(span1);

			string convertedString = MultiText.ConvertLiftStringToSimpleStringWithMarkers(liftStringToConvert);
			Assert.AreEqual("<span lang=\"language\">Text</span> to Mark Up!", convertedString);
		}

		[Test]
		public void ConvertLiftStringToSimpleStringWithMarkers_SpanWithHref_ReturnsMarkerWithHref()
		{
			LiftString liftStringToConvert = new LiftString("Text to Mark Up!");
			LiftSpan span1 = new LiftSpan(0, 4, "", "", "reference");
			liftStringToConvert.Spans.Add(span1);

			string convertedString = MultiText.ConvertLiftStringToSimpleStringWithMarkers(liftStringToConvert);
			Assert.AreEqual("<span href=\"reference\">Text</span> to Mark Up!", convertedString);
		}

		[Test]
		public void ConvertLiftStringToSimpleStringWithMarkers_SpanWithClass_ReturnsMarkerWithClass()
		{
			LiftString liftStringToConvert = new LiftString("Text to Mark Up!");
			LiftSpan span1 = new LiftSpan(0, 4, "", "ClassType", "");
			liftStringToConvert.Spans.Add(span1);

			string convertedString = MultiText.ConvertLiftStringToSimpleStringWithMarkers(liftStringToConvert);
			Assert.AreEqual("<span class=\"ClassType\">Text</span> to Mark Up!", convertedString);
		}

		[Test]
		public void StripMarkers_FormWithOutSpans_FormIsUntouched()
		{
			Assert.AreEqual("No spans here!", MultiText.StripMarkers("No spans here!"));
		}

		[Test]
		public void StripMarkers_FormWithTwoSpan_MarkersAreStripped()
		{
			Assert.AreEqual("Please strip this text!", MultiText.StripMarkers("<span>Please</span> strip <span href = \"ReferenceEquals\">this</span> text!"));
		}

		[Test]
		public void StripMarkers_notRealXml_MarkersAreTreatedAsText()
		{
			Assert.AreEqual("<spanPlease</span> strip <span href = \"ReferenceEqual>this<span> text!", MultiText.StripMarkers("<spanPlease</span> strip <span href = \"ReferenceEqual>this<span> text!"));
		}

		[Test]
		public void StripMarkers_FormNull_Throws()
		{
			Assert.Throws<ArgumentNullException>(() =>
MultiText.StripMarkers(null));
		}

		[Test]
		public void Equals_FirstHasFewerEmbeddedXmlElements_False()
		{
			var x = new MultiText();
			x.EmbeddedXmlElements.Add("string1");
			var y = new MultiText();
			y.EmbeddedXmlElements.Add("string1");
			y.EmbeddedXmlElements.Add("string2");
			Assert.IsFalse(x.Equals(y));
			Assert.IsFalse(y.Equals(x));
		}

		[Test]
		public void Equals_FirstHasMoreEmbeddedXmlElements_False()
		{
			var x = new MultiText();
			x.EmbeddedXmlElements.Add("string1");
			x.EmbeddedXmlElements.Add("string2");
			var y = new MultiText();
			y.EmbeddedXmlElements.Add("string1");
			Assert.IsFalse(x.Equals(y));
			Assert.IsFalse(y.Equals(x));
		}

		[Test]
		public void Equals_DifferentEmbeddedXmlElements_False()
		{
			var x = new MultiText();
			x.EmbeddedXmlElements.Add("string1");
			var y = new MultiText();
			y.EmbeddedXmlElements.Add("string2");
			Assert.IsFalse(x.Equals(y));
			Assert.IsFalse(y.Equals(x));
		}

		[Test]
		public void ObjectEquals_FirstHasFewerEmbeddedXmlElements_False()
		{
			var x = new MultiText();
			x.EmbeddedXmlElements.Add("string1");
			var y = new MultiText();
			y.EmbeddedXmlElements.Add("string1");
			y.EmbeddedXmlElements.Add("string2");
			Assert.IsFalse(x.Equals((object) y));
			Assert.IsFalse(y.Equals((object) x));
		}

		[Test]
		public void ObjectEquals_FirstHasMoreEmbeddedXmlElements_False()
		{
			var x = new MultiText();
			x.EmbeddedXmlElements.Add("string1");
			x.EmbeddedXmlElements.Add("string2");
			var y = new MultiText();
			y.EmbeddedXmlElements.Add("string1");
			Assert.IsFalse(x.Equals((object) y));
			Assert.IsFalse(y.Equals((object) x));
		}

		[Test]
		public void ObjectEquals_DifferentEmbeddedXmlElements_False()
		{
			var x = new MultiText();
			x.EmbeddedXmlElements.Add("string1");
			var y = new MultiText();
			y.EmbeddedXmlElements.Add("string2");
			Assert.IsFalse(x.Equals((object) y));
			Assert.IsFalse(y.Equals((object) x));
		}

		[Test]
		public void ObjectEquals_DifferentNumberOfForms_False()
		{
			var x = new MultiText();
			x["ws"] = "test";
			x["ws2"] = "test";
			var y = new MultiText();
			y["ws"] = "test";
			Assert.IsFalse(x.Equals((object)y));
			Assert.IsFalse(y.Equals((object)x));
		}

		[Test]
		public void ObjectEquals_SameContent_True()
		{
			var x = new MultiText();
			x["ws"] = "test";
			MultiText y = new MultiText();
			y.MergeIn(x);
			Assert.IsTrue(x.Equals((object)y));
			Assert.IsTrue(y.Equals((object)x));
		}

		[Test]
		public void ObjectEquals_Identity_True()
		{
			var x = new MultiText();
			Assert.IsTrue(x.Equals((object)x));
		}

		[Test]
		public void ObjectEquals_DifferentValues_False()
		{
			var x = new MultiText();
			x["ws"] = "test";
			var y = new MultiText();
			y["ws"] = "test1";
			Assert.IsFalse(x.Equals((object)y));
			Assert.IsFalse(y.Equals((object)x));
		}

		[Test]
		public void ObjectEquals_DifferentWritingSystems_False()
		{
			var x = new MultiText();
			x["ws"] = "test";
			var y = new MultiText();
			y["ws1"] = "test";
			Assert.IsFalse(x.Equals((object)y));
			Assert.IsFalse(y.Equals((object)x));
		}

		[Test]
		public void ObjectEquals_Null_False()
		{
			var x = new MultiText();
			x["ws"] = "test";
			Assert.IsFalse(x.Equals((object)null));
		}

		[Test]
		public void Equals_Null_False()
		{
			var x = new MultiText();
			x["ws"] = "test";
			Assert.IsFalse(x.Equals(null));
		}

		[Test]
		public void Equals_DifferentNumberOfForms_False()
		{
			var x = new MultiText();
			x["ws"] = "test";
			x["ws2"] = "test";
			var y = new MultiText();
			y["ws"] = "test";
			Assert.IsFalse(x.Equals(y));
			Assert.IsFalse(y.Equals(x));
		}

		[Test]
		public void Equals_SameContent_True()
		{
			var x = new MultiText();
			x["ws"] = "test";
			var y = new MultiText();
			y.MergeIn(x);
			Assert.IsTrue(x.Equals(y));
			Assert.IsTrue(y.Equals(x));
		}

		[Test]
		public void Equals_Identity_True()
		{
			var x = new MultiText();
			Assert.IsTrue(x.Equals(x));
		}

		[Test]
		public void Equals_DifferentValues_False()
		{
			var x = new MultiText();
			x["ws"] = "test";
			var y = new MultiText();
			y["ws"] = "test1";
			Assert.IsFalse(x.Equals(y));
			Assert.IsFalse(y.Equals(x));
		}

		[Test]
		public void Equals_DifferentWritingSystems_False()
		{
			var x = new MultiText();
			x["ws"] = "test";
			var y = new MultiText();
			y["ws1"] = "test";
			Assert.IsFalse(x.Equals(y));
			Assert.IsFalse(y.Equals(x));
		}
	}
	// ReSharper restore InconsistentNaming
}