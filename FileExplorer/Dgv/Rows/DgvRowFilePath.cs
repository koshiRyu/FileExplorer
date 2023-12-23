using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSearcher.Dgv.Rows
{
    /// <summary>
    ///     ファイルパスを表示する行バインドクラス
    /// </summary>
    internal class DgvRowFilePath : DataGridViewDataClassBase, IComparable
    {
        /// <summary>
        ///     ファイルパス
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        ///     パスの順序比較
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (!(obj is DgvRowFilePath)) return 1;
            return string.Compare(this.FilePath, ((DgvRowFilePath)obj).FilePath);
        }
    }
}
