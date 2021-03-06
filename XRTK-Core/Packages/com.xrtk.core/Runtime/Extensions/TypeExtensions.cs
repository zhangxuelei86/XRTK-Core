﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;

namespace XRTK.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Type"/> instances.
    /// </summary>
    public static class TypeExtensions
    {
        private static void BuildTypeCache()
        {
            foreach (var (type, guid) in
                from type in AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && !type.IsAbstract)
                let guid = type.GUID
                where !TypeCache.ContainsKey(guid)
                select (type, guid))
            {
                TypeCache.Add(guid, type);
            }
        }

        private static readonly Dictionary<Guid, Type> TypeCache = new Dictionary<Guid, Type>();

        /// <summary>
        /// Attempts to resolve the type using the class <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">Class <see cref="Guid"/> reference.</param>
        /// <param name="resolvedType">The resolved <see cref="Type"/>.</param>
        /// <returns>True if the <see cref="resolvedType"/> was successfully obtained from or added to the <see cref="TypeCache"/>, otherwise false.</returns>
        public static bool TryResolveType(Guid guid, out Type resolvedType)
        {
            resolvedType = null;

            if (TypeCache.Count == 0)
            {
                BuildTypeCache();
            }

            if (guid == Guid.Empty ||
                !TypeCache.TryGetValue(guid, out resolvedType))
            {
                return false;
            }

            if (resolvedType != null && !resolvedType.IsAbstract)
            {
                if (!TypeCache.ContainsKey(guid))
                {
                    TypeCache.Add(guid, resolvedType);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to resolve the type using a the <see cref="System.Type.AssemblyQualifiedName"/> or <see cref="Type.GUID"/> as <see cref="string"/>.
        /// </summary>
        /// <param name="typeRef">The <see cref="Type.GUID"/> or <see cref="System.Type.AssemblyQualifiedName"/> as <see cref="string"/>.</param>
        /// <param name="resolvedType">The resolved <see cref="Type"/>.</param>
        /// <returns>True if the <see cref="resolvedType"/> was successfully obtained from or added to the <see cref="TypeCache"/>, otherwise false.</returns>
        public static bool TryResolveType(string typeRef, out Type resolvedType)
        {
            resolvedType = null;

            if (string.IsNullOrEmpty(typeRef)) { return false; }

            if (Guid.TryParse(typeRef, out var guid))
            {
                return TryResolveType(guid, out resolvedType);
            }

            resolvedType = Type.GetType(typeRef);

            if (resolvedType != null)
            {
                if (resolvedType.GUID != Guid.Empty)
                {
                    return TryResolveType(guid, out resolvedType);
                }

                if (!resolvedType.IsAbstract)
                {
                    Debug.LogWarning($"{resolvedType.Name} is missing a {nameof(GuidAttribute)}. This extension has been upgraded to use System.Type.GUID instead of System.Type.AssemblyQualifiedName");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Recursively looks for generic type arguments in type hierarchy, starting with the
        /// root type provided. If no generic type arguments are found on a type, it's base
        /// type is checked.
        /// </summary>
        /// <param name="root">Root type to start looking for generic type arguments at.</param>
        /// <param name="maxRecursionDepth">The maximum recursion depth until execution gets canceled even if no results found.</param>
        /// <returns>Found generic type arguments array or null, if none found.</returns>
        public static Type[] FindTopmostGenericTypeArguments(this Type root, int maxRecursionDepth = 5)
        {
            var genericTypeArgs = root?.GenericTypeArguments;

            if (genericTypeArgs != null && genericTypeArgs.Length > 0)
            {
                return genericTypeArgs;
            }

            if (maxRecursionDepth > 0 && root != null)
            {
                return FindTopmostGenericTypeArguments(root.BaseType, --maxRecursionDepth);
            }

            Debug.LogError($"{nameof(FindTopmostGenericTypeArguments)} - Maximum recursion depth reached without finding generic type arguments.");
            return null;
        }
    }
}
