// Copyright (c) 2016-2024, SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using SIL.Acknowledgements;

// The following Acknowledgements test the AcknowledgementsProvider for SIL AboutBox
[assembly: Acknowledgement("ackAttrKey1", Name = "test acknowledgement name", Url = "http://www.google.com",
	Location = "test location", LicenseUrl = "http://opensource.org/licenses/MIT", Copyright = "test copyright 2024")]

[assembly: Acknowledgement("ackAttrKey2", Name = "my test name", LicenseUrl = "http://opensource.org/licenses/MIT",
	 Copyright = "my test 2017 copyright", Location = "my test location")]
