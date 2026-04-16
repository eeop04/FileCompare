namespace FileCompare
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCopyFromLeft_Click(object sender, EventArgs e)
        {
            CopySelectedFiles(txtLeftDir.Text, txtRightDir.Text, lvwLeftDir);
        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {
            CopySelectedFiles(txtRightDir.Text, txtLeftDir.Text, lvwRightDir);
        }

        private void CopySelectedFiles(string sourceDir, string destDir, ListView sourceListView)
        {
            if (string.IsNullOrWhiteSpace(sourceDir) || string.IsNullOrWhiteSpace(destDir))
            {
                MessageBox.Show(this, "양쪽 폴더를 모두 선택해야 복사할 수 있습니다.", "확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (sourceListView.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "복사할 항목을 선택해주세요.", "확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (ListViewItem item in sourceListView.SelectedItems)
            {
                string itemName = item.Text;
                
                if (item.SubItems.Count > 1 && item.SubItems[1].Text == "파일 폴더")
                {
                    MessageBox.Show(this, $"'{itemName}'은(는) 폴더입니다. 파일만 복사할 수 있습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    continue;
                }

                string sourcePath = System.IO.Path.Combine(sourceDir, itemName);
                string destPath = System.IO.Path.Combine(destDir, itemName);

                if (!System.IO.File.Exists(sourcePath)) continue;

                string promptMessage;
                if (System.IO.File.Exists(destPath))
                {
                    var sourceDate = System.IO.File.GetLastWriteTime(sourcePath).ToString("yyyy-MM-dd HH:mm:ss");
                    var destDate = System.IO.File.GetLastWriteTime(destPath).ToString("yyyy-MM-dd HH:mm:ss");
                    promptMessage = $"파일 '{sourcePath}' (수정일: {sourceDate}) 를\n파일 '{destPath}' (수정일: {destDate}) 에 덮어쓰기 복사하시겠습니까?";
                }
                else
                {
                    promptMessage = $"파일 '{sourcePath}' 를\n파일 '{destPath}' 에 복사하시겠습니까?";
                }

                if (MessageBox.Show(this, promptMessage, "파일 복사 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        System.IO.File.Copy(sourcePath, destPath, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"파일 복사 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            PopulateListView(lvwLeftDir, txtLeftDir.Text);
            PopulateListView(lvwRightDir, txtRightDir.Text);
            UpdateComparison();
        }

        private void btnLeftDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                if (!string.IsNullOrWhiteSpace(txtLeftDir.Text) && System.IO.Directory.Exists(txtLeftDir.Text))
                {
                    dlg.SelectedPath = txtLeftDir.Text;
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtLeftDir.Text = dlg.SelectedPath;
                    PopulateListView(lvwLeftDir, txtLeftDir.Text);
                    UpdateComparison();
                }
            }
        }

        private void btnRightDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                if (!string.IsNullOrWhiteSpace(txtRightDir.Text) && System.IO.Directory.Exists(txtRightDir.Text))
                {
                    dlg.SelectedPath = txtRightDir.Text;
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtRightDir.Text = dlg.SelectedPath;
                    PopulateListView(lvwRightDir, txtRightDir.Text);
                    UpdateComparison();
                }
            }
        }

        private void txtRightDir_TextChanged(object sender, EventArgs e)
        {

        }

        private void lvwLeftDir_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lvwRightDir_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void PopulateListView(ListView lv, string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !System.IO.Directory.Exists(folderPath))
                return;

            if (lv.Columns.Count == 0)
            {
                lv.View = View.Details;
                lv.Columns.Add("이름", 150);
                lv.Columns.Add("크기", 100);
                lv.Columns.Add("수정일", 150);
            }

            lv.BeginUpdate();
            lv.Items.Clear();

            try
            {
                // 폴더(디렉터리) 먼저 추가
                var dirInfo = new System.IO.DirectoryInfo(folderPath);
                var dirs = dirInfo.GetDirectories();
                Array.Sort(dirs, (a, b) => a.Name.CompareTo(b.Name));

                foreach (var d in dirs)
                {
                    var item = new ListViewItem(d.Name);
                    item.SubItems.Add("파일 폴더");
                    item.SubItems.Add(d.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    lv.Items.Add(item);
                }

                // 파일 추가
                var files = dirInfo.GetFiles();
                Array.Sort(files, (a, b) => a.Name.CompareTo(b.Name));

                foreach (var f in files)
                {
                    var item = new ListViewItem(f.Name);

                    long sizeKb = f.Length / 1024;
                    if (f.Length > 0 && sizeKb == 0) sizeKb = 1;

                    item.SubItems.Add($"{sizeKb:N0} KB");
                    item.SubItems.Add(f.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));

                    lv.Items.Add(item);
                }

                // 컬럼 너비 자동 조정 (컨텐츠 기준)
                for (int i = 0; i < lv.Columns.Count; i++)
                {
                    lv.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                MessageBox.Show(this, "폴더를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Show(this, "입출력 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "오류 발생: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lv.EndUpdate();
            }
        }

        private void UpdateComparison()
        {
            if (string.IsNullOrWhiteSpace(txtLeftDir.Text) || !System.IO.Directory.Exists(txtLeftDir.Text) ||
                string.IsNullOrWhiteSpace(txtRightDir.Text) || !System.IO.Directory.Exists(txtRightDir.Text))
            {
                return;
            }

            var leftDirInfo = new System.IO.DirectoryInfo(txtLeftDir.Text);
            var rightDirInfo = new System.IO.DirectoryInfo(txtRightDir.Text);

            var leftDict = new Dictionary<string, DateTime>();
            var rightDict = new Dictionary<string, DateTime>();

            foreach (var d in leftDirInfo.GetDirectories()) leftDict[d.Name] = d.LastWriteTime;
            foreach (var f in leftDirInfo.GetFiles()) leftDict[f.Name] = f.LastWriteTime;

            foreach (var d in rightDirInfo.GetDirectories()) rightDict[d.Name] = d.LastWriteTime;
            foreach (var f in rightDirInfo.GetFiles()) rightDict[f.Name] = f.LastWriteTime;

            ApplyComparisonColors(lvwLeftDir, leftDict, rightDict);
            ApplyComparisonColors(lvwRightDir, rightDict, leftDict);
        }

        private void ApplyComparisonColors(ListView lv, Dictionary<string, DateTime> myDict, Dictionary<string, DateTime> otherDict)
        {
            foreach (ListViewItem item in lv.Items)
            {
                string name = item.Text;
                if (!otherDict.ContainsKey(name))
                {
                    // 단독 파일
                    item.ForeColor = Color.Purple;
                }
                else
                {
                    var myTime = myDict[name];
                    var otherTime = otherDict[name];

                    // 사용자에게 보여지는 화면과 동일한 형식(초 단위)으로 비교
                    string myTimeStr = myTime.ToString("yyyy-MM-dd HH:mm:ss");
                    string otherTimeStr = otherTime.ToString("yyyy-MM-dd HH:mm:ss");

                    if (myTimeStr == otherTimeStr)
                    {
                        item.ForeColor = Color.Black; // 동일 (시간이 같음)
                    }
                    else if (myTime > otherTime)
                    {
                        item.ForeColor = Color.Red; // New (최신)
                    }
                    else
                    {
                        item.ForeColor = Color.Gray; // Old (오래됨)
                    }
                }
            }
        }
    }
}
