using System.Linq;

namespace FireworksFramework.Authoring
{
    /// <summary>Rules for the Ids designers let users type.</summary>
    public static class WixIds
    {
        /// <summary>
        /// Gets whether text is a WiX identifier: it starts with a letter or '_' and has only letters, digits, '_'
        /// and '.'.
        /// </summary>
        public static bool IsValidIdentifier(string? id) =>
            !string.IsNullOrEmpty(id) &&
            (IsAsciiLetter(id[0]) || id[0] == '_') &&
            id.All(c => IsAsciiLetter(c) || (c >= '0' && c <= '9') || c == '_' || c == '.');

        // char.IsAsciiLetter is .NET 7+; the designers also build for .NET Framework.
        private static bool IsAsciiLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
}
