
namespace SIL.Windows.Forms.Archiving.IMDI.Schema
{
	/// <summary>Interface to simplify access to session files (media and written)</summary>
	public interface IIMDISessionFile
	{
		/// <summary></summary>
		ResourceLinkType ResourceLink { get; set; }

		/// <summary></summary>
		VocabularyType Format { get; set; }

		/// <summary></summary>
		VocabularyType Type { get; set; }

		/// <summary></summary>
		string Size { get; set; }

		/// <summary></summary>
		DescriptionTypeCollection Description { get; set; }

		/// <summary></summary>
		AccessType Access { get; set; }

		/// <summary></summary>
		string FullPathAndFileName { get; set; }

		/// <summary></summary>
		string OutputDirectory { get; set; }
	}
}
