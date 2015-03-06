using System.IO;
using System.Net;

namespace SIL.WritingSystems
{
	public class SldrWritingSystemFactory : SldrWritingSystemFactory<WritingSystemDefinition>
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

	public abstract class SldrWritingSystemFactory<T> : IWritingSystemFactory<T> where T : WritingSystemDefinition
	{
		public T Create()
		{
			return ConstructDefinition();
		}

		public virtual T Create(string ietfLanguageTag)
		{
			// check SLDR for template
			string sldrCachePath = Path.Combine(Path.GetTempPath(), "SldrCache");
			Directory.CreateDirectory(sldrCachePath);
			string templatePath = Path.Combine(sldrCachePath, ietfLanguageTag + ".ldml");
			if (!GetLdmlFromSldr(templatePath, ietfLanguageTag))
			{
				// check SLDR cache for template
				if (!File.Exists(templatePath))
					templatePath = null;
			}

			// check template folder for template
			if (string.IsNullOrEmpty(templatePath) && !string.IsNullOrEmpty(TemplateFolder))
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

		public T Create(T ws)
		{
			return ConstructDefinition(ws);
		}

		/// <summary>
		/// The folder in which the repository looks for template LDML files when a writing system is wanted
		/// that cannot be found in the local store, global store, or SLDR.
		/// </summary>
		public string TemplateFolder { get; set; }

		/// <summary>
		/// Creates an empty writing system. This is implemented by subclasses to allow the use
		/// subclasses of WritingSystemDefinition.
		/// </summary>
		protected abstract T ConstructDefinition();

		/// <summary>
		/// Creates an empty writing system with the specified language tag. This is implemented
		/// by subclasses to allow the use subclasses of WritingSystemDefinition.
		/// </summary>
		protected abstract T ConstructDefinition(string ietfLanguageTag);

		/// <summary>
		/// Clones the specified writing system. This is implemented by subclasses to allow the
		/// use subclasses of WritingSystemDefinition.
		/// </summary>
		protected abstract T ConstructDefinition(T ws);

		/// <summary>
		/// Gets the a LDML file from the SLDR.
		/// </summary>
		protected virtual bool GetLdmlFromSldr(string path, string id)
		{
			try
			{
				Sldr.GetLdmlFile(path, id);
				return true;
			}
			catch (WebException)
			{
				return false;
			}
		}

		WritingSystemDefinition IWritingSystemFactory.Create()
		{
			return Create();
		}

		WritingSystemDefinition IWritingSystemFactory.Create(string ietfLanguageTag)
		{
			return Create(ietfLanguageTag);
		}

		WritingSystemDefinition IWritingSystemFactory.Create(WritingSystemDefinition ws)
		{
			return Create((T) ws);
		}
	}
}
