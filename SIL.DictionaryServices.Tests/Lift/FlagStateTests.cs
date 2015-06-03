using System.Collections.Generic;
using NUnit.Framework;
using SIL.Lift;
using SIL.TestUtilities;

namespace SIL.DictionaryServices.Tests.Lift
{
	[TestFixture]
	public class FlagStateCloneableTests : CloneableTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewCloneable()
		{
			return new FlagState();
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
								 new ValuesToSet(true, false)
							 };
			}
		}
	}

	[TestFixture]
	public class FlagStateTests
	{

	}
}
