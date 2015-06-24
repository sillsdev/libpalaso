namespace SIL.IO.FileLock
{
	public interface IFileLock
	{
		string LockName { get; }

		bool TryAcquireLock();

		bool ReleaseLock();
	}
}
