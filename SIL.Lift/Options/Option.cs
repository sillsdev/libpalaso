using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SIL.Extensions;
using SIL.UiBindings;

namespace SIL.Lift.Options
{
	[XmlRoot("option")]
	public class Option: IChoice
	{
		private MultiText _abbreviation;
		private MultiText _description;
		private string _humanReadableKey;

		public Option(): this(string.Empty, new MultiText()) {}

		public Option(string humanReadableKey, MultiText name) //, Guid guid)
		{
			_humanReadableKey = humanReadableKey;
			Name = name;
			//SearchKeys = new List<string>();
		}

		#region IChoice Members

		public string Label => Name.GetFirstAlternative();

		#endregion

		[XmlElement("key")]
		public string Key
		{
			get
			{
				if (String.IsNullOrEmpty(_humanReadableKey))
					return GetDefaultKey(); //don't actually save it yet

				return _humanReadableKey;
			}

			set
			{
				if (String.IsNullOrEmpty(value))
				{
					_humanReadableKey = GetDefaultKey();
				}

				// The idea here is, we're delaying setting the key in concrete for as long as
				// possible to allow the UI to continue to auto-create the key during a UI session.
				else if (value != GetDefaultKey())
				{
					_humanReadableKey = value.Trim();
				}
			}
		}

		//        [ReflectorProperty("name", typeof (MultiTextSerializorFactory),
		//            Required = true)]
		[XmlElement("name")]
		public MultiText Name { get; set; }

		//        [ReflectorProperty("abbreviation", typeof (MultiTextSerializorFactory),
		//            Required = false)]
		[XmlElement("abbreviation")]
		public MultiText Abbreviation
		{
			get => _abbreviation ?? Name;
			set => _abbreviation = value;
		}

		//
		//        [ReflectorProperty("description", typeof (MultiTextSerializorFactory),
		//            Required = false)]
		[XmlElement("description")]
		public MultiText Description
		{
			get
			{
				if (_description == null)
					_description = new MultiText();

				return _description;
			}
			set => _description = value;
		}

		[XmlElement("searchKeys")]
		public MultiText SearchKeys { get; set; }


		private string GetDefaultKey()
		{
			string name = Name.GetFirstAlternative();
			if (!String.IsNullOrEmpty(name))
			{
				return name;
			}
			return Guid.NewGuid().ToString();
		}

		//        [ReflectorProperty("guid", Required = false)]
		//        public Guid Guid
		//        {
		//            get
		//            {
		//                if (_guid == null || _guid == Guid.Empty)
		//                {
		//                    return Guid.NewGuid();
		//                }
		//                return _guid;
		//            }
		//            set { _guid = value; }
		//        }

		public override string ToString()
		{
			return Name.GetFirstAlternative();
		}

		public object GetDisplayProxy(string writingSystemId)
		{
			return new OptionDisplayProxy(this, writingSystemId);
		}

		#region Nested type: OptionDisplayProxy

		/// <summary>
		/// Gives a monolingual representation of the object for use by a combo-box
		/// </summary>
		public class OptionDisplayProxy
		{
			private readonly string _writingSystemId;
			private Option _option;

			public OptionDisplayProxy(Option option, string writingSystemId)
			{
				_writingSystemId = writingSystemId;
				_option = option;
			}

			public string Key
			{
				get { return _option.Key; }
			}

			public Option UnderlyingOption
			{
				get { return _option; }
				set { _option = value; }
			}

			public override string ToString()
			{
				return _option.Name.GetBestAlternative(_writingSystemId, "*");
			}
		}

		#endregion

		public IList<string> GetSearchKeys(string writingSystemId)
		{
			var keys = SearchKeys;
			if (keys == null)
				return new List<string>();
			var alt = SearchKeys.GetExactAlternative(writingSystemId);
			if (alt == null)
				return new List<string>();

			return alt.SplitTrimmed(',');
		}
	}
}