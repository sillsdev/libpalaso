using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Palaso.IO;
using Palaso.Reporting;
using Palaso.WritingSystems;

namespace Palaso.Lift
{
	public class WritingSystemsInLiftFileHelper
	{
		private readonly string _liftFilePath;
		private readonly IWritingSystemRepository _writingSystemRepository;

		public WritingSystemsInLiftFileHelper(IWritingSystemRepository writingSystemRepository, string liftFilePath)
		{
			_writingSystemRepository = writingSystemRepository;
			_liftFilePath = liftFilePath;
		}

		public IEnumerable<string> WritingSystemsInUse
		{
			get
			{
				var uniqueIds = new List<string>();
				using (var reader = XmlReader.Create(_liftFilePath))
				{
					while (reader.Read())
					{
						if (reader.MoveToAttribute("lang"))
						{
							if (!uniqueIds.Contains(reader.Value))
							{
								uniqueIds.Add(reader.Value);
							}
						}
					}
				}
				return uniqueIds;
			}
		}

		public void ReplaceWritingSystemId(string oldId, string newId)
		{
			try
			{
				FileUtils.GrepFile(_liftFilePath,
				String.Format(@"lang\s*=\s*[""']{0}[""']", Regex.Escape(oldId)),
				String.Format(@"lang=""{0}""", newId));
			}
			catch (Exception)
			{
				ErrorReport.NotifyUserOfProblem("Another program has the lift file open, so we cannot make the writing system change.  Make sure there are no other programs running that might be using that file.");
			}


		}

		public void CreateNonExistentWritingSystemsFoundInFile()
		{
			WritingSystemOrphanFinder.FindOrphans(WritingSystemsInUse, ReplaceWritingSystemId, _writingSystemRepository);
		}
	}
}
