using System.Collections.Generic;
using Palaso.Lift;

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
	public class LexField : MultiText
	{
		public string Type;
		public List<LexTrait> Traits = new List<LexTrait>();

		public LexField(string type)
		{
			Type = type;
		}
	}
}