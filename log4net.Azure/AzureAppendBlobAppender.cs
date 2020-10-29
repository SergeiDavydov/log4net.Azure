using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using log4net.Appender.Constants;
using log4net.Appender.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using log4net.Appender.Language;
using log4net.Appender.Utility;
using log4net.Core;

namespace log4net.Appender
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureAppendBlobAppender" /> class.
    /// </summary>
    /// <remarks>
    /// The instance of the <see cref="AzureAppendBlobAppender" /> class is set up to append
    /// results to an existing blob file
    /// </remarks>
    public class AzureAppendBlobAppender : BufferingAppenderSkeleton
    {
        private CloudStorageAccount _account;
        private CloudBlobClient _client;
        private CloudBlobContainer _cloudBlobContainer;

        public string ConnectionStringName { get; set; }
        private string _connectionString;
        private string _lineFeed = string.Empty;

        public string ConnectionString
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ConnectionStringName))
                {
                    return ConnectionStringUtilities.GetConnectionString(ConnectionStringName);
                }
                if (string.IsNullOrEmpty(_connectionString))
                    throw new ApplicationException(Resources.AzureConnectionStringNotSpecified);
                return _connectionString;
            }
            set => _connectionString = value;
        }

        private string _containerName;

        public string ContainerName
        {
            get
            {
                if (string.IsNullOrEmpty(_containerName))
                    throw new ApplicationException(Resources.ContainerNameNotSpecified);
                return _containerName;
            }
            set => _containerName = value;
        }

        private string _directoryName;

        public string DirectoryName
        {
            get
            {
                if (string.IsNullOrEmpty(_directoryName))
                    throw new ApplicationException(Resources.DirectoryNameNotSpecified);
                return _directoryName;
            }
            set => _directoryName = value;
        }

        private string _outputFormat;

        public string OutputFormat
        {
            get => _outputFormat is null ? Format.Xml.ToLowerInvariant() : _outputFormat.ToLowerInvariant();
            set => _outputFormat = value;
        }

        /// <summary>
        /// Sends the events.
        /// </summary>
        /// <param name="events">The events that need to be send.</param>
        /// <remarks>
        /// <para>
        /// The subclass must override this method to process the buffered events.
        /// </para>
        /// </remarks>
        protected override void SendBuffer(LoggingEvent[] events)
        {
            var appendBlob = _cloudBlobContainer.GetAppendBlobReference(Filename(_directoryName, _outputFormat));
            if (!appendBlob.Exists()) appendBlob.CreateOrReplace();
            else _lineFeed = Environment.NewLine;

            Parallel.ForEach(events, ProcessEvent);
        }

        private void ProcessEvent(LoggingEvent loggingEvent)
        {
            var appendBlob = _cloudBlobContainer.GetAppendBlobReference(Filename(_directoryName, _outputFormat));
            var output = string.Empty;

            if (OutputFormat.Equals(Format.Xml))
            {
                output = $"{_lineFeed}{loggingEvent.GetXmlString(Layout)}";
            }
            else if(OutputFormat.Equals(Format.Json))
            {
                output = $"{loggingEvent.GetJsonString()}";
            }
            else if (OutputFormat.Equals(Format.String))
            {
                output = $"{loggingEvent.GetString()}";
            }

            if (string.IsNullOrEmpty(output)) return;
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(output)))
            {
                appendBlob.AppendBlock(ms);
            }
        }

        private static string Filename(string directoryName, string fileFormat)
        {
            return $"{directoryName}/{DateTime.Today.ToString("yyyy_MM_dd", DateTimeFormatInfo.InvariantInfo)}.entry.log.{fileFormat}";
        }

        /// <summary>
        /// Initialize the appender based on the options set
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is part of the <see cref="T:log4net.Core.IOptionHandler"/> delayed object
        ///             activation scheme. The <see cref="M:log4net.Appender.BufferingAppenderSkeleton.ActivateOptions"/> method must 
        ///             be called on this object after the configuration properties have
        ///             been set. Until <see cref="M:log4net.Appender.BufferingAppenderSkeleton.ActivateOptions"/> is called this
        ///             object is in an undefined state and must not be used. 
        /// </para>
        /// <para>
        /// If any of the configuration properties are modified then 
        ///             <see cref="M:log4net.Appender.BufferingAppenderSkeleton.ActivateOptions"/> must be called again.
        /// </para>
        /// </remarks>
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            _account = CloudStorageAccount.Parse(ConnectionString);
            _client = _account.CreateCloudBlobClient();
            _cloudBlobContainer = _client.GetContainerReference(ContainerName.ToLower());
            _cloudBlobContainer.CreateIfNotExists();
        }
    }
}
