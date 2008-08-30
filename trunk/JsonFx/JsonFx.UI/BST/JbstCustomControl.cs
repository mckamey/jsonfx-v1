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

namespace JsonFx.JsonML.BST
{
	/// <summary>
	/// Internal representation of a nested JsonML+BST control.
	/// </summary>
	internal class JbstCustomControl : JbstControl, Json.IJsonSerializable
	{
		#region Constants

		// TODO: find out why content after this is being suppressed
		// TODO: add pretty printing
		private const string StartFormat = "function() {{ var t = new JsonML.BST(";
		private const string EndFormat = "); t.prototype = this; return ({0}).dataBind({{ \"jbst\": t, \"data\": this.data, \"index\": this.index }}); }}";
		public const string CustomControlPlaceholder = "if (this.data && this.data.jbst instanceof JsonML.BST) { return this.data.jbst.dataBind(this.data.data, this.data.item); }";

		#endregion Constants

		#region Fields

		private readonly JbstControl content;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="code"></param>
		public JbstCustomControl(string template)
			: base(template)
		{
			this.content = new JbstControl(String.Empty);
		}

		#endregion Init

		#region Properties

		public override JbstControlCollection ChildControls
		{
			get { return this.content.ChildControls; }
			set { this.content.ChildControls = value; }
		}

		#endregion Properties

		#region IJsonSerializable Members

		void JsonFx.Json.IJsonSerializable.WriteJson(JsonFx.Json.JsonWriter writer)
		{
			writer.TextWriter.Write(StartFormat, "");
			writer.Write(this.content);
			writer.TextWriter.Write(EndFormat, this.TagName);
		}

		void JsonFx.Json.IJsonSerializable.ReadJson(JsonFx.Json.JsonReader reader)
		{
			throw new NotImplementedException("JbstCodeBlock deserialization is not yet implemented.");
		}

		#endregion IJsonSerializable Members
	}
}
