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
using System.ComponentModel;
using System.Reflection;

namespace JsonFx.Serialization
{
	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter
	/// </summary>
	/// <remarks>
	/// Provides an extensibility point to control member naming at a very granular level.
	/// </remarks>
	public class DataNameResolver : IDataNameResolver
	{
		#region Name Resolution Methods

		/// <summary>
		/// Gets a value indicating if the property is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="isAnonymousType"></param>
		/// <returns></returns>
		/// <remarks>default implementation is must be read/write properties, or read-only anonymous</remarks>
		public virtual bool IsPropertyIgnored(PropertyInfo member, bool isAnonymousType)
		{
			return (!member.CanRead || (!member.CanWrite && !isAnonymousType));
		}

		/// <summary>
		/// Gets a value indicating if the field is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		/// <remarks>default implementation is must be public, non-readonly field</remarks>
		public virtual bool IsFieldIgnored(FieldInfo member)
		{
			return (!member.IsPublic || (member.IsStatic != member.DeclaringType.IsEnum) || member.IsInitOnly);
		}

		/// <summary>
		/// Gets a value indicating if the member is to be serialized.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		/// <remarks>default implementation checks for JsonIgnoreAttribute</remarks>
		public virtual bool IsIgnored(MemberInfo member)
		{
			if (TypeCoercionUtility.GetAttribute<DataIgnoreAttribute>(member) != null)
			{
				return true;
			}

			// TODO: extend here for JsonIgnoreAttribute, XmlIgnoreAttribute, DataContractAttribute, JsonSpecifiedPropertyAttribute
			//if (TypeCoercionUtility.GetAttribute<XmlIgnoreAttribute>(info) != null)
			//{
			//    return true;
			//}

			return false;
		}

		/// <summary>
		/// Determines if the property or field should not be serialized based upon its value.
		/// </summary>
		/// <param name="member"></param>
		/// <param name="target"></param>
		/// <param name="value"></param>
		/// <returns>if has a value equivalent to the DefaultValueAttribute</returns>
		/// <remarks>
		/// This is useful when default values need not be serialized.
		/// </remarks>
		public virtual bool IsValueIgnored(MemberInfo member, object target, object value)
		{
			Type objType = member.ReflectedType ?? member.DeclaringType;

			DataSpecifiedPropertyAttribute specifiedProperty = TypeCoercionUtility.GetAttribute<DataSpecifiedPropertyAttribute>(member);
			if (specifiedProperty != null && !String.IsNullOrEmpty(specifiedProperty.SpecifiedProperty))
			{
				PropertyInfo specProp = objType.GetProperty(specifiedProperty.SpecifiedProperty, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
				if (specProp != null)
				{
					// TODO: build GetterDelegate, cache under original member, execute
					object isSpecified = specProp.GetValue(target, null);
					if (isSpecified is Boolean && !Convert.ToBoolean(isSpecified))
					{
						return true;
					}
				}
			}

			//if (this.UseXmlSerializationAttributes)
			//{
			//    PropertyInfo specProp = objType.GetProperty(info.Name+"Specified");
			//    if (specProp != null)
			//    {
			// TODO: build GetterDelegate, cache under original member, execute
			//        object isSpecified = specProp.GetValue(obj, null);
			//        if (isSpecified is Boolean && !Convert.ToBoolean(isSpecified))
			//        {
			//            return true;
			//        }
			//    }
			//}

			return false;
		}

		/// <summary>
		/// Determines if the member value matches the DefaultValue attribute
		/// </summary>
		/// <param name="member"></param>
		/// <param name="value"></param>
		/// <returns>if has a value equivalent to the DefaultValueAttribute</returns>
		public virtual bool IsDefaultValue(MemberInfo member, object value)
		{
			DefaultValueAttribute attribute = TypeCoercionUtility.GetAttribute<DefaultValueAttribute>(member);
			if (attribute == null)
			{
				return false;
			}

			return (attribute.Value == value);
		}

		/// <summary>
		/// Gets the serialized name for the member.
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		public virtual string GetName(MemberInfo member)
		{
			DataNameAttribute attribute = TypeCoercionUtility.GetAttribute<DataNameAttribute>(member);

			// TODO: extend here for XmlNameAttribute, DataContractAttribute
			return (attribute != null) ? attribute.Name : null;
		}

		/// <summary>
		/// Gets the serialized name for the Enum value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public string GetName(Enum value)
		{
			Type type = value.GetType();

			string name = Enum.GetName(type, value);
			if (String.IsNullOrEmpty(name))
			{
				return null;
			}

			MemberInfo member = type.GetField(name);
			return this.GetName(member);
		}

		#endregion Name Resolution Methods
	}
}