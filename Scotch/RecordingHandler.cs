namespace Scotch;

public class RecordingHandler : DelegatingHandler
{
    private readonly string _cassettePath;

    private readonly List<string>? _headersToHide;

    public RecordingHandler(HttpMessageHandler innerHandler, string cassettePath, List<string>? headersToHide = null)
    {
        InnerHandler = innerHandler;
        _cassettePath = cassettePath;
        _headersToHide = headersToHide;
    }

    public RecordingHandler(string cassettePath)
    {
        InnerHandler = new HttpClientHandler();
        _cassettePath = cassettePath;
        _headersToHide = new List<string>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var baseResult = base.SendAsync(request, cancellationToken);

        await Task.Run(async () =>
        {
            var response = await baseResult;
            var interactionRequest = await Helpers.ToRequestAsync(request, _headersToHide);
            var interactionResponse = await Helpers.ToResponseAsync(response, _headersToHide);
            var httpInteraction = new HttpInteraction
            {
                Request = interactionRequest,
                Response = interactionResponse,
                RecordedAt = DateTimeOffset.Now
            };
            Cassette.UpdateInteraction(_cassettePath, httpInteraction);
        });

        return baseResult.Result;
    }
}
