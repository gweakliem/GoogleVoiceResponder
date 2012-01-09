using System;
using System.Windows.Forms;
using Sipek.Common;

namespace GoogleVoiceResponder
{
    public partial class TestForm : Form
    {
        private int _session;
        private SipekResources _resources = null;
        public bool IsInitialized
        {
            get { return SipekResources.StackProxy.IsInitialized; }
        }

        private SipekResources SipekResources
        {
            get { return _resources; }
        }

        public TestForm(SipekResources resources)
        {
            InitializeComponent();
            _resources = resources;
            SipekResources.StackProxy.setWavPlayerEndedCallback(WavPlayerEndedCallback);
        }

        private void btnTestCall_Click(object sender, EventArgs e)
        {
            var callback = txtPhoneNumber.Text;
            var stateMachine = SipekResources.CallManager.createOutboundCall(callback);
            _session = stateMachine.Session;

            if (_session == -1)
            {
                MessageBox.Show(String.Format("Call to {0} failed", callback));
            }
        }

        private void btnPlayWav_Click(object sender, EventArgs e)
        {
            if (_session != -1)
            {
                int result = SipekResources.StackProxy.playWav(SipekResources.Configurator.WavFile, _session);
                if (result == -1)
                {
                    MessageBox.Show(String.Format("playWav failed {0}", result));
                }
            }
        }

        private int WavPlayerEndedCallback(int callId, int playerId)
        {
            this.BeginInvoke(new IVoipProxy.WavPlayerEndedCallback(WavePlayerEndedUpdate), callId, playerId);
            return 0;
        }

        private int WavePlayerEndedUpdate(int callId, int playerId)
        {
            SipekResources.CallManager.OnUserRelease(_session);
            return 0;
        }

        private void btnHangup_Click(object sender, EventArgs e)
        {
            if (_session != -1)
            {
                SipekResources.CallManager.OnUserRelease(_session);
            }

        }
    }
}
