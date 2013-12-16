
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using SIL.Archiving.Properties;

namespace SIL.Archiving.Generic.AccessProtocol
{
	/// <summary>This class is used to deserialize the JSON data in %ProgramData%\SIL\Archiving\AccessProtocols.json</summary>
	[CollectionDataContract(ItemName = "AccessProtocol")]
	public class AccessProtocols : List<ArchiveAccessProtocol>
	{
		private static AccessProtocols _instance;

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
			if (_instance != null)
				return _instance;

			var dataDirectory = ArchivingFileSystem.SilCommonArchivingDataFolder;

			if (!Directory.Exists(dataDirectory))
				throw new DirectoryNotFoundException(dataDirectory);

			var fileName = Path.Combine(dataDirectory, "AccessProtocols.json");

			if (!File.Exists(fileName))
			{
				var jsonData = Resources.AccessProtocols;
				File.WriteAllText(fileName, jsonData);
			}

			using (FileStream stream = new FileStream(fileName, FileMode.Open))
			{
				DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(AccessProtocols));
				_instance = (AccessProtocols) ser.ReadObject(stream);
			}

			return _instance;
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
