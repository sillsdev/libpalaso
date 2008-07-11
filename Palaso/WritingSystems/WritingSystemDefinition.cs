using System;
using System.Collections.Generic;
using System.Text;

namespace Palaso.WritingSystems
{
	public class WritingSystemDefinition
	{
		private string _iso;
		private string _region;
		private string _variant;
		private string _languageName;
		private string _script;
		private string _abbreviation;

		private string _versionNumber;
		private string _versionDescription;

		private DateTime _dateModified;

		private string _defaultFontName;
		private string _keyboard;

		private bool _modified;

		/// <summary>
		/// Other classes that persist this need to know when our id changed, so the can
		/// clean up the old copy which is based on the old name.
		/// </summary>
		private string _storeID;

		/// <summary>
		/// singleton
		/// </summary>
		private static List<ScriptOption> _scriptOptions = new List<ScriptOption>();
	   /// <summary>
		/// singleton
		/// </summary>
		private static List<LanguageCode> _languageCodes;

		private bool _markedForDeletion;
		private string _nativeName;
		private bool _rightToLeftScript;


		public WritingSystemDefinition()
		{
			LoadScriptOptions();
			Modified = false;
		}

		public WritingSystemDefinition(string iso)
			: this()
		{
			_iso = iso;
		}

		public WritingSystemDefinition(string iso, string script, string region, string variant, string languageName, string abbreviation, bool rightToLeftScript)
			: this()
		{
			_region = region;
			_iso = iso;
			_abbreviation = abbreviation;
			_script = script;
			_languageName = languageName;
			_variant = variant;
			_rightToLeftScript = rightToLeftScript;
		}

		/// <summary>
		/// parse in the text of the script registry we get from http://unicode.org/iso15924/iso15924-text.html
		/// </summary>
		private static void LoadScriptOptions()
		{
		  if (_scriptOptions.Count > 0)
			  return;

		  //this one isn't an official script
		  _scriptOptions.Add(new ScriptOption("IPA", "Zipa"));
			//to help people find Latin
		  _scriptOptions.Add(new ScriptOption("Roman (Latin)", "Latn"));

			string[] scripts = Resource.scriptNames.Split('\n');
			foreach (string line in scripts)
			{
				string tline = line.Trim();
				if (tline.Length==0 || (tline.Length > 0 && tline[0]=='#'))
					continue;
				string[] fields = tline.Split(';');
				string label = fields[2];

				//these looks awful: "Korean (alias for Hangul + Han)"
				// and "Japanese (alias for Han + Hiragana + Katakana"
				if (label.IndexOf(" (alias") > -1)
				{
					label = label.Substring(0, fields[2].IndexOf(" (alias "));
				}
				_scriptOptions.Add(new ScriptOption(label, fields[0]));

			}

			_scriptOptions.Sort(ScriptOption.CompareScriptOptions);
		}


		public static IList<LanguageCode> LanguageCodes
		{
			get
			{
				if (_languageCodes != null)
				{
					return _languageCodes;
				}
				_languageCodes = new List<LanguageCode>();
				string[] languages = Resource.languageCodes.Split('\n');
				foreach (string line in languages)
				{
					if(line.Contains("Ref_Name"))//skip first line
						continue;
					string tline = line.Trim();
					if (tline.Length == 0)
						continue;
					string[] fields = tline.Split('\t');
					_languageCodes.Add(new LanguageCode(fields[0], fields[6]));
				}
				_languageCodes.Sort(LanguageCode.CompareByName);
				return _languageCodes;
			}
		}

		public class LanguageCode
		{
			private string _code;
			private string _name;

			public LanguageCode(string code, string name)
			{
				Code = code;
				Name = name;
			}


			public string Name
			{
				get
				{
					return _name;
				}
				set
				{
					_name = value;
				}
			}

			public string Code
			{
				get
				{
					return _code;
				}
				set
				{
					_code = value;
				}
			}

			public static int CompareByName(LanguageCode x, LanguageCode y)
			{
				if (x == null)
				{
					if (y == null)
					{
						return 0;
					}
					else
					{
						return -1;
					}
				}
				else
				{
					if (y == null)
					{
						return 1;
					}
					else
					{
						return x._name.CompareTo(y._name);
					}
				}
			}

		}

		public string VersionNumber
		{
			get { return _versionNumber; }
			set { _versionNumber = value; }
		}

		public string VersionDescription
		{
			get { return _versionDescription; }
			set { _versionDescription = value; }
		}

		public DateTime DateModified
		{
			get { return _dateModified; }
			set { _dateModified = value; }
		}

		public string Variant
		{
			get
			{
				return _variant;
			}
			set
			{
				UpdateString(ref _variant, value);

			}
		}

		public string Region
		{
			get
			{
				return _region;
			}
			set
			{
				UpdateString(ref _region, value);
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
				UpdateString(ref _iso, value);
			}
		}

		public string Abbreviation
		{
			get
			{
				return _abbreviation;
			}
			set
			{
				UpdateString(ref _abbreviation, value);
			}
		}

		public string Script
		{
			get
			{
				return _script;
			}
			set
			{
				UpdateString(ref _script, value);
			}
		}

		public string LanguageName
		{
			get
			{
				return _languageName;
			}
			set
			{
				UpdateString(ref _languageName, value);
			}
		}


		private void UpdateString(ref string field, string value)
		{
			if (field == value)
				return;

			//count null as same as ""
			if (String.IsNullOrEmpty(field) && String.IsNullOrEmpty(value))
			{
				return;
			}
			Modified = true;
			field = value;
		}

		/// <summary>
		/// Other classes that persist this need to know when our id changed, so the can
		/// clean up the old copy which is based on the old name.
		/// </summary>
		public string StoreID
		{
			get
			{
				return _storeID;
			}
			set
			{
				_storeID = value;
			}
		}

		public string DisplayLabel
		{
			get
			{
				if (!String.IsNullOrEmpty(_abbreviation))
				{
					return _abbreviation;
				}
				else
				{
					if (!String.IsNullOrEmpty(_iso))
					{
						return _iso;
					}
					else
					{
						if (!String.IsNullOrEmpty(_languageName))
						{
							string n = _languageName;
							return n.Substring(0, n.Length > 4 ? 4 : n.Length);
						}
					}
				}
				return "???";
			}
		}

		public string RFC4646
		{
			get
			{
				string id;
				if (String.IsNullOrEmpty(ISO))
				{
					id = "unknown";
				}
				else
				{
					id = ISO;
				}
				if (!String.IsNullOrEmpty(Script))
				{
					id += "-" + Script;
				}
				if (!String.IsNullOrEmpty(Region))
				{
					id += "-" + Region;
				}
				if (!String.IsNullOrEmpty(Variant))
				{
					id += "-" + Variant;
				}
				return id;
			}
		}

		public string VerboseDescription
		{
			get
			{
				StringBuilder summary = new StringBuilder();
				if (!String.IsNullOrEmpty(_variant))
				{
					summary.AppendFormat("{0}", _variant);
				}
				summary.AppendFormat(" {0}", string.IsNullOrEmpty(_languageName)?"???":_languageName);
				if (!String.IsNullOrEmpty(_region))
				{
					summary.AppendFormat(" in {0}", _region);
				}
				if (!String.IsNullOrEmpty(_script))
				{
					summary.AppendFormat(" written in {0} script", CurrentScriptOptionLabel);
				}

				summary.AppendFormat(". ({0})", RFC4646);
				return summary.ToString().Trim();
			}
		}

		private string CurrentScriptOptionLabel
		{
			get
			{
				ScriptOption option = CurrentScriptOption;
				if (option==null)
				{
					return _script;
				}
				return option.Label; //unrecognized, so return raw code
			}
		}

		/// <summary>
		/// If we don't have an option for the current script, returns null
		/// </summary>
		public ScriptOption CurrentScriptOption
		{
			get
			{
				string script = Script;
				if (String.IsNullOrEmpty(script))
				{
					script = "latn";
				}
				foreach (WritingSystemDefinition.ScriptOption option in _scriptOptions)
				{
					if (option.Code == script)
					{
						return option;
					}
				}
				return null;
			}
		}

		public List<ScriptOption> ScriptOptions
		{
			get
			{
				return _scriptOptions;
			}
		}

		public bool Modified
		{
			get
			{
				return _modified;
			}
			set
			{
				_modified = value;
			}
		}

		public bool MarkedForDeletion
		{
			get
			{
				return _markedForDeletion;
			}
			set
			{
				_markedForDeletion = value;
			}
		}

		public string DefaultFontName
		{
			get
			{
				return _defaultFontName;
			}
			set
			{
				UpdateString(ref _defaultFontName, value);
			}
		}

		public string Keyboard
		{
			get
			{
				return _keyboard;
			}
			set
			{
				UpdateString(ref _keyboard, value);
			}
		}

		public bool RightToLeftScript
		{
			get
			{
				return _rightToLeftScript;
			}
			set
			{
				if(value != _rightToLeftScript)
				{
					Modified = true;
					_rightToLeftScript = value;
				}
			}
		}

		public string NativeName
		{
			get
			{
				return _nativeName;
			}
			set
			{
				UpdateString(ref _nativeName, value);
			}
		}


		public class ScriptOption
		{
			private string _label;
			private string _code;

			public ScriptOption(string label, string code)
			{
				_label = label;
				_code = code;
			}

			public string Code
			{
				get
				{
					return _code;
				}
			}

			public string Label
			{
				get
				{
					return _label;
				}
			}

			public override string ToString()
			{
				return _label;
			}

			public static int CompareScriptOptions(ScriptOption x, ScriptOption y)
			{
				if (x == null)
				{
					if (y == null)
					{
						return 0;
					}
					else
					{
						return -1;
					}
				}
				else
				{
					if (y == null)
					{
						return 1;
					}
					else
					{
						return x.Label.CompareTo(y.Label);
					}
				}
			}
		}

		public WritingSystemDefinition Clone()
		{
			WritingSystemDefinition ws =
				new WritingSystemDefinition(_iso, _script, _region, _variant, _languageName, _abbreviation, _rightToLeftScript);
			return ws;
		}
	}
}