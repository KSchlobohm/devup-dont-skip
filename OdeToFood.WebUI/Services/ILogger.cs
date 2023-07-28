using System;

namespace OdeToFood.WebUI.Services
{
    internal interface ILogger
    {
        void LogInformation(string message, Exception ex = null);
        void LogError(string message, Exception ex = null);
    }
}
