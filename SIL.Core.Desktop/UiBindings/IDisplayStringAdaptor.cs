using System.Collections;
using System.Collections.Generic;

namespace SIL.UiBindings
{
	public interface IDisplayStringAdaptor
	{
		string GetDisplayLabel(object item);
		string GetToolTip(object item);
		string GetToolTipTitle(object item);
	}

	public class ToStringAutoCompleteAdaptor: IDisplayStringAdaptor
	{
		public string GetDisplayLabel(object item)
		{
			return item.ToString();
		}

		public string GetToolTip(object item)
		{
			return "";
		}

		public string GetToolTipTitle(object item)
		{
			return "";
		}
	}

	public interface IChoiceSystemAdaptor<KV, ValueT, KEY_CONTAINER>: IDisplayStringAdaptor
		where ValueT : class
	{
		void UpdateKeyContainerFromKeyValue(KV kv, KEY_CONTAINER kc);
		IDisplayStringAdaptor ToolTipAdaptor { get; }

		ValueT GetValueFromKeyValue(KV kv);

		KV GetKeyValueFromKey_Container(KEY_CONTAINER kc);
		KV GetKeyValueFromValue(ValueT t);

		KV GetValueFromForm(string form);

		object GetValueFromFormNonGeneric(string form);

		IEnumerable GetItemsToOffer(string text, IEnumerable items, IDisplayStringAdaptor adaptor);
	}

	/// <summary>
	/// This is used to convert from the IEnuerable&lt;string&gt; that the cache give us
	/// to the IEnumerable&lt;object&gt; that AutoComplete needs.
	/// </summary>
	public class StringToObjectEnumerableWrapper: IEnumerable<object>
	{
		private readonly IEnumerable<string> _stringCollection;

		public StringToObjectEnumerableWrapper(IEnumerable<string> stringCollection)
		{
			_stringCollection = stringCollection;
		}

		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			foreach (string s in _stringCollection)
			{
				yield return s;
			}
		}

		public IEnumerator GetEnumerator()
		{
			return ((IEnumerable<object>) this).GetEnumerator();
		}
	}

	public interface IChoice
	{
		string Label { get; }
		string Key { get; }
	}
}