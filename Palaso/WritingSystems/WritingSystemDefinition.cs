using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Palaso
{
	public class WritingSystemDefinition
	{
		private string _iso;
		private string _region;
		private string _variant;
		private string _languageName;

		/// <summary>
		/// The file names we should try to delete when next we are saved,
		/// caused by a change in properties used to construct the name.
		/// </summary>
		//private List<string> _oldFileNames = new List<string>();

		private string _abbreviation;
		private string _script;
		private string _previousRepositoryIdentifier;

		public WritingSystemDefinition()
		{

		}

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
				_variant = value;
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
				if (_region == value)
					return;
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
				_iso = value;
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
				if (_abbreviation == value)
					return;
				//no, abbreviation is not part of the name: RecordOldName();
				_abbreviation = value;
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
				if (_script == value)
					return;
				_script = value;
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
				_languageName = value;
			}
		}

		public string PreviousRepositoryIdentifier
		{
			get
			{
				return _previousRepositoryIdentifier;
			}
			set
			{
				_previousRepositoryIdentifier = value;
			}
		}
	}
}
