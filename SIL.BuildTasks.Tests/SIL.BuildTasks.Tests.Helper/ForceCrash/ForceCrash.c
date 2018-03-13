// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

#include <stdio.h>

#ifdef WIN32
__declspec(dllexport) void ForceCrash(int *p);
__declspec(dllexport) void OutputOnStderr();
#endif

void ForceCrash()
{
	// Force the calling app to crash if an invalid pointer (i.e. NULL) gets passed in
	int *p = NULL;
	*p = 42;
}

void OutputOnStderr()
{
	fprintf(stderr, "Just testing");
}