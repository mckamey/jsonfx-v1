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
			@"return (this.jbst instanceof JsonML.BST) ? this.jbst.dataBind({0}, {1}) : """";";

		private const string ControlCommand = "control";
		private const string ControlNameKey = "name";
		private const string ControlNameKeyAlt = JbstCustomControl.JbstPrefix+JbstCustomControl.ControlNameKey;
		private const string ControlDataKey = "data";
		private const string ControlDataKeyAlt = JbstCustomControl.JbstPrefix+JbstCustomControl.ControlDataKey;
		private const string ControlIndexKey = "index";
		private const string ControlIndexKeyAlt = JbstCustomControl.JbstPrefix+JbstCustomControl.ControlIndexKey;

		private const string ControlSimple =
			@"function(){{return {0}.dataBind({1},{2});}}";

		private const string ControlSimpleDebug =
			@"function() {{
				return {0}.dataBind({1}, {2});
			}}";

		private const string ControlStart =
			@"function(){var t=new JsonML.BST(";

		private const string ControlStartDebug =
			@"function() {
				var t = new JsonML.BST(";

		private const string ControlEndFormat =
			@");t.prototype=this;return {0}.dataBind({1},{2},t);}}";

		private const string ControlEndFormatDebug =
			@");
				t.prototype = this;
				return {0}.dataBind({1}, {2}, t);
			}}";

		private const string FunctionEvalExpression = "({0})()";

		private const string DefaultDataExpression = "this.data";
		private const string DefaultIndexExpression = "this.index";

		#endregion Constants

		#region Fields

		private readonly string commandName;
		private string controlName;
		private string dataExpr;
		private string indexExpr;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="commandName"></param>
		public JbstCustomControl(string commandName)
		{
			this.commandName = (commandName == null) ?
				String.Empty :
				commandName.ToLowerInvariant();
		}

		#endregion Init

		#region Properties

		[JsonName("rawName")]
		public override string RawName
		{
			get { return JbstCustomControl.JbstPrefix + this.commandName; }
		}

		#endregion Properties

		#region Render Methods

		private void EnsureControl()
		{
			#region Control Name

			// placeholder does not need a named JBST
			if (JbstCustomControl.ControlCommand.Equals(this.commandName, StringComparison.OrdinalIgnoreCase))
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
				this.controlName = JbstCompiler.EnsureIdent(this.controlName);
			}

			#endregion Control Name

			#region Control Data

			object dataExpr = null;
			if (this.Attributes.ContainsKey(ControlDataKey))
			{
				dataExpr = this.Attributes[ControlDataKey];
			}
			else if (this.Attributes.ContainsKey(ControlDataKeyAlt))
			{
				// backwards compatibility with "jbst:data"
				dataExpr = this.Attributes[ControlDataKeyAlt];
			}

			if (dataExpr == null)
			{
				this.dataExpr = DefaultDataExpression;
			}
			else if (dataExpr is JbstExpressionBlock)
			{
				// convert to inline expression
				this.dataExpr = ((JbstExpressionBlock)dataExpr).Code;
			}
			else if (dataExpr is JbstCodeBlock)
			{
				// convert to anonymous function expression
				this.dataExpr = String.Format(
					FunctionEvalExpression,
					JsonWriter.Serialize(dataExpr));
			}
			else
			{
				// convert to literal expression
				this.dataExpr = JsonWriter.Serialize(dataExpr);
			}

			#endregion Control Data

			#region Control Index

			object indexExpr = null;
			if (this.Attributes.ContainsKey(ControlIndexKey))
			{
				indexExpr = this.Attributes[ControlIndexKey];
			}
			else if (this.Attributes.ContainsKey(ControlIndexKeyAlt))
			{
				// backwards compatibility with "jbst:index"
				indexExpr = this.Attributes[ControlIndexKeyAlt];
			}

			if (indexExpr == null)
			{
				this.indexExpr = DefaultIndexExpression;
			}
			else if (indexExpr is JbstExpressionBlock)
			{
				// convert to inline expression
				this.indexExpr = ((JbstExpressionBlock)indexExpr).Code;
			}
			else if (indexExpr is JbstCodeBlock)
			{
				// convert to anonymous function expression
				this.indexExpr = String.Format(
					FunctionEvalExpression,
					JsonWriter.Serialize(indexExpr));
			}
			else
			{
				// convert to literal expression
				this.indexExpr = JsonWriter.Serialize(indexExpr);
			}

			#endregion Control Index

			this.Attributes.Clear();
		}

		private void RenderCustomControl(JsonWriter writer)
		{
			if (!this.ChildControlsSpecified)
			{
				if (writer.PrettyPrint)
				{
					writer.TextWriter.Write(ControlSimpleDebug, this.controlName, this.dataExpr, this.indexExpr);
				}
				else
				{
					writer.TextWriter.Write(ControlSimple, this.controlName, this.dataExpr, this.indexExpr);
				}
				return;
			}

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
				writer.TextWriter.Write(ControlEndFormatDebug, this.controlName, this.dataExpr, this.indexExpr);
			}
			else
			{
				writer.TextWriter.Write(ControlEndFormat, this.controlName, this.dataExpr, this.indexExpr);
			}
		}

		private void RenderPlaceholder(JsonWriter writer)
		{
			string placeholder = String.Format(
					JbstCustomControl.PlaceholderStatement,
					this.dataExpr,
					this.indexExpr
				);

			writer.Write(new JbstStatementBlock(placeholder));
		}

		#endregion Render Methods

		#region IJsonSerializable Members

		void IJsonSerializable.WriteJson(JsonWriter writer)
		{
			this.EnsureControl();

			switch (this.commandName)
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
