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

		public const string JbstPrefix = "jbst"+JbstContainerControl.PrefixDelim;
		private const string PlaceholderCommand = "placeholder";

		private const string PlaceholderStatement =
			@"return (this.jbst instanceof JsonML.BST) ? this.jbst.dataBind(this.data, this.index) : """";";

		private const string ControlCommand = "control";
		private const string ControlNameKey = JbstCustomControl.JbstPrefix+"name";

		private const string ControlSimple =
			@"function(){{return {0}.dataBind(this.data,this.index);}}";

		private const string ControlSimpleDebug =
			@"function() {{
				return {0}.dataBind(this.data, this.index);
			}}";

		private const string ControlStart =
			@"function(){var t=new JsonML.BST(";

		private const string ControlStartDebug =
			@"function() {
				var t = new JsonML.BST(";

		private const string ControlEndFormat =
			@");t.prototype=this;return {0}.dataBind(this.data,this.index,t);}}";

		private const string ControlEndFormatDebug =
			@");
				t.prototype = this;
				return {0}.dataBind(this.data, this.index, t);
			}}";

		#endregion Constants

		#region Fields

		private JbstControl renderProxy = null;
		private string commandName = null;
		private string controlName = null;

		#endregion Fields

		#region Properties

		public override string RawName
		{
			get { return JbstCustomControl.JbstPrefix + this.commandName; }
		}

		#endregion Properties

		#region Factory Method

		public static JbstContainerControl Create(string rawName, string path)
		{
			JbstCustomControl control = new JbstCustomControl();

			string prefix = SplitPrefix(rawName, out control.commandName);

			switch (control.commandName.ToLowerInvariant())
			{
				case JbstCustomControl.PlaceholderCommand:
				{
					control.renderProxy = new JbstStatementBlock(JbstCustomControl.PlaceholderStatement, path);
					break;
				}
				case JbstCustomControl.ControlCommand:
				{
					control.renderProxy = null;
					break;
				}
				default:
				{
					throw new NotSupportedException("Unknown JBST command: "+control.commandName);
				}
			}

			return control;
		}

		#endregion Factory Method

		#region Render Methods

		private void EnsureControl()
		{
			if (!this.AttributesSpecified)
			{
				return;
			}

			if (this.Attributes.ContainsKey(ControlNameKey))
			{
				this.controlName = this.Attributes[ControlNameKey] as string;
				controlName = JbstCompiler.EnsureIdent(controlName);
				this.Attributes.Remove(ControlNameKey);
			}

			this.Attributes.Clear();
		}

		private void RenderCustomControl(JsonWriter writer)
		{
			this.EnsureControl();
			if (String.IsNullOrEmpty(this.controlName))
			{
				// TODO: handle this more gracefully
				return;
			}

			if (!this.ChildControlsSpecified)
			{
				if (writer.PrettyPrint)
				{
					writer.TextWriter.Write(ControlSimpleDebug, controlName);
				}
				else
				{
					writer.TextWriter.Write(ControlSimple, controlName);
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
				writer.TextWriter.Write(ControlEndFormatDebug, controlName);
			}
			else
			{
				writer.TextWriter.Write(ControlEndFormat, controlName);
			}
		}

		#endregion Render Methods

		#region IJsonSerializable Members

		void IJsonSerializable.WriteJson(JsonWriter writer)
		{
			if (this.renderProxy != null)
			{
				writer.Write(this.renderProxy);
				return;
			}

			this.RenderCustomControl(writer);
		}

		void IJsonSerializable.ReadJson(JsonReader reader)
		{
			throw new NotImplementedException("JbstCodeBlock deserialization is not yet implemented.");
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
