using FileSearcher.Folder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using FileSearcher.Dgv;
using FileSearcher.Dgv.Rows;
using FileSearcher.Strings;

namespace FileSearcher.Forms
{
    /// <summary>
    ///     メインフォーム
    /// </summary>
    public partial class FrmMain : Form
    {
        /// <summary>
        ///     ファイルまs多はフォルダを示す列挙隊
        /// </summary>
        public enum FileFolder
        {
            /// <summary>ファイル</summary>
            File,
            /// <summary>フォルダ</summary>
            Folder
        }

        /// <summary>
        ///     DataGridView行バインドリスト
        /// </summary>
        private PagedSortableBindingList<DgvRowFilePath> sbList = new PagedSortableBindingList<DgvRowFilePath>();

        /// <summary>
        ///     DataridViewのバインドリストに表示するものすべて
        /// </summary>
        private List<DgvRowFilePath> lstList = new List<DgvRowFilePath>();

        /// <summary>
        ///     現在起動している非同期スレッド
        /// </summary>
        private int intRunCount = -1;

        /// <summary>
        ///     見つかったファイル数
        /// </summary>
        public int intFileCount = 0;

        /// <summary>
        ///     ファイル検索をキャンセルしたかどうか
        /// </summary>
        private bool blnCancel = false;

        /// <summary>
        ///     ストップウォッチ
        /// </summary>
        private Stopwatch sw = new Stopwatch();

        /// <summary>
        ///     非同期時に通知するためのコンテキスト
        /// </summary>
        private SynchronizationContext context;

        /// <summary>
        ///     コンストラクタ
        /// </summary>
        public FrmMain()
        {
            this.context = SynchronizationContext.Current;
            InitializeComponent();
        }

        /// <summary>
        ///     フォームロード時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.InitializeWindow();
        }

        /// <summary>
        ///     フォーム表示時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_Shown(object sender, EventArgs e)
        {
            this.txtFileName.Select();
        }

        /// <summary>
        ///     フォルダ参照
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefDir_Click(object sender, EventArgs e)
        {
            var folderSelector = new FolderSelector();
            if (folderSelector.ShowDialog() == DialogResult.OK)
            {
                this.txtBaseDir.Text = folderSelector.SelectedPath;
            }
        }

        /// <summary>
        ///     検索ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (this.btnSearch.Text.Contains("検索"))
            {
                this.lstList.Clear();
                this.sbList.Clear();
                this.blnCancel = false;
                this.intRunCount = 0;
                this.intFileCount = 0;
                this.sw.Start();
                this.tmChecker.Start();
                this.btnSearch.Text = "ｷｬﾝｾﾙ(F5)";
                this.btnSearch.BackColor = Color.Orange;
                this.SearchFile(this.txtFileName.Text, this.txtBaseDir.Text, this.chkIgnoreDirPath.Checked, FileFolder.Folder);
            }
            else
            {
                this.blnCancel = true;
                this.EndSearch();
            }
        }

        /// <summary>
        ///     ページ数変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numPage_ValueChanged(object sender, EventArgs e)
        {
            this.sbList.CurrentPage = (int)this.numPage.Value;
        }

        /// <summary>
        ///     ウィンドウ初期化
        /// </summary>
        private void InitializeWindow()
        {
            this.sbList.ListItem = this.lstList;
            this.dgvList.AutoGenerateColumns = false;
            this.dgvList.DataSource = this.sbList;
        }

        /// <summary>
        ///     ファイル検索を行う。
        /// </summary>
        /// <param name="strFileName">ファイル名</param>
        /// <param name="strDirOrFile">現在探索中のディレクトリまたはファイル</param>
        /// <param name="blnIgnoreDir">フォルダパスを無視する</param>
        /// <param name="kind">ファイルまたはフォルダ</param>
        private void SearchFile(string strFileName, string strDirOrFile, bool blnIgnoreDir, FileFolder kind)
        {
            if (this.blnCancel) return;

            Action action = () =>
            {
                Interlocked.Increment(ref this.intRunCount);
                try
                {
                    if (kind == FileFolder.File)
                    {
                        string strTarget = strDirOrFile;
                        if (blnIgnoreDir) strTarget = System.IO.Path.GetFileName(strTarget);
                        if (this.IsMatch(strFileName, strTarget))
                        {
                            this.context.Post(d =>
                            {
                                DgvRowFilePath rowBound = new DgvRowFilePath() { FilePath = strDirOrFile };
                                this.lstList.Add(rowBound);
                            }, null);
                        }
                        Interlocked.Increment(ref this.intFileCount);
                    }
                    else
                    {
                        this.context.Post(d =>
                        {
                            if (this.blnCancel) return;
                            this.lblStatus.Text = this.intFileCount + "ファイル検索済み 探索中：" + strDirOrFile.Replace(this.txtBaseDir.Text + "\\", "");
                            this.lblTime.Text = (this.sw.ElapsedMilliseconds / 1000) + "秒経過";
                        }, null);
                        string[] aryFiles = new string[0];
                        string[] aryFolders = new string[0];
                        try { aryFiles = System.IO.Directory.GetFiles(strDirOrFile); } catch (Exception) { }
                        try { aryFolders = System.IO.Directory.GetDirectories(strDirOrFile); } catch (Exception) { }
                        foreach (string file in aryFiles) this.SearchFile(strFileName, file, blnIgnoreDir, FileFolder.File);
                        foreach (string folder in aryFolders) this.SearchFile(strFileName, folder, blnIgnoreDir, FileFolder.Folder);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref this.intRunCount);
                }
            };

            Task.Run(action);
        }

        /// <summary>
        ///     文字列が合致しているかどうか
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="target"></param>
        private bool IsMatch(string keyword, string target)
        {
            keyword = this.GetSearchString(keyword);
            target = this.GetSearchString(target);

            string[] strSplit = keyword.Split(new char[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
            if (strSplit.Length == 0) return true;

            foreach (string str in strSplit)
            {
                if (target.Contains(str)) return true;
            }
            return false;
        }

        /// <summary>
        ///     検索用文字列にする
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string GetSearchString(string str)
        {
            return str.StringConvert(StringUtil.dwMapFlags.LCMAP_KATAKANA | StringUtil.dwMapFlags.LCMAP_HALFWIDTH | StringUtil.dwMapFlags.LCMAP_LOWERCASE);
        }

        /// <summary>
        ///     チェック用タイマー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmChecker_Tick(object sender, EventArgs e)
        {
            if (this.intRunCount == 0)
            {
                this.EndSearch();
            }
        }

        /// <summary>
        ///     検索終了
        /// </summary>
        private void EndSearch()
        {
            this.sw.Stop();
            this.tmChecker.Stop();

            this.lstList.Sort();
            this.sbList.UpdatePage();
            this.numPage.Maximum = this.sbList.TotalPages;
            this.lblTotalPage.Text = this.sbList.TotalPages.ToString();
            if (this.numPage.Value == 1) this.numPage_ValueChanged(this, EventArgs.Empty);
            else this.numPage.Value = 1;

            this.blnCancel = true;
            this.btnSearch.Text = "検索(F5)";
            this.btnSearch.BackColor = Color.Transparent;

            if (!this.blnCancel)
            {
                this.lblStatus.Text = "検索が完了しました。ファイル数：" + this.intFileCount;
                this.lblTime.Text = "";
                MessageBox.Show("検索が完了しました。\n" + (this.sw.ElapsedMilliseconds / 1000) + "秒かかりました。", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                this.lblStatus.Text = "キャンセルされました";
                this.lblTime.Text = "";
                MessageBox.Show("キャンセルされました", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        ///     セル内容クリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //ファイルを開く
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;

            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.RowIndex >= dgv.Rows.Count || e.ColumnIndex >= dgv.Columns.Count) return;

            DataGridViewCell cell = dgv[e.ColumnIndex, e.RowIndex];
            DataGridViewRow row = cell.OwningRow;
            DgvRowFilePath rowBound = row.DataBoundItem as DgvRowFilePath;
            if (rowBound == null) return;

            this.StartProcess(rowBound.FilePath);
        }

        /// <summary>
        ///     プロセスを開始する
        /// </summary>
        /// <param name="path"></param>
        private void StartProcess(string path)
        {
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true,  // 関連付けられたアプリケーションを使用してファイルを開くためには true に設定する
                };

                process.Start();
            }
        }

        /// <summary>
        ///     フォームキー押下時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5: this.btnSearch_Click(this.btnSearch, EventArgs.Empty); break;
            }
        }
    }
}
