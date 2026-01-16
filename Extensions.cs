using System;
using System.Windows.Forms;

namespace PiNodeMonitorWinForm
{
    public static class ControlExtensions
    {
        /// <summary>
        /// 컨트롤의 InvokeRequired를 체크하여 스레드 안전하게 액션을 실행합니다.
        /// </summary>
        public static void SafeInvoke(this Control control, Action action)
        {
            if (control == null || control.IsDisposed) return;

            if (control.InvokeRequired)
            {
                try
                {
                    control.Invoke(action);
                }
                catch (ObjectDisposedException) { }
            }
            else
            {
                action();
            }
        }
    }
}
