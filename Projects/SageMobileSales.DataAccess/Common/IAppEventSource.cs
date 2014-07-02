using System;

namespace SageMobileSales.DataAccess.Common
{
    public interface IAppEventSource
    {
        string WriteLine(Exception e);
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Critical(string message);
    }
}