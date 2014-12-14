using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;
using System.Threading;
using System.Reflection;
using AutoItX3Lib;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace KSTN_Facebook_Tool
{
    public partial class MainForm : Form
    {
        #region SMALL STUFFS
        // Disable WebBrowser Sounds
        const int FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;
        const int SET_FEATURE_ON_PROCESS = 0x00000002;

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(
            int FeatureEntry,
            [MarshalAs(UnmanagedType.U4)] int dwFlags,
            bool fEnable);

        static void DisableClickSounds()
        {
            CoInternetSetFeatureEnabled(
                FEATURE_DISABLE_NAVIGATION_SOUNDS,
                SET_FEATURE_ON_PROCESS,
                true);
        }


        private const int CS_DROPSHADOW = 0x00020000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                if (OSFeature.IsPresent(SystemParameter.DropShadow))
                {
                    parameters.ClassStyle = parameters.ClassStyle | CS_DROPSHADOW;
                }
                return parameters;
            }
        }
        #endregion

        SeleniumControl SE;
        public AutoItX3 autoIt = new AutoItX3();

        #region GENERAL MAINFORM
        public MainForm()
        {
            InitializeComponent();
            DisableClickSounds();

            autoIt.AutoItSetOption("WinTitleMatchMode", 2);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Program.loadingForm = new LoadingForm();
            SE = new SeleniumControl();
            txtUser.Focus();
            cbMethods.SelectedIndex = 0;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SE.quit();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (btnLogin.Text == "Đăng nhập")
            {
                SE.FBLogin(txtUser.Text, txtPass.Text);
            }
            else
            {
                if (!SE.ready)
                {
                    MessageBox.Show("Chương trình đang thực hiện 1 tác vụ, không thể đăng xuất!");
                    return;
                }
                SE.Logout();
            }
        }

        private void lblViewProfile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(lblViewProfile.Text);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TabControl1.SelectedTab == TabControl1.TabPages["tabPageInvite"])
            {
                dgGroups.Parent = GroupBoxInvite;
                dgGroups.Height = 310;
            }
            else
            {
                dgGroups.Parent = groupBox4;
                dgGroups.Height = 160;
            }
        }

        private void btnToggle_Click(object sender, EventArgs e)
        {
            if (SE.driver == null)
            {
                MessageBox.Show("Trình duyệt chưa được khởi tạo!");
                btnToggle.Checked = false;
            }
            else
            {
                SE.Toggle();
            }
        }
        #endregion

        #region TAB AUTOPOST
        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            //fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtBrowse1.Text = file;
            }
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            //fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtBrowse2.Text = file;
            }
        }

        private void btnBrowse3_Click(object sender, EventArgs e)
        {
            var fDialog = new System.Windows.Forms.OpenFileDialog();
            fDialog.Title = "Open Arial Bitmap File";
            fDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            //fDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = fDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = fDialog.FileName;
                txtBrowse3.Text = file;
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (SE.pause == false)
            {
                SE.pause = true;
                btnPause.Text = "Continue";
                lblTick.Text = "Dừng";

                txtContent.Enabled = true;
                txtDelay.Enabled = true;
                cbMethods.Enabled = true;
                txtBrowse1.Enabled = true;
                txtBrowse2.Enabled = true;
                txtBrowse3.Enabled = true;
                btnBrowse1.Enabled = true;
                btnBrowse2.Enabled = true;
                btnBrowse3.Enabled = true;
            }
            else
            {
                SE.pause = false;
                btnPause.Text = "Pause";

                lblTick.Text = "Resume";
                txtContent.Enabled = false;
                txtDelay.Enabled = false;
                cbMethods.Enabled = false;
                txtBrowse1.Enabled = false;
                txtBrowse2.Enabled = false;
                txtBrowse3.Enabled = false;
                btnBrowse1.Enabled = false;
                btnBrowse2.Enabled = false;
                btnBrowse3.Enabled = false;
            }
        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            if (txtBrowse1.Text == "" && txtBrowse2.Text == "" && txtBrowse3.Text == "" && txtContent.Text == "")
            {
                MessageBox.Show("Điền nội dung trước khi post bài!");
                return;
            }

            int delay;

            if (!int.TryParse(txtDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            btnPost.Enabled = false;
            txtContent.Enabled = false;
            txtDelay.Enabled = false;
            cbMethods.Enabled = false;
            txtBrowse1.Enabled = false;
            txtBrowse2.Enabled = false;
            txtBrowse3.Enabled = false;
            btnBrowse1.Enabled = false;
            btnBrowse2.Enabled = false;
            btnBrowse3.Enabled = false;
            dgGroups.Enabled = false;
            btnPause.Enabled = true;

            SE.AutoPost();
        }

        private void btnGroupExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.FileName = "GROUPS.txt";
            saveFile.ShowDialog();

            using (StreamWriter sw = new StreamWriter(saveFile.FileName, false))
            {
                if (dgGroups.Rows.Count > 0)
                {
                    foreach (DataGridViewRow row in dgGroups.Rows)
                    {
                        sw.WriteLine(row.Cells[1].Value + "");
                    }
                }
                else
                {
                    sw.WriteLine("No group found.");
                }
                sw.Close();
            }
        }
        #endregion

        #region TAB AUTOJOIN
        private void btnGroupJoin_Click(object sender, EventArgs e)
        {
            if (dgGroupSearch.Rows.Count == 0)
            {
                MessageBox.Show("Chưa có nhóm nào trong List!");
                return;
            }

            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }
            int delay;

            if (!int.TryParse(txtJoinDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            btnGroupJoin.Enabled = false;
            txtJoinDelay.Enabled = false;

            SE.AutoJoin();
        }

        private void btnGroupSearch_Click(object sender, EventArgs e)
        {
            if (txtGroupSearch.Text == "")
            {
                MessageBox.Show("Điền từ khóa tìm kiếm!");
                return;
            }

            int min;
            if (!int.TryParse(txtGroupSearchMin.Text, out min))
            {
                MessageBox.Show("Số lượng thành viên tối thiểu???");
            }

            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            txtGroupSearch.Enabled = false;
            txtGroupSearchMin.Enabled = false;
            btnGroupSearch.Enabled = false;

            dgGroupSearch.Rows.Clear();

            SE.GroupSearch();
        }
        #endregion

        #region TAB AUTOINVITE
        private void btnInvite_Click(object sender, EventArgs e)
        {
            if (txtInviteName.Text == "")
            {
                MessageBox.Show("Điền tên muốn mời trước khi bắt đầu Auto!");
                return;
            }

            int delay;

            if (!int.TryParse(txtInviteDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            txtInviteDelay.Enabled = false;
            txtInviteName.Enabled = false;
            btnInvite.Enabled = false;

            SE.AutoInvite();
        }
        #endregion

        #region TAB AUTOCOMMENT
        private void btnComment_Click(object sender, EventArgs e)
        {
            if (SE.ready == false)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }
            int delay;

            if (!int.TryParse(txtCommentDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            if (txtComment.Text == "")
            {
                MessageBox.Show("Không được bỏ trống nội dung bình luận");
                return;
            }

            if (!cbCommentToday.Checked && !cbCommentYesterday.Checked && !cbCommentBefore.Checked)
            {
                MessageBox.Show("Chọn thời gian bình luận!");
                return;
            }

            cbCommentBefore.Enabled = false;
            cbCommentToday.Enabled = false;
            cbCommentYesterday.Enabled = false;
            txtComment.Enabled = false;
            txtCommentDelay.Enabled = false;
            btnComment.Enabled = false;
            btnCommentPause.Enabled = true;

            SE.AutoComment();
        }

        private void btnCommentPause_Click(object sender, EventArgs e)
        {
            if (SE.pause == false)
            {
                SE.pause = true;
                btnCommentPause.Text = "Continue";
                lblCommentTick.Text = "Dừng";

                cbCommentBefore.Enabled = true;
                cbCommentToday.Enabled = true;
                cbCommentYesterday.Enabled = true;
                txtComment.Enabled = true;
                txtCommentDelay.Enabled = true;
            }
            else
            {
                SE.pause = false;
                btnCommentPause.Text = "Pause";

                cbCommentBefore.Enabled = false;
                cbCommentToday.Enabled = false;
                cbCommentYesterday.Enabled = false;
                txtComment.Enabled = false;
                txtCommentDelay.Enabled = false;
            }
        }
        #endregion

        #region TAB AUTOTAG
        private void btnTag_Click(object sender, EventArgs e)
        {
            if (!SE.ready)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác");
                return;
            }

            if (txtTagUrl.Text == "")
            {
                MessageBox.Show("Thêm đường dẫn ảnh hoặc bài viết trước khi Tag");
                return;
            }

            String tag_url = "";

            Match match = Regex.Match(txtTagUrl.Text, @"^https\:\/\/www\.facebook\.com\/(.*)", RegexOptions.None);

            if (match.Success)
            {
                tag_url = "https://m.facebook.com/" + match.Groups[1].Value;
            }
            else
            {
                match = Regex.Match(txtTagUrl.Text, @"^https\:\/\/m\.facebook\.com\/(.*)", RegexOptions.None);
                if (match.Success)
                {
                    tag_url = txtTagUrl.Text;
                }
                else
                {
                    MessageBox.Show("Đường dẫn bài viết/ảnh sai định dạng!\nVí dụ:\nhttps://www.facebook.com/photo.php?fbid=########\nhoặc\nhttps://m.facebook.com/photo.php?fbid=########");
                    return;
                }
            }

            btnTag.Enabled = false;
            txtTagUrl.Enabled = false;

            SE.AutoTag(tag_url);
        }
        #endregion

        #region TAB PM
        private void btnPMImportFriends_Click(object sender, EventArgs e)
        {
            if (!SE.ready)
            {
                MessageBox.Show("Chương trình đang thực hiện 1 tác vụ khác!");
                return;
            }

            btnPMImportFriends.Enabled = false;

            SE.ImportFriendList();
        }
        #endregion

        #region OTHER HELPERS
        public void addGroup2Grid(IWebElement k)
        {
            //dgGroups.Rows.Add(k.GetAttribute("innerHTML"), k.GetAttribute("href"), "");
            Thread t = new Thread(() => Program.mainForm.Invoke(new MethodInvoker(delegate() { dgGroups.Rows.Insert(0, k.GetAttribute("innerHTML"), k.GetAttribute("href"), ""); })));
            t.Start();
        }

        private void lblVer_Click(object sender, EventArgs e)
        {
            if (!btnToggle.Enabled)
            {
                Random rnd = new Random();
                if (rnd.Next(10) == 0)
                {
                    btnToggle.Enabled = true;
                }
            }
        }

        private void btnLicense_Click(object sender, EventArgs e)
        {
            License licForm = new License();
            licForm.ShowDialog();
        }
        #endregion
    }
}
