#region license
// Copyright (c) 2005 - 2007 Ayende Rahien (ayende@ayende.com)
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//     * Neither the name of Ayende Rahien nor the names of its
//     contributors may be used to endorse or promote products derived from this
//     software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

namespace Palaso
{
	#if MONO
	#else

	/// <summary>
	/// Common delegate definition
	/// </summary>
	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate TRet Func<TRet>();

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate TRet Func<TRet, A0>(A0 a0);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate TRet Func<TRet, A0, A1>(A0 a0, A1 a1);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate TRet Func<TRet, A0, A1, A2>(A0 a0, A1 a1, A2 a2);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate TRet Func<TRet, A0, A1, A2, A3>(A0 a0, A1 a1, A2 a2, A3 a3);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate TRet Func<TRet, A0, A1, A2, A3, A4>(A0 a0, A1 a1, A2 a2, A3 a3, A4 a4);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate TRet Func<TRet, A0, A1, A2, A3, A4, A5>(A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate TRet Func<TRet, A0, A1, A2, A3, A4, A5, A6>(A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate TRet Func<TRet, A0, A1, A2, A3, A4, A5, A6, A7>(A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate TRet Func<TRet, A0, A1, A2, A3, A4, A5, A6, A7, A8>(A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8);

	#endif

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate void Proc();

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate void Proc<A0>(A0 a0);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate void Proc<A0, A1>(A0 a0, A1 a1);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate void Proc<A0, A1, A2>(A0 a0, A1 a1, A2 a2);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate void Proc<A0, A1, A2, A3>(A0 a0, A1 a1, A2 a2, A3 a3);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate void Proc<A0, A1, A2, A3, A4>(A0 a0, A1 a1, A2 a2, A3 a3, A4 a4);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate void Proc<A0, A1, A2, A3, A4, A5>(A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate void Proc<A0, A1, A2, A3, A4, A5, A6>(A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7>(A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7);

	/// <summary>
	/// Common delegate definition
	/// </summary>
	public delegate void Proc<A0, A1, A2, A3, A4, A5, A6, A7, A8>(A0 a0, A1 a1, A2 a2, A3 a3, A4 a4, A5 a5, A6 a6, A7 a7, A8 a8);
}
