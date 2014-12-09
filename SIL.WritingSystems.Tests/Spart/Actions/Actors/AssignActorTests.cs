using System;
using NUnit.Framework;
using Spart.Actions;
using Spart.Parsers;
using Spart.Parsers.Primitives;
using Spart.Scanners;

namespace SIL.WritingSystems.Tests.Spart.Actions.Actors
{
	[TestFixture]
	public class AssignActorTests
	{
		[Test]
		public void AssignString()
		{
			IScanner scanner = Provider.NewScanner;
			StringParser parser = new StringParser(Provider.Text);
			object o = "test";
			parser.Act += delegate(Object sender, ActionEventArgs args)
								{
									o=args.Value;
								};
			parser.Parse(scanner);

			Assert.AreEqual(Provider.Text, o);
		}

		[Test]
		public void AssignStringShorthand()
		{
			IScanner scanner = Provider.NewScanner;
			object o = "test";
			Parser parser = new StringParser(Provider.Text)[delegate(Object sender, ActionEventArgs args)
																	{
																		o = args.Value;
																	}];
			parser.Parse(scanner);

			Assert.AreEqual(Provider.Text, o);
		}

	}

}