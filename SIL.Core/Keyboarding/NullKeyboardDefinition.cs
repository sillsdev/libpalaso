using System;
using System.Collections.Generic;
using System.Text;

namespace SIL.Keyboarding
{
	/// <summary>
	/// This class represents an unsupported or unrecognized keyboard definition.
	/// It allows round tripping unrecognized keyboard formats to ldml
	/// </summary>
	public class NullKeyboardDefinition : DefaultKeyboardDefinition
	{
		public NullKeyboardDefinition(string id = "<no keyboard>") : base(id, "(default)")
		{
			IsAvailable = false;
		}

		public override bool Equals(object obj)
		{
			return Id == (obj as NullKeyboardDefinition)?.Id;
		}
	}
}
