using FatedTimezoneBot.Logic.Utility;
using NUnit.Framework;
using System;
using System.Globalization;

namespace FatedTimezoneBot.Logic.Tests
{
    public class TimeUtilityTests
    {
        [Test]
        public void NonTimeStringsTests()
        {
            string[] badStrings = { "", "at 5999", "13:00pm", "woodchuck", "at 50c" };
            foreach (string s in badStrings)
            {
                Assert.IsFalse(TimeUtilities.ContainsTime(s), $"{s} marked as returning a time when it shouldn't");
            }
        }

        [Test]
        public void TimeStringsTest()
        {
            string[] goodStrings = { "4:30","3:00 pm", "12pm", "12AM"};
            foreach (string s in goodStrings)
            {
                Assert.IsTrue(TimeUtilities.ContainsTime(s), $"{s} marked as not returning a time when it shouldn't");
            }
        }

        [Test]
        public void ParseTimeTest()
        {
            // Just do every possible time.
            DateTime now = DateTime.Now;
            DateTime date = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

            while (date.Day == now.Day)
            {
                string dateString = date.ToString("t");

                Assert.IsTrue(TimeUtilities.ContainsTime(dateString));
                DateTime parsedDate = TimeUtilities.GetTimeFromText(dateString, TimeZoneInfo.Local);

                Assert.AreEqual(0, DateTime.Compare(date, parsedDate), $"returned date {parsedDate.ToString("g")} did not match expected date {date.ToString("g")} from string {dateString}");

                date = date.AddMinutes(1);
            }
        }

        [Test]
        public void ParseShortTimeTest()
        {
            // Just do every possible time.
            DateTime now = DateTime.Now;
            DateTime date = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

            while (date.Day == now.Day)
            {
                foreach (string formatString in new string[] { "hhtt", "hh tt" })
                {
                    string dateString = date.ToString(formatString, CultureInfo.InvariantCulture);

                    Assert.IsTrue(TimeUtilities.ContainsTime(dateString), $"{dateString} was not flagged as a time, but should have been.");
                    DateTime parsedDate = TimeUtilities.GetTimeFromText(dateString, TimeZoneInfo.Local);

                    Assert.AreEqual(0, DateTime.Compare(date, parsedDate), $"returned date {parsedDate.ToString("g")} did not match expected date {date.ToString("g")} from string {dateString}");
                }

                date = date.AddHours(1);
            }
        }
    }
}