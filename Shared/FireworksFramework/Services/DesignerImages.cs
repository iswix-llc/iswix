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
            var image = Load(typeof(DesignerImages).Assembly, MenuPrefix + name + ".png")
                ?? throw new InvalidOperationException($"No menu image '{name}' is embedded in {typeof(DesignerImages).Assembly.GetName().Name}.");
            if (Load(typeof(DesignerImages).Assembly, MenuPrefix + name + ".dark.png") is { } dark)
            {
                MenuDarks.Add(image, dark);
            }

            return image;
        }

        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Image, Image> MenuDarks =
            new System.Runtime.CompilerServices.ConditionalWeakTable<Image, Image>();

        /// <summary>
        /// The drawing for dark backgrounds of an image <see cref="MenuImage"/> gave, for a theme to use there in place
        /// of adapting the light one; null for any other image.
        /// </summary>
        public static Image? DarkMenuImage(Image image) => MenuDarks.TryGetValue(image, out var dark) ? dark : null;

        /// <summary>Gets the icon of the component picker dialogs.</summary>
        public static Icon ComponentPickerIcon()
        {
            using var stream = typeof(DesignerImages).Assembly.GetManifestResourceStream(MenuPrefix + "component_picker.ico")
                ?? throw new InvalidOperationException("The component picker icon is not embedded in FireworksFramework.");
            return new Icon(stream);
        }

        // The menu images are drawn by Designers\DesignerIcons.ps1, each with a .dark.png beside it.
        private const string MenuPrefix = "FireworksFramework.Images.Menus.";

        /// <summary>
        /// Adds <c>{resourcePrefix}{name}.png</c> from <paramref name="assembly"/> for each name, keyed by the name.
        /// The list is switched to 32-bit color so the PNGs keep their transparency. A <c>{name}.dark.png</c> embedded
        /// beside one is its drawing for dark backgrounds; see <see cref="DarkImages"/>.
        /// </summary>
        public static void Add(ImageList imageList, Assembly assembly, string resourcePrefix, params string[] names)
        {
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            foreach (var name in names)
            {
                imageList.Images.Add(name, Load(assembly, resourcePrefix + name + ".png")
                    ?? throw new InvalidOperationException($"The image '{resourcePrefix}{name}.png' is not embedded in {assembly.GetName().Name}."));
                if (Load(assembly, resourcePrefix + name + ".dark.png") is { } dark)
                {
                    DarkImages(imageList)[name] = dark;
                }
            }
        }

        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ImageList, System.Collections.Generic.Dictionary<string, Image>> Darks =
            new System.Runtime.CompilerServices.ConditionalWeakTable<ImageList, System.Collections.Generic.Dictionary<string, Image>>();

        /// <summary>
        /// An image list's images drawn for dark backgrounds, by key: a theme uses them there in place of adapting the
        /// light ones.
        /// </summary>
        public static System.Collections.Generic.IDictionary<string, Image> DarkImages(ImageList imageList) =>
            Darks.GetValue(imageList, _ => new System.Collections.Generic.Dictionary<string, Image>());

        private static Image? Load(Assembly assembly, string resource)
        {
            using var stream = assembly.GetManifestResourceStream(resource);
            if (stream is null)
            {
                return null;
            }

            using var image = Image.FromStream(stream);
            // A copy, since the image list reads its images later, after the stream is closed.
            return new Bitmap(image);
        }
    }
}
