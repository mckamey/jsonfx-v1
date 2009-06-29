#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2009 Stephen M. McKamey

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
using System.Collections;

using JsonFx.Json;

namespace JsonFx.UI.Jbst
{
	/// <summary>
	/// Internal representation of a nested JBST control.
	/// </summary>
	internal class JbstCustomControl : JbstContainerControl, IJsonSerializable
	{
		#region Constants

		public const string JbstPrefix = "jbst"+JbstWriter.PrefixDelim;
		private const string PlaceholderCommand = "placeholder";

		private const string PlaceholderStatement =
			@"function(t) {{
				if (t) {{
					return JsonML.BST(t).dataBind({0}, {1}, {2});
				}}
			}}";

		private const string ControlCommand = "control";
		private const string ControlNameKey = "name";
		private const string ControlNameKeyAlt = JbstCustomControl.JbstPrefix+JbstCustomControl.ControlNameKey;
		private const string ControlDataKey = "data";
		private const string ControlDataKeyAlt = JbstCustomControl.JbstPrefix+JbstCustomControl.ControlDataKey;
		private const string ControlIndexKey = "index";
		private const string ControlIndexKeyAlt = JbstCustomControl.JbstPrefix+JbstCustomControl.ControlIndexKey;
		private const string ControlCountKey = "count";
		private const string ControlCountKeyAlt = JbstCustomControl.JbstPrefix+JbstCustomControl.ControlIndexKey;

		private const string ControlSimpleFormat =
			@"function() {{
				return JsonML.BST({0}).dataBind({1}, {2}, {3});
			}}";

		private const string ControlStart =
			@"function() {
				var t = JsonML.BST(";

		private const string ControlEndFormat =
			@");
				t.prototype = this;
				return JsonML.BST({0}).dataBind({1}, {2}, {3}, t);
			}}";

		private const string ControlInlineStart =
			@"function() {
				return JsonML.BST(";

		private const string ControlInlineEndFormat =
			@").dataBind({0}, {1}, {2});
			}}";

		private const string FunctionEvalExpression = "({0})()";

		private const string DefaultDataExpression = "this.data";
		private const string DefaultIndexExpression = "this.index";
		private const string DefaultCountExpression = "this.count";

		#endregion Constants

		#region Fields

		private bool isProcessed;
		private readonly string commandType;
		private string nameExpr;
		private string dataExpr;
		private string indexExpr;
		private string countExpr;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="commandName"></param>
		public JbstCustomControl(string commandName)
		{
			this.commandType = (commandName == null) ?
				String.Empty :
				commandName.ToLowerInvariant();
		}

		#endregion Init

		#region Properties

		[JsonName("rawName")]
		public override string RawName
		{
			get { return JbstCustomControl.JbstPrefix + this.commandType; }
		}

		#endregion Properties

		#region Render Methods

		private void EnsureControl()
		{
			if (this.isProcessed)
			{
				return;
			}

			// placeholder does not need a named JBST
			if (JbstCustomControl.ControlCommand.Equals(this.commandType, StringComparison.OrdinalIgnoreCase))
			{
				this.nameExpr = this.ProcessArgument(String.Empty, ControlNameKey, ControlNameKeyAlt);
				//this.nameExpr = JsonWriter.EnsureValidIdentifier(this.nameExpr, true, false);

				if (String.IsNullOrEmpty(this.nameExpr) && !this.ChildControlsSpecified)
				{
					throw new InvalidOperationException("JBST Control requires either a named template or an anonymous inline template.");
				}
			}

			this.dataExpr = this.ProcessArgument(DefaultDataExpression, ControlDataKey, ControlDataKeyAlt);
			this.indexExpr = this.ProcessArgument(DefaultIndexExpression, ControlIndexKey, ControlIndexKeyAlt);
			this.countExpr = this.ProcessArgument(DefaultCountExpression, ControlCountKey, ControlCountKeyAlt);

			this.Attributes.Clear();
			this.isProcessed = true;
		}

		/// <summary>
		/// Processes each argument allowing string literals to code expressions to function calls.
		/// </summary>
		/// <param name="defaultValue">the default value if none was supplied</param>
		/// <param name="keys">an ordered list of keys to check</param>
		/// <returns>the resulting expression</returns>
		private string ProcessArgument(string defaultValue, params string[] keys)
		{
			object argument = null;
			foreach (string key in keys)
			{
				if (this.Attributes.ContainsKey(key))
				{
					argument = this.Attributes[key];
					break;
				}
			}

			string value;
			if (argument == null)
			{
				value = defaultValue;
			}
			else if (argument is string)
			{
				// directly use as inline expression
				value = (string)argument;
			}
			else if (argument is JbstExpressionBlock)
			{
				// convert to inline expression
				value = ((JbstExpressionBlock)argument).Code;
			}
			else if (argument is JbstCodeBlock)
			{
				// convert to anonymous function expression
				value = String.Format(
					FunctionEvalExpression,
					EcmaScriptWriter.Serialize(argument));
			}
			else
			{
				// convert to literal expression
				value = EcmaScriptWriter.Serialize(argument);
			}

			return (value ?? String.Empty).Trim();
		}

		/// <summary>
		/// Controls the control rendering style
		/// </summary>
		/// <param name="writer"></param>
		private void RenderCustomControl(JsonWriter writer)
		{
			if (!this.ChildControlsSpecified)
			{
				this.RenderSimpleCustomControl(writer);
			}
			else if (String.IsNullOrEmpty(this.nameExpr))
			{
				this.RenderInlineCustomControl(writer);
			}
			else
			{
				this.RenderNestedCustomControl(writer);
			}
		}

		/// <summary>
		/// Renders a simple data binding call to a named template.
		/// </summary>
		/// <param name="writer"></param>
		private void RenderSimpleCustomControl(JsonWriter writer)
		{
			writer.TextWriter.Write(ControlSimpleFormat,
				this.nameExpr,
				this.dataExpr,
				this.indexExpr,
				this.countExpr);
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
				ControlInlineEndFormat,
				this.dataExpr,
				this.indexExpr,
				this.countExpr);
		}

		/// <summary>
		/// Renders a data binding call to a named template with a nested inline anonymous template.
		/// </summary>
		/// <param name="writer"></param>
		private void RenderNestedCustomControl(JsonWriter writer)
		{
			writer.TextWriter.Write(ControlStart);

			writer.Write(new EnumerableAdapter(this));

			writer.TextWriter.Write(
				ControlEndFormat,
				this.nameExpr,
				this.dataExpr,
				this.indexExpr,
				this.countExpr);
		}

		private void RenderPlaceholder(JsonWriter writer)
		{
			writer.TextWriter.Write(
				JbstCustomControl.PlaceholderStatement,
				this.dataExpr,
				this.indexExpr,
				this.countExpr);
		}

		#endregion Render Methods

		#region IJsonSerializable Members

		void IJsonSerializable.WriteJson(JsonWriter writer)
		{
			this.EnsureControl();

			switch (this.commandType)
			{
				case JbstCustomControl.ControlCommand:
				{
					this.RenderCustomControl(writer);
					break;
				}
				case JbstCustomControl.PlaceholderCommand:
				{
					this.RenderPlaceholder(writer);
					break;
				}
				default:
				{
					throw new NotSupportedException("Unknown JBST command: "+this.RawName);
				}
			}
		}

		void IJsonSerializable.ReadJson(JsonReader reader)
		{
			throw new NotImplementedException("JbstCustomControl deserialization is not implemented.");
		}

		#endregion IJsonSerializable Members

		#region Enumerable Adapter

		/// <summary>
		/// A simple adapter for exposing the IEnumerable interface without exposing the IJsonSerializable interface
		/// </summary>
		/// <remarks>
		/// In order to wrap the output of the JbstControl IJsonSerializable was required, but this takes
		/// precedent over the IEnumerable interface which is what should be rendered inside the wrapper.
		/// </remarks>
		private class EnumerableAdapter : IEnumerable
		{
			#region Fields

			private readonly IEnumerable enumerable;

			#endregion Fields

			#region Init

			/// <summary>
			/// Ctor
			/// </summary>
			/// <param name="enumerable"></param>
			public EnumerableAdapter(IEnumerable enumerable)
			{
				this.enumerable = enumerable;
			}

			#endregion Init

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.enumerable.GetEnumerator();
			}

			#endregion IEnumerable Members
		}

		#endregion Enumerable Adapter
	}
}
