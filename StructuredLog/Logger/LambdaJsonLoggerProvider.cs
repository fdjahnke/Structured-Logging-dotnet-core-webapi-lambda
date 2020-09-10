using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StructuredLog.Logger
{
    public class LambdaJsonLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        // Private fields
        private readonly LambdaLoggerOptions _options;
        private IExternalScopeProvider _scopeProvider;
        private readonly ConcurrentDictionary<string, LambdaJsonLogger> _loggers;

        // Constants
        private const string DEFAULT_CATEGORY_NAME = "Default";

        /// <summary>
        /// Creates the provider
        /// </summary>
        /// <param name="options"></param>
        public LambdaJsonLoggerProvider(LambdaLoggerOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
            _loggers = new ConcurrentDictionary<string, LambdaJsonLogger>();
            _scopeProvider = options.IncludeScopes ? new LoggerExternalScopeProvider() : NullExternalScopeProvider.Instance;
        }

        /// <summary>
        /// Creates the logger with the specified category.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            var name = string.IsNullOrEmpty(categoryName) ? DEFAULT_CATEGORY_NAME : categoryName;

            return _loggers.GetOrAdd(name, loggerName =>
            {
                return new LambdaJsonLogger(name, _options)
                {
                    ScopeProvider = _scopeProvider
                };
            });
        }

        /// <inheritdoc />
        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;

            foreach (var logger in _loggers)
            {
                logger.Value.ScopeProvider = _scopeProvider;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }
    }
}
