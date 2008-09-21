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
using System.Collections;

using JsonFx.Json;

namespace JsonFx.UI.Jbst
{
	/// <summary>
	/// Internal representation of a nested JsonML+BST control.
	/// </summary>
	internal class JbstCustomControl : JbstControl, IJsonSerializable
	{
		#region Constants

		public const string JbstPrefix = "jbst"+JbstControl.PrefixDelim;

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
			@");t.prototype=this;t.args={1};return {0}.dataBind(this.data,this.index,t,{1});}}";

		private const string ControlEndFormatDebug =
			@");
				t.prototype = this;
				t.args = {1};
				return {0}.dataBind(this.data, this.index, t);
			}}";

		public const string CustomControlPlaceholder =
			@"if(this.jbst&&""function""===typeof this.jbst.databind){return this.jbst.dataBind(this.data,this.index);}else{return this.data;}";

		public const string CustomControlPlaceholderDebug =
			@"if (this.jbst && ""function"" === typeof this.jbst.databind) {
				return this.jbst.dataBind(this.data, this.index);
			} else {
				return this.data;
			}";

		#endregion Constants

		#region Fields

		private string controlName;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="controlName"></param>
		public JbstCustomControl(string controlName)
			: base(controlName)
		{
			this.ControlName = JbstCompiler.EnsureIdent(this.TagName);
			this.TagName = String.Empty;
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the identifier of this control
		/// </summary>
		protected string ControlName
		{
			get { return this.controlName; }
			set { this.controlName = value; }
		}

		public override string RawName
		{
			get
			{
				if (String.IsNullOrEmpty(this.Prefix))
				{
					return this.ControlName;
				}
				return this.Prefix + PrefixDelim + this.ControlName;
			}
		}

		#endregion Properties

		#region IJsonSerializable Members

		void JsonFx.Json.IJsonSerializable.WriteJson(JsonFx.Json.JsonWriter writer)
		{
			if (!this.ChildControlsSpecified)
			{
				if (writer.PrettyPrint)
				{
					writer.TextWriter.Write(ControlSimpleDebug, this.ControlName);
				}
				else
				{
					writer.TextWriter.Write(ControlSimple, this.ControlName);
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

			// TODO: flesh out story for nested template args
			string args = JsonWriter.Serialize(this.Attributes);
			this.Attributes.Clear();

			writer.Write(new EnumerableAdapter(this));

			if (writer.PrettyPrint)
			{
				writer.TextWriter.Write(ControlEndFormatDebug, this.ControlName, args);
			}
			else
			{
				writer.TextWriter.Write(ControlEndFormat, this.ControlName, args);
			}
		}

		void JsonFx.Json.IJsonSerializable.ReadJson(JsonFx.Json.JsonReader reader)
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
