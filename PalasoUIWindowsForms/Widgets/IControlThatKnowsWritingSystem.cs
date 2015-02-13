// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms
{
	public interface IControlThatKnowsWritingSystem
	{
		string Name { get; }
		IWritingSystemDefinition WritingSystem { get; }
		string Text { get; set; }
	}
}
