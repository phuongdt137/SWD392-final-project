using System.Net;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace HSMS.Attendance.Checks;

// Chạy nguyên pipeline MVC trong bộ nhớ, không mở cổng mạng hoặc khởi động Program của ứng dụng thật.
internal sealed class InProcessServer : IServer
{
    public IFeatureCollection Features { get; } = new FeatureCollection();
    private Func<IFeatureCollection, Task> _process = null!;

    public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken) where TContext : notnull
    {
        _process = async features =>
        {
            var context = application.CreateContext(features);
            Exception? failure = null;
            try
            {
                await application.ProcessRequestAsync(context);
                await ((TestResponse)features.Get<IHttpResponseFeature>()!).FinishAsync();
            }
            catch (Exception error) { failure = error; throw; }
            finally { application.DisposeContext(context, failure); }
        };
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public void Dispose() { }

    public HttpMessageHandler CreateHandler() => new Handler(this);

    private sealed class Handler(InProcessServer server) : HttpMessageHandler
    {
        private readonly CookieContainer _cookies = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri!;
            var headers = new HeaderDictionary { ["Host"] = uri.Authority };
            foreach (var header in request.Headers) headers[header.Key] = header.Value.ToArray();
            if (request.Content != null)
                foreach (var header in request.Content.Headers) headers[header.Key] = header.Value.ToArray();
            headers["Cookie"] = _cookies.GetCookieHeader(uri);
            var response = new TestResponse();
            var features = new FeatureCollection();
            features.Set<IHttpRequestFeature>(new HttpRequestFeature
            {
                Protocol = "HTTP/1.1", Method = request.Method.Method, Scheme = uri.Scheme,
                Path = uri.AbsolutePath, PathBase = "", QueryString = uri.Query, RawTarget = uri.PathAndQuery,
                Headers = headers,
                Body = request.Content == null ? Stream.Null : new MemoryStream(await request.Content.ReadAsByteArrayAsync(cancellationToken))
            });
            features.Set<IHttpResponseFeature>(response);
            features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(response.Body));
            features.Set<IHttpRequestLifetimeFeature>(new HttpRequestLifetimeFeature { RequestAborted = cancellationToken });
            await server._process(features);
            var message = new HttpResponseMessage((HttpStatusCode)response.StatusCode)
            {
                Content = new ByteArrayContent(((MemoryStream)response.Body).ToArray()), RequestMessage = request
            };
            foreach (var header in response.Headers)
            {
                if (!message.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                    message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
            foreach (var cookie in response.Headers.SetCookie)
                if (cookie != null) _cookies.SetCookies(uri, cookie);
            return message;
        }
    }

    private sealed class TestResponse : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = 200;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; } = new MemoryStream();
        public bool HasStarted { get; private set; }
        private readonly Stack<(Func<object, Task> Callback, object State)> _starting = [];
        private readonly Stack<(Func<object, Task> Callback, object State)> _completed = [];
        public void OnStarting(Func<object, Task> callback, object state) => _starting.Push((callback, state));
        public void OnCompleted(Func<object, Task> callback, object state) => _completed.Push((callback, state));
        public async Task FinishAsync()
        {
            while (_starting.TryPop(out var item)) await item.Callback(item.State);
            HasStarted = true;
            while (_completed.TryPop(out var item)) await item.Callback(item.State);
        }
    }
}
