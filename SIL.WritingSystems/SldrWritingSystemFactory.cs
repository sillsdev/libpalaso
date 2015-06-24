using System.IO;

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

	public abstract class SldrWritingSystemFactory<T> : WritingSystemFactory<T> where T : WritingSystemDefinition
	{
		public override T Create(string ietfLanguageTag)
		{
			// check SLDR for template
			if (!Directory.Exists(Sldr.SldrCachePath))
				Directory.CreateDirectory(Sldr.SldrCachePath);
			string templatePath;
			string filename;
			switch (GetLdmlFromSldr(Sldr.SldrCachePath, ietfLanguageTag, out filename))
			{
				case SldrStatus.FromSldr:
				case SldrStatus.FromCache:
					templatePath = Path.Combine(Sldr.SldrCachePath, filename);
					break;

				default:
					templatePath = null;
					break;
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
				ws = base.Create(ietfLanguageTag);
			}

			return ws;
		}

		/// <summary>
		/// Gets the a LDML file from the SLDR.
		/// </summary>
		protected virtual SldrStatus GetLdmlFromSldr(string path, string id, out string filename)
		{
			return Sldr.GetLdmlFile(path, id, out filename);
		}
	}
}
