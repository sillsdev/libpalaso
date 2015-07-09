using System;
using System.ComponentModel;

namespace SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	/// <summary>
	/// This class stores the information used to define various writing system properties.
	/// Normally this would be internal, but some applications need visibility to migrate 
	/// legacy files to this version of the writing system definition.
	/// </summary>
	public class WritingSystemDefinitionV0
	{
		public WritingSystemDefinitionV0()
		{
			SortUsing = SortRulesType.DefaultOrdering;
		   // _defaultFontSize = 10; //arbitrary
		}

		public string VersionNumber { get; set; }

		public string VersionDescription { get; set; }

		public DateTime DateModified { get; set; }

		public string Variant { get; set; }

		public string Region{get; set;}

		public string ISO639{get;set;}

		public string Abbreviation{ get;set;}

		public string Script{get;set;}

		public string LanguageName{get; set;}

		public string Rfc5646
		{
			get
			{
				string id = String.IsNullOrEmpty(ISO639) ? string.Empty : ISO639;
				if (!String.IsNullOrEmpty(Script))
				{
					id += "-" + Script;
				}
				if (!String.IsNullOrEmpty(Region))
				{
					id += "-" + Region;
				}
				if (!String.IsNullOrEmpty(Variant))
				{
					id += "-" + Variant;
				}
				return id;
			}
		}

		/// <summary>
		/// Other classes that persist this need to know when our id changed, so they can
		/// clean up the old copy which is based on the old name.
		/// </summary>
		public string StoreID { get; set; }

		public string DefaultFontName{get;set;}

		public float DefaultFontSize{get;set;}

		public string Keyboard{get;set;}

		public bool RightToLeftScript{get;set;}

		public SortRulesType SortUsing { get; set; }

		public string SortRules { get; set; }

		public string SpellCheckingId { get; set; }

		public bool IsLegacyEncoded { get; set; }

		public enum SortRulesType
		{
			/// <summary>
			/// Default Unicode ordering rules (actually CustomICU without any rules)
			/// </summary>
			[Description("Default Ordering")]
			DefaultOrdering,
			/// <summary>
			/// Custom Simple (Shoebox/Toolbox) style rules
			/// </summary>
			[Description("Custom Simple (Shoebox style) rules")]
			CustomSimple,
			/// <summary>
			/// Custom ICU rules
			/// </summary>
			[Description("Custom ICU rules")]
			CustomICU,
			/// <summary>
			/// Use the sort rules from another language. When this is set, the SortRules are interpreted as a cultureId for the language to sort like.
			/// </summary>
			[Description("Same as another language")]
			OtherLanguage
		}
	}
}