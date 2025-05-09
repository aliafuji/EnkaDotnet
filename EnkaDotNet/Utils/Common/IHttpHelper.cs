using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Utils.Common
{
    public interface IHttpHelper : IDisposable
    {
        Task<T> Get<T>(string relativeUrl, bool bypassCache = false, CancellationToken cancellationToken = default) where T : class;
        void ClearCache();
        void RemoveFromCache(string relativeUrl);
        (long CurrentEntryCount, int ExpiredCountNotAvailable) GetCacheStats();
    }
}