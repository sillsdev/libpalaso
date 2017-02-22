namespace SIL.WritingSystems
{
	public class RegionSubtag : Subtag
	{
		/// <summary>
		/// Initializes a new private-use instance of the <see cref="RegionSubtag"/> class.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="name">The name.</param>
		public RegionSubtag(string code, string name = null)
			: base(code, name, true, false)
		{
		}

		internal RegionSubtag(string code, string name, bool isPrivateUse, bool isDeprecated)
			: base(code, name, isPrivateUse, isDeprecated)
		{
		}

		public RegionSubtag(RegionSubtag subtag, string name)
			: this(subtag.Code, name, subtag.IsPrivateUse, subtag.IsDeprecated)
		{
		}

		public static implicit operator RegionSubtag(string code)
		{
			if (string.IsNullOrEmpty(code))
				return null;

			RegionSubtag subtag;
			if (!StandardSubtags.RegisteredRegions.TryGet(code, out subtag))
				subtag = new RegionSubtag(code);
			return subtag;
		}
	}
}
