using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using IsWiX.WiXModel.Core;
using Wxs = IsWiX.WiXModel;

namespace CustomTablesDesigner
{
    /// <summary>
    /// The selected column's schema, for the property grid. The values are the column's when it's selected; a value
    /// changed in the grid is written with <see cref="Write"/>, which the designer applies to a copy of the table to
    /// check it before applying it to the document.
    /// </summary>
    internal sealed class ColumnProperties
    {
        const string Identity = "1. Identity";
        const string Schema = "2. Schema";
        const string Validation = "3. Validation";
        const string Merging = "4. Merge Modules";

        public ColumnProperties(Wxs.Column column)
        {
            ColumnId = column.Id ?? string.Empty;
            Id = ColumnId;
            Description = column.Description;
            PrimaryKey = Literal(column.PrimaryKey) ?? false;
            Type = Literal(column.Type) ?? Wxs.ColumnType.String;
            Width = Literal(column.Width);
            Nullable = Literal(column.Nullable) ?? false;
            Localizable = Literal(column.Localizable) ?? false;
            Category = Literal(column.Category);
            MinValue = Literal(column.MinValue);
            MaxValue = Literal(column.MaxValue);
            Set = column.Set;
            KeyTable = column.KeyTable;
            KeyColumn = Literal(column.KeyColumn);
            Modularize = Literal(column.Modularize);
        }

        /// <summary>The column's Id when it was selected, which finds it to write a change to.</summary>
        [Browsable(false)]
        public string ColumnId { get; }

        [Category(Identity), Description("The column's name. Rows refer to their values by it, and follow a rename.")]
        public string Id { get; set; }

        [Category(Identity), Description("What the column holds, for the _Validation table.")]
        public string? Description { get; set; }

        [Category(Schema), DisplayName("Primary Key"), Description("Whether the column is in the table's primary key. The key's columns come first, the first column is always in it, and they can't be null.")]
        public bool PrimaryKey { get; set; }

        [Category(Schema), Description("What the column holds: text (String), a number (Int), or a file's contents (Binary).")]
        public Wxs.ColumnType Type { get; set; }

        [Category(Schema), Description("For a String column, its most characters (0 for no limit, up to 255); for an Int column, 2 or 4 bytes.")]
        public int? Width { get; set; }

        [Category(Schema), Description("Whether a row can leave the column empty.")]
        public bool Nullable { get; set; }

        [Category(Schema), Description("Whether the column's values can be localized.")]
        public bool Localizable { get; set; }

        [Category(Validation), Description("The kind of value the column holds, for the _Validation table. A Binary column's is Binary.")]
        public Wxs.ColumnCategory? Category { get; set; }

        [Category(Validation), DisplayName("Min Value"), Description("The smallest value of a number, date or version in the column.")]
        public long? MinValue { get; set; }

        [Category(Validation), DisplayName("Max Value"), Description("The largest value of a number, date or version in the column.")]
        public long? MaxValue { get; set; }

        [Category(Validation), Description("The values the column can hold, separated by semicolons.")]
        public string? Set { get; set; }

        [Category(Validation), DisplayName("Key Table"), Description("The table (or tables, separated by semicolons) whose key this column refers to.")]
        public string? KeyTable { get; set; }

        [Category(Validation), DisplayName("Key Column"), Description("The column, 1 to 32, of the Key Table this column refers to.")]
        public int? KeyColumn { get; set; }

        [Category(Merging), Description("How the column's values are made unique when the table is in a merge module.")]
        public Wxs.ColumnModularize? Modularize { get; set; }

        /// <summary>
        /// Writes one property's new value to the column <paramref name="columnId"/> of <paramref name="table"/>; throws
        /// with a message for the user when the value can't be.
        /// </summary>
        public static void Write(Wxs.CustomTable table, string columnId, string property, object? value)
        {
            var column = CustomTableDocument.Column(table, columnId);
            switch (property)
            {
                case nameof(Id):
                    string id = ((string?)value ?? string.Empty).Trim();
                    CheckId(table, columnId, id);
                    CustomTableDocument.RenameColumn(table, columnId, id);
                    break;
                case nameof(Description):
                    column.Description = Text(value);
                    break;
                case nameof(PrimaryKey):
                    CustomTableDocument.SetPrimaryKey(table, columnId, (bool)value!);
                    break;
                case nameof(Type):
                    WriteType(column, (Wxs.ColumnType)value!);
                    break;
                case nameof(Width):
                    WriteWidth(column, (int?)value);
                    break;
                case nameof(Nullable):
                    column.Nullable = (bool)value!;
                    break;
                case nameof(Localizable):
                    column.Localizable = (bool)value! ? true : (WixValue<bool>?)null;
                    break;
                case nameof(Category):
                    column.Category = value is Wxs.ColumnCategory category ? category : (WixValue<Wxs.ColumnCategory>?)null;
                    break;
                case nameof(MinValue):
                    column.MinValue = value is long min ? min : (WixValue<long>?)null;
                    break;
                case nameof(MaxValue):
                    column.MaxValue = value is long max ? max : (WixValue<long>?)null;
                    break;
                case nameof(Set):
                    column.Set = Text(value);
                    break;
                case nameof(KeyTable):
                    column.KeyTable = Text(value);
                    break;
                case nameof(KeyColumn):
                    if (value is int keyColumn && (keyColumn < 1 || keyColumn > 32))
                    {
                        throw new InvalidOperationException("The key column is a column number, 1 to 32.");
                    }

                    column.KeyColumn = value is int number ? number : (WixValue<int>?)null;
                    break;
                case nameof(Modularize):
                    column.Modularize = value is Wxs.ColumnModularize modularize ? modularize : (WixValue<Wxs.ColumnModularize>?)null;
                    break;
            }
        }

        // An Int column is 2 or 4 bytes wide; a Binary column's category is Binary, and only its.
        static void WriteType(Wxs.Column column, Wxs.ColumnType type)
        {
            column.Type = type;
            if (type == Wxs.ColumnType.Int && Literal(column.Width) is not (2 or 4))
            {
                column.Width = 4;
            }

            if (type == Wxs.ColumnType.Binary)
            {
                column.Category = Wxs.ColumnCategory.Binary;
            }
            else if (Literal(column.Category) == Wxs.ColumnCategory.Binary)
            {
                column.Category = null;
            }
        }

        static void WriteWidth(Wxs.Column column, int? width)
        {
            switch (Literal(column.Type))
            {
                case Wxs.ColumnType.Int when width is not (2 or 4):
                    throw new InvalidOperationException("An Int column is 2 or 4 bytes wide.");
                case Wxs.ColumnType.String when width is < 0 or > 255:
                    throw new InvalidOperationException("A String column's width is its most characters: 0 (no limit) to 255.");
            }

            column.Width = width is int value ? value : (WixValue<int>?)null;
        }

        // A Windows Installer column name: letters, digits, underscores and periods, starting with a letter or
        // underscore, and unique in its table.
        static void CheckId(Wxs.CustomTable table, string columnId, string id)
        {
            if (id.Length == 0)
            {
                throw new InvalidOperationException("A column needs a name.");
            }

            if (!Regex.IsMatch(id, @"^[A-Za-z_][A-Za-z0-9_.]*$"))
            {
                throw new InvalidOperationException("A column's name is letters, digits, underscores and periods, and starts with a letter or underscore.");
            }

            if (id != columnId && table.Columns.Any(c => c.Id == id))
            {
                throw new InvalidOperationException($"The table already has a column named '{id}'.");
            }
        }

        static string? Text(object? value) => value is string text && text.Trim().Length > 0 ? text.Trim() : null;

        static T? Literal<T>(WixValue<T>? value) where T : struct => value is { IsLiteral: true } literal ? literal.Value : null;
    }
}
