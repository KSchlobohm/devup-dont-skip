using System;
using System.Diagnostics;

namespace OdeToFood.WebUI.Services
{
    public class DiagnosticsLogger : ILogger
    {
        public void LogInformation(string message, Exception ex = null)
        {
            Debug.WriteLine(message);
            Trace.TraceInformation(message);
            if (ex != null)
            {
                Debug.WriteLine(ex);
                Trace.TraceInformation(ex.ToString());
            }
        }

        public void LogError(string message, Exception ex = null)
        {
            Trace.TraceError(message);
            if (ex != null)
            {
                Trace.TraceError(ex.ToString());
            }
        }
    }
}