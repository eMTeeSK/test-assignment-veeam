namespace GZipTest.Core
{
    public interface ILogger
    {
        void ErrorLog(string msg);
        void ProgressInfo(string message);
        void Log(string v);
    }
}
