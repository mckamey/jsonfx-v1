using System;

using Ninject;

namespace JbstOnline
{
	public class AppState
	{
		#region Fields

		private readonly IKernel Kernel;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="kernel">IoC</param>
		[Inject]
		public AppState(IKernel kernel)
		{
			this.Kernel = kernel;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the Inversion of Control container
		/// </summary>
		public IKernel IoC
		{
			get { return this.Kernel; }
		}

		#endregion Properties
	}
}
