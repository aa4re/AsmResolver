﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net
{
    public static class Utilities
    {
        internal static string GetFullName(this IMemberReference reference, IHasTypeSignature signature)
        {
            var methodSignature = signature as MethodSignature;

            var parameterString = methodSignature != null
                ? "(" + methodSignature.Parameters.Select(x => x.ParameterType).GetTypeArrayString() + ")"
                : string.Empty;

            return string.Format("{0} {1}::{2}{3}",
                signature.TypeSignature.FullName, reference.DeclaringType.FullName, reference.Name, parameterString);
        }

        internal static string GetTypeArrayString(this IEnumerable<ITypeDescriptor> types)
        {
            return string.Join(", ", types.Select(x => x.FullName));
        }

        public static bool IsTypeOf(this ITypeDescriptor type, string @namespace, string name)
        {
            return type.Namespace == @namespace && type.Name == name;
        }

        public static TypeSignature GetEnumUnderlyingType(this TypeDefinition type)
        {
            return (from field in type.Fields
                    where
                        field.Name == "value__" && field.Attributes.HasFlag(FieldAttributes.Public) &&
                        field.Signature != null
                    select field.Signature.FieldType).FirstOrDefault();
        }

        public static IAssemblyDescriptor GetAssembly(this IResolutionScope scope)
        {
            while (true)
            {
                var assembly = scope as IAssemblyDescriptor;
                if (assembly != null)
                    return assembly;
                
                var module = scope as ModuleDefinition;
                if (module != null)
                    return module.Header.GetStream<TableStream>().GetTable<AssemblyDefinition>()[0];

                var type = scope as ITypeDefOrRef;
                if (type == null)
                    return null;
                scope = type.ResolutionScope;
            }
        }

        public static bool GetMaskedAttribute(this uint self, uint mask, uint attribute)
        {
            return (self & mask) == attribute;
        }

        public static uint SetMaskedAttribute(this uint self, uint mask, uint attribute, bool value)
        {
            if (value)
            {
                self &= ~mask;
                return self | attribute;
            }

            return (self & ~(mask & attribute));
        }

        public static string ToHexString(this byte[] self)
        {
            return string.Concat(self.Select(x => x.ToString("x2")));
        }

        public static TEnum SetFlag<TEnum>(this Enum self, TEnum flag, bool value)
        {
            return (TEnum)Convert.ChangeType(
                    Convert.ToUInt64(self) & ~Convert.ToUInt64(flag) | (value ? Convert.ToUInt64(flag) : 0),
                    typeof(TEnum));
        }
    }
}
