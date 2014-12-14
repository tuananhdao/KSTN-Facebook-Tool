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
using System.Security.Cryptography;

namespace KSTN_Facebook_Tool
{
    public partial class License : Form
    {
        public License()
        {
            InitializeComponent();
        }

        private void License_Shown(object sender, EventArgs e)
        {
            if (txtLicense.Text == "")
            {
                txtLicense.Text = "Trong giây lát...";
                CalculateLicense();
            }
        }

        private async void CalculateLicense()
        {
            txtLicense.Text = await Task.Factory.StartNew(() =>FingerPrint.Value());
            CalculateMD5Hash(txtLicense.Text + DateTime.Today.Month);
        }

        public void CalculateMD5Hash(string input)
        {
            // To calculate MD5 hash from an input string

            MD5 md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            // convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {

                //to make hex string use lower case instead of uppercase add parameter “X2″

                sb.Append(hash[i].ToString("X2"));

            }

            txtKey.Text = sb.ToString();
        }
    }
}
