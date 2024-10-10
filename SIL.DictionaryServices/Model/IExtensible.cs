// Copyright (c) 2010-2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;

namespace SIL.DictionaryServices.Model
{
	public interface IExtensible
	{
		List<LexTrait> Traits { get; }
		List<LexField> Fields { get; }
	}
}