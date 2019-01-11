using System.IO;

namespace SIL.WritingSystems
{
	public class LdmlInFolderWritingSystemFactory : LdmlInFolderWritingSystemFactory<WritingSystemDefinition>
	{
		public LdmlInFolderWritingSystemFactory(LdmlInFolderWritingSystemRepository<WritingSystemDefinition> writingSystemRepository)
			: base(writingSystemRepository)
		{
		}

		protected override WritingSystemDefinition ConstructDefinition()
		{
			return new WritingSystemDefinition();
		}

		protected override WritingSystemDefinition ConstructDefinition(string ietfLanguageTag)
		{
			return new WritingSystemDefinition(ietfLanguageTag);
		}

		protected override WritingSystemDefinition ConstructDefinition(WritingSystemDefinition ws, bool cloneId = false)
		{
			return new WritingSystemDefinition(ws, cloneId);
		}
	}

	public abstract class LdmlInFolderWritingSystemFactory<T> : SldrWritingSystemFactory<T> where T : WritingSystemDefinition
	{
		private readonly LdmlInFolderWritingSystemRepository<T> _writingSystemRepository;

		protected LdmlInFolderWritingSystemFactory(LdmlInFolderWritingSystemRepository<T> writingSystemRepository)
		{
			_writingSystemRepository = writingSystemRepository;
		}

		public override bool Create(string ietfLanguageTag, out T ws)
		{
			// check local repo for template
			T existingWS;
			if (_writingSystemRepository.TryGet(ietfLanguageTag, out existingWS))
			{
				ws = ConstructDefinition(existingWS);
				string templatePath = _writingSystemRepository.GetFilePathFromLanguageTag(existingWS.LanguageTag);
				if (File.Exists(templatePath))
					ws.Template = templatePath;
				return true;
			}

			// check global repo for template
			if (_writingSystemRepository.GlobalWritingSystemRepository != null
				&& _writingSystemRepository.GlobalWritingSystemRepository.TryGet(ietfLanguageTag, out existingWS))
			{
				ws = ConstructDefinition(existingWS);
				string templatePath = _writingSystemRepository.GlobalWritingSystemRepository.GetFilePathFromLanguageTag(existingWS.LanguageTag);
				if (File.Exists(templatePath))
					ws.Template = templatePath;
				return true;
			}

			return base.Create(ietfLanguageTag, out ws);
		}
	}
}
