namespace SIL.WritingSystems
{
	public class RegionSubtag : Subtag
	{
		public RegionSubtag(string code, bool isPrivateUse)
			: base(code, isPrivateUse)
		{
		}

		public RegionSubtag(string code, string name, bool isPrivateUse)
			: base(code, name, isPrivateUse)
		{
		}

		public RegionSubtag(RegionSubtag subtag, string name)
			: this(subtag.Code, name, subtag.IsPrivateUse)
		{
		}

		public static implicit operator RegionSubtag(string code)
		{
			if (string.IsNullOrEmpty(code))
				return null;

			RegionSubtag subtag;
			if (!StandardSubtags.Iso3166Regions.TryGet(code, out subtag))
				subtag = new RegionSubtag(code, true);
			return subtag;
		}
	}
}
