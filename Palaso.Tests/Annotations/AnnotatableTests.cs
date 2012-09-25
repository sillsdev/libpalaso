using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.Annotations;
using Palaso.Tests.Code;

namespace Palaso.Tests.Annotations
{
	[TestFixture]
	public class AnnotatableIClonableGenericTests : IClonableGenericTests<Annotatable>
	{
		public override Annotatable CreateNewClonable()
		{
			return new Annotatable();
		}

		public override string ExceptionList
		{
			get { return ""; }
		}

		protected override List<DefaultValues> DefaultValuesForTypes
		{
			get
			{
				return new List<DefaultValues>
							 {
								 new DefaultValues(new Annotation{IsOn = false}, new Annotation{IsOn = true})
							 };
			}
		}
	}

	[TestFixture]
	public class AnnotationIClonableGenericTests : IClonableGenericTests<Annotation>
	{
		public override Annotation CreateNewClonable()
		{
			return new Annotation();
		}

		public override string ExceptionList
		{
			get { return ""; }
		}

		protected override List<DefaultValues> DefaultValuesForTypes
		{
			get
			{
				return new List<DefaultValues>
							 {
								 new DefaultValues(42, 7)
							 };
			}
		}
	}
}
