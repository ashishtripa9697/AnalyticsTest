using EmployeeAnalyticsAPI.GlobleService.StaticLogic;
using System.Text;

namespace EmployeeAnalyticsAPI.GlobleService.Middleware
{
    #region //File Logger which is responsible for the write Issue in file/Databse table
    public sealed class FileLogger
    {
        private static readonly Lazy<FileLogger> _instance = new(() => new FileLogger());
        public static FileLogger Instance => _instance.Value;

        private readonly string _logDirectory;
        private readonly object _lock= new();
        public FileLogger()   
        {
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void Log(string level,string message,Exception? ex=null,string? traceId=null)
        {
           var fileName=$"log_{DateTime.Now:yyyyMMdd}.txt";
           var filePath=Path.Combine(_logDirectory,fileName);
           var sb=new StringBuilder();
            sb.AppendLine(MessageLogic.PlusLine);
            sb.AppendLine(MessageLogic.DashedLine);
            sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Level: {level.ToUpper()}");
            if(!string.IsNullOrEmpty(traceId))
            {
                sb.AppendLine($"TraceId: {traceId}");
            }
            sb.AppendLine($"Message: {message}");
            if(ex!=null)
            {
                sb.AppendLine($"Exception : {ex.GetType().FullName}");
                sb.AppendLine($"Exception Source: {ex.Source}");
                sb.AppendLine($"Exception Message: {ex.Message}");
                sb.AppendLine("Stack Trace:");
                sb.AppendLine(ex.StackTrace ?? MessageLogic.NoStackTrace);
                if(ex.InnerException!=null)
                {
                    sb.AppendLine("Inner Exception:");
                    sb.AppendLine($"Inner Exception Type: {ex.InnerException.GetType().FullName}");
                    sb.AppendLine($"Inner Exception Message: {ex.InnerException.Message}");
                    sb.AppendLine("Inner Exception Stack Trace:");
                    sb.AppendLine(ex.InnerException.StackTrace ?? MessageLogic.NoStackTraceInnerException);
                }
               
            }
            sb.AppendLine(MessageLogic.DoubleLine);
            sb.AppendLine();
            sb.AppendLine(MessageLogic.SignleLine);
            lock (_lock)
            {
                File.AppendAllText(filePath, sb.ToString());
            }
        }
        public void Info(string msg)=> Log("INFO", msg);
        public void Warn(string msg, Exception? ex=null)=> Log("WARN", msg, ex);
        public void Error(string msg,Exception exception, string? traceId=null)=> Log("ERROR", msg, exception, traceId);
    }
    #endregion
}
