using AsteriskAddIn.Intefaces;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AsteriskAddIn
{
    [ComVisible(true), Guid("29C03F10-7459-4368-BD9D-CA26B386C142"), ProgId("AddIn.Asterisk")]
    public class AsteriskComponent : IInitDone, ILanguageExtender
    {
        private const string addInName = "Asterisk";
        private AsteriskAddInManager manager = new AsteriskAddInManager();
        private StringBuilder _lastError;

        #region Constructor
        public AsteriskComponent() {}

        #endregion

        #region Properties

        public StringBuilder LastError
        {
            get
            {
                return _lastError;
            }

            set
            {
                _lastError = value;
            }
        }

        #endregion

        #region IInitDone implementation
        public void Init([MarshalAs(UnmanagedType.IDispatch)] object pConnection)
        {
            AddIn.load(pConnection);
        }

        public void Done() {}

        public void setMemManager(ref object mem) {}


        public void GetInfo(ref object pInfo)
        {
            ((System.Array)pInfo).SetValue("2000", 0);
        }
        #endregion

        #region ILanguageExtender implementation

        public void RegisterExtensionAs(ref string bstrExtensionName)
        {
            bstrExtensionName = addInName;
        }

        #region Methods
        enum Methods
        {   
            //Числовые идентификаторы методов (процедур или функций) нашей внешней компоненты
            Connect,
            Disconnect,
            RegisterEvent,
            SetFilter,
            Call,
            isConnected,    
            LastMethod
        }

        public void CallAsFunc(int lMethodNum, ref object pvarRetValue, [MarshalAs(UnmanagedType.SafeArray)] ref Array paParam)
        {
            //Здесь внешняя компонента выполняет код Функций
            Methods method = (Methods)lMethodNum;
            switch (method)
            {
                case Methods.Connect:
                    pvarRetValue = _Connect();
                    break;
                case Methods.RegisterEvent:
                    pvarRetValue = _RegisterEvent(_getSafeParam(paParam,0).ToString());
                    break;
                case Methods.SetFilter:
                    pvarRetValue = _SetFilter(_getSafeParam(paParam, 0).ToString());
                    break;
                case Methods.isConnected:
                    pvarRetValue = _isConnected();
                    break;
                default:
                    break;
            }
        }
        
        public void CallAsProc(int lMethodNum, [MarshalAs(UnmanagedType.SafeArray)] ref Array paParams)
        {
            //Здесь внешняя компонента выполняет код процедур
            Methods method = (Methods)lMethodNum;
            switch (method)
            {
                case Methods.Disconnect:
                    _Disconnect();
                    break;
                case Methods.Call:
                    _Call(_getSafeParam(paParams, 0).ToString(), _getSafeParam(paParams, 1).ToString());
                    break;
                default:
                    break;
            }
        }

        public void GetNMethods(ref int plMethods)
        {
            //Здесь 1С получает количество доступных из ВК методов
            plMethods = (int)Methods.LastMethod;
        }

        public void FindMethod(string bstrMethodName, ref int plMethodNum)
        {
            //Здесь 1С получает числовой идентификатор метода (процедуры или функции) по имени (названию) процедуры или функции
            string name = bstrMethodName.ToLower();
            //todo getMethodByName
            switch (name)
            {
                case "подключиться":
                    plMethodNum = (int)Methods.Connect;
                    break;
                case "отключиться":
                    plMethodNum = (int)Methods.Disconnect;
                    break;
                case "установитьфильтрнасобытия":
                    plMethodNum = (int)Methods.RegisterEvent;
                    break;
                case "установитьфильтрнасодержание":
                    plMethodNum = (int)Methods.SetFilter;
                    break;
                case "позвонить":
                    plMethodNum = (int)Methods.Call;
                    break;
                case "подключен":
                    plMethodNum = (int)Methods.isConnected;
                    break;
                default:
                    plMethodNum = -1;
                    break;
            }
        }

        public void GetMethodName(int lMethodNum, int lMethodAlias, ref string pbstrMethodName)
        {
            //Здесь 1С (теоретически) получает имя метода по его идентификатору. lMethodAlias - номер синонима.
            
            Methods method = (Methods)lMethodNum;
            switch (method)
            {
                case Methods.Connect:
                    pbstrMethodName = "Подключиться";
                    break;
                case Methods.Disconnect:
                    pbstrMethodName = "Отключиться";
                    break;
                case Methods.RegisterEvent:
                    pbstrMethodName = "УстановитьФильтрНаСобытия";
                    break;
                case Methods.SetFilter:
                    pbstrMethodName = "УстановитьФильтрНаСодержание";
                    break;
                case Methods.Call:
                    pbstrMethodName = "Позвонить";
                    break;
                case Methods.isConnected:
                    pbstrMethodName = "Подключен";
                    break;
                default:
                    pbstrMethodName = "";
                    break;
            }
        }

        public void GetNParams(int lMethodNum, ref int plParams)
        {
            //Здесь 1С получает количество параметров у метода (процедуры или функции)
            Methods method = (Methods)lMethodNum;
            switch (method)
            {
                case Methods.RegisterEvent:
                    plParams = 1;
                    break;
                case Methods.SetFilter:
                    plParams = 1;
                    break;
                case Methods.Call:
                    plParams = 2;
                    break;
                default:
                    plParams = 0;
                    break;
            }
        }

        public void GetParamDefValue(int lMethodNum, int lParamNum, ref object pvarParamDefValue)
        {
            //Здесь 1С получает значения параметров процедуры или функции по умолчанию
            pvarParamDefValue = null; //Нет значений по умолчанию
        }

        public void HasRetVal(int lMethodNum, ref bool pboolRetValue)
        {
            Methods method = (Methods)lMethodNum;
            switch (method)
            {
                case Methods.Disconnect:
                    pboolRetValue = false;
                    break;
                case Methods.Call:
                    pboolRetValue = false;
                    break;
                default:
                    pboolRetValue = true;
                    break;
            }
            //Здесь 1С узнает, возвращает ли метод значение (т.е. является процедурой или функцией)
        }
        #endregion

        #region Properties

        enum Props
        {   
            //Числовые идентификаторы свойств нашей внешней компоненты
            Login,
            Password,
            Address,
            Port,
            LastProp
        }

        public void FindProp(string bstrPropName, ref int plPropNum)
        {
            //Здесь 1С ищет числовой идентификатор свойства по его текстовому имени
            string name = bstrPropName.ToLower();
            //todo getPropByName
            switch (name)
            {
                case "логин":
                    plPropNum = (int)Props.Login;
                    break;
                case "пароль":
                    plPropNum = (int)Props.Password; 
                    break;
                case "адрес":
                    plPropNum = (int)Props.Address; 
                    break;
                case "порт":
                    plPropNum = (int)Props.Port;
                    break;
                default:
                    plPropNum = -1;
                    break;
            }

        }

        public void GetNProps(ref int plProps)
        {

            //Здесь 1С получает количество доступных из ВК свойств
            plProps = (int)Props.LastProp;
        }

        public void GetPropName(int lPropNum, int lPropAlias, ref string pbstrPropName)
        {

            //Здесь 1С узнает имя свойства по его идентификатору. lPropAlias - номер псевдонима

            Props prop = (Props)lPropNum;

            switch (prop)
            {
                case Props.Login:
                    pbstrPropName = "Логин";
                    break;
                case Props.Password:
                    pbstrPropName = "Пароль";
                    break;
                case Props.Address:
                    pbstrPropName = "Адрес";
                    break;
                case Props.Port:
                    pbstrPropName = "Порт";
                    break;
                default:
                    pbstrPropName = "";
                    break;
            }

        }

        public void GetPropVal(int lPropNum, ref object pvarPropVal)
        {
            //Здесь 1С узнает значения свойств 
            Props prop = (Props)lPropNum;
            switch (prop)
            {
                case Props.Login:
                    pvarPropVal = manager.Login;
                    break;
                case Props.Password:
                    pvarPropVal = manager.Password;
                    break;
                case Props.Address:
                    pvarPropVal = manager.Address;
                    break;
                case Props.Port:
                    pvarPropVal = manager.Port;
                    break;
                default:
                    pvarPropVal = null;
                    break;
            }
        }

        public void SetPropVal(int lPropNum, ref object varPropVal)
        {
            //Здесь 1С изменяет значения свойств
            Props prop = (Props)lPropNum;
            switch (prop)
            {
                case Props.Login:
                    manager.Login = (string)varPropVal;
                    break;
                case Props.Password:
                    manager.Password = (string)varPropVal;
                    break;
                case Props.Address:
                    manager.Address = (string)varPropVal;
                    break;
                case Props.Port:
                    manager.Port = (int)varPropVal;
                    break;
                default:
                    break;
            }
        }

        public void IsPropReadable(int lPropNum, ref bool pboolPropRead)
        {
            //Здесь 1С узнает, какие свойства доступны для чтения
            pboolPropRead = true; // Все свойства доступны для чтения
        }

        public void IsPropWritable(int lPropNum, ref bool pboolPropWrite)
        {
            //Здесь 1С узнает, какие свойства доступны для записи
            pboolPropWrite = true; // Все свойства доступны для записи
        }

        #endregion

        #endregion

        #region Methods and events helpers
        private void _Call(string internalNumber, string number)
        {
            manager.Originate(internalNumber, number);
        }

        private bool _isConnected() {
            return manager.isConnected;
        }

        private bool _Connect()
        {
            bool isConnected = false;
            try
            {
                if (manager != null)
                {
                    manager.AsteriskEvent += _AsteriskEvent;
                    isConnected = manager.Connect();
                }
            }
            catch (Exception e)
            {
                _SetLastError(e);
            }
            
                
            return isConnected;
        }

        private void _Disconnect()
        {
            try
            {
                if (manager != null)
                    manager.Disconnect();
            }
            catch (Exception e)
            {

                _SetLastError(e);
            } 
        }

        private bool _RegisterEvent(string events)
        {
            bool result = true;

            try
            {
                if (manager != null)
                {
                    manager.RegisteredEvents.Clear();
                    foreach (string item in events.Split(new Char[] { '|' }))
                    {
                        if (!manager.RegisteredEvents.Contains(item))
                            manager.RegisteredEvents.Add(item);
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception e)
            {

                _SetLastError(e);
                result = false;
            }

            return result;
        }

        private bool _SetFilter(string pattern)
        {   
            bool result = true;

            try
            {
                if(manager != null)
                {
                    manager.Filter = pattern;
                }
            }
            catch (Exception e)
            {
                _SetLastError(e);
                result = false;
            }

            return result;
        }

        private void _AsteriskEvent(object sender, AsteriskEventArgs e)
        {
            AddIn.AsyncEvent.ExternalEvent("Asterisk.AddIn", e.Name, e.Data);
        }

        private void _SetLastError(Exception e)
        {
            LastError.Clear();
            LastError.Append(e.Message);
        }

        private object _getSafeParam(Array args, int index)
        {
            if (args.Length > 0)
            {
                return args.GetValue(index);
            }
            else
                return null;
        }

        #endregion
    }
}
