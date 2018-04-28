using System.Diagnostics;
using System.Threading;

namespace Wibblr.Metrics.Core
{
    public struct ProfileKey
    {
        public string sessionId;
        public string name;
        public int process;
        public string thread;

        public ProfileKey(string sessionId, string name)
        {
            this.sessionId = sessionId;
            this.name = name;
            process = Process.GetCurrentProcess().Id;
            thread = Thread.CurrentThread.Name ?? Thread.CurrentThread.ManagedThreadId.ToString();
        }
    }
}
