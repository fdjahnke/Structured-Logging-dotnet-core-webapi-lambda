using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StructuredLog.Logger
{
    public class LogEntry : Dictionary<string, object>
    {
        private readonly List<object> _scopeItems = new List<object>();


        /// <summary>
        /// Add a single object to the scope of this logItem.
        /// The item will be part of a list of items named scope.
        /// </summary>
        /// <param name="scopeValue">Any object that should be logged as unnamed scope.</param>
        public void AddScope(object scopeValue)
        {
            if (scopeValue == null) return;
            _scopeItems.Add(scopeValue);
            base["Scope"] = _scopeItems;
        }
    }
}
