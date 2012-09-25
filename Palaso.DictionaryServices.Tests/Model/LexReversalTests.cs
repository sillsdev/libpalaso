using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Tests.Code;
using Palaso.Text;

namespace Palaso.DictionaryServices.Tests.Model
{
	[TestFixture]
	public class LexReversalIClonableGenericTests : IClonableGenericTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewClonable()
		{
			return new LexReversal();
		}

		public override string ExceptionList
		{
			//PropertyChanged: No good way to clone eventhandlers
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			get { return "|_parent|PropertyChanged|"; }
		}

		protected override List<DefaultValues> DefaultValuesForTypes
		{
			get
			{
				return new List<DefaultValues>
							 {
								new DefaultValues("to be", "!(to be)"),
								new DefaultValues(new List<string>{"to", "be"}, new List<string>{"!","to","be"}),
								new DefaultValues(new []{new LanguageForm("en", "en_form", null)}, new []{new LanguageForm("de", "de_form", null)})
							 };
			}
		}
	}
}
