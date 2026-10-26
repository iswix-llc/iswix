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

        /// <summary>Gets whether the designer has an item that edits <paramref name="element"/>, or one of the elements above it.</summary>
        bool Handles(XElement element);

        /// <summary>
        /// Selects the item that edits <paramref name="element"/>, or one of the elements above it. Does nothing if
        /// the designer has no such item.
        /// </summary>
        void SelectElement(XElement element);
    }
}
