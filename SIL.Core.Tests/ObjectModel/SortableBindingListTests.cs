using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using SIL.ObjectModel;

namespace SIL.Tests.ObjectModel
{
	// Taken and modified from http://betimvwframework.codeplex.com/
	// Apache License 2.0: http://betimvwframework.codeplex.com/license

	[TestFixture]
	public class SortableBindingListTests
	{
		#region Constructor tests
		[Test]
		public void InitializesWithZeroElements()
		{
			SortableBindingList<int> list = new SortableBindingList<int>();
			Assert.AreEqual(0, list.Count);
		}

		[Test]
		public void CopiesAllElementsFromList()
		{
			IList<Person> persons = SortableBindingListFactory.GetPersons();
			SortableBindingList<Person> list = new SortableBindingList<Person>(persons);
			Assert.AreEqual(persons.Count, list.Count);
		}
		#endregion

		#region IsSorted tests
		[Test]
		public void IsSorted_FalseWhenNotSorted()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();
			Assert.IsFalse(bindingList.IsSorted);
		}

		[Test]
		public void IsSorted_TrueWhenSorted()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();
			PropertyDescriptor propertyDescriptor = PropertyDescriptorHelper.Get(bindingList[0], "Id");
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Ascending);

			Assert.IsTrue(bindingList.IsSorted);
		}
		#endregion

		#region SupportsSearching tests
		[Test]
		public void SupportsSearching_True()
		{
			IBindingList bindingList = new SortableBindingList<int>();
			Assert.IsTrue(bindingList.SupportsSearching);
		}
		#endregion

		#region SupportsSorting tests
		[Test]
		public void SupportsSorting_True()
		{
			IBindingList bindingList = new SortableBindingList<int>();
			Assert.IsTrue(bindingList.SupportsSorting);
		}
		#endregion

		#region ApplySort tests
		[Test]
		public void ApplySort_GenericComparable_Desc()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();

			IList sortedByAddressDescending = new[]
            {
                bindingList[2],
                bindingList[1],
                bindingList[0]
            };

			PropertyDescriptor propertyDescriptor = PropertyDescriptorHelper.Get(bindingList[0], "Address");
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Descending);
			AssertSortableBindingList.HaveSameElements(sortedByAddressDescending, bindingList);
		}

		[Test]
		public void ApplySort_GenericComparable_Asc()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();

			IList sortedByAddressAscending = new[]
            {
                bindingList[0],
                bindingList[1],
                bindingList[2]
            };

			PropertyDescriptor propertyDescriptor = PropertyDescriptorHelper.Get(bindingList[0], "Address");
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Ascending);
			AssertSortableBindingList.HaveSameElements(sortedByAddressAscending, bindingList);
		}

		[Test]
		public void ApplySort_Comparable_Desc()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();

			IList sortedByIdDescending = new[]
            {
                bindingList[2],
                bindingList[1],
                bindingList[0]
            };

			PropertyDescriptor propertyDescriptor = PropertyDescriptorHelper.Get(bindingList[0], "Id");
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Descending);
			AssertSortableBindingList.HaveSameElements(sortedByIdDescending, bindingList);
		}

		[Test]
		public void ApplySort_Comparable_Asc()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();

			IList sortedByIdAscending = new[]
            {
                bindingList[0],
                bindingList[1],
                bindingList[2]
            };

			PropertyDescriptor propertyDescriptor = PropertyDescriptorHelper.Get(bindingList[0], "Id");
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Ascending);
			AssertSortableBindingList.HaveSameElements(sortedByIdAscending, bindingList);
		}
		#endregion

		#region other sorting tests
		[Test]
		public void ReusePropertyComparerIfSortIsAppliedForSecondTime()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();

			IList sortedByAddressDescending = new[]
            {
                bindingList[2],
                bindingList[1],
                bindingList[0]
            };

			PropertyDescriptor propertyDescriptor = PropertyDescriptorHelper.Get(bindingList[0], "Address");
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Descending);
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Descending);
			AssertSortableBindingList.HaveSameElements(sortedByAddressDescending, bindingList);
		}

		[Test]
		public void SetSortDirectionCore()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();
			PropertyDescriptor propertyDescriptor = PropertyDescriptorHelper.Get(bindingList[0], "Id");
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Descending);
			ListSortDirection actual = bindingList.SortDirection;
			ListSortDirection expected = ListSortDirection.Descending;
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void SortOnPropertiesOfSameType()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();

			IList sortedByFirstNameAscending = new[]
            {
                bindingList[0],
                bindingList[1],
                bindingList[2]
            };

			IList sortedByFirstNameDescending = new[]
            {
                bindingList[2],
                bindingList[1],
                bindingList[0]
            };

			IList sortedByLastNameAscending = new[]
            {
                bindingList[1],
                bindingList[2],
                bindingList[0]
            };

			IList sortedByLastNameDescending = new[]
            {
                bindingList[0],
                bindingList[2],
                bindingList[1]
            };

			PropertyDescriptor firstNameProperty = PropertyDescriptorHelper.Get(bindingList[0], "FirstName");
			PropertyDescriptor lastNameProperty = PropertyDescriptorHelper.Get(bindingList[0], "LastName");

			bindingList.ApplySort(lastNameProperty, ListSortDirection.Descending);
			AssertSortableBindingList.HaveSameElements(sortedByLastNameDescending, bindingList);

			bindingList.ApplySort(firstNameProperty, ListSortDirection.Descending);
			AssertSortableBindingList.HaveSameElements(sortedByFirstNameDescending, bindingList);

			bindingList.ApplySort(firstNameProperty, ListSortDirection.Ascending);
			AssertSortableBindingList.HaveSameElements(sortedByFirstNameAscending, bindingList);

			bindingList.ApplySort(lastNameProperty, ListSortDirection.Ascending);
			AssertSortableBindingList.HaveSameElements(sortedByLastNameAscending, bindingList);
		}
		#endregion

		#region Find tests
		[Test]
		public void Find_ItemFound_ReturnIndexOfItem()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();
			PropertyDescriptor propertyDescriptor = PropertyDescriptorHelper.Get(bindingList[0], "Address");
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Descending);

			object key = new GenericComparableAddress("2");
			int actual = bindingList.Find(propertyDescriptor, key);

			int expected = 1;

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void Find_ItemNotFound_ReturnMinusOne()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();
			PropertyDescriptor propertyDescriptor = PropertyDescriptorHelper.Get(bindingList[0], "Address");
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Descending);

			object key = new GenericComparableAddress("4");
			int actual = bindingList.Find(propertyDescriptor, key);

			int expected = -1;

			Assert.AreEqual(expected, actual);
		}
		#endregion

		#region RemoveSort tests
		[Test]
		public void RemoveSort_ReturnFalseForIsSorted()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();
			PropertyDescriptor propertyDescriptor = PropertyDescriptorHelper.Get(bindingList[0], "Id");
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Ascending);
			bindingList.RemoveSort();

			Assert.IsFalse(bindingList.IsSorted);
		}

		[Test]
		public void RemoveSort_SetSortPropertyToNull()
		{
			IBindingList bindingList = SortableBindingListFactory.Create();
			PropertyDescriptor propertyDescriptor = PropertyDescriptorHelper.Get(bindingList[0], "Id");
			bindingList.ApplySort(propertyDescriptor, ListSortDirection.Ascending);
			bindingList.RemoveSort();

			Assert.IsNull(bindingList.SortProperty);
		}
		#endregion

		#region other tests
		[Test]
		public void ChangingBindingListChangesUnderlyingList()
		{
			IList<string> underlyingList = new List<string> { "A" };
			SortableBindingList<string> sortableBindingList = new SortableBindingList<string>(underlyingList);
			sortableBindingList.Add("B");
			Assert.AreEqual(2, underlyingList.Count);
			Assert.AreEqual("B", underlyingList[1]);
		}
		#endregion
	}

	#region Supporting classes for tests
	public static class SortableBindingListFactory
	{
		public static IList<Person> GetPersons()
		{
			return new[]
            {
                new Person(new ComparableIdentification(1), "A", "3", new NotComparableDate(1980, 04, 30), new GenericComparableAddress("1")),
                new Person(new ComparableIdentification(2), "B", "1", new NotComparableDate(1982, 01, 30), new GenericComparableAddress("2")),
                new Person(new ComparableIdentification(3), "C", "2", new NotComparableDate(1984, 02, 20), new GenericComparableAddress("3"))
            };
		}

		public static SortableBindingList<Person> Create()
		{
			return new SortableBindingList<Person>(GetPersons());
		}
	}

	public static class PropertyDescriptorHelper
	{
		public static PropertyDescriptor Get(object component, string propertyName)
		{
			PropertyDescriptor propertyDescriptor;
			if (TryGet(component, propertyName, out propertyDescriptor))
			{
				return propertyDescriptor;
			}

			throw new ArgumentException(string.Format("The property '{0}' was not found.", propertyName));
		}

		public static bool TryGet(object component, string propertyName, out PropertyDescriptor propertyDescriptor)
		{
			PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(component);
			foreach (PropertyDescriptor aPropertyDescriptor in propertyDescriptors)
			{
				if (aPropertyDescriptor.Name == propertyName)
				{
					propertyDescriptor = aPropertyDescriptor;
					return true;
				}
			}

			propertyDescriptor = null;
			return false;
		}
	}

	public class Person
	{
		public Person(ComparableIdentification id, string firstName, string lastName, NotComparableDate birthday, GenericComparableAddress address)
		{
			Id = id;
			FirstName = firstName;
			LastName = lastName;
			Birthday = birthday;
			Address = address;
		}

		public ComparableIdentification Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public NotComparableDate Birthday { get; set; }

		public GenericComparableAddress Address { get; set; }
	}

	public class ComparableIdentification : IComparable
	{
		public ComparableIdentification(int id)
		{
			Id = id;
		}

		public int Id { get; set; }

		#region IComparable Members

		public int CompareTo(object obj)
		{
			ComparableIdentification other = obj as ComparableIdentification;
			if (other == null)
			{
				return 1;
			}

			return Id.CompareTo(other.Id);
		}

		#endregion
	}

	public static class AssertSortableBindingList
	{
		public static void HaveSameElements(IList expected, IBindingList actual)
		{
			Assert.AreEqual(expected.Count, actual.Count);
			for (int i = 0; i < actual.Count; ++i)
			{
				Assert.IsTrue(ReferenceEquals(expected[i], actual[i]));
			}
		}
	}

	public class NotComparableDate
	{
		private DateTime _date;

		public NotComparableDate(int year, int month, int day)
		{
			_date = new DateTime(year, month, day);
		}

		public override string ToString()
		{
			return _date.ToString("dd/MM/yyyy");
		}
	}

	public class GenericComparableAddress : IComparable<GenericComparableAddress>, IEquatable<GenericComparableAddress>
	{
		public GenericComparableAddress(string location)
		{
			Location = location;
		}

		public string Location { get; set; }

		#region IComparable<AddressGenericComparable> Members

		public int CompareTo(GenericComparableAddress other)
		{
			return String.Compare(Location, other.Location, StringComparison.Ordinal);
		}

		#endregion

		#region IEquatable<AddressGenericComparable> Members

		public bool Equals(GenericComparableAddress other)
		{
			return Equals((object)other);
		}

		#endregion

		public override bool Equals(object obj)
		{
			GenericComparableAddress other = obj as GenericComparableAddress;

			if (other == null)
			{
				return false;
			}

			return CompareTo(other) == 0;
		}

		public override int GetHashCode()
		{
			return Location.GetHashCode();
		}
	}
	#endregion
}
