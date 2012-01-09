using System;
using System.Collections.Generic;
using System.Timers;
using System.Media;
using Sipek.Common;
using Sipek.Common.CallControl;
using Sipek.Sip;


namespace GoogleVoiceResponder
{
    /// <summary>
    /// ConcreteFactory 
    /// Implementation of AbstractFactory. 
    /// </summary>
    public class SipekResources : AbstractFactory
    {
        MainForm _form; // reference to MainForm to provide timer context
        IMediaProxyInterface _mediaProxy = new CMediaPlayerProxy();
        ICallLogInterface _callLogger = new CCallLog();
        pjsipStackProxy _stackProxy = pjsipStackProxy.Instance;
        SipekConfigurator _config = new SipekConfigurator();

        #region Constructor
        public SipekResources(MainForm mf)
        {
            _form = mf;

            // initialize sip struct at startup
            SipConfigStruct.Instance.stunServer = this.Configurator.StunServerAddress;
            SipConfigStruct.Instance.publishEnabled = this.Configurator.PublishEnabled;
            SipConfigStruct.Instance.expires = this.Configurator.Expires;
            SipConfigStruct.Instance.VADEnabled = this.Configurator.VADEnabled;
            SipConfigStruct.Instance.ECTail = this.Configurator.ECTail;
            SipConfigStruct.Instance.nameServer = this.Configurator.NameServer;

            // initialize modules
            _callManager.StackProxy = _stackProxy;
            _callManager.Config = _config;
            _callManager.Factory = this;
            _callManager.MediaProxy = _mediaProxy;
            _stackProxy.Config = _config;
            _registrar.Config = _config;
            _messenger.Config = _config;

            // do not save account state
            for (int i = 0; i < 5; i++)
            {
                GoogleVoiceResponder.Default.cfgSipAccountState[i] = "0";
                GoogleVoiceResponder.Default.cfgSipAccountIndex[i] = "0";
            }
        }
        #endregion Constructor

        #region AbstractFactory methods
        public ITimer createTimer()
        {
            return new GUITimer(_form);
        }

        public IStateMachine createStateMachine()
        {
            // TODO: check max number of calls
            return new CStateMachine();
        }

        #endregion

        #region Other Resources
        public pjsipStackProxy StackProxy
        {
            get { return _stackProxy; }
            set { _stackProxy = value; }
        }

        public SipekConfigurator Configurator
        {
            get { return _config; }
        }

        // getters
        public IMediaProxyInterface MediaProxy
        {
            get { return _mediaProxy; }
            set { }
        }

        public ICallLogInterface CallLogger
        {
            get { return _callLogger; }
            set { }
        }

        private IRegistrar _registrar = pjsipRegistrar.Instance;
        public IRegistrar Registrar
        {
            get { return _registrar; }
        }

        private IPresenceAndMessaging _messenger = pjsipPresenceAndMessaging.Instance;
        public IPresenceAndMessaging Messenger
        {
            get { return _messenger; }
        }

        private CCallManager _callManager = CCallManager.Instance;
        public CCallManager CallManager
        {
            get { return CCallManager.Instance; }
        }
        #endregion
    }

    #region Concrete implementations

    public class GUITimer : ITimer
    {
        Timer _guiTimer;
        MainForm _form;


        public GUITimer(MainForm mf)
        {
            _form = mf;
            _guiTimer = new Timer();
            if (this.Interval > 0) _guiTimer.Interval = this.Interval;
            _guiTimer.Interval = 100;
            _guiTimer.Enabled = true;
            _guiTimer.Elapsed += new ElapsedEventHandler(_guiTimer_Tick);
        }

        void _guiTimer_Tick(object sender, EventArgs e)
        {
            _guiTimer.Stop();
            //_elapsed(sender, e);
            // Synchronize thread with GUI because SIP stack works with GUI thread only
            if ((_form.IsDisposed) || (_form.Disposing) || (!_form.IsInitialized))
            {
                return;
            }
            _form.Invoke(_elapsed, new object[] { sender, e });
        }

        public bool Start()
        {
            _guiTimer.Start();
            return true;
        }

        public bool Stop()
        {
            _guiTimer.Stop();
            return true;
        }

        private int _interval;
        public int Interval
        {
            get { return _interval; }
            set { _interval = value; _guiTimer.Interval = value; }
        }

        private TimerExpiredCallback _elapsed;
        public TimerExpiredCallback Elapsed
        {
            set
            {
                _elapsed = value;
            }
        }
    }


    // Accounts
    public class SipekAccount : IAccount
    {
        private int _index = -1;
        private int _accountIdentification = -1;

        public bool Enabled
        {
            get
            {
                bool value;
                if (Boolean.TryParse(GoogleVoiceResponder.Default.cfgSipAccountEnabled[_index], out value))
                {
                    return value;
                }
                return false;
            }

            set { GoogleVoiceResponder.Default.cfgSipAccountEnabled[_index] = value.ToString(); }
        }

        /// <summary>
        /// Temp storage!
        /// The account index assigned by voip stack
        /// </summary>
        public int Index
        {
            get
            {
                int value;
                if (Int32.TryParse(GoogleVoiceResponder.Default.cfgSipAccountIndex[_index], out value))
                {
                    return value;
                }
                return -1;
            }
            set { GoogleVoiceResponder.Default.cfgSipAccountIndex[_index] = value.ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index">the account identification used by configuration (values 0..4)</param>
        public SipekAccount(int index)
        {
            _index = index;
        }

        #region Properties

        public string AccountName
        {
            get
            {
                return GoogleVoiceResponder.Default.cfgSipAccountNames[_index];
            }
            set
            {
                GoogleVoiceResponder.Default.cfgSipAccountNames[_index] = value;
            }
        }

        public string HostName
        {
            get
            {
                return GoogleVoiceResponder.Default.cfgSipAccountAddresses[_index];
            }
            set
            {
                GoogleVoiceResponder.Default.cfgSipAccountAddresses[_index] = value;
            }
        }

        public string Id
        {
            get
            {
                return GoogleVoiceResponder.Default.cfgSipAccountIds[_index];
            }
            set
            {
                GoogleVoiceResponder.Default.cfgSipAccountIds[_index] = value;
            }
        }

        public string UserName
        {
            get
            {
                return GoogleVoiceResponder.Default.cfgSipAccountUsername[_index];
            }
            set
            {
                GoogleVoiceResponder.Default.cfgSipAccountUsername[_index] = value;
            }
        }

        public string Password
        {
            get
            {
                return GoogleVoiceResponder.Default.cfgSipAccountPassword[_index];
            }
            set
            {
                GoogleVoiceResponder.Default.cfgSipAccountPassword[_index] = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return GoogleVoiceResponder.Default.cfgSipAccountDisplayName[_index];
            }
            set
            {
                GoogleVoiceResponder.Default.cfgSipAccountDisplayName[_index] = value;
            }
        }

        public string DomainName
        {
            get
            {
                return GoogleVoiceResponder.Default.cfgSipAccountDomains[_index];
            }
            set
            {
                GoogleVoiceResponder.Default.cfgSipAccountDomains[_index] = value;
            }
        }

        public int RegState
        {
            get
            {
                int value;
                if (Int32.TryParse(GoogleVoiceResponder.Default.cfgSipAccountState[_index], out value))
                {
                    return value;
                }
                return 0;
            }
            set
            {
                GoogleVoiceResponder.Default.cfgSipAccountState[_index] = value.ToString();
            }
        }

        public string ProxyAddress
        {
            get
            {
                return GoogleVoiceResponder.Default.cfgSipAccountProxyAddresses[_index];
            }
            set
            {
                GoogleVoiceResponder.Default.cfgSipAccountProxyAddresses[_index] = value;
            }
        }

        public ETransportMode TransportMode
        {
            get
            {
                int value;
                if (Int32.TryParse(GoogleVoiceResponder.Default.cfgSipAccountTransport[_index], out value))
                {
                    return (ETransportMode)value;
                }
                return (ETransportMode.TM_UDP); // default
            }
            set
            {
                GoogleVoiceResponder.Default.cfgSipAccountTransport[_index] = ((int)value).ToString();
            }
        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class SipekConfigurator : IConfiguratorInterface
    {
        public bool IsNull { get { return false; } }

        public bool CFUFlag
        {
            get { return GoogleVoiceResponder.Default.cfgCFUFlag; }
            set { GoogleVoiceResponder.Default.cfgCFUFlag = value; }
        }
        public string CFUNumber
        {
            get { return GoogleVoiceResponder.Default.cfgCFUNumber; }
            set { GoogleVoiceResponder.Default.cfgCFUNumber = value; }
        }
        public bool CFNRFlag
        {
            get { return GoogleVoiceResponder.Default.cfgCFNRFlag; }
            set { GoogleVoiceResponder.Default.cfgCFNRFlag = value; }
        }
        public string CFNRNumber
        {
            get { return GoogleVoiceResponder.Default.cfgCFNRNumber; }
            set { GoogleVoiceResponder.Default.cfgCFNRNumber = value; }
        }
        public bool DNDFlag
        {
            get { return GoogleVoiceResponder.Default.cfgDNDFlag; }
            set { GoogleVoiceResponder.Default.cfgDNDFlag = value; }
        }
        public bool AAFlag
        {
            get { return GoogleVoiceResponder.Default.cfgAAFlag; }
            set { GoogleVoiceResponder.Default.cfgAAFlag = value; }
        }

        public bool CFBFlag
        {
            get { return GoogleVoiceResponder.Default.cfgCFBFlag; }
            set { GoogleVoiceResponder.Default.cfgCFBFlag = value; }
        }

        public string CFBNumber
        {
            get { return GoogleVoiceResponder.Default.cfgCFBNumber; }
            set { GoogleVoiceResponder.Default.cfgCFBNumber = value; }
        }

        public int SIPPort
        {
            get { return GoogleVoiceResponder.Default.cfgSipPort; }
            set { GoogleVoiceResponder.Default.cfgSipPort = value; }
        }

        public bool PublishEnabled
        {
            get
            {
                SipConfigStruct.Instance.publishEnabled = GoogleVoiceResponder.Default.cfgSipPublishEnabled;
                return GoogleVoiceResponder.Default.cfgSipPublishEnabled;
            }
            set
            {
                SipConfigStruct.Instance.publishEnabled = value;
                GoogleVoiceResponder.Default.cfgSipPublishEnabled = value;
            }
        }

        public string StunServerAddress
        {
            get
            {
                SipConfigStruct.Instance.stunServer = GoogleVoiceResponder.Default.cfgStunServerAddress;
                return GoogleVoiceResponder.Default.cfgStunServerAddress;
            }
            set
            {
                GoogleVoiceResponder.Default.cfgStunServerAddress = value;
                SipConfigStruct.Instance.stunServer = value;
            }
        }

        public EDtmfMode DtmfMode
        {
            get
            {
                return (EDtmfMode)GoogleVoiceResponder.Default.cfgDtmfMode;
            }
            set
            {
                GoogleVoiceResponder.Default.cfgDtmfMode = (int)value;
            }
        }

        public int Expires
        {
            get
            {
                SipConfigStruct.Instance.expires = GoogleVoiceResponder.Default.cfgRegistrationTimeout;
                return GoogleVoiceResponder.Default.cfgRegistrationTimeout;
            }
            set
            {
                GoogleVoiceResponder.Default.cfgRegistrationTimeout = value;
                SipConfigStruct.Instance.expires = value;
            }
        }

        public int ECTail
        {
            get
            {
                SipConfigStruct.Instance.ECTail = GoogleVoiceResponder.Default.cfgECTail;
                return GoogleVoiceResponder.Default.cfgECTail;
            }
            set
            {
                GoogleVoiceResponder.Default.cfgECTail = value;
                SipConfigStruct.Instance.ECTail = value;
            }
        }

        public bool VADEnabled
        {
            get
            {
                SipConfigStruct.Instance.VADEnabled = GoogleVoiceResponder.Default.cfgVAD;
                return GoogleVoiceResponder.Default.cfgVAD;
            }
            set
            {
                GoogleVoiceResponder.Default.cfgVAD = value;
                SipConfigStruct.Instance.VADEnabled = value;
            }
        }


        public string NameServer
        {
            get
            {
                SipConfigStruct.Instance.nameServer = GoogleVoiceResponder.Default.cfgNameServer;
                return GoogleVoiceResponder.Default.cfgNameServer;
            }
            set
            {
                GoogleVoiceResponder.Default.cfgNameServer = value;
                SipConfigStruct.Instance.nameServer = value;
            }
        }

        /// <summary>
        /// The position of default account in account list. Does NOT mean same as DefaultAccountIndex
        /// </summary>
        public int DefaultAccountIndex
        {
            get
            {
                return GoogleVoiceResponder.Default.cfgSipAccountDefault;
            }
            set
            {
                GoogleVoiceResponder.Default.cfgSipAccountDefault = value;
            }
        }

        public List<IAccount> Accounts
        {
            get
            {
                List<IAccount> accList = new List<IAccount>();
                for (int i = 0; i < 5; i++)
                {
                    IAccount item = new SipekAccount(i);
                    accList.Add(item);
                }
                return accList;

            }
        }

        public void Save()
        {
            // save properties
            GoogleVoiceResponder.Default.Save();
        }

        public List<string> CodecList
        {
            get
            {
                List<string> codecList = new List<string>();
                foreach (string item in GoogleVoiceResponder.Default.cfgCodecList)
                {
                    codecList.Add(item);
                }
                return codecList;
            }
            set
            {
                GoogleVoiceResponder.Default.cfgCodecList.Clear();
                List<string> cl = value;
                foreach (string item in cl)
                {
                    GoogleVoiceResponder.Default.cfgCodecList.Add(item);
                }
            }
        }


        public string WavFile {
            get
            {
                SipConfigStruct.Instance.wavFile = GoogleVoiceResponder.Default.responseWavFile;
                return GoogleVoiceResponder.Default.responseWavFile;
            }
            set
            {
                GoogleVoiceResponder.Default.responseWavFile = value;
                SipConfigStruct.Instance.wavFile = value;
            }
        }



        public bool AutoPlayHangup { get; set; }
    }


    //////////////////////////////////////////////////////
    // Media proxy
    // internal class
    public class CMediaPlayerProxy : IMediaProxyInterface
    {
        SoundPlayer player = new SoundPlayer();

        #region Methods

        public int playTone(ETones toneId)
        {
#if false
            string fname;

            switch (toneId)
            {
                case ETones.EToneDial:
                    fname = "Sounds/dial.wav";
                    break;
                case ETones.EToneCongestion:
                    fname = "Sounds/congestion.wav";
                    break;
                case ETones.EToneRingback:
                    fname = "Sounds/ringback.wav";
                    break;
                case ETones.EToneRing:
                    fname = "Sounds/ring.wav";
                    break;
                default:
                    fname = "";
                    break;
            }

            player.SoundLocation = fname;
            player.Load();
            player.PlayLooping();
#endif
            return 1;
        }

        public int stopTone()
        {
            player.Stop();
            return 1;
        }

        #endregion

    }

    #endregion Concrete Implementations

}
