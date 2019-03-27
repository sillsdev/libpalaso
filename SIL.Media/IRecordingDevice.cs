// Copyright (c) 2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using NAudio.Wave;

namespace SIL.Media
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