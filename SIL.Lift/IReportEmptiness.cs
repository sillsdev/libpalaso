// Copyright (c) 2009-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using JetBrains.Annotations;

namespace SIL.Lift
{
	public interface IReportEmptiness
	{
		[PublicAPI]
		bool ShouldHoldUpDeletionOfParentObject { get; }
		[PublicAPI]
		bool ShouldCountAsFilledForPurposesOfConditionalDisplay { get; }

		bool ShouldBeRemovedFromParentDueToEmptiness { get; }

		void RemoveEmptyStuff();
	}
}