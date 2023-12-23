using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSearcher.Dgv
{
    /// <summary>
    ///     ページ機能付きソート可能バインドリスト..
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedSortableBindingList<T> : SortableBindingList<T> where T:class
    {
        private IList<T> list;
        private int _pageSize = 100;
        private int _currentPage = 1;

        /// <summary>
        ///     リストアイテム
        /// </summary>
        public IList<T> ListItem
        {
            get { return this.list; }
            set { this.list = value; }
        }

        /// <summary>
        ///     コンストラクタ
        /// </summary>
        public PagedSortableBindingList()
        {
        }

        /// <summary>
        ///     ページサイズ
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                _pageSize = value;
                UpdatePage();
            }
        }

        /// <summary>
        ///     現在ページ
        /// </summary>
        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                _currentPage = value;
                UpdatePage();
            }
        }

        /// <summary>
        ///     総ページ数
        /// </summary>
        public int TotalPages
        {
            get
            {
                if (this.list.Count == 0) return 1;
                return (this.list.Count + _pageSize - 1) / _pageSize;
            }
        }

        /// <summary>
        ///     ページ更新
        /// </summary>
        public void UpdatePage()
        {
            int startIndex = Math.Max((_currentPage - 1) * _pageSize, 0);
            int endIndex = Math.Min(_currentPage * _pageSize, this.list.Count);

            this.Clear();
            for (int i = startIndex; i < endIndex; i++)
            {
                this.Add(this.list[i]);
            }

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
    }

}
