using System;
using System.ComponentModel;

namespace SIL.UiBindings
{
	public interface IBindableControl<TValueType>
	{
		event EventHandler ValueChanged;
		event EventHandler GoingAway;

		/// <summary>
		/// The value or a key, as appropriate
		/// </summary>
		TValueType Value { get; set; }
	}

	public interface IValueHolder<TValueType>: INotifyPropertyChanged
	{
		/// <summary>
		/// The value or a key, as appropriate
		/// </summary>
		TValueType Value { get; set; }
	}
}