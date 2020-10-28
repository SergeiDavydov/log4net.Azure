using System;
using System.Globalization;
using System.Xml.Linq;
using log4net.Core;
using log4net.Layout;
using System.IO;
using Newtonsoft.Json;

namespace log4net.Appender.Extensions
{
    internal static class LoggingEventExtensions
    {
        internal static string GetXmlString(this LoggingEvent loggingEvent, ILayout layout = null)
        {
            var message = loggingEvent.RenderedMessage + Environment.NewLine + loggingEvent.GetExceptionString();
            if (layout != null)
            {
                using (var w = new StringWriter())
                {
                    layout.Format(w, loggingEvent);
                    message = w.ToString();
                }
            }

            var logXml = new XElement(
                "LogEntry",
                new XElement("UserName", loggingEvent.UserName),
                new XElement("TimeStamp",
                    loggingEvent.TimeStamp.ToString(CultureInfo.InvariantCulture)),
                new XElement("ThreadName", loggingEvent.ThreadName),
                new XElement("LoggerName", loggingEvent.LoggerName),
                new XElement("Level", loggingEvent.Level),
                new XElement("Identity", loggingEvent.Identity),
                new XElement("Domain", loggingEvent.Domain),
                new XElement("CreatedOn", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                new XElement("RenderedMessage", message),
                new XElement("Location", loggingEvent.LocationInformation.FullInfo)
                );

            if (loggingEvent.Properties != null && loggingEvent.Properties.Count > 0)
            {
                var props = loggingEvent.Properties;
                if (props.Contains("AddPropertiesToXml"))
                {
                    foreach (var k in props.GetKeys())
                    {
                        var key = k.Replace(":", "_")
                                   .Replace("@", "_")
                                   .Replace(".", "_");
                        logXml.Add(new XElement(key, props[k].ToString()));
                    }
                }
            }

            if (loggingEvent.ExceptionObject != null)
            {
                logXml.Add(new XElement("Exception", loggingEvent.ExceptionObject.ToString()));
            }

            return logXml.ToString();
        }

        internal static string GetJsonString(this LoggingEvent loggingEvent)
        {
            var exception = loggingEvent.GetExceptionString();
            var message = loggingEvent.RenderedMessage;

            if (!string.IsNullOrEmpty(exception))
            {
                message += $" | Exception: {exception}";
                message += $" | Location: {loggingEvent.LocationInformation.FullInfo}";
            }

            var logJson = new
            {
                date = loggingEvent.TimeStamp.ToString(CultureInfo.InvariantCulture),
                level = loggingEvent.Level.DisplayName,
                appname = loggingEvent.Domain,
                logger = loggingEvent.LoggerName,
                thread = loggingEvent.ThreadName,
                message
            };

            return JsonConvert.SerializeObject(logJson) + Environment.NewLine;
        }

        internal static string GetString(this LoggingEvent loggingEvent)
        {
            var exception = loggingEvent.GetExceptionString();
            var message = loggingEvent.RenderedMessage;

            if (!string.IsNullOrEmpty(exception))
            {
                message += $" | Exception: {exception}";
                message += $" | Location: {loggingEvent.LocationInformation.FullInfo}";
            }

            var logString = $"Time : {loggingEvent.TimeStamp.ToString(CultureInfo.InvariantCulture)} | " +
                            $"Level : {loggingEvent.Level.DisplayName} | " +
                            $"AppName : {loggingEvent.Domain} | " +
                            $"Logger : {loggingEvent.LoggerName} | " +
                            $"Thread : {loggingEvent.ThreadName} | " +
                            $"Message : {message}";

            return logString + Environment.NewLine;
        }

        internal static string MakeRowKey(this LoggingEvent loggingEvent)
        {
            return $"{DateTime.MaxValue.Ticks - loggingEvent.TimeStamp.Ticks:D19}.{Guid.NewGuid().ToString().ToLower()}";
        }

        internal static string MakePartitionKey(this LoggingEvent loggingEvent, PartitionKeyTypeEnum partitionKeyType)
        {
            switch (partitionKeyType)
            {
                case PartitionKeyTypeEnum.LoggerName:
                    return loggingEvent.LoggerName;
                case PartitionKeyTypeEnum.DateReverse:
                    // subtract from DateMaxValue the Tick Count of the current hour
                    // so a Table Storage Partition spans an hour
                    return $"{(DateTime.MaxValue.Ticks - loggingEvent.TimeStamp.Date.AddHours(loggingEvent.TimeStamp.Hour).Ticks + 1):D19}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(partitionKeyType), partitionKeyType, null);
            }
        }
    }
}
