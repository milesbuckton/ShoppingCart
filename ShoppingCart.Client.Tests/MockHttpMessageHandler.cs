using System.Net;
using System.Net.Http.Json;

namespace ShoppingCart.Client.Tests
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly List<(Func<HttpRequestMessage, bool> Matcher, HttpResponseMessage Response)> _responses = [];
        private readonly List<HttpRequestMessage> _requests = [];

        public IReadOnlyList<HttpRequestMessage> Requests => _requests;

        public void RespondWith(HttpStatusCode statusCode)
        {
            _responses.Add((_ => true, new HttpResponseMessage(statusCode)));
        }

        public void RespondWith<T>(HttpStatusCode statusCode, T body)
        {
            _responses.Add((_ => true, new HttpResponseMessage(statusCode)
            {
                Content = JsonContent.Create(body)
            }));
        }

        public void RespondWith(Func<HttpRequestMessage, bool> matcher, HttpStatusCode statusCode)
        {
            _responses.Add((matcher, new HttpResponseMessage(statusCode)));
        }

        public void RespondWith<T>(Func<HttpRequestMessage, bool> matcher, HttpStatusCode statusCode, T body)
        {
            _responses.Add((matcher, new HttpResponseMessage(statusCode)
            {
                Content = JsonContent.Create(body)
            }));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _requests.Add(request);

            foreach ((Func<HttpRequestMessage, bool> matcher, HttpResponseMessage response) in _responses)
            {
                if (matcher(request))
                    return Task.FromResult(response);
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        }

        public HttpClient CreateClient()
        {
            return new HttpClient(this)
            {
                BaseAddress = new Uri("http://localhost")
            };
        }
    }
}
