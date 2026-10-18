using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Configs;
using MKtest.Services.HTTPpay;
using System.Globalization;
using System.Net;

namespace MKtest.Tests;

[TestClass]
public sealed class HttpPayProtocolTests
{
    [TestMethod]
    public void BuildUrl_WithoutTimestamp_PreservesFieldsAndEncodesSpecialCharacters()
    {
        var config = NewConfig();
        config.IncludeTimestamp = false;

        string url = HTTPpayBuilder.BuildUrl(config, 37);
        var uri = new Uri(url);
        var fields = ParseQuery(uri.Query);

        Assert.AreEqual("http", uri.Scheme);
        Assert.AreEqual("192.0.2.25", uri.Host);
        Assert.AreEqual(13180, uri.Port);
        Assert.AreEqual("/payments", uri.AbsolutePath);
        Assert.AreEqual(7, fields.Count);
        Assert.AreEqual("T 1/02", fields["terminal"]);
        Assert.AreEqual("48&5", fields["route"]);
        Assert.AreEqual("2", fields["trip"]);
        Assert.AreEqual("2026-10-18 18:15:00", fields["tripDate"]);
        Assert.AreEqual("37", fields["pTotal"]);
        Assert.AreEqual("9", fields["pCurrent"]);
        Assert.IsFalse(fields.ContainsKey("timestamp"));
    }

    [TestMethod]
    public void BuildUrl_WithTimestamp_IncludesTimestampAndCurrentPaymentCount()
    {
        var config = NewConfig();
        config.IncludeTimestamp = true;

        DateTime before = DateTime.Now.AddSeconds(-2);
        string url = HTTPpayBuilder.BuildUrl(config, 100);
        DateTime after = DateTime.Now.AddSeconds(2);
        var fields = ParseQuery(new Uri(url).Query);

        Assert.AreEqual(8, fields.Count);
        Assert.AreEqual("100", fields["pTotal"]);
        Assert.AreEqual("9", fields["pCurrent"]);
        Assert.IsTrue(
            DateTime.TryParseExact(fields["timestamp"], "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime stamp));
        Assert.IsTrue(stamp >= before && stamp <= after,
            "Timestamp should reflect when the HTTP request was built.");
    }

    [TestMethod]
    [DataRow("HTTP/1.1 200 OK\r\nContent-Length: 0\r\n\r\n", true, 200, "OK - Запрос успешно обработан")]
    [DataRow("HTTP/1.1 400 Bad Request\r\n\r\n", false, 400, "400 - Ошибка в формате данных")]
    [DataRow("HTTP/1.1 404 Not Found\r\n\r\n", false, 404, "404 - Страница не найдена")]
    [DataRow("HTTP/1.1 500 Internal Server Error\r\n\r\n", false, 500, "500 - Внутренняя ошибка сервера")]
    [DataRow("HTTP/1.1 503 Service Unavailable\r\n\r\n", false, 503, "503 - Ошибка")]
    public void ResponseParser_MapsHttpStatusCorrectly(
        string raw, bool success, int statusCode, string expectedMessage)
    {
        var result = new HttpPayResponseParser().Parse(raw);
        Assert.AreEqual(success, result.Success);
        Assert.AreEqual(statusCode, result.StatusCode);
        Assert.AreEqual(expectedMessage, result.Message);
    }

    [TestMethod]
    [DataRow("garbled-response")]
    [DataRow("HTTP/1.1 NotANumber Something")]
    public void ResponseParser_RejectsInvalidStatusLine(string raw)
    {
        var result = new HttpPayResponseParser().Parse(raw);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(0, result.StatusCode);
        Assert.AreEqual("Некорректный HTTP-ответ", result.Message);
    }

    [TestMethod]
    public async Task HttpPayService_UsesUrlTransportAndParserWithoutNetwork()
    {
        var fake = new RecordingTransport
        {
            Response = "HTTP/1.1 200 OK\r\nContent-Length: 0\r\n\r\n"
        };
        using var source = new CancellationTokenSource();
        using var service = new HTTPpayService(
            NewConfig(), new HttpPayUrlBuilder(), fake, new HttpPayResponseParser());

        HTTPpayResponse result = await service.SendAsync(42, source.Token);

        Assert.AreEqual(1, fake.RequestCount);
        Assert.AreEqual(source.Token, fake.Token);
        Assert.IsNotNull(fake.LastUrl);
        var fields = ParseQuery(new Uri(fake.LastUrl).Query);
        Assert.AreEqual("42", fields["pTotal"]);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(200, result.StatusCode);
    }

    [TestMethod]
    public async Task HttpPayService_MapsTransportTimeoutToResponse()
    {
        var transport = new RecordingTransport { Timeout = true };
        using var service = new HTTPpayService(
            NewConfig(), new HttpPayUrlBuilder(), transport, new HttpPayResponseParser());

        HTTPpayResponse result = await service.SendAsync(40);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(0, result.StatusCode);
        Assert.AreEqual("Таймаут запроса", result.Message);
    }

    private static HTTPpayConfig NewConfig() => new()
    {
        IP = "192.0.2.25",
        Port = 13180,
        Terminal = "T 1/02",
        Route = "48&5",
        Trip = 2,
        TripDate = new DateTime(2026, 10, 18, 18, 15, 0),
        CurrentPayments = 9,
        IncludeTimestamp = false
    };

    private static Dictionary<string, string> ParseQuery(string query) =>
        query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(pair => pair.Split('=', 2))
            .ToDictionary(
                parts => WebUtility.UrlDecode(parts[0]),
                parts => WebUtility.UrlDecode(parts.Length > 1 ? parts[1] : ""));

    private sealed class RecordingTransport : IHttpPayTransport
    {
        public string Response { get; set; } = "";
        public bool Timeout { get; set; }
        public string? LastUrl { get; private set; }
        public CancellationToken Token { get; private set; }
        public int RequestCount { get; private set; }

        public Task<string> GetRawAsync(string url, CancellationToken token)
        {
            RequestCount++;
            LastUrl = url;
            Token = token;
            if (Timeout)
                throw new OperationCanceledException();
            return Task.FromResult(Response);
        }
    }
}
