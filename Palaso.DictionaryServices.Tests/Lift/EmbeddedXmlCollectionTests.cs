using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Lift;
using Palaso.TestUtilities;
namespace Palaso.DictionaryServices.Tests.Lift
{
	[TestFixture]
	public class EmbeddedXmlCollectionIClonableGenericTests : IClonableGenericTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewClonable()
		{
			return new EmbeddedXmlCollection();
		}

		public override string ExceptionList
		{
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			get { return "|_parent|"; }
		}

		protected override List<ValuesToSet> DefaultValuesForTypes
		{
			get
			{
				return new List<ValuesToSet>
							 {
								 new ValuesToSet("to be", "!(to be)"),
								new ValuesToSet(new List<string>{"to", "be"}, new List<string>{"!","to","be"})
							 };
			}
		}
	}

	[TestFixture]
	public class EmbeddedXmlCollectionTests
	{
	}
}
