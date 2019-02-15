using NAudio.Wave;

namespace SIL.Media.Naudio
{
	public interface IRecordingDevice
	{
		string Id { get; }
		int DeviceNumber { get; set; }
		string GenericName { get; set; }
		string ProductName { get; }
		WaveInCapabilities Capabilities { get; set; }
	}
}