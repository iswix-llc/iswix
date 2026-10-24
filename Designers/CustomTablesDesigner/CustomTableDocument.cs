using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using IsWiX.WiXModel.Core;
using Wxs = IsWiX.WiXModel;

namespace CustomTablesDesigner
{
    // The custom tables of a package or fragment, read and written through the typed model.
    internal static class CustomTableDocument
    {
        /// <summary>Gets whether the document is one this designer edits: a WiX package or fragment.</summary>
        public static bool IsValidDocument(XDocument document)
        {
            var root = document?.Root;
            var first = root?.Elements().FirstOrDefault()?.Name;
            return root?.Name == Wxs.Wix.ElementName && (first == Wxs.Package.ElementName || first == Wxs.Fragment.ElementName);
        }

        // The <Package> or <Fragment> the tables belong to.
        static WixElement Container(XDocument document) => WixElement.Wrap(document.Root!.Elements().First());

        public static IEnumerable<Wxs.CustomTable> Tables(XDocument document) => Container(document) switch
        {
            Wxs.Package package => package.CustomTables,
            Wxs.Fragment fragment => fragment.CustomTables,
            _ => Enumerable.Empty<Wxs.CustomTable>(),
        };

        public static Wxs.CustomTable Find(XDocument document, string id) => Tables(document).First(t => t.Id == id);

        static IEnumerable<Wxs.EnsureTable> EnsureTables(XDocument document, string? id) =>
            document.Descendants(Wxs.EnsureTable.ElementName).Select(e => (Wxs.EnsureTable)WixElement.Wrap(e)).Where(e => e.Id == id);

        // A table with a primary key column, and the EnsureTable that makes sure it is created even when empty. Both
        // are added through the model, so they land at their canonical positions.
        public static void Create(XDocument document, string id)
        {
            var table = new Wxs.CustomTable { Id = id };
            table.Add(new Wxs.Column
            {
                Id = "Id",
                Category = Wxs.ColumnCategory.Identifier,
                PrimaryKey = true,
                Nullable = false,
                Type = Wxs.ColumnType.String,
                Width = 72,
            });

            switch (Container(document))
            {
                case Wxs.Package package:
                    package.Add(table);
                    package.Add(new Wxs.EnsureTable { Id = id });
                    break;
                case Wxs.Fragment fragment:
                    fragment.Add(table);
                    fragment.Add(new Wxs.EnsureTable { Id = id });
                    break;
            }
        }

        // The table's EnsureTable follows it.
        public static void Rename(XDocument document, Wxs.CustomTable table, string id)
        {
            foreach (var ensureTable in EnsureTables(document, table.Id).ToList())
            {
                ensureTable.Id = id;
            }
            table.Id = id;
        }

        public static void Drop(XDocument document, Wxs.CustomTable table)
        {
            string? id = table.Id;
            table.RemoveFromParent();
            if (!Tables(document).Any(t => t.Id == id))
            {
                foreach (var ensureTable in EnsureTables(document, id).ToList())
                {
                    ensureTable.RemoveFromParent();
                }
            }
        }

        // The Row element behind each grid row, so saving only touches what changed.
        public static Dictionary<DataRow, Wxs.Row> LoadRows(Wxs.CustomTable table, DataTable dataTable)
        {
            var rows = new Dictionary<DataRow, Wxs.Row>();
            foreach (var row in table.Rows)
            {
                DataRow dataRow = dataTable.NewRow();
                foreach (var data in row.DataElements)
                {
                    if (data.Column != null && dataTable.Columns.Contains(data.Column))
                    {
                        dataRow[data.Column] = Text(data);
                    }
                }
                dataTable.Rows.Add(dataRow);
                rows.Add(dataRow, row);
            }
            return rows;
        }

        // Writes the grid back: removed rows are removed, new rows added after the others, and in the rest only the
        // values that changed are written.
        public static void SaveRows(Wxs.CustomTable table, DataTable dataTable, Dictionary<DataRow, Wxs.Row> rows)
        {
            var current = new HashSet<DataRow>(dataTable.Rows.Cast<DataRow>().Where(IsLive));
            foreach (var removed in rows.Where(r => !current.Contains(r.Key)).ToList())
            {
                removed.Value.RemoveFromParent();
                rows.Remove(removed.Key);
            }

            foreach (DataRow dataRow in dataTable.Rows)
            {
                if (!IsLive(dataRow))
                {
                    continue;
                }

                if (!rows.TryGetValue(dataRow, out var row))
                {
                    row = new Wxs.Row();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        row.Add(new Wxs.Data { Column = column.ColumnName, Value = dataRow[column].ToString() });
                    }
                    rows.Add(dataRow, table.Add(row));
                    continue;
                }

                foreach (DataColumn column in dataTable.Columns)
                {
                    string value = dataRow[column].ToString() ?? string.Empty;
                    var data = row.DataElements.FirstOrDefault(d => d.Column == column.ColumnName);
                    if (data == null)
                    {
                        row.Add(new Wxs.Data { Column = column.ColumnName, Value = value });
                    }
                    else if (Text(data) != value)
                    {
                        // A value written as element text moves to the Value attribute.
                        foreach (var text in data.Xml.Nodes().OfType<XText>().ToList())
                        {
                            text.Remove();
                        }
                        data.Value = value;
                    }
                }
            }
        }

        static bool IsLive(DataRow row) => row.RowState != DataRowState.Deleted && row.RowState != DataRowState.Detached;

        // A cell's value: the Value attribute, or the element's text as older sources wrote it.
        static string Text(Wxs.Data data) => data.Value ?? (data.Xml.Nodes().OfType<XText>().Any() ? data.Xml.Value : string.Empty);
    }
}
