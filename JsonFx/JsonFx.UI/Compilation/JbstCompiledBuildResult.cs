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
