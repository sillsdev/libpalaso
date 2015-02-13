namespace SIL.WritingSystems
{
	public class ScriptSubtag : Subtag
	{
		public ScriptSubtag(string code, bool isPrivateUse)
			: base(code, isPrivateUse)
		{
		}

		public ScriptSubtag(string code, string name, bool isPrivateUse)
			: base(code, name, isPrivateUse)
		{
		}

		public ScriptSubtag(ScriptSubtag subtag, string name)
			: this(subtag.Code, name, subtag.IsPrivateUse)
		{
		}

		public string ShortName
		{
			get
			{
				int index = Name.IndexOf(" (", System.StringComparison.Ordinal);
				if (index < 0)
					return Name;
				return Name.Substring(0, index);
			}
		}

		public static implicit operator ScriptSubtag(string code)
		{
			if (string.IsNullOrEmpty(code))
				return null;

			ScriptSubtag subtag;
			if (!StandardSubtags.Iso15924Scripts.TryGet(code, out subtag))
				subtag = new ScriptSubtag(code, true);
			return subtag;
		}
	}
}
