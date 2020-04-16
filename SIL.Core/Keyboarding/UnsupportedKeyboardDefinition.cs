using System;
using System.Collections.Generic;
using System.Text;

namespace SIL.Keyboarding
{
	/// <summary>
	/// This class represents an unsupported or unrecognized keyboard definition.
	/// It allows round tripping unrecognized keyboard formats to ldml
	/// </summary>
	public class UnsupportedKeyboardDefinition : DefaultKeyboardDefinition
	{
		public UnsupportedKeyboardDefinition(string id) : base(id, "(default)")
		{
			IsAvailable = false;
		}

		public override bool Equals(object obj)
		{
			return Id == (obj as UnsupportedKeyboardDefinition)?.Id;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 71;
				hash *= 31 + Id.GetHashCode();
				hash *= 31 + base.GetHashCode();
				return hash;
			}
		}
	}
}
