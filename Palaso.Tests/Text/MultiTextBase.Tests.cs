using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
//using Exortech.NetReflector;
using NUnit.Framework;
using System.ComponentModel;
using Palaso.Text;
using System.Collections;

namespace Palaso.Tests.Text
{
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

		//        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		//        public void GetIndexerThrowsWhenAltIsMissing()
		//        {
		//            MultiTextBase text = new MultiTextBase();
		//            text["foo"] = "alpha";
		//            string s = text["gee"];
		//        }
		//
		//        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
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
			MultiTextBase MultiTextBase = new MultiTextBase();
			MultiTextBase["wsWithNullElement"] = null;
			MultiTextBase["wsWithEmptyElement"] = "";
			MultiTextBase["wsWithContent"] = "hello";
			Assert.AreEqual(String.Empty, MultiTextBase.GetExactAlternative("missingWs"));
			Assert.AreEqual(String.Empty, MultiTextBase.GetExactAlternative("wsWithEmptyElement"));
			Assert.AreEqual("hello", MultiTextBase.GetBestAlternative("missingWs"));
			Assert.AreEqual("hello", MultiTextBase.GetBestAlternative("wsWithEmptyElement"));
			Assert.AreEqual("hello*", MultiTextBase.GetBestAlternative("wsWithEmptyElement", "*"));
			Assert.AreEqual("hello", MultiTextBase.GetBestAlternative("wsWithNullElement"));
			Assert.AreEqual("hello*", MultiTextBase.GetBestAlternative("wsWithNullElement", "*"));
			Assert.AreEqual("hello", MultiTextBase.GetExactAlternative("wsWithContent"));
			Assert.AreEqual("hello", MultiTextBase.GetBestAlternative("wsWithContent"));
			Assert.AreEqual("hello", MultiTextBase.GetBestAlternative("wsWithContent", "*"));
		}


		[Test]
		public void SerializeWithXmlSerializer()
		{
			MultiTextBase text = new MultiTextBase();
			text["foo"] = "alpha";
			text["boo"] = "beta";
			string answer =
				@"<?xml version='1.0' encoding='utf-16'?>
<TestMultiTextHolder xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
  <name>
	<form lang='foo'>alpha</form>
	<form lang='boo'>beta</form>
  </name>
</TestMultiTextHolder>";
			CheckSerializeWithXmlSerializer(text, answer);
		}

		[Test]
		public void SerializeEmptyWithXmlSerializer()
		{
			MultiTextBase text = new MultiTextBase();
			string answer =
				@"<?xml version='1.0' encoding='utf-16'?>
<TestMultiTextHolder xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
  <name />
</TestMultiTextHolder>";
			CheckSerializeWithXmlSerializer(text, answer);
		}


		public void CheckSerializeWithXmlSerializer(MultiTextBase MultiTextBase, string answer)
		{

			XmlSerializer ser = new XmlSerializer(typeof(TestMultiTextHolder));

			StringWriter writer = new System.IO.StringWriter();
			TestMultiTextHolder holder = new TestMultiTextHolder();
			holder.Name = MultiTextBase;
			ser.Serialize(writer, holder);

			string mtxml = writer.GetStringBuilder().ToString();
			mtxml = mtxml.Replace('"', '\'');
			Debug.WriteLine(mtxml);
			Assert.AreEqual(answer, mtxml);
		}

		//        [Test]
		//        public void DeSerializeWithNetReflector()
		//        {
		//            MultiTextBase text = new MultiTextBase();
		//            text["foo"] = "alpha";
		//
		//            NetReflectorTypeTable t = new NetReflectorTypeTable();
		//            t.Add(typeof(MultiTextBase));
		//            t.Add(typeof(TestMultiTextHolder));
		//
		//
		//            string answer =
		//                @"<testMultiTextHolder>
		//                    <name>
		//				        <form starred='false' ws='en'>verb</form>
		//				        <form starred='false' ws='fr'>verbe</form>
		//				        <form starred='false' ws='es'>verbo</form>
		//			        </name>
		//                </testMultiTextHolder>";
		//            NetReflectorReader r = new NetReflectorReader(t);
		//            TestMultiTextHolder h = (TestMultiTextHolder)r.Read(answer);
		//            Assert.AreEqual(3, h._name.Count);
		//            Assert.AreEqual("verbo",h._name["es"]);
		//        }

		[Test]
		public void DeSerializesWithOldWsAttributes()
		{
			MultiTextBase t = DeserializeWithXmlSerialization(
				@"<TestMultiTextHolder>
					 <name>
						<form ws='en'>verb</form>
						<form ws='fr'>verbe</form>
						<form ws='es'>verbo</form>
					</name>
					</TestMultiTextHolder>
				");
			Assert.AreEqual(3, t.Forms.Length);
			Assert.AreEqual("verbo", t["es"]);
		}

		[Test]
		public void DeSerializesWithNewWsAttributes()
		{
			MultiTextBase t = DeserializeWithXmlSerialization(
				@"<TestMultiTextHolder>
					 <name>
						<form lang='en'>verb</form>
						<form lang='fr'>verbe</form>
						<form lang='es'>verbo</form>
					</name>
					</TestMultiTextHolder>
				");
			Assert.AreEqual(3, t.Forms.Length);
			Assert.AreEqual("verbo", t["es"]);
		}

		[Test]
		public void DeSerializesWhenEmpty()
		{
			MultiTextBase t = DeserializeWithXmlSerialization(
				@"  <TestMultiTextHolder>
						<name/>
					</TestMultiTextHolder>");
			Assert.AreEqual(0, t.Forms.Length);
		}


		private MultiTextBase DeserializeWithXmlSerialization(string answer)
		{
			StringReader r = new StringReader(answer);
			System.Xml.Serialization.XmlSerializer serializer = new XmlSerializer(typeof(TestMultiTextHolder));
			TestMultiTextHolder holder = serializer.Deserialize(r) as TestMultiTextHolder;
			return holder.Name;
		}

		//  [ReflectorType("testMultiTextHolder")]
		public class TestMultiTextHolder
		{
			[XmlIgnore]
			public MultiTextBase _name;

			[XmlElement("name")]
			//    [ReflectorProperty("name", typeof(MultiTextSerializorFactory), Required = true)]
				public MultiTextBase Name
			{
				get { return _name; }
				set { _name = value; }
			}
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
			MultiTextBase MultiTextBase = new MultiTextBase();
			MultiTextBase.SetAnnotationOfAlternativeIsStarred("zz", true);
			Assert.AreEqual(String.Empty, MultiTextBase.GetExactAlternative("zz"));
			Assert.IsTrue(MultiTextBase.GetAnnotationOfAlternativeIsStarred("zz"));
			MultiTextBase.SetAnnotationOfAlternativeIsStarred("zz", false);
			Assert.IsFalse(MultiTextBase.GetAnnotationOfAlternativeIsStarred("zz"));
		}

		[Test]
		public void ClearingAnnotationOfEmptyAlternativeRemovesTheAlternative()
		{
			MultiTextBase MultiTextBase = new MultiTextBase();
			MultiTextBase.SetAnnotationOfAlternativeIsStarred("zz", true);
			MultiTextBase.SetAnnotationOfAlternativeIsStarred("zz", false);
			Assert.IsFalse(MultiTextBase.ContainsAlternative("zz"));
		}

		[Test]
		public void ClearingAnnotationOfNonEmptyAlternative()
		{
			MultiTextBase MultiTextBase = new MultiTextBase();
			MultiTextBase.SetAnnotationOfAlternativeIsStarred("zz", true);
			MultiTextBase["zz"] = "hello";
			MultiTextBase.SetAnnotationOfAlternativeIsStarred("zz", false);
			Assert.IsTrue(MultiTextBase.ContainsAlternative("zz"));
		}

		[Test]
		public void EmptyingTextOfFlaggedAlternativeDoesNotDeleteIfFlagged()
		{
			// REVIEW: not clear really what behavior we want here, since user deletes via clearing text
			MultiTextBase MultiTextBase = new MultiTextBase();
			MultiTextBase["zz"] = "hello";
			MultiTextBase.SetAnnotationOfAlternativeIsStarred("zz", true);
			MultiTextBase["zz"] = "";
			Assert.IsTrue(MultiTextBase.ContainsAlternative("zz"));
		}

		[Test]
		public void AnnotationOfMisssingAlternative()
		{
			MultiTextBase MultiTextBase = new MultiTextBase();
			Assert.IsFalse(MultiTextBase.GetAnnotationOfAlternativeIsStarred("zz"));
			Assert.IsFalse(MultiTextBase.ContainsAlternative("zz"), "should not cause the creation of the alt");
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

	}
}