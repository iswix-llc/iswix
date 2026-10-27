using System;

namespace FireworksFramework.Services
{
    /// <summary>
    /// Lets a designer take the user to somewhere else: to the file that defines what an element refers to. The host sets
    /// <see cref="GoToDefinition"/> once; with none set (a host that can't open files) the designers offer nothing.
    /// </summary>
    public static class DesignerNavigation
    {
        /// <summary>
        /// Opens the file and selects the element in it. The arguments are the file's path, the element's name without
        /// its namespace (<c>ComponentGroup</c>) and its Id; the result says whether the file was opened.
        /// </summary>
        public static Func<string, string, string, bool>? GoToDefinition { get; set; }
    }
}
