using System;

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
	}
}
