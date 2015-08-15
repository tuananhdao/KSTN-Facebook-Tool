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
        public OpenQA.Selenium.Chrome.ChromeDriver driver;
        public OpenQA.Selenium.Chrome.ChromeDriver driver2;

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;

        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);
        [DllImport("User32")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        IEnumerable<int> newFirefoxPids;

        Dictionary<String, String> links = new Dictionary<string, string>();
        public SortedDictionary<string, string> pages = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> events = new SortedDictionary<string, string>();
        //Thread t;
        public bool ready = true;
        public bool ready2 = true;
        public bool pause = false;

        // User info
        public String user_id = "";
        public String user_id_img = "";
        public String access_token = "";


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
            if (driver2 != null)
            {
                driver2.Quit();
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

            //Program.loadingForm = new LoadingForm();
            //Program.loadingForm.setText("KHỞI TẠO HỆ THỐNG...");
            //Program.loadingForm.Show();
            //Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(() => Program.loadingForm.ShowDialog()));

            //t = new System.Threading.Thread(() => Program.loadingForm.ShowDialog());
            //t.Start(); // LoadingForm.Show()
            // Bật trình duyệt khi Login
            if (driver == null)
            {
                setReady(false, "Đang khởi tạo hệ thống");
                //var profile = new OpenQA.Selenium.Firefox.FirefoxProfile();
                //profile.SetPreference("general.useragent.override", "NokiaC5-00/061.005 (SymbianOS/9.3; U; Series60/3.2 Mozilla/5.0; Profile/MIDP-2.1 Configuration/CLDC-1.1) AppleWebKit/525 (KHTML, like Gecko) Version/3.0 Safari/525 3gpp-gba");
                //profile.SetPreference("webdriver.load.strategy", "unstable");
                //profile.SetPreference("permissions.default.stylesheet", 2);
                //profile.SetPreference("permissions.default.image", 2);
                //profile.AddExtension("App/Firefox/firebug.xpi");
                //profile.SetPreference("dom.ipc.plugins.enabled.libflashplayer.so", "false");
                IEnumerable<int> pidsBefore = Process.GetProcessesByName("chrome").Select(p => p.Id);
                try
                {
                    //this.driver = await Task.Factory.StartNew(() => new OpenQA.Selenium.Firefox.FirefoxDriver(profile));
                    /*
                    OpenQA.Selenium.Firefox.FirefoxBinary firefox = new OpenQA.Selenium.Firefox.FirefoxBinary("App/Firefox/firefox.exe");
                    this.driver = new OpenQA.Selenium.Firefox.FirefoxDriver(firefox, profile);*/
                    var chromeDriverService = OpenQA.Selenium.Chrome.ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Application.ExecutablePath) + @"\App");
                    chromeDriverService.HideCommandPromptWindow = true;
                    OpenQA.Selenium.Chrome.ChromeOptions chromeDriverOptions = new OpenQA.Selenium.Chrome.ChromeOptions();
                    chromeDriverOptions.AddArgument("--user-agent=NokiaC5-00/061.005 (SymbianOS/9.3; U; Series60/3.2 Mozilla/5.0; Profile/MIDP-2.1 Configuration/CLDC-1.1) AppleWebKit/525 (KHTML, like Gecko) Version/3.0 Safari/525 3gpp-gba"); //Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0
                    chromeDriverOptions.AddArgument("ignore-certificate-errors");
                    chromeDriverOptions.AddArgument("no-sandbox");
                    chromeDriverOptions.AddExtension(Path.GetDirectoryName(Application.ExecutablePath) + @"\App\block.crx");
                    driver = await Task.Factory.StartNew(() => new OpenQA.Selenium.Chrome.ChromeDriver(chromeDriverService, chromeDriverOptions));
                    //Thread.Sleep(1000);
                }
                catch
                {
                    //Program.loadingForm.RequestStop();
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
                IEnumerable<int> pidsAfter = Process.GetProcessesByName("chrome").Select(p => p.Id);

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
                    //Program.loadingForm.RequestStop();
                    MessageBox.Show("Không tìm thấy cửa sổ Trình duyệt!");
                    Exceptions_Handler();
                }

                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));
                driver.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
                driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(30));
                setReady(true);
            }

            //Program.loadingForm.setText("ĐĂNG NHẬP TÀI KHOẢN FACEBOOK...");
            setReady(false, "Đang đăng nhập");
            await Task.Factory.StartNew(() => Navigate(links["fb_url"]));

            if (await Task.Factory.StartNew(() => driver.FindElementsByName("email").Count) == 0)
            {
                //Program.loadingForm.RequestStop();
                //t.Abort();
                //t.Join();
                MessageBox.Show("Có lỗi với đường truyền mạng hoặc tài khoản facebook của bạn!\nHãy kiểm tra lại");
                Program.mainForm.btnLogin.Enabled = true;
                Program.mainForm.txtUser.Enabled = true;
                Program.mainForm.txtPass.Enabled = true;
                foreach (Control item in Program.mainForm.dgGroups.Controls.OfType<Control>())
                {
                    if (item.Name == "group_loading_gif")
                        Program.mainForm.dgGroups.Controls.Remove(item);
                }
                setReady(true);
                return;
            }
            
            await Task.Factory.StartNew(() => InputValueAdd("email", user));
            await Task.Factory.StartNew(() => InputValueAdd("pass", pass));
            await Task.Factory.StartNew(() => Click("login"));
            /*
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            await Task.Factory.StartNew(() => wait.Until<IWebElement>((d) =>
            {
                return d.FindElement(By.Name("xc_message"));
            }));*/
            //await Task.Factory.StartNew(() => ((IJavaScriptExecutor)driver).ExecuteScript("alert(123)"));
            driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(300));
            string after_login_url = await Task.Factory.StartNew(() => driver.Url);
            driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(30));

            check_after_login:
            if (after_login_url.Contains("home.php") || after_login_url.Contains("phoneacquire"))
            {
                Program.mainForm.btnLogin.Text = "Đăng nhập thành công!";
                Program.mainForm.AcceptButton = null;
                Program.mainForm.btnPost.Enabled = true;
                Program.mainForm.btnInvite.Enabled = true;
                Program.mainForm.btnGroupSearch.Enabled = true;
                Program.mainForm.btnGroupJoin.Enabled = true;
                Program.mainForm.btnPMImportFriends.Enabled = true;
                Program.mainForm.btnPM.Enabled = true;
                Program.mainForm.btnPMSendFrRequests.Enabled = true;
                Program.mainForm.btnInteractionsFollow.Enabled = true;
                Program.mainForm.btnInteractionsPoke.Enabled = true;
                Program.mainForm.btnInteractionsLike.Enabled = true;
                Program.mainForm.btnPMImportProfile.Enabled = true;
                Program.mainForm.btnPMImportGroup.Enabled = true;
                Program.mainForm.btnCommentImportComment.Enabled = true;
                Program.mainForm.btnPostImportGroups.Enabled = true;
                Program.mainForm.btnGroupImportFriends.Enabled = true;
                Program.mainForm.btnCommentScan.Enabled = true;
                Program.mainForm.btnFanpageComment.Enabled = true;
                Program.mainForm.btnFanpageGroupPost.Enabled = true;
                Program.mainForm.btnEventInviteFriends.Enabled = true;
                Program.mainForm.btnFanpageSeeder.Enabled = true;
                Program.mainForm.btnFanpageInviteFriends.Enabled = true;
                Program.mainForm.btnFanpageLike.Enabled = true;
                Program.mainForm.btnGraphSearch.Enabled = true;
                
                var photos = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, '?v=photos')]"));
                if (photos.Count > 0)
                {
                    String href = photos[0].GetAttribute("href");
                    Match match = Regex.Match(href, @".com\/([A-Za-z0-9\-\.]+)\?v\=photos", RegexOptions.None);
                    if (match.Success)
                    {
                        user_id = match.Groups[1].Value;
                    }
                }

                var nodes = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//img[contains(@src, 'fbcdn-profile-a.akamaihd.net')]"));
                if (nodes.Count > 0)
                {
                    Program.mainForm.lblUsername.Text = nodes[0].GetAttribute("alt");
                }

                try
                {
                    user_id_img = user_id;
                    await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            user_id_img = driver.FindElementByName("target").GetAttribute("value");
                        }
                        catch { }
                    });

                    Program.mainForm.pbAvatar.WaitOnLoad = false;
                    Program.mainForm.pbAvatar.LoadAsync(links["facebook_graph"] + "/" + user_id_img + "/picture");
                    Program.mainForm.lblViewProfile.Text = "https://facebook.com/" + user_id_img;
                }
                catch { }
                
                //Program.mainForm.Focus();
                //Program.loadingForm.setText("ĐĂNG NHẬP THÀNH CÔNG! ĐANG TẢI DANH SÁCH NHÓM...");
                Program.mainForm.lblStatus.Text = "Tải danh sách nhóm...";
                Program.mainForm.dgGroups.Rows.Clear();
                if (Program.mainForm.cbGroupReload.Checked)
                {
                    try
                    {
                        DataSet DS = new DataSet();
                        DS.ReadXml(RemoveSpecialCharacters(user_id_img) + "_groups.xml");

                        bool empty = true;

                        foreach (DataRow dr in DS.Tables[0].Rows)
                        {
                            Program.mainForm.dgGroups.Rows.Add(dr[0], dr[1], dr[2], dr[3]);
                            await TaskEx.Delay(1);
                            if (dr[3].ToString() == "1") empty = false;
                        }

                        if (empty)
                        {
                            foreach (DataGridViewRow row in Program.mainForm.dgGroups.Rows)
                            {
                                DataGridViewCheckBoxCell chk = (DataGridViewCheckBoxCell)row.Cells[3];
                                chk.Value = 1;
                            }
                        }
                    }
                    catch { }
                }

                if (Program.mainForm.dgGroups.RowCount == 0) await getGroups();
                else
                {
                    //Program.loadingForm.RequestStop();
                    //t.Abort();
                    //t.Join();
                    Program.mainForm.lblProgress.Text = "0/" + Program.mainForm.dgGroups.Rows.Count;
                    setReady(true, "Số lượng nhóm: " + Program.mainForm.dgGroups.Rows.Count);
                }

                Program.mainForm.btnLogin.Text = "Đăng xuất";
                Program.mainForm.btnLogin.Enabled = true;

                foreach (Control item in Program.mainForm.dgGroups.Controls.OfType<Control>())
                {
                    if (item.Name == "group_loading_gif")
                        Program.mainForm.dgGroups.Controls.Remove(item);
                }
            }
            else
            {
                if (getUrl().Contains("home.php"))
                {
                    goto check_after_login; // check again, just in case 100ms is not enough
                }

                Program.mainForm.txtUser.Enabled = true;
                Program.mainForm.txtPass.Enabled = true;
                foreach (Control item in Program.mainForm.dgGroups.Controls.OfType<Control>())
                {
                    if (item.Name == "group_loading_gif")
                        Program.mainForm.dgGroups.Controls.Remove(item);
                }
                //Program.loadingForm.RequestStop();
                //t.Abort();
                //t.Join();
                if (getUrl().Contains("checkpoint"))
                    MessageBox.Show("Hãy vô hiệu hóa bảo mật tài khoản trước khi sử dụng AUTO!");
                
                MessageBox.Show("Kiểm tra lại thông tin đăng nhập!\nNếu bạn chắc chắn thông tin đăng nhập là đúng,\nhãy đăng nhập lại tài khoản trên trình duyệt trước khi tiếp tục!");
                Program.mainForm.btnLogin.Enabled = true;
                setReady(true);
                return;
            }
            // await Task.Factory.StartNew(() => new WebDriverWait(driver, TimeSpan.FromSeconds(10))); // Chờ tải xong trang
        }

        public async Task getGroups()
        {
            setReady(false, "Đang lấy danh sách nhóm");
            Program.mainForm.dgGroups.Rows.Clear();
            await Task.Factory.StartNew(() => Navigate(links["fb_groups_2"]));

            var td = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//div[@id='root']//table//tbody//tr//td"));
            var e2 = td[0].FindElements(By.XPath("./div"));
            if (e2.Count == 3)
            {
                var e = e2[1].FindElements(By.XPath(".//li//table//tbody//tr//td//a"));

                foreach (IWebElement k in e)
                {
                    addGroup2Grid(k);
                    await TaskEx.Delay(10);
                }
            }

            var see_more_btns = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, '?seemore')]"));

            if (see_more_btns.Count > 0)
            {
                await Task.Factory.StartNew(() => ClickElement(see_more_btns[0]));

                td = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//div[@id='root']//table//tbody//tr//td"));
                e2 = td[0].FindElements(By.XPath("./div"));
                if (e2.Count == 3)
                {
                    var e = e2[1].FindElements(By.XPath(".//li//table//tbody//tr//td//a"));

                    foreach (IWebElement k in e)
                    {
                        addGroup2Grid(k);
                        await TaskEx.Delay(10);
                    }
                }
            }
            
            /*
            if (see_more_btns.Count == 0)
            {
                var divs = driver.FindElementsByXPath("//div[@id='root']//div");
                var h3 = divs[1].FindElements(By.TagName("h3"));
                foreach (IWebElement _h3 in h3)
                {
                    var k = _h3.FindElement(By.TagName("a"));
                    addGroup2Grid(k);
                    await TaskEx.Delay(10);
                }
                if (Program.mainForm.dgGroups.Rows.Count == 0)
                {
                    var e2 = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//table//tbody//tr//td//div"));
                    var e = e2[3].FindElements(By.XPath(".//li//table//tbody//tr//td//a"));

                    foreach (IWebElement k in e)
                    {
                        addGroup2Grid(k);
                        await TaskEx.Delay(10);
                    }
                }
            }
            else
            {
                await Task.Factory.StartNew(() => ClickElement(see_more_btns[0]));
                var e2 = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//table//tbody//tr//td//div"));
                var e = e2[3].FindElements(By.XPath(".//li//table//tbody//tr//td//a"));

                foreach (IWebElement k in e)
                {
                    addGroup2Grid(k);
                    await TaskEx.Delay(10);
                }
            }*/

            //Program.loadingForm.RequestStop();
            //t.Abort();
            //t.Join();

            /*
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
            }*/

            //Program.mainForm.dgGroups.DataSource = Program.mainForm.dt;
            //Program.mainForm.dgGroupInvites.DataSource = Program.mainForm.dt;

            Program.mainForm.lblProgress.Text = "0/" + Program.mainForm.dgGroups.Rows.Count;
            setReady(true, "Số lượng nhóm: " + Program.mainForm.dgGroups.Rows.Count);
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
                Program.mainForm.btnInteractionsFollow.Enabled = false;
                Program.mainForm.btnInteractionsPoke.Enabled = false;
                Program.mainForm.btnInteractionsLike.Enabled = false;
                Program.mainForm.btnCommentImportComment.Enabled = false;
                Program.mainForm.btnPostImportGroups.Enabled = false;
                Program.mainForm.btnGroupImportFriends.Enabled = false;
                Program.mainForm.btnLogin.Enabled = true;
                Program.mainForm.btnCommentScan.Enabled = false;
                Program.mainForm.btnFanpageComment.Enabled = false;
                Program.mainForm.btnFanpageGroupPost.Enabled = false;
                Program.mainForm.btnEventInviteFriends.Enabled = false;
                Program.mainForm.btnFanpageSeeder.Enabled = false;
                Program.mainForm.btnFanpageInviteFriends.Enabled = false;
                Program.mainForm.btnFanpageLike.Enabled = false;
                Program.mainForm.btnGraphSearch.Enabled = false;
                Program.mainForm.dgGroups.Rows.Clear();
                pages.Clear();
                Program.mainForm.cbFanpage.Items.Clear();
                Program.mainForm.panelFanpageComment.Controls.Clear();
            }
            setReady(true, "Đăng xuất thành công");
        }
        #endregion

        public async void AutoPost()
        {
            if (!pause)
                Program.mainForm.lblCountDown.Text = "POSTING";
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
                row.Cells[3].Value = "0";
                Program.mainForm.groups_to_xml();

                await Task.Factory.StartNew(() => Navigate(row.Cells[1].Value.ToString()));
                Program.mainForm.lblStatus.Text = driver.Title;

                int tries = 0;
                while (true && !pause)
                {
                    int headers = await Task.Factory.StartNew(() => FindHeader());
                    if (headers > 0 || tries > 10)
                    {
                        break;
                    }
                    tries++;
                    Program.mainForm.lblStatus.Text = "(!) Lỗi đường truyền. Nhấn Dừng để tránh sót nhóm. (" + tries + ")";
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
                            Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblStatus.Text, "Group không cho đăng bài");
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
                            Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblStatus.Text, "Skip - Không tìm thấy nút đăng bài!");
                            continue;
                        }
                        await Task.Factory.StartNew(() => Click("view_post"));

                        var url_to_comment_a = await Task.Factory.StartNew(() => driver.FindElements(By.XPath("//a[contains(@href, 'view=permalink')]")));
                        string post_url = Uri.UnescapeDataString(driver.Url);
                        if (url_to_comment_a.Count > 0)
                            post_url = url_to_comment_a[0].GetAttribute("href");

                        Program.mainForm.dgPostResult.Rows.Insert(0, driver.Title, post_url);
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
                        Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblStatus.Text, "Group không cho đăng bài");
                        continue;
                    }
                    await Task.Factory.StartNew(() => Click("lgc_view_photo"));

                    if (await Task.Factory.StartNew(() => driver.FindElementsByName("xc_message").Count) == 0 || await Task.Factory.StartNew(() => driver.FindElementsByName("file1").Count) == 0 || await Task.Factory.StartNew(() => driver.FindElementsByName("photo_upload").Count) == 0)
                    {
                        Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblStatus.Text, "Skip - Không tìm thấy nút đăng bài!");
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
                        Match match = Regex.Match(Uri.UnescapeDataString(await Task.Factory.StartNew(() => driver.Url)) + "", @"\?photo_id\=([A-Za-z0-9\-]+)\&", RegexOptions.None);
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
                        Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblStatus.Text, result_url);
                    }
                    catch { }
                }

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;
                    Program.mainForm.lblCountDown.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblCountDown.Text = "GO";
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
            Program.mainForm.lblCountDown.Text = "Ready";

            MessageBox.Show("Đã hoàn thành đăng bài trong " + Program.mainForm.dgPostResult.Rows.Count + " nhóm!");

            setReady(true);
        }

        public async void AutoInvite()
        {
            setReady(false, "Đang tự động mời nhóm");
            Program.mainForm.lblCountDown.Text = "Đang mời";
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
                Program.mainForm.lblStatus.Text = row.Cells[0].Value.ToString();

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
                    Program.mainForm.dgInvitedGroups.Rows.Insert(0, "Đã gia nhập/Tên không đúng/Nhóm không cho phép");
                    continue;
                }

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;
                    Program.mainForm.lblCountDown.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblCountDown.Text = "Đang mời";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.txtInviteDelay.Enabled = true;
            Program.mainForm.txtInviteName.Enabled = true;
            Program.mainForm.btnInvite.Enabled = true;
            Program.mainForm.lblCountDown.Text = "Ready";

            MessageBox.Show("Đã hoàn thành mời " + progress + " nhóm!");

            setReady(true);
        }

        public async void GroupSearch()
        {
            setReady(false, "Đang tự động tìm nhóm");
            int success = 0;
            int skip = 0;
            Program.mainForm.lblStatus.Text = "Đang quét...";

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

                    Program.mainForm.lblStatus.Text = "Đang quét: " + a.GetAttribute("innerHTML");

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
            Program.mainForm.lblStatus.Text = "Sẵn sàng";

            setReady(true, "Tự động tìm nhóm: " + success);
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

            Program.mainForm.lblCountDown.Text = "Đang join...";

            while (Program.mainForm.dgGroupSearch.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

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
                    if (pause)
                        break;

                    Program.mainForm.lblCountDown.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblCountDown.Text = "GO";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.lblCountDown.Text = "Ready";

            Program.mainForm.txtJoinDelay.Enabled = true;
            Program.mainForm.btnGroupJoin.Enabled = true;

            MessageBox.Show("Hoàn thành gia nhập nhóm!");

            setReady(true, "Hoàn thành gia nhập nhóm");
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

            Program.mainForm.lblCountDown.Text = "GO";

            while (Program.mainForm.dgCommentBrowse.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                string post_url = Program.mainForm.dgCommentBrowse.Rows[0].Cells[0].Value.ToString();
                if (!post_url.Contains("facebook.com/"))
                {
                    Program.mainForm.dgCommentBrowse.Rows.RemoveAt(0);
                    continue;
                }
                if (post_url.Contains("www.facebook"))
                {
                    post_url = post_url.Replace("www.facebook", "m.facebook").Replace("/permalink/", "?view=permalink&id=");
                }
                await Task.Factory.StartNew(() => Navigate(post_url));

                int tries = 0;
                while (true && !pause)
                {
                    int headers = await Task.Factory.StartNew(() => FindHeader());
                    if (headers > 0 || tries > 10)
                    {
                        break;
                    }
                    tries++;
                    Program.mainForm.lblStatus.Text = "(!) Lỗi đường truyền. Nhấn Dừng để tránh sót nhóm. (" + tries + ")";
                    await Task.Factory.StartNew(() => Navigate(post_url));
                    await TaskEx.Delay(1000);
                }

                Program.mainForm.lblStatus.Text = driver.Title;

                if (await Task.Factory.StartNew(() => driver.FindElementsByName("comment_text").Count) == 0)
                {
                    Program.mainForm.dgCommentBrowse.Rows.RemoveAt(0);
                    Program.mainForm.lblStatus.Text = "Skip bài đăng";
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

                    Program.mainForm.lblCountDown.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblCountDown.Text = "GO";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.lblCountDown.Text = "Ready";
            Program.mainForm.txtComment.Enabled = true;
            Program.mainForm.txtCommentDelay.Enabled = true;
            Program.mainForm.btnCommentBrowse.Enabled = true;
            Program.mainForm.btnCommentImportComment.Enabled = true;
            Program.mainForm.dgCommentBrowse.Enabled = true;

            MessageBox.Show("Hoàn thành bình luận nhóm!");
            setReady(true, "Bình luận nhóm hoàn thành!");
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
                Program.mainForm.lblStatus.Text = driver.Title;

                string post_xpath = "";
                if (Program.mainForm.cbCommentOnlyMe.Checked)
                    post_xpath = "//div[@id='m_group_stories_container']//a[contains(@href,'__tn__=C') and contains(@href, '" + user_id + "')]";
                else
                    post_xpath = "//div[@id='m_group_stories_container']//a[contains(@href,'__tn__=C')]";
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
                            Program.mainForm.lblStatus.Text = "Skip bài đăng";
                            Program.mainForm.dgComment.Rows.Insert(0, driver.Title, "Skip - Đang chờ phê duyệt");
                            continue;
                        }

                        await Task.Factory.StartNew(() => driver.ExecuteScript(@"document.getElementsByName('comment_text')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtComment.Text) + "';"));

                        IWebElement btnSubmit = driver.FindElementByXPath("//form[@method='post']//input[@type='submit']");
                        await Task.Factory.StartNew(() => ClickElement(btnSubmit));
                        try
                        {
                            Program.mainForm.dgComment.Rows.Insert(0, driver.Title, driver.Url.Replace("m.facebook", "www.facebook"));
                        }
                        catch { }

                        for (int i = 0; i < delay + 1; i++)
                        {
                            if (pause)
                                break;

                            Program.mainForm.lblCountDown.Text = delay - i + "";
                            if (i == delay)
                            {
                                Program.mainForm.lblCountDown.Text = "GO";
                            }
                            await TaskEx.Delay(1000);
                        }
                    }
                }
            }

            Program.mainForm.lblCountDown.Text = "Ready";
            Program.mainForm.btnCommentScan.Enabled = true;
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
                    string _uid = profile.GetAttribute("href").Replace("https://m.facebook.com/", "");
                    if (_uid.Contains("profile.php"))
                        _uid = _uid.Replace("profile.php?id=", "").Split('&')[0];
                    else
                        _uid = _uid.Split('?')[0];
                    Program.mainForm.dgUID.Rows.Insert(0, profile.Text, _uid);
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
            setReady(true, "Nhập thành công danh sách bạn bè");
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
                    Program.mainForm.dgGroups.Rows.Insert(0, profile.Text, profile.GetAttribute("href"), "", "1");
                    await TaskEx.Delay(10);
                }

                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
                var m_more_friends = await Task.Factory.StartNew(() => driver.FindElementsById("m_more_friends"));
                if (m_more_friends.Count == 0)
                    break;
                else
                    await Task.Factory.StartNew(() => ClickElement(m_more_friends[0].FindElement(By.TagName("a"))));
            }

            Program.mainForm.groups_to_xml();

            MessageBox.Show("Nhập danh sách bạn bè thành công!");
            Program.mainForm.btnGroupImportFriends.Enabled = true;
            setReady(true, "Nhập thành công danh sách bạn bè");
        }

        public async void ImportGroupMembers(String group_url)
        {
            setReady(false, "Đang Import từ Group");
            if (group_url.Contains("www.facebook.com/")) group_url = group_url.Replace("www.facebook", "m.facebook");
            if (!group_url.Contains("m.facebook.com/")) group_url = "https://m.facebook.com/groups/" + group_url;
            group_url += "?view=members&refid=18";
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
                    string _uid = member.FindElement(By.TagName("a")).GetAttribute("href").Replace("https://m.facebook.com/", "");
                    if (_uid.Contains("profile.php"))
                        _uid = _uid.Replace("profile.php?id=", "").Split('&')[0];
                    else
                        _uid = _uid.Split('?')[0];
                    Program.mainForm.dgUID.Rows.Insert(0, member.FindElement(By.TagName("a")).Text, _uid);
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

            setReady(true, "Số thành viên: " + progress);
        }

        public async void ImportProfileFriends(String profile_url)
        {
            setReady(false, "Đang Import từ Profile");
            if (profile_url.Contains("www.facebook.com/")) profile_url = profile_url.Replace("www.facebook", "m.facebook");
            if (!profile_url.Contains("m.facebook.com/")) profile_url = "https://m.facebook.com/" + profile_url;
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
                        string _uid = member.GetAttribute("href").Replace("https://m.facebook.com/", "");
                        if (_uid.Contains("profile.php"))
                            _uid = _uid.Replace("profile.php?id=", "").Split('&')[0];
                        else
                            _uid = _uid.Split('?')[0];
                        Program.mainForm.dgUID.Rows.Insert(0, member.Text, _uid);
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

            setReady(true, "Số bạn bè: " + progress);
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

            while (Program.mainForm.dgInteractions.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                String user_url = "https://m.facebook.com/" + Program.mainForm.dgInteractions.Rows[0].Cells[0].Value.ToString();

                await Task.Factory.StartNew(() => Navigate(user_url));

                var user_info = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//div[@id='m-timeline-cover-section']//a[contains(@href,'?v=info')]"));
                if (user_info.Count > 0)
                    await Task.Factory.StartNew(() => ClickElement(user_info[0]));

                string message_to_send = Program.mainForm.txtPM.Text.Replace("{username}", driver.Title);

                string hometown = "HN";
                string current_city = "HN";

                if (message_to_send.Contains("{hometown}") || message_to_send.Contains("{current_city}"))
                {
                    var livings = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//div[@id='living']//div[@title]//td[@valign='top']"));
                    if (livings.Count == 4)
                    {
                        hometown = livings[3].Text;
                        current_city = livings[1].Text;
                    }
                    message_to_send = message_to_send.Replace("{hometown}", hometown);
                    message_to_send = message_to_send.Replace("{current_city}", current_city);
                }

                var messages = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, '/messages/thread/')]"));
                if (messages.Count == 0)
                {
                    try
                    {
                        Program.mainForm.dgInteractions.Rows.RemoveAt(0);
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

                var bodies = await Task.Factory.StartNew(() => driver.FindElementsByName("body"));

                if (bodies.Count == 0)
                {
                    try
                    {
                        Program.mainForm.dgInteractions.Rows.RemoveAt(0);
                        Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Có lỗi xảy ra khi gửi tin nhắn");
                    }
                    catch { }
                    continue;
                }

                await Task.Factory.StartNew(() => InputValueAdd("body", message_to_send));

                var btnSends = await Task.Factory.StartNew(() => driver.FindElementsByName("send"));

                if (btnSends.Count > 0)
                    await Task.Factory.StartNew(() => ClickElement(btnSends[0]));
                else
                {
                    btnSends = await Task.Factory.StartNew(() => driver.FindElementsByName("Send"));
                    if (btnSends.Count > 0)
                        await Task.Factory.StartNew(() => ClickElement(btnSends[0]));
                }

                Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Đã gửi tin nhắn");

                Program.mainForm.dgInteractions.Rows.RemoveAt(0);

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;
                    Program.mainForm.lblCountDown.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblCountDown.Text = "GO";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.lblCountDown.Text = "Ready";

            Program.mainForm.dgInteractions.Enabled = true;
            Program.mainForm.txtPM.Enabled = true;
            Program.mainForm.txtPMDelay.Enabled = true;
            Program.mainForm.btnPM.Enabled = true;

            MessageBox.Show("Hoàn thành gửi tin nhắn!");
            setReady(true, "Số lượng gửi: " + Program.mainForm.dgPMResult.Rows.Count);
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

            Program.mainForm.lblCountDown.Text = "GO";

            while (Program.mainForm.dgInteractions.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                string friend_url = "https://m.facebook.com/" + Program.mainForm.dgInteractions.Rows[0].Cells[0].Value.ToString();

                await Task.Factory.StartNew(() => Navigate(friend_url));
                int tries = 0;
                while (true && !pause)
                {
                    int headers = await Task.Factory.StartNew(() => FindHeader());
                    if (headers > 0 || tries > 10)
                    {
                        break;
                    }
                    tries++;
                    Program.mainForm.lblStatus.Text = "(!) Lỗi đường truyền. Nhấn Dừng để tránh sót nhóm. (" + tries + ")";
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
                    Program.mainForm.dgInteractions.Rows.RemoveAt(0);
                    Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Đã kết bạn/Không cho phép");
                    continue;
                }

                Program.mainForm.dgInteractions.Rows.RemoveAt(0);

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;

                    Program.mainForm.lblCountDown.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblCountDown.Text = "GO";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.btnPMSendFrRequests.Enabled = true;
            Program.mainForm.lblCountDown.Text = "Ready";

            MessageBox.Show("Hoàn thành kết bạn!");
            setReady(true, "Kết bạn hoàn thành!");
        }

        public async void AutoFollow()
        {
            setReady(false, "Đang tự động theo dõi");
            int delay = 5;
            Program.mainForm.lblCountDown.Text = "GO";

            while (Program.mainForm.dgInteractions.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                string friend_url = "https://m.facebook.com/" + Program.mainForm.dgInteractions.Rows[0].Cells[0].Value.ToString();

                await Task.Factory.StartNew(() => Navigate(friend_url));
                int tries = 0;
                while (true && !pause)
                {
                    int headers = await Task.Factory.StartNew(() => FindHeader());
                    if (headers > 0 || tries > 10)
                    {
                        break;
                    }
                    tries++;
                    Program.mainForm.lblStatus.Text = "(!) Lỗi đường truyền. Nhấn Dừng để tránh sót. (" + tries + ")";
                    await Task.Factory.StartNew(() => Navigate(friend_url));
                    await TaskEx.Delay(1000);
                }

                var btnAddFriend = driver.FindElementsByXPath("//a[contains(@href, 'subscribe.php')]");
                if (btnAddFriend.Count == 1)
                {
                    await Task.Factory.StartNew(() => ClickElement(btnAddFriend[0]));
                    Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Theo dõi thành công");
                }
                else
                {
                    Program.mainForm.dgInteractions.Rows.RemoveAt(0);
                    Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Đã theo dõi/Không cho phép");
                    continue;
                }

                Program.mainForm.dgInteractions.Rows.RemoveAt(0);

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;

                    Program.mainForm.lblCountDown.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblCountDown.Text = "GO";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.btnInteractionsFollow.Enabled = true;
            Program.mainForm.lblCountDown.Text = "Ready";

            MessageBox.Show("Hoàn thành theo dõi!");
            setReady(true, "Theo dõi hoàn thành!");
        }

        public async void AutoPoke()
        {
            setReady(false, "Đang tự động Poke");
            int delay = 5;
            Program.mainForm.lblCountDown.Text = "GO";

            while (Program.mainForm.dgInteractions.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                string friend_url = "https://m.facebook.com/" + Program.mainForm.dgInteractions.Rows[0].Cells[0].Value.ToString();

                await Task.Factory.StartNew(() => Navigate(friend_url));
                int tries = 0;
                while (true && !pause)
                {
                    int headers = await Task.Factory.StartNew(() => FindHeader());
                    if (headers > 0 || tries > 10)
                    {
                        break;
                    }
                    tries++;
                    Program.mainForm.lblStatus.Text = "(!) Lỗi đường truyền. Nhấn Dừng để tránh sót. (" + tries + ")";
                    await Task.Factory.StartNew(() => Navigate(friend_url));
                    await TaskEx.Delay(1000);
                }

                var btnAddFriend = driver.FindElementsByXPath("//a[contains(@href, '/pokes/inline/?poke_target=')]");
                if (btnAddFriend.Count == 1)
                {
                    await Task.Factory.StartNew(() => ClickElement(btnAddFriend[0]));
                    Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Poke thành công");
                }
                else
                {
                    Program.mainForm.dgInteractions.Rows.RemoveAt(0);
                    Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Không poke được");
                    continue;
                }

                Program.mainForm.dgInteractions.Rows.RemoveAt(0);

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;

                    Program.mainForm.lblCountDown.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblCountDown.Text = "GO";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.btnInteractionsPoke.Enabled = true;
            Program.mainForm.lblCountDown.Text = "Ready";

            MessageBox.Show("Hoàn thành Poke!");
            setReady(true, "Poke hoàn thành!");
        }

        public async void InteractionsAutoLike()
        {
            setReady(false, "Đang tự động Like");
            int delay = 5;
            Program.mainForm.lblCountDown.Text = "GO";

            while (Program.mainForm.dgInteractions.Rows.Count > 0)
            {
                if (pause)
                {
                    pause = false;
                    break;
                }

                string friend_url = "https://m.facebook.com/" + Program.mainForm.dgInteractions.Rows[0].Cells[0].Value.ToString();

                await Task.Factory.StartNew(() => Navigate(friend_url));
                int tries = 0;
                while (true && !pause)
                {
                    int headers = await Task.Factory.StartNew(() => FindHeader());
                    if (headers > 0 || tries > 10)
                    {
                        break;
                    }
                    tries++;
                    Program.mainForm.lblStatus.Text = "(!) Lỗi đường truyền. Nhấn Dừng để tránh sót. (" + tries + ")";
                    await Task.Factory.StartNew(() => Navigate(friend_url));
                    await TaskEx.Delay(1000);
                }
                /*
                var btnAddFriend = driver.FindElementsByXPath("//a[contains(@href, '/pokes/inline/?poke_target=')]");
                if (btnAddFriend.Count == 1)
                {
                    await Task.Factory.StartNew(() => ClickElement(btnAddFriend[0]));
                    Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Poke thành công");
                }
                else
                {
                    Program.mainForm.dgInteractions.Rows.RemoveAt(0);
                    Program.mainForm.dgPMResult.Rows.Insert(0, driver.Title, "Không poke được");
                    continue;
                }*/
                var timeline = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href,'v=timeline')]"));
                if (timeline.Count > 0)
                {
                    await Task.Factory.StartNew(() => ClickElement(timeline[0]));
                }
                int j = 0;
                while (true)
                {
                    var like_buttons = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, 'like.php')]"));
                    if (like_buttons.Count > j)
                    {
                        await Task.Factory.StartNew(() => ClickElement(like_buttons[j]));

                        j++;
                        for (int i = 0; i < delay + 1; i++)
                        {
                            if (pause)
                                break;

                            Program.mainForm.lblCountDown.Text = delay - i + "";
                            if (i == delay)
                            {
                                Program.mainForm.lblCountDown.Text = "GO";
                            }
                            await TaskEx.Delay(1000);
                        }
                        Program.mainForm.dgPMResult.Rows.Insert(0, "Like " + driver.Title, driver.Url.Replace("m.facebook", "www.facebook"));
                        await Task.Factory.StartNew(() => driver.Navigate().Back());
                    }
                    else
                        break;

                }


                Program.mainForm.dgInteractions.Rows.RemoveAt(0);

                for (int i = 0; i < delay + 1; i++)
                {
                    if (pause)
                        break;

                    Program.mainForm.lblCountDown.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblCountDown.Text = "GO";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.btnInteractionsLike.Enabled = true;
            Program.mainForm.lblCountDown.Text = "Ready";

            MessageBox.Show("Hoàn thành Like!");
            setReady(true, "Like hoàn thành!");
        }

        public async Task getPages()
        {
            if (!ready) return;
            setReady(false, "Đang lấy danh sách Pages");

            if (pages.Count > 0) return;

            if (user_id != "")
            {
                await Task.Factory.StartNew(() => Navigate("https://m.facebook.com/" + user_id + "?v=likes&refid=17"));

                while (true)
                {
                    var afrefs = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, 'fref=none')]"));

                    if (afrefs.Count > 0)
                    {
                        foreach (IWebElement afref in afrefs)
                        {
                            try
                            {
                                pages.Add(afref.Text, afref.GetAttribute("href"));
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        break;
                    }

                    var next = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, 'v=likes&sectionid=9999')]"));
                    if (next.Count > 0)
                    {
                        await Task.Factory.StartNew(() => ClickElement(next[0]));
                    }
                    else
                    {
                        break;
                    }
                }
            }

            setReady(true);
        }

        public async Task getEvents()
        {
            if (user_id == "") return;

            if (!ready) return;
            setReady(false, "Đang lấy danh sách Sự kiện");

            if (events.Count > 0) return;

            await Task.Factory.StartNew(() => Navigate("https://m.facebook.com/events/upcoming"));

            var upcoming_events = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href,'arefdashboardfilter=upcoming')]"));

            if (upcoming_events.Count > 0)
            {
                foreach (IWebElement upcoming_event in upcoming_events)
                {
                    var upcoming_event_name = upcoming_event.FindElements(By.XPath("..//h4"));
                    if (upcoming_event_name.Count == 0)
                        continue;
                    events.Add(upcoming_event_name[0].Text, upcoming_event.GetAttribute("href"));
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
                Program.mainForm.lblCountDown.Text = "GO";

                foreach (CheckBox _page in Program.mainForm.panelFanpageComment.Controls)
                {
                    if (pause)
                    {
                        pause = false;
                        break;
                    }

                    if (_page.Checked == false) continue;

                    Program.mainForm.lblStatus.Text = _page.Text;

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

                            await Task.Factory.StartNew(() => driver.ExecuteScript(@"document.getElementsByName('comment_text')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtFanpageComment.Text) + "';"));

                            IWebElement btnSubmit = driver.FindElementByXPath("//form[@method='post']//input[@type='submit']");

                            Program.mainForm.dgFanpageCommentResults.Rows.Insert(0, _page.Text, driver.Url.Replace("m.facebook", "www.facebook"));
                            await Task.Factory.StartNew(() => ClickElement(btnSubmit));
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

                        Program.mainForm.lblCountDown.Text = delay - i + "";
                        if (i == delay)
                        {
                            Program.mainForm.lblCountDown.Text = "GO";
                        }
                        await TaskEx.Delay(1000);
                    }
                }
            }

            Program.mainForm.lblCountDown.Text = "Ready";
            Program.mainForm.btnFanpageComment.Enabled = true;

            MessageBox.Show("Hoàn thành bình luận Fanpage!");
            setReady(true, "Bình luận xong!");
        }

        private async Task ChromeAgent()
        {
            if (driver2 != null)
            {
                return;
            }
            ready2 = false;
            Program.mainForm.lblStatus.Text = "Khởi tạo kết nối nâng cao";

            IEnumerable<int> pidsBefore = Process.GetProcessesByName("chrome").Select(p => p.Id);

            var chromeDriverService = OpenQA.Selenium.Chrome.ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Application.ExecutablePath) + @"\App");
            chromeDriverService.HideCommandPromptWindow = true;
            OpenQA.Selenium.Chrome.ChromeOptions chromeDriverOptions = new OpenQA.Selenium.Chrome.ChromeOptions();
            driver2 = await Task.Factory.StartNew(() => new OpenQA.Selenium.Chrome.ChromeDriver(chromeDriverService, chromeDriverOptions));


            IEnumerable<int> pidsAfter = Process.GetProcessesByName("chrome").Select(p => p.Id);
            var newChromePids = pidsAfter.Except(pidsBefore);

            try
            {
                foreach (int pid in newChromePids)
                {
                    int hWnd = Process.GetProcessById(pid).MainWindowHandle.ToInt32();
                    ShowWindow(hWnd, SW_HIDE);
                }
            }
            catch
            {
                MessageBox.Show("Có lỗi xảy ra! Hãy khởi động lại chương trình");
                Exceptions_Handler();
            }

            Program.mainForm.lblStatus.Text = "Thiết lập kết nối nâng cao";

            driver2.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));

            await Task.Factory.StartNew(() => driver2.Url = "https://www.facebook.com/");


            WebDriverWait wait = new WebDriverWait(driver2, TimeSpan.FromSeconds(60));

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    driver2.Url = "https://www.facebook.com/";
                }
                catch
                {
                    ((IJavaScriptExecutor)driver2).ExecuteScript("return window.stop()");
                }
            });

            await Task.Factory.StartNew(() =>
            {
                wait.Until<Boolean>((d) =>
                {
                    return ((IJavaScriptExecutor)driver2).ExecuteScript("return document.readyState").Equals("complete");
                });
            });

            try
            {
                driver2.ExecuteScript(@"document.getElementsByName('email')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtUser.Text) + "';");
            }
            catch
            {
                MessageBox.Show("Đường truyền mạng quá chậm, vui lòng thử lại sau!");
                Exceptions_Handler();
            }

            await Task.Factory.StartNew(() =>
            {
                driver2.ExecuteScript(@"document.getElementsByName('pass')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtPass.Text) + "';");
                driver2.ExecuteScript(@"document.getElementById('loginbutton').click();");
            });

            await Task.Factory.StartNew(() =>
            {
                wait.Until<Boolean>((d) =>
                {
                    return ((IJavaScriptExecutor)driver2).ExecuteScript("return document.readyState").Equals("complete");
                });
            });

            await TaskEx.Delay(1000);
            ready2 = true;
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

            Program.mainForm.lblStatus.Text = "Lấy nội dung đăng...";

            if (Program.mainForm.radFanpageImage.Checked)
            {
                string page_name = Program.mainForm.cbFanpage.Text;
                await Task.Factory.StartNew(() => Navigate(pages[page_name]));
                var page_posts = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//div[@id='recent']//div[@id]//a[contains(@href, '/photos/') or contains(@href, 'story.php?story_fbid')]"));

                if (page_posts.Count > 0)
                {
                    text_to_share = page_posts[0].GetAttribute("href");
                    text_to_share = text_to_share.Replace("/m.facebook", "/www.facebook");
                }
            }

            if (Program.mainForm.radFanpageOther.Checked)
            {
                text_to_share = Program.mainForm.txtFanpageOther.Text;
            }

            if (text_to_share == "")
            {
                text_to_share = pages[Program.mainForm.cbFanpage.Text].Replace("/m.facebook", "/www.facebook");
            }

            Program.mainForm.lblStatus.Text = "Quét mục tiêu";
            List<string> post_targets = new List<string>();

            await Task.Factory.StartNew(() => Navigate(links["fb_groups"]));

            var e2 = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//table//tbody//tr//td//div"));
            if (e2.Count < 4)
            {
                await Task.Factory.StartNew(() => Navigate(links["fb_groups_2"]));
                var divs = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//div[@id='root']//div"));
                var h3 = await Task.Factory.StartNew(() => divs[1].FindElements(By.TagName("h3")));
                foreach (IWebElement _h3 in h3)
                {
                    var k = await Task.Factory.StartNew(() => _h3.FindElement(By.TagName("a")));
                    post_targets.Add(k.GetAttribute("href").Replace("/m.facebook", "/www.facebook"));
                }
            }
            else
            {
                var e = await Task.Factory.StartNew(() => e2[3].FindElements(By.XPath(".//li//table//tbody//tr//td//a")));

                foreach (IWebElement k in e)
                {
                    post_targets.Add(k.GetAttribute("href").Replace("/m.facebook", "/www.facebook"));
                }
            }

            await ChromeAgent();
            ready2 = false;
            WebDriverWait wait = new WebDriverWait(driver2, TimeSpan.FromSeconds(60));
            Program.mainForm.lblCountDown.Text = "GO";

            if (post_targets.Count > 0)
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
                        await Task.Factory.StartNew(() => driver2.Url = post_target);
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
                    catch { } //NoAlertPresentException Ex

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
                    Program.mainForm.lblStatus.Text = "[" + (post_targets.IndexOf(post_target) + 1) + "/" + post_targets.Count + "]" + driver2.Title;

                    var xhpc = await Task.Factory.StartNew(() => driver2.FindElementsByXPath("//a[@data-endpoint='/ajax/composerx/attachment/group/post/']"));
                    if (xhpc.Count == 0) continue;
                    await Task.Factory.StartNew(() => ClickElement(xhpc[0]));
                    await TaskEx.Delay(5000);

                    var xhpc_text = await Task.Factory.StartNew(() => wait.Until<IWebElement>((d) =>
                    {
                        return d.FindElement(By.Name("xhpc_message_text"));
                    }));

                    await Task.Factory.StartNew(() => ClickElement(xhpc_text));

                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var rnd = new Random();
                    string random_tag = " #" + new string(
                        Enumerable.Repeat(chars, rnd.Next(8) + 1)
                                  .Select(s => s[rnd.Next(s.Length)])
                                  .ToArray());

                    await Task.Factory.StartNew(() => driver2.ExecuteScript(@"document.getElementsByName('xhpc_message_text')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(text_to_share) + "';"));

                    xhpc_text.SendKeys(OpenQA.Selenium.Keys.Enter);

                    await TaskEx.Delay(5000);
                    await Task.Factory.StartNew(() => driver2.ExecuteScript(@"document.getElementsByName('xhpc_message_text')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtFanpageURLText.Text) + random_tag + "';"));
                    xhpc_text.SendKeys(OpenQA.Selenium.Keys.Enter);
                    await TaskEx.Delay(500);
                    var btn_post = driver2.FindElementsByXPath("//form[contains(@action, 'updatestatus.php') and @class='']//button[@type='submit']");
                    if (btn_post.Count == 0) continue;
                    await Task.Factory.StartNew(() => ClickElement(btn_post[0]));

                    for (int i = 0; i < delay + 1; i++)
                    {
                        if (pause)
                            break;

                        Program.mainForm.lblCountDown.Text = delay - i + "";
                        if (i == delay)
                        {
                            Program.mainForm.lblCountDown.Text = "GO";
                        }
                        await TaskEx.Delay(1000);
                    }

                    string post_url = post_target;
                    var group_wall_posts = await Task.Factory.StartNew(() => driver2.FindElementsByXPath("//a[contains(@href, '/permalink/')]"));
                    if (group_wall_posts.Count > 0)
                    {
                        post_url = group_wall_posts[0].GetAttribute("href");
                    }
                    Program.mainForm.dgFanpageGroupResults.Rows.Insert(0, driver2.Title, post_url);
                }
            }
            ready2 = true;

            Program.mainForm.lblCountDown.Text = "Ready";
            Program.mainForm.btnFanpageGroupPost.Enabled = true;

            MessageBox.Show("Hoàn thành quảng bá Fanpage!");
            setReady(true, "Hoàn thành QC Fanpage!");
        }

        public async void FanpageInviteFriends()
        {
            setReady(false, "Đang mời bạn bè like Page");
            string page_name = Program.mainForm.cbFanpage.Text;
            Program.mainForm.lblCountDown.Text = "GO";
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

                    Program.mainForm.btnFanpageInviteFriends.Text = "Dừng [" + success + "]";

                    Match match = Regex.Match(send_page_invite[0].GetAttribute("href"), @"invitee_id\=([0-9]+)\&page_id", RegexOptions.None);
                    if (match.Success)
                    {
                        Program.mainForm.dgFanpageGroupResults.Rows.Insert(0, "Mời like page", match.Groups[1].Value);
                    }

                    await Task.Factory.StartNew(() => ClickElement(send_page_invite[0]));

                    success++;
                }
            }

            Program.mainForm.btnFanpageInviteFriends.Text = "Mời tất cả bạn bè";
            Program.mainForm.btnFanpageInviteFriends.Enabled = true;

            MessageBox.Show("Hoàn thành mời bạn bè!");
            setReady(true);
        }

        public async void FanpageLike()
        {
            int delay = 5;

            setReady(false, "Like bài trên Page Wall");

            string page_name = Program.mainForm.cbFanpage.Text;
            await Task.Factory.StartNew(() => Navigate(pages[page_name]));

            while (true)
            {
                int j = 0;
                while (true)
                {
                    var like_buttons = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, 'action=like')]"));
                    if (like_buttons.Count > j)
                    {
                        await Task.Factory.StartNew(() => ClickElement(like_buttons[j]));
                        Program.mainForm.dgFanpageGroupResults.Rows.Insert(0, "Like", driver.Url);
                        j++;
                        for (int i = 0; i < delay + 1; i++)
                        {
                            if (pause)
                                break;

                            Program.mainForm.lblCountDown.Text = delay - i + "";
                            if (i == delay)
                            {
                                Program.mainForm.lblCountDown.Text = "GO";
                            }
                            await TaskEx.Delay(1000);
                        }
                        await Task.Factory.StartNew(() => driver.Navigate().Back());
                    }
                    else
                        break;
                }

                var btn_page_more = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, 'sectionLoadingID=m_timeline_loading_div') and not(contains(@href,'timecutoff'))]"));
                if (btn_page_more.Count == 0)
                    break;
                await Task.Factory.StartNew(() => ClickElement(btn_page_more[0]));
            }

            Program.mainForm.btnFanpageLike.Enabled = true;
            MessageBox.Show("Hoàn thành Like bài đăng Page");
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
                            Program.mainForm.lblStatus.Text = friend_name;
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

                            Program.mainForm.lblCountDown.Text = delay - i + "";
                            if (i == delay)
                            {
                                Program.mainForm.lblCountDown.Text = "GO";
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

            Program.mainForm.lblCountDown.Text = "Ready";
            Program.mainForm.btnEventInviteFriends.Enabled = true;

            MessageBox.Show("Hoàn thành mời bạn bè!");
            setReady(true);
        }

        public async void FanpageSeeder()
        {
            int delay;

            if (!int.TryParse(Program.mainForm.txtFanpageSeederDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }

            int seeder_count;

            if (!int.TryParse(Program.mainForm.txtFanpageSeederCount.Text, out seeder_count) || delay < 0)
            {
                MessageBox.Show("Số lượng bài tối thiêu: số nguyên không nhỏ hơn 0");
                return;
            }

            setReady(false);
            Program.mainForm.lblStatus.Text = "Đang quét danh sách bài viết";
            string[] comments = Program.mainForm.txtFanpageSeeder.Lines;

            string page_name = Program.mainForm.cbFanpage.Text;
            await Task.Factory.StartNew(() => Navigate(pages[page_name]));

            HashSet<string> page_post_urls = new HashSet<string>();

            while (true)
            {
                var page_posts = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//div[@id='recent']//div[@id]"));
                if (page_posts.Count > 0)
                {
                    foreach (IWebElement page_post in page_posts)
                    {
                        var page_post_url = await Task.Factory.StartNew(() => page_post.FindElements(By.XPath(".//a[contains(@href, 'story.php') or contains(@href, '/photos/')]")));
                        if (page_post_url.Count == 0)
                            continue;
                        page_post_urls.Add(page_post_url[0].GetAttribute("href"));
                    }
                }

                if (page_post_urls.Count >= seeder_count)
                    break;

                var btn_page_more = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href, 'sectionLoadingID=m_timeline_loading_div') and not(contains(@href,'timecutoff'))]"));
                if (btn_page_more.Count == 0)
                    break;
                await Task.Factory.StartNew(() => ClickElement(btn_page_more[0]));
            }


            if (page_post_urls.Count > 0)
            {
                Program.mainForm.lblStatus.Text = "Quét thành công " + page_post_urls.Count + " bài";

                foreach (string page_post_url in page_post_urls)
                {
                    if (pause)
                    {
                        pause = false;
                        break;
                    }

                    await Task.Factory.StartNew(() => Navigate(page_post_url));

                    string comment_text;

                    Random rnd = new Random();
                    comment_text = comments[rnd.Next(comments.Length)];

                    if (Program.mainForm.cbFanpageSeeder.Checked)
                    {
                        string allPageCode = await Task.Factory.StartNew(() => driver.PageSource);
                        int tries = 0;
                        while (allPageCode.Contains(comment_text))
                        {
                            comment_text = comments[rnd.Next(comments.Length)];
                            if (tries > 5) break;
                            tries++;
                        }
                    }


                    if (await Task.Factory.StartNew(() => driver.FindElementsByName("comment_text").Count) == 0) continue;

                    await Task.Factory.StartNew(() => driver.ExecuteScript(@"document.getElementsByName('comment_text')[0].value = '" + System.Web.HttpUtility.JavaScriptStringEncode(comment_text) + "';"));

                    IWebElement btnSubmit = driver.FindElementByXPath("//form[@method='post']//input[@type='submit']");
                    await Task.Factory.StartNew(() => ClickElement(btnSubmit));

                    Program.mainForm.dgFanpageGroupResults.Rows.Insert(0, comment_text, driver.Url);

                    for (int i = 0; i < delay + 1; i++)
                    {
                        if (pause)
                            break;

                        Program.mainForm.lblCountDown.Text = delay - i + "";
                        if (i == delay)
                        {
                            Program.mainForm.lblCountDown.Text = "GO";
                        }
                        await TaskEx.Delay(1000);
                    }
                }
            }

            Program.mainForm.lblCountDown.Text = "Ready";
            Program.mainForm.btnFanpageSeeder.Enabled = true;
            Program.mainForm.txtFanpageSeederDelay.Enabled = true;
            Program.mainForm.cbFanpageSeeder.Enabled = true;
            Program.mainForm.txtFanpageSeeder.Enabled = true;

            MessageBox.Show("Hoàn thành Fanpage Seeder!");
            setReady(true);
        }

        public async void GraphSearch()
        {
            setReady(false, "Lấy thông tin Graph Search");
            ready2 = false;

            string search_request = "";
            if (Program.mainForm.cbGraphSearchMutual.Checked)
                search_request += "/me/friends/friends";
            string gs_relationship = ((KeyValuePair<string, string>)Program.mainForm.cbGraphSearchRelationship.SelectedItem).Value;
            if (gs_relationship != "") search_request += "/" + gs_relationship + "/users";
            //string gs_job = ((KeyValuePair<string, string>)Program.mainForm.cbGraphSearchJob.SelectedItem).Value;
            string gs_location = ((KeyValuePair<string, string>)Program.mainForm.cbGraphSearchLocation.SelectedItem).Value;
            if (gs_location != "") search_request += "/" + gs_location + "/residents-near/present";
            List<string> gs_pages = new List<string>();
            if (Program.mainForm.txtGraphSearchPage1.Text.Contains("https://www.facebook.com/"))
            {
                await Task.Factory.StartNew(() => Navigate(Program.mainForm.txtGraphSearchPage1.Text.Replace("www.facebook", "m.facebook")));
                var temp1 = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href,'/pages/more/')]"));
                if (temp1.Count > 0)
                {
                    Match match = Regex.Match(temp1[0].GetAttribute("href"), @"\/pages\/more\/([0-9]+)", RegexOptions.None);
                    if (match.Success)
                    {
                        gs_pages.Add(match.Groups[1].Value);
                    }
                }
            }
            if (Program.mainForm.txtGraphSearchPage2.Text.Contains("https://www.facebook.com/"))
            {
                await Task.Factory.StartNew(() => Navigate(Program.mainForm.txtGraphSearchPage2.Text.Replace("www.facebook", "m.facebook")));
                var temp1 = await Task.Factory.StartNew(() => driver.FindElementsByXPath("//a[contains(@href,'/pages/more/')]"));
                if (temp1.Count > 0)
                {
                    Match match = Regex.Match(temp1[0].GetAttribute("href"), @"\/pages\/more\/([0-9]+)", RegexOptions.None);
                    if (match.Success)
                    {
                        gs_pages.Add(match.Groups[1].Value);
                    }
                }
            }
            if (gs_pages.Count > 0)
            {
                foreach (string gs_page in gs_pages)
                {
                    search_request += "/" + gs_page + "/likers";
                }
            }
            if (Program.mainForm.txtGraphSearchUsersNamed.Text != "")
                search_request += "/str/" + Program.mainForm.txtGraphSearchUsersNamed.Text + "/users-named";
            string gs_gender = ((KeyValuePair<string, string>)Program.mainForm.cbGraphSearchGender.SelectedItem).Value;
            if (gs_gender != "") search_request += "/" + gs_gender;
            int gs_age1, gs_age2;
            if (int.TryParse(Program.mainForm.txtGraphSearchAge1.Text, out gs_age1) && int.TryParse(Program.mainForm.txtGraphSearchAge2.Text, out gs_age2) && gs_age2 >= gs_age1)
            {
                search_request += "/" + gs_age1 + "/" + gs_age2 + "/users-age-2";
            }

            int num_of_slash = 0;
            for (int k = 0; k < search_request.Length; k++)
            {
                if (search_request[k] == '/')
                    num_of_slash++;
            }
            if (num_of_slash > 3 || (num_of_slash == 3 && !search_request.Contains("users-age-2") && !search_request.Contains("users-named")))
                search_request += "/intersect";

            if (search_request != "" || Program.mainForm.txtGraphSearchGraphURL.Text.Contains("https://www.facebook.com/search"))
            {
                if (!Program.mainForm.txtGraphSearchGraphURL.Text.Contains("https://www.facebook.com/search"))
                    search_request = "https://www.facebook.com/search" + search_request;
                else
                    search_request = Program.mainForm.txtGraphSearchGraphURL.Text;
                Program.mainForm.txtGraphSearchGraphURL.Text = search_request;
                await ChromeAgent();
                Program.mainForm.lblStatus.Text = "Hoàn thành kết nối nâng cao";
                await Task.Factory.StartNew(() => driver2.Url = search_request);
                var targets = await Task.Factory.StartNew(() => driver2.FindElementsByXPath("//div[@id='BrowseResultsContainer']//div[@data-bt and @id and contains(@data-bt,'rank')]"));

                if (targets.Count > 0)
                {
                    Program.mainForm.lblStatus.Text = "Bắt đầu quét khách hàng mục tiêu";
                    // <div id='BrowseResultsContainer'>
                    foreach (IWebElement target in targets)
                    {
                        string sub_header = "";
                        var sub_headers = await Task.Factory.StartNew(() => target.FindElements(By.XPath(".//a[contains(@href,'ref=br_rs')]")));
                        if (sub_headers.Count > 1)
                            sub_header = sub_headers[1].Text;
                        Program.mainForm.dgUID.Rows.Insert(0, sub_header, target.GetAttribute("data-bt").Replace("{\"id\":", "").Split(',')[0]);
                    }
                    // <div id='~~~browse_result_below_fold'>
                    var targets_below_fold = await Task.Factory.StartNew(() => driver2.FindElementsByXPath("//div[contains(@id,'browse_result_below_fold')]//div[@data-bt and @id and contains(@data-bt,'rank')]"));
                    if (targets_below_fold.Count > 0)
                    {
                        foreach (IWebElement target_below_fold in targets_below_fold)
                        {
                            string sub_header = "";
                            var sub_headers = await Task.Factory.StartNew(() => target_below_fold.FindElements(By.XPath(".//a[contains(@href,'ref=br_rs')]")));
                            if (sub_headers.Count > 1)
                                sub_header = sub_headers[1].Text;
                            Program.mainForm.dgUID.Rows.Insert(0, sub_header, target_below_fold.GetAttribute("data-bt").Replace("{\"id\":", "").Split(',')[0]);
                        }
                    }
                    int scraped_page = 0;
                    while (true)
                    {
                        if (pause)
                            break;
                        driver2.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(0));
                        if (await Task.Factory.StartNew(() => driver2.FindElementsById("browse_end_of_results_footer").Count) == 1)
                            break;
                        driver2.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(3));
                        driver2.ExecuteScript(@"window.scrollTo(0,document.body.scrollHeight);");
                        await TaskEx.Delay(1000);
                        var scrolling_pager_container = await Task.Factory.StartNew(() => driver2.FindElementsByXPath("//div[contains(@id,'fbBrowseScrollingPagerContainer')]"));
                        while (scrolling_pager_container.Count > scraped_page && !pause)
                        {
                            var user_items = await Task.Factory.StartNew(() => scrolling_pager_container[scraped_page].FindElements(By.XPath(".//div[@data-bt and @id and contains(@data-bt,'rank')]")));
                            if (user_items.Count > 0)
                                foreach (IWebElement user_item in user_items)
                                {
                                    string sub_header = "";
                                    var sub_headers = await Task.Factory.StartNew(() => user_item.FindElements(By.XPath(".//a[contains(@href,'ref=br_rs')]")));
                                    if (sub_headers.Count > 1)
                                        sub_header = sub_headers[1].Text;
                                    Program.mainForm.lblStatus.Text = sub_header;
                                    Program.mainForm.dgUID.Rows.Insert(0, sub_header, user_item.GetAttribute("data-bt").Replace("{\"id\":", "").Split(',')[0]);
                                }

                            scraped_page++;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Không tìm thấy kết quả nào! Hãy chắc chắn bạn đang dùng ngôn ngữ Tiếng Việt hoặc English(US) cho Tài khoản FB của mình");
                }
            }

            Program.mainForm.btnGraphSearch.Enabled = true;
            Program.mainForm.txtGraphSearchPage1.Enabled = true;
            Program.mainForm.txtGraphSearchPage2.Enabled = true;
            Program.mainForm.cbGraphSearchRelationship.Enabled = true;
            Program.mainForm.cbGraphSearchGender.Enabled = true;
            Program.mainForm.cbGraphSearchLocation.Enabled = true;
            Program.mainForm.txtGraphSearchUsersNamed.Enabled = true;
            Program.mainForm.txtGraphSearchAge1.Enabled = true;
            Program.mainForm.txtGraphSearchAge2.Enabled = true;
            Program.mainForm.txtGraphSearchGraphURL.ReadOnly = false;
            Program.mainForm.txtGraphSearchGraphURL.Text = "";

            MessageBox.Show("Hoàn thành tìm kiếm nâng cao");
            setReady(true);
            ready2 = true;
        }

        #region OTHER HELPERS
        public void setReady(bool status, String message = "Sẵn sàng")
        {
            this.ready = status;
            Program.mainForm.lblStatus.Text = message;
            if (status)
            {
                Program.mainForm.imgStatus.Image = System.Drawing.Bitmap.FromFile("green.png");
                pause = false;
                Program.mainForm.btnPauseAll.Enabled = false;
                Program.mainForm.lblCountDown.Text = "Ready";
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
