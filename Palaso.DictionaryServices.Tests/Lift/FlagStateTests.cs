using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Lift;
using Palaso.Tests.Code;

namespace Palaso.DictionaryServices.Tests.Lift
{
	[TestFixture]
	public class FlagStateIClonableGenericTests : IClonableGenericTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewClonable()
		{
			return new FlagState();
		}

		public override string ExceptionList
		{
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			//PropertyChanged: No good way to clone eventhandlers
			get { return "|_parent|PropertyChanged|"; }
		}

		protected override List<DefaultValues> DefaultValuesForTypes
		{
			get
			{
				return new List<DefaultValues>
							 {
								 new DefaultValues(true, false)
							 };
			}
		}
	}

	[TestFixture]
	public class FlagStateTests
	{

	}
}
