#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

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
using System.Collections.Generic;

using JsonFx.Json;

namespace JsonFx.UI.Jbst
{
	/// <summary>
	/// Internal representation of a JBST custom control
	/// </summary>
	internal class JbstCustomControl : JbstCustomControlBase
	{
		#region Constants

		public const string ControlCommand = "control";

		private const string ControlSimpleFormat =
			@"function() {{
				return JsonML.BST({0}).dataBind({1}, {2}, {3});
			}}";

		private const string ControlWrapperStartFormat =
			@"function() {{
				return JsonML.BST({0}).dataBind({1}, {2}, {3}, ";

		private const string ControlWrapperEnd =
			@");
			}";

		private const string ControlInlineStart =
			@"function() {
				return JsonML.BST(";

		private const string ControlInlineEndFormat =
			@").dataBind({0}, {1}, {2});
			}}";

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public JbstCustomControl()
			: base(JbstCustomControl.ControlCommand)
		{
		}

		#endregion Init

		#region Render Methods

		/// <summary>
		/// Controls the control rendering style
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(JsonWriter writer)
		{
			if (String.IsNullOrEmpty(this.NameExpr) && !this.ChildControlsSpecified)
			{
				throw new InvalidOperationException("JBST Control requires either a named template or an anonymous inline template.");
			}

			if (!this.ChildControlsSpecified)
			{
				this.RenderSimpleCustomControl(writer);
			}
			else if (String.IsNullOrEmpty(this.NameExpr))
			{
				this.RenderInlineCustomControl(writer);
			}
			else
			{
				this.RenderWrapperCustomControl(writer);
			}
		}

		/// <summary>
		/// Renders a simple data binding call to a named template.
		/// </summary>
		/// <param name="writer"></param>
		private void RenderSimpleCustomControl(JsonWriter writer)
		{
			writer.TextWriter.Write(
				JbstCustomControl.ControlSimpleFormat,
				this.NameExpr,
				this.DataExpr,
				this.IndexExpr,
				this.CountExpr);
		}

		/// <summary>
		/// Renders a data binding call to an inline anonymous template.
		/// </summary>
		/// <param name="writer"></param>
		private void RenderInlineCustomControl(JsonWriter writer)
		{
			writer.TextWriter.Write(ControlInlineStart);

			writer.Write(new EnumerableAdapter(this));

			writer.TextWriter.Write(
				JbstCustomControl.ControlInlineEndFormat,
				this.DataExpr,
				this.IndexExpr,
				this.CountExpr);
		}

		/// <summary>
		/// Renders a data binding call to a named template with a nested inline anonymous placeholder template.
		/// </summary>
		/// <param name="writer"></param>
		private void RenderWrapperCustomControl(JsonWriter writer)
		{
			writer.TextWriter.Write(
				ControlWrapperStartFormat,
				this.NameExpr,
				this.DataExpr,
				this.IndexExpr,
				this.CountExpr);

			Dictionary<string, object> options = new Dictionary<string, object>(1);
			options["$"/*placholder name*/] = new EnumerableAdapter(this);
			writer.Write(options);

			writer.TextWriter.Write(JbstCustomControl.ControlWrapperEnd);
		}

		#endregion Render Methods
	}
}
