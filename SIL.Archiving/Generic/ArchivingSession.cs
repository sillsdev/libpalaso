
using System.Collections.Generic;

namespace SIL.Archiving.Generic
{
	/// <summary>Contains the materials being archived, and their metadata</summary>
	public class ArchivingSession : ArchivingGenericObject
	{
		/// <summary>The people who participated in this session</summary>
		public ArchivingActorCollection Actors;

		/// <summary>The files to include in this session</summary>
		public List<IArchivingFile> Files;

		/// <summary>Default constructor</summary>
		public ArchivingSession()
		{
			Actors = new ArchivingActorCollection();
			Files = new List<IArchivingFile>();
			Location = new ArchivingLocation();
		}
	}
}
