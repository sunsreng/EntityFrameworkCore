// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Describes metadata needed to decide on a type mapping for a property or type.
    /// </summary>
    public abstract class TypeMappingInfo
    {
        private readonly ValueConverter _customConverter;

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        protected TypeMappingInfo()
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="property"> The property for which mapping is needed. </param>
        protected TypeMappingInfo([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            Property = property;

            MemberInfo = property.GetDefiningMemberInfo();

            var principals = property.FindPrincipals().ToList();

            StoreClrType = principals
                .Select(p => p.GetStoreClrType())
                .FirstOrDefault(t => t != null)
                ?.UnwrapNullableType();

            _customConverter = principals
                .Select(p => p.GetValueConverter())
                .FirstOrDefault(c => c != null);

            var mappingHints = _customConverter?.MappingHints ?? default;

            if (_customConverter != null)
            {
                ValueConverterInfo = new ValueConverterInfo(
                    _customConverter.ModelType,
                    _customConverter.StoreType,
                    i => _customConverter,
                    mappingHints);
            }

            IsKeyOrIndex = property.IsKeyOrForeignKey() || property.IsIndex();

            Size = principals.Select(p => p.GetMaxLength()).FirstOrDefault(t => t != null) ?? mappingHints.Size;

            IsUnicode = principals.Select(p => p.IsUnicode()).FirstOrDefault(t => t != null)
                        ?? mappingHints.IsUnicode;

            IsRowVersion = property.IsConcurrencyToken && property.ValueGenerated == ValueGenerated.OnAddOrUpdate;

            IsFixedLength = mappingHints.IsFixedLength;

            Precision = mappingHints.Precision;

            Scale = mappingHints.Scale;

            ModelClrType = property.ClrType.UnwrapNullableType();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        protected TypeMappingInfo([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            ModelClrType = type.UnwrapNullableType();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="member"> The property or field for which mapping is needed. </param>
        protected TypeMappingInfo([NotNull] MemberInfo member)
        {
            Check.NotNull(member, nameof(member));

            ModelClrType = member.GetMemberType().UnwrapNullableType();
            MemberInfo = member;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" />.
        /// </summary>
        /// <param name="type"> The CLR type in the model for which mapping is needed. </param>
        /// <param name="keyOrIndex"> If <c>true</c>, then a special mapping for a key or index may be returned. </param>
        /// <param name="unicode"> Specifies Unicode or Ansi mapping, or <c>null</c> for default. </param>
        /// <param name="size"> Specifies a size for the mapping, or <c>null</c> for default. </param>
        /// <param name="rowVersion"> Specifies a row-version, or <c>null</c> for default. </param>
        /// <param name="fixedLength"> Specifies a fixed length mapping, or <c>null</c> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <c>null</c> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <c>null</c> for default. </param>
        protected TypeMappingInfo(
            [NotNull] Type type,
            bool keyOrIndex,
            bool? unicode = null,
            int? size = null,
            bool? rowVersion = null,
            bool? fixedLength = null,
            int? precision = null,
            int? scale = null)
            : this(type)
        {
            IsKeyOrIndex = keyOrIndex;
            Size = size;
            IsUnicode = unicode;
            IsRowVersion = rowVersion;
            Precision = precision;
            Scale = scale;
            IsFixedLength = fixedLength;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TypeMappingInfo" /> with the given <see cref="ValueConverterInfo" />.
        /// </summary>
        /// <param name="source"> The source info. </param>
        /// <param name="builtInConverter"> The converter to apply. </param>
        protected TypeMappingInfo(
            [NotNull] TypeMappingInfo source,
            ValueConverterInfo builtInConverter)
        {
            Check.NotNull(source, nameof(source));

            Property = source.Property;
            ModelClrType = source.ModelClrType;
            StoreClrType = source.StoreClrType;
            IsRowVersion = source.IsRowVersion;
            IsKeyOrIndex = source.IsKeyOrIndex;

            if (source._customConverter != null)
            {
                _customConverter = source._customConverter;

                ValueConverterInfo = new ValueConverterInfo(
                    _customConverter.ModelType,
                    builtInConverter.StoreClrType,
                    i => _customConverter.ComposeWith(builtInConverter.Create()),
                    _customConverter.MappingHints.With(builtInConverter.MappingHints));
            }
            else
            {
                ValueConverterInfo = builtInConverter;
            }

            // ReSharper disable once VirtualMemberCallInConstructor
            var mappingHints = ValueConverterInfo?.MappingHints ?? default;

            Size = source.Size ?? mappingHints.Size;
            IsUnicode = source.IsUnicode ?? mappingHints.IsUnicode;
            IsFixedLength = source.IsFixedLength ?? mappingHints.IsFixedLength;
            Scale = source.Scale ?? mappingHints.Scale;
            Precision = source.Precision ?? mappingHints.Precision;
        }

        /// <summary>
        ///     Returns a new <see cref="TypeMappingInfo" /> with the given converter applied.
        /// </summary>
        /// <param name="converterInfo"> The converter to apply. </param>
        /// <returns> The new mapping info. </returns>
        public abstract TypeMappingInfo WithBuiltInConverter(ValueConverterInfo converterInfo);

        /// <summary>
        ///     The property for which mapping is needed.
        /// </summary>
        public virtual IProperty Property { get; }

        /// <summary>
        ///     Indicates whether or not the mapping is part of a key or index.
        /// </summary>
        public virtual bool IsKeyOrIndex { get; }

        /// <summary>
        ///     Indicates the store-size to use for the mapping, or null if none.
        /// </summary>
        public virtual int? Size { get; }

        /// <summary>
        ///     Indicates whether or not the mapping supports Unicode, or null if not defined.
        /// </summary>
        public virtual bool? IsUnicode { get; }

        /// <summary>
        ///     Indicates whether or not the mapping will be used for a row version, or null if not defined.
        /// </summary>
        public virtual bool? IsRowVersion { get; }

        /// <summary>
        ///     The suggested precision of the mapped data type.
        /// </summary>
        public virtual int? Precision { get; }

        /// <summary>
        ///     The suggested scale of the mapped data type.
        /// </summary>
        public virtual int? Scale { get; }

        /// <summary>
        ///     Whether or not the mapped data type is fixed length.
        /// </summary>
        public virtual bool? IsFixedLength { get; }

        /// <summary>
        ///     The CLR type set to use when reading/writing to/from the store.
        /// </summary>
        public virtual Type StoreClrType { get; }

        /// <summary>
        ///     The field or property info for the property.
        /// </summary>
        public virtual MemberInfo MemberInfo { get; }

        /// <summary>
        ///     The <see cref="ValueConverter" /> to use when reading/writing to/from the store.
        /// </summary>
        public virtual ValueConverterInfo? ValueConverterInfo { get; }

        /// <summary>
        ///     The CLR type in the model.
        /// </summary>
        public virtual Type ModelClrType { get; }

        /// <summary>
        ///     The CLR type targeted by the type mapping when reading/writing to/from the store.
        /// </summary>
        public virtual Type TargetClrType => ValueConverterInfo?.StoreClrType.UnwrapNullableType() ?? StoreClrType ?? ModelClrType;

        /// <summary>
        ///     Compares this <see cref="TypeMappingInfo" /> to another to check if they represent the same mapping.
        /// </summary>
        /// <param name="other"> The other object. </param>
        /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
        protected virtual bool Equals([NotNull] TypeMappingInfo other)
            => ModelClrType == other.ModelClrType
               && MemberInfo == other.MemberInfo
               && StoreClrType == other.StoreClrType
               && IsKeyOrIndex == other.IsKeyOrIndex
               && Size == other.Size
               && IsUnicode == other.IsUnicode
               && IsRowVersion == other.IsRowVersion
               && IsFixedLength == other.IsFixedLength
               && Precision == other.Precision
               && Scale == other.Scale
               && Equals(_customConverter, other._customConverter);

        /// <summary>
        ///     Compares this <see cref="TypeMappingInfo" /> to another to check if they represent the same mapping.
        /// </summary>
        /// <param name="obj"> The other object. </param>
        /// <returns> <c>True</c> if they represent the same mapping; <c>false</c> otherwise. </returns>
        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                   || obj.GetType() == GetType()
                   && Equals((TypeMappingInfo)obj));

        /// <summary>
        ///     Returns a hash code for this object.
        /// </summary>
        /// <returns> The hash code. </returns>
        public override int GetHashCode()
        {
            var hashCode = (StoreClrType != null ? StoreClrType.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ IsKeyOrIndex.GetHashCode();
            hashCode = (hashCode * 397) ^ (Size?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (MemberInfo?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (IsUnicode?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (IsRowVersion?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (IsFixedLength?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (Scale?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (Precision?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (_customConverter?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (ModelClrType?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
}
