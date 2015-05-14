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
            Program.loadingForm.setText("KHỞI TẠO TRÌNH DUYỆT...");
            //Program.loadingForm.Show();
            //Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(() => Program.loadingForm.ShowDialog()));

            t = new System.Threading.Thread(() => Program.loadingForm.ShowDialog());
            t.Start(); // LoadingForm.Show()
            // Bật trình duyệt khi Login
            if (driver == null)
            {
                setReady(false, "Đang khởi tạo trình duyệt");
                var profile = new OpenQA.Selenium.Firefox.FirefoxProfile();
                profile.SetPreference("general.useragent.override", "NokiaC5-00/061.005 (SymbianOS/9.3; U; Series60/3.2 Mozilla/5.0; Profile/MIDP-2.1 Configuration/CLDC-1.1) AppleWebKit/525 (KHTML, like Gecko) Version/3.0 Safari/525 3gpp-gba");
                //profile.SetPreference("webdriver.load.strategy", "unstable");
                //profile.SetPreference("permissions.default.stylesheet", 2);
                profile.SetPreference("permissions.default.image", 2);
                //profile.SetPreference("dom.ipc.plugins.enabled.libflashplayer.so", "false");
                IEnumerable<int> pidsBefore = Process.GetProcessesByName("firefox").Select(p => p.Id);
                try
                {
                    //this.driver = await Task.Factory.StartNew(() => new OpenQA.Selenium.Firefox.FirefoxDriver(profile));
                    this.driver = new OpenQA.Selenium.Firefox.FirefoxDriver(profile);
                }
                catch
                {
                    Program.loadingForm.RequestStop();
                    MessageBox.Show("Hãy cài đặt trình duyệt Firefox 34 trước khi sử dụng chương trình! Download tại: http://kstnk57.com/AUTO/firefox.rar");
                    Exceptions_Handler();
                }
                IEnumerable<int> pidsAfter = Process.GetProcessesByName("firefox").Select(p => p.Id);

                newFirefoxPids = pidsAfter.Except(pidsBefore);

                try
                {
                    foreach (int pid in newFirefoxPids)
                    {
                        int hWnd = Process.GetProcessById(pid).MainWindowHandle.ToInt32();
                        ShowWindow(hWnd, SW_HIDE);
                    }
                }
                catch
                {
                    // newFirefoxPids.Count == 0
                    Program.loadingForm.RequestStop();
                    MessageBox.Show("Không tìm thấy cửa sổ Firefox!");
                    Exceptions_Handler();
                }

                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
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
                Program.mainForm.btnGroupSearchFr.Enabled = true;
                Program.mainForm.btnGroupJoin.Enabled = true;
                Program.mainForm.btnTag.Enabled = true;
                Program.mainForm.btnPMImportFriends.Enabled = true;
                Program.mainForm.btnPM.Enabled = true;
                Program.mainForm.btnPMSendFrRequests.Enabled = true;
                Program.mainForm.btnPMImportProfile.Enabled = true;
                Program.mainForm.btnPMImportGroup.Enabled = true;
                Program.mainForm.btnCommentImportComment.Enabled = true;
                Program.mainForm.btnReply.Enabled = true;
                Program.mainForm.btnEdit.Enabled = true;
                Program.mainForm.btnPostImportGroups.Enabled = true;

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

                //Program.mainForm.Focus();
                Program.loadingForm.setText("ĐĂNG NHẬP THÀNH CÔNG! ĐANG TẢI DANH SÁCH NHÓM...");

                try
                {
                    DataSet DS = new DataSet();
                    DS.ReadXml(user_id + "_groups.xml");
                    foreach (DataRow dr in DS.Tables[0].Rows)
                    {
                        Program.mainForm.dgGroups.Rows.Add(dr[0], dr[1], dr[2]);
                    }
                }
                catch { }
                if (Program.mainForm.dgGroups.RowCount == 0) await getGroups();
                else
                {
                    Program.loadingForm.RequestStop();
                    t.Abort();
                    t.Join();
                    Program.mainForm.lblProgress.Text = "0/" + Program.mainForm.dgGroups.Rows.Count;
                    setReady(true, "Số lượng nhóm: " + Program.mainForm.dgGroups.Rows.Count + " | Ready");
                }

                try
                {
                    Program.mainForm.pbAvatar.Load(links["facebook_graph"] + "/" + user_id + "/picture");
                    Program.mainForm.lblViewProfile.Text = "https://facebook.com/" + user_id;
                }
                catch { }
                var nodes = driver.FindElementsByXPath("//div[@id='bookmarkmenu']//a");
                if (nodes.Count > 0)
                {
                    Program.mainForm.lblUsername.Text = nodes[1].GetAttribute("innerHTML");
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
                Program.mainForm.AcceptButton = Program.mainForm.btnLogin;

                Program.mainForm.txtUser.Enabled = true;
                Program.mainForm.txtPass.Enabled = true;
                Program.mainForm.btnPost.Enabled = false;
                Program.mainForm.btnInvite.Enabled = false;
                Program.mainForm.btnGroupSearch.Enabled = false;
                Program.mainForm.btnGroupSearchFr.Enabled = false;
                Program.mainForm.btnGroupJoin.Enabled = false;
                Program.mainForm.btnTag.Enabled = false;
                Program.mainForm.btnPMImportFriends.Enabled = false;
                Program.mainForm.btnPM.Enabled = false;
                Program.mainForm.btnPMSendFrRequests.Enabled = false;
                Program.mainForm.btnCommentImportComment.Enabled = false;
                Program.mainForm.btnReply.Enabled = false;
                Program.mainForm.btnEdit.Enabled = false;
                Program.mainForm.btnPostImportGroups.Enabled = false;
                Program.mainForm.btnLogin.Enabled = true;
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

            while (Program.mainForm.dgGroups.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                DataGridViewRow row = Program.mainForm.dgGroups.Rows[0];
                progress++;
                Program.mainForm.lblProgress.Text = progress + "/" + Program.mainForm.dgGroups.Rows.Count;

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
                            Program.mainForm.dgGroups.Rows.RemoveAt(0);
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
                            Program.mainForm.dgGroups.Rows.RemoveAt(0);
                            continue;
                        }
                        await Task.Factory.StartNew(() => Click("view_post"));
                        Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblPostingGroup.Text, Uri.UnescapeDataString(driver.Url));
                        Program.mainForm.dgGroups.Rows.RemoveAt(0);
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
                        Program.mainForm.dgGroups.Rows.RemoveAt(0);
                        continue;
                    }
                    await Task.Factory.StartNew(() => Click("lgc_view_photo"));

                    if (driver.FindElementsByName("xc_message").Count == 0 || driver.FindElementsByName("file1").Count == 0 || driver.FindElementsByName("file2").Count == 0 || driver.FindElementsByName("file3").Count == 0 || driver.FindElementsByName("photo_upload").Count == 0)
                    {
                        Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblPostingGroup.Text, "Skip - Không tìm thấy nút đăng bài!");
                        Program.mainForm.dgGroups.Rows.RemoveAt(0);
                        continue;
                    }
                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var rnd = new Random();
                    string random_tag = " #" + new string(
                        Enumerable.Repeat(chars, rnd.Next(8) + 1)
                                  .Select(s => s[rnd.Next(s.Length)])
                                  .ToArray());
                    await Task.Factory.StartNew(() => driver.ExecuteScript(@"document.getElementsByName('xc_message')[0].innerHTML = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtContent.Text) + random_tag + "';"));

                    if (Program.mainForm.txtBrowse1.Text != "")
                    {
                        await Task.Factory.StartNew(() => FileInputAdd("file1", Program.mainForm.txtBrowse1.Text));
                    }
                    if (Program.mainForm.txtBrowse2.Text != "")
                    {
                        await Task.Factory.StartNew(() => FileInputAdd("file2", Program.mainForm.txtBrowse2.Text));
                    }
                    if (Program.mainForm.txtBrowse3.Text != "")
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
                        Program.mainForm.dgGroups.Rows.RemoveAt(0);
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

            while (Program.mainForm.dgGroups.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                DataGridViewRow row = Program.mainForm.dgGroups.Rows[0];
                progress++;
                Program.mainForm.lblInviteProgress.Text = progress + "/" + Program.mainForm.dgGroups.Rows.Count;
                Program.mainForm.lblInvitingGroup.Text = row.Cells[0].Value.ToString();

                String group_id = row.Cells[1].Value.ToString().Substring(30);
                await Task.Factory.StartNew(() => Navigate(links["fb_group_add"] + group_id + "&refid=18"));

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
                    Program.mainForm.dgGroups.Rows.RemoveAt(0);
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
                    Program.mainForm.dgGroups.Rows.RemoveAt(0);
                }
                catch
                {
                    // Tìm không thấy
                    Program.mainForm.dgInvitedGroups.Rows.Insert(0, "Không tìm thấy: Đã gia nhập hoặc Tên không đúng!");
                    Program.mainForm.dgGroups.Rows.RemoveAt(0);
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

                    string memcount = Regex.Match(div.GetAttribute("innerHTML"), @"\d+\.\d+").Value;
                    if (memcount == "")
                        memcount = Regex.Match(div.GetAttribute("innerHTML"), @"\d+").Value;

                    Program.mainForm.lblSearching.Text = "Đang quét: " + a.GetAttribute("innerHTML");

                    if (int.Parse(memcount.Replace(".", string.Empty)) >= int.Parse(Program.mainForm.txtGroupSearchMin.Text))
                    {
                        //Program.mainForm.dgGroupSearch.Rows.Add(a.GetAttribute("innerHTML"), a.GetAttribute("href"), memcount.Replace(".", string.Empty));
                        //success++;
                        grname[i] = a.GetAttribute("innerHTML");
                        grlink[i] = a.GetAttribute("href");
                        grcount[i] = memcount.Replace(".", string.Empty);
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

        public async void AutoTag(String tag_url)
        {
            setReady(false, "Đang tự động tag");

            await Task.Factory.StartNew(() => Navigate(tag_url));
            int paged = 0;

            while (true)
            {
                var iTags = driver.FindElementsByXPath("//form[@method='post']//span//a");

                if (iTags.Count != 2)
                {
                    if (paged == 0)
                        MessageBox.Show("Xem lại URL bài viết/ảnh!");
                    else
                        MessageBox.Show("Facebook yêu cầu xác nhận hình ảnh!");
                    Program.mainForm.btnTag.Enabled = true;
                    Program.mainForm.txtTagUrl.Enabled = true;
                    setReady(true, "Tag thất bại | Ready");
                    return;
                }

                ClickElement(iTags[1]);

                for (int i = 0; i < paged; i++)
                {
                    if (driver.FindElementsByName("show_more").Count != 0)
                    {
                        await Task.Factory.StartNew(() => ClickElement(driver.FindElementByName("show_more")));
                    }
                    else
                    {
                        return;
                    }
                }

                var checkboxes = driver.FindElementsByXPath("//fieldset//table//tr");

                if (checkboxes.Count == 0)
                    break;

                foreach (IWebElement checkbox in checkboxes)
                {
                    await Task.Factory.StartNew(() => ClickElement(checkbox));
                    Program.mainForm.dgTag.Rows.Insert(0, checkbox.Text);
                }

                if (driver.FindElementsByName("done").Count == 0)
                {
                    MessageBox.Show("Xem lại URL bài viết/ảnh, không tìm thấy nút done!");
                    Program.mainForm.btnTag.Enabled = true;
                    Program.mainForm.txtTagUrl.Enabled = true;
                    setReady(true, "Tag thất bại | Ready");
                    return;
                }

                await Task.Factory.StartNew(() => Click("done"));

                if (driver.FindElementsByName("post").Count == 0)
                {
                    MessageBox.Show("Không tìm thấy nút Post!");
                    Program.mainForm.btnTag.Enabled = true;
                    Program.mainForm.txtTagUrl.Enabled = true;
                    setReady(true, "Tag thất bại | Ready");
                    return;
                }

                await Task.Factory.StartNew(() => Click("post"));

                paged++;
            }

            MessageBox.Show("Tag hoàn thành!");
            Program.mainForm.btnTag.Enabled = true;
            Program.mainForm.txtTagUrl.Enabled = true;
            setReady(true, "Tag thành công | Ready");
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

        public async void ImportGroupMembers(String group_url)
        {
            setReady(false, "Đang Import từ Group");

            await Task.Factory.StartNew(() => Navigate(group_url));

            int progress = 0;

            while (true)
            {
                var members = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//div[contains(@id, 'member_')]"));
                if (members.Count == 0)
                {
                    break;
                }

                foreach (IWebElement member in members)
                {
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

            await Task.Factory.StartNew(() => Navigate(profile_url));

            int progress = 0;

            while (true)
            {
                var members = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, 'fref=fr_tab')]"));
                if (members.Count == 0)
                {
                    break;
                }

                foreach (IWebElement member in members)
                {
                    Program.mainForm.dgUID.Rows.Insert(0, member.Text, member.GetAttribute("href"));
                    await TaskEx.Delay(10);
                    progress++;
                }

                var more = await Task.Factory.StartNew(() => driver.FindElementsById("m_more_friends"));
                if (more.Count == 0)
                    break;
                await Task.Factory.StartNew(() => ClickElement(more[0].FindElement(By.TagName("a"))));
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

                await Task.Factory.StartNew(() => InputValueAdd("body", Program.mainForm.txtPM.Text + random_tag));

                await Task.Factory.StartNew(() => Click("Send"));

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

        public async void AutoReplyComment()
        {
            setReady(false, "Đang Reply Comment");
            int delay;
            if (!int.TryParse(Program.mainForm.txtReplyDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }
            int max;
            if (!int.TryParse(Program.mainForm.txtReplyMAX.Text, out max) || max < 0)
            {
                MessageBox.Show("Max Reply/1 URL: số nguyên không nhỏ hơn 0");
                return;
            }

            Program.mainForm.lblReplyTick.Text = "Đang lấy danh sách URL bình luận";

            if (Program.mainForm.dgReplyURLs.Rows.Count == 0)
            {
                while (Program.mainForm.dgReplyBrowse.Rows.Count > 0)
                {
                    String _txtReplyURL = Program.mainForm.dgReplyBrowse.Rows[0].Cells[0].Value.ToString();
                    Program.mainForm.dgReplyBrowse.Rows.RemoveAt(0);

                    if (!_txtReplyURL.Contains("facebook.com"))
                        continue;
                    await Task.Factory.StartNew(() => Navigate(_txtReplyURL));

                    var comments = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, '/comment/replies/') and contains(@href, 'comment_form')]"));
                    if (comments.Count > 0)
                    {
                        int comment_count = 0;
                        foreach (IWebElement comment in comments)
                        {
                            if (comment_count >= max)
                                break;
                            Program.mainForm.dgReplyURLs.Rows.Add(comment.GetAttribute("href"));
                            comment_count++;
                        }
                    }
                }
            }

            Program.mainForm.lblReplyTick.Text = "Đang Reply";

            while (Program.mainForm.dgReplyURLs.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                await Task.Factory.StartNew(() => Navigate(Program.mainForm.dgReplyURLs.Rows[0].Cells[0].Value.ToString()));
                await TaskEx.Delay(2000);

                while (true && !pause)
                {
                    int headers = await Task.Factory.StartNew(() => FindHeader());
                    if (headers > 0)
                    {
                        break;
                    }
                    Program.mainForm.lblReplyTick.Text = "(!) Mạng";
                    await Task.Factory.StartNew(() => Navigate(Program.mainForm.txtReplyURL.Text));
                    await TaskEx.Delay(1000);
                }

                if (await Task.Factory.StartNew(() => driver.FindElementsByName("comment_text").Count) == 0)
                {
                    Program.mainForm.dgReplyURLs.Rows.RemoveAt(0);
                    Program.mainForm.lblReplyTick.Text = "Skip bài Reply";
                    Program.mainForm.dgReplyResult.Rows.Insert(0, "Skip - Không cho Reply");
                    continue;
                }

                string random_tag = "";

                if (Program.mainForm.cbReplyRandomTag.Checked)
                {
                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var rnd = new Random();
                    random_tag = " #" + new string(
                        Enumerable.Repeat(chars, rnd.Next(8) + 1)
                                  .Select(s => s[rnd.Next(s.Length)])
                                  .ToArray());
                }

                // await Task.Factory.StartNew(() => InputValueAdd("comment_text", Program.mainForm.txtReplyContent.Text + random_tag));
                await Task.Factory.StartNew(() => driver.ExecuteScript(@"document.getElementsByName('comment_text')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtReplyContent.Text + random_tag) + "';"));

                var btnSubmit = driver.FindElementByXPath("//form[@method='post']//input[@type='submit']");
                await Task.Factory.StartNew(() => ClickElement(btnSubmit));

                IWebElement comment_name = driver.FindElementByTagName("h3");
                Program.mainForm.dgReplyURLs.Rows.RemoveAt(0);
                Program.mainForm.dgReplyResult.Rows.Insert(0, comment_name.Text);

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;

                    Program.mainForm.lblReplyTick.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblReplyTick.Text = "Đang Reply";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.lblReplyTick.Text = "Ready";

            Program.mainForm.txtReplyURL.Enabled = true;
            Program.mainForm.txtReplyContent.Enabled = true;
            Program.mainForm.txtReplyDelay.Enabled = true;
            Program.mainForm.btnReply.Enabled = true;
            Program.mainForm.btnReplyPause.Enabled = false;

            MessageBox.Show("Đã hoàn thành Reply");
            setReady(true);
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

        #region OTHER HELPERS
        public void setReady(bool status, String message = "Ready")
        {
            this.ready = status;
            Program.mainForm.lblStatus.Text = message;
            if (status)
            {
                Program.mainForm.imgStatus.Image = System.Drawing.Bitmap.FromFile("green.png");
            }
            else
            {
                Program.mainForm.imgStatus.Image = System.Drawing.Bitmap.FromFile("red.gif");
            }
        }
        #endregion
    }
}
