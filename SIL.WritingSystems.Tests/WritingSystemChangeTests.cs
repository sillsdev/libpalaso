﻿using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class WritingSystemLogEventTests
	{
		[Test]
		public void ChangeEventConstructor_Data_SetsAllProperties()
		{
			var change = new WritingSystemLogChangeEvent("aaa", "bbb");
			Assert.That(change.From, Is.EqualTo("aaa"));
			Assert.That(change.To, Is.EqualTo("bbb"));
		}

		[Test]
		public void DeleteEventConstructor_Data_SetsAllProperties()
		{
			var change = new WritingSystemLogDeleteEvent("aaa");
			Assert.That(change.ID, Is.EqualTo("aaa"));
		}

		[Test]
		public void AddEventConstructor_Data_SetsAllProperties()
		{
			var change = new WritingSystemLogAddEvent("aaa");
			Assert.That(change.ID, Is.EqualTo("aaa"));
		}
	}
}