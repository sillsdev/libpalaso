using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text;

using NUnit.Framework;

using Palaso.WritingSystems;
using Palaso.UI.WindowsForms.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems
{

	[TestFixture]
	public class LookupIsoCodeModelTests
	{
		private LookupIsoCodeModel _model;

		[SetUp]
		public void Setup()
		{
			_model = new LookupIsoCodeModel();
		}

		[Test, Ignore("By hand only")]
		public void LookupISODialog()
		{
			var dialog = new LookupISOCodeDialog();
			Application.Run(dialog);
			MessageBox.Show("code returned:" + dialog.SelectedLanguage.Code);
		}


	}
}