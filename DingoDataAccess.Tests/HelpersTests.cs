using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Moq;
using DingoDataAccess;
using Microsoft.Extensions.Logging;

namespace DingoDataAccess.Tests
{
    public class HelpersTests
    {
        [Fact]
        public void AlwaysPasses()
        {
            Assert.True(true);
        }

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("07a47155-c7b8-4d60-a102-385f31b74eac", true)]
        [InlineData("00000000-0000-0000-0000-000000000000", true)]
        [InlineData("00000000-0000-0000-0000-000000000000'); DROP TABLE STUDENTS; --", false)]
        [InlineData("00000000-0000-0000-0000-00000000000000", false)]
        [InlineData("00000000-0000-0000-0000-00000000000", false)]
        [InlineData("------------------------------------", false)]
        [InlineData("000000000000000000000000000000000000", false)]
        [InlineData(" ", false)]
        [InlineData("                                    ", false)]
        public void FullVerifyGuidExpected(string possibleGuid, bool expectedResult)
        {
            var mock = new Mock<ILogger>();

            Assert.True(Helpers.FullVerifyGuid(ref possibleGuid, mock.Object) == expectedResult);
        }
    }
}
