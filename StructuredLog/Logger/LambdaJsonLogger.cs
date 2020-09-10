using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StructuredLog.Logger
{
public class LambdaJsonLogger : ILogger
{

    private readonly string _categoryName;
    private readonly LambdaLoggerOptions _options;

    public IExternalScopeProvider ScopeProvider { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="categoryName">Category name for the logger.</param>
    /// <param name="options">Options for the logger.</param>
    public LambdaJsonLogger(string categoryName, LambdaLoggerOptions options)
    {
        _categoryName = categoryName;
        _options = options;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        if (!IsEnabled(logLevel))
        {
            return;
        }

        var logEntry = new LogEntry();

        if (_options.IncludeLogLevel)
        {
            logEntry.Add("LogLevel", logLevel.ToString());
        }

        CreateScopeInformation(logEntry);

        if (_options.IncludeCategory)
        {
            logEntry.Add("Category", _categoryName);
        }

        if (_options.IncludeEventId)
        {
            logEntry.Add("EventId", eventId.Id);
        }

        var text = formatter.Invoke(state, exception);
        logEntry.Add("Text", text);

        if (_options.IncludeException && exception != null)
        {
            logEntry.Add("Exception", new ErrorMessage { Error = exception?.Message, StackTrace = exception?.StackTrace });
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var json = JsonSerializer.Serialize<LogEntry>(logEntry, options);
        Amazon.Lambda.Core.LambdaLogger.Log(json);
    }

    private void CreateScopeInformation(LogEntry logInfo)
    {
        var scopeProvider = ScopeProvider;

        if (_options.IncludeScopes && scopeProvider != null)
        {
            scopeProvider.ForEachScope((scope, logItem) =>
            {
                if (scope is IEnumerable<KeyValuePair<string, object>> keyValuePairs)
                {
                    foreach (var item in keyValuePairs)
                    {
                        logItem[item.Key] = item.Value;
                    }
                }
                else if (scope is KeyValuePair<string, object> keyValue)
                {
                    logItem[keyValue.Key] = keyValue.Value;
                }
                else
                {
                    logItem.AddScope(scope);
                }
            }, (logInfo));
        }
    }

    public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? new NoOpDisposable();

    public bool IsEnabled(LogLevel logLevel)
    {
        return (_options.Filter == null || _options.Filter(_categoryName, logLevel));
    }
}
}
