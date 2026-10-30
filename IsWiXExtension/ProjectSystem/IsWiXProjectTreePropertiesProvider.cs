// Copyright (c) ISWIX LLC. Licensed under the MS-PL.

using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.ProjectSystem;
using System.ComponentModel.Composition;

namespace IsWiX.ProjectSystem
{
    /// <summary>The icon Solution Explorer shows for an IsWiX project (.iswixproj).</summary>
    [Export(typeof(IProjectTreePropertiesProvider))]
    [AppliesTo(IsWiXCapabilities.IsWiXProject)]
    [Order(1000)]
    internal sealed class IsWiXProjectTreePropertiesProvider : IProjectTreePropertiesProvider
    {
        public void CalculatePropertyValues(IProjectTreeCustomizablePropertyContext propertyContext, IProjectTreeCustomizablePropertyValues propertyValues)
        {
            if (propertyValues.Flags.Contains(ProjectTreeFlags.Common.ProjectRoot))
            {
                propertyValues.Icon = KnownMonikers.Install.ToProjectSystemType();
                propertyValues.ExpandedIcon = KnownMonikers.Install.ToProjectSystemType();
            }
        }
    }

    /// <summary>The capabilities IsWiX's project-system components apply to (IsWiX.DesignTime.targets declares them).</summary>
    internal static class IsWiXCapabilities
    {
        /// <summary>An IsWiX project (.iswixproj).</summary>
        public const string IsWiXProject = "IsWiXProject";
    }
}
