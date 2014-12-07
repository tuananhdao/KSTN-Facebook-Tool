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

namespace KSTN_Facebook_Tool
{
    class SeleniumControl
    {
        OpenQA.Selenium.Firefox.FirefoxDriver driver;
        //OpenQA.Selenium.PhantomJS.PhantomJSDriver driver;
        Dictionary<String, String> links = new Dictionary<string, string>();
        Thread t = new System.Threading.Thread(() => Program.loadingForm.ShowDialog());
        public bool ready = true;

        // User info
        String user_id;

        public SeleniumControl()
        {
            // this.driver = new OpenQA.Selenium.Firefox.FirefoxDriver();

            // URLs
            links["fb_url"] = "https://mbasic.facebook.com";
            links["fb_get_token"] = "https://www.facebook.com/dialog/oauth?client_id=145634995501895&redirect_uri=https%3A%2F%2Fdevelopers.facebook.com%2Ftools%2Fexplorer%2Fcallback&response_type=token&scope=publish_actions,publish_stream,user_groups,user_friends";
            links["fb_groups"] = links["fb_url"] + "/browsegroups/?seemore";
            links["facebook_graph"] = "http://graph.facebook.com";
            links["fb_group_add"] = "https://mbasic.facebook.com/groups/members/search/?group_id=";
            links["fb_photo_id"] = "https://www.facebook.com/photo.php?fbid=";
            links["fb_group_search_query"] = "https://m.facebook.com/search/?search=group&ssid=0&o=69&refid=46&pn=2&query="; // + &s=25 #skip
        }

        public void quit()
        {
            if (driver != null)
                driver.Quit();
        }

        private void Exceptions_Handler()
        {

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

            Program.loadingForm.setText("KHỞI TẠO TRÌNH DUYỆT...");
            //Program.loadingForm.Show();
            //Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(() => Program.loadingForm.ShowDialog()));

            t.Start();
            // Bật trình duyệt khi Login
            
            this.driver = new OpenQA.Selenium.Firefox.FirefoxDriver();
            Program.mainForm.autoIt.WinSetState("Mozilla Firefox", "", 0);
            /*
            var driverService = OpenQA.Selenium.PhantomJS.PhantomJSDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            driver = new OpenQA.Selenium.PhantomJS.PhantomJSDriver(driverService);*/

            Program.loadingForm.setText("ĐĂNG NHẬP TÀI KHOẢN FACEBOOK...");
            await Task.Factory.StartNew(() => FBLoginTask(user, pass));

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

                Program.loadingForm.setText("ĐĂNG NHẬP THÀNH CÔNG! ĐANG TẢI VỀ DANH SÁCH NHÓM...");

                //Program.mainForm.Focus();
                await getGroups();
                Program.loadingForm.RequestStop();
                t.Abort();
                t.Join();
            }
            else
            {
                Program.loadingForm.RequestStop();
                t.Abort();
                t.Join();
                MessageBox.Show("Kiểm tra lại thông tin đăng nhập!");
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
            await Task.Factory.StartNew(() => Navigate(links["fb_groups"]));

            var e = driver.FindElementsByXPath("//table//tbody//tr//td//div")[3].FindElements(By.XPath(".//li//table//tbody//tr//td//a"));
            foreach (IWebElement k in e)
            {
                //Program.mainForm.dt.Rows.Add(k.GetAttribute("innerHTML"), k.GetAttribute("href"), "");
                addGroup2Grid(k);
                //Program.mainForm.Invoke(new MethodInvoker(delegate() { addGroup2Grid(k); }));
                //new Thread(() => Program.mainForm.Invoke(new MethodInvoker(delegate() { addGroup2Grid(k); }))).Start();
            }

            //Program.mainForm.dgGroups.DataSource = Program.mainForm.dt;
            //Program.mainForm.dgGroupInvites.DataSource = Program.mainForm.dt;

            var nodes = driver.FindElementsByXPath("//div[@id='header']//div//a");
            Match match = Regex.Match(nodes[2].GetAttribute("href"), @"/([A-Za-z0-9\-]+)\?ref_component", RegexOptions.None);
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
        }

        private void addGroup2Grid(IWebElement k)
        {
            Program.mainForm.dgGroups.Rows.Add(k.GetAttribute("innerHTML"), k.GetAttribute("href"), "");
        }

        public async Task AutoPost()
        {
            Program.mainForm.lblTick.Text = "POSTING";
            ready = false;
            int delay;
            int progress = 0;

            if (!int.TryParse(Program.mainForm.txtDelay.Text, out delay) || delay < 10)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 10");
                Exceptions_Handler();
            }

            foreach (DataGridViewRow row in Program.mainForm.dgGroups.Rows)
            {
                progress++;
                Program.mainForm.lblProgress.Text = progress + "/" + Program.mainForm.dgGroups.Rows.Count;
                Program.mainForm.lblPostingGroup.Text = row.Cells[0].Value.ToString();

                //Navigate(row.Cells[1].Value.ToString());
                await Task.Factory.StartNew(() => Navigate(row.Cells[1].Value.ToString()));

                if (Program.mainForm.txtBrowse1.Text == "" && Program.mainForm.txtBrowse2.Text == "" && Program.mainForm.txtBrowse3.Text == "")
                {
                    // Không ảnh
                    if (Program.mainForm.txtContent.Text != "")
                    {
                        //driver.Manage().Timeouts().SetPageLoadTimeout(TimeSpan.FromSeconds(10));
                        //InputValueAdd("xc_message", Program.mainForm.txtContent.Text);
                        if (driver.FindElementsByName("xc_message").Count == 0)
                            continue;
                        driver.ExecuteScript(@"document.getElementsByName('xc_message')[0].innerHTML = '" + System.Web.HttpUtility.JavaScriptStringEncode(Program.mainForm.txtContent.Text) + "';");
                        //Click("view_post");
                        await Task.Factory.StartNew(() => Click("view_post"));
                        Program.mainForm.dgPostResult.Rows.Add(Program.mainForm.lblPostingGroup.Text, Uri.UnescapeDataString(driver.Url));
                    }
                    else
                    {
                        MessageBox.Show("Điền nội dung trước khi post bài!");
                    }
                }
                else
                {
                    // Có ảnh
                    try
                    {
                        //Click("lgc_view_photo");
                        await Task.Factory.StartNew(() => Click("lgc_view_photo"));
                    }
                    catch {
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
                    Program.mainForm.lblTick.Text = delay - i + "";
                    if (i == delay)
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

            ready = true;
        }

        public async Task AutoInvite()
        {
            ready = false;
            Program.mainForm.lblInviteTick.Text = "Đang mời";
            int delay;

            if (!int.TryParse(Program.mainForm.txtInviteDelay.Text, out delay) || delay < 10)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 10");
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

            ready = true;
        }

        public async Task GroupSearch()
        {
            ready = false;

            ready = true;
        }
    }
}
