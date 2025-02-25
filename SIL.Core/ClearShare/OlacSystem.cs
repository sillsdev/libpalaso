using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using SIL.Code;
using SIL.Reporting;

namespace SIL.Core.ClearShare
{
	///-----------------------------------------------------------------------------------------
	/// <summary>
	/// Serializes/Deserializes a work and all the information ClearShare has about it, as
	/// an OLAC record.
	/// </summary>
	///-----------------------------------------------------------------------------------------
	public class OlacSystem
	{
		private List<Role> _roles;
		private static readonly XNamespace s_nsOlac = "http://www.language-archives.org/OLAC/1.1/";
		private static readonly XNamespace s_nsDc = "http://purl.org/dc/elements/1.1/";

		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		public void LoadWorkFromXml(Work work, string xml)
		{
			//TODO: parse the xml as olac. For a first pass, we can ignore anything we don't understand.
			//Eventually, we'll want to round-trip things we don't understand.

			Guard.AgainstNull(work, "Work");

			var doc = XDocument.Load(XmlReader.Create(new StringReader(xml)));

			foreach (var contributor in doc.Descendants((new XElement(s_nsDc + "contributor")).Name))
			{
				var roleCode = contributor.Attributes().Single(a => a.Name == s_nsOlac + "code").Value;
				if (!TryGetRoleByCode(roleCode, out var role))
					role = GetRoles().ElementAt(0);
				work.Contributions.Add(new Contribution(contributor.Value, role));
			}
		}

		/// ------------------------------------------------------------------------------------
		public string GetOlacRecordElement()
		{
			return $@"<olac:olac xmlns:olac='{s_nsOlac}' xmlns:dc='{s_nsDc}'
				xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
				xsi:schemaLocation='http://www.language-archives.org/OLAC/1.1/
				http://www.language-archives.org/OLAC/1.1/olac.xsd'>";
		}

		/// ------------------------------------------------------------------------------------
		public string GetContributorElement(string roleCode, string contributorName)
		{
			Guard.AgainstNull(roleCode, "roleCode");
			Guard.AgainstNull(contributorName, "contributorName");

			return $"<dc:contributor xsi:type='olac:role' olac:code='{roleCode}' " +
				$"view='Compiler'>{contributorName}</dc:contributor>";
		}

		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		public string GetXmlForWork(Work work)
		{
			var bldr = new StringBuilder();

			if (work != null)
			{
				bldr.Append(GetOlacRecordElement());

				foreach (var contributor in work.Contributions)
					bldr.Append(GetContributorElement(contributor.Role.Code, contributor.ContributorName));

				bldr.Append("</olac:olac>");
			}

			return bldr.ToString();

			/*            <dc:language xsi:type="olac:language" olac:code="adz"
				  view="Adzera"/>

			  <dc:subject xsi:type="olac:language" olac:code="adz"
				  view="Adzera"/>



	  <dc:title>Language</dc:title>
	  <dc:publisher>New York: Holt</dc:publisher>
			 */

		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Get all the roles in the system's controlled vocabulary
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public IEnumerable<Role> GetRoles(string customRolesXmlFilename = null)
		{
			List<Role> ReadRoles(Stream stream)
			{
				List<Role> roles;

				try
				{
					using (var xmlReader = XmlReader.Create(stream))
					{
						var doc = XDocument.Load(xmlReader);

						// This is a bit confusing because the role heading node is at the same level
						// (i.e. a sibling of) as all the associated term nodes. Therefore, the first
						// thing to do is get the heading nodes that are children of section nodes.
						// Then find the one heading node whose content is "Role". Then backup to the
						// section containing that heading node and take all "term" child nodes of
						// that section (i.e. that are siblings of the role heading node).
						roles = doc.Descendants("body").Descendants("section")
							.Elements("heading")
							.Where(n => n.Value == "Role").Ancestors("section").First()
							.Descendants("term")
							.Select(n => new Role(n.Element("code").Value,
								n.Element("name").Value, n.Element("definition").Value)).ToList();
						xmlReader.Close();
					}
				}
				finally
				{
					stream.Close();
					stream.Dispose();
				}

				return roles;
			}

			if (_roles == null)
			{
				if (customRolesXmlFilename != null)
				{
					try
					{
						_roles = ReadRoles(new FileStream(customRolesXmlFilename, FileMode.Open));
					}
					catch (Exception e)
					{
						ErrorReport.ReportNonFatalException(e);
					}
				}

				_roles ??= ReadRoles(Assembly.GetExecutingAssembly()
					.GetManifestResourceStream("SIL.Core.ClearShare.OlacRoles.xml"));
			}

			return _roles;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Used to look up roles in the system's controlled vocabulary
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool TryGetRoleByCode(string code, out Role role)
		{
			role = GetRoles().FirstOrDefault(r => r.Code == code);
			return role != null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Used to look up roles in the system's controlled vocabulary
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		public Role GetRoleByCodeOrThrow(string code)
		{
			var role = GetRoles().FirstOrDefault(r => r.Code == code);

			if (role == null)
			{
				var msg = $"This version of OLAC does not contain a role with code '{code}'.";
				throw new ArgumentOutOfRangeException(msg);
			}

			return role;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Used to look up roles in the system's controlled vocabulary
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[PublicAPI]
		public bool TryGetRoleByName(string name, out Role role)
		{
			role = GetRoles().FirstOrDefault(r => r.Name == name);
			return role != null;
		}
	}
}
