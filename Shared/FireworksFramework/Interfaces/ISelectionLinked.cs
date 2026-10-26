using System;
using System.Xml.Linq;

namespace FireworksFramework.Interfaces
{
    /// <summary>
    /// A designer whose selection can follow the XML editor's caret and the other way round. The elements are those
    /// of the document the designer was created for.
    /// </summary>
    public interface ISelectionLinked
    {
        /// <summary>Occurs when the user selects something in the designer.</summary>
        event EventHandler SelectedElementChanged;

        /// <summary>Gets the element the designer's selection edits, or null if nothing is selected.</summary>
        XElement? SelectedElement { get; }

        /// <summary>Gets whether the designer has an item that edits <paramref name="element"/> (or one of the elements above it), at the attribute <paramref name="attribute"/> of it if that is not null. A designer that has an item for the element whatever the attribute ignores it.</summary>
        bool Handles(XElement element, string? attribute);

        /// <summary>
        /// Gets the name of the attribute of <see cref="SelectedElement"/> that the selection is on (a cell of a
        /// grid, say), or null if it is on the element as a whole.
        /// </summary>
        string? SelectedAttribute { get; }

        /// <summary>
        /// Selects the item that edits <paramref name="element"/>, and the field of it that edits
        /// <paramref name="attribute"/> if that is not null and the designer has one. Does nothing if the designer
        /// has no such item.
        /// </summary>
        void SelectElement(XElement element, string? attribute);
    }
}
