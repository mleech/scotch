using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Scotch;

public class Request
{
    public string? Body { get; set; }
    public IDictionary<string, string>? ContentHeaders { get; set; }
    public string Method { get; set; }
    public IDictionary<string, string> RequestHeaders { get; set; }
    public string Uri { get; set; }
}

public class Status
{
    public HttpStatusCode Code { get; set; }
    public string? Message { get; set; }
}

public class Response
{
    public string? Body { get; set; }
    public IDictionary<string, string>? ContentHeaders { get; set; }

    public Version HttpVersion { get; set; }
    public IDictionary<string, string>? ResponseHeaders { get; set; }
    public Status Status { get; set; }

    public HttpResponseMessage ToHttpResponseMessage(HttpRequestMessage requestMessage)
    {
        var result = new HttpResponseMessage(Status.Code);
        result.ReasonPhrase = Status.Message;
        result.Version = HttpVersion;
        foreach (var h in ResponseHeaders ?? new Dictionary<string, string>()) result.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());

        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(Body ?? string.Empty));
        foreach (var h in ContentHeaders ?? new Dictionary<string, string>()) content.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());

        result.Content = content;
        result.RequestMessage = requestMessage;
        return result;
    }
}

public class HttpInteraction
{
    public DateTimeOffset RecordedAt { get; set; }
    public Request Request { get; set; }
    public Response Response { get; set; }
}

public static class Helpers
{
    public static bool RequestsMatch(Request receivedRequest, Request recordedRequest)
    {
        return receivedRequest.Method.Equals(recordedRequest.Method, StringComparison.OrdinalIgnoreCase)
               && receivedRequest.Uri.Equals(recordedRequest.Uri, StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<Request> ToRequestAsync(HttpRequestMessage request, List<string>? headersToHide = null)
    {
        var requestBody = await ToStringAsync(request.Content);
        return new Request
        {
            Method = request.Method.ToString(),
            Uri = request.RequestUri.ToString(),
            RequestHeaders = ToHeaders(request.Headers, headersToHide),
            ContentHeaders = ToContentHeaders(request.Content),
            Body = requestBody
        };
    }

    public static async Task<Response> ToResponseAsync(HttpResponseMessage response, List<string>? headersToHide = null)
    {
        var responseBody = await ToStringAsync(response.Content);
        return new Response
        {
            Status = new Status
            {
                Code = response.StatusCode,
                Message = response.ReasonPhrase
            },
            ResponseHeaders = ToHeaders(response.Headers, headersToHide),
            ContentHeaders = ToContentHeaders(response.Content),
            Body = responseBody,
            HttpVersion = response.Version
        };
    }

    private static IDictionary<string, string> ToContentHeaders(HttpContent? content)
    {
        return content == null ? new Dictionary<string, string>() : ToHeaders(content.Headers);
    }

    private static IDictionary<string, string> ToHeaders(HttpHeaders headers, List<string>? headersToHide = null)
    {
        headersToHide ??= new List<string>();

        IDictionary<string, string> dict = new Dictionary<string, string>();
        foreach (var h in headers) dict.Add(h.Key, headersToHide.Contains(h.Key) ? "********" : string.Join(",", h.Value));

        return dict;
    }

    private static async Task<string> ToStringAsync(HttpContent? content)
    {
        return content == null ? string.Empty : await content.ReadAsStringAsync();
    }
}
