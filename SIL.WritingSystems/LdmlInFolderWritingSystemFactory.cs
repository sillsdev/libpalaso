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

		protected override WritingSystemDefinition ConstructDefinition(WritingSystemDefinition ws)
		{
			return new WritingSystemDefinition(ws);
		}
	}

	public abstract class LdmlInFolderWritingSystemFactory<T> : SldrWritingSystemFactory<T> where T : WritingSystemDefinition
	{
		private readonly LdmlInFolderWritingSystemRepository<T> _writingSystemRepository;

		protected LdmlInFolderWritingSystemFactory(LdmlInFolderWritingSystemRepository<T> writingSystemRepository)
		{
			_writingSystemRepository = writingSystemRepository;
		}

		public override T Create(string ietfLanguageTag)
		{
			// check local repo for template
			T existingWS;
			if (_writingSystemRepository.TryGet(ietfLanguageTag, out existingWS))
			{
				T newWS = ConstructDefinition(existingWS);
				string templatePath = _writingSystemRepository.GetFilePathFromLanguageTag(existingWS.LanguageTag);
				if (File.Exists(templatePath))
					newWS.Template = templatePath;
				return newWS;
			}

			// check global repo for template
			if (_writingSystemRepository.GlobalWritingSystemRepository != null
				&& _writingSystemRepository.GlobalWritingSystemRepository.TryGet(ietfLanguageTag, out existingWS))
			{
				T newWS = ConstructDefinition(existingWS);
				string templatePath = _writingSystemRepository.GlobalWritingSystemRepository.GetFilePathFromLanguageTag(existingWS.LanguageTag);
				if (File.Exists(templatePath))
					newWS.Template = templatePath;
				return newWS;
			}

			return base.Create(ietfLanguageTag);
		}
	}
}
