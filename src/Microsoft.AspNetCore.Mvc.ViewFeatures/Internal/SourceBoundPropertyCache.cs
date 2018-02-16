// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    internal class SourceBoundPropertyCache
    {
        private const string TempDataPrefix = "TempDataProperty-";

        private readonly ConcurrentDictionary<Type, IReadOnlyList<SourceBoundPropertyCacheItem>> _cache =
            new ConcurrentDictionary<Type, IReadOnlyList<SourceBoundPropertyCacheItem>>();

        public IReadOnlyList<SourceBoundPropertyCacheItem> GetOrAdd(Type type)
        {
            if (!_cache.TryGetValue(type, out var cacheItems))
            {
                cacheItems = _cache.GetOrAdd(type, GetCacheItems(type));
            }

            return cacheItems;
        }

        private static IReadOnlyList<SourceBoundPropertyCacheItem> GetCacheItems(Type type)
        {
            var cacheItems = new List<SourceBoundPropertyCacheItem>();
            var propertyHelpers = PropertyHelper.GetVisibleProperties(type);
            for (var i = 0; i < propertyHelpers.Length; i++)
            {
                var propertyHelper = propertyHelpers[i];
                var property = propertyHelper.Property;

                var customAttributes = property.GetCustomAttributes(inherit: false);
                for (var j = 0; j < customAttributes.Length; j++)
                {
                    var attribute = customAttributes[j];
                    string key;
                    BoundPropertySource lifetimeKind;
                    if (attribute is ViewDataAttribute viewData)
                    {
                        ValidateProperty(property, nameof(ViewDataAttribute));

                        key = viewData.Key ?? property.Name;
                        lifetimeKind = BoundPropertySource.ViewData;
                    }
                    else if (attribute is TempDataAttribute tempData)
                    {
                        EnsureValidTempDataProperty(property);

                        key = tempData.Key ?? TempDataPrefix + property.Name;
                        lifetimeKind = BoundPropertySource.TempData;
                    }
                    else
                    {
                        continue;
                    }

                    cacheItems.Add(new SourceBoundPropertyCacheItem(propertyHelper, lifetimeKind, key));
                }
            }

            return cacheItems;
        }

        private static void ValidateProperty(PropertyInfo property, string attributeName)
        {
            if (!(property.SetMethod != null &&
                property.SetMethod.IsPublic &&
                property.GetMethod != null &&
                property.GetMethod.IsPublic))
            {
                throw new InvalidOperationException(
                    Resources.FormatTempDataProperties_PublicGetterSetter(property.DeclaringType.FullName, property.Name, attributeName));
            }
        }

        private static void EnsureValidTempDataProperty(PropertyInfo property)
        {
            ValidateProperty(property, nameof(TempDataAttribute));

            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (!TempDataSerializer.CanSerializeType(propertyType, out var errorMessage))
            {
                var messageWithPropertyInfo = Resources.FormatTempDataProperties_InvalidType(
                    property.DeclaringType.FullName,
                    property.Name,
                    nameof(TempDataAttribute));

                throw new InvalidOperationException($"{messageWithPropertyInfo} {errorMessage}");
            }
        }
    }
}
