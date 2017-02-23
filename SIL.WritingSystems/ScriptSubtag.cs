namespace SIL.WritingSystems
{
	public class ScriptSubtag : Subtag
	{
		/// <summary>
		/// Initializes a new private-use instance of the <see cref="ScriptSubtag"/> class.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="name">The name.</param>
		public ScriptSubtag(string code, string name = null)
			: base(code, name, true, false)
		{
		}

		internal ScriptSubtag(string code, string name, bool isPrivateUse, bool isDeprecated)
			: base(code, name, isPrivateUse, isDeprecated)
		{
		}

		public ScriptSubtag(ScriptSubtag subtag, string name)
			: this(subtag.Code, name, subtag.IsPrivateUse, subtag.IsDeprecated)
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
			if (!StandardSubtags.RegisteredScripts.TryGet(code, out subtag))
				subtag = new ScriptSubtag(code);
			return subtag;
		}
	}
}
