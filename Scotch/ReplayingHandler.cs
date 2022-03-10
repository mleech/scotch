namespace Scotch;

public class ReplayingHandler : DelegatingHandler
{
    private readonly string _cassettePath;

    public ReplayingHandler(HttpMessageHandler innerHandler, string cassettePath)
    {
        InnerHandler = innerHandler;
        _cassettePath = cassettePath;
    }

    public ReplayingHandler(string cassettePath)
    {
        InnerHandler = new HttpClientHandler();
        _cassettePath = cassettePath;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var interactions = Cassette.ReadCassette(_cassettePath);
        var receivedRequest = await Helpers.ToRequestAsync(request);

        HttpInteraction matchedInteraction;

        try
        {
            matchedInteraction = interactions.First(i => Helpers.RequestsMatch(receivedRequest, i.Request));
        }
        catch (InvalidOperationException)
        {
            throw new VCRException($"No interaction found for request {receivedRequest.Method} {receivedRequest.Uri}");
        }

        var matchedResponse = matchedInteraction.Response;
        var responseMessage = matchedResponse.ToHttpResponseMessage(request);
        return responseMessage;
    }
}
