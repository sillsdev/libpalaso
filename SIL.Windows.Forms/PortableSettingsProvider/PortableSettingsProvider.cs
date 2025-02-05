// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2025 SIL Global
// <copyright from='2010' to='2024' company='SIL Global'>
//		Copyright (c) 2025 SIL Global
//
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright>
#endregion
//
// This class (as ported) originated in FieldWorks (under the GNU Lesser General Public License),
// but we decided to make it available in SIL.Windows.Forms to make it more readily available to
// other projects.
//
// File: PortableSettingsProvider.cs
// Original author: D. Olson
//
// <remarks>
// This class is based on a class by the same name found at
// https://www.codeproject.com/KB/vb/CustomSettingsProvider.aspx. The original was written in
// VB.Net so this is a C# port of that. Other changes include some variable name changes,
// making the settings file path a public static property, special handling of string
// collections, getting rid of the IsRoaming support and all-around method rewriting.
// The original code is under the CPOL license (https://www.codeproject.com/info/cpol10.aspx).
// </remarks>
// ---------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using SIL.Xml;

namespace SIL.Windows.Forms.PortableSettingsProvider
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class is a settings provider that allows applications to specify in what file and
	/// where in a file system it's settings will be stored. It's good for portable apps.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class PortableSettingsProvider : SettingsProvider
	{
		// XML Root Node name.
		private const string Root = "Settings";

		protected XmlDocument m_settingsXml;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Path to the settings file folder (does not include file name.) This can be
		/// specified when the default is not desired. It also allows tests to specify a
		/// temp. location which can be deleted on test cleanup.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string SettingsFileFolder { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Settings file name (not including path). This can be specified when the default is
		/// not desired (which is the application's name). It also allows tests to specify a
		/// temp. location which can be deleted on test cleanup.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string SettingsFileName { get; set; }

		/// ------------------------------------------------------------------------------------
		public PortableSettingsProvider()
		{
			var tmpFolder = (SettingsFileFolder ?? string.Empty);

			if (tmpFolder.Trim() != string.Empty)
				return;

			var appFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			appFolder = Path.Combine(appFolder, ApplicationName);
			if (!Directory.Exists(appFolder))
				Directory.CreateDirectory(appFolder);

			SettingsFileFolder = appFolder;
		}

		/// ------------------------------------------------------------------------------------
		public override void Initialize(string name, NameValueCollection nvc)
		{
			base.Initialize(ApplicationName, nvc);

			m_settingsXml = new XmlDocument();

			try
			{
				m_settingsXml.Load(GetFullSettingsFilePath());
			}
			catch
			{
				XmlDeclaration dec = m_settingsXml.CreateXmlDeclaration("1.0", "utf-8", null);
				m_settingsXml.AppendChild(dec);
				XmlNode nodeRoot = m_settingsXml.CreateNode(XmlNodeType.Element, Root, null);
				m_settingsXml.AppendChild(nodeRoot);
			}
		}

		#region Properties and Property-like methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the name of the currently running application.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override string ApplicationName
		{
			get
			{
				return (Application.ProductName.Trim().Length > 0 ? Application.ProductName :
					Path.GetFileNameWithoutExtension(Application.ExecutablePath));
			}
			set { }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the full path to the application's settings file (includes the path and file
		/// name).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string GetFullSettingsFilePath()
		{
			return Path.Combine(GetSettingsFileFolder(), GetSettingsFileName());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the path to the application settings file, not including the file name.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public virtual string GetSettingsFileFolder()
		{
			return SettingsFileFolder;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the name of the application settings file, not including the path.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public virtual string GetSettingsFileName()
		{
			var tmpFileName = (SettingsFileName ?? string.Empty);

			return (tmpFileName.Trim() == string.Empty ?
				ApplicationName + ".settings" : SettingsFileName);
		}

		#endregion

		#region Methods for getting property values from XML.
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a collection of property values.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override SettingsPropertyValueCollection GetPropertyValues(
			SettingsContext context, SettingsPropertyCollection props)
		{
			SettingsPropertyValueCollection propValues = new SettingsPropertyValueCollection();

			foreach (SettingsProperty setting in props)
				propValues.Add(GetValue(setting));

			return propValues;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets from the XML the value for the specified property.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private SettingsPropertyValue GetValue(SettingsProperty setting)
		{
			var value = new SettingsPropertyValue(setting);
			value.IsDirty = false;
			value.SerializedValue = string.Empty;

			try
			{
				XmlNode node = m_settingsXml.SelectSingleNode(Root + "/" + setting.Name);

				if (!GetStringCollection(value, node))
					if (!GetFormSettings(value, node))
						if (!GetGridSettings(value, node))
							value.SerializedValue = node.InnerText;
			}
			catch
			{
				if ((setting.DefaultValue != null))
					value.SerializedValue = setting.DefaultValue.ToString();
			}

			return value;
		}

		/// ------------------------------------------------------------------------------------
		private bool GetStringCollection(SettingsPropertyValue value, XmlNode node)
		{
			if (node.Attributes.GetNamedItem("type") != null &&
				node.Attributes["type"].Value == typeof(StringCollection).FullName)
			{
				var sc = new StringCollection();
				foreach (XmlNode childNode in node.ChildNodes)
					sc.Add(childNode.InnerText);

				value.PropertyValue = sc;
				return true;
			}

			return false;
		}

		/// ------------------------------------------------------------------------------------
		private bool GetFormSettings(SettingsPropertyValue value, XmlNode node)
		{
			if (node.Attributes.GetNamedItem("type") != null &&
				node.Attributes["type"].Value == typeof(FormSettings).FullName)
			{
				value.PropertyValue =
					XmlSerializationHelper.DeserializeFromString<FormSettings>(node.InnerXml) ?? new FormSettings();

				return true;
			}

			return false;
		}

		/// ------------------------------------------------------------------------------------
		private bool GetGridSettings(SettingsPropertyValue value, XmlNode node)
		{
			if (node.Attributes.GetNamedItem("type") != null &&
				node.Attributes["type"].Value == typeof(GridSettings).FullName)
			{
				value.PropertyValue =
					XmlSerializationHelper.DeserializeFromString<GridSettings>(node.InnerXml) ?? new GridSettings();

				return true;
			}

			return false;
		}

		#endregion

		#region Methods for saving property values to XML.
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the values for the specified properties and saves the XML file in which
		/// they're stored.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override void SetPropertyValues(SettingsContext context,
			SettingsPropertyValueCollection propvals)
		{
			// Iterate through the settings to be stored. Only dirty settings are included
			// in propvals, and only ones relevant to this provider
			foreach (SettingsPropertyValue propVal in propvals)
			{
				if (propVal.Property.Attributes.ContainsKey(typeof(ApplicationScopedSettingAttribute)))
					continue;

				XmlElement settingNode = null;

				try
				{
					settingNode = (XmlElement)m_settingsXml.SelectSingleNode(Root + "/" + propVal.Name);
				}
				catch { }

				// Check if node exists.
				if ((settingNode != null))
					SetPropNodeValue(settingNode, propVal);
				else
				{
					// Node does not exist so create one.
					settingNode = m_settingsXml.CreateElement(propVal.Name);
					SetPropNodeValue(settingNode, propVal);
					m_settingsXml.SelectSingleNode(Root).AppendChild(settingNode);
				}
			}

			try
			{
				// Loading the XML into a new document will make all the indentation correct.
				var tmpXmlDoc = new XmlDocument();
				tmpXmlDoc.LoadXml(m_settingsXml.OuterXml);
				tmpXmlDoc.Save(GetFullSettingsFilePath());
			}
			catch { }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the value of a node to that found in the specified property. This method
		/// specially handles various types of properties (e.g. string collections).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void SetPropNodeValue(XmlNode propNode, SettingsPropertyValue propVal)
		{
			if (propVal.Property.PropertyType == typeof(StringCollection))
				SetStringCollection(propVal, propNode);
			else if (propVal.Property.PropertyType == typeof(FormSettings))
				SetFormSettings(propVal, propNode);
			else if (propVal.Property.PropertyType == typeof(GridSettings))
				SetGridSettings(propVal, propNode);
			else
			{
				if (propVal.SerializedValue != null)
					propNode.InnerText = propVal.SerializedValue.ToString();
				else if (propNode.ParentNode != null)
					propNode.ParentNode.RemoveChild(propNode);
			}
		}

		/// ------------------------------------------------------------------------------------
		private void SetStringCollection(SettingsPropertyValue propVal, XmlNode propNode)
		{
			if (propVal.PropertyValue == null)
				return;

			propNode.RemoveAll();
			var attrib = m_settingsXml.CreateAttribute("type");
			attrib.Value = typeof(StringCollection).FullName;
			propNode.Attributes.Append(attrib);
			int i = 0;
			foreach (string str in propVal.PropertyValue as StringCollection)
			{
				var node = m_settingsXml.CreateElement(propVal.Name + i++);
				node.AppendChild(m_settingsXml.CreateTextNode(str));
				propNode.AppendChild(node);
			}
		}

		/// ------------------------------------------------------------------------------------
		private void SetFormSettings(SettingsPropertyValue propVal, XmlNode propNode)
		{
			var formSettings = propVal.PropertyValue as FormSettings;
			if (formSettings == null)
				return;

			propNode.RemoveAll();
			var attrib = m_settingsXml.CreateAttribute("type");
			attrib.Value = typeof(FormSettings).FullName;
			propNode.Attributes.Append(attrib);

			propNode.InnerXml =
				(XmlSerializationHelper.SerializeToString(formSettings, true) ?? string.Empty);
		}

		/// ------------------------------------------------------------------------------------
		private void SetGridSettings(SettingsPropertyValue propVal, XmlNode propNode)
		{
			var gridSettings = propVal.PropertyValue as GridSettings;
			if (gridSettings == null)
				return;

			propNode.RemoveAll();
			var attrib = m_settingsXml.CreateAttribute("type");
			attrib.Value = typeof(GridSettings).FullName;
			propNode.Attributes.Append(attrib);

			propNode.InnerXml =
				(XmlSerializationHelper.SerializeToString(gridSettings, true) ?? string.Empty);
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a comma-delimited string from an array of integers.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static string GetStringFromIntArray(int[] array)
		{
			if (array == null)
				return string.Empty;

			StringBuilder bldr = new StringBuilder();
			foreach (int i in array)
				bldr.AppendFormat("{0}, ", i);

			return bldr.ToString().TrimEnd(',', ' ');
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets an int array from a comma-delimited string of numbers.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static int[] GetIntArrayFromString(string str)
		{
			List<int> array = new List<int>();

			if (str != null)
			{
				string[] pieces = str.Split(',');
				foreach (string piece in pieces)
				{
					int i;
					if (int.TryParse(piece, out i))
						array.Add(i);
				}
			}

			return array.ToArray();
		}
	}
}
