﻿/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification;
using Hl7.Fhir.Utility;
using Hl7.Fhir.Validation;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Hl7.Fhir.Introspection
{
    [System.Diagnostics.DebuggerDisplay(@"\{Name={Name} ElementType={ElementType.Name}}")]
    public class PropertyMapping : IElementDefinitionSummary
    {
        private PropertyMapping()
        {
            // no public constructors
        }

        public string Name { get; internal set; }

        public bool IsCollection { get; internal set; }

        /// <summary>
        /// The element is of an atomic .NET type, not a FHIR generated POCO.
        /// </summary>
        public bool IsPrimitive { get; private set; }

        /// <summary>
        /// The element is a primitive (<seealso cref="IsPrimitive"/>) and 
        /// represents the primitive `value` attribute/property in the FHIR serialization.
        /// </summary>
        public bool RepresentsValueElement { get; private set; }

        public bool InSummary { get; private set; }
        public bool IsMandatoryElement { get; private set; }

        /// <summary>
        /// The native type of the element.
        /// </summary>
        /// <remarks>If the element is a collection or is nullable, this reflects the
        /// collection item or the type that is made nullable respectively.
        /// </remarks>
        public Type ImplementingType { get; private set; }

        /// <summary>
        /// The native type of the element.
        /// </summary>
        [Obsolete("This element had a different name in R3 and R4. Please use ImplementingType from now on.")]
        public Type ElementType
        {
            get => ImplementingType;
            set => ImplementingType = value;
        }

        public int Order { get; private set; }

        public XmlRepresentation SerializationHint { get; private set; }

        /// <summary>
        /// Specifies whether this element contains a choice (either a choice element or a contained resource)
        /// </summary>
        /// <remarks>In the case of a DataChoice, these elements have names ending in [x] in the StructureDefinition
        /// and allow a (possibly restricted) set of types to be used. These are reflected
        /// in the <see cref="FhirType"/> property.</remarks>
        public ChoiceType Choice { get; private set; }

        /// <summary>
        /// This element is a polymorphic Resource, any resource is allowed here.
        /// </summary>
        /// <remarks>These are elements like DomainResource.contained, Parameters.resource etc.</remarks>
        [Obsolete("This property is never initialized and its value will always be false.")]
        public bool IsResourceChoice { get; private set; }

        /// <summary>
        /// The list of possible FHIR types for this element, represented as native types.
        /// </summary>
        /// <remark> <para>
        /// These are the defined (choice) types for this element as specified in the
        /// FHIR data definitions. It is derived from the actual type in the POCO class and 
        /// the [AllowedTypes] attribute and may by a [DeclaredTypes] attribute.
        /// </para>
        /// <para>
        /// May be a non-FHIR .NET primitive type for value elements of
        /// primitive FHIR datatypes (e.g. FhirBoolean.Value) or other primitive
        /// attributes (e.g. Extension.url).
        /// </para>
        /// </remark>
        public Type[] FhirType { get; private set; }        // may be multiple if this is a choice

        /// <summary>
        /// True when the element is of type '*', e.g. Extension.value[x]. Any type is allowed.
        /// </summary>
        //public bool IsOpen { get; private set; }

        private PropertyInfo _propInfo;
        private FhirRelease _createdVersion;

        public ClassMapping Parent { get; private set; }

        [Obsolete("Use TryCreate() instead.")]
        public static PropertyMapping Create(PropertyInfo prop, FhirRelease version = (FhirRelease)int.MaxValue)
            => TryCreate(prop, out var mapping, version) ? mapping : null;

        public static bool TryCreate(PropertyInfo prop, out PropertyMapping result, FhirRelease version) =>
            TryCreate(prop, out result, version, parent: null);

        public static bool TryCreate(PropertyInfo prop, out PropertyMapping result, FhirRelease version, ClassMapping parent)
        {
            if (prop == null) throw Error.ArgumentNull(nameof(prop));
            result = default;

            // If there is no [FhirElement] on the property, skip it
            var elementAttr = ClassMapping.GetAttribute<FhirElementAttribute>(prop, version);
            if (elementAttr == null) return false;

            // If there is an explicit [NotMapped] on the property, skip it
            // (in combination with `Since` useful to remove a property from the serialization)
            var notmappedAttr = ClassMapping.GetAttribute<NotMappedAttribute>(prop, version);
            if (notmappedAttr != null) return false;

            result = new PropertyMapping
            {
                Parent = parent,
                Name = elementAttr.Name,
                InSummary = elementAttr.InSummary,
                Choice = elementAttr.Choice,
                SerializationHint = elementAttr.XmlSerialization,
                Order = elementAttr.Order,
                _propInfo = prop,
                _createdVersion = version
            };

            var cardinalityAttr = ClassMapping.GetAttribute<CardinalityAttribute>(prop, version);
            result.IsMandatoryElement = cardinalityAttr != null ? cardinalityAttr.Min > 0 : false;

            // We broadly use .IsArray here - this means arrays in POCOs cannot be used to represent
            // FHIR repeating elements. If we would allow this, we'd also have stuff like `string` and binary
            // data as repeating element, and would need to exclude these exceptions on a case by case basis.
            // This is pretty ugly, so we prefer to not support arrays - you should use lists instead.
            result.IsCollection = ReflectionHelper.IsTypedCollection(prop.PropertyType) && !prop.PropertyType.IsArray;

            // Get to the actual (native) type representing this element
            result.ImplementingType = prop.PropertyType;
            if (result.IsCollection) result.ImplementingType = ReflectionHelper.GetCollectionItemType(prop.PropertyType);
            if (ReflectionHelper.IsNullableType(result.ImplementingType)) result.ImplementingType = ReflectionHelper.GetNullableArgument(result.ImplementingType);
            result.IsPrimitive = isAllowedNativeTypeForDataTypeValue(result.ImplementingType);

            // Determine the .NET type that represents the FHIR type for this element.
            // This is normally just the ImplementingType itself, but can be overridden
            // with the [DeclaredType] attribute.
            var declaredType = ClassMapping.GetAttribute<DeclaredTypeAttribute>(prop, version);
            var fhirType = declaredType?.Type ?? result.ImplementingType;

            // The [AllowedElements] attribute can specify a set of allowed types
            // for this element. Take this list as the declared list of FHIR types.
            // If not present assume this is the implementing FHIR type above
            var allowedTypes = ClassMapping.GetAttribute<AllowedTypesAttribute>(prop, version);

            result.FhirType = allowedTypes?.Types?.Any() == true ?
                allowedTypes.Types : new[] { fhirType };

            if (result.FhirType == null || !result.FhirType.Any())
                throw new InvalidOperationException();

            // Check wether this property represents a native .NET type
            // marked to receive the class' primitive value in the fhir serialization
            // (e.g. the value from the Xml 'value' attribute or the Json primitive member value)
            if (result.IsPrimitive) result.RepresentsValueElement = isPrimitiveValueElement(elementAttr, prop);

            return true;
        }

        private static bool isPrimitiveValueElement(FhirElementAttribute valueElementAttr, PropertyInfo prop)
        {
            var isValueElement = valueElementAttr != null && valueElementAttr.IsPrimitiveValue;

            if (isValueElement && !isAllowedNativeTypeForDataTypeValue(prop.PropertyType))
                throw Error.Argument(nameof(prop), "Property {0} is marked for use as a primitive element value, but its .NET type ({1}) " +
                    "is not supported by the serializer.".FormatWith(buildQualifiedPropName(prop), prop.PropertyType.Name));

            return isValueElement;

        }

        private static string buildQualifiedPropName(PropertyInfo p) => p.DeclaringType.Name + "." + p.Name;

        private static bool isAllowedNativeTypeForDataTypeValue(Type type)
        {
            // Special case, allow Nullable<enum>
            if (ReflectionHelper.IsNullableType(type))
                type = ReflectionHelper.GetNullableArgument(type);

            return type.IsEnum() ||
                    PrimitiveTypeConverter.CanConvert(type);
        }

        internal Func<object, object> Getter
        {
            get
            {
#if USE_CODE_GEN
                LazyInitializer.EnsureInitialized(ref _getter, () => _propInfo.GetValueGetter());
#else
                LazyInitializer.EnsureInitialized(ref _getter, () => instance => _propInfo.GetValue(instance, null));
#endif
                return _getter;
            }
        }

        private Func<object, object> _getter;

        internal Action<object, object> Setter
        {
            get
            {
#if USE_CODE_GEN
                LazyInitializer.EnsureInitialized(ref _setter, () => _propInfo.GetValueSetter());
#else
                LazyInitializer.EnsureInitialized(ref _setter, () => (instance, value) => _propInfo.SetValue(instance, value, null));
#endif
                return _setter;
            }
        }

        string IElementDefinitionSummary.ElementName => this.Name;

        bool IElementDefinitionSummary.IsCollection => this.IsCollection;

        bool IElementDefinitionSummary.IsRequired => this.IsMandatoryElement;

        bool IElementDefinitionSummary.InSummary => this.InSummary;

        bool IElementDefinitionSummary.IsChoiceElement => this.Choice == ChoiceType.DatatypeChoice;

        bool IElementDefinitionSummary.IsResource => this.Choice == ChoiceType.ResourceChoice;

        string IElementDefinitionSummary.DefaultTypeName => null;

        ITypeSerializationInfo[] IElementDefinitionSummary.Type
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _types, buildTypes);
                return _types;
            }
        }

        private ITypeSerializationInfo[] _types;

        string IElementDefinitionSummary.NonDefaultNamespace => null;

        XmlRepresentation IElementDefinitionSummary.Representation =>
            SerializationHint != XmlRepresentation.None ?
            SerializationHint : XmlRepresentation.XmlElement;

        int IElementDefinitionSummary.Order => Order;

        private Action<object, object> _setter;


        public object GetValue(object instance) => Getter(instance);

        public void SetValue(object instance, object value) => Setter(instance, value);

        private ITypeSerializationInfo[] buildTypes()
        {
            _ = ClassMapping.TryGetMappingForType(FhirType[0], _createdVersion, out var elementTypeMapping);

            if (elementTypeMapping.IsNestedType)
            {
                var info = elementTypeMapping;
                return new ITypeSerializationInfo[] { info };
            }
            else if (this.IsPrimitive)
            {
                // Backwards compat hack: the primitives (since .value is never queried, this
                // means Element.id, Narrative.div and Extension.url) should be returned as FHIR type names, not
                // system (CQL) type names.
                var bwcompatType = Name switch
                {
                    "url" => "uri",
                    "id" => "string",
                    "div" => "xhtml",
                    _ => throw new NotSupportedException($"Encountered unexpected primitive type {Name} in backward compat behaviour for ITypedElement.InstanceType.")
                };

                return new[] { (ITypeSerializationInfo)new PocoTypeReferenceInfo(bwcompatType) };
            }
            else
            {
                var names = FhirType.Select(ft => getFhirTypeName(ft));
                return names.Select(n => (ITypeSerializationInfo)new PocoTypeReferenceInfo(n)).ToArray();
            }

            string getFhirTypeName(Type ft)
            {
                // The special case where the mapping name is a backbone element name can safely
                // be ignored here, since that is handled by the first case in the if statement above.
                if (ClassMapping.TryGetMappingForType(ft, _createdVersion, out var tm))
                    return ((IStructureDefinitionSummary)tm).TypeName;
                else
                    throw new NotSupportedException($"Type '{ft.Name}' is listed as an allowed type for property " +
                        $"'{buildQualifiedPropName(_propInfo)}', but it does not seem to" +
                        $"be a valid FHIR type POCO.");
            }
        }

        struct PocoTypeReferenceInfo : IStructureDefinitionReference
        {
            public PocoTypeReferenceInfo(string canonical)
            {
                ReferredType = canonical;
            }

            public string ReferredType { get; private set; }
        }
    }
}
