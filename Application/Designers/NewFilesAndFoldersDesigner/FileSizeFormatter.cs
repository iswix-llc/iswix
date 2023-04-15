using System;

namespace Designers.NewFilesAndFolders
{
    class FileSizeFormatter : IFormatProvider, ICustomFormatter
    {

        #region IFormatProvider Members

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        #endregion

        #region ICustomFormatter Members

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!this.Equals(formatProvider)) return null;

            var fileSize = (long) arg;

            if (fileSize < 1024) return fileSize.ToString("#,#0") + " bytes";

            return (fileSize / 1024).ToString("#,#0") + " KB";
        }

        #endregion
    }
}
