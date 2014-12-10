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
        String user_id;

        public SeleniumControl()
        {
            // URLs
            links["fb_url"] = "https://m.facebook.com";
            links["fb_get_token"] = "https://www.facebook.com/dialog/oauth?client_id=145634995501895&redirect_uri=https%3A%2F%2Fdevelopers.facebook.com%2Ftools%2Fexplorer%2Fcallback&response_type=token&scope=publish_actions,publish_stream,user_groups,user_friends";
            links["fb_groups"] = links["fb_url"] + "/browsegroups/?seemore";
            links["facebook_graph"] = "http://graph.facebook.com";
            links["fb_group_add"] = "https://m.facebook.com/groups/members/search/?group_id=";
            links["fb_photo_id"] = "https://www.facebook.com/photo.php?fbid=";
            links["fb_group_search_query"] = "https://m.facebook.com/search/?search=group&ssid=0&o=69&refid=46&pn=2&query="; // + &s=25 #skip
        }

        public void quit()
        {
            if (driver != null)
            {
                Program.mainForm.autoIt.WinSetState("Mozilla Firefox", "", 1);
                driver.Quit();
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
                //OpenQA.Selenium.IWebElement iElement = driver.FindElementByName(element_name);
                OpenQA.Selenium.IWebElement iElement = driver.FindElement(By.Name(element_name));
                driver.ExecuteScript(@"document.getElementsByName('" + element_name + "')[0].focus();");
                iElement.SendKeys(localpath);
            }
            catch
            {
                throw new Exception();
            }
        }

        private void InputValueAdd(String input_name, String value)
        {
            try
            {
                OpenQA.Selenium.IWebElement iElement = driver.FindElementByName(input_name);
                iElement.Clear();
                iElement.SendKeys(value);
            }
            catch
            {
                throw new Exception();
            }
        }

        private void Click(String element_name)
        {
            try
            {
                OpenQA.Selenium.IWebElement iElement = driver.FindElementByName(element_name);
                iElement.Click();
            }
            catch
            {
                throw new Exception();
            }
        }

        public async Task FBLogin(String user, String pass)
        {
            Program.mainForm.btnLogin.Enabled = false;
            Program.mainForm.TopMost = true;

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
                this.driver = new OpenQA.Selenium.Firefox.FirefoxDriver(profile);
                Program.mainForm.autoIt.WinSetState("Mozilla Firefox", "", 0);
                driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(30));
                setReady(true);
            }
            
            /*
            var driverService = OpenQA.Selenium.PhantomJS.PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            driver = new OpenQA.Selenium.PhantomJS.PhantomJSDriver(driverService);*/

            Program.loadingForm.setText("ĐĂNG NHẬP TÀI KHOẢN FACEBOOK...");
            setReady(false, "Đang đăng nhập");
            await Task.Factory.StartNew(() => FBLoginTask(user, pass));
            setReady(true);
            //driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));
            Program.mainForm.TopMost = false;

            if (getUrl().Contains("home.php") || getUrl().Contains("phoneacquire"))
            {

                Program.mainForm.btnLogin.Text = "Đăng nhập thành công!";
                Program.mainForm.txtUser.Enabled = false;
                Program.mainForm.txtPass.Enabled = false;
                Program.mainForm.btnPost.Enabled = true;
                Program.mainForm.btnInvite.Enabled = true;
                Program.mainForm.btnGroupSearch.Enabled = true;
                Program.mainForm.btnGroupSearchFr.Enabled = true;

                //Program.mainForm.Focus();
                Program.loadingForm.RequestStop();
                t.Abort();
                t.Join();
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

        private void FBLoginTask(String user, String pass)
        {
            Navigate(links["fb_url"]);
            InputValueAdd("email", user);
            InputValueAdd("pass", pass);
            Click("login");
            /*
            WebDriverWait wait = new WebDriverWait(webDriver, timeoutInSeconds);
            wait.until(ExpectedConditions.visibilityOfElementLocated(By.id<locator>));
            or

            wait.until(ExpectedConditions.elementToBeClickable(By.id<locator>));
            */
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

            var e = driver.FindElementsByXPath("//table//tbody//tr//td//div")[3].FindElements(By.XPath(".//li//table//tbody//tr//td//a"));
            foreach (IWebElement k in e)
            {
                //Program.mainForm.dt.Rows.Add(k.GetAttribute("innerHTML"), k.GetAttribute("href"), "");
                addGroup2Grid(k);
                await TaskEx.Delay(1);
                //Program.mainForm.Invoke(new MethodInvoker(delegate() { addGroup2Grid(k); }));
                //new Thread(() => Program.mainForm.Invoke(new MethodInvoker(delegate() { addGroup2Grid(k); }))).Start();
                //Thread t = new Thread(() => Program.mainForm.Invoke(new MethodInvoker(delegate() { addGroup2Grid(k); })));
                //t.Start();
            }

            //Program.mainForm.dgGroups.DataSource = Program.mainForm.dt;
            //Program.mainForm.dgGroupInvites.DataSource = Program.mainForm.dt;

            Program.mainForm.lblProgress.Text = "0/" + Program.mainForm.dgGroups.Rows.Count;

            setReady(true);
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

                if (Program.mainForm.txtBrowse1.Text == "" && Program.mainForm.txtBrowse2.Text == "" && Program.mainForm.txtBrowse3.Text == "")
                {
                    // Không ảnh
                    if (Program.mainForm.txtContent.Text != "")
                    {
                        //driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(10));
                        //InputValueAdd("xc_message", Program.mainForm.txtContent.Text);
                        while (driver.FindElementsById("header").Count == 0)
                        {
                            await Task.Factory.StartNew(() => driver.Navigate().Refresh());
                            Program.mainForm.lblTick.Text = "(!) Mạng";
                            await TaskEx.Delay(1000);
                        }

                        if (driver.FindElementsByName("xc_message").Count == 0)
                        {
                            continue;
                        }
                        driver.ExecuteScript(@"document.getElementsByName('xc_message')[0].innerHTML = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtContent.Text) + "';");
                        //Click("view_post");
                        try
                        {
                            await Task.Factory.StartNew(() => Click("view_post"));
                        }
                        catch
                        {
                            continue;
                        }
                        Program.mainForm.dgPostResult.Rows.Add(Program.mainForm.lblPostingGroup.Text, Uri.UnescapeDataString(driver.Url));
                    }
                    else
                    {
                        MessageBox.Show("Điền nội dung trước khi post bài!");
                    }
                }
                else
                {
                    while (driver.FindElementsById("header").Count == 0)
                    {
                        await Task.Factory.StartNew(() => driver.Navigate().Refresh());
                        Program.mainForm.lblTick.Text = "(!) Mạng";
                        await TaskEx.Delay(1000);
                    }
                    // Có ảnh
                    try
                    {
                        //Click("lgc_view_photo");
                        await Task.Factory.StartNew(() => Click("lgc_view_photo"));
                    }
                    catch
                    {
                        continue;
                    }
                    //await Task.Factory.StartNew(() => new WebDriverWait(driver, TimeSpan.FromSeconds(10)));

                    driver.ExecuteScript(@"document.getElementsByName('xc_message')[0].innerHTML = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtContent.Text) + "';");

                    if (Program.mainForm.txtBrowse1.Text != "")
                    {
                        FileInputAdd("file1", Program.mainForm.txtBrowse1.Text);
                    }
                    if (Program.mainForm.txtBrowse2.Text != "")
                    {
                        FileInputAdd("file2", Program.mainForm.txtBrowse2.Text);
                    }
                    if (Program.mainForm.txtBrowse3.Text != "")
                    {
                        FileInputAdd("file3", Program.mainForm.txtBrowse3.Text);
                    }

                    // Click("photo_upload");
                    await Task.Factory.StartNew(() => Click("photo_upload"));

                    String result_url = "";
                    Match match = Regex.Match(Uri.UnescapeDataString(driver.Url) + "", @"\?photo_fbid\=([A-Za-z0-9\-]+)\&id\=", RegexOptions.None);
                    if (match.Success)
                    {
                        result_url = links["fb_photo_id"] + match.Groups[1].Value;
                    }
                    else
                    {
                        result_url = Uri.UnescapeDataString(driver.Url);
                    }
                    Program.mainForm.dgPostResult.Rows.Add(Program.mainForm.lblPostingGroup.Text, result_url);
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

                String group_id = row.Cells[1].Value.ToString().Substring(35);
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
                    await Task.Factory.StartNew(() => input.Click());
                    var btnSubmits = form2.FindElements(By.TagName("input"));
                    IWebElement btnSubmit = btnSubmits[btnSubmits.Count - 1];
                    await Task.Factory.StartNew(() => btnSubmit.Click());
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

            MessageBox.Show("Hoàn thành tìm nhóm! + (" + success + ")");
            Program.mainForm.lblSearching.Text = "Ready";

            setReady(true);
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
                    await Task.Factory.StartNew(() => inputs[2].Click());
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

            Program.mainForm.txtJoinDelay.Enabled = true;
            Program.mainForm.btnGroupJoin.Enabled = true;

            MessageBox.Show("Hoàn thành Join nhóm!");

            setReady(true);
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
                Program.mainForm.imgStatus.Image = System.Drawing.Bitmap.FromFile("red.png");
            }
        }
    }
}
