using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using SIL.Annotations;

namespace SIL.Lift.Options
{
	/// <summary>
	/// This is like a PossibilityList in FieldWorks, or RangeSet in Toolbox
	/// </summary>
	[XmlRoot("optionsList")]
	public class OptionsList
	{
		private List<Option> _options;

		public OptionsList()
		{
			_options = new List<Option>();
		}

		/// <summary>
		/// just to get the old xml format (which includes a &lt;options&gt; element around the options) read in
		/// </summary>
		[XmlElement("options")]
		public OptionsListWrapper options
		{
			set { _options = value.options; }
			get
			{
				//                OptionsListWrapper w = new OptionsListWrapper();
				//                w._options = _options;
				//                return w;
				return null;
			}
		}

		[XmlElement(typeof (Option), ElementName = "option")]
		public List<Option> Options
		{
			get { return _options; }
			set { _options = value; }
		}



		public void SaveToFile(string path)
		{
			XmlSerializer serializer = new XmlSerializer(typeof (OptionsList));
			XmlAttributeOverrides overrides = new XmlAttributeOverrides();
			XmlAttributes ignoreAttr = new XmlAttributes();
			ignoreAttr.XmlIgnore = true;
			overrides.Add(typeof (Annotatable), "IsStarred", ignoreAttr);

			using (StreamWriter writer = File.CreateText(path))
			{
				serializer.Serialize(writer, this);
				writer.Close();
			}
		}

		public Option GetOptionFromKey(string value)
		{
			foreach (Option option in Options)
			{
				if (option.Key == value)
				{
					return option;
				}
			}
			return null;
		}
	}

	/// <summary>
	/// Just makes the xml serialization work right
	/// </summary>
	public class OptionsListWrapper
	{
		[XmlElement(typeof (Option), ElementName = "option")]
		public List<Option> options;
	}
}