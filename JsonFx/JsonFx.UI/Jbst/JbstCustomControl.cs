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

		public const string JbstPrefix = "jbst"+JbstCompiler.PrefixDelim;
		private const string PlaceholderCommand = "placeholder";

		private const string PlaceholderStatement =
			@"return this.$jbst?JsonML.BST(this.$jbst).dataBind({0},{1},{2}):"""";";

		private const string ControlCommand = "control";
		private const string ControlNameKey = "name";
		private const string ControlNameKeyAlt = JbstCustomControl.JbstPrefix+JbstCustomControl.ControlNameKey;
		private const string ControlDataKey = "data";
		private const string ControlDataKeyAlt = JbstCustomControl.JbstPrefix+JbstCustomControl.ControlDataKey;
		private const string ControlIndexKey = "index";
		private const string ControlIndexKeyAlt = JbstCustomControl.JbstPrefix+JbstCustomControl.ControlIndexKey;
		private const string ControlCountKey = "count";
		private const string ControlCountKeyAlt = JbstCustomControl.JbstPrefix+JbstCustomControl.ControlIndexKey;

		private const string ControlSimple =
			@"function(){{return JsonML.BST({0}).dataBind({1},{2},{3});}}";

		private const string ControlSimpleDebug =
			@"function() {{
				return JsonML.BST({0}).dataBind({1}, {2}, {3});
			}}";

		private const string ControlStart =
			@"function(){var t=JsonML.BST(";

		private const string ControlStartDebug =
			@"function() {
				var t = JsonML.BST(";

		private const string ControlEndFormat =
			@");t.prototype=this;return JsonML.BST({0}).dataBind({1},{2},{3},t);}}";

		private const string ControlEndFormatDebug =
			@");
				t.prototype = this;
				return JsonML.BST({0}).dataBind({1}, {2}, {3}, t);
			}}";

		private const string ControlInlineStart =
			@"function(){return JsonML.BST(";

		private const string ControlInlineStartDebug =
			@"function() {
				return JsonML.BST(";

		private const string ControlInlineEndFormat =
			@").dataBind({0},{1},{2});}}";

		private const string ControlInlineEndFormatDebug =
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
		private string controlName;
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

			#region Control Name

			// placeholder does not need a named JBST
			if (JbstCustomControl.ControlCommand.Equals(this.commandType, StringComparison.OrdinalIgnoreCase))
			{
				if (this.Attributes.ContainsKey(ControlNameKey))
				{
					this.controlName = this.Attributes[ControlNameKey] as string;
				}
				else if (this.Attributes.ContainsKey(ControlNameKeyAlt))
				{
					// backwards compatibility with "jbst:name"
					this.controlName = this.Attributes[ControlNameKeyAlt] as string;
				}
				this.controlName = JsonWriter.EnsureValidIdentifier(this.controlName, true, false);

				if (String.IsNullOrEmpty(this.controlName) && !this.ChildControlsSpecified)
				{
					throw new InvalidOperationException("JBST Control requires either a named template or an anonymous inline template.");
				}
			}

			#endregion Control Name

			#region Control Data

			object dataParam = null;
			if (this.Attributes.ContainsKey(ControlDataKey))
			{
				dataParam = this.Attributes[ControlDataKey];
			}
			else if (this.Attributes.ContainsKey(ControlDataKeyAlt))
			{
				// backwards compatibility with "jbst:data"
				dataParam = this.Attributes[ControlDataKeyAlt];
			}

			if (dataParam == null)
			{
				this.dataExpr = DefaultDataExpression;
			}
			else if (dataParam is JbstExpressionBlock)
			{
				// convert to inline expression
				this.dataExpr = ((JbstExpressionBlock)dataParam).Code;
			}
			else if (dataParam is JbstCodeBlock)
			{
				// convert to anonymous function expression
				this.dataExpr = String.Format(
					FunctionEvalExpression,
					JsonWriter.Serialize(dataParam));
			}
			else
			{
				// convert to literal expression
				this.dataExpr = JsonWriter.Serialize(dataParam);
			}
			this.dataExpr = this.dataExpr.Trim();

			#endregion Control Data

			#region Control Index

			object indexParam = null;
			if (this.Attributes.ContainsKey(ControlIndexKey))
			{
				indexParam = this.Attributes[ControlIndexKey];
			}
			else if (this.Attributes.ContainsKey(ControlIndexKeyAlt))
			{
				// backwards compatibility with "jbst:index"
				indexParam = this.Attributes[ControlIndexKeyAlt];
			}

			if (indexParam == null)
			{
				this.indexExpr = DefaultIndexExpression;
			}
			else if (indexParam is JbstExpressionBlock)
			{
				// convert to inline expression
				this.indexExpr = ((JbstExpressionBlock)indexParam).Code;
			}
			else if (indexParam is JbstCodeBlock)
			{
				// convert to anonymous function expression
				this.indexExpr = String.Format(
					FunctionEvalExpression,
					JsonWriter.Serialize(indexParam));
			}
			else
			{
				// convert to literal expression
				this.indexExpr = JsonWriter.Serialize(indexParam);
			}
			this.indexExpr = this.indexExpr.Trim();

			#endregion Control Index

			#region Control Count

			object countParam = null;
			if (this.Attributes.ContainsKey(ControlCountKey))
			{
				countParam = this.Attributes[ControlCountKey];
			}
			else if (this.Attributes.ContainsKey(ControlCountKeyAlt))
			{
				// backwards compatibility with "jbst:count"
				countParam = this.Attributes[ControlCountKeyAlt];
			}

			if (countParam == null)
			{
				this.countExpr = DefaultCountExpression;
			}
			else if (countParam is JbstExpressionBlock)
			{
				// convert to inline expression
				this.countExpr = ((JbstExpressionBlock)countParam).Code;
			}
			else if (countParam is JbstCodeBlock)
			{
				// convert to anonymous function expression
				this.countExpr = String.Format(
					FunctionEvalExpression,
					JsonWriter.Serialize(countParam));
			}
			else
			{
				// convert to literal expression
				this.countExpr = JsonWriter.Serialize(countParam);
			}
			this.countExpr = this.countExpr.Trim();

			#endregion Control Count

			this.Attributes.Clear();
			this.isProcessed = true;
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
			else if (String.IsNullOrEmpty(this.controlName))
			{
				this.RenderInlineCustomControl(writer);
			}
			else
			{
				RenderNestedCustomControl(writer);
			}
		}

		/// <summary>
		/// Renders a simple data binding call to a named template.
		/// </summary>
		/// <param name="writer"></param>
		private void RenderSimpleCustomControl(JsonWriter writer)
		{
			if (writer.PrettyPrint)
			{
				writer.TextWriter.Write(
					ControlSimpleDebug,
					this.controlName,
					this.dataExpr,
					this.indexExpr,
					this.countExpr);
			}
			else
			{
				writer.TextWriter.Write(
					ControlSimple,
					this.controlName,
					this.dataExpr,
					this.indexExpr,
					this.countExpr);
			}
		}

		/// <summary>
		/// Renders a data binding call to an inline anonymous template.
		/// </summary>
		/// <param name="writer"></param>
		private void RenderInlineCustomControl(JsonWriter writer)
		{
			if (writer.PrettyPrint)
			{
				writer.TextWriter.Write(ControlInlineStartDebug);
			}
			else
			{
				writer.TextWriter.Write(ControlInlineStart);
			}

			writer.Write(new EnumerableAdapter(this));

			if (writer.PrettyPrint)
			{
				writer.TextWriter.Write(
					ControlInlineEndFormatDebug,
					this.dataExpr,
					this.indexExpr,
					this.countExpr);
			}
			else
			{
				writer.TextWriter.Write(
					ControlInlineEndFormat,
					this.dataExpr,
					this.indexExpr,
					this.countExpr);
			}
		}

		/// <summary>
		/// Renders a data binding call to a named template with a nested inline anonymous template.
		/// </summary>
		/// <param name="writer"></param>
		private void RenderNestedCustomControl(JsonWriter writer)
		{
			if (writer.PrettyPrint)
			{
				writer.TextWriter.Write(ControlStartDebug);
			}
			else
			{
				writer.TextWriter.Write(ControlStart);
			}

			writer.Write(new EnumerableAdapter(this));

			if (writer.PrettyPrint)
			{
				writer.TextWriter.Write(
					ControlEndFormatDebug,
					this.controlName,
					this.dataExpr,
					this.indexExpr,
					this.countExpr);
			}
			else
			{
				writer.TextWriter.Write(
					ControlEndFormat,
					this.controlName,
					this.dataExpr,
					this.indexExpr,
					this.countExpr);
			}
		}

		private void RenderPlaceholder(JsonWriter writer)
		{
			string placeholder = String.Format(
					JbstCustomControl.PlaceholderStatement,
					this.dataExpr,
					this.indexExpr,
					this.countExpr);

			writer.Write(new JbstStatementBlock(placeholder));
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
			throw new NotImplementedException("JbstCustomControl deserialization is not yet implemented.");
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
