﻿using System;
using System.IO;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Serilog;
using Serilog.Events;
using Umbraco.Core.Hosting;

namespace Umbraco.Core.Logging.Serilog
{

    ///<summary>
    /// Implements <see cref="ILogger"/> on top of Serilog.
    ///</summary>
    public class SerilogLogger<T> : ILogger, IDisposable
    {
        public global::Serilog.ILogger SerilogLog { get; }

        /// <summary>
        /// Initialize a new instance of the <see cref="SerilogLogger<T>"/> class with a configuration file.
        /// </summary>
        /// <param name="logConfigFile"></param>
        public SerilogLogger(FileInfo logConfigFile)
        {
            SerilogLog = new LoggerConfiguration()
                .ReadFrom.AppSettings(filePath: logConfigFile.FullName)
                .CreateLogger();
        }

        public SerilogLogger(LoggerConfiguration logConfig)
        {
            //Configure Serilog static global logger with config passed in
            SerilogLog = logConfig.CreateLogger();
        }

        /// <summary>
        /// Creates a logger with some pre-defined configuration and remainder from config file
        /// </summary>
        /// <remarks>Used by UmbracoApplicationBase to get its logger.</remarks>
        public static SerilogLogger<T> CreateWithDefaultConfiguration(IHostingEnvironment hostingEnvironment, ILoggingConfiguration loggingConfiguration)
        {
            var loggerConfig = new LoggerConfiguration();
            loggerConfig
                .MinimalConfiguration(hostingEnvironment, loggingConfiguration)
                .ReadFromConfigFile(loggingConfiguration)
                .ReadFromUserConfigFile(loggingConfiguration);

            return new SerilogLogger<T>(loggerConfig);
        }

        /// <summary>
        /// Gets a contextualized logger.
        /// </summary>
        private global::Serilog.ILogger LoggerFor(Type reporting)
            => SerilogLog.ForContext(reporting);

        /// <summary>
        /// Maps Umbraco's log level to Serilog's.
        /// </summary>
        private LogEventLevel MapLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Fatal:
                    return LogEventLevel.Fatal;
                case LogLevel.Information:
                    return LogEventLevel.Information;
                case LogLevel.Verbose:
                    return LogEventLevel.Verbose;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
            }

            throw new NotSupportedException($"LogLevel \"{level}\" is not supported.");
        }

        /// <inheritdoc/>
        public bool IsEnabled(Type reporting, LogLevel level)
            => LoggerFor(reporting).IsEnabled(MapLevel(level));

        /// <inheritdoc/>
        public void LogCritical(string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(typeof(T)).Fatal(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogCritical(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            var logger = LoggerFor(typeof(T));            
            logger.Fatal(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogError(string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(typeof(T)).Error(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            var logger = LoggerFor(typeof(T));
            logger.Error(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogWarning(string message, params object[] propertyValues)
        {
            LoggerFor(typeof(T)).Warning(message, propertyValues);
        }

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(typeof(T)).Warning(exception, messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogInformation(string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(typeof(T)).Information(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogDebug(string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(typeof(T)).Debug(messageTemplate, propertyValues);
        }

        /// <inheritdoc/>
        public void LogTrace(string messageTemplate, params object[] propertyValues)
        {
            LoggerFor(typeof(T)).Verbose(messageTemplate, propertyValues);
        }

        public void Dispose()
        {
            SerilogLog.DisposeIfDisposable();
        }
    }
}
