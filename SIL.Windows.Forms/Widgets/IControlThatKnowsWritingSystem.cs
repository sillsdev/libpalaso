// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using SIL.WritingSystems;

namespace SIL.Windows.Forms.Widgets
{
	public interface IControlThatKnowsWritingSystem
	{
		string Name { get; }
		WritingSystemDefinition WritingSystem { get; }
		string Text { get; set; }
	}
}
