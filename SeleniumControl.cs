using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium.Support.UI;
using System.Data;
using System.Web;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace KSTN_Facebook_Tool
{
    class SeleniumControl
    {
        public OpenQA.Selenium.Firefox.FirefoxDriver driver;

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;

        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);
        [DllImport("User32")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        IEnumerable<int> newFirefoxPids;

        Dictionary<String, String> links = new Dictionary<string, string>();
        public Dictionary<string, string> pages = new Dictionary<string, string>();
        public Dictionary<string, string> events = new Dictionary<string, string>();
        Thread t;
        public bool ready = true;
        public bool pause = false;

        // User info
        public String user_id = "";

        public SeleniumControl()
        {
            // URLs
            links["fb_url"] = "https://m.facebook.com";
            links["fb_get_token"] = "https://www.facebook.com/dialog/oauth?client_id=145634995501895&redirect_uri=https%3A%2F%2Fdevelopers.facebook.com%2Ftools%2Fexplorer%2Fcallback&response_type=token&scope=publish_actions,publish_stream,user_groups,user_friends";
            links["fb_groups"] = links["fb_url"] + "/groups/?seemore&refid=27";
            links["fb_groups_2"] = links["fb_url"] + "/groups/?s&refid=27";
            links["facebook_graph"] = "https://graph.facebook.com";
            links["fb_group_add"] = "https://m.facebook.com/groups/members/search/?group_id=";
            links["fb_photo_id"] = "https://m.facebook.com/photo.php?fbid=";
            links["fb_group_search_query"] = "https://m.facebook.com/search/?search=group&ssid=0&o=69&refid=46&pn=2&query="; // + &s=25 #skip
            links["friend_list"] = "https://m.facebook.com/USER_ID?v=friends&mutual&startindex=0";
        }

        #region GENERAL CONTROLS

        public void quit()
        {
            if (driver != null)
            {
                driver.Quit();
            }
        }

        public void Toggle()
        {
            if (Program.mainForm.btnToggle.Checked)
            {
                //Program.mainForm.autoIt.WinSetState(driver.Title, "", 1);
                /*
                foreach (int pid in newFirefoxPids)
                {
                    Process p = Process.GetProcessById(pid);
                    ShowWindow(p.MainWindowHandle.ToInt32(), SW_SHOW);
                    ShowWindow(p.MainWindowHandle.ToInt32(), SW_RESTORE);
                    SetForegroundWindow(p.MainWindowHandle);
                }*/
            }
            else
            {
                //Program.mainForm.autoIt.WinSetState(driver.Title, "", 0);
                /*
                foreach (int pid in newFirefoxPids)
                {
                    int hWnd = Process.GetProcessById(pid).MainWindowHandle.ToInt32();
                    ShowWindow(hWnd, SW_HIDE);
                }*/
            }
        }

        private void Exceptions_Handler()
        {
            Process.GetCurrentProcess().Kill();
        }

        private void Navigate(String URL)
        {
            try
            {
                driver.Url = URL;
            }
            catch { }
        }

        private void Navigate2(OpenQA.Selenium.Firefox.FirefoxDriver driver, String URL)
        {
            try
            {
                driver.Url = URL;
            }
            catch { }
        }

        public String getUrl()
        {
            return driver.Url;
        }

        private void FileInputAdd(String element_name, String localpath)
        {
            try
            {
                OpenQA.Selenium.IWebElement iElement = driver.FindElement(By.Name(element_name));
                driver.ExecuteScript(@"document.getElementsByName('" + element_name + "')[0].focus();");
                iElement.SendKeys(localpath);
            }
            catch { }
        }

        private void InputValueAdd(String input_name, String value)
        {
            try
            {
                OpenQA.Selenium.IWebElement iElement = driver.FindElementByName(input_name);
                iElement.Clear();
                iElement.SendKeys(value);
            }
            catch { } // I took the responsibility for this. Just in case the Internet is down
        }

        private void Click(String element_name)
        {
            try
            {
                OpenQA.Selenium.IWebElement iElement = driver.FindElementByName(element_name);
                iElement.Click();
                Thread.Sleep(100);
            }
            catch { }
        }

        private void ClickElement(IWebElement e)
        {
            try
            {
                e.Click();
                Thread.Sleep(100);
            }
            catch { }
        }

        private int FindHeader()
        {
            try
            {
                var headers = driver.FindElementsById("root");
                return headers.Count;
            }
            catch
            {
                return 0;
            }
        }
        #endregion

        #region LOGIN_LOGOUT
        public async void FBLogin(String user, String pass)
        {
            Program.mainForm.btnLogin.Enabled = false;

            if (user == "" || pass == "")
            {
                MessageBox.Show("Điền thông tin đăng nhập!");
                Program.mainForm.btnLogin.Enabled = true;
                return;
            }

            Program.loadingForm = new LoadingForm();
            Program.loadingForm.setText("KHỞI TẠO HỆ THỐNG...");
            //Program.loadingForm.Show();
            //Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(() => Program.loadingForm.ShowDialog()));

            t = new System.Threading.Thread(() => Program.loadingForm.ShowDialog());
            t.Start(); // LoadingForm.Show()
            // Bật trình duyệt khi Login
            if (driver == null)
            {
                setReady(false, "Đang khởi tạo hệ thống");
                var profile = new OpenQA.Selenium.Firefox.FirefoxProfile();
                profile.SetPreference("general.useragent.override", "NokiaC5-00/061.005 (SymbianOS/9.3; U; Series60/3.2 Mozilla/5.0; Profile/MIDP-2.1 Configuration/CLDC-1.1) AppleWebKit/525 (KHTML, like Gecko) Version/3.0 Safari/525 3gpp-gba");
                //profile.SetPreference("webdriver.load.strategy", "unstable");
                //profile.SetPreference("permissions.default.stylesheet", 2);
                profile.SetPreference("permissions.default.image", 2);
                profile.AddExtension("App/Firefox/firebug.xpi");
                //profile.SetPreference("dom.ipc.plugins.enabled.libflashplayer.so", "false");
                IEnumerable<int> pidsBefore = Process.GetProcessesByName("firefox").Select(p => p.Id);
                try
                {
                    //this.driver = await Task.Factory.StartNew(() => new OpenQA.Selenium.Firefox.FirefoxDriver(profile));
                    OpenQA.Selenium.Firefox.FirefoxBinary firefox = new OpenQA.Selenium.Firefox.FirefoxBinary("App/Firefox/firefox.exe");
                    this.driver = new OpenQA.Selenium.Firefox.FirefoxDriver(firefox, profile);
                }
                catch
                {
                    Program.loadingForm.RequestStop();
                    MessageBox.Show("Thiếu File!?");
                    /*
                    if (MessageBox.Show("Để chạy chương trình, bạn cần cài đặt trình duyệt Mozilla Firefox tích hợp. Cài đặt ngay?", "Cài đặt trình duyệt", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                    {
                        MessageBox.Show("Chương trình có thể bị treo trong vài giây tới. Nhấn OK để bắt đầu cài đặt!");
                        // install firefox 34
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.CreateNoWindow = false;
                        startInfo.UseShellExecute = false;
                        startInfo.FileName = "firefox34.exe";
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.Arguments = "-ms";
                        try
                        {
                            using (Process exeProcess = Process.Start(startInfo))
                            {
                                exeProcess.WaitForExit();
                            }
                        }
                        catch { }

                        string[] folders = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Mozilla\\Firefox\\Profiles\\", "*.default");
                        string prefs_js = folders[0] + "\\prefs.js";

                        StreamWriter sw;
                        sw = File.AppendText(prefs_js);
                        sw.WriteLine("user_pref('app.update.auto', false);");
                        sw.WriteLine("user_pref('app.update.enabled', false);");
                        sw.Close();

                        MessageBox.Show("Hoàn thành cài đặt! Hãy khởi động lại chương trình!");
                    }*/

                    Exceptions_Handler();
                }
                IEnumerable<int> pidsAfter = Process.GetProcessesByName("firefox").Select(p => p.Id);

                newFirefoxPids = pidsAfter.Except(pidsBefore);

                try
                {
                    foreach (int pid in newFirefoxPids)
                    {
                        int hWnd = Process.GetProcessById(pid).MainWindowHandle.ToInt32();
                        //ShowWindow(hWnd, SW_HIDE);
                    }
                }
                catch
                {
                    // newFirefoxPids.Count == 0
                    Program.loadingForm.RequestStop();
                    MessageBox.Show("Không tìm thấy cửa sổ Firefox!");
                    Exceptions_Handler();
                }

                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));
                driver.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
                driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(30));
                setReady(true);
            }

            Program.loadingForm.setText("ĐĂNG NHẬP TÀI KHOẢN FACEBOOK...");
            setReady(false, "Đang đăng nhập");
            await Task.Factory.StartNew(() => Navigate(links["fb_url"]));

            if (driver.FindElementsByName("email").Count == 0)
            {
                Program.loadingForm.RequestStop();
                t.Abort();
                t.Join();
                MessageBox.Show("Có lỗi với đường truyền mạng hoặc tài khoản facebook của bạn!\nHãy kiểm tra lại");
                Program.mainForm.btnLogin.Enabled = true;
                setReady(true);
                return;
            }

            InputValueAdd("email", user);
            InputValueAdd("pass", pass);
            await Task.Factory.StartNew(() => Click("login"));

            setReady(true);

            if (getUrl().Contains("home.php") || getUrl().Contains("phoneacquire"))
            {

                Program.mainForm.btnLogin.Text = "Đăng nhập thành công!";
                Program.mainForm.AcceptButton = null;
                Program.mainForm.txtUser.Enabled = false;
                Program.mainForm.txtPass.Enabled = false;
                Program.mainForm.btnPost.Enabled = true;
                Program.mainForm.btnInvite.Enabled = true;
                Program.mainForm.btnGroupSearch.Enabled = true;
                Program.mainForm.btnGroupJoin.Enabled = true;
                Program.mainForm.btnPMImportFriends.Enabled = true;
                Program.mainForm.btnPM.Enabled = true;
                Program.mainForm.btnPMSendFrRequests.Enabled = true;
                Program.mainForm.btnPMImportProfile.Enabled = true;
                Program.mainForm.btnPMImportGroup.Enabled = true;
                Program.mainForm.btnCommentImportComment.Enabled = true;
                Program.mainForm.btnEdit.Enabled = true;
                Program.mainForm.btnPostImportGroups.Enabled = true;
                Program.mainForm.btnGroupImportFriends.Enabled = true;
                Program.mainForm.btnCommentScan.Enabled = true;
                Program.mainForm.btnFanpageComment.Enabled = true;
                Program.mainForm.btnFanpageGroupPost.Enabled = true;
                Program.mainForm.btnEventInviteFriends.Enabled = true;

                var photos = driver.FindElementsByXPath("//a[contains(@href, '?v=photos')]");
                if (photos.Count > 0)
                {
                    String href = photos[0].GetAttribute("href");
                    Match match = Regex.Match(href, @".com\/([A-Za-z0-9\-\.]+)\?v\=photos", RegexOptions.None);
                    if (match.Success)
                    {
                        user_id = match.Groups[1].Value;
                    }
                }

                var nodes = driver.FindElementsByXPath("//img[contains(@src, 'fbcdn-profile-a.akamaihd.net')]");
                if (nodes.Count > 0)
                {
                    Program.mainForm.lblUsername.Text = nodes[0].GetAttribute("alt");
                }

                try
                {
                    string user_id_img = user_id;
                    if (user_id_img == "profile.php")
                    {
                        user_id_img = driver.FindElementByName("target").GetAttribute("value");
                    }

                    Program.mainForm.pbAvatar.WaitOnLoad = false;
                    Program.mainForm.pbAvatar.LoadAsync(links["facebook_graph"] + "/" + user_id_img + "/picture");
                    Program.mainForm.lblViewProfile.Text = "https://facebook.com/" + user_id_img;
                }
                catch { }

                //Program.mainForm.Focus();
                Program.loadingForm.setText("ĐĂNG NHẬP THÀNH CÔNG! ĐANG TẢI DANH SÁCH NHÓM...");

                if (Program.mainForm.cbGroupReload.Checked)
                {
                    try
                    {
                        Program.mainForm.dgGroups.Rows.Clear();
                        DataSet DS = new DataSet();
                        DS.ReadXml(RemoveSpecialCharacters(user_id) + "_groups.xml");
                        foreach (DataRow dr in DS.Tables[0].Rows)
                        {
                            Program.mainForm.dgGroups.Rows.Add(dr[0], dr[1], dr[2], dr[3]);
                        }
                    }
                    catch { }
                }

                if (Program.mainForm.dgGroups.RowCount == 0) await getGroups();
                else
                {
                    Program.loadingForm.RequestStop();
                    t.Abort();
                    t.Join();
                    Program.mainForm.lblProgress.Text = "0/" + Program.mainForm.dgGroups.Rows.Count;
                    setReady(true, "Số lượng nhóm: " + Program.mainForm.dgGroups.Rows.Count + " | Ready");
                }

                Program.mainForm.btnLogin.Text = "Đăng xuất";
                Program.mainForm.btnLogin.Enabled = true;
            }
            else
            {
                Program.loadingForm.RequestStop();
                t.Abort();
                t.Join();
                if (getUrl().Contains("checkpoint"))
                    MessageBox.Show("Hãy vô hiệu hóa bảo mật tài khoản trước khi sử dụng AUTO!");
                MessageBox.Show("Kiểm tra lại thông tin đăng nhập!\nNếu bạn chắc chắn thông tin đăng nhập là đúng,\nhãy đăng nhập lại tài khoản trên trình duyệt trước khi tiếp tục!");
                Program.mainForm.btnLogin.Enabled = true;
                return;
            }

            // await Task.Factory.StartNew(() => new WebDriverWait(driver, TimeSpan.FromSeconds(10))); // Chờ tải xong trang
        }

        public async Task getGroups()
        {
            setReady(false, "Đang lấy danh sách nhóm");
            await Task.Factory.StartNew(() => Navigate(links["fb_groups"]));

            Program.loadingForm.RequestStop();
            t.Abort();
            t.Join();
            Program.mainForm.dgGroups.Rows.Clear();
            var e2 = driver.FindElementsByXPath("//table//tbody//tr//td//div");
            if (e2.Count < 4)
            {
                // MessageBox.Show("Không thể lấy được danh sách nhóm!");
                await Task.Factory.StartNew(() => Navigate(links["fb_groups_2"]));
                var divs = driver.FindElementsByXPath("//div[@id='root']//div");
                var h3 = divs[1].FindElements(By.TagName("h3"));
                foreach (IWebElement _h3 in h3)
                {
                    var k = _h3.FindElement(By.TagName("a"));
                    addGroup2Grid(k);
                    await TaskEx.Delay(10);
                }
            }
            else
            {
                var e = e2[3].FindElements(By.XPath(".//li//table//tbody//tr//td//a"));

                foreach (IWebElement k in e)
                {
                    //Program.mainForm.dt.Rows.Add(k.GetAttribute("innerHTML"), k.GetAttribute("href"), "");
                    addGroup2Grid(k);
                    await TaskEx.Delay(10);
                    //Program.mainForm.Invoke(new MethodInvoker(delegate() { addGroup2Grid(k); }));
                    //new Thread(() => Program.mainForm.Invoke(new MethodInvoker(delegate() { addGroup2Grid(k); }))).Start();
                    //Thread t = new Thread(() => Program.mainForm.Invoke(new MethodInvoker(delegate() { addGroup2Grid(k); })));
                    //t.Start();
                }
            }

            //Program.mainForm.dgGroups.DataSource = Program.mainForm.dt;
            //Program.mainForm.dgGroupInvites.DataSource = Program.mainForm.dt;

            Program.mainForm.lblProgress.Text = "0/" + Program.mainForm.dgGroups.Rows.Count;
            setReady(true, "Số lượng nhóm: " + Program.mainForm.dgGroups.Rows.Count + " | Ready");
            Program.mainForm.groups_to_xml();
        }

        private void addGroup2Grid(IWebElement k)
        {
            Program.mainForm.addGroup2Grid(k);
        }

        public async void Logout()
        {
            setReady(false, "Đang đăng xuất");
            var nodes = driver.FindElementsByXPath("//a[contains(@href, 'logout.php')]");
            if (nodes.Count == 1)
            {
                await Task.Factory.StartNew(() => ClickElement(nodes[0]));
            }
            var login = driver.FindElementsByName("login");
            if (login.Count == 0)
            {
                Exceptions_Handler();
            }
            else
            {
                Program.mainForm.btnLogin.Text = "Đăng nhập";
                user_id = "";
                Program.mainForm.AcceptButton = Program.mainForm.btnLogin;

                Program.mainForm.txtUser.Enabled = true;
                Program.mainForm.txtPass.Enabled = true;
                Program.mainForm.btnPost.Enabled = false;
                Program.mainForm.btnInvite.Enabled = false;
                Program.mainForm.btnGroupSearch.Enabled = false;
                Program.mainForm.btnGroupJoin.Enabled = false;
                Program.mainForm.btnPMImportFriends.Enabled = false;
                Program.mainForm.btnPM.Enabled = false;
                Program.mainForm.btnPMSendFrRequests.Enabled = false;
                Program.mainForm.btnCommentImportComment.Enabled = false;
                Program.mainForm.btnEdit.Enabled = false;
                Program.mainForm.btnPostImportGroups.Enabled = false;
                Program.mainForm.btnGroupImportFriends.Enabled = false;
                Program.mainForm.btnLogin.Enabled = true;
                Program.mainForm.btnCommentScan.Enabled = false;
                Program.mainForm.btnFanpageComment.Enabled = false;
                Program.mainForm.btnFanpageGroupPost.Enabled = false;
                Program.mainForm.btnEventInviteFriends.Enabled = false;
                Program.mainForm.dgGroups.Rows.Clear();
            }
            setReady(true, "Đăng xuất thành công | Ready");
        }
        #endregion

        public async void AutoPost()
        {
            if (!pause)
                Program.mainForm.lblTick.Text = "POSTING";
            setReady(false, "Đang tự động post bài");
            int delay;
            int progress = 0;

            if (!int.TryParse(Program.mainForm.txtDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                Exceptions_Handler();
            }

            foreach (DataGridViewRow row in Program.mainForm.dgGroups.Rows)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                progress++;
                Program.mainForm.lblProgress.Text = progress + "/" + Program.mainForm.dgGroups.Rows.Count;

                if (row.Cells[3].Value.ToString() == "0") continue;

                await Task.Factory.StartNew(() => Navigate(row.Cells[1].Value.ToString()));
                Program.mainForm.lblPostingGroup.Text = driver.Title;

                while (true && !pause)
                {
                    int headers = await Task.Factory.StartNew(() => FindHeader());
                    if (headers > 0)
                    {
                        break;
                    }
                    int header_tags = await Task.Factory.StartNew(() => driver.FindElementsById("errorPageContainer").Count);
                    if (header_tags == 0) break;
                    Program.mainForm.lblTick.Text = "(!) Mạng";
                    await Task.Factory.StartNew(() => Navigate(row.Cells[1].Value.ToString()));
                    await TaskEx.Delay(1000);
                }

                if (Program.mainForm.txtBrowse1.Text == "" && Program.mainForm.txtBrowse2.Text == "" && Program.mainForm.txtBrowse3.Text == "")
                {
                    // Không ảnh
                    if (Program.mainForm.txtContent.Text != "")
                    {
                        if (driver.FindElementsByName("xc_message").Count == 0)
                        {
                            Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblPostingGroup.Text, "Group không cho đăng bài");
                            continue;
                        }
                        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                        var rnd = new Random();
                        string random_tag = " #" + new string(
                            Enumerable.Repeat(chars, rnd.Next(8) + 1)
                                      .Select(s => s[rnd.Next(s.Length)])
                                      .ToArray());
                        driver.ExecuteScript(@"document.getElementsByName('xc_message')[0].innerHTML = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtContent.Text) + random_tag + "';");

                        if (driver.FindElementsByName("view_post").Count == 0)
                        {
                            Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblPostingGroup.Text, "Skip - Không tìm thấy nút đăng bài!");
                            continue;
                        }
                        await Task.Factory.StartNew(() => Click("view_post"));
                        Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblPostingGroup.Text, Uri.UnescapeDataString(driver.Url));
                    }
                    else
                    {
                        // Không ảnh + Không nội dung
                        MessageBox.Show("Điền nội dung trước khi post bài!");
                    }
                }
                else
                {
                    // Có ảnh
                    if (driver.FindElementsByName("lgc_view_photo").Count == 0)
                    {
                        Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblPostingGroup.Text, "Group không cho đăng bài");
                        continue;
                    }
                    await Task.Factory.StartNew(() => Click("lgc_view_photo"));

                    if (driver.FindElementsByName("xc_message").Count == 0 || driver.FindElementsByName("file1").Count == 0 || driver.FindElementsByName("photo_upload").Count == 0)
                    {
                        Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblPostingGroup.Text, "Skip - Không tìm thấy nút đăng bài!");
                        continue;
                    }
                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var rnd = new Random();
                    string random_tag = " #" + new string(
                        Enumerable.Repeat(chars, rnd.Next(8) + 1)
                                  .Select(s => s[rnd.Next(s.Length)])
                                  .ToArray());
                    await Task.Factory.StartNew(() => driver.ExecuteScript(@"document.getElementsByName('xc_message')[0].innerHTML = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtContent.Text) + random_tag + "';"));

                    if (Program.mainForm.txtBrowse1.Text != "" && driver.FindElementsByName("file1").Count > 0)
                    {
                        await Task.Factory.StartNew(() => FileInputAdd("file1", Program.mainForm.txtBrowse1.Text));
                    }
                    if (Program.mainForm.txtBrowse2.Text != "" && driver.FindElementsByName("file2").Count > 0)
                    {
                        await Task.Factory.StartNew(() => FileInputAdd("file2", Program.mainForm.txtBrowse2.Text));
                    }
                    if (Program.mainForm.txtBrowse3.Text != "" && driver.FindElementsByName("file3").Count > 0)
                    {
                        await Task.Factory.StartNew(() => FileInputAdd("file3", Program.mainForm.txtBrowse3.Text));
                    }

                    await Task.Factory.StartNew(() => Click("photo_upload"));

                    String result_url = "";
                    try
                    {
                        Match match = Regex.Match(Uri.UnescapeDataString(driver.Url) + "", @"\?photo_id\=([A-Za-z0-9\-]+)\&", RegexOptions.None);
                        if (match.Success)
                        {
                            result_url = links["fb_photo_id"] + match.Groups[1].Value;
                        }
                        else
                        {
                            match = Regex.Match(Uri.UnescapeDataString(driver.Url) + "", @"\?photo_fbid\=([A-Za-z0-9\-]+)\&", RegexOptions.None);
                            if (match.Success)
                            {
                                result_url = links["fb_photo_id"] + match.Groups[1].Value;
                            }
                            else
                                result_url = Uri.UnescapeDataString(driver.Url);
                        }
                        Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblPostingGroup.Text, result_url);
                    }
                    catch { }
                }

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause || Program.mainForm.dgGroups.Rows.Count == 0)
                        break;
                    Program.mainForm.lblTick.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblTick.Text = "POSTING";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.btnPost.Enabled = true;
            Program.mainForm.txtContent.Enabled = true;
            Program.mainForm.txtDelay.Enabled = true;
            Program.mainForm.txtBrowse1.Enabled = true;
            Program.mainForm.txtBrowse2.Enabled = true;
            Program.mainForm.txtBrowse3.Enabled = true;
            Program.mainForm.btnBrowse1.Enabled = true;
            Program.mainForm.btnBrowse2.Enabled = true;
            Program.mainForm.btnBrowse3.Enabled = true;
            Program.mainForm.dgGroups.Enabled = true;
            Program.mainForm.btnPause.Enabled = false;
            Program.mainForm.lblTick.Text = "Ready";

            MessageBox.Show("Đã hoàn thành đăng bài trong " + Program.mainForm.dgPostResult.Rows.Count + " nhóm!");

            setReady(true);
        }

        public async void AutoInvite()
        {
            setReady(false, "Đang tự động mời nhóm");
            Program.mainForm.lblInviteTick.Text = "Đang mời";
            int delay;

            if (!int.TryParse(Program.mainForm.txtInviteDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                Exceptions_Handler();
            }

            int progress = 0;

            foreach (DataGridViewRow row in Program.mainForm.dgGroups.Rows)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                progress++;
                Program.mainForm.lblInviteProgress.Text = progress + "/" + Program.mainForm.dgGroups.Rows.Count;
                Program.mainForm.lblInvitingGroup.Text = row.Cells[0].Value.ToString();

                if (row.Cells[3].Value.ToString() == "0") continue;

                await Task.Factory.StartNew(() => Navigate(row.Cells[1].Value.ToString()));

                var btnAddMembers = driver.FindElementsByXPath("//a[contains(@href, '/groups/members/search/')]");

                if (btnAddMembers.Count > 0)
                    await Task.Factory.StartNew(() => ClickElement(btnAddMembers[0]));

                try
                {
                    InputValueAdd("query_term", Program.mainForm.txtInviteName.Text);
                    IWebElement form = driver.FindElementsByTagName("form")[1];
                    IWebElement btnSearch = form.FindElements(By.TagName("input"))[4];
                    await Task.Factory.StartNew(() => ClickElement(btnSearch));
                }
                catch
                {
                    // Group không cho mời
                    Program.mainForm.dgInvitedGroups.Rows.Insert(0, "Không cho phép gia nhập!");
                    continue;
                }

                try
                {
                    IWebElement form2 = driver.FindElementsByTagName("form")[2];
                    IWebElement div = form2.FindElements(By.TagName("div"))[1];
                    IWebElement input = div.FindElements(By.TagName("input"))[0];
                    input.Click();
                    var btnSubmits = form2.FindElements(By.TagName("input"));
                    IWebElement btnSubmit = btnSubmits[btnSubmits.Count - 1];
                    btnSubmit.Click();
                    Program.mainForm.dgInvitedGroups.Rows.Insert(0, row.Cells[0].Value.ToString());
                }
                catch
                {
                    // Tìm không thấy
                    Program.mainForm.dgInvitedGroups.Rows.Insert(0, "Không tìm thấy: Đã gia nhập hoặc Tên không đúng!");
                    continue;
                }

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;
                    Program.mainForm.lblInviteTick.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblInviteTick.Text = "Đang mời";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.txtInviteDelay.Enabled = true;
            Program.mainForm.txtInviteName.Enabled = true;
            Program.mainForm.btnInvite.Enabled = true;
            Program.mainForm.btnInvitePause.Enabled = false;
            Program.mainForm.lblInviteTick.Text = "Ready";

            MessageBox.Show("Đã hoàn thành mời " + progress + " nhóm!");

            setReady(true);
        }

        public async void GroupSearch()
        {
            setReady(false, "Đang tự động tìm nhóm");
            int success = 0;
            int skip = 0;
            Program.mainForm.lblSearching.Text = "Đang quét...";

            while (success < 10)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                await Task.Factory.StartNew(() => Navigate(links["fb_group_search_query"] + HttpUtility.UrlEncode(Program.mainForm.txtGroupSearch.Text) + "&s=" + skip));

                var groups = driver.FindElementsByXPath("//form[@method='post']//tbody//tr");

                String[] grname = new String[groups.Count];
                String[] grlink = new String[groups.Count];
                String[] grcount = new String[groups.Count];

                for (int i = 0; i < groups.Count; i++)
                {
                    if (success == 10)
                        break;
                    IWebElement a = groups[i].FindElement(By.TagName("a"));
                    IWebElement div = groups[i].FindElement(By.XPath(".//td//div"));

                    int memnum = 0;
                    string memcount = Regex.Match(div.GetAttribute("innerHTML"), @"\d+\.\d+").Value;
                    if (memcount != "")
                    {
                        memnum = int.Parse(memcount.Replace(".", string.Empty));
                    }
                    else
                    {
                        memcount = Regex.Match(div.GetAttribute("innerHTML"), @"\d+\,\d+").Value;
                        if (memcount != "")
                        {
                            memnum = int.Parse(memcount.Replace(",", string.Empty));
                        }
                        else
                        {
                            memcount = Regex.Match(div.GetAttribute("innerHTML"), @"\d+").Value;
                            memnum = int.Parse(memcount);
                        }
                    }

                    Program.mainForm.lblSearching.Text = "Đang quét: " + a.GetAttribute("innerHTML");

                    if (memnum >= int.Parse(Program.mainForm.txtGroupSearchMin.Text))
                    {
                        //Program.mainForm.dgGroupSearch.Rows.Add(a.GetAttribute("innerHTML"), a.GetAttribute("href"), memcount.Replace(".", string.Empty));
                        //success++;
                        grname[i] = a.GetAttribute("innerHTML");
                        grlink[i] = a.GetAttribute("href");
                        grcount[i] = memnum.ToString();
                    }
                }

                for (int i = 0; i < groups.Count; i++)
                {
                    if (grlink[i] == null)
                        continue;
                    await Task.Factory.StartNew(() => Navigate(grlink[i]));

                    var inputs = driver.FindElementsByXPath("//form[@method='post']//input");
                    if (inputs.Count == 3 && !driver.FindElementByXPath("//form[@method='post']").GetAttribute("action").Contains("canceljoin"))
                    {
                        Program.mainForm.dgGroupSearch.Rows.Add(grname[i], grlink[i], grcount[i]);
                        success++;
                    }
                }

                skip += 10;
            }

            Program.mainForm.txtGroupSearch.Enabled = true;
            Program.mainForm.txtGroupSearchMin.Enabled = true;
            Program.mainForm.btnGroupSearch.Enabled = true;

            MessageBox.Show("Hoàn thành tìm nhóm! (" + success + ")");
            Program.mainForm.lblSearching.Text = "Ready";

            setReady(true, "Tự động tìm nhóm: " + success + "| Ready");
        }

        public async void AutoJoin()
        {
            setReady(false, "Đang tự động gia nhập nhóm");
            int delay;

            if (!int.TryParse(Program.mainForm.txtJoinDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            Program.mainForm.lblJoinTick.Text = "Đang join...";

            while (Program.mainForm.dgGroupSearch.Rows.Count > 0)
            {
                DataGridViewRow row = Program.mainForm.dgGroupSearch.Rows[0];

                await Task.Factory.StartNew(() => Navigate(row.Cells[1].Value.ToString()));

                var inputs = driver.FindElementsByXPath("//form[@method='post']//input");
                if (inputs.Count == 3)
                {
                    ClickElement(inputs[2]);
                }
                else
                {
                    Program.mainForm.dgGroupSearch.Rows.Remove(row);
                    continue;
                }

                Program.mainForm.dgGroupSearch.Rows.Remove(row);

                for (int i = 0; i < delay + 1; i++)
                {
                    Program.mainForm.lblJoinTick.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblJoinTick.Text = "Đang join";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.lblJoinTick.Text = "Ready";

            Program.mainForm.txtJoinDelay.Enabled = true;
            Program.mainForm.btnGroupJoin.Enabled = true;

            MessageBox.Show("Hoàn thành gia nhập nhóm!");

            setReady(true, "Hoàn thành gia nhập nhóm | Ready");
        }

        public async void AutoComment2()
        {
            setReady(false, "Đang tự động bình luận nhóm");
            int delay;

            if (!int.TryParse(Program.mainForm.txtCommentDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            Program.mainForm.lblCommentTick.Text = "Đang Comment";

            while (Program.mainForm.dgCommentBrowse.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                string post_url = Program.mainForm.dgCommentBrowse.Rows[0].Cells[0].Value.ToString();

                await Task.Factory.StartNew(() => Navigate(post_url));

                while (true && !pause)
                {
                    int headers = await Task.Factory.StartNew(() => FindHeader());
                    if (headers > 0)
                    {
                        break;
                    }
                    Program.mainForm.lblCommentTick.Text = "(!) Mạng";
                    await Task.Factory.StartNew(() => Navigate(post_url));
                    await TaskEx.Delay(1000);
                }

                Program.mainForm.lblCommenting.Text = driver.Title;

                if (await Task.Factory.StartNew(() => driver.FindElementsByName("comment_text").Count) == 0)
                {
                    Program.mainForm.dgCommentBrowse.Rows.RemoveAt(0);
                    Program.mainForm.lblCommentTick.Text = "Skip bài đăng";
                    Program.mainForm.dgComment.Rows.Insert(0, driver.Title, "Skip - Đang chờ phê duyệt");
                    continue;
                }

                // InputValueAdd("comment_text", Program.mainForm.txtComment.Text);
                await Task.Factory.StartNew(() => driver.ExecuteScript(@"document.getElementsByName('comment_text')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtComment.Text) + "';"));

                IWebElement btnSubmit = driver.FindElementByXPath("//form[@method='post']//input[@type='submit']");
                await Task.Factory.StartNew(() => ClickElement(btnSubmit));
                try
                {
                    Program.mainForm.dgComment.Rows.Insert(0, driver.Title, driver.Url);
                }
                catch { }

                Program.mainForm.dgCommentBrowse.Rows.RemoveAt(0);

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;

                    Program.mainForm.lblCommentTick.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblCommentTick.Text = "Đang Comment";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.lblCommentTick.Text = "Ready";
            Program.mainForm.txtComment.Enabled = true;
            Program.mainForm.txtCommentDelay.Enabled = true;
            Program.mainForm.btnCommentPause.Enabled = false;
            Program.mainForm.btnCommentBrowse.Enabled = true;
            Program.mainForm.btnCommentImportComment.Enabled = true;
            Program.mainForm.dgCommentBrowse.Enabled = true;

            MessageBox.Show("Hoàn thành bình luận nhóm!");
            setReady(true, "Bình luận nhóm hoàn thành! | Ready");
        }

        public async void AutoCommentScan()
        {
            setReady(false, "Đang quét bài đăng nhóm");
            int delay;

            if (!int.TryParse(Program.mainForm.txtCommentDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            await Task.Factory.StartNew(() => Navigate(links["fb_groups"]));

            List<string> groups = new List<string>();
            var e2 = driver.FindElementsByXPath("//table//tbody//tr//td//div");
            if (e2.Count < 4)
            {
                await Task.Factory.StartNew(() => Navigate(links["fb_groups_2"]));
                var divs = driver.FindElementsByXPath("//div[@id='root']//div");
                var h3 = divs[1].FindElements(By.TagName("h3"));
                foreach (IWebElement _h3 in h3)
                {
                    var k = _h3.FindElement(By.TagName("a"));
                    groups.Add(k.GetAttribute("href"));
                }
            }
            else
            {
                var e = e2[3].FindElements(By.XPath(".//li//table//tbody//tr//td//a"));

                foreach (IWebElement k in e)
                {
                    groups.Add(k.GetAttribute("href"));
                }
            }

            foreach (string group in groups)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                await Task.Factory.StartNew(() => Navigate(group));
                Program.mainForm.lblCommenting.Text = driver.Title;

                string post_xpath = "";
                if (Program.mainForm.cbCommentOnlyMe.Checked)
                    post_xpath = "//div[@id='m_group_stories_container']//a[contains(@href,'fref=nf') and contains(@href, '" + user_id + "')]";
                else
                    post_xpath = "//div[@id='m_group_stories_container']//a[contains(@href,'fref=nf')]";
                var post_match = driver.FindElementsByXPath(post_xpath);
                if (post_match.Count > 0)
                {
                    var post_url = post_match[0].FindElements(By.XPath("(.//ancestor::div[@id])[last()]")); //(.//ancestor::div[@id])[last()]
                    if (post_url.Count > 0)
                    {
                        var url_to_comment_a = driver.FindElements(By.XPath("//a[contains(@href, 'view=permalink')]"));
                        if (Program.mainForm.cbCommentOnlyMe.Checked)
                        {
                            url_to_comment_a = post_url[0].FindElements(By.XPath(".//a[contains(@href, 'view=permalink')]"));
                        }

                        await Task.Factory.StartNew(() => ClickElement(url_to_comment_a[0]));

                        if (await Task.Factory.StartNew(() => driver.FindElementsByName("comment_text").Count) == 0)
                        {
                            Program.mainForm.dgCommentBrowse.Rows.RemoveAt(0);
                            Program.mainForm.lblCommentTick.Text = "Skip bài đăng";
                            Program.mainForm.dgComment.Rows.Insert(0, driver.Title, "Skip - Đang chờ phê duyệt");
                            continue;
                        }

                        await Task.Factory.StartNew(() => driver.ExecuteScript(@"document.getElementsByName('comment_text')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtComment.Text) + "';"));

                        IWebElement btnSubmit = driver.FindElementByXPath("//form[@method='post']//input[@type='submit']");
                        await Task.Factory.StartNew(() => ClickElement(btnSubmit));
                        try
                        {
                            Program.mainForm.dgComment.Rows.Insert(0, driver.Title, driver.Url);
                        }
                        catch { }

                        for (int i = 0; i < delay + 1; i++)
                        {
                            if (pause)
                                break;

                            Program.mainForm.lblCommentTick.Text = delay - i + "";
                            if (i == delay)
                            {
                                Program.mainForm.lblCommentTick.Text = "Đang Comment";
                            }
                            await TaskEx.Delay(1000);
                        }
                    }
                }
            }

            Program.mainForm.lblCommenting.Text = "";
            Program.mainForm.lblCommentTick.Text = "Ready";
            Program.mainForm.btnCommentScan.Enabled = true;
            Program.mainForm.btnCommentScanPause.Enabled = false;
            MessageBox.Show("Đã quét xong bài đăng nhóm!");
            setReady(true);
        }

        public async void ImportFriendList()
        {
            setReady(false, "Đang nhập danh sách bạn bè");

            if (user_id == "")
            {
                MessageBox.Show("Ứng dụng không thể lấy được USER ID của bạn!\nQuá trình này sẽ diễn ra không thành công!");
            }

            await Task.Factory.StartNew(() => Navigate("https://m.facebook.com/" + user_id + "?v=friends&refid=17"));

            while (true)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                var profiles = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, 'fref=fr')]"));
                if (profiles.Count == 0)
                    break;

                foreach (IWebElement profile in profiles)
                {
                    if (profile.Text == "") continue;
                    Program.mainForm.dgUID.Rows.Insert(0, profile.Text, profile.GetAttribute("href"));
                    await TaskEx.Delay(10);
                }

                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
                var m_more_friends = await Task.Factory.StartNew(() => driver.FindElementsById("m_more_friends"));
                if (m_more_friends.Count == 0)
                    break;
                else
                    await Task.Factory.StartNew(() => ClickElement(m_more_friends[0].FindElement(By.TagName("a"))));
            }

            MessageBox.Show("Nhập danh sách bạn bè thành công!");
            Program.mainForm.btnPMImportFriends.Enabled = true;
            setReady(true, "Nhập thành công danh sách bạn bè | Ready");
        }

        public async void GroupImportFriends()
        {
            setReady(false, "Đang nhập danh sách bạn bè");

            if (user_id == "")
            {
                MessageBox.Show("Ứng dụng không thể lấy được USER ID của bạn!\nQuá trình này sẽ diễn ra không thành công!");
            }

            await Task.Factory.StartNew(() => Navigate("https://m.facebook.com/" + user_id + "?v=friends&refid=17"));

            while (true)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                var profiles = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, 'fref=fr')]"));
                if (profiles.Count == 0)
                    break;

                foreach (IWebElement profile in profiles)
                {
                    if (profile.Text == "") continue;
                    Program.mainForm.dgGroups.Rows.Insert(0, profile.Text, profile.GetAttribute("href"));
                    await TaskEx.Delay(10);
                }

                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
                var m_more_friends = await Task.Factory.StartNew(() => driver.FindElementsById("m_more_friends"));
                if (m_more_friends.Count == 0)
                    break;
                else
                    await Task.Factory.StartNew(() => ClickElement(m_more_friends[0].FindElement(By.TagName("a"))));
            }

            MessageBox.Show("Nhập danh sách bạn bè thành công!");
            Program.mainForm.btnGroupImportFriends.Enabled = true;
            setReady(true, "Nhập thành công danh sách bạn bè | Ready");
        }

        public async void ImportGroupMembers(String group_url)
        {
            setReady(false, "Đang Import từ Group");

            await Task.Factory.StartNew(() => Navigate(group_url));

            int progress = 0;

            while (true)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                var members = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//table[contains(@id, 'member_')]"));
                if (members.Count == 0)
                {
                    break;
                }

                foreach (IWebElement member in members)
                {
                    if (member.FindElement(By.TagName("a")).Text == "") continue;
                    Program.mainForm.dgUID.Rows.Insert(0, member.FindElement(By.TagName("a")).Text, member.FindElement(By.TagName("a")).GetAttribute("href"));
                    await TaskEx.Delay(10);
                    progress++;
                }

                var more = await Task.Factory.StartNew(() => driver.FindElementsById("m_more_item"));
                if (more.Count == 0)
                    break;
                await Task.Factory.StartNew(() => ClickElement(more[0].FindElement(By.TagName("a"))));
            }

            if (progress > 0)
            {
                MessageBox.Show("Import Group xong! (" + progress + " thành viên)");
            }
            else
            {
                MessageBox.Show("Không tìm thấy thành viên nào cả! Hãy kiểm tra lại Group ID!");
            }

            Program.mainForm.txtPMImportGroup.Enabled = true;
            Program.mainForm.btnPMImportGroup.Enabled = true;

            setReady(true, "Số thành viên: " + progress + " | Ready");
        }

        public async void ImportProfileFriends(String profile_url)
        {
            setReady(false, "Đang Import từ Profile");
            if (!profile_url.Contains('/')) profile_url = "https://m.facebook.com/" + profile_url;
            await Task.Factory.StartNew(() => Navigate(profile_url));

            int progress = 0;

            var friend_list_url = driver.FindElementsByXPath("//a[contains(@href, 'v=friends')]");

            if (friend_list_url.Count > 0)
            {
                await Task.Factory.StartNew(() => ClickElement(friend_list_url[0]));

                while (true)
                {
                    if (pause)
                    {
                        pause = false;
                        break;
                    }

                    var members = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, 'fref=fr_tab')]"));
                    if (members.Count == 0)
                    {
                        break;
                    }

                    foreach (IWebElement member in members)
                    {
                        if (member.Text == "") continue;
                        Program.mainForm.dgUID.Rows.Insert(0, member.Text, member.GetAttribute("href"));
                        await TaskEx.Delay(10);
                        progress++;
                    }

                    var more = await Task.Factory.StartNew(() => driver.FindElementsById("m_more_friends"));
                    if (more.Count == 0)
                        break;
                    await Task.Factory.StartNew(() => ClickElement(more[0].FindElement(By.TagName("a"))));
                }
            }


            if (progress > 0)
            {
                MessageBox.Show("Import Profile xong! (" + progress + " bạn bè)");
            }
            else
            {
                MessageBox.Show("Không tìm thấy kết quả nào cả! Hãy kiểm tra lại ID Profile");
            }

            Program.mainForm.txtPMImportProfile.Enabled = true;
            Program.mainForm.btnPMImportProfile.Enabled = true;

            setReady(true, "Số bạn bè: " + progress + " | Ready");
        }

        public async void AutoPM()
        {
            setReady(false, "Đang gửi tin nhắn");

            int delay;

            if (!int.TryParse(Program.mainForm.txtPMDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            while (Program.mainForm.dgUID.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                String user_url = Program.mainForm.dgUID.Rows[0].Cells[1].Value.ToString();

                await Task.Factory.StartNew(() => Navigate(user_url));

                var messages = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, '/messages/thread/')]"));
                if (messages.Count == 0)
                {
                    try
                    {
                        Program.mainForm.dgUID.Rows.RemoveAt(0);
                        Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Profile không cho gửi tin nhắn");
                    }
                    catch { }
                    continue;
                }

                try
                {
                    messages[0].Click();
                }
                catch { }

                var bodies = driver.FindElementsByName("body");

                if (bodies.Count == 0)
                {
                    try
                    {
                        Program.mainForm.dgUID.Rows.RemoveAt(0);
                    }
                    catch { }
                    continue;
                }
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var rnd = new Random();
                string random_tag = " #" + new string(
                    Enumerable.Repeat(chars, rnd.Next(8) + 1)
                              .Select(s => s[rnd.Next(s.Length)])
                              .ToArray());

                await Task.Factory.StartNew(() => InputValueAdd("body", Program.mainForm.txtPM.Text.Replace("{username}", driver.Title) + random_tag));

                var btnSends = driver.FindElementsByName("send");

                if (btnSends.Count > 0)
                    await Task.Factory.StartNew(() => ClickElement(btnSends[0]));

                Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Đã gửi tin nhắn");

                try
                {
                    Program.mainForm.dgUID.Rows.RemoveAt(0);
                }
                catch { }

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;
                    Program.mainForm.lblPMTick.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblPMTick.Text = "Đang PM";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.lblPMTick.Text = "Ready";

            Program.mainForm.dgUID.Enabled = true;
            Program.mainForm.txtPM.Enabled = true;
            Program.mainForm.txtPMDelay.Enabled = true;
            Program.mainForm.btnPM.Enabled = true;

            MessageBox.Show("Hoàn thành gửi tin nhắn! Số lượng gửi: " + Program.mainForm.dgPMResult.Rows.Count);
            setReady(true, "Số lượng gửi: " + Program.mainForm.dgPMResult.Rows.Count + "| Ready");
        }

        public async void AutoEdit()
        {
            setReady(false, "Đang tự động Edit bài");
            int delay;

            if (!int.TryParse(Program.mainForm.txtEditDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            Program.mainForm.lblEditTick.Text = "Đang Edit";

            while (Program.mainForm.dgEditBrowse.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                string post_url = Program.mainForm.dgEditBrowse.Rows[0].Cells[0].Value.ToString();

                await Task.Factory.StartNew(() => Navigate(post_url));

                while (true && !pause)
                {
                    int headers = await Task.Factory.StartNew(() => FindHeader());
                    if (headers > 0)
                    {
                        break;
                    }
                    Program.mainForm.lblEditTick.Text = "(!) Mạng";
                    await Task.Factory.StartNew(() => Navigate(post_url));
                    await TaskEx.Delay(1000);
                }

                Program.mainForm.lblEditing.Text = driver.Title;

                if (await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, '/edit/post/')]").Count) == 0)
                {
                    Program.mainForm.dgEditBrowse.Rows.RemoveAt(0);
                    Program.mainForm.lblEditTick.Text = "Skip bài edit";
                    Program.mainForm.dgEditResult.Rows.Insert(0, driver.Title, "Skip - Không thể edit");
                    continue;
                }
                await Task.Factory.StartNew(() => ClickElement(driver.FindElementByXPath("//a[contains(@href, '/edit/post/')]")));

                await Task.Factory.StartNew(() => driver.ExecuteScript(@"document.getElementsByName('p_text')[0].innerHTML = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtEditContent.Text) + "';"));

                IWebElement btnSubmit = driver.FindElementByXPath("//form[@method='post']//input[@type='submit']");
                await Task.Factory.StartNew(() => ClickElement(btnSubmit));
                Program.mainForm.dgEditResult.Rows.Insert(0, driver.Title, driver.Url);
                Program.mainForm.dgEditBrowse.Rows.RemoveAt(0);

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;

                    Program.mainForm.lblEditTick.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblEditTick.Text = "Đang Edit";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.lblEditTick.Text = "Ready";
            Program.mainForm.txtEditContent.Enabled = true;
            Program.mainForm.txtEditDelay.Enabled = true;
            Program.mainForm.btnEditPause.Enabled = false;
            Program.mainForm.btnEditBrowse.Enabled = true;
            Program.mainForm.btnEdit.Enabled = true;
            Program.mainForm.dgEditBrowse.Enabled = true;

            MessageBox.Show("Hoàn thành Edit bài!");
            setReady(true, "Edit hoàn thành! | Ready");
        }

        public async void AutoAddFriends()
        {
            setReady(false, "Đang tự động kết bạn");
            int delay;

            if (!int.TryParse(Program.mainForm.txtPMDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            Program.mainForm.lblPMSendFrRequestsTick.Text = "Đang KB";

            while (Program.mainForm.dgUID.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                string friend_url = Program.mainForm.dgUID.Rows[0].Cells[1].Value.ToString();

                await Task.Factory.StartNew(() => Navigate(friend_url));

                while (true && !pause)
                {
                    int headers = await Task.Factory.StartNew(() => FindHeader());
                    if (headers > 0)
                    {
                        break;
                    }
                    Program.mainForm.lblPMSendFrRequestsTick.Text = "(!) Mạng";
                    await Task.Factory.StartNew(() => Navigate(friend_url));
                    await TaskEx.Delay(1000);
                }

                var btnAddFriend = driver.FindElementsByXPath("//a[contains(@href, 'profile_add_friend.php')]");
                if (btnAddFriend.Count == 1)
                {
                    await Task.Factory.StartNew(() => ClickElement(btnAddFriend[0]));
                    Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Đã gửi lời mời kết bạn!");
                }
                else
                {
                    Program.mainForm.dgUID.Rows.RemoveAt(0);
                    Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Không cho phép kết bạn!");
                    continue;
                }

                Program.mainForm.dgUID.Rows.RemoveAt(0);

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;

                    Program.mainForm.lblPMSendFrRequestsTick.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblPMSendFrRequestsTick.Text = "Đang KB";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.btnPMSendFrRequests.Enabled = true;
            Program.mainForm.btnPMSendFrRequestsPause.Enabled = false;
            Program.mainForm.lblPMSendFrRequestsTick.Text = "Ready";

            MessageBox.Show("Hoàn thành kết bạn!");
            setReady(true, "Kết bạn hoàn thành! | Ready");
        }

        public async Task getPages()
        {
            if (!ready) return;
            setReady(false);

            if (pages.Count > 0) return;

            if (user_id != "")
            {
                await Task.Factory.StartNew(() => Navigate("https://m.facebook.com/" + user_id + "?v=likes&refid=17"));

                var afrefs = driver.FindElementsByXPath("//a[contains(@href, 'fref=none')]");

                if (afrefs.Count > 0)
                {
                    foreach (IWebElement afref in afrefs)
                    {
                        pages.Add(afref.Text, afref.GetAttribute("href"));
                    }
                    var next = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, 'v=likes')]"));
                    if (next.Count > 0)
                    {
                        await Task.Factory.StartNew(() => ClickElement(next[0]));

                        var afrefs_more = driver.FindElementsByXPath("//a[contains(@href, 'fref=none')]");

                        if (afrefs_more.Count > 0)
                        {
                            foreach (IWebElement afref_more in afrefs_more)
                            {
                                pages.Add(afref_more.Text, afref_more.GetAttribute("href"));
                            }
                        }
                    }
                }
            }

            setReady(true);
        }

        public async Task getEvents()
        {
            if (user_id == "") return;

            if (!ready) return;
            setReady(false);

            if (events.Count > 0) return;

            await Task.Factory.StartNew(() => Navigate("https://m.facebook.com/events/upcoming"));

            var upcoming_events = driver.FindElementsByXPath("//a[contains(@href,'arefdashboardfilter=upcoming')]");

            if (upcoming_events.Count > 0)
            {
                foreach (IWebElement upcoming_event in upcoming_events)
                {
                    IWebElement upcoming_event_name = upcoming_event.FindElement(By.XPath("..//h4"));
                    events.Add(upcoming_event_name.Text, upcoming_event.GetAttribute("href"));
                }
            }

            setReady(true);
        }

        public async void FanpageComment()
        {
            setReady(false, "Đang bình luận Fanpage");

            int delay;

            if (!int.TryParse(Program.mainForm.txtFanpageCommentDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            if (Program.mainForm.panelFanpageComment.Controls.Count > 0)
            {
                Program.mainForm.lblFanpageCommentTick.Text = "Đang BL";

                foreach (CheckBox _page in Program.mainForm.panelFanpageComment.Controls)
                {
                    if (pause)
                    {
                        pause = false;
                        break;
                    }

                    if (_page.Checked == false) continue;

                    Program.mainForm.lblFanpageCommenting.Text = _page.Text;

                    await Task.Factory.StartNew(() => Navigate(pages[_page.Text]));

                    var page_posts = driver.FindElementsByXPath("//div[@id='recent']//div[@id]");

                    if (page_posts.Count > 0)
                    {
                        var post = page_posts[0].FindElements(By.XPath(".//a[contains(@href, 'story.php') or contains(@href, '/photos/')]"));

                        if (post.Count > 0)
                        {
                            await Task.Factory.StartNew(() => ClickElement(post[0]));

                            var comments = driver.FindElementsByXPath("//a[contains(@href, '/comment/replies/')]");

                            if (comments.Count > 0)
                            {
                                await Task.Factory.StartNew(() => ClickElement(comments[0]));
                            }

                            if (await Task.Factory.StartNew(() => driver.FindElementsByName("comment_text").Count) == 0) continue;

                            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                            var rnd = new Random();
                            string random_tag = " #" + new string(
                                Enumerable.Repeat(chars, rnd.Next(8) + 1)
                                          .Select(s => s[rnd.Next(s.Length)])
                                          .ToArray());

                            await Task.Factory.StartNew(() => driver.ExecuteScript(@"document.getElementsByName('comment_text')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtFanpageComment.Text) + random_tag + "';"));

                            IWebElement btnSubmit = driver.FindElementByXPath("//form[@method='post']//input[@type='submit']");
                            await Task.Factory.StartNew(() => ClickElement(btnSubmit));

                            Program.mainForm.dgFanpageCommentResults.Rows.Insert(0, _page.Text, driver.Url);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    for (int i = 0; i < delay + 1; i++)
                    {
                        if (pause)
                            break;

                        Program.mainForm.lblFanpageCommentTick.Text = delay - i + "";
                        if (i == delay)
                        {
                            Program.mainForm.lblFanpageCommentTick.Text = "Đang BL";
                        }
                        await TaskEx.Delay(1000);
                    }
                }
            }

            Program.mainForm.lblFanpageCommentTick.Text = "Ready";
            Program.mainForm.btnFanpageComment.Enabled = true;
            Program.mainForm.btnFanpageCommentPause.Enabled = false;

            MessageBox.Show("Hoàn thành bình luận Fanpage!");
            setReady(true, "Bình luận xong!");
        }

        public async void FanpagePost()
        {
            setReady(false, "Đang quảng cáo Fanpage");

            int delay;

            if (!int.TryParse(Program.mainForm.txtFanpageGroupPostDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            string text_to_share = "";

            Program.mainForm.lblFanpageGroupTick.Text = "Lấy nội dung đăng...";

            if (Program.mainForm.radFanpageImage.Checked)
            {
                string page_name = Program.mainForm.cbFanpage.Text;
                await Task.Factory.StartNew(() => Navigate(pages[page_name]));
                var page_posts = driver.FindElementsByXPath("//div[@id='recent']//div[@id]//a[contains(@href, '/photos/')]");

                if (page_posts.Count > 0)
                {
                    text_to_share = page_posts[0].GetAttribute("href");
                }
            }

            if (Program.mainForm.radFanpageOther.Checked)
            {
                text_to_share = Program.mainForm.txtFanpageOther.Text;
            }

            if (text_to_share == "")
            {
                if (Program.mainForm.txtFanpageURLText.Text != "") text_to_share = Program.mainForm.txtFanpageURLText.Text + "\r\n";
                text_to_share += pages[Program.mainForm.cbFanpage.Text];
            }

            Program.mainForm.lblFanpageGroupTick.Text = "Quét mục tiêu";
            List<string> post_targets = new List<string>();

            if (Program.mainForm.cbFanpageGroups.Checked)
            {
                await Task.Factory.StartNew(() => Navigate(links["fb_groups"]));

                var e2 = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//table//tbody//tr//td//div"));
                if (e2.Count < 4)
                {
                    await Task.Factory.StartNew(() => Navigate(links["fb_groups_2"]));
                    var divs = driver.FindElementsByXPath("//div[@id='root']//div");
                    var h3 = divs[1].FindElements(By.TagName("h3"));
                    foreach (IWebElement _h3 in h3)
                    {
                        var k = _h3.FindElement(By.TagName("a"));
                        post_targets.Add(k.GetAttribute("href").Replace("/m.facebook", "/www.facebook"));
                    }
                }
                else
                {
                    var e = e2[3].FindElements(By.XPath(".//li//table//tbody//tr//td//a"));

                    foreach (IWebElement k in e)
                    {
                        post_targets.Add(k.GetAttribute("href").Replace("/m.facebook", "/www.facebook"));
                    }
                }
            }
            Program.mainForm.lblFanpageGroupTick.Text = "Khởi tạo";
            var profile = new OpenQA.Selenium.Firefox.FirefoxProfile();
            profile.SetPreference("general.useragent.override", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
            profile.SetPreference("permissions.default.image", 2);
            //profile.SetPreference("permissions.default.stylesheet", 2);
            profile.AddExtension("App/Firefox/firebug.xpi");

            OpenQA.Selenium.Firefox.FirefoxBinary firefox = new OpenQA.Selenium.Firefox.FirefoxBinary("App/Firefox/firefox.exe");
            OpenQA.Selenium.Firefox.FirefoxDriver driver2 = new OpenQA.Selenium.Firefox.FirefoxDriver(firefox, profile);
            driver2.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
            driver2.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
            driver2.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(60));

            Program.mainForm.lblFanpageGroupTick.Text = "Mã hóa";
            //await Task.Factory.StartNew(() => Navigate2(driver2, "https://www.facebook.com/"));
            WebDriverWait wait = new WebDriverWait(driver2, TimeSpan.FromSeconds(60));

            try
            {
                driver2.Url = "https://www.facebook.com/";
            }
            catch
            {
                ((IJavaScriptExecutor)driver2).ExecuteScript("return window.stop()");
            }

            wait.Until<Boolean>((d) =>
            {
                return ((IJavaScriptExecutor)driver2).ExecuteScript("return document.readyState").Equals("complete");
            });

            try
            {
                driver2.ExecuteScript(@"document.getElementsByName('email')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtUser.Text) + "';");
            }
            catch
            {
                MessageBox.Show("Đường truyền mạng quá chậm, vui lòng thử lại sau!");
                goto FanpageGroupFinish;
            }
            
            driver2.ExecuteScript(@"document.getElementsByName('pass')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtPass.Text) + "';");
            driver2.ExecuteScript(@"document.getElementById('loginbutton').click();");

            wait.Until<Boolean>((d) =>
            {
                return ((IJavaScriptExecutor)driver2).ExecuteScript("return document.readyState").Equals("complete");
            });

            await TaskEx.Delay(2000);

            Program.mainForm.lblFanpageGroupTick.Text = "Bắt đầu đăng";

            if (Program.mainForm.cbFanpageGroups.Checked)
            {
                foreach (string post_target in post_targets)
                {
                    if (pause)
                    {
                        pause = false;
                        break;
                    }
                    ((IJavaScriptExecutor)driver2).ExecuteScript("return window.stop()");
                    //await Task.Factory.StartNew(() => Navigate2(driver2, post_target));

                    try
                    {
                        driver2.Url = post_target;
                    }
                    catch
                    {
                        ((IJavaScriptExecutor)driver2).ExecuteScript("return window.stop()");
                        continue;
                    }

                    try
                    {
                        IAlert alert = driver2.SwitchTo().Alert();
                        alert.Accept();
                    }
                    catch (NoAlertPresentException Ex)
                    { }

                    try
                    {
                        wait.Until<Boolean>((d) =>
                        {
                            return ((IJavaScriptExecutor)driver2).ExecuteScript("return document.readyState").Equals("complete");
                        });
                    }
                    catch
                    {
                        ((IJavaScriptExecutor)driver2).ExecuteScript("return window.stop()");
                        continue;
                    }
                    Program.mainForm.lblFanpageGroupPosting.Text = "[" + (post_targets.IndexOf(post_target) + 1) + "/" + post_targets.Count + "]" + driver2.Title;

                    var xhpc = await Task.Factory.StartNew(() => driver2.FindElementsByXPath("//a[@data-endpoint='/ajax/composerx/attachment/group/post/']"));
                    if (xhpc.Count == 0) continue;
                    await Task.Factory.StartNew(() => ClickElement(xhpc[0]));
                    await TaskEx.Delay(1000);

                    var xhpc_text = wait.Until<IWebElement>((d) =>
                    {
                        return d.FindElement(By.XPath("//form[contains(@action, 'updatestatus.php')]//textarea[@name='xhpc_message_text']"));
                    });
                    //await Task.Factory.StartNew(() => ClickElement(xhpc_text));

                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var rnd = new Random();
                    string random_tag = " #" + new string(
                        Enumerable.Repeat(chars, rnd.Next(8) + 1)
                                  .Select(s => s[rnd.Next(s.Length)])
                                  .ToArray());
                    driver2.ExecuteScript(@"document.getElementsByName('xhpc_message_text')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(text_to_share) + random_tag + "';");

                    xhpc_text.SendKeys(OpenQA.Selenium.Keys.Enter);

                    await TaskEx.Delay(5000);
                    var btn_post = driver2.FindElementsByXPath("//form[contains(@action, 'updatestatus.php') and @class='']//button[@type='submit']");
                    if (btn_post.Count == 0) continue;
                    await Task.Factory.StartNew(() => ClickElement(btn_post[0]));

                    Program.mainForm.dgFanpageGroupResults.Rows.Insert(0, driver2.Title, post_target);
                    for (int i = 0; i < delay + 1; i++)
                    {
                        if (pause)
                            break;

                        Program.mainForm.lblFanpageGroupTick.Text = delay - i + "";
                        if (i == delay)
                        {
                            Program.mainForm.lblFanpageGroupTick.Text = "Đang đăng bài";
                        }
                        await TaskEx.Delay(1000);
                    }
                }
            }

            FanpageGroupFinish:

            if (driver2 != null)
            {
                driver2.Quit();
            }

            Program.mainForm.lblFanpageGroupTick.Text = "Ready";
            Program.mainForm.btnFanpageGroupPost.Enabled = true;
            Program.mainForm.btnFanpageGroupPostPause.Enabled = false;

            MessageBox.Show("Hoàn thành quảng bá Fanpage!");
            setReady(true, "Hoàn thành QC Fanpage!");
        }

        public async void FanpageInviteFriends()
        {
            setReady(false);
            string page_name = Program.mainForm.cbFanpage.Text;
            await Task.Factory.StartNew(() => Navigate(pages[page_name]));
            var invite_buttons = driver.FindElementsByXPath("//a[contains(@href,'invite_friends')]");
            if (invite_buttons.Count == 0)
            {
                var view_more = driver.FindElementsByXPath("//a[contains(@href,'/pages/context/hidden/')]");
                if (view_more.Count > 0)
                {
                    await Task.Factory.StartNew(() => ClickElement(view_more[0]));
                    invite_buttons = driver.FindElementsByXPath("//a[contains(@href,'invite_friends')]");
                }
            }
            if (invite_buttons.Count > 0)
            {
                await Task.Factory.StartNew(() => ClickElement(invite_buttons[0]));

                Program.mainForm.btnFanpageInviteFriends.Enabled = true;

                int success = 0;

                while (true)
                {
                    if (pause)
                    {
                        pause = false;
                        break;
                    }

                    var send_page_invite = driver.FindElementsByXPath("//a[contains(@href,'/send_page_invite/')]");
                    if (send_page_invite.Count == 0) break;

                    Program.mainForm.btnFanpageInviteFriends.Text = "Dừng [" + success + "/" + send_page_invite.Count + "]";

                    await Task.Factory.StartNew(() => ClickElement(send_page_invite[0]));

                    success++;
                }
            }

            Program.mainForm.btnFanpageInviteFriends.Text = "Mời tất cả bạn bè";
            Program.mainForm.btnFanpageInviteFriends.Enabled = true;

            MessageBox.Show("Hoàn thành mời bạn bè!");
            setReady(true);
        }

        public async void EventsInviteFriends()
        {
            int delay;

            if (!int.TryParse(Program.mainForm.txtEventInviteDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            setReady(false);

            string event_name = Program.mainForm.cbEvents.Text;
            await Task.Factory.StartNew(() => Navigate(events[event_name]));
            var invite_buttons = driver.FindElementsByXPath("//a[contains(@href,'friendselect.php')]");

            if (invite_buttons.Count > 0)
            {
                await Task.Factory.StartNew(() => ClickElement(invite_buttons[0]));

                var friends_to_invite = driver.FindElementsByXPath("//a[contains(@href,'friendinvite.php')]");

                if (friends_to_invite.Count > 0)
                {
                    string invite_url = friends_to_invite[0].GetAttribute("href").Substring(0, friends_to_invite[0].GetAttribute("href").IndexOf("&ids=") + 5);

                    List<string> ids = new List<string>();

                    while (true)
                    {
                        string _ids = "";

                        friends_to_invite = driver.FindElementsByXPath("//a[contains(@href,'friendinvite.php')]");
                        if (friends_to_invite.Count == 0) break;
                        foreach (IWebElement friend_to_invite in friends_to_invite)
                        {
                            string friend_uid = friend_to_invite.GetAttribute("href").Substring(friend_to_invite.GetAttribute("href").IndexOf("&ids=") + 5);
                            string friend_name = friend_to_invite.FindElement(By.XPath("../..")).Text.Replace(friend_to_invite.Text, "");
                            _ids += friend_uid + ",";
                            Program.mainForm.lblEventInviting.Text = friend_name;
                            Program.mainForm.dgEventInvite.Rows.Insert(0, friend_name, "https://www.facebook.com/" + friend_uid);
                        }

                        ids.Add(_ids.Substring(0, _ids.Length - 1));

                        int _out;
                        var friends_select = driver.FindElementsByXPath("(//a[contains(@href,'friendselect.php')])[last()]");
                        if (friends_select.Count == 0 || int.TryParse(friends_select[0].Text, out _out)) break;

                        await Task.Factory.StartNew(() => ClickElement(friends_select[0]));
                    }

                    foreach (string _id in ids)
                    {
                        if (pause)
                        {
                            pause = false;
                            break;
                        }

                        await Task.Factory.StartNew(() => Navigate(invite_url + _id));
                        await Task.Factory.StartNew(() => ClickElement(driver.FindElementByName("send")));

                        for (int i = 0; i < delay + 1; i++)
                        {
                            if (pause)
                                break;

                            Program.mainForm.lblEventInviteTick.Text = delay - i + "";
                            if (i == delay)
                            {
                                Program.mainForm.lblEventInviteTick.Text = "Đang mời";
                            }
                            await TaskEx.Delay(1000);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Sự kiện đã kết thúc! Không cho phép mời!");
            }

            Program.mainForm.lblEventInviteTick.Text = "Ready";
            Program.mainForm.btnEventInviteFriends.Enabled = true;
            Program.mainForm.btnEventInviteFriendsPause.Enabled = false;

            MessageBox.Show("Hoàn thành mời bạn bè!");
            setReady(true);
        }

        #region OTHER HELPERS
        public void setReady(bool status, String message = "Ready")
        {
            this.ready = status;
            Program.mainForm.lblStatus.Text = message;
            if (status)
            {
                Program.mainForm.imgStatus.Image = System.Drawing.Bitmap.FromFile("green.png");
                pause = false;
                Program.mainForm.btnPauseAll.Enabled = false;
            }
            else
            {
                Program.mainForm.imgStatus.Image = System.Drawing.Bitmap.FromFile("red.gif");
                Program.mainForm.btnPauseAll.Enabled = true;
            }
        }

        public string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        #endregion
    }
}
