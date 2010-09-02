using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.DictionaryServices;
using Palaso.DictionaryServices.Model;
using Palaso.DictionaryServices.Queries;
using Palaso.Lift.Options;

namespace Palaso.DictionaryServices.Tests.Queries
{
	[TestFixture]
	public class CustomFieldQueryTests
	{
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

		private LexExampleSentence AddOptionRefCollectionPropertyToSenseAtExampleSentenceLevel(LexSense sense, string _fieldLabel)
		{
			LexExampleSentence exampleSentence = new LexExampleSentence();
			OptionRefCollection optionRefCollection = exampleSentence.GetOrCreateProperty<OptionRefCollection>(_fieldLabel);
			optionRefCollection.Add("option1");
			optionRefCollection.Add("option2");
			sense.ExampleSentences.Add(exampleSentence);
			return exampleSentence;
		}

		private LexExampleSentence AddOptionRefCollectionPropertyToSenseAtExampleSentenceLevel(LexEntry entry, string _fieldLabel)
		{
			LexSense sense = new LexSense();
			entry.Senses.Add(sense);
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
	}
}
