﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SIL.Code;
using SIL.Windows.Forms.Properties;

namespace SIL.Windows.Forms.ClearShare
{
	///-----------------------------------------------------------------------------------------
	/// <summary>
	/// Serializes/Deserializes a work and all the information ClearShare has about it, as
	/// an OLAC record.
	/// </summary>
	///-----------------------------------------------------------------------------------------
	public class OlacSystem
	{
		private IEnumerable<Role> _roles;
		private static readonly XNamespace s_nsOlac = "http://www.language-archives.org/OLAC/1.1/";
		private static readonly XNamespace s_nsDc = "http://purl.org/dc/elements/1.1/";

		/// ------------------------------------------------------------------------------------
		public void LoadWorkFromXml(Work work, string xml)
		{
			//TODO: parse the xml as olac. For a first pass, we can ignore anything we don't understand.
			//Eventually, we'll want to round-trip things we don't understand.

			Guard.AgainstNull(work, "Work");

			var doc = XDocument.Load(XmlReader.Create(new StringReader(xml)));

			foreach (var contributor in doc.Descendants((new XElement(s_nsDc + "contributor")).Name))
			{
				Role role = GetRoles().ElementAt(0);
				var roleCode = contributor.Attributes().Single(a => a.Name == s_nsOlac + "code").Value;
				TryGetRoleByCode(roleCode, out role);
				work.Contributions.Add(new Contribution(contributor.Value, role));
			}
		}

		/// ------------------------------------------------------------------------------------
		public string GetOlacRecordElement()
		{
			return string.Format(@"<olac:olac xmlns:olac='{0}' xmlns:dc='{1}'
						xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
						xsi:schemaLocation='http://www.language-archives.org/OLAC/1.1/
						http://www.language-archives.org/OLAC/1.1/olac.xsd'>", s_nsOlac, s_nsDc);
		}

		/// ------------------------------------------------------------------------------------
		public string GetContributorElement(string roleCode, string contributorName)
		{
			Guard.AgainstNull(roleCode, "roleCode");
			Guard.AgainstNull(contributorName, "contributorName");

			return string.Format("<dc:contributor xsi:type='olac:role' olac:code='{0}' " +
				"view='Compiler'>{1}</dc:contributor>", roleCode, contributorName);
		}

		/// ------------------------------------------------------------------------------------
		public string GetXmlForWork(Work work)
		{
			var bldr =  new StringBuilder();

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
		public IEnumerable<Role> GetRoles()
		{
			if (_roles == null)
			{
				// TODO: Provide a way for user-specified roles to be read from a roles.xml
				// file in a folder somewhere related to the application.
				var doc = XDocument.Parse(Resources.OlacRoles);

				// This is a bit confusing because the role heading node is at the same level
				// (i.e. a sibling of) as all the associated term nodes. Therefore, the first
				// thing to do is get the heading nodes that are children of section nodes.
				// Then find the one heading node whose content is "Role". Then backup to the
				// section containing that heading node and take all "term" child nodes of
				// that section (i.e. that are siblings of the role heading node).
				_roles = doc.Descendants("body").Descendants("section").Elements("heading")
						.Where(n => n.Value == "Role").Ancestors("section").First().Descendants("term")
						.Select(n => new Role(n.Element("code").Value, n.Element("name").Value, n.Element("definition").Value));
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
		public Role GetRoleByCodeOrThrow(string code)
		{
			var role = GetRoles().FirstOrDefault(r => r.Code == code);

			if (role == null)
			{
				var msg = string.Format("This version of OLAC does not contain a role with code '{0}'.", code);
				throw new ArgumentOutOfRangeException(msg);
			}

			return role;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Used to look up roles in the system's controlled vocabulary
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool TryGetRoleByName(string name, out Role role)
		{
			role = GetRoles().FirstOrDefault(r => r.Name == name);
			return role != null;
		}
	}
}
