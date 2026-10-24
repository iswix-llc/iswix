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
