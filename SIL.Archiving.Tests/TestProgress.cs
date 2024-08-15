namespace SIL.Archiving.Tests
{
	internal class TestProgress : IArchivingProgressDisplay
	{
		public int Step { get; private set; } = 0;
		public string ArchiveTypeName { get; }

		internal TestProgress(string type)
		{
			ArchiveTypeName = $"{type} (Test)";
		}

		public void IncrementProgress()
		{
			Step++;
		}

		public string GetMessage(ArchivingDlgViewModel.StringId msgId)
		{
			return $"Test implementation message for {msgId}";
		}
	}
}
