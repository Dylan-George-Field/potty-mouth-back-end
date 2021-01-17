using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TryScanMe.Functions.Extensions;

namespace TryScanMe.Tests
{
    [TestClass]
    public class DateTimeExtensionTests
    {

        [TestMethod]
        public void UnderFiveSecondsAgo()
        {
            var date = DateTime.UtcNow;

            var result = date.ToFriendlyDate();

            Assert.AreEqual("Just now", result);
        }

        [TestMethod]
        public void FiveSecondsAgo()
        {
            var date = DateTime.UtcNow.AddSeconds(-4);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("Just now", result);
        }

        [TestMethod]
        public void FiftyNineSecondsAgo()
        {
            var date = DateTime.UtcNow.AddSeconds(-59);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("59 seconds ago", result);
        }

        [TestMethod]
        public void OneMinuteAgo()
        {
            var date = DateTime.UtcNow.AddSeconds(-60);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("1 minute ago", result);
        }

        [TestMethod]
        public void TwoMinutesAgo()
        {
            var date = DateTime.UtcNow.AddMinutes(-2);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("2 minutes ago", result);
        }

        [TestMethod]
        public void FiftyNineMinutesAgo()
        {
            var date = DateTime.UtcNow.AddMinutes(-59);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("59 minutes ago", result);
        }

        [TestMethod]
        public void OneHourAgo()
        {
            var date = DateTime.UtcNow.AddHours(-1);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("1 hour ago", result);
        }

        [TestMethod]
        public void TwoHoursAgo()
        {
            var date = DateTime.UtcNow.AddHours(-2);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("2 hours ago", result);
        }

        [TestMethod]
        public void OneDayAgo()
        {
            var date = DateTime.UtcNow.AddDays(-1);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("1 day ago", result);
        }

        [TestMethod]
        public void SixDaysAgo()
        {
            var date = DateTime.UtcNow.AddDays(-6);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("6 days ago", result);
        }

        [TestMethod]
        public void OneWeekAgo()
        {
            var date = DateTime.UtcNow.AddDays(-7);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("1 week ago", result);
        }

        [TestMethod]
        public void OneMonthAgo()
        {
            var date = DateTime.UtcNow.AddDays(-28);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("1 month ago", result);
        }

        [TestMethod]
        public void ElevenMonthsAgo()
        {
            var date = DateTime.UtcNow.AddMonths(-11);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("11 months ago", result);
        }

        [TestMethod]
        public void OneYearAgo()
        {
            var date = DateTime.UtcNow.AddMonths(-12);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("1 year ago", result);
        }

        [TestMethod]
        public void OneHundredYearAgo()
        {
            var date = DateTime.UtcNow.AddYears(-100);

            var result = date.ToFriendlyDate();

            Assert.AreEqual("100 years ago", result);
        }
    }
}
