using System.Runtime.InteropServices;

namespace AsteriskAddIn.Intefaces
{
    [Guid("ab634005-f13d-11d0-a459-004095e1daea"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStatusLine
    {
        void SetStatusLine(string bstrStatusLine);
        void ResetStatusLine();
    }
}
