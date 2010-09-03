using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.DictionaryServices;
using Palaso.DictionaryServices.Model;
using Palaso.DictionaryServices.Queries;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Tests.Queries
{
	[TestFixture]
	public class CustomFieldQueryTests
	{

		//OptionRefCollectionType
		[Test]
		public void GetResults_EntryWithPopulatedRefCollectionInSenseExists_ReturnsValues()
		{
			LexEntry entryToQuery = new LexEntry();
			string _fieldLabel = "OptionRefCollection";
			AddOptionRefCollectionPropertyToEntryAtSenseLevel(entryToQuery, _fieldLabel);
			CustomFieldQuery query = new CustomFieldQuery(_fieldLabel);
			List<IDictionary<string, object>> results = new List<IDictionary<string, object>>(query.GetResults(entryToQuery));
			Assert.AreEqual("option1", results[0][_fieldLabel]);
			Assert.AreEqual("option2", results[1][_fieldLabel]);
		}

		[Test]
		public void GetResults_EntryWithPopulatedRefCollectionInLexEntryExists_ReturnsValues()
		{
			LexEntry entryToQuery = new LexEntry();
			string _fieldLabel = "OptionRefCollection";
			AddOptionRefCollectionPropertyToEntryAtEntryLevel(entryToQuery, _fieldLabel);
			CustomFieldQuery query = new CustomFieldQuery(_fieldLabel);
			List<IDictionary<string, object>> results = new List<IDictionary<string, object>>(query.GetResults(entryToQuery));
			Assert.AreEqual("option1", results[0][_fieldLabel]);
			Assert.AreEqual("option2", results[1][_fieldLabel]);
		}

		[Test]
		public void GetResults_EntryWithPopulatedRefCollectionInExampleSentenceExists_ReturnsValues()
		{
			LexEntry entryToQuery = new LexEntry();
			LexSense sense = new LexSense();
			entryToQuery.Senses.Add(sense);
			string _fieldLabel = "OptionRefCollection";
			AddOptionRefCollectionPropertyToSenseAtExampleSentenceLevel(sense, _fieldLabel);
			CustomFieldQuery query = new CustomFieldQuery(_fieldLabel);
			List<IDictionary<string, object>> results = new List<IDictionary<string, object>>(query.GetResults(entryToQuery));
			Assert.AreEqual("option1", results[0][_fieldLabel]);
			Assert.AreEqual("option2", results[1][_fieldLabel]);
		}

		[Test]
		public void GetResults_EntryWithPopulatedRefCollectionsInAllLevelsExist_ReturnsAllValues()
		{
			LexEntry entryToQuery = new LexEntry();
			string _fieldLabel = "OptionRefCollection";
			AddOptionRefCollectionPropertyToEntryAtEntryLevel(entryToQuery, _fieldLabel);
			LexSense sense = AddOptionRefCollectionPropertyToEntryAtSenseLevel(entryToQuery, _fieldLabel);
			AddOptionRefCollectionPropertyToSenseAtExampleSentenceLevel(sense, _fieldLabel);
			CustomFieldQuery query = new CustomFieldQuery(_fieldLabel);
			List<IDictionary<string, object>> results = new List<IDictionary<string, object>>(query.GetResults(entryToQuery));
			Assert.AreEqual(6, results.Count);
			Assert.AreEqual("option1", results[0][_fieldLabel]);
			Assert.AreEqual("option2", results[1][_fieldLabel]);
			Assert.AreEqual("option1", results[2][_fieldLabel]);
			Assert.AreEqual("option2", results[3][_fieldLabel]);
			Assert.AreEqual("option1", results[4][_fieldLabel]);
			Assert.AreEqual("option2", results[5][_fieldLabel]);
		}

		//MultiTextType
		[Test]
		public void GetResults_EntryWithPopulatedMultiTextInSenseExists_ReturnsValues()
		{
			LexEntry entryToQuery = new LexEntry();
			string _fieldLabel = "MultiText";
			AddMultiTextPropertyToEntryAtSenseLevel(entryToQuery, _fieldLabel);
			CustomFieldQuery query = new CustomFieldQuery(_fieldLabel);
			List<IDictionary<string, object>> results = new List<IDictionary<string, object>>(query.GetResults(entryToQuery));
			Assert.AreEqual("german 1", results[0][_fieldLabel]);
			Assert.AreEqual("de", results[0]["WritingSystem"]);
			Assert.AreEqual("english 1", results[1][_fieldLabel]);
			Assert.AreEqual("en", results[1]["WritingSystem"]);
		}

		[Test]
		public void GetResults_EntryWithPopulatedMultiTextInLexEntryExists_ReturnsValues()
		{
			LexEntry entryToQuery = new LexEntry();
			string _fieldLabel = "MultiText";
			AddMultiTextPropertyToEntryAtEntryLevel(entryToQuery, _fieldLabel);
			CustomFieldQuery query = new CustomFieldQuery(_fieldLabel);
			List<IDictionary<string, object>> results = new List<IDictionary<string, object>>(query.GetResults(entryToQuery));
			Assert.AreEqual("german 1", results[0][_fieldLabel]);
			Assert.AreEqual("de", results[0]["WritingSystem"]);
			Assert.AreEqual("english 1", results[1][_fieldLabel]);
			Assert.AreEqual("en", results[1]["WritingSystem"]);
		}

		[Test]
		public void GetResults_EntryWithPopulatedMultiTextInExampleSentenceExists_ReturnsValues()
		{
			LexEntry entryToQuery = new LexEntry();
			LexSense sense = new LexSense();
			entryToQuery.Senses.Add(sense);
			string _fieldLabel = "MultiText";
			AddMultiTextPropertyToSenseAtExampleSentenceLevel(sense, _fieldLabel);
			CustomFieldQuery query = new CustomFieldQuery(_fieldLabel);
			List<IDictionary<string, object>> results = new List<IDictionary<string, object>>(query.GetResults(entryToQuery));
			Assert.AreEqual("german 1", results[0][_fieldLabel]);
			Assert.AreEqual("de", results[0]["WritingSystem"]);
			Assert.AreEqual("english 1", results[1][_fieldLabel]);
			Assert.AreEqual("en", results[1]["WritingSystem"]);
		}

		[Test]
		public void GetResults_EntryWithPopulatedMultiTextInAllLevelsExist_ReturnsAllValues()
		{
			LexEntry entryToQuery = new LexEntry();
			string _fieldLabel = "MultiText";
			AddMultiTextPropertyToEntryAtEntryLevel(entryToQuery, _fieldLabel);
			LexSense sense = AddMultiTextPropertyToEntryAtSenseLevel(entryToQuery, _fieldLabel);
			AddMultiTextPropertyToSenseAtExampleSentenceLevel(sense, _fieldLabel);
			CustomFieldQuery query = new CustomFieldQuery(_fieldLabel);
			List<IDictionary<string, object>> results = new List<IDictionary<string, object>>(query.GetResults(entryToQuery));
			Assert.AreEqual(6, results.Count);
			Assert.AreEqual("german 1", results[0][_fieldLabel]);
			Assert.AreEqual("de", results[0]["WritingSystem"]);
			Assert.AreEqual("english 1", results[1][_fieldLabel]);
			Assert.AreEqual("en", results[1]["WritingSystem"]);
			Assert.AreEqual("german 1", results[2][_fieldLabel]);
			Assert.AreEqual("de", results[2]["WritingSystem"]);
			Assert.AreEqual("english 1", results[3][_fieldLabel]);
			Assert.AreEqual("en", results[3]["WritingSystem"]);
			Assert.AreEqual("german 1", results[4][_fieldLabel]);
			Assert.AreEqual("de", results[4]["WritingSystem"]);
			Assert.AreEqual("english 1", results[5][_fieldLabel]);
			Assert.AreEqual("en", results[5]["WritingSystem"]);
		}

		//OptionRefType
		private LexExampleSentence AddOptionRefCollectionPropertyToSenseAtExampleSentenceLevel(LexSense sense, string _fieldLabel)
		{
			LexExampleSentence exampleSentence = new LexExampleSentence();
			OptionRefCollection optionRefCollection = exampleSentence.GetOrCreateProperty<OptionRefCollection>(_fieldLabel);
			optionRefCollection.Add("option1");
			optionRefCollection.Add("option2");
			sense.ExampleSentences.Add(exampleSentence);
			return exampleSentence;
		}

		private LexSense AddOptionRefCollectionPropertyToEntryAtSenseLevel(LexEntry entry, string _fieldLabel)
		{
			LexSense sense = new LexSense();
			OptionRefCollection optionRefCollection = sense.GetOrCreateProperty<OptionRefCollection>(_fieldLabel);
			optionRefCollection.Add("option1");
			optionRefCollection.Add("option2");
			entry.Senses.Add(sense);
			return sense;
		}

		private LexSense AddOptionRefCollectionPropertyToEntryAtSenseLevel(LexSense sense, string _fieldLabel)
		{
			OptionRefCollection optionRefCollection = sense.GetOrCreateProperty<OptionRefCollection>(_fieldLabel);
			optionRefCollection.Add("option1");
			optionRefCollection.Add("option2");
			return sense;
		}

		private void AddOptionRefCollectionPropertyToEntryAtEntryLevel(LexEntry entry, string _fieldLabel)
		{
			OptionRefCollection optionRefCollection = entry.GetOrCreateProperty<OptionRefCollection>(_fieldLabel);
			optionRefCollection.Add("option1");
			optionRefCollection.Add("option2");
		}

		//MultiText Type
		private LexExampleSentence AddMultiTextPropertyToSenseAtExampleSentenceLevel(LexSense sense, string _fieldLabel)
		{
			LexExampleSentence exampleSentence = new LexExampleSentence();
			MultiText multiText = exampleSentence.GetOrCreateProperty<MultiText>(_fieldLabel);
			multiText["de"] = "german 1";
			multiText["en"] = "english 1";
			sense.ExampleSentences.Add(exampleSentence);
			return exampleSentence;
		}

		private LexSense AddMultiTextPropertyToEntryAtSenseLevel(LexEntry entry, string _fieldLabel)
		{
			LexSense sense = new LexSense();
			MultiText multiText = sense.GetOrCreateProperty<MultiText>(_fieldLabel);
			multiText["de"] = "german 1";
			multiText["en"] = "english 1";
			entry.Senses.Add(sense);
			return sense;
		}

		private LexSense AddMultiTextPropertyToEntryAtSenseLevel(LexSense sense, string _fieldLabel)
		{
			MultiText multiText = sense.GetOrCreateProperty<MultiText>(_fieldLabel);
			multiText["de"] = "german 1";
			multiText["en"] = "english 1";
			return sense;
		}

		private void AddMultiTextPropertyToEntryAtEntryLevel(LexEntry entry, string _fieldLabel)
		{
			MultiText multiText = entry.GetOrCreateProperty<MultiText>(_fieldLabel);
			multiText["de"] = "german 1";
			multiText["en"] = "english 1";
		}
	}
}
