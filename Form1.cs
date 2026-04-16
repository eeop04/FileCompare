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
                string sourcePath = System.IO.Path.Combine(sourceDir, itemName);
                string destPath = System.IO.Path.Combine(destDir, itemName);

                bool isFolder = (item.SubItems.Count > 1 && item.SubItems[1].Text == "파일 폴더");

                if (isFolder)
                {
                    if (!System.IO.Directory.Exists(sourcePath)) continue;

                    string promptMessage = $"폴더 '{itemName}'(와)과 하위 내용 전체를 복사하시겠습니까?";
                    if (System.IO.Directory.Exists(destPath))
                    {
                        promptMessage = $"대상 경로에 이미 '{itemName}' 폴더가 존재합니다.\n해당 내용에 병합(오래된 파일 덮어쓰지 않음)을 진행하시겠습니까?";
                    }

                    if (MessageBox.Show(this, promptMessage, "폴더 복사 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        try
                        {
                            CopyDirectory(sourcePath, destPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, $"폴더 복사 중 오류가 발생했습니다.\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    if (!System.IO.File.Exists(sourcePath)) continue;

                    string promptMessage = null;
                    if (System.IO.File.Exists(destPath))
                    {
                        var sourceTime = System.IO.File.GetLastWriteTime(sourcePath);
                        var destTime = System.IO.File.GetLastWriteTime(destPath);

                        string sourceTimeStr = sourceTime.ToString("yyyy-MM-dd HH:mm:ss");
                        string destTimeStr = destTime.ToString("yyyy-MM-dd HH:mm:ss");

                        // 동일 파일(검은색) 이거나 오래된 파일(회색)에서 최신(빨간색)으로 복사할 때는 아무 경고 없이 자동 무시(스킵)
                        if (sourceTimeStr == destTimeStr || sourceTime < destTime)
                        {
                            continue;
                        }

                        // 최신 파일(빨간색)에서 오래된 파일(회색)으로 덮어쓸 때만 경고문 띄움
                        promptMessage = $"파일 '{sourcePath}' (수정일: {sourceTimeStr}) 를\n파일 '{destPath}' (수정일: {destTimeStr}) 에 덮어쓰기 복사하시겠습니까?";
                    }
                    else
                    {
                        // 반대편에 아예 없는 단독 파일(보라색) 복사
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
            }

            PopulateListView(lvwLeftDir, txtLeftDir.Text);
            PopulateListView(lvwRightDir, txtRightDir.Text);
            UpdateComparison();
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            System.IO.Directory.CreateDirectory(destDir);

            foreach (string file in System.IO.Directory.GetFiles(sourceDir))
            {
                string targetFilePath = System.IO.Path.Combine(destDir, System.IO.Path.GetFileName(file));

                // 파일 복사 전 시간 체크 로직 추가 (최신 파일을 덮어쓰지 않도록 보호)
                if (System.IO.File.Exists(targetFilePath))
                {
                    var sourceTime = System.IO.File.GetLastWriteTime(file);
                    var destTime = System.IO.File.GetLastWriteTime(targetFilePath);

                    // 원본이 더 오래된 파일이면 복사를 건너뜁니다 (보호)
                    if (sourceTime < destTime)
                    {
                        continue;
                    }
                }

                System.IO.File.Copy(file, targetFilePath, true);
            }

            foreach (string directory in System.IO.Directory.GetDirectories(sourceDir))
            {
                string targetDirPath = System.IO.Path.Combine(destDir, System.IO.Path.GetFileName(directory));
                CopyDirectory(directory, targetDirPath);
            }
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

            // key: "[DIR]폴더이름" or "[FILE]파일이름"
            var leftDict = new Dictionary<string, DateTime>();
            var rightDict = new Dictionary<string, DateTime>();

            foreach (var d in leftDirInfo.GetDirectories()) leftDict[$"[DIR]{d.Name}"] = d.LastWriteTime;
            foreach (var f in leftDirInfo.GetFiles()) leftDict[$"[FILE]{f.Name}"] = f.LastWriteTime;

            foreach (var d in rightDirInfo.GetDirectories()) rightDict[$"[DIR]{d.Name}"] = d.LastWriteTime;
            foreach (var f in rightDirInfo.GetFiles()) rightDict[$"[FILE]{f.Name}"] = f.LastWriteTime;

            ApplyComparisonColors(lvwLeftDir, leftDict, rightDict);
            ApplyComparisonColors(lvwRightDir, rightDict, leftDict);
        }

        private void ApplyComparisonColors(ListView lv, Dictionary<string, DateTime> myDict, Dictionary<string, DateTime> otherDict)
        {
            foreach (ListViewItem item in lv.Items)
            {
                bool isFolder = (item.SubItems.Count > 1 && item.SubItems[1].Text == "파일 폴더");
                string key = (isFolder ? "[DIR]" : "[FILE]") + item.Text;

                if (!otherDict.ContainsKey(key))
                {
                    // 단독 파일/폴더
                    item.ForeColor = Color.Purple;
                }
                else
                {
                    var myTime = myDict.ContainsKey(key) ? myDict[key] : DateTime.MinValue;
                    var otherTime = otherDict[key];

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

                // 선택 항목 배경이나 아이템 리프레시 방지 버그를 위해 UseItemStyleForSubItems 켜기
                item.UseItemStyleForSubItems = true;
            }
        }

        private void txtLeftDir_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
