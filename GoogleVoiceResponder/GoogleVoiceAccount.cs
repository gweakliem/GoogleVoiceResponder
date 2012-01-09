using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoogleVoiceResponder
{
    public class GoogleVoiceAccount
    {
        private static GoogleVoiceAccount _instance;

        public static GoogleVoiceAccount Instance
        {
            get 
            { 
                if (_instance == null)
                {
                    _instance = new GoogleVoiceAccount();
                }
                return _instance;

            }
        }
        public string Username
        {
            get
            {
                return GoogleVoiceResponder.Default.googleVoiceUsername;
            }
            set
            {
                GoogleVoiceResponder.Default.googleVoiceUsername = value;
            }
        }

        public string Password
        {
            get
            {
                return GoogleVoiceResponder.Default.googleVoicePassword;
            }
            set
            {
                GoogleVoiceResponder.Default.googleVoicePassword = value;
            }
        }
    }
}
