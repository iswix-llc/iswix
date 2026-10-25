using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FireworksFramework.Services
{
    /// <summary>
    /// Fills a menu with the files a new item (a service, a shortcut) can be made for, to pick one from in place of a
    /// dialog. Files are shown by where they're installed (<c>[Directory]Subdirectory\file</c>), either as a list or
    /// in a submenu per folder.
    /// </summary>
    public static class CandidateMenu
    {
        /// <summary>
        /// Replaces <paramref name="items"/> with an item for each candidate, which calls <paramref name="pick"/> when
        /// clicked; with none, a disabled item saying <paramref name="noneText"/>.
        /// </summary>
        public static void Fill<T>(ToolStripItemCollection items, IEnumerable<T> candidates, Func<T, string> destinationPath,
            Action<T> pick, string noneText, bool byFolder)
        {
            items.Clear();
            var sorted = candidates.OrderBy(destinationPath, StringComparer.OrdinalIgnoreCase).ToList();
            if (sorted.Count == 0)
            {
                items.Add(new ToolStripMenuItem(noneText) { Enabled = false });
                return;
            }

            if (!byFolder)
            {
                foreach (var candidate in sorted)
                {
                    items.Add(Item(destinationPath(candidate), candidate, pick));
                }

                return;
            }

            foreach (var folder in sorted.GroupBy(c => Folder(destinationPath(c)), StringComparer.OrdinalIgnoreCase))
            {
                var submenu = new ToolStripMenuItem(Escape(folder.Key));
                foreach (var candidate in folder)
                {
                    var path = destinationPath(candidate);
                    submenu.DropDownItems.Add(Item(path.Substring(Folder(path).Length).TrimStart('\\'), candidate, pick));
                }

                items.Add(submenu);
            }
        }

        private static ToolStripMenuItem Item<T>(string text, T candidate, Action<T> pick)
        {
            var item = new ToolStripMenuItem(Escape(text));
            item.Click += (s, e) => pick(candidate);
            return item;
        }

        // "[INSTALLLOCATION]bin\App.exe" is in "[INSTALLLOCATION]bin", "[INSTALLLOCATION]App.exe" in "[INSTALLLOCATION]".
        private static string Folder(string path)
        {
            var slash = path.LastIndexOf('\\');
            if (slash >= 0)
            {
                return path.Substring(0, slash);
            }

            var bracket = path.IndexOf(']');
            return bracket >= 0 ? path.Substring(0, bracket + 1) : string.Empty;
        }

        // A menu item's text shows an ampersand as an access key unless it's doubled.
        private static string Escape(string text) => text.Replace("&", "&&");
    }
}
