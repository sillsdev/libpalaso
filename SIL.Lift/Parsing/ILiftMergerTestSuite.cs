namespace SIL.Lift.Parsing
{
	/// <summary>
	/// The idea here was to have a way to help implementers know what new tests are needed as the
	/// parser grows.  WeSay uses it, but it has not been kept up to date, as of April 2008.
	/// </summary>
	public interface ILiftMergerTestSuite
	{
		///<summary></summary>
		void NewEntryGetsGuid();
		///<summary></summary>
		void NewEntryWithTextIdIgnoresIt();
		///<summary></summary>
		void NewEntryTakesGivenDates();
		///<summary></summary>
		void NewEntryNoDatesUsesNow();
		///<summary></summary>
		void EntryGetsEmptyLexemeForm();
		///<summary></summary>
		void NewWritingSystemAlternativeHandled();
		///<summary></summary>
		void NewEntryGetsLexemeForm();
		///<summary></summary>
		void EntryWithChildren();
		///<summary></summary>
		void ModifiedDatesRetained();
		///<summary></summary>
		void ChangedEntryFound();
		///<summary></summary>
		void UnchangedEntryPruned();
		///<summary></summary>
		void EntryWithIncomingUnspecifiedModTimeNotPruned();
		///<summary></summary>
		void MergingSameEntryLackingGuidId_TwiceFindMatch();
	}
}