using System;
using System.Collections.Generic;
using NUnit.Framework;
using SIL.Reporting;

namespace SIL.Tests.Reporting
{
	[TestFixture]
	public class AnalyticsEventSenderTests
	{
		// 'Correct' values were obtained by actually running the JavaScript from the site.
		[Test]
		public void DomainHash_CorrectlyHashesFlexUrl()
		{
			Assert.That(AnalyticsEventSender.GetDomainHash("flex.palaso.org"), Is.EqualTo(39240052));
		}

		[Test]
		public void DomainHash_CorrectlyHashesFieldworksUrl()
		{
			Assert.That(AnalyticsEventSender.GetDomainHash("fieldworks.sil.org"), Is.EqualTo(100181257));
		}

		[Test]
		public void DomainHash_CorrectlyHashesWeSayUrl()
		{
			Assert.That(AnalyticsEventSender.GetDomainHash("WeSay.palaso.org"), Is.EqualTo(45558018));
		}

		[Test]
		public void DomainHash_CorrectlyHashesLongUrl()
		{
			Assert.That(AnalyticsEventSender.GetDomainHash("This_is_a_very_long_string_for_a_url_to_test_32_bit_overflow.palaso.org"), Is.EqualTo(52249804));
		}

		[Test]
		public void UserIdsInExpectedRange()
		{
			var previous = new HashSet<int>();
			int min = int.MaxValue;
			int max = int.MinValue;
			// There is no absolute reason there could not be a collision for any given set of guids, nor values
			// concentrated in a small range, so using a new random set each time would not give predictable results.
			// OTOH, if it fails with some new algorithm, that algorithm probably deserves some reconsideration,
			// though it MIGHT just need a different test case.
			TestUserIdFromGuid(Guid.Parse("9b8c5bb8-bcda-48a5-b0be-8be69108573e"), previous, ref min, ref max);
			TestUserIdFromGuid(Guid.Parse("7f5da174-dfd3-45b5-a029-78126f580fd8"), previous, ref min, ref max);
			TestUserIdFromGuid(Guid.Parse("49004b22-eb98-4fe9-9c4e-b753a7c92273"), previous, ref min, ref max);
			TestUserIdFromGuid(Guid.Parse("0b009eaf-7e2f-4cc0-b34a-e47aa3e58b37"), previous, ref min, ref max);
			TestUserIdFromGuid(Guid.Parse("6a4183d7-981a-4310-b27d-48ed5d78dbbe"), previous, ref min, ref max);
			TestUserIdFromGuid(Guid.Parse("824f38e5-5d31-4525-9501-f048cb0c5402"), previous, ref min, ref max);
			TestUserIdFromGuid(Guid.Parse("b0a770f4-f346-461c-b5bf-4cd0109f3dae"), previous, ref min, ref max);

			// With six random samples, we should get something beyond the middle of the range.
			Assert.That(min, Is.LessThan(1500000000));
			Assert.That(max, Is.GreaterThan(1800000000));
		}

		void TestUserIdFromGuid(Guid input, HashSet<int> previous, ref int min, ref int max)
		{
			var result = AnalyticsEventSender.GetUserId(input);
			Assert.That(result, Is.GreaterThanOrEqualTo(1000000000), "web examples indicate user IDs should have ten digits");
			// I don't believe this can fail since it is an int; this is the largest possible int.
			Assert.That(result, Is.LessThanOrEqualTo(2147483647), "web examples indicate user IDs should not exceed 2147483647");
			Assert.That(previous, Has.No.Member(result));
			previous.Add(result);
			min = Math.Min(min, result);
			max = Math.Max(max, result);
		}
	}
}
