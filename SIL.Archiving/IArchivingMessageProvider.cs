namespace SIL.Archiving
{
	public interface IArchivingProgressDisplay
	{
		void IncrementProgress();
		string GetMessage(ArchivingDlgViewModel.StringId msgId);
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Short name/description of the archiving program or standard used for this type of
		/// archiving. (Should fit in the frame "Archive using ___".) Typically not localized,
		/// but can potentially be.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		string ArchiveTypeName { get; }
	}
}
