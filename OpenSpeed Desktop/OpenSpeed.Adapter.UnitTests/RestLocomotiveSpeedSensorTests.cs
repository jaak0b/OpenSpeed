using System.Net;
using FakeItEasy;
using OpenSpeed.Adapter;
using OpenSpeed.Core.Models;

namespace OpenSpeed.Adapter.UnitTests;

[TestFixture]
public class RestLocomotiveSpeedSensorTests
{
    private sealed class StubHttpHandler(string responseJson) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            LastRequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            });
        }
    }

    private const string BaseUrl = "http://192.168.1.42";

    [Test]
    public async Task GetStatusAsync_StatusZeroInResponse_ReturnsMeasurementStatusWaiting()
    {
        var handler = new StubHttpHandler("""{"status":0}""");
        var sut = new RestLocomotiveSpeedSensor(new HttpClient(handler));

        var result = await sut.GetStatusAsync(BaseUrl);

        Assert.That(result, Is.EqualTo(MeasurementStatus.WaitingForMeasurement));
    }

    [Test]
    public async Task GetStatusAsync_StatusOneInResponse_ReturnsMeasurementStatusMeasuring()
    {
        var handler = new StubHttpHandler("""{"status":1}""");
        var sut = new RestLocomotiveSpeedSensor(new HttpClient(handler));

        var result = await sut.GetStatusAsync(BaseUrl);

        Assert.That(result, Is.EqualTo(MeasurementStatus.Measuring));
    }

    [Test]
    public async Task TryGetResultAsync_ValidJson_DeserializesAllFields()
    {
        var handler = new StubHttpHandler("""{"id":5,"speed_kmh":12.5,"train_length_cm":28.0}""");
        var sut = new RestLocomotiveSpeedSensor(new HttpClient(handler));

        var result = await sut.TryGetResultAsync(BaseUrl);

        Assert.That(result.Id, Is.EqualTo(5));
        Assert.That(result.SpeedKmh, Is.EqualTo(12.5));
        Assert.That(result.TrainLengthCm, Is.EqualTo(28.0));
    }

    [Test]
    public async Task TryGetResultAsync_Called_RequestUriEndsWithResult()
    {
        var handler = new StubHttpHandler("""{"id":1,"speed_kmh":0,"train_length_cm":0}""");
        var sut = new RestLocomotiveSpeedSensor(new HttpClient(handler));

        await sut.TryGetResultAsync(BaseUrl);

        Assert.That(handler.LastRequestUri?.AbsolutePath, Does.EndWith("result"));
    }

    [Test]
    public async Task ResetMeasurementAsync_Called_RequestUriEndsWithReset()
    {
        var handler = new StubHttpHandler("""{"status":"reset"}""");
        var sut = new RestLocomotiveSpeedSensor(new HttpClient(handler));

        await sut.ResetMeasurementAsync(BaseUrl);

        Assert.That(handler.LastRequestUri?.AbsolutePath, Does.EndWith("reset"));
    }
}
