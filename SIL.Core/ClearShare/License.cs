namespace SIL.Core.ClearShare
{
	/// <summary>
	/// Describes a single license, under which many works can be licensed for use
	/// </summary>
	public class License
	{
		/// <summary>
		/// a web location describing the license
		/// </summary>
		public string Url { get; private set; }

		public string Name { get; private set; }

		//TODO: support the full six options at http://creativecommons.org/licenses/, plus public domain

		/// ------------------------------------------------------------------------------------
		public static License CreativeCommons_Attribution_ShareAlike =>
			new License
			{
				Name = "Creative Commons. Attribution-ShareAlike 3.0",
				Url = "http://creativecommons.org/licenses/by-sa/3.0/"
			};

		/// ------------------------------------------------------------------------------------
		public static License CreativeCommons_Attribution =>
			new License
			{
				Name = "Creative Commons. Attribution 3.0",
				Url = "http://creativecommons.org/licenses/by/3.0/"
			};

		/// ------------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			unchecked
			{
				int result = (Url != null ? Url.GetHashCode() : 0);
				result = (result * 397) ^ (Name != null ? Name.GetHashCode() : 0);
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

			return AreContentsEqual(other as License);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns true if the contents of this License are the same as those of the
		/// specified License.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AreContentsEqual(License other)
		{
			return (other != null && Name.Equals(other.Name) && Url.Equals(other.Url));
		}
	}
}
