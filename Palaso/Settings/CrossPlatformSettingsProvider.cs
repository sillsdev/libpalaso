using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Palaso.Settings
{
	/// <summary>
	/// A custom SettingsProvider implementation functional on both Windows and Linux (the default mono implementation was buggy and incomplete)
	/// </summary>
	public class CrossPlatformSettingsProvider : SettingsProvider, IApplicationSettingsProvider
	{
		private static readonly object LockObject = new Object();

		protected string UserRoamingLocation = null;
		protected string UserLocalLocation = null;
		private static string CompanyAndProductPath;
		/// <summary>
		/// Indicates if the settings should be saved in roaming or local location, defaulted to false;
		/// </summary>
		public bool IsRoaming = false;

		private string UserConfigLocation { get { return IsRoaming ? UserRoamingLocation : UserLocalLocation; } }

		/// <summary>
		/// Default constructor for this provider class
		/// </summary>
		public CrossPlatformSettingsProvider()
		{
			UserRoamingLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GetFullSettingsPath());
			UserLocalLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), GetFullSettingsPath());
		}

		private static string GetFullSettingsPath()
		{
			var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
			var basePath = GetCompanyAndProductPath(assembly);
			CompanyAndProductPath = basePath;
			return Path.Combine(basePath, assembly.GetName().Version.ToString());
		}

		private static string GetCompanyAndProductPath(Assembly assembly)
		{
			var companyAttributes =
				(AssemblyCompanyAttribute[])assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
			var companyName = "NoCompanyNameInAssemblyInfo";
			var productName = "NoProductNameInAssemblyInfo";
			if(companyAttributes.Length > 0)
			{
				companyName = companyAttributes[0].Company;
			}
			var productAttributes =
				(AssemblyProductAttribute[])assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
			if(productAttributes.Length > 0)
			{
				productName = productAttributes[0].Product;
			}
			return Path.Combine(companyName, productName);
		}

		public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
		{
			lock(LockObject)
			{
				//Create new collection of values
				var values = new SettingsPropertyValueCollection();

				//Iterate through the settings to be retrieved
				foreach(SettingsProperty setting in collection)
				{
					var value = new SettingsPropertyValue(setting);
					value.IsDirty = false;
					value.SerializedValue = GetValue(SettingsXml, context["GroupName"].ToString(), setting);
					values.Add(value);
				}
				return values;
			}
		}

		public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
		{
			lock(LockObject)
			{
				//Iterate through the settings to be stored, only dirty settings for this provider are in collection
				foreach(SettingsPropertyValue propval in collection)
				{
					var groupName = context["GroupName"].ToString();
					var groupNode = SettingsXml.SelectSingleNode("/configuration/userSettings/" + context["GroupName"]);
					if(groupNode == null)
					{
						var parentNode = SettingsXml.SelectSingleNode("/configuration/userSettings");
						groupNode = SettingsXml.CreateElement(groupName);
						parentNode.AppendChild(groupNode);
					}
					var section = (XmlElement)SettingsXml.SelectSingleNode("/configuration/configSections/sectionGroup/section");
					if(section == null)
					{
						var parentNode = SettingsXml.SelectSingleNode("/configuration/configSections/sectionGroup");
						section = SettingsXml.CreateElement("section");
						section.SetAttribute("name", groupName);
						section.SetAttribute("type", String.Format("{0}, {1}", typeof(ClientSettingsSection), Assembly.GetAssembly(typeof(ClientSettingsSection))));
						parentNode.AppendChild(section);
					}
					SetValue(groupNode, propval);
				}
				Directory.CreateDirectory(UserConfigLocation);
				SettingsXml.Save(Path.Combine(UserConfigLocation, "user.config"));
			}
		}

		private void SetValue(XmlNode groupNode, SettingsPropertyValue propVal)
		{
			XmlElement settingNode;

			try
			{
				settingNode = (XmlElement)groupNode.SelectSingleNode("setting[@name='" + propVal.Name + "']");
			}
			catch(Exception ex)
			{
				settingNode = null;
			}

			//Check to see if the node exists, if so then set its new value
			if(settingNode != null)
			{
				var valueNode = (XmlElement)settingNode.SelectSingleNode("value");
				SetValueNodeContentsFromProp(propVal, valueNode);
			}
			else
			{
				settingNode = SettingsXml.CreateElement("setting");
				settingNode.SetAttribute("name", propVal.Name);
				settingNode.SetAttribute("serializeAs", propVal.Property.SerializeAs.ToString());
				var valueNode = SettingsXml.CreateElement("value");
				SetValueNodeContentsFromProp(propVal, valueNode);
				settingNode.AppendChild(valueNode);
				groupNode.AppendChild(settingNode);
			}
		}

		private void SetValueNodeContentsFromProp(SettingsPropertyValue propVal, XmlElement valueNode)
		{
			if(valueNode == null)
			{
				throw new ArgumentNullException("valueNode");
			}
			if(propVal.Property.SerializeAs == SettingsSerializeAs.String)
			{
				valueNode.InnerText = propVal.SerializedValue.ToString();
			}
			else if(propVal.Property.SerializeAs == SettingsSerializeAs.Xml)
			{
				if(!String.IsNullOrEmpty(propVal.SerializedValue as String))
				{
					var subDoc = new XmlDocument();
					subDoc.LoadXml((string)propVal.SerializedValue);
					if(valueNode.FirstChild != null)
					{
						valueNode.RemoveChild(valueNode.FirstChild);
					}
					valueNode.AppendChild(SettingsXml.ImportNode(subDoc.DocumentElement, true));
				}
			}
			else
			{
				throw new NotImplementedException("CrossPlatformSettingsProvider does not yet handle settings of "+
															 propVal.Property.SerializeAs);
			}
		}

		public override string Name
		{
			get
			{
				return @"CrossPlatformLocalFileSettingsProvider";
			}
		}


		public override string ApplicationName
		{
			get
			{
				var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
				var assemblyAttributes = (AssemblyProductAttribute[])assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
				var appName = @"Product not set in AssemblyInfo";
				if(assemblyAttributes.Length > 0)
				{
					appName = assemblyAttributes[0].Product;
				}
				return appName;
			}
			set { } //Do nothing
		}

		public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
		{
			var document = GetPreviousSettingsXml();
			if(document == null)
			{
				return null;
			}
			var value = new SettingsPropertyValue(property);
			value.IsDirty = false;
			value.SerializedValue = GetValue(document, context["GroupName"].ToString(), property);
			return value;
		}

		/// <summary>
		/// Return the previous version settings document if there is one, otherwise null.
		/// </summary>
		/// <returns></returns>
		private static XmlDocument GetPreviousSettingsXml()
		{
			XmlDocument document = null;
			var appSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
														  CompanyAndProductPath);
			if(Directory.Exists(appSettingsPath))
			{
				var directoryList =
				new List<string>(
						Directory.EnumerateDirectories(appSettingsPath));
				if(directoryList.Count > 0)
				{
					var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
					directoryList.Sort(VersionDirectoryComparison);
					var previousDirectory = directoryList[0];
					if(!previousDirectory.EndsWith(assembly.GetName().Version.ToString()) && directoryList.Count <= 1)
					{
						return null;
					}
					if(previousDirectory.EndsWith(assembly.GetName().Version.ToString()))
					{
						previousDirectory = directoryList[1];
					}
					var settingsLocation = Path.Combine(previousDirectory, "user.config");
					if(File.Exists(settingsLocation))
					{
						document = new XmlDocument();
						try
						{
							document.Load(settingsLocation);
						}
						catch(Exception)
						{
							//Don't blow up if we can't load old settings, just lose them.
							document = null;
							Console.WriteLine(@"Failed to load old settings file.");
						}
					}
				}
			}
			return document;
		}

		/// <summary>
		/// Returns the directories in order of decreasing versions with non version named directories at the end.
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		private static int VersionDirectoryComparison(string first, string second)
		{
			Version firstVersion = null;
			Version secondVersion = null;
			Version.TryParse(first.Substring(first.LastIndexOf(Path.DirectorySeparatorChar) + 1), out firstVersion);
			Version.TryParse(second.Substring(second.LastIndexOf(Path.DirectorySeparatorChar) + 1), out secondVersion);
			if(firstVersion != null && secondVersion != null)
			{
				return secondVersion.CompareTo(firstVersion);
			}
			//Some user may be messing with us, but one of these doesn't have settings.
			if(firstVersion != null)
			{
				return -1;
			}
			if(secondVersion != null)
			{
				return 1;
			}
			return String.Compare(first, second, StringComparison.Ordinal);
		}

		public void Reset(SettingsContext context)
		{
			lock(LockObject)
			{
				_settingsXml = null;
				File.Delete(Path.Combine(UserConfigLocation, "user.config"));
			}
		}

		public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
		{
			lock(LockObject)
			{
				var oldDoc = GetPreviousSettingsXml();
				if(oldDoc != null)
				{
					Directory.CreateDirectory(UserConfigLocation);
					oldDoc.Save(Path.Combine(UserConfigLocation, "user.config"));
					_settingsXml = oldDoc;
				}
			}
		}

		private object GetValue(XmlDocument settingsXml, String groupName, SettingsProperty setting)
		{
			var settingValue = setting.DefaultValue;
			var valueNode = settingsXml.SelectSingleNode("configuration/userSettings/" + (String.IsNullOrEmpty(groupName) ? "/" : groupName) + "/setting[@name='" + setting.Name + "']");

			if(valueNode != null)
			{
				if(setting.SerializeAs == SettingsSerializeAs.String)
				{
					settingValue = valueNode.InnerText;
				}
				else if(setting.SerializeAs == SettingsSerializeAs.Xml)
				{
					settingValue = valueNode.FirstChild.InnerXml;
				}
			}

			return settingValue;
		}

		private XmlDocument _settingsXml;

		private XmlDocument SettingsXml
		{
			get
			{
				lock(LockObject)
				{
					if(_settingsXml == null)
					{
						_settingsXml = new XmlDocument();
						if(File.Exists(Path.Combine(UserConfigLocation, "user.config")))
						{
							_settingsXml.Load(Path.Combine(UserConfigLocation, "user.config"));
						}
						else
						{
							//Create new document
							var dec = _settingsXml.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
							_settingsXml.AppendChild(dec);

							var nodeRoot = _settingsXml.CreateNode(XmlNodeType.Element, "configuration", "");
							var configSections = _settingsXml.CreateNode(XmlNodeType.Element, "configSections", "");
							var sectionGroup = _settingsXml.CreateElement("sectionGroup");
							sectionGroup.SetAttribute("name", "userSettings");
							var userSettingsAssembly = Assembly.GetAssembly(typeof(UserSettingsGroup));
							var typeValue = String.Format("{0}, {1}", typeof(UserSettingsGroup), userSettingsAssembly);
							sectionGroup.SetAttribute("type", typeValue);
							configSections.AppendChild(sectionGroup);
							var userSettings =  _settingsXml.CreateNode(XmlNodeType.Element, "userSettings", "");
							nodeRoot.AppendChild(configSections);
							nodeRoot.AppendChild(userSettings);
							_settingsXml.AppendChild(nodeRoot);
						}
					}

					return _settingsXml;
				}
			}
		}
	}
}
