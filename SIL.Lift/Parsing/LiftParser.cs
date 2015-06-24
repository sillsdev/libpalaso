using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using SIL.Lift.Merging;
using SIL.Lift.Validation;

namespace SIL.Lift.Parsing
{
	/// <summary>
	/// This class takes a file or DOM of lift and makes calls on a supplied "merger" object for what it finds there.
	/// This design allows the same parser to be used for WeSay, FLEx, and unit tests, which all have different
	/// domain models which they populate based on these calls.
	/// </summary>
	public class LiftParser<TBase, TEntry, TSense, TExample>
		where TBase : class
		where TEntry : class, TBase
		where TSense : class, TBase
		where TExample : class, TBase
	{
		// Parsing Errors should throw an exception
		///<summary></summary>
		public event EventHandler<ErrorArgs> ParsingWarning;
		///<summary></summary>
		public event EventHandler<StepsArgs> SetTotalNumberSteps;
		///<summary></summary>
		public event EventHandler<ProgressEventArgs> SetStepsCompleted;
		///<summary></summary>
		public event EventHandler<MessageArgs> SetProgressMessage;

		private readonly ILexiconMerger<TBase, TEntry, TSense, TExample> _merger;
	  //  private readonly ILimittedMerger<TBase, TEntry, TSense, TExample> _limitedMerger;
		private const string _wsAttributeLabel = "lang";

		private string _pathToLift;
		private bool _cancelNow;
		private DateTime _defaultCreationModificationUTC=default(DateTime);
		private ILiftChangeDetector _changeDetector;
		private ILiftChangeReport _changeReport;

		///<summary>
		/// Constructor.
		///</summary>
		public LiftParser(ILexiconMerger<TBase, TEntry, TSense, TExample> merger)
		{
			_merger = merger;
		 //   _limitedMerger = _merger as ILimittedMerger<TBase, TEntry, TSense, TExample>;
		}

		/// <summary>
		///
		/// </summary>
//        public virtual void ReadLiftDom(XmlDocument doc, DateTime defaultCreationModificationUTC)
//        {
//            DefaultCreationModificationUTC = defaultCreationModificationUTC;
//
//            XmlNodeList entryNodes = doc.SelectNodes("/lift/entry");
//            int numberOfEntriesRead = 0;
//            const int kProgressReportingInterval = 50;
//            int nextProgressPoint = numberOfEntriesRead + kProgressReportingInterval;
//            ProgressTotalSteps = entryNodes.Count;
//            foreach (XmlNode node in entryNodes)
//            {
//                ReadEntry(node);
//                numberOfEntriesRead++;
//                if (numberOfEntriesRead >= nextProgressPoint)
//                {
//                    ProgressStepsCompleted = numberOfEntriesRead;
//                    nextProgressPoint = numberOfEntriesRead + kProgressReportingInterval;
//                }
//                if (_cancelNow)
//                {
//                    break;
//                }
//            }
//        }

		internal void ReadRangeElement(string range, XmlNode node)
		{
			string id = Utilities.GetStringAttribute(node, "id");
			string guid = Utilities.GetOptionalAttributeString(node, "guid");
			string parent = Utilities.GetOptionalAttributeString(node, "parent");
			LiftMultiText description = LocateAndReadMultiText(node, "description");
			LiftMultiText label = LocateAndReadMultiText(node, "label");
			LiftMultiText abbrev = LocateAndReadMultiText(node, "abbrev");
			_merger.ProcessRangeElement(range, id, guid, parent, description, label, abbrev, node.OuterXml);
		}

		internal void ReadFieldDefinition(XmlNode node)
		{
			string tag = Utilities.GetStringAttribute(node, "tag"); //NB: Tag is correct (as of v12).  Changed to "type" only on *instances* of <field> element
			LiftMultiText description = ReadMultiText(node);
			_merger.ProcessFieldDefinition(tag, description);
		}

		internal TEntry ReadEntry(XmlNode node)
		{
			if (_changeReport != null)
			{
				string id = Utilities.GetOptionalAttributeString(node, "id");
				if (ChangeReport.GetChangeType(id) == LiftChangeReport.ChangeType.None)
					return default(TEntry);
			}

			Extensible extensible = ReadExtensibleElementBasics(node);
			DateTime dateDeleted = GetOptionalDate(node, "dateDeleted", default(DateTime));
			if(dateDeleted != default(DateTime))
			{
				_merger.EntryWasDeleted(extensible, dateDeleted);
				return default(TEntry);
			}

			int homograph = 0;
			string order = Utilities.GetOptionalAttributeString(node, "order");
			if (!String.IsNullOrEmpty(order))
			{
				if (!Int32.TryParse(order, out homograph))
					homograph = 0;
			}
			TEntry entry = _merger.GetOrMakeEntry(extensible, homograph);
			if (entry == null)// pruned
			{
				return entry;
			}


			LiftMultiText lexemeForm = LocateAndReadMultiText(node, "lexical-unit");
			if (!lexemeForm.IsEmpty)
			{
				_merger.MergeInLexemeForm(entry, lexemeForm);
			}
			LiftMultiText citationForm = LocateAndReadMultiText(node, "citation");
			if (!citationForm.IsEmpty)
			{
				_merger.MergeInCitationForm(entry, citationForm);
			}

			ReadNotes(node, entry);

			var nodes = node.SelectNodes("sense");
			if (nodes != null)
			{
				foreach (XmlNode n in nodes)
					ReadSense(n, entry);
			}
			nodes = node.SelectNodes("relation");
			if (nodes != null)
			{
				foreach (XmlNode n in nodes)
					ReadRelation(n, entry);
			}
			nodes = node.SelectNodes("variant");
			if (nodes != null)
			{
				foreach (XmlNode n in nodes)
					ReadVariant(n, entry);
			}
			nodes = node.SelectNodes("pronunciation");
			if (nodes != null)
			{
				foreach (XmlNode n in nodes)
					ReadPronunciation(n, entry);
			}
			nodes = node.SelectNodes("etymology");
			if (nodes != null)
			{
				foreach (XmlNode n in nodes)
					ReadEtymology(n, entry);
			}
			ReadExtensibleElementDetails(entry, node);
			_merger.FinishEntry(entry);
			return entry;
		}

		private void ReadNotes(XmlNode node, TBase e)
		{
			// REVIEW (SRMc): Should we detect multiple occurrences of the same
			// type of note?  See ReadExtensibleElementDetails() for how field
			// elements are handled in this regard.
			var nodes = node.SelectNodes("note");
			if (nodes != null)
			{
				foreach (XmlNode noteNode in nodes)
				{
					string noteType = Utilities.GetOptionalAttributeString(noteNode, "type");
					LiftMultiText noteText = ReadMultiText(noteNode);
					_merger.MergeInNote(e, noteType, noteText, noteNode.OuterXml);
				}
			}
		}

		private void ReadRelation(XmlNode n, TBase parent)
		{
			string targetId = Utilities.GetStringAttribute(n, "ref");
			string relationFieldName = Utilities.GetStringAttribute(n, "type");

			_merger.MergeInRelation(parent, relationFieldName, targetId, n.OuterXml);
		}

		private void ReadPronunciation(XmlNode node, TEntry entry)
		{
//            if (_limitedMerger != null && _limitedMerger.DoesPreferXmlForPhonetic)
//            {
//                _limitedMerger.MergeInPronunciation(entry, node.OuterXml);
//                return;
//            }

			LiftMultiText contents = ReadMultiText(node);
			TBase pronunciation = _merger.MergeInPronunciation(entry, contents, node.OuterXml);
			if (pronunciation != null)
			{
				var nodes = node.SelectNodes("media");
				if (nodes != null)
				{
					foreach (XmlNode n in nodes)
						ReadMedia(n, pronunciation);
				}
				ReadExtensibleElementDetails(pronunciation, node);
			}
		}

		private void ReadVariant(XmlNode node, TEntry entry)
		{
			LiftMultiText contents = ReadMultiText(node);
			TBase variant = _merger.MergeInVariant(entry, contents, node.OuterXml);
			if (variant != null)
				ReadExtensibleElementDetails(variant, node);
		}

		private void ReadEtymology(XmlNode node, TEntry entry)
		{
//            if (_limitedMerger != null && _limitedMerger.DoesPreferXmlForEtymology)
//            {
//                _limitedMerger.MergeInEtymology(entry, node.OuterXml);
//                return;
//            }

			string source = Utilities.GetOptionalAttributeString(node, "source");
			string type = Utilities.GetOptionalAttributeString(node, "type");
			LiftMultiText form = LocateAndReadMultiText(node, null);
			LiftMultiText gloss = LocateAndReadOneElementPerFormData(node, "gloss");
			TBase etymology = _merger.MergeInEtymology(entry, source, type, form, gloss, node.OuterXml);
			if (etymology != null)
				ReadExtensibleElementDetails(etymology, node);
		}

		private void ReadPicture(XmlNode n, TSense parent)
		{
			string href = Utilities.GetStringAttribute(n, "href");
			LiftMultiText caption = LocateAndReadMultiText(n, "label");
			if(caption.IsEmpty)
			{
				caption = null;
			}
			_merger.MergeInPicture(parent, href, caption);
		}

		private void ReadMedia(XmlNode n, TBase parent)
		{
			string href = Utilities.GetStringAttribute(n, "href");
			LiftMultiText caption = LocateAndReadMultiText(n, "label");
			if (caption.IsEmpty)
			{
				caption = null;
			}
			_merger.MergeInMedia(parent, href, caption);
		}

		/// <summary>
		/// Read the grammatical-info information for either a sense or a reversal.
		/// </summary>
		private void ReadGrammi(TBase senseOrReversal, XmlNode senseNode)
		{
			XmlNode grammiNode = senseNode.SelectSingleNode("grammatical-info");
			if (grammiNode != null)
			{
				string val = Utilities.GetStringAttribute(grammiNode, "value");
				_merger.MergeInGrammaticalInfo(senseOrReversal, val, GetTraitList(grammiNode));
			}
		}

		/// <summary>
		/// Used for elements with traits that are not top level objects (extensibles) or forms.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private List<Trait> GetTraitList(XmlNode node)
		{
			List<Trait> traits = new List<Trait>();
			var nodes = node.SelectNodes("trait");
			if (nodes != null)
			{
				foreach (XmlNode traitNode in nodes)
					traits.Add(GetTrait(traitNode));
			}
			return traits;
		}

		private void ReadSense(XmlNode node, TEntry entry)
		{
			TSense sense = _merger.GetOrMakeSense(entry, ReadExtensibleElementBasics(node), node.OuterXml);
			FinishReadingSense(node, sense);
		}

		private void FinishReadingSense(XmlNode node, TSense sense)
		{
			if (sense != null)//not been pruned
			{
				ReadGrammi(sense, node);
				LiftMultiText gloss = LocateAndReadOneElementPerFormData(node, "gloss");
				if (!gloss.IsEmpty)
				{
					_merger.MergeInGloss(sense, gloss);
				}
				LiftMultiText def = LocateAndReadMultiText(node, "definition");
				if (!def.IsEmpty)
				{
					_merger.MergeInDefinition(sense, def);
				}

				ReadNotes(node, sense);
				var nodes = node.SelectNodes("example");
				if (nodes != null)
				{
					foreach (XmlNode n in nodes)
						ReadExample(n, sense);
				}
				nodes = node.SelectNodes("relation");
				if (nodes != null)
				{
					foreach (XmlNode n in nodes)
						ReadRelation(n, sense);
				}
				nodes = node.SelectNodes("illustration");
				if (nodes != null)
				{
					foreach (XmlNode n in nodes)
						ReadPicture(n, sense);
				}
				nodes = node.SelectNodes("reversal");
				if (nodes != null)
				{
					foreach (XmlNode n in nodes)
						ReadReversal(n, sense);
				}
				nodes = node.SelectNodes("subsense");
				if (nodes != null)
				{
					foreach (XmlNode n in nodes)
						ReadSubsense(n, sense);
				}
				ReadExtensibleElementDetails(sense, node);
			}
			// return sense;
		}

		private void ReadSubsense(XmlNode node, TSense sense)
		{
			TSense subsense = _merger.GetOrMakeSubsense(sense, ReadExtensibleElementBasics(node), node.OuterXml);
			if (subsense != null)//wesay can't handle these in April 2008
			{
				FinishReadingSense(node, subsense);
			}
		}

		private void ReadExample(XmlNode node, TSense sense)
		{
			TExample example = _merger.GetOrMakeExample(sense, ReadExtensibleElementBasics(node));
			if (example != null)//not been pruned
			{
				LiftMultiText exampleSentence = LocateAndReadMultiText(node, null);
				if (!exampleSentence.IsEmpty)
				{
					_merger.MergeInExampleForm(example, exampleSentence);
				}
				var nodes = node.SelectNodes("translation");
				if (nodes != null)
				{
					foreach (XmlNode translationNode in nodes)
					{
						LiftMultiText translation = ReadMultiText(translationNode);
						string type = Utilities.GetOptionalAttributeString(translationNode, "type");
						_merger.MergeInTranslationForm(example, type, translation, translationNode.OuterXml);
					}
				}
				string source = Utilities.GetOptionalAttributeString(node, "source");
				if (source != null)
				{
					_merger.MergeInSource(example, source);
				}

				// REVIEW(SRMc): If you don't think the note element should be valid
				// inside an example, then remove the next line and the corresponding
				// chunk from the rng file.
				// JH says: LIFT ver 0.13 is going to make notes available to all extensibles
				// todo: remove this when that is true
				ReadNotes(node, example);

				ReadExtensibleElementDetails(example, node);
			}
			return;
		}

		private void ReadReversal(XmlNode node, TSense sense)
		{
			string type = Utilities.GetOptionalAttributeString(node, "type");
			XmlNodeList nodelist = node.SelectNodes("main");
			if (nodelist != null && nodelist.Count > 1)
			{
				NotifyFormatError(new LiftFormatException(String.Format("Only one <main> element is allowed inside a <reversal> element:\r\n{0}", node.OuterXml)));
			}
			TBase parent = null;
			if (nodelist != null && nodelist.Count == 1)
				parent = ReadParentReversal(type, nodelist[0]);
			LiftMultiText text = ReadMultiText(node);
			TBase reversal = _merger.MergeInReversal(sense, parent, text, type, node.OuterXml);
			if (reversal != null)
				ReadGrammi(reversal, node);
		}

		private TBase ReadParentReversal(string type, XmlNode node)
		{
			XmlNodeList nodelist = node.SelectNodes("main");
			if (nodelist != null && nodelist.Count > 1)
			{
				NotifyFormatError(new LiftFormatException(String.Format("Only one <main> element is allowed inside a <main> element:\r\n{0}", node.OuterXml)));
			}
			TBase parent = null;
			if (nodelist != null && nodelist.Count == 1)
				parent = ReadParentReversal(type, nodelist[0]);
			LiftMultiText text = ReadMultiText(node);
			TBase reversal = _merger.GetOrMakeParentReversal(parent, text, type);
			if (reversal != null)
				ReadGrammi(reversal, node);
			return reversal;
		}

		/// <summary>
		/// read enough for finding a potential match to merge with
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private Extensible ReadExtensibleElementBasics(XmlNode node)
		{
			Extensible extensible = new Extensible();
			extensible.Id = Utilities.GetOptionalAttributeString(node, "id");//actually not part of extensible (as of 8/1/2007)

			//todo: figure out how to actually look it up:
			//      string flexPrefix = node.OwnerDocument.GetPrefixOfNamespace("http://fieldworks.sil.org");
//            string flexPrefix = "flex";
//            if (flexPrefix != null && flexPrefix != string.Empty)
			{
				string guidString = Utilities.GetOptionalAttributeString(node, /*flexPrefix + ":guid"*/"guid");
				if (guidString != null)
				{
					try
					{
						extensible.Guid = new Guid(guidString);
					}
					catch (Exception)
					{
						NotifyFormatError(new LiftFormatException(String.Format("{0} is not a valid GUID", guidString)));
					}
				}
			}
			extensible.CreationTime = GetOptionalDate(node, "dateCreated", DefaultCreationModificationUTC);
			extensible.ModificationTime = GetOptionalDate(node, "dateModified", DefaultCreationModificationUTC);

			return extensible;
		}

		/// <summary>
		/// Once we have the thing we're creating/merging with, we can read in any details,
		/// i.e. traits, fields, and annotations
		/// </summary>
		/// <param name="target"></param>
		/// <param name="node"></param>
		/// <returns></returns>
		private void ReadExtensibleElementDetails(TBase target, XmlNode node)
		{
			var nodes = node.SelectNodes("field");
			if (nodes != null)
			{
				foreach (XmlNode fieldNode in nodes)
				{
					string fieldType = Utilities.GetStringAttribute(fieldNode, "type");
					//string priorFieldWithSameTag = String.Format("preceding-sibling::field[@type='{0}']", fieldType);
					//    JH removed this Oct 2010... I can't figure out why this limitation, which is not supported logicall
					//                or by the schema, was in here.  It was preventing, for example, multiple variants.
					//                if(fieldNode.SelectSingleNode(priorFieldWithSameTag) != null)
					//                {
					// a fatal error
					//                    throw new LiftFormatException(String.Format("Field with same type ({0}) as sibling not allowed. Context:{1}", fieldType, fieldNode.ParentNode.OuterXml));
					//                }
					_merger.MergeInField(target,
										 fieldType,
										 GetOptionalDate(fieldNode, "dateCreated", default(DateTime)),
										 GetOptionalDate(fieldNode, "dateModified", default(DateTime)),
										 ReadMultiText(fieldNode),
										 GetTraitList(fieldNode));
				}
			}
			ReadTraits(node, target);
			//todo: read annotations
		}

		private void ReadTraits(XmlNode node, TBase target)
		{
			var nodes = node.SelectNodes("trait");
			if (nodes != null)
			{
				foreach (XmlNode traitNode in nodes)
					_merger.MergeInTrait(target,GetTrait(traitNode));
			}
		}


		/// <summary>
		/// careful, can't return null, so give MinValue
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <param name="attributeName"></param>
		/// <param name="defaultDateTime">the time to use if this attribute isn't found</param>
		/// <returns></returns>
		private DateTime GetOptionalDate(XmlNode xmlNode, string attributeName, DateTime defaultDateTime)
		{
			if (xmlNode.Attributes != null)
			{
				XmlAttribute attr = xmlNode.Attributes[attributeName];
				if (attr == null)
					return defaultDateTime;

				/* if the incoming data lacks a time, we'll have a kind of 'unspecified', else utc */

				try
				{
					return Extensible.ParseDateTimeCorrectly(attr.Value);
				}
				catch (Exception e)
				{
					NotifyFormatError(e); // not a fatal error
				}
			}
			return defaultDateTime;
		}

		private LiftMultiText LocateAndReadMultiText(XmlNode node, string query)
		{
			XmlNode element;
			if (query == null)
			{
				element = node;
			}
			else
			{
				element = node.SelectSingleNode(query);
				XmlNodeList nodes = node.SelectNodes(query);
				if (nodes != null && nodes.Count > 1)
				{
					throw new LiftFormatException(
						String.Format("Duplicated element of type {0} unexpected. Context:{1}",
						query,
						nodes[0].ParentNode == null ? "??" : nodes[0].ParentNode.OuterXml));
				}
			}

			if (element != null)
			{
				return ReadMultiText(element);
			}
			return new LiftMultiText();
		}
//
//        private List<LiftMultiText> LocateAndReadOneOrMoreMultiText(XmlNode node, string query)
//        {
//            List<LiftMultiText> results = new List<LiftMultiText>();
//            foreach (XmlNode n in node.SelectNodes(query))
//            {
//                results.Add(ReadMultiText(n));
//            }
//            return results;
//        }

		private LiftMultiText LocateAndReadOneElementPerFormData(XmlNode node, string query)
		{
			Debug.Assert(query != null);
			LiftMultiText text = new LiftMultiText();
			ReadFormNodes(node.SelectNodes(query), text);
			return text;
		}

		internal  LiftMultiText ReadMultiText(XmlNode node)
		{
			LiftMultiText text = new LiftMultiText();
			ReadFormNodes(node.SelectNodes("form"), text);
			return text;
		}

		private void ReadFormNodes(XmlNodeList nodesWithForms, LiftMultiText text)
		{
			foreach (XmlNode formNode in nodesWithForms)
			{
				try
				{
					string lang = Utilities.GetStringAttribute(formNode, _wsAttributeLabel);
					XmlNode textNode= formNode.SelectSingleNode("text");
					if (textNode != null)
					{
						// Add the separator if we need it.
						if (textNode.InnerText.Length > 0)
							text.AddOrAppend(lang, "", "; ");
						foreach (XmlNode node in textNode.ChildNodes)
						{
							if (node.Name == "span")
							{
								var span = text.AddSpan(lang,
											 Utilities.GetOptionalAttributeString(node, "lang"),
											 Utilities.GetOptionalAttributeString(node, "class"),
											 Utilities.GetOptionalAttributeString(node, "href"),
											 node.InnerText.Length);
								ReadSpanContent(text, lang, span, node);
							}
							else
							{
								text.AddOrAppend(lang, node.InnerText, "");
							}
						}
					}
					var nodelist = formNode.SelectNodes("annotation");
					if (nodelist != null)
					{
						foreach (XmlNode annotationNode in nodelist)
						{
							Annotation annotation = GetAnnotation(annotationNode);
							annotation.LanguageHint = lang;
							text.Annotations.Add(annotation);
						}
					}
				}
				catch (Exception e)
				{
					// not a fatal error
					NotifyFormatError(e);
				}
			}
		}

		private void ReadSpanContent(LiftMultiText text, string lang, LiftSpan span, XmlNode node)
		{
			Debug.Assert(node.Name == "span");
			foreach (XmlNode xn in node.ChildNodes)
			{
				if (xn.Name == "span")
				{
					var spanLang = Utilities.GetOptionalAttributeString(xn, "lang");
					if (spanLang == lang)
						spanLang = null;
					var spanInner = new LiftSpan(text.LengthOfAlternative(lang),
						xn.InnerText.Length,
						spanLang,
						Utilities.GetOptionalAttributeString(xn, "class"),
						Utilities.GetOptionalAttributeString(xn, "href"));
					span.Spans.Add(spanInner);
					ReadSpanContent(text, lang, spanInner, xn);
				}
				else
				{
					text.AddOrAppend(lang, xn.InnerText, "");
				}

			}
		}

		private Trait GetTrait(XmlNode traitNode)
		{
			Trait t = new Trait(Utilities.GetStringAttribute(traitNode, "name"),
								Utilities.GetStringAttribute(traitNode, "value"));
			var nodelist = traitNode.SelectNodes("annotation");
			if (nodelist != null)
			{
				foreach (XmlNode annotationNode in nodelist)
				{
					Annotation annotation = GetAnnotation(annotationNode);
					t.Annotations.Add(annotation);
				}
			}
			return t;
		}

		private  Annotation GetAnnotation(XmlNode annotationNode)
		{
			return new Annotation(Utilities.GetOptionalAttributeString(annotationNode, "name"),
								  Utilities.GetOptionalAttributeString(annotationNode, "value"),
								  GetOptionalDate(annotationNode, "when", default(DateTime)),
								  Utilities.GetOptionalAttributeString(annotationNode, "who"));
		}

		//private static bool NodeContentIsJustAString(XmlNode node)
		//{
		//    return node.InnerText != null
		//                        && (node.ChildNodes.Count == 1)
		//                        && (node.ChildNodes[0].NodeType == XmlNodeType.Text)
		//                        && node.InnerText.Trim() != string.Empty;
		//}

//        public LexExampleSentence ReadExample(XmlNode xmlNode)
//        {
//            LexExampleSentence example = new LexExampleSentence();
//            LocateAndReadMultiText(xmlNode, "source", example.Sentence);
//            //NB: will only read in one translation
//            LocateAndReadMultiText(xmlNode, "trans", example.Translation);
//            return example;
//        }
//

		/// <summary>
		/// Read a LIFT file. Must be the current lift version.
		/// </summary>
		public int ReadLiftFile(string pathToLift)
		{
			_pathToLift = pathToLift;	// may need this to find its ranges file.
			if (_defaultCreationModificationUTC == default(DateTime))
			{
				_defaultCreationModificationUTC = File.GetLastWriteTimeUtc(pathToLift);
			}

			ProgressTotalSteps = GetEstimatedNumberOfEntriesInFile(pathToLift);
			ProgressStepsCompleted = 0;

			if (Validator.GetLiftVersion(pathToLift) != Validator.LiftVersion)
			{
				throw new LiftFormatException("Programmer should migrate the lift file before calling this method.");
			}

			int numberOfEntriesRead;
			if (_changeDetector != null && _changeDetector.CanProvideChangeRecord)
			{
				ProgressMessage = "Detecting Changes To Lift File...";
				_changeReport = _changeDetector.GetChangeReport(new NullProgress());
			}

			using (XmlReader reader = XmlReader.Create(pathToLift, NormalReaderSettings))
			{
				reader.ReadStartElement("lift");
				ReadHeader(reader);
				numberOfEntriesRead = ReadEntries(reader);
			}
			if (_changeReport != null && _changeReport.IdsOfDeletedEntries.Count > 0)
			{
				ProgressMessage = "Removing entries that were removed from the Lift file...";
				foreach (string id in _changeReport.IdsOfDeletedEntries)
				{
					Extensible eInfo = new Extensible();
					eInfo.Id = id;
					_merger.EntryWasDeleted(eInfo, default(DateTime) /* we don't know... why is this part of the interface, anyhow? */);
				}
			}
			return numberOfEntriesRead;
		}

		/// <summary>
		/// Intended to be fast, and only (probably) acurate
		/// </summary>
		/// <param name="pathToLift"></param>
		/// <returns></returns>
		internal static int GetEstimatedNumberOfEntriesInFile(string pathToLift)
		{
			int count = 0;
			using (FileStream stream = File.OpenRead(pathToLift))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					while (reader.Peek() >= 0)
					{
						string line = reader.ReadLine();
						if (line != null)
							count += System.Text.RegularExpressions.Regex.Matches(line, "<entry").Count;
					}
				}
			}
			return count;
		}

		private static XmlReaderSettings NormalReaderSettings
		{
			get
			{
				XmlReaderSettings readerSettings = new XmlReaderSettings();
				readerSettings.ValidationType = ValidationType.None;
				readerSettings.IgnoreComments = true;
				return readerSettings;
			}
		}


		private int ReadEntries(XmlReader reader)
		{
// Process all of the entry elements, reading them into memory one at a time.
			ProgressMessage = "Reading entries from LIFT file";
			if (!reader.IsStartElement("entry"))
				reader.ReadToFollowing("entry");	// not needed if no <header> element.

			const int kProgressReportingInterval = 50;
			int numberOfEntriesRead = 0;
			int nextProgressPoint = numberOfEntriesRead + kProgressReportingInterval;

			while (reader.IsStartElement("entry"))
			{
				string entryXml = reader.ReadOuterXml();
				if (!String.IsNullOrEmpty(entryXml))
				{
					ReadEntry(GetNodeFromString(entryXml));
				}
				numberOfEntriesRead++;
				if (numberOfEntriesRead >= nextProgressPoint)
				{
					ProgressStepsCompleted = numberOfEntriesRead;
					nextProgressPoint = numberOfEntriesRead + kProgressReportingInterval;
				}
				if (_cancelNow)
				{
					break;
				}
			}
			return numberOfEntriesRead;
		}

		/// <summary>
		/// used to adapt between the DOM/XmlNode-based stuff John wrote initially, and the Reader-based stuff Steve added
		/// </summary>
		private static XmlNode GetNodeFromString(string xml)
		{
			XmlDocument document = new XmlDocument();
			document.PreserveWhitespace = true;	// needed to preserve newlines in "multiparagraph" forms.
			document.LoadXml(xml);
			return document.FirstChild;
		}

		private void ReadHeader(XmlReader reader)
		{
			if (reader.IsStartElement("header"))
			{
				ProgressMessage = "Reading LIFT file header";
				bool headerIsEmpty = reader.IsEmptyElement;
				reader.ReadStartElement("header");
				if (!headerIsEmpty)
				{
					ReadRanges(reader); // can exist here or after fields
					if (reader.IsStartElement("fields"))
					{
						bool fieldsIsEmpty = reader.IsEmptyElement;
						reader.ReadStartElement("fields");
						if (!fieldsIsEmpty)
						{
							while (reader.IsStartElement("field"))
							{
								string fieldXml = reader.ReadOuterXml();
								if (!String.IsNullOrEmpty(fieldXml))
								{
									ReadFieldDefinition(GetNodeFromString(fieldXml));
								}
							}
							Debug.Assert(reader.LocalName == "fields");
							reader.ReadEndElement(); // </fields>
						}
					}
					ReadRanges(reader); // can exist here or before fields

					// Not sure why this is needed, but the assert is sometimes thrown otherwise if
					// whitespace separates the end elements here.
					reader.MoveToContent();
					Debug.Assert(reader.LocalName == "header");
					reader.ReadEndElement(); // </header>
				}
			}
		}

		private void ReadRanges(XmlReader reader)
		{
			if (reader.IsStartElement("ranges"))
			{
				bool rangesIsEmpty = reader.IsEmptyElement;
				reader.ReadStartElement("ranges");
				if (!rangesIsEmpty)
				{
					while (reader.IsStartElement("range"))
					{
						bool rangeIsEmpty = reader.IsEmptyElement;

						string id = reader.GetAttribute("id");
						string href = reader.GetAttribute("href");
						string guid = reader.GetAttribute("guid");
						ProgressMessage = string.Format("Reading LIFT range {0}", id);
						reader.ReadStartElement();
						if (string.IsNullOrEmpty(href))
						{
							while (reader.IsStartElement("range-element"))
							{
								string rangeXml = reader.ReadOuterXml();
								if (!String.IsNullOrEmpty(rangeXml))
								{
									ReadRangeElement(id, GetNodeFromString(rangeXml));
								}
							}
						}
						else
						{
							ReadExternalRange(href, id, guid);
						}
						if (!rangeIsEmpty)
						{
							reader.MoveToContent();
							reader.ReadEndElement(); // </range>
						}
					}
					Debug.Assert(reader.LocalName == "ranges");
					reader.ReadEndElement();	// </ranges>
				}
			}
		}

//        /// <summary>
//        /// Return the number of entries processed from the most recent file.
//        /// </summary>
//		this was a confusing way to return the results of a parse operation.
//        the parser should really return this value, if flex
//      needs it
//            public int EntryCount
//		{
//			get { return _count; }
//		}

		/// <summary>
		/// Read a range from a separate file.
		/// </summary>
		/// <param name="pathToRangeFile"></param>
		/// <param name="rangeId"></param>
		/// <param name="rangeGuid"></param>
		private void ReadExternalRange(string pathToRangeFile, string rangeId, string rangeGuid)
		{
			if (pathToRangeFile.StartsWith("file://"))
				pathToRangeFile = pathToRangeFile.Substring(7);
			if (!File.Exists(pathToRangeFile))
			{
				// try to find range file next to the lift file (may have been copied to another
				// directory or another machine)
				string dir = Path.GetDirectoryName(_pathToLift);
				string file = Path.GetFileName(pathToRangeFile);
				if (dir != null && file != null)
					pathToRangeFile = Path.Combine(dir, file);
				if (!File.Exists(pathToRangeFile))
					return;		// ignore missing range file without error.
			}
			using (XmlReader reader = XmlReader.Create(pathToRangeFile, NormalReaderSettings))
			{
				reader.ReadStartElement("lift-ranges");
				while (reader.IsStartElement("range"))
				{
					string id = reader.GetAttribute("id");
					// unused	string guid = reader.GetAttribute("guid");
					bool foundDesiredRange = id == rangeId;
					bool emptyRange = reader.IsEmptyElement;
					reader.ReadStartElement();
					if (!emptyRange)
					{
						while (reader.IsStartElement("range-element"))
						{
							string rangeElementXml = reader.ReadOuterXml();
							if (foundDesiredRange && !String.IsNullOrEmpty(rangeElementXml))
							{
								ReadRangeElement(id, GetNodeFromString(rangeElementXml));
							}
						}
						Debug.Assert(reader.LocalName == "range");
						reader.ReadEndElement(); // </range>
					}
					if (foundDesiredRange)
						return;		// we've seen the range we wanted from this file.
				}
			}
		}


		#region Progress

		///<summary>
		/// This class passes on progress report related information to an event handler.
		///</summary>
		public class StepsArgs : EventArgs
		{
			private int _steps;

			///<summary>
			/// Get/set the current state of the progress bar in number of steps.
			///</summary>
			public int Steps
			{
				get { return _steps; }
				set { _steps = value; }
			}
		}

		///<summary>
		/// This class passes on an exception that has been caught to an event handler.
		///</summary>
		public class ErrorArgs : EventArgs
		{
			private Exception _exception;

			///<summary>
			/// Get/set the Exception object being passed.
			///</summary>
			public Exception Exception
			{
				get { return _exception; }
				set { _exception = value; }
			}
		}

		///<summary>
		/// This class passes on a a status message string to an event handler.
		///</summary>
		public class MessageArgs : EventArgs
		{
			private string _msg;

			///<summary>
			/// Get/set the status message string.
			///</summary>
			public string Message
			{
				get { return _msg; }
				set { _msg = value; }
			}
		}

		private int ProgressStepsCompleted
		{
			set
			{
				if (SetStepsCompleted != null)
				{
					ProgressEventArgs e = new ProgressEventArgs(value);
					SetStepsCompleted.Invoke(this, e);
					_cancelNow = e.Cancel;
				}
			}
		}

		private int ProgressTotalSteps
		{
			set
			{
				if (SetTotalNumberSteps != null)
				{
					StepsArgs e = new StepsArgs();
					e.Steps = value;
					SetTotalNumberSteps.Invoke(this, e);
				}
			}
		}

		private string ProgressMessage
		{
			set
			{
				if (SetProgressMessage != null)
				{
					MessageArgs e = new MessageArgs();
					e.Message = value;
					SetProgressMessage.Invoke(this, e);
				}
			}
		}

		///<summary>
		/// Get/set the default DateTime value use for creation or modification times.
		///</summary>
		public DateTime DefaultCreationModificationUTC
		{
			get { return _defaultCreationModificationUTC; }
			set { _defaultCreationModificationUTC = value; }
		}

		/// <summary>
		/// Optional object that will tell us which entries actually need parsing/adding.
		/// NB: it is up to the client of this class to do any deleting that the detector says is needed
		/// </summary>
		public ILiftChangeDetector ChangeDetector
		{
			get { return _changeDetector; }
			set { _changeDetector = value; }
		}

		///<summary></summary>
		public ILiftChangeReport ChangeReport
		{
			get { return _changeReport; }
			set { _changeReport = value; }
		}

		/// <summary>
		/// NB: this will always conver the exception to a LiftFormatException, if it isn't already
		/// </summary>
		/// <param name="error"></param>
		private void NotifyFormatError(Exception error)
		{
			if (ParsingWarning != null)
			{

				//it's important to pass this on as a format error, which the client should be expecting
				//to report without crashing.
				if (!(error is LiftFormatException))
				{
					error = new LiftFormatException(error.Message, error);
				}
				ErrorArgs e = new ErrorArgs();
				e.Exception = error;
				ParsingWarning.Invoke(this, e);
			}
		}

		///<summary>
		/// This class passes on progress/cancel information to an event handler.
		///</summary>
		public class ProgressEventArgs : EventArgs
		{
			private readonly int _progress;
			private bool _cancel;

			///<summary>
			/// Constructor.
			///</summary>
			public ProgressEventArgs(int progress)
			{
				_progress = progress;
			}

			///<summary>
			/// Get the progress value for this event.
			///</summary>
			public int Progress
			{
				get { return _progress; }
			}

			///<summary>
			/// Get/set the cancel flag for this event.
			///</summary>
			public bool Cancel
			{
				get { return _cancel; }
				set { _cancel = value; }
			}
		}

		#endregion

	}
}
