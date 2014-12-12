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

namespace KSTN_Facebook_Tool
{
    class SeleniumControl
    {
        public OpenQA.Selenium.Firefox.FirefoxDriver driver;
        //OpenQA.Selenium.PhantomJS.PhantomJSDriver driver;
        Dictionary<String, String> links = new Dictionary<string, string>();
        Thread t;
        public bool ready = true;
        public bool pause = false;

        // User info
        String user_id = "";

        public SeleniumControl()
        {
            // URLs
            links["fb_url"] = "https://m.facebook.com";
            links["fb_get_token"] = "https://www.facebook.com/dialog/oauth?client_id=145634995501895&redirect_uri=https%3A%2F%2Fdevelopers.facebook.com%2Ftools%2Fexplorer%2Fcallback&response_type=token&scope=publish_actions,publish_stream,user_groups,user_friends";
            links["fb_groups"] = links["fb_url"] + "/browsegroups/?seemore";
            links["facebook_graph"] = "https://graph.facebook.com";
            links["fb_group_add"] = "https://m.facebook.com/groups/members/search/?group_id=";
            links["fb_photo_id"] = "https://www.facebook.com/photo.php?fbid=";
            links["fb_group_search_query"] = "https://m.facebook.com/search/?search=group&ssid=0&o=69&refid=46&pn=2&query="; // + &s=25 #skip
        }

        public void quit()
        {
            if (driver != null && !driver.ToString().Contains("null"))
            {
                Program.mainForm.autoIt.WinSetState("Mozilla Firefox", "", 1);
                driver.Quit();
            }
        }

        public void Toggle()
        {
            if (Program.mainForm.btnToggle.Checked)
            {
                Program.mainForm.autoIt.WinSetState("Mozilla Firefox", "", 1);
            }
            else
            {
                Program.mainForm.autoIt.WinSetState("Mozilla Firefox", "", 0);
            }
        }

        private void Exceptions_Handler()
        {
            Process.GetCurrentProcess().Kill();
        }

        private void Navigate(String URL)
        {
            driver.Url = URL;
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
            }
            catch { }
        }

        public async Task FBLogin(String user, String pass)
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
                this.driver = new OpenQA.Selenium.Firefox.FirefoxDriver(profile);
                Program.mainForm.autoIt.WinSetState("Mozilla Firefox", "", 0);
                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));
                driver.Manage().Timeouts().SetScriptTimeout(TimeSpan.FromSeconds(10));
                driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(30));
                setReady(true);
            }

            /*
            var driverService = OpenQA.Selenium.PhantomJS.PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            driver = new OpenQA.Selenium.PhantomJS.PhantomJSDriver(driverService);*/

            Program.loadingForm.setText("ĐĂNG NHẬP TÀI KHOẢN FACEBOOK...");
            setReady(false, "Đang đăng nhập");
            await Task.Factory.StartNew(() => Navigate(links["fb_url"]));
            try
            {
                InputValueAdd("email", user);
                InputValueAdd("pass", pass);
                await Task.Factory.StartNew(() => Click("login"));
            }
            catch
            {
                Program.loadingForm.RequestStop();
                t.Abort();
                t.Join();
                MessageBox.Show("Có lỗi với đường truyền mạng hoặc tài khoản facebook của bạn!\nHãy kiểm tra lại");
                Program.mainForm.btnLogin.Enabled = true;
                return;
            }

            setReady(true);
            //driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));

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
                Program.mainForm.btnComment.Enabled = true;

                //Program.mainForm.Focus();
                Program.loadingForm.setText("ĐĂNG NHẬP THÀNH CÔNG! ĐANG TẢI DANH SÁCH NHÓM...");
                await getGroups();
            }
            else
            {
                Program.loadingForm.RequestStop();
                t.Abort();
                t.Join();
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


            var nodes = driver.FindElementsByXPath("//div[@id='header']//div//a");
            Match match = Regex.Match(nodes[1].GetAttribute("href"), @"/([A-Za-z0-9\-]+)\?ref_component", RegexOptions.None);
            if (match.Success)
            {
                user_id = match.Groups[1].Value;
                Program.mainForm.pbAvatar.Load(links["facebook_graph"] + "/" + user_id + "/picture");
                Program.mainForm.lblViewProfile.Text = "https://facebook.com/" + user_id;
            }

            nodes = driver.FindElementsByXPath("//td[@style]//a");
            if (nodes.Count == 5)
            {
                match = Regex.Match(nodes[4].GetAttribute("innerHTML"), @"\((.*)\)$", RegexOptions.None);
                if (match.Success)
                {
                    Program.mainForm.lblUsername.Text = match.Groups[1].Value;
                }
            }

            Program.loadingForm.RequestStop();
            t.Abort();
            t.Join();

            var e = driver.FindElementsByXPath("//table//tbody//tr//td//div")[3].FindElements(By.XPath(".//li//table//tbody//tr//td//a"));
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

            //Program.mainForm.dgGroups.DataSource = Program.mainForm.dt;
            //Program.mainForm.dgGroupInvites.DataSource = Program.mainForm.dt;

            Program.mainForm.lblProgress.Text = "0/" + Program.mainForm.dgGroups.Rows.Count;
            Program.mainForm.btnLogin.Text = "Đăng xuất";
            Program.mainForm.btnLogin.Enabled = true;
            setReady(true, "Số lượng nhóm: " + Program.mainForm.dgGroups.Rows.Count + " | Ready");
        }

        private void addGroup2Grid(IWebElement k)
        {
            Program.mainForm.addGroup2Grid(k);
        }

        public async Task AutoPost()
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
                progress++;
                Program.mainForm.lblProgress.Text = progress + "/" + Program.mainForm.dgGroups.Rows.Count;
                Program.mainForm.lblPostingGroup.Text = row.Cells[0].Value.ToString();

                /*
                if (pause)
                {
                    Program.mainForm.txtContent.Enabled = true;
                    Program.mainForm.txtDelay.Enabled = true;
                    Program.mainForm.cbMethods.Enabled = true;
                    Program.mainForm.txtBrowse1.Enabled = true;
                    Program.mainForm.txtBrowse2.Enabled = true;
                    Program.mainForm.txtBrowse3.Enabled = true;
                    Program.mainForm.btnBrowse1.Enabled = true;
                    Program.mainForm.btnBrowse2.Enabled = true;
                    Program.mainForm.btnBrowse3.Enabled = true;
                }*/

                while (pause)
                {
                    await TaskEx.Delay(1000);
                }

                /*
                Program.mainForm.txtContent.Enabled = false;
                Program.mainForm.txtDelay.Enabled = false;
                Program.mainForm.cbMethods.Enabled = false;
                Program.mainForm.txtBrowse1.Enabled = false;
                Program.mainForm.txtBrowse2.Enabled = false;
                Program.mainForm.txtBrowse3.Enabled = false;
                Program.mainForm.btnBrowse1.Enabled = false;
                Program.mainForm.btnBrowse2.Enabled = false;
                Program.mainForm.btnBrowse3.Enabled = false;*/

                //Navigate(row.Cells[1].Value.ToString());
                await Task.Factory.StartNew(() => Navigate(row.Cells[1].Value.ToString()));
                while (true)
                {
                    var temp = await Task.Factory.StartNew(() => driver.FindElementsById("header"));
                    if (temp.Count > 0)
                    {
                        break;
                    }
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
                            continue;
                        }
                        driver.ExecuteScript(@"document.getElementsByName('xc_message')[0].innerHTML = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtContent.Text) + "';");

                        if (driver.FindElementsByName("view_post").Count == 0)
                        {
                            continue;
                        }
                        await Task.Factory.StartNew(() => Click("view_post"));
                        Program.mainForm.dgPostResult.Rows.Add(Program.mainForm.lblPostingGroup.Text, Uri.UnescapeDataString(driver.Url));
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
                        continue;
                    }
                    await Task.Factory.StartNew(() => Click("lgc_view_photo"));

                    if (driver.FindElementsByName("xc_message").Count == 0 || driver.FindElementsByName("file1").Count == 0 || driver.FindElementsByName("file2").Count == 0 || driver.FindElementsByName("file3").Count == 0 || driver.FindElementsByName("photo_upload").Count == 0)
                    {
                        continue;
                    }
                    await Task.Factory.StartNew(() => driver.ExecuteScript(@"document.getElementsByName('xc_message')[0].innerHTML = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtContent.Text) + "';"));

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
                    Match match = Regex.Match(Uri.UnescapeDataString(driver.Url) + "", @"\?photo_id\=([A-Za-z0-9\-]+)\&", RegexOptions.None);
                    if (match.Success)
                    {
                        result_url = links["fb_photo_id"] + match.Groups[1].Value;
                    }
                    else
                    {
                        result_url = Uri.UnescapeDataString(driver.Url);
                    }
                    Program.mainForm.dgPostResult.Rows.Insert(0, Program.mainForm.lblPostingGroup.Text, result_url);
                }

                for (int i = 0; i < delay + 1; i++)
                {
                    if (!pause)
                        Program.mainForm.lblTick.Text = delay - i + "";
                    if (i == delay && !pause)
                    {
                        Program.mainForm.lblTick.Text = "POSTING";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.btnPost.Enabled = true;
            Program.mainForm.txtContent.Enabled = false;
            Program.mainForm.txtDelay.Enabled = true;
            Program.mainForm.cbMethods.Enabled = true;
            Program.mainForm.txtBrowse1.Enabled = true;
            Program.mainForm.txtBrowse2.Enabled = true;
            Program.mainForm.txtBrowse3.Enabled = true;
            Program.mainForm.btnBrowse1.Enabled = true;
            Program.mainForm.btnBrowse2.Enabled = true;
            Program.mainForm.btnBrowse3.Enabled = true;
            Program.mainForm.dgGroups.Enabled = true;
            Program.mainForm.btnPause.Enabled = false;
            Program.mainForm.lblTick.Text = "Ready";

            MessageBox.Show("Đã hoàn thành đăng bài trong " + progress + " nhóm!");

            setReady(true);
        }

        public async Task AutoInvite()
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
                    await Task.Factory.StartNew(() => btnSearch.Click());
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
                    Program.mainForm.lblInviteTick.Text = delay - i + "";
                    if (i == delay)
                    {
                        Program.mainForm.lblInviteTick.Text = "Đang mời";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.txtInviteDelay.Enabled = false;
            Program.mainForm.txtInviteName.Enabled = false;
            Program.mainForm.btnInvite.Enabled = false;
            Program.mainForm.lblInviteTick.Text = "Ready";

            MessageBox.Show("Đã hoàn thành mời " + progress + " nhóm!");

            setReady(true);
        }

        public async Task GroupSearch()
        {
            setReady(false, "Đang tự động tìm nhóm");
            int success = 0;
            int skip = 0;
            Program.mainForm.lblSearching.Text = "Đang quét...";

            while (success < 10)
            {
                await Task.Factory.StartNew(() => driver.Url = links["fb_group_search_query"] + HttpUtility.UrlEncode(Program.mainForm.txtGroupSearch.Text) + "&s=" + skip);

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
                    await Task.Factory.StartNew(() => driver.Url = grlink[i]);

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

        public async Task AutoJoin()
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
                    inputs[2].Click();
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

        public async Task AutoComment()
        {
            setReady(false, "Đang tự động bình luận nhóm");
            int delay;

            if (!int.TryParse(Program.mainForm.txtCommentDelay.Text, out delay) || delay < 0)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 0");
                return;
            }
            Program.mainForm.lblCommentTick.Text = "Đang Comment...";
            Program.mainForm.lblCommenting.Text = "Đang lấy danh sách bài viết";
            await Task.Factory.StartNew(() => Navigate("https://m.facebook.com/" + user_id + "/allactivity?log_filter=groups"));

            List<String> group_log_links = new List<string>();

            var RecentStories = driver.FindElementsByXPath("//div[contains(@id, 'RecentStories')]");
            foreach (var RecentStory in RecentStories)
            {
                var tables = RecentStory.FindElements(By.XPath(".//table"));
                foreach (var table in tables)
                {
                    var a = table.FindElements(By.TagName("a"));
                    if (a.Count == 2 && a[0].GetAttribute("href").Contains("groups"))
                    {
                        await Task.Factory.StartNew(() => group_log_links.Add(a[0].GetAttribute("href")));
                    }
                }
            }

            Program.mainForm.lblCommentTick.Text = "Đang Comment";

            foreach (String post_url in group_log_links)
            {
                while (pause)
                {
                    await TaskEx.Delay(1000);
                }

                await Task.Factory.StartNew(() => Navigate(post_url));
                Program.mainForm.lblCommenting.Text = driver.Title;


                InputValueAdd("comment_text", Program.mainForm.txtComment.Text);

                IWebElement btnSubmit = driver.FindElementByXPath("//form[@method='post']//input[@type='submit']");
                await Task.Factory.StartNew(() => btnSubmit.Click());
                Program.mainForm.dgComment.Rows.Add(driver.Title, driver.Url);

                for (int i = 0; i < delay + 1; i++)
                {
                    if (!pause)
                        Program.mainForm.lblCommentTick.Text = delay - i + "";
                    if (i == delay && !pause)
                    {
                        Program.mainForm.lblCommentTick.Text = "Đang Comment";
                    }
                    await TaskEx.Delay(1000);
                }
            }

            Program.mainForm.lblCommentTick.Text = "Ready";
            Program.mainForm.cbCommentBefore.Enabled = true;
            Program.mainForm.cbCommentToday.Enabled = true;
            Program.mainForm.cbCommentYesterday.Enabled = true;
            Program.mainForm.txtComment.Enabled = true;
            Program.mainForm.txtCommentDelay.Enabled = true;
            Program.mainForm.btnComment.Enabled = true;
            Program.mainForm.btnCommentPause.Enabled = false;

            MessageBox.Show("Hoàn thành bình luận nhóm!");
            setReady(true, "Bình luận nhóm hoàn thành! | Ready");
        }

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

        public async Task Logout()
        {
            var nodes = driver.FindElementsByXPath("//td[@style]//a");
            if (nodes.Count == 5)
            {
                await Task.Factory.StartNew(() => nodes[4].Click());
            }
            var login = driver.FindElementsByName("login");
            if (login.Count == 0)
            {
                Exceptions_Handler();
            }
            else
            {
                Program.mainForm.btnLogin.Text = "Đăng nhập";
                Program.mainForm.dgGroups.Rows.Clear();
                Program.mainForm.AcceptButton = Program.mainForm.btnLogin;

                Program.mainForm.txtUser.Enabled = true;
                Program.mainForm.txtPass.Enabled = true;
                Program.mainForm.btnPost.Enabled = false;
                Program.mainForm.btnInvite.Enabled = false;
                Program.mainForm.btnGroupSearch.Enabled = false;
                Program.mainForm.btnGroupSearchFr.Enabled = false;
                Program.mainForm.btnGroupJoin.Enabled = false;
                Program.mainForm.btnComment.Enabled = false;
            }
        }
    }
}
