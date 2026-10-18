using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Services;
using MKtest.Services.SSH;

namespace MKtest.Tests;

[TestClass]
public sealed class TimeCommandsServiceTests
{
    private sealed class RecordingExecutor : ISshCommandExecutor
    {
        public List<string> Commands { get; } = new();
        public string Response { get; set; } = "OK";

        public string ExecuteCommand(string command)
        {
            Commands.Add(command);
            return Response;
        }

        public string ExecuteDirectCommand(string command) =>
            throw new InvalidOperationException("TimeCommandsService must use the authenticated SSH shell");
    }

    [TestMethod]
    public void CheckTimeStatus_SendsTimedatectlStatus_AndReportsProgress()
    {
        var fake = new RecordingExecutor { Response = "Local time: Sun 2026-10-18 18:18:00 MSK" };
        var service = new TimeCommandsService(fake);
        string? message = null;
        service.OnCommandExecuted += value => message = value;

        string response = service.CheckTimeStatus();

        CollectionAssert.AreEqual(new[] { "timedatectl status" }, fake.Commands);
        Assert.AreEqual(fake.Response, response);
        Assert.AreEqual("Проверка статуса времени...", message);
    }

    [TestMethod]
    public void ReadCurrentTime_SendsExactDateFormat()
    {
        var fake = new RecordingExecutor();
        var service = new TimeCommandsService(fake);

        Assert.AreEqual("OK", service.ReadCurrentTime());
        CollectionAssert.AreEqual(new[] { "date '+%Y-%m-%dT%H:%M:%S'" }, fake.Commands);
    }

    [TestMethod]
    [DataRow(true, "timedatectl set-ntp true", "Включение NTP синхронизации...")]
    [DataRow(false, "timedatectl set-ntp false", "Отключение NTP синхронизации...")]
    public void SetNtp_UsesExactCommand(bool enabled, string expectedCommand, string expectedMessage)
    {
        var fake = new RecordingExecutor();
        var service = new TimeCommandsService(fake);
        string? message = null;
        service.OnCommandExecuted += value => message = value;

        string result = enabled ? service.EnableNTP() : service.DisableNTP();

        Assert.AreEqual("OK", result);
        CollectionAssert.AreEqual(new[] { expectedCommand }, fake.Commands);
        Assert.AreEqual(expectedMessage, message);
    }

    [TestMethod]
    public void SetManualDateTime_SendsExactLinuxDateCommand()
    {
        var fake = new RecordingExecutor();
        var service = new TimeCommandsService(fake);
        string? message = null;
        service.OnCommandExecuted += value => message = value;

        string result = service.SetManualDateTime("2026-10-18 18:45:00");

        Assert.AreEqual("OK", result);
        CollectionAssert.AreEqual(
            new[] { "date -s \"2026-10-18 18:45:00\"" }, fake.Commands);
        Assert.AreEqual("Установка времени: 2026-10-18 18:45:00", message);
    }
}
