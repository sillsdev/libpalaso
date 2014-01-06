
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using SIL.Archiving.Properties;

namespace SIL.Archiving.Generic.AccessProtocol
{
	/// <summary>This class is used to deserialize the JSON data in %ProgramData%\SIL\Archiving\AccessProtocols.json</summary>
	[CollectionDataContract(ItemName = "AccessProtocol")]
	public class AccessProtocols : List<ArchiveAccessProtocol>
	{
		private static AccessProtocols _instance;
		private static AccessProtocols _customInstance;
		private const string kProtocolFileName = "AccessProtocols.json";
		private const string kCustomProtocolFileName = "CustomAccessProtocols.json";

		/// <summary />
		public AccessProtocols() { }

		/// <summary />
		public AccessProtocols(IEnumerable<ArchiveAccessProtocol> items)
		{
			foreach (var item in items)
				Add(item);
		}

		/// <summary />
		public static AccessProtocols Load()
		{
			return _instance ?? (_instance = LoadFromFile(kProtocolFileName, Resources.AccessProtocols));
		}

		/// <summary />
		public static AccessProtocols LoadCustom()
		{
			return _customInstance ??
				   (_customInstance = LoadFromFile(kCustomProtocolFileName, Resources.CustomAccessProtocols));
		}

		private static AccessProtocols LoadFromFile(string protocolFileName, string resourceName)
		{
			var dataDirectory = ArchivingFileSystem.SilCommonArchivingDataFolder;

			if (!Directory.Exists(dataDirectory))
				throw new DirectoryNotFoundException(dataDirectory);

			var fileName = Path.Combine(dataDirectory, protocolFileName);

			if (!File.Exists(fileName))
			{
				var jsonData = resourceName;
				File.WriteAllText(fileName, jsonData);
			}

			using (FileStream stream = new FileStream(fileName, FileMode.Open))
			{
				DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof (AccessProtocols));
				return (AccessProtocols) ser.ReadObject(stream);
			}
		}

		/// <summary />
		public static AccessProtocols LoadStandardAndCustom()
		{
			if (_instance == null)
				Load();

			if (_customInstance == null)
				LoadCustom();

			AccessProtocols all = new AccessProtocols();

			if (_instance != null)
				all.AddRange(_instance);

			if (_customInstance != null)
				all.AddRange(_customInstance);

			return all;
		}

		/// <summary />
		public static void SaveCustom(AccessProtocols customProtocols)
		{
			var dataDirectory = ArchivingFileSystem.SilCommonArchivingDataFolder;

			if (!Directory.Exists(dataDirectory))
				throw new DirectoryNotFoundException(dataDirectory);

			var fileName = Path.Combine(dataDirectory, kCustomProtocolFileName);

			using (FileStream stream = new FileStream(fileName, FileMode.Create))
			{
				DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AccessProtocols));
				ser.WriteObject(stream, customProtocols);
			}
		}
	}

	/// <summary>Information for each access protocol in the JSON file</summary>
	[DataContract]
	public class ArchiveAccessProtocol
	{
		/// <summary />
		[DataMember(Name = "protocol", EmitDefaultValue = true, Order = 1)]
		public string ProtocolName = string.Empty;

		/// <summary />
		[DataMember(Name = "documentation", EmitDefaultValue = true, Order = 2)]
		public string DocumentationFile = string.Empty;

		/// <summary />
		[DataMember(Name = "choices", EmitDefaultValue = false, Order = 3)]
		public List<AccessOption> Choices;

		public override string ToString()
		{
			return ProtocolName;
		}

		/// <summary />
		public string ChoicesToCsv()
		{
			StringBuilder sb = new StringBuilder();

			foreach (var option in Choices)
				sb.AppendDelimiter(option.OptionName, ",");

			return sb.ToString();
		}

		/// <summary />
		public void SetChoicesFromCsv(string choicesCsv)
		{
			var choices = choicesCsv.Split(',');
			Choices = new List<AccessOption>();

			foreach (var choice in choices)
				Choices.Add(new AccessOption { OptionName = choice.Trim() });
		}

		/// <summary />
		public string GetDocumentaionUri()
		{
			if (string.IsNullOrEmpty(DocumentationFile))
				return string.Empty;

			var dataDirectory = ArchivingFileSystem.SilCommonArchivingDataFolder;

			if (!Directory.Exists(dataDirectory))
				throw new DirectoryNotFoundException(dataDirectory);

			var fileName = Path.Combine(dataDirectory, DocumentationFile);

			// try to create the file if it is not there
			if (!File.Exists(fileName))
			{
				var pos = DocumentationFile.LastIndexOf('.');
				var resourceName = (pos > -1) ? DocumentationFile.Substring(0, pos) : DocumentationFile;
				var resourceString = Resources.ResourceManager.GetString(resourceName);
				if (!string.IsNullOrEmpty(resourceString))
					File.WriteAllText(fileName, resourceString);
			}

			if (!File.Exists(fileName))
				throw new FileNotFoundException(fileName);

			return new Uri(fileName, UriKind.Absolute).AbsoluteUri;
		}
	}

	/// <summary>Information for each choice belonging to an AccessProtocol</summary>
	[DataContract]
	public class AccessOption
	{
		/// <summary />
		[DataMember(Name = "label", EmitDefaultValue = true, Order = 1)]
		public string OptionName = string.Empty;

		/// <summary />
		[DataMember(Name = "description", EmitDefaultValue = true, Order = 2)]
		public string Description = string.Empty;

		public override string ToString()
		{
			return OptionName;
		}
	}
}
