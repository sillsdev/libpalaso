using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Palaso
{
	public class WritingSystemRepository
	{
		private string _path;

		/// <summary>
		/// Use the default repository
		/// </summary>
		public WritingSystemRepository()
		{
			string p =
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SIL");
			Directory.CreateDirectory(p);
			p = Path.Combine(p, "WritingSystemRepository");
			Directory.CreateDirectory(p);
			PathToWritingSystems = p;
		}

		/// <summary>
		/// use a special path for the repository
		/// </summary>
		/// <param name="path"></param>
		public WritingSystemRepository(string path)
		{
			PathToWritingSystems = path;
		}

		public string PathToWritingSystems
		{
			get
			{
				return _path;
			}
			set
			{
				_path = value;
				if (!Directory.Exists(_path))
				{
					string parent = Directory.GetParent(_path).FullName;
					if (!Directory.Exists(parent))
					{
						throw new ApplicationException(
							"The writing system repository cannot be created because its parent folder, " + parent +
							", does not exist.");
					}
					Directory.CreateDirectory(_path);
				}
			}
		}

		public IEnumerable WritingSystemDefinitions
		{
			get
			{
				return new List<WritingSystemDefinition>();
			}
		}

		public WritingSystemDefinition CreateNewDefinition()
		{
			return new WritingSystemDefinition();
		}


		public void SaveDefinition(WritingSystemDefinition definition)
		{
			LdmlAdaptor adaptor = new LdmlAdaptor();
			adaptor.SaveToRepository(this, definition);
		}

		public WritingSystemDefinition LoadDefinition(string identifier)
		{
			LdmlAdaptor adaptor = new LdmlAdaptor();
			WritingSystemDefinition definition = new WritingSystemDefinition();
			adaptor.Load(this, identifier, definition);
			return definition;
		}

		public string GetFileName(WritingSystemDefinition definition)
		{
			LdmlAdaptor adaptor = new LdmlAdaptor();
			return adaptor.GetFileName(definition);
		}
	}
}