using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KSTN_Facebook_Tool
{
    public partial class TermsPolicies : Form
    {
        public TermsPolicies()
        {
            InitializeComponent();
        }

        private void TermsPolicies_Shown(object sender, EventArgs e)
        {
            txtMain.SelectionFont = new Font(txtMain.Font, FontStyle.Bold);
            txtMain.AppendText("Điều khoản và chính sách người dùng cuối" + Environment.NewLine);
            txtMain.SelectionFont = new Font(txtMain.Font, FontStyle.Bold);
            txtMain.AppendText(Environment.NewLine + "1. Chính sách" + Environment.NewLine + Environment.NewLine);
            txtMain.AppendText("1.1 Nhà phát triển cam kết không thu thập, lưu trữ cũng như sử dụng thông tin người dùng dưới bất kỳ hình thức nào." + Environment.NewLine);
            txtMain.AppendText("1.2 Trong khoảng thời gian hoạt động, ứng dụng sẽ không điều khiển thiết bị của bạn từ xa (nhằm mục đích botnet, ads click, DDOS, trojan... )" + Environment.NewLine);
            txtMain.AppendText("1.3 Ứng dụng luôn đảm bảo bảo mật thông tin cá nhân và bảo vệ quyền riêng tư của bạn trong quá trình sử dụng." + Environment.NewLine);
            txtMain.AppendText("1.4 Bạn có quyền được sửa đổi, sao chép chương trình với bất kỳ mục đích sử dụng nào." + Environment.NewLine);
            txtMain.SelectionFont = new Font(txtMain.Font, FontStyle.Bold);
            txtMain.AppendText(Environment.NewLine + "2. Điều khoản" + Environment.NewLine + Environment.NewLine);
            txtMain.AppendText("Bằng việc sử dụng ứng dụng này, bạn đã đồng ý các thỏa thuận sau:" + Environment.NewLine);
            txtMain.AppendText("2.1 Trong khoảng thời gian hoạt động, ứng dụng có toàn quyền điều khiển các dữ liệu và thao tác dựa trên các thông tin bạn cung cấp, tôn trọng tối đa chính sách người dùng cuối." + Environment.NewLine);
            txtMain.AppendText("2.2 Trong trường hợp xảy ra sự cố, nhà phát triển hoàn toàn không chịu trách nhiệm với thiết bị, tài khoản, thông tin cá nhân của bạn." + Environment.NewLine);
            txtMain.AppendText("2.3 Bạn đồng ý tự chịu trách nhiệm về mọi vi phạm nghĩa vụ dựa trên điều khoản và nguyên tắc của các bên thứ 3 liên quan.");
            txtMain.SelectionStart = 0;
        }
    }
}
