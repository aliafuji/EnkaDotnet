using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Tests.Http
{
    /// <summary>
    /// Simple fake HttpMessageHandler that returns pre-queued responses in order.
    /// </summary>
    internal sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new Queue<HttpResponseMessage>();
        private int _callCount;

        public int CallCount => _callCount;

        public void Enqueue(HttpResponseMessage response) => _responses.Enqueue(response);

        public void Enqueue(HttpStatusCode statusCode, string content = "")
        {
            var response = new HttpResponseMessage(statusCode);
            if (!string.IsNullOrEmpty(content))
                response.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
            _responses.Enqueue(response);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _callCount);
            if (_responses.Count == 0)
                throw new System.InvalidOperationException("FakeHttpMessageHandler: No more queued responses.");
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
