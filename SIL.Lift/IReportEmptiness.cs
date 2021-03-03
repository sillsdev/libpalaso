// Copyright (c) 2009-2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

namespace SIL.Lift
{
	public interface IReportEmptiness
	{
		bool ShouldHoldUpDeletionOfParentObject { get; }
		bool ShouldCountAsFilledForPurposesOfConditionalDisplay { get; }

		bool ShouldBeRemovedFromParentDueToEmptiness { get; }

		void RemoveEmptyStuff();
	}
}