using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Google.Voice;
using Google.Voice.Entities;
using Sipek.Common;
using Sipek.Common.CallControl;
using Sipek.Sip;

namespace GoogleVoiceResponder
{
    public partial class MainForm : Form
    {
        private const string GV_USERNAME_KEY = "username";
        private const string GV_PASSWORD_KEY = "password";
        private readonly GoogleVoice _gv;
        private SipekResources _resources = null;
        private Mutex _sipekMutex;
        public bool IsInitialized
        {
            get { return SipekResources.StackProxy.IsInitialized; }
        }

        private SipekResources SipekResources
        {
            get { return _resources; }
        }
        private int _session = -1;
        public MainForm()
        {
            InitializeComponent();
            _gv = new GoogleVoice();
            // Create resource object containing SipekSdk and other Sipek related data
            _resources = new SipekResources(this);
            _sipekMutex = new Mutex(false);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GetFolderResult getTextsResult;
            lock (_gv)
            {
                getTextsResult = _gv.Texts(null, true);
            }
            if (getTextsResult.unreadCounts.inbox > 0)
            {
                lbStatus.Items.Add(String.Format("received {0} text messages", getTextsResult.unreadCounts.inbox));
                foreach (var msg in getTextsResult.messages)
                {
                    ThreadPool.QueueUserWorkItem(OnProcessTexts, msg.Value);
                }
            }

        }

        private void OnProcessTexts(Object state)
        {
            if (_sipekMutex.WaitOne())
            {
                if (_session != -1)
                {
                    _sipekMutex.ReleaseMutex();
                    return;
                }
                try
                {
                    BeginInvoke(new InitiateCallDelegate(InitiateCall), (Google.Voice.Entities.Message)state);
                }
                catch
                {
                    _sipekMutex.ReleaseMutex();
                    throw;
                }
            }
        }

        private delegate void InitiateCallDelegate(Google.Voice.Entities.Message msg);
        private void InitiateCall(Google.Voice.Entities.Message msg)
        {
            msg.isRead = true;
            var callback = msg.phoneNumber;
            BeginInvoke(new EventHandler(UpdateStatus), String.Format("Calling {0}", msg.displayNumber),
                        EventArgs.Empty);

            var stateMachine = SipekResources.CallManager.createOutboundCall(callback);
            _session = stateMachine.Session;

            if (_session != -1)
            {
                var messages = new List<Google.Voice.Entities.Message>();
                messages.Clear();
                messages.Add(msg);
                BeginInvoke(new EventHandler(UpdateStatus), String.Format("Connected to {0}", callback),
                            EventArgs.Empty);
                bool markMsgResult;
                lock (_gv)
                {
                    markMsgResult = _gv.MarkMessages(messages);
                }
                if (!markMsgResult)
                {
                    BeginInvoke(new EventHandler(UpdateStatus),
                                String.Format("Failed to mark message {0} from {1} read", msg.id,
                                              msg.displayNumber),
                                EventArgs.Empty);
                }
            }
            else
            {
                BeginInvoke(new EventHandler(UpdateStatus), String.Format("Call to {0} failed", callback),
                            EventArgs.Empty);
            }
        }


        private void UpdateStatus(object obj, EventArgs e)
        {
            lbStatus.Items.Add((string)obj);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {

                Font = SystemFonts.DefaultFont;

                timer1.Interval = 1000;
                statusStrip1.Text = "Enter your Google Voice Credentials and select a file to respond to your messages with";

                SipekResources.CallManager.CallStateRefresh += new DCallStateRefresh(OnCallStateChanged);
                SipConfigStruct.Instance.logLevel = 6;
                SipConfigStruct.Instance.autoPlayHangup = true;
                SipekResources.Configurator.AutoPlayHangup = true;
                var stat = SipekResources.CallManager.Initialize(pjsipStackProxy.Instance);
                if (stat == 0)
                {
                    SipekResources.CallManager.CallLogger = SipekResources.CallLogger;
                    pjsipRegistrar.Instance.registerAccounts();
                }
                else
                {
                    lbStatus.Items.Add("Sipek Initialization failed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("OnLoad: " + ex.Message);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                lbStatus.Items.Clear();
                var loginResult = _gv.Login(GoogleVoiceAccount.Instance.Username, GoogleVoiceAccount.Instance.Password);
                if (!loginResult.RequiresRelogin)
                {
                    lbStatus.Items.Add("Logged in " + GoogleVoiceAccount.Instance.Username);
                    btnStart.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                lbStatus.Items.Add(ex.Message);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
            _sipekMutex.WaitOne();
            if (_session != -1)
            {
                SipekResources.CallManager.OnUserRelease(_session);
            }
            SipekResources.CallManager.Shutdown();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            _sipekMutex.WaitOne();
            if (_session != -1)
            {
                SipekResources.CallManager.OnUserRelease(_session);
            }
            btnStop.Enabled = false;
            btnStart.Enabled = true;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            lbStatus.Items.Add("Started polling");
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            timer1.Start();
        }

        delegate void DRefreshForm();
        public void OnCallStateChanged(int sessionId)
        {
            if (InvokeRequired)
                this.BeginInvoke(new DRefreshForm(this.RefreshForm));
            else
                RefreshForm();
        }

        private void RefreshForm()
        {
            try
            {
                // get entire call list
                Dictionary<int, IStateMachine> callList = SipekResources.CallManager.CallList;

                foreach (KeyValuePair<int, IStateMachine> kvp in callList)
                {
                    string number = kvp.Value.CallingNumber;
                    string name = kvp.Value.CallingName;

                    string duration = kvp.Value.Duration.ToString();
                    if (duration.IndexOf('.') > 0) duration = duration.Remove(duration.IndexOf('.')); // remove miliseconds
                    // show name & number or just number
                    string display = name.Length > 0 ? name + " / " + number : number;
                    string stateName = kvp.Value.StateId.ToString();
                    if (SipekResources.CallManager.Is3Pty) stateName = "CONFERENCE";
                    lbStatus.Items.Add(String.Format("Call {0} {1} duration {2}",display, stateName, duration));

                }
            }
            catch (Exception e)
            {
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void configureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(SipekResources))
            {
                settingsForm.ShowDialog(this);
            }
            
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new TestForm(SipekResources);
            form.Show(this);
        }

    }
}
