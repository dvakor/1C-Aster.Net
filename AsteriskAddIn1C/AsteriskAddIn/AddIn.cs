using AsteriskAddIn.Intefaces;

namespace AsteriskAddIn
{
    
    internal static class AddIn
    {   
        private static object m_AddInObject;
        private static IErrorLog m_ErrorInfo;
        private static IAsyncEvent m_AsyncEvent;
        private static IStatusLine m_StatusLine;

        public static void load(object AddInObject)
        {
            m_AddInObject = AddInObject;
            // Вызываем неявно QueryInterface
            m_ErrorInfo = (IErrorLog)AddInObject;
            m_AsyncEvent = (IAsyncEvent)AddInObject;
            m_StatusLine = (IStatusLine)AddInObject;
        }

        public static IErrorLog ErrorLog
        {
            get
            {
                return m_ErrorInfo;
            }
        }

        public static IAsyncEvent AsyncEvent
        {
            get
            {
                return m_AsyncEvent;
            }
        }

        public static IStatusLine StatusLine
        {
            get
            {
                return m_StatusLine;
            }
        }

    }
}
