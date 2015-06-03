using System.Collections.Generic;
using NUnit.Framework;
using Palaso.TestUtilities;
using SIL.Lift;
using SIL.Text;

namespace SIL.DictionaryServices.Tests.Lift
{
	[TestFixture]
	public class PictureRefCloneableTests : CloneableTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewCloneable()
		{
			return new PictureRef();
		}

		public override string ExceptionList
		{
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			//PropertyChanged: No good way to clone eventhandlers
			get { return "|_parent|PropertyChanged|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
							 {
								 new ValuesToSet("to be", "!(to be)"),
								 new ValuesToSet(
									 new MultiText{Forms=new[]{new LanguageForm("en", "en_form", null), }},
									 new MultiText{Forms=new[]{new LanguageForm("de", "de_form", null), }})
							 };
			}
		}
	}
}
