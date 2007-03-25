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

		public abstract void Write(TextWriter writer, CssOptions options);

		protected static bool IsPrettyPrint(CssOptions options)
		{
			return (options & CssOptions.PrettyPrint) > 0;
		}

		#endregion Methods

		#region Object Overrides

		public override string ToString()
		{
			StringWriter writer = new StringWriter();

			this.Write(writer, CssOptions.PrettyPrint);

			return writer.ToString();
		}

		#endregion Object Overrides
	}

	public interface ICssValue
	{
		#region Methods

		void Write(TextWriter writer, CssOptions options);

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

		public CssString(string value)
		{
			this.value = value;
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

		public override void Write(TextWriter writer, CssOptions options)
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

		public override void Write(TextWriter writer, CssOptions options)
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
		private CssBlock block = null;

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

		public CssBlock Block
		{
			get { return this.block; }
			set { this.block = value; }
		}

		#endregion Properties

		#region Methods

		public override void Write(TextWriter writer, CssOptions options)
		{
			bool prettyPrint = IsPrettyPrint(options);

			writer.Write('@');
			writer.Write(this.ident);

			writer.Write(' ');
			writer.Write(this.value);

			if (this.block != null)
			{
				if (prettyPrint)
				{
					writer.WriteLine();
				}
				this.block.Write(writer, options);
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

	public class CssBlock : CssObject, ICssValue
	{
		#region Fields

		private string value = null;

		#endregion Fields

		#region Properties

		public string Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		#endregion Properties

		#region Methods

		public override void Write(TextWriter writer, CssOptions options)
		{
			bool prettyPrint = IsPrettyPrint(options);

			writer.Write('{');
			if (prettyPrint)
			{
				writer.WriteLine();
			}
			writer.Write(this.value);
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

		private CssSelector selector = new CssSelector();
		private List<CssDeclaration> declarations = new List<CssDeclaration>();

		#endregion Fields

		#region Properties

		public CssSelector Selector
		{
			get { return this.selector; }
		}

		public List<CssDeclaration> Declarations
		{
			get { return this.declarations; }
		}

		#endregion Properties

		#region Methods

		public override void Write(TextWriter writer, CssOptions options)
		{
			bool prettyPrint = IsPrettyPrint(options);

			this.Selector.Write(writer, options);
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
		private CssValue value = null;

		#endregion Fields

		#region Properties

		public string Property
		{
			get { return this.property; }
			set { this.property = value; }
		}

		public CssValue Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		#endregion Properties

		#region Methods

		public override void Write(TextWriter writer, CssOptions options)
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

	public class CssValue : CssObject
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

		public override void Write(TextWriter writer, CssOptions options)
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

	#region Options

	[Flags]
	public enum CssOptions
	{
		None=0x00,
		PrettyPrint=0x01
	}

	#endregion Options
}
