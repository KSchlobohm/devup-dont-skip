using System;

namespace OdeToFood.WebUI.Services
{
    public interface ILogger
    {
        void LogInformation(string message, Exception ex = null);
        void LogError(string message, Exception ex = null);
    }
}
