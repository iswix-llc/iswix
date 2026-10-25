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

        // The grid's view of a table: a column for each Column, typed and constrained as the table defines it (integers
        // 2 or 4 bytes wide, strings no longer than their width, the primary key columns together unique), and a row for
        // each Row. Throws when the rows don't fit the columns, which is how a schema change is checked before it's made.
        public static DataTable BuildDataTable(Wxs.CustomTable table, out Dictionary<DataRow, Wxs.Row> rows)
        {
            var dataTable = new DataTable(table.Id ?? string.Empty);
            var keys = new List<DataColumn>();
            foreach (var column in table.Columns)
            {
                var dataColumn = new DataColumn(column.Id ?? string.Empty)
                {
                    // Nullable unless it says otherwise, though WiX's default is no: sources often leave it out.
                    AllowDBNull = Literal(column.Nullable) ?? true,
                };
                int width = Literal(column.Width) ?? 0;
                switch (Literal(column.Type))
                {
                    case Wxs.ColumnType.Int:
                        dataColumn.DataType = width == 2 ? typeof(short) : typeof(int);
                        break;
                    case Wxs.ColumnType.String when width > 0:
                        dataColumn.MaxLength = width;
                        break;
                }

                dataTable.Columns.Add(dataColumn);
                if (Literal(column.PrimaryKey) == true)
                {
                    keys.Add(dataColumn);
                }
            }

            if (keys.Count > 0)
            {
                dataTable.PrimaryKey = keys.ToArray();
            }

            rows = LoadRows(table, dataTable);
            dataTable.AcceptChanges();
            return dataTable;
        }

        // The Row element behind each grid row, so saving only touches what changed.
        static Dictionary<DataRow, Wxs.Row> LoadRows(Wxs.CustomTable table, DataTable dataTable)
        {
            var rows = new Dictionary<DataRow, Wxs.Row>();
            foreach (var row in table.Rows)
            {
                DataRow dataRow = dataTable.NewRow();
                foreach (var data in row.DataElements)
                {
                    if (data.Column != null && dataTable.Columns.Contains(data.Column))
                    {
                        // An empty value in a number column is no value.
                        string text = Text(data);
                        var dataColumn = dataTable.Columns[data.Column]!;
                        bool number = dataColumn.DataType != typeof(string);
                        if (text.Length > 0 || !number)
                        {
                            try
                            {
                                dataRow[dataColumn] = text;
                            }
                            catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
                            {
                                throw new InvalidOperationException(number
                                    ? $"The value '{text}' in the column '{data.Column}' isn't a whole number that fits it."
                                    : $"The value '{text}' is longer than the column '{data.Column}' allows.", ex);
                            }
                        }
                    }
                }
                dataTable.Rows.Add(dataRow);
                rows.Add(dataRow, row);
            }
            return rows;
        }

        static T? Literal<T>(WixValue<T>? value) where T : struct => value is { IsLiteral: true } literal ? literal.Value : null;

        public static Wxs.Column Column(Wxs.CustomTable table, string id) => table.Columns.First(c => c.Id == id);

        // "Column1", "Column2"... whichever the table doesn't have yet.
        public static string NewColumnId(Wxs.CustomTable table)
        {
            int index = 1;
            while (table.Columns.Any(c => c.Id == "Column" + index))
            {
                index++;
            }

            return "Column" + index;
        }

        // A nullable text column, beside another one.
        public static void InsertColumn(Wxs.CustomTable table, string id, string besideId, bool below)
        {
            var column = new Wxs.Column
            {
                Id = id,
                Category = Wxs.ColumnCategory.Text,
                Nullable = true,
                Type = Wxs.ColumnType.String,
                Width = 72,
            };
            PlaceNextTo(column.Xml, Column(table, besideId).Xml, below);
        }

        // The rows' values follow the column.
        public static void RenameColumn(Wxs.CustomTable table, string id, string newId)
        {
            Column(table, id).Id = newId;
            foreach (var data in table.Rows.SelectMany(r => r.DataElements).Where(d => d.Column == id).ToList())
            {
                data.Column = newId;
            }
        }

        // The rows' values go with it.
        public static void DeleteColumn(Wxs.CustomTable table, string id)
        {
            Column(table, id).RemoveFromParent();
            foreach (var data in table.Rows.SelectMany(r => r.DataElements).Where(d => d.Column == id).ToList())
            {
                data.RemoveFromParent();
            }
        }

        // One place up (-1) or down (1).
        public static void MoveColumn(Wxs.CustomTable table, string id, int offset)
        {
            var columns = table.Columns.ToList();
            int index = columns.FindIndex(c => c.Id == id);
            MoveNextTo(columns[index].Xml, columns[index + offset].Xml, below: offset > 0);
        }

        // A key column can't be null.
        public static void SetPrimaryKey(Wxs.CustomTable table, string id, bool key)
        {
            var column = Column(table, id);
            if (key)
            {
                column.PrimaryKey = true;
                column.Nullable = false;
            }
            else
            {
                column.PrimaryKey = null;
            }
        }

        // Windows Installer's rules for a table's primary key: the first column is in it, and its columns come first.
        public static void CheckKeys(Wxs.CustomTable table)
        {
            var keys = table.Columns.Select(c => Literal(c.PrimaryKey) == true).ToList();
            if (keys.Count == 0)
            {
                throw new InvalidOperationException("A table needs at least one column.");
            }

            if (!keys[0])
            {
                throw new InvalidOperationException("The first column must be in the primary key: Windows Installer requires one.");
            }

            if (keys.SkipWhile(k => k).Any(k => k))
            {
                throw new InvalidOperationException("The primary key's columns must come first, before the others.");
            }
        }

        // Moves an element next to a sibling: it takes its line along, and starts a line indented like the sibling's.
        static void MoveNextTo(XElement element, XElement sibling, bool below)
        {
            if (element == sibling)
            {
                return;
            }

            WixElement.Wrap(element).RemoveFromParent();
            PlaceNextTo(element, sibling, below);
        }

        // Puts a detached element next to a sibling, on a line of its own indented like the sibling's.
        static void PlaceNextTo(XElement element, XElement sibling, bool below)
        {
            string? indent = sibling.PreviousNode is XText text && string.IsNullOrWhiteSpace(text.Value) ? text.Value : null;
            if (below)
            {
                sibling.AddAfterSelf(indent is null ? new object[] { element } : new object[] { new XText(indent), element });
            }
            else
            {
                sibling.AddBeforeSelf(indent is null ? new object[] { element } : new object[] { element, new XText(indent) });
            }
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
