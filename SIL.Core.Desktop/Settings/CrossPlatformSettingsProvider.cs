using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using SIL.IO;
using SIL.Reporting;

namespace SIL.Settings
{
	/// <summary>
	/// A custom SettingsProvider implementation functional on both Windows and Linux (the default mono implementation was buggy and incomplete)
	/// </summary>
	/// <example>
	/// var settingsProvider = new CrossPlatformSettingsProvider();
	/// //optionally pre-check for problems
	/// if(settingsProvider.CheckForErrorsInFile()) ...
	/// </example>
	public class CrossPlatformSettingsProvider : SettingsProvider, IApplicationSettingsProvider
	{
		//Protected only for unit testing. I can't get InternalsVisibleTo to work, possibly because of strong naming.
		protected const string UserConfigFileName = "user.config";
		private static readonly object LockObject = new Object();

		protected string UserRoamingLocation = null;
		protected string UserLocalLocation = null;
		private static string CompanyAndProductPath;

		private bool _reportReadingErrorsDirectlyToUser = true;

		// May be overridden in a derived class to control where settings are looked for
		// when that class is used as the provider. Must do any initialization before calls to GetCompanyAndProductPath,
		// that is, before trying to use instances of the relevant settings.
		protected virtual string ProductName
		{
			get { return null; }
		}

		/// <summary>
		/// Indicates if the settings should be saved in roaming or local location, defaulted to false;
		/// </summary>
		public bool IsRoaming = false;

		/// <summary>
		/// Where we expect to find the config file. Protected for unit testing.
		/// </summary>
		protected string UserConfigLocation
		{
			get { return IsRoaming ? UserRoamingLocation : UserLocalLocation; }
		}

		private readonly Dictionary<string, string> _renamedSections;

		/// <summary>
		/// Default constructor for this provider class
		/// </summary>
		public CrossPlatformSettingsProvider()
		{
			_renamedSections = new Dictionary<string, string>();
			UserRoamingLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				GetFullSettingsPath());
			UserLocalLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				GetFullSettingsPath());
			// When running multiple builds in parallel we have to use separate directories for
			// each build, otherwise some unit tests might fail.
			var buildAgentSubdir = Environment.GetEnvironmentVariable("BUILDAGENT_SUBKEY");
			if (!string.IsNullOrEmpty(buildAgentSubdir))
			{
				UserRoamingLocation = Path.Combine(UserRoamingLocation, buildAgentSubdir);
				UserLocalLocation = Path.Combine(UserLocalLocation, buildAgentSubdir);
			}
		}

		/// <summary>
		/// This Settings Provider will automatically delete a corrupted settings file, silently.
		/// If you want to control when it does that, and get a message describing the problem
		/// so that you can tell the user, call this before anything else touches the settings.
		/// </summary>
		/// <returns>an exception or null</returns>
		public Exception CheckForErrorsInSettingsFile()
		{
			if(!_initialized)
				throw new ApplicationException("CrossPlatformSettingsProvider: Call Initialize() before CheckForErrorsInFile()");

			// If config file read error, assign to _lastReadingError rather than report to user.
			_reportReadingErrorsDirectlyToUser = false;
			// If settings are null, load them from the config file, reporting any error.
			_ = SettingsXml;
			return _lastReadingError;
		}

		public IDictionary<string, string> RenamedSections
		{
			get { return _renamedSections; }
		}

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
// ReSharper disable ConditionIsAlwaysTrueOrFalse
			if(name != null && config != null) //ReSharper lies
// ReSharper restore ConditionIsAlwaysTrueOrFalse
			{
				base.Initialize(name, config);
			}
			_initialized = true;
		}

		private string GetFullSettingsPath()
		{
			var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
			var basePath = GetCompanyAndProductPath(assembly);
			CompanyAndProductPath = basePath;
			return Path.Combine(basePath, assembly.GetName().Version.ToString());
		}

		private string GetCompanyAndProductPath(Assembly assembly)
		{
			var companyAttributes =
				(AssemblyCompanyAttribute[])assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
			var companyName = "NoCompanyNameInAssemblyInfo";
			var productName = "NoProductNameInAssemblyInfo";
			if(companyAttributes.Length > 0)
			{
				companyName = companyAttributes[0].Company;
			}
			if (ProductName != null)
				productName = ProductName;
			else
			{
			var productAttributes =
					(AssemblyProductAttribute[])assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
				if(productAttributes.Length > 0)
				{
					productName = productAttributes[0].Product;
				}
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
				// We need to forget any cached version of the XML. Otherwise, when more than one lot of settings
				// is saved in the same file, the provider that is doing the save for one of them may have stale
				// (or missing) settings for the other. We want to write the dirty properties over a current
				// version of everything else that has been saved in the file.
				_settingsXml = null;

				//Iterate through the settings to be stored, only dirty settings for this provider are in collection
				foreach(SettingsPropertyValue propVal in collection)
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
					SetValue(groupNode, propVal);
				}
				Directory.CreateDirectory(UserConfigLocation);
				RobustIO.SaveXml(SettingsXml, Path.Combine(UserConfigLocation, UserConfigFileName));
			}
		}

		private void SetValue(XmlNode groupNode, SettingsPropertyValue propVal)
		{
			// Paranoid check on data coming from .NET or Mono
			if(propVal == null)
			{
				return;
			}

			XmlElement settingNode;

			try
			{
				settingNode = (XmlElement)groupNode.SelectSingleNode("setting[@name='" + propVal.Name + "']");
			}
			catch(Exception)
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
				// In some cases the serialized value in the propVal can return null.
				// Set the contents of the setting xml to the empty string in that case.
				var serializedValue = propVal.SerializedValue;
				valueNode.InnerText = serializedValue != null ? serializedValue.ToString() : String.Empty;
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

		public override string Description
		{
			get { return "@WorkingLocalFileProviderForWinAndMono"; }
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
					if(previousDirectory.EndsWith(assembly.GetName().Version.ToString()))
					{
						if (directoryList.Count == 1)
						{
							// The only directory is for the current version; no need to upgrade.
							return null;
						}
						previousDirectory = directoryList[1];
					}
					var settingsLocation = Path.Combine(previousDirectory, UserConfigFileName);
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
		/// Returns the directories in order based on finding the most recently modified user.config file (putting that first).
		/// More specifically, a folder with a config file is 'less than' one that has none; if both have config files,
		/// the most recently modified is less than the other.
		/// This allows a new install to inherit settings from whatever pre-existing settings were last modified.
		/// This in turn allows settings to be inherited across channels (like BloomAlpha and BloomSHRP) that may have
		/// different version number sequences, and still a new install will get the most recent settings from any
		/// previous versions. It works even if the version numbers are not in a consistent order.
		/// </summary>
		/// <remarks>Protected only for unit testing. I can't get InternalsVisibleTo to work, possibly because of strong naming.</remarks>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		protected static int VersionDirectoryComparison(string first, string second)
		{
			var firstConfigPath = Path.Combine(first, UserConfigFileName);
			var secondConfigPath = Path.Combine(second, UserConfigFileName);
			if (!File.Exists(firstConfigPath))
			{
				if (File.Exists(secondConfigPath))
					return 1; // second is 'less' (comes first)
				return first.CompareTo(second); // arbitrary since neither is any use, but give a consistent result.
			}
			if (!File.Exists(secondConfigPath))
				return -1; // first is less (comes first).

			// Reversing the arguments like this means that second comes before first if it has a LARGER mod time.
			return new FileInfo(secondConfigPath).LastWriteTimeUtc.CompareTo(new FileInfo(firstConfigPath).LastWriteTimeUtc);
		}

		public void Reset(SettingsContext context)
		{
			lock(LockObject)
			{
				_settingsXml = null;
				File.Delete(Path.Combine(UserConfigLocation, UserConfigFileName));
			}
		}

		public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
		{
			lock (LockObject)
			{
				XmlDocument oldDoc = GetPreviousSettingsXml();
				if (oldDoc != null)
				{
					if (_renamedSections.Count > 0)
					{
						XmlNode userSettingsNode = oldDoc.SelectSingleNode("configuration/userSettings");
						if (userSettingsNode != null)
						{
							foreach (XmlElement sectionNode in userSettingsNode.ChildNodes.OfType<XmlElement>().ToArray())
							{
								string newName;
								if (_renamedSections.TryGetValue(sectionNode.Name, out newName))
								{
									XmlElement newSectionNode = oldDoc.CreateElement(newName);
									foreach (XmlNode child in sectionNode.ChildNodes)
										newSectionNode.AppendChild(child.CloneNode(true));
									userSettingsNode.ReplaceChild(newSectionNode, sectionNode);
								}
							}
						}
					}
					Directory.CreateDirectory(UserConfigLocation);
					oldDoc.Save(Path.Combine(UserConfigLocation, UserConfigFileName));
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
		private bool _initialized;
		private XmlException _lastReadingError;

		/// <summary>
		/// Get xml settings, loading them from the config file if they're null.
		/// </summary>
		/// <remarks>
		/// If there's an error reading the config file, report the error,
		/// make a copy of the corrupt file (with extension `.bad`), delete the file,
		/// and create a new config file.
		/// </remarks>
		private XmlDocument SettingsXml
		{
			get
			{
				lock(LockObject)
				{
					if(_settingsXml == null)
					{
						_settingsXml = new XmlDocument();
						var userConfigFilePath = Path.Combine(UserConfigLocation, UserConfigFileName);
						if(File.Exists(userConfigFilePath))
						{
							try
							{
								_settingsXml.Load(Path.Combine(UserConfigLocation, UserConfigFileName));
								return _settingsXml;
							}
							catch (XmlException e)
							{
								//a partial reading can leave the _settingsXml in a weird state. Start over:
								_settingsXml = new XmlDocument();

								Logger.WriteError("Problem with contents of " + userConfigFilePath, e);

								//This ErrorReport was actually keeping Bloom from starting at all.
								//It now calls CheckForErrorsInFile() which lets it control the messaging;
								//that also sets this to false.

								if(_reportReadingErrorsDirectlyToUser)
									ErrorReport.ReportNonFatalExceptionWithMessage(e, "A settings file was corrupted. Some user settings may have been lost.");
								_lastReadingError = e;

								try
								{
									File.Copy(userConfigFilePath, userConfigFilePath + ".bad", true);
								}
								catch (Exception)
								{
									//not worth dying over
								}
								try
								{
									File.Delete(userConfigFilePath);
								}
								catch (Exception deletionError)
								{
									Logger.WriteError("Could not delete " + userConfigFilePath, deletionError);
									ErrorReport.ReportFatalMessageWithStackTrace("Please delete the configuration file at " + userConfigFilePath + " and then re-run the program.");
									throw deletionError;
								}
							}
						}

						//If there was no file, or if the file was corrupt create a new document
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

					return _settingsXml;
				}
			}
		}
	}
}
