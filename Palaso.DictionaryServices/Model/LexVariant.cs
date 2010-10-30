using System;
using System.Collections.Generic;
using System.ComponentModel;
using LiftIO.Parsing;
using Palaso.Lift;
using Palaso.Reporting;

namespace Palaso.DictionaryServices.Model
{
	/// <summary>
	/// Components:
	/// Ref, pronunciation, relation: not implemented
	///
	/// </summary>
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