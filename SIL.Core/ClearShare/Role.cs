using System;
using System.Text;

namespace SIL.Core.ClearShare
{
	public class Role
	{
		public string Code { get; private set; }
		public string Name { get; private set; }
		public string Definition { get; private set; }
		public override string ToString() { return Name; }

		/// ------------------------------------------------------------------------------------
		public Role(string code, string name, string definition)
		{
			Code = code;
			Name = name;

			if (definition != null)
			{
				var bldr = new StringBuilder(definition.Length);
				foreach (var word in definition.Split(new[] { ' ', '\t', '\n', '\r' },
					StringSplitOptions.RemoveEmptyEntries))
				{
					bldr.AppendFormat("{0} ", word);
				}

				bldr.Length--;
				Definition = bldr.ToString();
			}
		}

		/// ------------------------------------------------------------------------------------
		public Role Clone()
		{
			return new Role(Code, Name, Definition);
		}

		/// ------------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			unchecked
			{
				int result = (Code != null ? Code.GetHashCode() : 0);
				result = (result * 397) ^ (Name != null ? Name.GetHashCode() : 0);
				result = (result * 397) ^ (Definition != null ? Definition.GetHashCode() : 0);
				return result;
			}
		}

		/// ------------------------------------------------------------------------------------
		public override bool Equals(object other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (GetType() != other.GetType())
				return false;

			return AreContentsEqual(other as Role);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns true if the contents of this Role are the same as those of the specified
		/// Role.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AreContentsEqual(Role other)
		{
			if (other == null)
				return false;

			// REVIEW: Do we really care if name and definition are different. Perhaps only
			// the code really matters.
			return (Code.Equals(other.Code) && Name.Equals(other.Name) &&
				Definition.Equals(other.Definition));
		}
	}
}