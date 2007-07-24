using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Palaso
{
	public class WritingSystemRepository
	{
		private string _path;

		public WritingSystemRepository(string path)
		{
			_path = path;
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

		public string Path
		{
			get
			{
				return _path;
			}
		}
	}

	public class WritingSystemDefinition
	{
		private string _iso;
		private string _region;
		private string _variant;

		/// <summary>
		/// The file names we should try to delete when next we are saved,
		/// caused by a change in properties used to construct the name.
		/// </summary>
		private List<string> _oldFileNames = new List<string>();

		public string Variant
		{
			get
			{
				return _variant;
			}
			set
			{
				if(_variant == value)
					return;
				RecordOldName();
				_variant = value;
			}
		}

		private void RecordOldName()
		{
			_oldFileNames.Add(FileName);
		}

		public string Region
		{
			get
			{
				return _region;
			}
			set
			{
				if (_region == value)
					return;
				RecordOldName();
				_region = value;
			}
		}

		public string ISO
		{
			get
			{
				return _iso;
			}
			set
			{
				if (_iso == value)
					return;
				RecordOldName();
				_iso = value;
			}
		}

		public void SaveToRepository(WritingSystemRepository repository)
		{
			foreach (string fileName in _oldFileNames)
			{
				string path = Path.Combine(repository.Path, fileName);
				if (File.Exists(path))
				{
					try
					{
						File.Delete(path);
					}
					catch (Exception error)
					{
						//swallow. It's ok, we're just trying to clean up.
					}

				}
			}
			File.WriteAllText(Path.Combine(repository.Path,FileName), "hello");
		}

		public string FileName
		{
			get
			{
				string name = "";
				if (String.IsNullOrEmpty(_iso))
				{
					name = "unknown";
				}
				else
				{
					name = _iso;
				}
				if (!String.IsNullOrEmpty(_region))
				{
					name += "-" + _region;
				}
				if (!String.IsNullOrEmpty(_variant))
				{
					name += "-" + _variant;
				}
				return name + ".ldml";
			}
		}
	}
}
