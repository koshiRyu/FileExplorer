using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FileSearcher.Folder
{
    /// <summary>
    ///     フォルダ選択
    /// </summary>
    public class FolderSelector
    {
        /// <summary>
        ///     選択パス
        /// </summary>
        public string SelectedPath { get; private set; }

        /// <summary>
        ///     ダイアログを表示する
        /// </summary>
        /// <returns></returns>
        public DialogResult ShowDialog()
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;

                CommonFileDialogResult result = dialog.ShowDialog();

                if (result == CommonFileDialogResult.Ok)
                {
                    SelectedPath = dialog.FileName;
                    return DialogResult.OK;
                }

                return DialogResult.Cancel;
            }
        }
    }

}
