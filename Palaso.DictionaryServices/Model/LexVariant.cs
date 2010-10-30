using System;
using System.Collections.Generic;
using System.ComponentModel;
using LiftIO.Parsing;
using Palaso.Lift;
using Palaso.Reporting;

namespace Palaso.DictionaryServices.Model
{
	public interface IExtensible //: PalasoDataObject
	{
		List<LexTrait> Traits { get; }
		List<LexField> Fields { get; }
	}

	public class LexTrait
	{
		public string Name;
		public string Value;

		public LexTrait(string name, string value)
		{
			Name = name;
			Value = value;
		}
	}
	public class LexField: MultiText
	{
		public string Type;
		public List<LexTrait> Traits = new List<LexTrait>();

		public LexField(string type)
		{
			Type = type;
		}
	}
	public sealed class LexVariant:  MultiText, IExtensible
	{
		public LexVariant()
		{
			Traits = new List<LexTrait>();
			Fields = new List<LexField>();
		}

		public List<LexTrait> Traits{get;private set;}
		public List<LexField> Fields { get; private set; }

	}
}