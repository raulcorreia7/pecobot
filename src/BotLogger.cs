
using System;

namespace MightyPecoBot
{

    public class BotLogger
    {

        public enum LOG_LEVEL
        {
            MESSAGES_ONLY,
            NO_LOG,
            DEBUG,
            ERROR
        }

        //FIXME: VALIDATE USER INPUT
        public static LOG_LEVEL LogLevel = LOG_LEVEL.MESSAGES_ONLY;

        public static void LogMessage(string message)
        {
            PrintLog(message, LOG_LEVEL.MESSAGES_ONLY);

        }

        public static void LogDebug(string message)
        {
            PrintLog(message, LOG_LEVEL.DEBUG);
        }

        public static void LogError(string message)
        {
            PrintLog(message, LOG_LEVEL.ERROR);
        }

        public static void PrintLog(string message, LOG_LEVEL level)
        {   //[HH:MM:SS] - Level - Message
            if (LogLevel == LOG_LEVEL.NO_LOG) return;
            if (LogLevel >= level || level == LOG_LEVEL.ERROR)
            {

                DateTime now = DateTime.Now;
                string time = now.ToString("HH:mm:ss");
                string log_name = LogLevelString(level);
                Console.WriteLine($"[{time}] [{log_name}] {message}");
            }
        }

        public static string LogLevelString(LOG_LEVEL level)
        {
            switch (level)
            {
                case LOG_LEVEL.NO_LOG:
                    return "NO LOG";
                case LOG_LEVEL.MESSAGES_ONLY:
                    return "MESSAGE";
                case LOG_LEVEL.DEBUG:
                    return "DEBUG";
                case LOG_LEVEL.ERROR:
                    return "ERROR";
                default:
                    return "UNKNOWN";
            }
        }
    }
}