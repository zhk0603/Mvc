// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    /// <summary>
    /// Marker interface that indicates that the <see cref="IRazorPage"/> instance should not be activated using
    /// <see cref="IRazorPageActivator"/>.
    /// </summary>
    public interface ISkipRazorPageActivation
    {
    }
}
