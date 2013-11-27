namespace Palaso.Lift
{
	public interface ILiftReaderWriterProvider<T> where T : class, new()
	{
		ILiftWriter<T> CreateWriter(string liftFilePath);
		ILiftReader<T> CreateReader();
	}
}