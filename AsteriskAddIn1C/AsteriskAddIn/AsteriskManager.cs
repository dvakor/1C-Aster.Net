using System;
using System.Threading.Tasks;
using AsterNET.Manager;
using AsterNET.Manager.Event;
using System.Text;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AsterNET.Manager.Action;
using System.Threading;

namespace AsteriskAddIn
{
    public class AsteriskAddInManager
    {
        #region Private fields

        private ManagerConnection _connection;
        private string _login;
        private string _password;
        private string _address;
        private int _port;
        private List<string> _registeredEvents;
        private string _filter;
        private CONVERSION_METHOD _method;

        #endregion

        #region Constructor

        public AsteriskAddInManager()
        {
            Login = "";
            Password = "";
            Address = "";
            Port = 5038;
            RegisteredEvents = new List<string>();
            Filter = "";
            Method = CONVERSION_METHOD.JSON;
        }

        #endregion

        #region AsteriskEventHandler

        public delegate void AsteriskEventHandler(object sender, AsteriskEventArgs e);

        public event AsteriskEventHandler AsteriskEvent;

        #endregion

        #region Properties
        public string Login
        {
            get
            {
                return _login;
            }

            set
            {
                _login = value;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                _password = value;
            }
        }

        public string Address
        {
            get
            {
                return _address;
            }

            set
            {
                _address = value;
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }

            set
            {
                _port = value;
            }
        }

        public List<string> RegisteredEvents
        {
            get
            {
                return _registeredEvents;
            }

            set
            {
                _registeredEvents = value;
            }
        }

        public string Filter
        {
            get
            {
                return _filter;
            }

            set
            {
                _filter = value;
            }
        }

        public bool isConnected
        {
            get
            {
                return _connection.IsConnected();
            }
        }

        private CONVERSION_METHOD Method
        {
            get
            {
                return _method;
            }

            set
            {
                _method = value;
            }
        }
        #endregion

        #region Connection
        public bool Connect(string s_login, string s_password, string s_address, int i_port = 0)
        {
            Login = s_login;
            Password = s_password;
            Address = s_address;
            if (i_port > 0)
            {
                Port = i_port;
            }

            Thread th = new Thread(delegate ()
            {
                Connect();
            });

            th.Start();

            return true;
            //return Connect();
        }

        public bool Connect()
        {
            if (String.IsNullOrEmpty(Login) || String.IsNullOrEmpty(Address))
            {
                throw new ArgumentException("Не верны данные для подключения, проверте логин, пароль, адрес и порт!");
            }

            if (_connection != null)
            {
                if (_connection.IsConnected())
                {
                    _connection.Logoff();
                }
            }
            else
            {
                _connection = new ManagerConnection();
                _connection.FireAllEvents = true;
                _connection.UnhandledEvent += _connection_UnhandledEvent;
                //_connection.Dial += _connection_Event_Dial; ;
                //_connection.Bridge += _connection_Event_Bridge;
                //_connection.Hangup += _connection_Event_Hangup;

            }

            _connection.Username = Login;
            _connection.Password = Password;
            _connection.Hostname = Address;
            _connection.Port = Port;

            _connection.Login();

            return _connection.IsConnected();
        }

        public void Disconnect()
        {
            if (_connection.IsConnected())
            {
                _connection.Logoff();
            }
        }
        #endregion

        #region Events

        private void _connection_UnhandledEvent(object sender, ManagerEvent e)
        {
            string name = e.GetType().Name;
            if (AsteriskEvent != null && (RegisteredEvents == null || RegisteredEvents.Count == 0 || RegisteredEvents.Contains(name)))
            {
                if (!String.IsNullOrEmpty(Filter))
                {
                    Regex regex = new Regex(Filter);
                    if (regex.IsMatch(e.ToString()))
                        AsteriskEvent(this, new AsteriskEventArgs(name, convert(e)));
                }
                else
                {
                    AsteriskEvent(this, new AsteriskEventArgs(name, convert(e)));
                }
            }
        }

        #region Event conversion 
        private string convert(object input)
        {
            string result;

            switch (Method)
            {
                case CONVERSION_METHOD.JSON:
                    result = objectToJSON(input);
                    break;
                case CONVERSION_METHOD.XML:
                    result = objectToXML(input);
                    break;
                default:
                    result = "";
                    break;
            }

            return result;
        }

        private string objectToJSON(object input)
        {
            string json = new JavaScriptSerializer().Serialize(input);

            return json;
        }

        private string objectToXML(object input)
        {
            return "";
        }

        enum CONVERSION_METHOD
        {
            XML,
            JSON
        }
        #endregion

        #endregion

        #region Methods
        public void Originate(string channel, string number)
        {
            try
            {
                _connection.SendAction(new OriginateAction()
                {
                    Channel = String.Format("SIP/{0}", channel),
                    Exten = number,
                    Context = "from-internal",
                    Priority = "1",
                    CallerId = channel,
                    Timeout = 3000000
                });
                AsteriskEvent(this, new AsteriskEventArgs("calling", "OK"));
            }
            catch (Exception e)
            {
                AsteriskEvent(this, new AsteriskEventArgs("calling", e.StackTrace));
            }
            
            
        }
        #endregion

    }

    public class AsteriskEventArgs
    {
        private string data;
        private string name;

        public AsteriskEventArgs(string name, string data)
        {
            Name = name;
            Data = data;
        }

        public string Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }
    }
}
