using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StructuredLog.Logger
{
    public class NoOpDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
