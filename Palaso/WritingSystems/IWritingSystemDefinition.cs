using System;
using System.Collections.Generic;
using Palaso.WritingSystems.Collation;

namespace Palaso.WritingSystems
{
	public interface IWritingSystemDefinition
	{
		/// <summary>
		/// True when the validity of the writing system defn's tag is being enforced. This is the normal and default state.
		/// Setting this true will throw unless the tag has previously been put into a valid state.
		/// Attempting to Save the writing system defn will set this true (and may throw).
		/// </summary>
		bool RequiresValidTag { get; set; }

		///<summary>
		///This is the version of the locale data contained in this writing system.
		///This should not be confused with the version of our writingsystemDefinition implementation which is mostly used for migration purposes.
		///That information is stored in the "LatestWritingSystemDefinitionVersion" property.
		///</summary>
		string VersionNumber { get; set; }

		string VersionDescription { get; set; }
		DateTime DateModified { get; set; }
		IEnumerable<Iso639LanguageCode> ValidLanguages { get; }
		IEnumerable<Iso15924Script> ValidScript { get; }
		IEnumerable<IanaSubtag> ValidRegions { get; }
		IEnumerable<IanaSubtag> ValidVariants { get; }

		/// <summary>
		/// Adjusts the BCP47 tag to indicate the desired form of Ipa by inserting fonipa in the variant and emic or etic in private use where necessary.
		/// </summary>
		IpaStatusChoices IpaStatus { get; set; }

		/// <summary>
		/// Adjusts the BCP47 tag to indicate that this is an "audio writing system" by inserting "audio" in the private use and "Zxxx" in the script
		/// </summary>
		bool IsVoice { get; set; }

		/// <summary>
		/// A string representing the subtag of the same name as defined by BCP47.
		/// Note that the variant also includes the private use subtags. These are appended to the variant subtags seperated by "-x-"
		/// Also note the convenience methods "SplitVariantAndPrivateUse" and "ConcatenateVariantAndPrivateUse" for easier
		/// variant/ private use handling
		/// </summary>
// Todo: this could/should become an ordered list of variant tags
		string Variant { get; set; }

		/// <summary>
		/// A string representing the subtag of the same name as defined by BCP47.
		/// </summary>
		string Region { get; set; }

		/// <summary>
		/// A string representing the subtag of the same name as defined by BCP47.
		/// </summary>
		string Language { get; set; }

		/// <summary>
		/// The desired abbreviation for the writing system
		/// </summary>
		string Abbreviation { get; set; }

		/// <summary>
		/// A string representing the subtag of the same name as defined by BCP47.
		/// </summary>
		string Script { get; set; }

		/// <summary>
		/// The language name to use. Typically this is the language name associated with the BCP47 language subtag as defined by the IANA subtag registry
		/// </summary>
		string LanguageName { get; set; }

		/// <summary>
		/// Used by IWritingSystemRepository to identify writing systems. Only change this if you would like to replace a writing system with the same StoreId
		/// already contained in the repo. This is useful creating a temporary copy of a writing system that you may or may not care to persist to the
		/// IWritingSystemRepository.
		/// Typical use would therefor be:
		/// ws.Clone(wsorig);
		/// ws.StoreId=wsOrig.StoreId;
		/// **make changes to ws**
		/// repo.Set(ws);
		/// </summary>
		string StoreID { get; set; }

		/// <summary>
		/// A automatically generated descriptive label for the writing system definition.
		/// </summary>
		string DisplayLabel { get; }

		string ListLabel { get; }

		/// <summary>
		/// The current BCP47 tag which is a concatenation of the Language, Script, Region and Variant properties.
		/// </summary>
		string Bcp47Tag { get; }

		/// <summary>
		/// The identifier for this writing syetm definition. Use this in files and as a key to the IWritingSystemRepository.
		/// Note that this is usually identical to the Bcp47 tag and should rarely differ.
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Indicates whether the writing system definition has been modified.
		/// Note that this flag is automatically set by all methods that cause a modification and is reset by the IwritingSystemRepository.Save() method
		/// </summary>
		bool Modified { get; set; }

		bool MarkedForDeletion { get; set; }

		/// <summary>
		/// The font used to display data encoded in this writing system
		/// </summary>
		string DefaultFontName { get; set; }

		/// <summary>
		/// the preferred font size to use for data encoded in this writing system.
		/// </summary>
		float DefaultFontSize { get; set; }

		/// <summary>
		/// This tracks the keyboard that should be used for this writing system on this computer.
		/// It is not shared with other users of the project.
		/// </summary>
		IKeyboardDefinition LocalKeyboard { get; set; }

		/// <summary>
		/// Keyboards known to have been used with this writing system. Not all may be available on this system.
		/// Enhance: document (or add to this interface?) a way of getting available keyboards.
		/// </summary>
		IEnumerable<IKeyboardDefinition> KnownKeyboards { get; }

		/// <summary>
		/// Note that a new keyboard is known to be used for this writing system.
		/// </summary>
		/// <param name="newKeyboard"></param>
		void AddKnownKeyboard(IKeyboardDefinition newKeyboard);

		/// <summary>
		/// Returns the available keyboards (known to Keyboarding.Controller) that are not KnownKeyboards for this writing system.
		/// </summary>
		IEnumerable<IKeyboardDefinition> OtherAvailableKeyboards { get; }

		/// <summary>
		/// Indicates whether this writing system is read and written from left to right or right to left
		/// </summary>
		bool RightToLeftScript { get; set; }

		/// <summary>
		/// The windows "NativeName" from the Culture class
		/// </summary>
		string NativeName { get; set; }

		/// <summary>
		/// Indicates the type of sort rules used to encode the sort order.
		/// Note that the actual sort rules are contained in the SortRules property
		/// </summary>
		WritingSystemDefinition.SortRulesType SortUsing { get; set; }

		/// <summary>
		/// The sort rules that efine the sort order.
		/// Note that you must indicate the type of sort rules used by setting the "SortUsing" property
		/// </summary>
		string SortRules { get; set; }

		/// <summary>
		/// The id used to select the spell checker.
		/// </summary>
		string SpellCheckingId { get; set; }

		/// <summary>
		/// Returns an ICollator interface that can be used to sort strings based
		/// on the custom collation rules.
		/// </summary>
		ICollator Collator { get; }

		/// <summary>
		/// Indicates whether this writing system is unicode encoded or legacy encoded
		/// </summary>
		bool IsUnicodeEncoded { get; set; }

		/// <summary>
		/// Adds a valid BCP47 registered variant subtag to the variant. Any other tag is inserted as private use.
		/// </summary>
		/// <param name="registeredVariantOrPrivateUseSubtag">A valid variant tag or another tag which will be inserted into private use.</param>
		void AddToVariant(string registeredVariantOrPrivateUseSubtag);

		/// <summary>
		/// Sets all BCP47 language tag components at once.
		/// This method is useful for avoiding invalid intermediate states when switching from one valid tag to another.
		/// </summary>
		/// <param name="language">A valid BCP47 language subtag.</param>
		/// <param name="script">A valid BCP47 script subtag.</param>
		/// <param name="region">A valid BCP47 region subtag.</param>
		/// <param name="variant">A valid BCP47 variant subtag.</param>
		void SetAllComponents(string language, string script, string region, string variant);

		/// <summary>
		/// enforcing a minimum on _defaultFontSize, while reasonable, just messed up too many IO unit tests
		/// </summary>
		/// <returns></returns>
		float GetDefaultFontSizeOrMinimum();

		/// <summary>
		/// A convenience method for sorting like anthoer language
		/// </summary>
		/// <param name="languageCode">A valid language code</param>
		void SortUsingOtherLanguage(string languageCode);

		/// <summary>
		/// A convenience method for sorting with custom ICU rules
		/// </summary>
		/// <param name="sortRules">custom ICU sortrules</param>
		void SortUsingCustomICU(string sortRules);

		/// <summary>
		/// A convenience method for sorting with "shoebox" style rules
		/// </summary>
		/// <param name="sortRules">"shoebox" style rules</param>
		void SortUsingCustomSimple(string sortRules);

		/// <summary>
		/// Tests whether the current custom collation rules are valid.
		/// </summary>
		/// <param name="message">Used for an error message if rules do not validate.</param>
		/// <returns>True if rules are valid, false otherwise.</returns>
		bool ValidateCollationRules(out string message);

		string ToString();

		/// <summary>
		/// Creates a clone of the current writing system.
		/// Note that this excludes the properties: Modified, MarkedForDeletion and StoreID
		/// </summary>
		/// <returns></returns>
		WritingSystemDefinition Clone();

		bool Equals(Object obj);
		bool Equals(WritingSystemDefinition other);

		/// <summary>
		/// Parses the supplied BCP47 tag and sets the Language, Script, Region and Variant properties accordingly
		/// </summary>
		/// <param name="completeTag">A valid BCP47 tag</param>
		void SetTagFromString(string completeTag);
	}


	/// <summary>
	/// An additional interface that is typically implemented along with IWritingSystemDefinition. This interface gives access to two pieces of
	/// information which may be present in an older LDML file, especially one which does not have KnownKeyboards, and which may be used
	/// to determine a keyboard to be used when KnownKeyboards and LocalKeyboard are not set.
	/// </summary>
	public interface ILegacyWritingSystemDefinition
	{
		/// <summary>
		/// This field retrieves the value obtained from the FieldWorks LDML extension fw:windowsLCID.
		/// This is used only when current information in LocalKeyboard or KnownKeyboards is not useable.
		/// There is no public setter because it is not useful to modify this or set it in new LDML files.
		/// </summary>
		string WindowsLcid { get; }

		/// <summary>
		/// Legacy keyboard information. Current code should use LocalKeyboard or KnownKeyboards.
		/// This field is kept because it is useful in figuring out a keyboard to use when importing an old LDML file.
		/// </summary>
		string Keyboard { get; set; }
	}
}