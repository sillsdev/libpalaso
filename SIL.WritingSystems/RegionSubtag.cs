namespace SIL.WritingSystems
{
	public class RegionSubtag : Subtag
	{
		/// <summary>
		/// Initializes a new private-use instance of the <see cref="RegionSubtag"/> class.
		/// </summary>
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

		public virtual string DisplayLabel
		{
			get
			{
				// There are two standard Private Use Regions (AA and ZZ). Show users which is which in lists and comboboxes.
				// The first item in the combobox is blank (and only the PU c'tor is available); don't add its code to its DisplayLabel.
				return IsPrivateUse && !string.IsNullOrEmpty(Name) ? $"{Name} ({Code})" : Name;
			}
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
