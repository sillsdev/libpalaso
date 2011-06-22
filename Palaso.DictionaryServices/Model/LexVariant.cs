using System;
using System.Collections.Generic;
using System.ComponentModel;
using Palaso.Lift;
using Palaso.Reporting;

namespace Palaso.DictionaryServices.Model
{
	/// <summary>
	/// not implemented: ref
	/// not implemented: pronunciation
	/// not implemented: relation
	///
	/// </summary>
	public sealed class LexVariant:  MultiText, IExtensible
	{
		public LexVariant()
		{
			Traits = new List<LexTrait>();
			Fields = new List<LexField>();
		}

		#region IExtensible
		public List<LexTrait> Traits { get; private set; }
		public List<LexField> Fields { get; private set; }
		#endregion
	}
}