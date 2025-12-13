using System.Threading.Tasks;

namespace PiNodeMonitorWinForm.Services.Sms
{
    public interface ISmsProvider
    {
        // returns true if sent successfully, or throws exception / returns false
        Task<bool> SendSmsAsync(string to, string message);
    }
}
