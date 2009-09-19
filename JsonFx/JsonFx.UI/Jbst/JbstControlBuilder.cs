using System;
using System.Collections;
using System.CodeDom;
using System.Web.UI;

namespace JsonFx.UI.Jbst
{
	public class JbstControlBuilder :
		System.Web.UI.ControlBuilder
	{
		#region ControlBuilder Members

		public override bool AllowWhitespaceLiterals()
		{
			return base.AllowWhitespaceLiterals();
		}

		public override void AppendLiteralString(string s)
		{
			base.AppendLiteralString(s);
		}

		public override void AppendSubBuilder(ControlBuilder subBuilder)
		{
			base.AppendSubBuilder(subBuilder);
		}

		public override Type BindingContainerType
		{
			get
			{
				return base.BindingContainerType;
			}
		}

		public override object BuildObject()
		{
			return base.BuildObject();
		}

		public override void CloseControl()
		{
			base.CloseControl();
		}

#if !__MonoCS__
// remove for Mono Framework
		public override Type DeclareType
		{
			get { return base.DeclareType; }
		}
#endif

		public override Type GetChildControlType(string tagName, IDictionary attribs)
		{
			return base.GetChildControlType(tagName, attribs);
		}

		public override bool HasBody()
		{
			return base.HasBody();
		}

		public override bool HtmlDecodeLiterals()
		{
			return base.HtmlDecodeLiterals();
		}

		public override void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string id, IDictionary attribs)
		{
			base.Init(parser, parentBuilder, type, tagName, id, attribs);
		}

		public override bool NeedsTagInnerText()
		{
			return base.NeedsTagInnerText();
		}

		public override void OnAppendToParentBuilder(ControlBuilder parentBuilder)
		{
			base.OnAppendToParentBuilder(parentBuilder);
		}

		public override void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit, CodeTypeDeclaration baseType, CodeTypeDeclaration derivedType, CodeMemberMethod buildMethod, CodeMemberMethod dataBindingMethod)
		{
			base.ProcessGeneratedCode(codeCompileUnit, baseType, derivedType, buildMethod, dataBindingMethod);
		}

		public override void SetTagInnerText(string text)
		{
			base.SetTagInnerText(text);
		}

		#endregion ControlBuilder Members
	}
}
