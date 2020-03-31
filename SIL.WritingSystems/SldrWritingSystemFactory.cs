using System;
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

		protected override WritingSystemDefinition ConstructDefinition(WritingSystemDefinition ws, bool cloneId = false)
		{
			return new WritingSystemDefinition(ws, cloneId);
		}
	}

	public abstract class SldrWritingSystemFactory<T> : WritingSystemFactory<T> where T : WritingSystemDefinition
	{
		public override bool Create(string ietfLanguageTag, out T ws)
		{
			// check SLDR for template
			if (!Directory.Exists(Sldr.SldrCachePath))
				Directory.CreateDirectory(Sldr.SldrCachePath);
			string templatePath;
			string filename;
			SldrStatus sldrStatus = GetLdmlFromSldr(Sldr.SldrCachePath, ietfLanguageTag, out filename);
			switch (sldrStatus)
			{
				case SldrStatus.FromSldr:
				case SldrStatus.FromCache:
					templatePath = Path.Combine(Sldr.SldrCachePath, filename);
					break;

				default:
					templatePath = null;
					break;
			}

			if (!string.IsNullOrEmpty(templatePath))
			{
				ws = ConstructDefinition();
				var loader = new LdmlDataMapper(this);
				var errorEncountered = false;
				loader.Read(templatePath, ws, e => {
					sldrStatus = SldrStatus.NotFound;
					errorEncountered = true;
				});
				if (!errorEncountered)
				{
					ws.Template = templatePath;
					return sldrStatus == SldrStatus.FromSldr;
				}
			}

			return base.Create(ietfLanguageTag, out ws) && sldrStatus == SldrStatus.NotFound;
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
