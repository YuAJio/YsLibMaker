using System.Threading.Tasks;

namespace YS_BTPrint
{
    public interface IBluetoothService
    {
        Task Print(string deviceName, byte[] buffer);
    }
}