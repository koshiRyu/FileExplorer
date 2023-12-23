using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace FileSearcher.Dgv.Rows
{
    /// <summary>
    ///     DataGridViewのカスタムクラスの基底クラスです。
    /// </summary>
    public class DataGridViewDataClassBase : INotifyPropertyChanged
    {
        /// <summary>
        ///     プロパティ変更イベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     プロパティ変更イベントを発生させます。
        /// </summary>
        /// <param name="name"></param>
        public void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        ///     プロパティ変更イベントを発生させながらプロパティに関連するフィールドに値をセットします。
        /// </summary>
        /// <typeparam name="T">データ型</typeparam>
        /// <param name="field">フィールド変数</param>
        /// <param name="value">セットする値</param>
        /// <param name="propName">プロパティ名（イベントの発生に必要）</param>
        public void SetValue<T>(out T field, T value, string propName)
        {
            field = value;
            this.OnPropertyChanged(propName);
        }

        /// <summary>
        ///     入力操作をした場合プロパティ変更イベントを発生させながらプロパティに関連するフィールドを整数に変換して値をセットします。
        /// </summary>
        /// <typeparam name="T">データ型</typeparam>
        /// <param name="field">フィールド変数</param>
        /// <param name="value">セットする値</param>
        /// <param name="propName">プロパティ名（イベントの発生に必要）</param>
        public void SetValueInputInt(ref int? field, string value, string propName)
        {
            int intValue;

            if (int.TryParse(value, out intValue))
            {
                //整数を入力した場合
                field = intValue;
            }
            else if (value == null || value.Trim().Length == 0)
            {
                //何も入力しない場合
                field = null;
            }
            else
            {
                //文字や記号を含めて入力した場合
                //何もしない
            }

            this.OnPropertyChanged(propName);
        }
    }
}
