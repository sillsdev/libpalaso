using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using JetBrains.Annotations;
using static SIL.Archiving.Resources.Resources;

namespace SIL.Archiving.Generic.AccessProtocol
{
	/// <summary>This class is used to deserialize the JSON data in %ProgramData%\SIL\Archiving\AccessProtocols.json</summary>
	[CollectionDataContract(ItemName = "AccessProtocol")]
	public class AccessProtocols : List<ArchiveAccessProtocol>
	{
		private static AccessProtocols _instance;
		private static AccessProtocols _customInstance;
		public const string kProtocolFileName = "AccessProtocols.json";
		private const string kCustomProtocolFileName = "CustomAccessProtocols.json";

		/// <summary />
		protected AccessProtocols() { }

		/// <summary />
		protected AccessProtocols(IEnumerable<ArchiveAccessProtocol> items)
		{
			foreach (var item in items)
				Add(item);
		}

		/// <summary />
		public static AccessProtocols Load() { return Load(null); }

		/// <summary />
		public static AccessProtocols Load(string programDirectory) =>
			_instance ??= LoadFromFile(kProtocolFileName, GetResource(Name.AccessProtocols_json), programDirectory);

		/// <summary />
		public static AccessProtocols LoadCustom()
		{
			return _customInstance ??=
				LoadFromFile(kCustomProtocolFileName, GetResource(Name.CustomAccessProtocols_json), null);
		}

		private static AccessProtocols LoadFromFile(string protocolFileName, string resourceName, string programDirectory)
		{
			var useDefault = string.IsNullOrEmpty(programDirectory);

			var dataDirectory = useDefault ? ArchivingFileSystem.SilCommonArchivingDataFolder : programDirectory;

			if (Path.GetFileName(dataDirectory) != ArchivingFileSystem.kAccessProtocolFolderName)
			{
				var parentFolder = Path.GetDirectoryName(dataDirectory);
				if (parentFolder != null && Path.GetFileName(parentFolder) == ArchivingFileSystem.kAccessProtocolFolderName)
					dataDirectory = parentFolder;
			}

			if (!Directory.Exists(dataDirectory))
				throw new DirectoryNotFoundException(dataDirectory);

			var fileName = Path.Combine(dataDirectory, protocolFileName);

			// if not in the programDirectory, look in the archiving folder
			if (!File.Exists(fileName) && !useDefault)
			{
				dataDirectory = ArchivingFileSystem.SilCommonArchivingDataFolder;
				fileName = Path.Combine(dataDirectory, protocolFileName);
			}

			if (!File.Exists(fileName))
			{
				// create file if not found yet
				var jsonData = resourceName;
				File.WriteAllText(fileName, jsonData);
			}

			using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof (AccessProtocols));
				return (AccessProtocols) ser.ReadObject(stream);
			}
		}

		/// <summary />
		[PublicAPI]
		public static AccessProtocols LoadStandardAndCustom() { return LoadStandardAndCustom(null); }

		/// <summary />
		public static AccessProtocols LoadStandardAndCustom(string programDirectory)
		{
			if (_instance == null)
				Load(programDirectory);

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
		[PublicAPI]
		public static void SaveCustom(AccessProtocols customProtocols) { SaveCustom(customProtocols, null); }

		/// <summary />
		public static void SaveCustom(AccessProtocols customProtocols, string programDirectory)
		{
			var dataDirectory = string.IsNullOrEmpty(programDirectory) ? ArchivingFileSystem.SilCommonArchivingDataFolder : programDirectory;

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
		[PublicAPI]
		public string ChoicesToCsv()
		{
			StringBuilder sb = new StringBuilder();

			foreach (var option in Choices)
				sb.AppendDelimiter(option.OptionName, ",");

			return sb.ToString();
		}

		/// <summary />
		[PublicAPI]
		public void SetChoicesFromCsv(string choicesCsv)
		{
			if (choicesCsv == null)
				throw new ArgumentNullException(nameof(choicesCsv));

			Choices = choicesCsv.Split(',').Select(c => c.Trim()).Distinct()
				.Select(n => new AccessOption{ OptionName = n}).ToList();
		}

		/// <summary>
		/// Gets the path to the documentation for this access protocol 
		/// </summary>
		[PublicAPI]
		public string GetDocumentationUri() => GetDocumentationUri(null);

		/// <summary>
		/// Gets the path to the documentation for this access protocol 
		/// </summary>
		public string GetDocumentationUri(string programDirectory)
		{
			if (string.IsNullOrEmpty(DocumentationFile))
				return string.Empty;

			var useDefault = string.IsNullOrEmpty(programDirectory);
			var dataDirectory = string.IsNullOrEmpty(programDirectory) ? ArchivingFileSystem.SilCommonArchivingDataFolder : programDirectory;

			if (!Directory.Exists(dataDirectory))
				throw new DirectoryNotFoundException(dataDirectory);

			var fileName = Path.Combine(dataDirectory, DocumentationFile);

			// if not in the programDirectory, look in the archiving folder
			if (!File.Exists(fileName) && !useDefault)
			{
				dataDirectory = ArchivingFileSystem.SilCommonArchivingDataFolder;
				fileName = Path.Combine(dataDirectory, DocumentationFile);
			}

			// try to create the file if it is not there
			if (!File.Exists(fileName))
			{
				var pos = DocumentationFile.LastIndexOf('.');
				var resourceName = pos > -1 ? DocumentationFile.Substring(0, pos) : DocumentationFile;
				var resourceString = GetResource(resourceName);
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

		/// <summary />
		[PublicAPI]
		public string ValueMember => OptionName;

		/// <summary />
		[PublicAPI]
		public string DisplayMember => string.IsNullOrEmpty(Description) ? OptionName : Description;
	}
}
