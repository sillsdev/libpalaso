// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using JetBrains.Annotations;
using NAudio.Wave;

namespace SIL.Media
{
	public interface IRecordingDevice
	{
		string Id { get; }
		int DeviceNumber { get; set; }
		string GenericName { get; set; }
		string ProductName { get; }
		[PublicAPI]
		WaveInCapabilities Capabilities { get; set; }
	}
}