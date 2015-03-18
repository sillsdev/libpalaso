using System.IO;

namespace SIL.WritingSystems
{
	public class WritingSystemFactory : WritingSystemFactoryBase<WritingSystemDefinition>
	{
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

	public abstract class WritingSystemFactory<T> : WritingSystemFactoryBase<T> where T : WritingSystemDefinition
	{
		/// <summary>
		/// The folder in which the repository looks for template LDML files when a writing system is wanted.
		/// </summary>
		public string TemplateFolder { get; set; }

		public override T Create(string ietfLanguageTag)
		{
			string templatePath = null;
			// check template folder for template
			if (!string.IsNullOrEmpty(TemplateFolder))
			{
				templatePath = Path.Combine(TemplateFolder, ietfLanguageTag + ".ldml");
				if (!File.Exists(templatePath))
					templatePath = null;
			}

			T ws;
			if (!string.IsNullOrEmpty(templatePath))
			{
				ws = ConstructDefinition();
				var loader = new LdmlDataMapper(this);
				loader.Read(templatePath, ws);
				ws.Template = templatePath;
			}
			else
			{
				ws = ConstructDefinition(ietfLanguageTag);
			}

			return ws;
		}
	}
	 
}
