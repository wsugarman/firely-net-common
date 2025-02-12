﻿/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hl7.Fhir.Utility
{
#if NET40
    public class TypeInfoPolyfill
    {
        public Type Type;

        public TypeInfoPolyfill(Type type)
        {
            Type = type;
        }

        public bool IsValueType
        {
            get
            {
                return Type.IsValueType;
            }
        }

        public bool IsGenericType
        {
            get
            {
                return Type.IsGenericType;
            }
        }

        public bool IsGenericTypeDefinition
        {
            get
            {
                return Type.IsGenericTypeDefinition;
            }
        }

        public bool ContainsGenericParameters
        {
            get
            {
                return Type.ContainsGenericParameters;
            }
        }

        public bool IsInterface
        {
            get
            {
                return Type.IsInterface;
            }
        }

        public Type[] ImplementedInterfaces
        {
            get
            {
                return Type.GetInterfaces();
            }
        }

        public FieldInfo[] DeclaredFields
        {
            get
            {
                return Type.GetFields(BindingFlags.Public | BindingFlags.Static);
            }
        }

        public Type[] GenericTypeParameters
        {
            get
            {
                return Type.GetGenericArguments();
            }
        }

        public Type[] GenericTypeArguments
        {
            get
            {
                return Type.GetGenericArguments();
            }
        }

        public FieldInfo GetDeclaredField(string fieldName)
        {
            return DeclaredFields.SingleOrDefault(f => f.Name == fieldName);
        }

        public bool IsEnum
        {
            get
            {
                return Type.IsEnum;
            }
        }

        public bool IsAbstract
        {
            get
            {
                return Type.IsAbstract;
            }
        }

        public Type BaseType
        {
            get
            {
                return Type.BaseType;
            }
        }

        public Assembly Assembly
        {
            get
            {
                return Type.Assembly;
            }
        }

        public bool IsAssignableFrom(TypeInfoPolyfill otherType)
        {
            return Type.IsAssignableFrom(otherType.Type);
        }

        public T GetCustomAttribute<T>() where T : Attribute
        {
            return (T)Type.GetCustomAttributes(typeof(T), true).FirstOrDefault();
        }

        public IEnumerable<T> GetCustomAttributes<T>() where T : Attribute
        {
            return Type.GetCustomAttributes(typeof(T), true).Cast<T>();
        }

        public bool IsDefined(Type type, bool inherit)
        {
            return Type.IsDefined(type, inherit);
        }

        public bool IsClass
        {
            get
            {
                return Type.IsClass;
            }
        }
    }

    public static class Net40TypeExtensions
    {
        public static TypeInfoPolyfill GetTypeInfo(this Type type)
        {
            return new TypeInfoPolyfill(type);
        }

        public static PropertyInfo GetRuntimeProperty(this Type type, string propertyName)
        {
            return type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        }

        public static T GetCustomAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return (T)memberInfo.GetCustomAttributes(typeof(T), true).FirstOrDefault();
        }

        public static T GetCustomAttribute<T>(this Assembly assembly) where T : Attribute
        {
            return (T)assembly.GetCustomAttributes(typeof(T), true).FirstOrDefault();
        }

        public static Type[] GetExportedTypes(this Assembly assembly)
        {
            return assembly.GetTypes().Where(t => t.IsVisible).ToArray();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return memberInfo.GetCustomAttributes(typeof(T), true).Cast<T>();
        }
    }
#endif

    public static class ReflectionHelper
    {
        public static bool IsAValueType(this Type t)
        {
            return t.GetTypeInfo().IsValueType;
        }

        public static bool CanBeTreatedAsType(this Type currentType, Type typeToCompareWith)
        {
            // Always return false if either Type is null
            if (currentType == null || typeToCompareWith == null)
                return false;

            // Return the result of the assignability test
            return typeToCompareWith.GetTypeInfo().IsAssignableFrom(currentType.GetTypeInfo());
        }


        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        public static T GetAttributeOnEnum<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetTypeInfo().GetDeclaredField(enumVal.ToString());
            var attributes = memInfo.GetCustomAttributes(typeof(T), false);

            return (T)attributes.FirstOrDefault();
        }

        public static PropertyInfo FindProperty(Type t, string name) =>
            t.GetTypeInfo().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// Returns all public, non-static properties for the given type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> FindPublicProperties(Type t)
        {
            if (t == null) throw Error.ArgumentNull("t");

            return t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// Returns true if the type is a primitive value type that has a parameterless public constructor.
        /// </summary>
        public static bool HasDefaultPublicConstructor(Type t) =>
            t.GetTypeInfo().IsValueType || GetDefaultPublicConstructor(t) != null;

        internal static ConstructorInfo GetDefaultPublicConstructor(Type t)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            return t.GetTypeInfo().GetConstructors(bindingFlags).SingleOrDefault(c => !c.GetParameters().Any());
        }

        /// <summary>
        /// Returns true if the type is a <see cref="Nullable{T}"/>.
        /// </summary>
        public static bool IsNullableType(Type type) => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        /// <summary>
        /// If the given type is a <see cref="Nullable{T}"/>, this function will return T.
        /// </summary>
        public static Type GetNullableArgument(Type type) =>
            IsNullableType(type)
                ? type.GetTypeInfo().GenericTypeArguments[0]
                : throw Error.Argument("type", "Type {0} is not a Nullable<T>".FormatWith(type.Name));

        /// <summary>
        /// Returns true if the given type is a .NET2.0+ typed collection.
        /// </summary>
        public static bool IsTypedCollection(Type type)
        {
            return type.IsArray || ImplementsGenericDefinition(type, typeof(ICollection<>));
        }


        public static IList CreateGenericList(Type itemType)
        {
            return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
        }


        public static bool IsClosedGenericType(Type type) => type.GetTypeInfo().IsGenericType && !type.GetTypeInfo().ContainsGenericParameters;


        public static bool IsOpenGenericTypeDefinition(Type type) => type.GetTypeInfo().IsGenericTypeDefinition;

        public static bool IsConstructedFromGenericTypeDefinition(Type type, Type genericBase)
        {
            return type.GetGenericTypeDefinition() == genericBase;
        }

        /// <summary>
        /// Gets the type of the typed collection's items.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The type of the typed collection's items.</returns>
        public static Type GetCollectionItemType(Type type)
        {
            if (type == null) throw Error.ArgumentNull("type");

            if (type.IsArray)
            {
                return type.GetElementType();
            }
            else if (ImplementsGenericDefinition(type, typeof(ICollection<>), out Type genericListType))
            {
                //EK: If I look at ImplementsGenericDefinition, I don't think this can actually occur.
                //if (genericListType.IsGenericTypeDefinition)
                //throw Error.Argument("type", "Type {0} is not a collection.", type.Name);

                return genericListType.GetTypeInfo().GenericTypeArguments[0];
            }
            else if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                return null;
            }
            else
            {
                throw Error.Argument("type", "Type {0} is not a collection.".FormatWith(type.Name));
            }
        }
        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition) =>
            ImplementsGenericDefinition(type, genericInterfaceDefinition, out _);

        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, out Type implementingType)
        {
            if (type == null) throw Error.ArgumentNull("type");
            if (genericInterfaceDefinition == null) throw Error.ArgumentNull("genericInterfaceDefinition");

            if (!genericInterfaceDefinition.GetTypeInfo().IsInterface || !genericInterfaceDefinition.GetTypeInfo().IsGenericTypeDefinition)
                throw Error.Argument("genericInterfaceDefinition", "'{0}' is not a generic interface definition.".FormatWith(genericInterfaceDefinition.Name));

            if (type.GetTypeInfo().IsInterface)
            {
                if (type.GetTypeInfo().IsGenericType)
                {
                    Type interfaceDefinition = type.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = type;
                        return true;
                    }
                }
            }

            foreach (Type i in type.GetTypeInfo().ImplementedInterfaces)
            {
                if (i.GetTypeInfo().IsGenericType)
                {
                    Type interfaceDefinition = i.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = i;
                        return true;
                    }
                }
            }

            implementingType = null;
            return false;
        }

        public static bool IsEnum(this Type t) => t.GetTypeInfo().IsEnum;

#if NET40
        public static T GetAttribute<T>(TypeInfoPolyfill typeInfo) where T : Attribute => typeInfo.GetCustomAttribute<T>();
        public static IEnumerable<T> GetAttributes<T>(TypeInfoPolyfill typeInfo) where T : Attribute => typeInfo.GetCustomAttributes<T>();
#endif

        public static T GetAttribute<T>(MemberInfo member) where T : Attribute => member.GetCustomAttribute<T>();

        public static IEnumerable<Attribute> GetAttributes(MemberInfo member) => member.GetCustomAttributes();

        public static IEnumerable<Attribute> GetAttributes(Type type) => type.GetTypeInfo().GetCustomAttributes();

        public static IEnumerable<T> GetAttributes<T>(MemberInfo member) where T : Attribute
            => member.GetCustomAttributes<T>();


        internal static IEnumerable<FieldInfo> FindEnumFields(Type t) => t.GetTypeInfo().DeclaredFields.Where(a => a.IsPublic && a.IsStatic);

        public static bool IsRepeatingElement(object value) => IsRepeatingElement(value, out _);

        public static bool IsRepeatingElement(object value, out ICollection element)
        {
            element = value as ICollection;
            return element is not null && !element.GetType().IsArray;
        }

        public static bool IsArray(object value) => value.GetType().IsArray;

        public static string PrettyTypeName(Type t)
        {
            // http://stackoverflow.com/questions/1533115/get-generictype-name-in-good-format-using-reflection-on-c-sharp#answer-25287378 
            return t.GetTypeInfo().IsGenericType ? string.Format(
                "{0}<{1}>", t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.CurrentCulture)),
                string.Join(", ", t.GetTypeInfo().GenericTypeParameters.ToList().Select(PrettyTypeName)))
            : t.Name;
        }
    }
}
