using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace KSTN_Facebook_Tool
{
    public partial class LoadingForm : Form
    {
        public LoadingForm()
        {
            InitializeComponent();
        }

        private volatile String title;

        public void setText(String txt)
        {
            title = txt;
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        private volatile bool _shouldStop = false;
        

        private void LoadingForm_Shown(object sender, EventArgs e)
        {
            check();
        }

        private async void check()
        {
            while (true)
            {
                txtLoading.Text = title;
                if (_shouldStop)
                {
                    this.Close();
                }
                await TaskEx.Delay(1000);
            }
        }
    }
}
