using Yarp.ReverseProxy.Forwarder;

namespace aiExecBackend.Extensions;

public interface IRequestForwarder
{
    Task ForwardAsync(HttpContext context, string urlToForwardTo);
}

public class RequestForwarder(IHttpForwarder forwarder) : IRequestForwarder
{
    private readonly HttpMessageInvoker _httpClient = new(new SocketsHttpHandler
    {
        KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always
    });

    private readonly NoPathTransformer _transformer = new();

    public async Task ForwardAsync(HttpContext context, string urlToForwardTo)
    {
        await forwarder.SendAsync(
            context,
            urlToForwardTo,
            _httpClient,
            new ForwarderRequestConfig(),
            _transformer);
    }
}

internal class NoPathTransformer : HttpTransformer
{
    public override async ValueTask TransformRequestAsync(HttpContext httpContext,
        HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken)
    {
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);
        proxyRequest.RequestUri = new Uri(destinationPrefix);
    }
}