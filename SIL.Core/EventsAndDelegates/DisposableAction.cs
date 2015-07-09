#region license
// Copyright (c) 2005 - 2007 Ayende Rahien (ayende@ayende.com)
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//     * Neither the name of Ayende Rahien nor the names of its
//     contributors may be used to endorse or promote products derived from this
//     software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;

namespace SIL.EventsAndDelegates
{
	/// <summary>
	/// Better sytnax for context operation.
	/// Wraps a delegate that is executed when the Dispose method is called.
	/// This allows to do context sensitive things easily.
	/// Basically, it mimics Java's anonymous classes.
	/// </summary>
	/// <typeparam name="T">
	/// The type of the parameter that the delegate to execute on dispose
	/// will accept
	/// </typeparam>
	public class DisposableAction<T> : IDisposable
	{
		readonly Proc<T> _action;
		readonly T _val;

		/// <summary>
		/// Initializes a new instance of the <see cref="DisposableAction&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="action">The action to execute on dispose</param>
		/// <param name="val">The value that will be passed to the action on dispose</param>
		public DisposableAction(Proc<T> action, T val)
		{
			if (action == null)
				throw new ArgumentNullException("action");
			_action = action;
			_val = val;
		}


		/// <summary>
		/// Gets the value associated with this action
		/// </summary>
		/// <value>The value.</value>
		public T Value
		{
			get { return _val; }
		}


		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_action(_val);
		}
	}


	/// <summary>
	/// Better sytnax for context operation.
	/// Wraps a delegate that is executed when the Dispose method is called.
	/// This allows to do context sensitive things easily.
	/// Basically, it mimics Java's anonymous classes.
	/// </summary>
	public class DisposableAction : IDisposable
	{
		readonly Proc _action;

		/// <summary>
		/// Initializes a new instance of the <see cref="DisposableAction"/> class.
		/// </summary>
		/// <param name="action">The action to execute on dispose</param>
		public DisposableAction(Proc action)
		{
			if (action == null)
				throw new ArgumentNullException("action");
			_action = action;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_action();
		}
	}
}
