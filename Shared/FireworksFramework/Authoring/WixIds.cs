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
            (char.IsAsciiLetter(id[0]) || id[0] == '_') &&
            id.All(c => char.IsAsciiLetterOrDigit(c) || c == '_' || c == '.');
    }
}
