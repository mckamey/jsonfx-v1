#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2008 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion License

using System;

using JsonFx.Handlers;

namespace JsonFx.Compilation
{
	/// <summary>
	/// The compilation result for JBSTs adds additional information.
	/// </summary>
	public abstract class JbstCompiledBuildResult : JsonFx.Handlers.CompiledBuildResult
	{
		#region Properties

		/// <summary>
		/// Gets the keys needed by the compiled JBST
		/// </summary>
		public abstract string[] GlobalizationKeys
		{
			get;
		}

		#endregion Properties

		#region Factory Methods

		/// <summary>
		/// JbstCompiledBuildResult Factory method
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <returns></returns>
		protected internal static new JbstCompiledBuildResult Create(string virtualPath)
		{
			return CompiledBuildResult.Create(virtualPath) as JbstCompiledBuildResult;
		}

		#endregion Factory Methods
	}
}
