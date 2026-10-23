using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKtest.Services.SSH;

namespace MKtest.Tests;

[TestClass]
public sealed class BeelinkTimeValueParserTests
{
    [TestMethod]
    public void Timedatectl_RussianTruncatedLocalLabel_UsesBeelinkTimeNotUtc()
    {
        const string output =
            "timedatectl status\n" +
            "e: Сб 2026-10-17 20:47:51 MSK\n" +
            "           Universal time: Сб 2026-10-17 17:47:51 UTC\n" +
            "                 RTC time: Сб 2026-10-17 17:47:51";

        Assert.AreEqual("2026-10-17 20:47:51",
            BeelinkTimeValueParser.ExtractFromTimedatectl(output));
    }

    [TestMethod]
    public void Timedatectl_EnglishLocalTime_UsesLocalTimestamp()
    {
        const string output =
            "Local time: Sun 2026-10-18 18:15:44 MSK\n" +
            "Universal time: Sun 2026-10-18 15:15:44 UTC\n" +
            "RTC time: Sun 2026-10-18 15:15:44";

        Assert.AreEqual("2026-10-18 18:15:44",
            BeelinkTimeValueParser.ExtractFromTimedatectl(output));
    }

    [TestMethod]
    public void Timedatectl_UtcBeforeLocal_IgnoresUtcAndRtc()
    {
        const string output =
            "Universal time: Sat 2026-10-17 17:47:51 UTC\n" +
            "RTC time: Sat 2026-10-17 17:47:51\n" +
            "Local time: Sat 2026-10-17 20:47:51 MSK";

        Assert.AreEqual("2026-10-17 20:47:51",
            BeelinkTimeValueParser.ExtractFromTimedatectl(output));
    }

    [TestMethod]
    [DataRow("Universal time: Sat 2026-10-17 17:47:51 UTC\nRTC time: Sat 2026-10-17 17:47:51")]
    [DataRow("")]
    [DataRow("timedatectl status\nERROR: Нет подключения")]
    [DataRow("Local time: Сб 2026-02-30 20:47:51 MSK")]
    public void Timedatectl_WithoutValidLocalTime_ReturnsNull(string response)
    {
        Assert.IsNull(BeelinkTimeValueParser.ExtractFromTimedatectl(response));
    }

    [TestMethod]
    public void Timedatectl_Null_ReturnsNull()
    {
        Assert.IsNull(BeelinkTimeValueParser.ExtractFromTimedatectl(null));
    }

    [TestMethod]
    [DataRow("2026-10-18T18:20:30", "2026-10-18 18:20:30")]
    [DataRow("2026-10-18 18:20:30", "2026-10-18 18:20:30")]
    [DataRow("date '+%Y-%m-%dT%H:%M:%S'\r\n2026-10-18T18:20:30", "2026-10-18 18:20:30")]
    public void DateCommand_ValidTimestamp_FormatsLocalTime(string input, string expected)
    {
        Assert.AreEqual(expected, BeelinkTimeValueParser.Extract(input));
    }

    [TestMethod]
    [DataRow("date '+%Y-%m-%dT%H:%M:%S'")]
    [DataRow("2026-13-18T18:20:30")]
    [DataRow("2026-10-18T25:20:30")]
    [DataRow("ERROR: Нет подключения")]
    public void DateCommand_NoValidTimestamp_ReturnsNull(string input)
    {
        Assert.IsNull(BeelinkTimeValueParser.Extract(input));
    }
}
