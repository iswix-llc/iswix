using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace FireworksFramework.Services
{
    /// <summary>
    /// Fills WinForms image lists from PNGs embedded in an assembly. Image lists set up in the WinForms designer are
    /// stored as a BinaryFormatter blob in the .resx, which .NET 9 can no longer read; this keeps the images as files.
    /// </summary>
    public static class DesignerImages
    {
        /// <summary>
        /// The tree icons the Files and Folders, Services and Shortcuts designers share, in the order of their
        /// ImageLibrary enums.
        /// </summary>
        public static readonly string[] Library =
        {
            "folder_open", "folder_closed", "computer", "file_dll", "file", "web_file",
            "database_file", "service", "xml_file", "blue_folder_closed", "delete", "blue_folder_open",
        };

        /// <summary>Adds the shared <see cref="Library"/> icons to an image list.</summary>
        public static void AddLibrary(ImageList imageList) =>
            Add(imageList, typeof(DesignerImages).Assembly, "FireworksFramework.Images.Designers.", Library);

        /// <summary>
        /// Gets one of the menu images the WinForms designers share (<c>Images\Menus</c>): <c>new_folder</c>,
        /// <c>rename</c>, <c>delete</c>, <c>expand_all</c>, <c>collapse_all</c>, <c>refresh</c> or <c>new_shortcut</c>.
        /// </summary>
        /// <remarks>
        /// Set in code rather than stored in the .resx: images dotnet build writes into a .resx for .NET Framework
        /// need System.Resources.Extensions and a binding redirect, which Visual Studio can't be given.
        /// </remarks>
        public static Image MenuImage(string name)
        {
            using var stream = MenuResource(name);
            using var image = Image.FromStream(stream);
            return new Bitmap(image);
        }

        /// <summary>Gets the icon of the component picker dialogs.</summary>
        public static Icon ComponentPickerIcon()
        {
            using var stream = MenuResource("component_picker");
            return new Icon(stream);
        }

        // The menu images keep their original formats (GIF, PNG, BMP, ICO), so the name has no extension.
        private static System.IO.Stream MenuResource(string name)
        {
            var assembly = typeof(DesignerImages).Assembly;
            var prefix = "FireworksFramework.Images.Menus." + name + ".";
            var resource = Array.Find(assembly.GetManifestResourceNames(), r => r.StartsWith(prefix, StringComparison.Ordinal))
                ?? throw new InvalidOperationException($"No menu image '{name}' is embedded in {assembly.GetName().Name}.");
            return assembly.GetManifestResourceStream(resource)!;
        }

        /// <summary>
        /// Adds <c>{resourcePrefix}{name}.png</c> from <paramref name="assembly"/> for each name, keyed by the name.
        /// The list is switched to 32-bit color so the PNGs keep their transparency.
        /// </summary>
        public static void Add(ImageList imageList, Assembly assembly, string resourcePrefix, params string[] names)
        {
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            foreach (var name in names)
            {
                using var stream = assembly.GetManifestResourceStream(resourcePrefix + name + ".png")
                    ?? throw new InvalidOperationException($"The image '{resourcePrefix}{name}.png' is not embedded in {assembly.GetName().Name}.");
                using var image = Image.FromStream(stream);
                // A copy, since the image list reads its images later, after the stream is closed.
                imageList.Images.Add(name, new Bitmap(image));
            }
        }
    }
}
