using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Palaso.Code
{
	public interface IClonableGeneric<T>
	{
		/// <summary>
		/// Deepclone
		/// </summary>
		/// <returns></returns>
		T Clone();
		//bool Equals(T other);
	}
}
