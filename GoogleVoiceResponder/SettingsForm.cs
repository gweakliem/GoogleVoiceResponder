using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sipek.Common;

namespace GoogleVoiceResponder
{
    public partial class SettingsForm : Form
    {
        private SipekResources _resources = null;
        public SipekResources SipekResources
        {
            get { return _resources; }
        }

        public SettingsForm(SipekResources resources)
        {
            InitializeComponent();
            _resources = resources;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            GoogleVoiceAccount.Instance.Username = txtLoginEmail.Text;
            GoogleVoiceAccount.Instance.Password = txtPassword.Text;

            IAccount account = SipekResources.Configurator.Accounts[0];

            account.Enabled = true;
            account.HostName = textBoxRegistrarAddress.Text;
            account.ProxyAddress = "";
            account.AccountName = textBoxDisplayName.Text;
            account.DisplayName = textBoxDisplayName.Text;
            account.Id = textBoxUsername.Text;
            account.UserName = textBoxUsername.Text;
            account.Password = textBoxPassword.Text;
            account.DomainName = textBoxDomain.Text;
            account.TransportMode = ETransportMode.TM_UDP;

            SipekResources.Configurator.WavFile = txtFileName.Text;
            SipekResources.Configurator.AutoPlayHangup = true;
            SipekResources.Configurator.DefaultAccountIndex = 0;

            SipekResources.Configurator.Save();

            // set device Id
            //SipekResources.StackProxy.setSoundDevice(mMixers.Playback.DeviceDetail.MixerName, mMixers.Recording.DeviceDetail.MixerName);

            Close();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            txtLoginEmail.Text = GoogleVoiceAccount.Instance.Username;
            txtPassword.Text = GoogleVoiceAccount.Instance.Password;
            txtFileName.Text = SipekResources.Configurator.WavFile;
            IAccount account = SipekResources.Configurator.Accounts[0];

            textBoxRegistrarAddress.Text = account.HostName;
            textBoxDisplayName.Text = account.AccountName;
            textBoxUsername.Text = account.Id;
            textBoxUsername.Text = account.UserName;
            textBoxPassword.Text = account.Password;
            textBoxDomain.Text = account.DomainName;
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtFileName.Text = openFileDialog1.FileName;
            }
        }
    }
}
