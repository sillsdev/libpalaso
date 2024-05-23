using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SIL.DblBundle;
using SIL.DblBundle.Text;
using SIL.Windows.Forms.DblBundle;

namespace SIL.Windows.Forms.TestApp
{
	public partial class ExistingProjectsList : ProjectsListBase<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>
	{
		private class SampleProject : IProjectInfo
		{
			private readonly int _id;
			public string Name => "Sample " + _id;
			public string Id => "sample" + _id;
			public DblMetadataLanguage Language => new DblMetadataLanguage {Name = "English", Iso = "en"};

			public SampleProject(int id)
			{
				_id = id;
			}
		}

		public ExistingProjectsList()
		{
			InitializeComponent();
		}

		protected override DataGridViewColumn FillColumn => colFullName;

		protected override IEnumerable<string> AllProjectFolders => Array.Empty<string>();

		public const string kProjectFileExtension = ".hearthis";

		protected override IEnumerable<object> GetAdditionalRowData(IProjectInfo projectInfo)
		{
			yield return projectInfo.Name;
			yield return projectInfo is SampleProject ? "Sample project" : "Other";
		}

		protected override string ProjectFileExtension => kProjectFileExtension;

		protected override IEnumerable<Tuple<string, IProjectInfo>> Projects
		{
			get
			{
				foreach (var project in base.Projects)
					yield return project;

				for (int i = 1; i < 1500; i++)
					yield return new Tuple<string, IProjectInfo>("sample" + i, new SampleProject(i));
			}
		}

		protected override IProjectInfo GetProjectInfo(string path)
		{
			var bundle = new TextBundle<DblTextMetadata<DblMetadataLanguage>, DblMetadataLanguage>(path);
			return bundle.Metadata;
		}

		protected override string GetRecordingProjectName(Tuple<string, IProjectInfo> project)
		{
			if (project.Item2 is DblTextMetadata<DblMetadataLanguage>)
				return project.Item2.Id;
			return base.GetRecordingProjectName(project);
		}
	}
}
