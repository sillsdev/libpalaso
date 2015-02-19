using System.Collections.Generic;
using System.Linq;

namespace SIL.WritingSystems
{
	/// <summary>
	/// This class represents a variant from the IANA language subtag registry.
	/// </summary>
	public class VariantSubtag : Subtag
	{
		private readonly HashSet<string> _prefixes;

		/// <summary>
		/// Initializes a new private-use instance of the <see cref="VariantSubtag"/> class.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="name">The name.</param>
		public VariantSubtag(string code, string name = null)
			: base(code, name, true)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VariantSubtag"/> class.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="name">The name.</param>
		/// <param name="isPrivateUse">if set to <c>true</c> this is a private use subtag.</param>
		/// <param name="prefixes">The prefixes.</param>
		internal VariantSubtag(string code, string name, bool isPrivateUse, IEnumerable<string> prefixes)
			: base(code, name, isPrivateUse)
		{
			_prefixes = new HashSet<string>(prefixes ?? Enumerable.Empty<string>());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:VariantSubtag"/> class.
		/// </summary>
		/// <param name="subtag">The subtag.</param>
		/// <param name="name">The name.</param>
		public VariantSubtag(VariantSubtag subtag, string name)
			: this(subtag.Code, name, subtag.IsPrivateUse, subtag._prefixes)
		{
		}

		/// <summary>
		/// Determines whether this subtag can be used with the specified lang tag.
		/// </summary>
		/// <param name="langTag">The lang tag.</param>
		/// <returns>
		///		<c>true</c> if this subtag can be used with the specified lang tag, otherwise <c>false</c>.
		/// </returns>
		public bool IsVariantOf(string langTag)
		{
			if (_prefixes.Count == 0)
				return true;

			return _prefixes.Any(langTag.StartsWith);
		}

		/// <summary>
		/// Gets the prefixes.
		/// </summary>
		/// <value>The prefixes.</value>
		public IEnumerable<string> Prefixes
		{
			get { return _prefixes; }
		}

		public static implicit operator VariantSubtag(string code)
		{
			if (string.IsNullOrEmpty(code))
				return null;

			VariantSubtag subtag;
			if (!StandardSubtags.RegisteredVariants.TryGet(code, out subtag))
			{
				if (!StandardSubtags.CommonPrivateUseVariants.TryGet(code, out subtag))
					subtag = new VariantSubtag(code);
			}
			return subtag;
		}
	}
}
