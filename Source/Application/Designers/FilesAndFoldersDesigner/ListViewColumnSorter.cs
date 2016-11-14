using System;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;

namespace WixShield.Designers.FilesAndFolders
{
    /// <summary>
    /// Implements the IComparer interface for the purpose of using the implementation to sort the 
    /// columns of a list
    /// </summary>
    class ListViewColumnSorter : IComparer
    {
        /// <summary>
        /// The column which is to be sorted.
        /// </summary>
        private int ColumnToSort;
        /// <summary>
        /// The direction to sort in, e.g. Ascending.
        /// </summary>
        private SortOrder OrderOfSort;

        /// <summary>
        /// ListViewColumnSorter constructor.
        /// </summary>
        public ListViewColumnSorter()
        {
            SortColumn = 0;
            Order = SortOrder.Ascending;
        }
        /// <summary>
        /// Compares two ListView Objects using case sensitive comparrison and checking for date and integer types
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negagive if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y)
        {
            int result;
            var listViewItemX = (ListViewItem) x;
            var listViewItemY = (ListViewItem) y;
            try
            {
                // casting x and y as DateTime to see if we need to compare as DateTime
                var dateTimeX = DateTime.Parse(listViewItemX.SubItems[ColumnToSort].Text);
                var dateTimeY = DateTime.Parse(listViewItemY.SubItems[ColumnToSort].Text);
                result = DateTime.Compare(dateTimeX, dateTimeY);
            }
            catch
            {
                var stringX = listViewItemX.SubItems[ColumnToSort].Text;
                var stringY = listViewItemY.SubItems[ColumnToSort].Text;

                if ( (stringX.Contains(" bytes")) || (stringX.Contains(" KB")) )
                {
                    // can't assume that if first item (X) is bytes that the second is too
                    // must convert both back to bytes then parse and compare.
                    // strip the ' bytes' or ' KB'
                    var stringXbase = (stringX.Contains(" bytes")) ? stringX.TrimEnd(" bytes".ToCharArray()) : stringX.TrimEnd(" KB".ToCharArray());
                    var stringYbase = (stringY.Contains(" bytes")) ? stringY.TrimEnd(" bytes".ToCharArray()) : stringY.TrimEnd(" KB".ToCharArray());
                    decimal dItemXMultiplier = (stringX.Contains(" bytes")) ? 1 : 1024;
                    decimal dItemYMultiplier = (stringY.Contains(" bytes")) ? 1 : 1024;
                    // parse as decimals
                    var decimalX = Decimal.Parse(stringXbase) * dItemXMultiplier;
                    var decimalY = Decimal.Parse(stringYbase) * dItemYMultiplier;
                    // compare and return result
                    result = Decimal.Compare(decimalX, decimalY);
                }
                else
                {
                    // otherwise compare as strings
                    result = String.Compare(stringX, stringY);
                }
            }
            if (Order == SortOrder.Descending) result *= -1;

            return result;
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting. Defaults to "0"
        /// </summary>
        public int SortColumn
        {
            get { return ColumnToSort; }
            set { ColumnToSort = value; }
        }


        /// <summary>
        /// Gets or sets the direction fo the column sort, e.g. Ascending. Defaults to SortOrder.None;
        /// </summary>
        public SortOrder Order
        {
            get { return OrderOfSort; }
            set { OrderOfSort = value; }
        }
    }
}
