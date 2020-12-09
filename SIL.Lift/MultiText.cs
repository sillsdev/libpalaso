//using Exortech.NetReflector;
//using Exortech.NetReflector.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SIL.Extensions;
using SIL.Lift.Parsing;
using SIL.Text;

namespace SIL.Lift
{
	/// <summary>
	/// MultiText holds an array of LanguageForms, indexed by writing system ID.
	/// </summary>
	//NO: we haven't been able to do a reasonably compact xml representation except with custom deserializer
	//[ReflectorType("multiText")]
	[XmlInclude(typeof (LanguageForm))]
	public class MultiText : MultiTextBase, IPalasoDataObjectProperty, IReportEmptiness, IXmlSerializable
	{
		public List<string> EmbeddedXmlElements = new List<string>();

		public MultiText(PalasoDataObject parent)
		{
			Parent = parent;
		}

		public MultiText() {}

		/// <summary>
		/// We added this pesky "backreference" solely to enable fast
		/// searching in db4o (5.5), which could
		///  find strings fast, but can't be queried for the owner
		/// quickly, if there is an intervening collection.  Since
		/// each string in WeSay is part of a collection of writing
		/// system alternatives, that means we can't quickly get
		/// an answer, for example, to the question Get all
		/// the Entries that contain a senses which have the reversal "cat".
		///
		/// NOW (2009) it is a TODO to look at removing this.
		///
		/// Using this field, we can do a query asking for all
		/// the LanguageForms matching "cat".
		/// This can all be done in a single, fast query.
		///  In code, we can then follow the
		/// LanguageForm._parent up to the multitext, then this _parent
		/// up to it's owner, etc., on up the hierarchy to get the Entries.
		///
		/// Subclasses should provide a property which set the proper class.
		///
		/// 23 Jan 07, note: starting to switch to using these for notifying parent of changes, too.
		/// </summary>
		[XmlIgnore]
		public PalasoDataObject Parent { get; set; }

		/// <summary>
		/// Subclasses should provide a "Parent" property which set the proper class.
		/// </summary>
		public PalasoDataObject ParentAsObject => Parent;

		///<summary>
		/// required by IXmlSerializable
		///</summary>
		public XmlSchema GetSchema()
		{
			return null;
		}

		///<summary>
		/// required by IXmlSerializable.
		/// This is wierd and sad, but this is tuned to the format we want in OptionLists.
		///</summary>
		public virtual void ReadXml(XmlReader reader)
		{
			//enhance: this is a maximally inefficient way to read it, but ok if we're just using it for option lists
			XmlDocument d = new XmlDocument();
			d.LoadXml(reader.ReadOuterXml());
			foreach (XmlNode form in d.SelectNodes("*/form"))
			{
				string s = form.InnerText.Trim().Replace('\n', ' ').Replace("  ", " ");
				if (form.Attributes.GetNamedItem("ws") != null) //old style, but out there
				{
					SetAlternative(form.Attributes["ws"].Value, s);
				}
				else
				{
					SetAlternative(form.Attributes["lang"].Value, s);
				}
			}
			//reader.ReadEndElement();
		}

		///<summary>
		/// required by IXmlSerializable.
		/// This is wierd and sad, but this is tuned to the format we want in OptionLists.
		///</summary>
		public void WriteXml(XmlWriter writer)
		{
			foreach (LanguageForm form in Forms)
			{
				writer.WriteStartElement("form");
				writer.WriteAttributeString("lang", form.WritingSystemId);
				//notice, no <text> wrapper

				//the following makes us safe against codes like 0x1F, which can be easily
				//introduced via a right-click menu in a standard edit box (at least on windows)
				writer.WriteString(form.Form.EscapeAnyUnicodeCharactersIllegalInXml());
				writer.WriteEndElement();
			}
		}

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject => Empty;

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay => !Empty;

		public bool ShouldBeRemovedFromParentDueToEmptiness => Empty;

		/*
		/// <summary>
		/// skip those forms which are in audio writing systems
		/// </summary>
		public IList<LanguageForm> GetActualTextForms(WritingSystemCollection writingSytems)
		{
			var x= Forms.Where((f) => !writingSytems[f.WritingSystemId].IsAudio);
			return new List<LanguageForm>(x);
		}

		public IList<LanguageForm> GetAudioForms(WritingSystemCollection writingSytems)
		{
			var x = Forms.Where((f) => writingSytems[f.WritingSystemId].IsAudio);
			return new List<LanguageForm>(x);
		}
		*/
		public void RemoveEmptyStuff()
		{
			List<LanguageForm> condemened = new List<LanguageForm>();
			foreach (LanguageForm f in Forms)
			{
				if (string.IsNullOrEmpty(f.Form))
				{
					condemened.Add(f);
				}
			}
			foreach (LanguageForm f in condemened)
			{
				RemoveLanguageForm(f);
			}
		}

		#endregion

		public new static MultiText Create(Dictionary<string, string> forms)
		{
			MultiText m = new MultiText();
			CopyForms(forms, m);
			return m;
		}

		public static MultiText Create(LiftMultiText liftMultiText)
		{
			if(liftMultiText == null)
			{
				throw new ArgumentNullException("liftMultiText");
			}
			MultiText m = new MultiText();
			Dictionary<string, string> forms = new Dictionary<string, string>();
			foreach (KeyValuePair<string, LiftString> pair in liftMultiText)
			{
				if (pair.Value != null)
				{
					forms.Add(pair.Key, ConvertLiftStringToSimpleStringWithMarkers(pair.Value));
				}
			}
			CopyForms(forms, m);
			return m;
		}

		public static MultiText Create(Dictionary<string, string> forms, Dictionary<string, List<LiftSpan>> spans)
		{
			if (forms == null)
				throw new ArgumentNullException("forms");
			if (spans == null)
				spans = new Dictionary<string, List<LiftSpan>>();
			MultiText m = new MultiText();
			CopyForms(forms, m);
			m.CopySpans(spans);
			return m;
		}

		void CopySpans(Dictionary<string, List<LiftSpan>> spans)
		{
			foreach (var key in spans.Keys)
			{
				LanguageForm form = Find(key);
				if (form == null)
					continue;
				foreach (var span in spans[key])
				{
					form.AddSpan(span.Index, span.Length, span.Lang, span.Class, span.LinkURL);
				}
			}
		}


		public static string ConvertLiftStringToSimpleStringWithMarkers(LiftString liftString)
		{
			string stringWithSpans = liftString.Text;
			SortedDictionary<KeyValuePair<int, string>, object> spanSorter=
				new SortedDictionary<KeyValuePair<int, string>, object>(new SpanPositionComparer());

			//Sort the Span markers by position
			foreach (LiftSpan span in liftString.Spans)
			{
				string openMarker = BuildOpenMarker(span);
				spanSorter.Add(new KeyValuePair<int, string>(span.Index, openMarker), null);

				string closeMarker = "</span>";
				spanSorter.Add(new KeyValuePair<int, string>(span.Index + span.Length, closeMarker), null);
			}

			//Add the Span Markers one at a time from back to front
			foreach (KeyValuePair<KeyValuePair<int, string>, object> positionAndSpan in spanSorter)
			{
				stringWithSpans = stringWithSpans.Insert(positionAndSpan.Key.Key, positionAndSpan.Key.Value);
			}
			return stringWithSpans;
		}

		private static string BuildOpenMarker(LiftSpan span)
		{
			string openMarker = string.Format(
				"<span");
			if (!String.IsNullOrEmpty(span.Lang))
			{
				openMarker += " lang=\"" + span.Lang +"\"";
			}
			if (!String.IsNullOrEmpty(span.LinkURL))
			{
				openMarker += " href=\"" + span.LinkURL +"\"";
			}
			if (!String.IsNullOrEmpty(span.Class))
			{
				openMarker += " class=\"" + span.Class +"\"";
			}
			openMarker += ">";
			return openMarker;
		}

		private class SpanPositionComparer:IComparer<KeyValuePair<int, string>>
		{
			public int Compare(KeyValuePair<int, string> positionAndMarkerX, KeyValuePair<int, string> positionAndMarkerY)
			{
				if(positionAndMarkerX.Key < positionAndMarkerY.Key)
				{
					return 1;
				}
				if (positionAndMarkerX.Key > positionAndMarkerY.Key)
				{
					return -1;
				}
				else
				{
					if(positionAndMarkerX.Value == "</span>")
					{
						return 1;
					}
					if(positionAndMarkerY.Value == "</span>")
					{
						return -1;
					}
					else
					{
						return 1;
					}
				}
			}
		}

		public static string StripMarkers(string textToStrip)
		{
			if(string.IsNullOrEmpty(textToStrip))
			{
				throw new ArgumentNullException("textToStrip");
			}

			string wrappedTextToStrip = "<text>" + textToStrip + "</text>";
			XmlReaderSettings fragmentReaderSettings = new XmlReaderSettings();
			fragmentReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;
			XmlReader testerForWellFormedness = XmlReader.Create(new StringReader(wrappedTextToStrip));

			string strippedString = "";
			try
			{
				while(testerForWellFormedness.Read())
				{
					strippedString += testerForWellFormedness.ReadString();
				}
			}
			catch
			{
				//If the text is not well formed XML just return it as text
				strippedString = textToStrip;
			}
			return strippedString;
		}

		public bool ContainsEqualForm(string form, string writingSystemId)
		{
			return null != this.Forms.FirstOrDefault(f=> f.WritingSystemId ==writingSystemId && f.Form == form);
		}

		public virtual IPalasoDataObjectProperty Clone()
		{
			var clone = new MultiText();
			clone.EmbeddedXmlElements = new List<string>(EmbeddedXmlElements);
			clone.Forms = Forms.Select(f => (LanguageForm) f.Clone()).ToArray();
			return clone;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as MultiText);
		}

		public virtual bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals(other as MultiText);
		}

		public bool Equals(MultiText other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (EmbeddedXmlElements.Count != other.EmbeddedXmlElements.Count) return false;
			if (!EmbeddedXmlElements.SequenceEqual(other.EmbeddedXmlElements)) return false;
			if (!base.Equals(other)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 43;
				hash *= 67 + Count.GetHashCode();
				hash *= 67 + EmbeddedXmlElements?.GetHashCode() ?? 0;
				hash *= 67 + base.GetHashCode();
				return hash;
			}
		}
	}
}