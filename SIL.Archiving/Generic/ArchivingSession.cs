using System.Collections.Generic;

namespace SIL.Archiving.Generic
{
	/// <summary>Contains the materials being archived, and their metadata</summary>
	public interface IArchivingSession : IArchivingGenericObject
	{
		/// <summary>The people who participated in this session</summary>
		//ArchivingActorCollection Actors { get; set; }

		/// <summary>The files to include in this session</summary>
		List<IArchivingFile> Files { get; set; }
	}
}
