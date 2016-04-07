// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

using Internal.Metadata.NativeFormat.Writer;

using Cts = Internal.TypeSystem;
using Ecma = System.Reflection.Metadata;

using Debug = System.Diagnostics.Debug;
using FieldAttributes = System.Reflection.FieldAttributes;

namespace ILCompiler.Metadata
{
    partial class Transform<TPolicy>
    {
        internal EntityMap<Cts.FieldDesc, MetadataRecord> _fields =
            new EntityMap<Cts.FieldDesc, MetadataRecord>(EqualityComparer<Cts.FieldDesc>.Default);

        private Action<Cts.FieldDesc, Field> _initFieldDef;
        private Action<Cts.FieldDesc, MemberReference> _initFieldRef;

        public override MetadataRecord HandleQualifiedField(Cts.FieldDesc field)
        {
            return HandleFieldReference(field);
        }

        private Field HandleFieldDefinition(Cts.FieldDesc field)
        {
            Debug.Assert(field.GetTypicalFieldDefinition() == field);
            Debug.Assert(_policy.GeneratesMetadata(field));
            return (Field)_fields.GetOrCreate(field, _initFieldDef ?? (_initFieldDef = InitializeFieldDefinition));
        }

        private void InitializeFieldDefinition(Cts.FieldDesc entity, Field record)
        {
            record.Name = HandleString(entity.Name);
            record.Signature = new FieldSignature
            {
                Type = HandleType(entity.FieldType),
                // TODO: CustomModifiers
            };
            record.Flags = GetFieldAttributes(entity);

            var ecmaField = entity as Cts.Ecma.EcmaField;
            if (ecmaField != null)
            {
                Ecma.MetadataReader reader = ecmaField.MetadataReader;
                Ecma.FieldDefinition fieldDef = reader.GetFieldDefinition(ecmaField.Handle);
                Ecma.ConstantHandle defaultValueHandle = fieldDef.GetDefaultValue();
                if (!defaultValueHandle.IsNil)
                {
                    record.DefaultValue = HandleConstant(ecmaField.Module, defaultValueHandle);
                }

                Ecma.CustomAttributeHandleCollection customAttributes = fieldDef.GetCustomAttributes();
                if (customAttributes.Count > 0)
                {
                    record.CustomAttributes = HandleCustomAttributes(ecmaField.Module, customAttributes);
                }
            }

            // TODO: Offset
        }

        private MemberReference HandleFieldReference(Cts.FieldDesc field)
        {
            Debug.Assert(field.GetTypicalFieldDefinition() == field);
            Debug.Assert(!_policy.GeneratesMetadata(field));
            return (MemberReference)_fields.GetOrCreate(field, _initFieldRef ?? (_initFieldRef = InitializeFieldReference));
        }

        private void InitializeFieldReference(Cts.FieldDesc entity, MemberReference record)
        {
            record.Name = HandleString(entity.Name);
            record.Parent = HandleType(entity.OwningType);
            record.Signature = new FieldSignature
            {
                Type = HandleType(entity.FieldType),
                // TODO: CustomModifiers
            };
        }

        private FieldAttributes GetFieldAttributes(Cts.FieldDesc field)
        {
            FieldAttributes result;

            var ecmaField = field as Cts.Ecma.EcmaField;
            if (ecmaField != null)
            {
                var fieldDefinition = ecmaField.MetadataReader.GetFieldDefinition(ecmaField.Handle);
                result = fieldDefinition.Attributes;
            }
            else
            {
                result = 0;

                if (field.IsStatic)
                    result |= FieldAttributes.Static;
                if (field.IsInitOnly)
                    result |= FieldAttributes.InitOnly;
                if (field.IsLiteral)
                    result |= FieldAttributes.Literal;
                if (field.HasRva)
                    result |= FieldAttributes.HasFieldRVA;

                // Not set: Visibility, NotSerialized, SpecialName, RTSpecialName, HasFieldMarshal, HasDefault
            }

            return result;
        }
    }
}
