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
	public class PictureRefIClonableGenericTests : IClonableGenericTests<IPalasoDataObjectProperty>
	{
		public override IPalasoDataObjectProperty CreateNewClonable()
		{
			return new PictureRef();
		}

		public override string ExceptionList
		{
			//_parent: We are doing top down clones. Children shouldn't make clones of their parents, but parents of their children.
			//PropertyChanged: No good way to clone eventhandlers
			get { return "|_parent|PropertyChanged|"; }
		}

		public override Dictionary<Type, object> DefaultValuesForTypes
		{
			get
			{
				var multiText = new MultiText();
				multiText.SetAlternative("en", "en text");
				return new Dictionary<Type, object>
						   {
							   {typeof(string), "Hark! A string!"},
							   {typeof(MultiText), multiText}
						   };
			}
		}
	}
}
