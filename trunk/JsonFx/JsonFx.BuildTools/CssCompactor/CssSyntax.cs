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
#endregion BuildTools License

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace BuildTools.CssCompactor
{
	#region Base Types

	/// <summary>
	/// http://www.w3.org/TR/css3-syntax/#style
	/// </summary>
	public abstract class CssObject
	{
		#region Methods

		public abstract void Write(TextWriter writer, CssCompactor.Options options);

		protected static bool IsPrettyPrint(CssCompactor.Options options)
		{
			return (options & CssCompactor.Options.PrettyPrint) > 0;
		}

		#endregion Methods

		#region Object Overrides

		public override string ToString()
		{
			StringWriter writer = new StringWriter();

			this.Write(writer, CssCompactor.Options.PrettyPrint);

			return writer.ToString();
		}

		#endregion Object Overrides
	}

	public interface ICssValue
	{
		#region Methods

		void Write(TextWriter writer, CssCompactor.Options options);

		#endregion Methods
	}

	public class CssString : CssObject, ICssValue
	{
		#region Fields

		private string value = null;

		#endregion Fields

		#region Init

		public CssString()
		{
		}

		#endregion Init

		#region Properties

		public virtual string Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		#endregion Properties

		#region Methods

		public override void Write(TextWriter writer, CssCompactor.Options options)
		{
			writer.Write(this.Value);
		}

		#endregion Methods
	}

	#endregion Base Types

	#region Grammar

	public class CssStyleSheet : CssObject
	{
		#region Fields

		private List<CssStatement> statements = new List<CssStatement>();

		#endregion Fields

		#region Properties

		public List<CssStatement> Statements
		{
			get { return this.statements; }
		}

		#endregion Properties

		#region Methods

		public override void Write(TextWriter writer, CssCompactor.Options options)
		{
			bool prettyPrint = IsPrettyPrint(options);

			foreach (CssStatement statement in this.statements)
			{
				statement.Write(writer, options);
				if (prettyPrint)
				{
					writer.WriteLine();
				}
			}
		}

		#endregion Methods
	}

	public abstract class CssStatement : CssObject
	{
	}

	public class CssAtRule : CssStatement, ICssValue
	{
		#region Fields

		private string ident = null;
		private string value = null;

		// replacing block with Statement list
		//private CssBlock block = null;
		private List<CssStatement> block = new List<CssStatement>();

		#endregion Fields

		#region Properties

		public string Ident
		{
			get { return this.ident; }
			set { this.ident = value; }
		}

		public string Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		// replacing block with Statement list
		//public CssBlock Block
		public List<CssStatement> Block
		{
			get { return this.block; }
			set { this.block = value; }
		}

		#endregion Properties

		#region Methods

		public override void Write(TextWriter writer, CssCompactor.Options options)
		{
			bool prettyPrint = IsPrettyPrint(options);

			writer.Write('@');
			writer.Write(this.ident);

			if (!String.IsNullOrEmpty(this.value))
			{
				writer.Write(' ');
				writer.Write(this.value);
			}

			if (this.block.Count > 0)
			{
				if (prettyPrint)
				{
					writer.WriteLine();
				}
				writer.Write('{');
				if (prettyPrint)
				{
					writer.WriteLine();
				}
				//this.block.Write(writer, options);
				foreach (CssStatement statement in this.block)
				{
					statement.Write(writer, options);
				}
				if (prettyPrint)
				{
					writer.WriteLine();
				}
				writer.Write('}');
				if (prettyPrint)
				{
					writer.WriteLine();
				}
			}
			else
			{
				writer.Write(';');
			}
		}

		#endregion Methods
	}

	public class CssBlock : CssValueList, ICssValue
	{
		#region Methods

		public override void Write(TextWriter writer, CssCompactor.Options options)
		{
			bool prettyPrint = IsPrettyPrint(options);

			writer.Write('{');
			if (prettyPrint)
			{
				writer.WriteLine();
			}
			base.Write(writer, options);
			if (prettyPrint)
			{
				writer.WriteLine();
			}
			writer.Write('}');
		}

		#endregion Methods
	}

	public class CssRuleSet : CssStatement
	{
		#region Fields

		private List<CssSelector> selectors = new List<CssSelector>();
		private List<CssDeclaration> declarations = new List<CssDeclaration>();

		#endregion Fields

		#region Properties

		public List<CssSelector> Selectors
		{
			get { return this.selectors; }
		}

		public List<CssDeclaration> Declarations
		{
			get { return this.declarations; }
		}

		#endregion Properties

		#region Methods

		public override void Write(TextWriter writer, CssCompactor.Options options)
		{
			bool prettyPrint = IsPrettyPrint(options);

			bool comma = false;

			foreach (CssString selector in this.Selectors)
			{
				if (comma)
				{
					writer.Write(",");
					if (prettyPrint)
					{
						writer.WriteLine();
					}
				}
				else
				{
					comma = true;
				}

				selector.Write(writer, options);
			}
			
			if (prettyPrint)
			{
				writer.WriteLine();
			}
			writer.Write("{");
			if (prettyPrint)
			{
				writer.WriteLine();
			}

			foreach (CssDeclaration dec in this.Declarations)
			{
				dec.Write(writer, options);
			}

			writer.Write("}");
			if (prettyPrint)
			{
				writer.WriteLine();
			}
		}

		#endregion Methods
	}

	public class CssSelector : CssString
	{
	}

	public class CssDeclaration : CssObject
	{
		#region Fields

		private string property = null;
		private CssValueList value = null;

		#endregion Fields

		#region Properties

		public string Property
		{
			get { return this.property; }
			set { this.property = value; }
		}

		public CssValueList Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		#endregion Properties

		#region Methods

		public override void Write(TextWriter writer, CssCompactor.Options options)
		{
			bool prettyPrint = IsPrettyPrint(options);
			if (prettyPrint)
			{
				writer.Write('\t');
			}
			writer.Write(this.Property);
			writer.Write(':');
			if (prettyPrint)
			{
				writer.Write(" ");
			}
			this.Value.Write(writer, options);
			writer.Write(";");
			if (prettyPrint)
			{
				writer.WriteLine();
			}
		}

		#endregion Methods
	}

	public class CssValueList : CssObject
	{
		#region Fields

		private List<ICssValue> values = new List<ICssValue>();

		#endregion Fields

		#region Properties

		public List<ICssValue> Values
		{
			get { return this.values; }
		}

		#endregion Properties

		#region Methods

		public override void Write(TextWriter writer, CssCompactor.Options options)
		{
			bool space = false;

			foreach (ICssValue value in this.Values)
			{
				if (space)
				{
					writer.Write(" ");
				}
				else
				{
					space = true;
				}

				value.Write(writer, options);
			}
		}

		#endregion Methods
	}

	#endregion Grammar
}
