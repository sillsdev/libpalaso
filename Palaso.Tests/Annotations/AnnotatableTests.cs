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

		public override Dictionary<Type, object> DefaultValuesForTypes
		{
			get
			{
				return new Dictionary<Type, object>
							 {
								 {typeof(Annotation), new Annotation()}
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

		public override Dictionary<Type, object> DefaultValuesForTypes
		{
			get
			{
				return new Dictionary<Type, object>
							 {
								 {typeof(int), 42}
							 };
			}
		}
	}
}
