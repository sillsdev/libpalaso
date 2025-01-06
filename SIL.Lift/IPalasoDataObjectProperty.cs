// Copyright (c) 2009-2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using SIL.ObjectModel;

namespace SIL.Lift
{
	public interface IPalasoDataObjectProperty : ICloneable<IPalasoDataObjectProperty>, IEquatable<IPalasoDataObjectProperty>
	{
		PalasoDataObject Parent { set; }
	}
}