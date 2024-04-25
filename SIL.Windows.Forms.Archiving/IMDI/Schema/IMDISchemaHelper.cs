
namespace SIL.Windows.Forms.Archiving.IMDI.Schema
{
	/// <summary>Functions to simplify access to IMDI objects</summary>
	public static class IMDISchemaHelper
	{
		/// <summary>Creates a new Vocabulary_Type object and sets the Value</summary>
		/// <param name="value"></param>
		/// <param name="isClosedVocabulary"></param>
		/// <param name="link"></param>
		/// <returns></returns>
		public static VocabularyType SetVocabulary(string value, bool isClosedVocabulary, string link)
		{
			if (value == null)
				return null;

			return new VocabularyType
			{
				Value = value,
				Type = isClosedVocabulary
					? VocabularyTypeValueType.ClosedVocabulary
					: VocabularyTypeValueType.OpenVocabulary,
				Link = link
			};
		}
	}
}
