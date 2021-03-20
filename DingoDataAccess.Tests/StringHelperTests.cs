using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DingoDataAccess.Tests
{
    public class StringHelperTests
    {
        [Theory]
        [InlineData("ThatAlpaca#1234", true, "ThatAlpaca", 1234)]
        [InlineData("ThatAlpa#ca#1234", true, "ThatAlpa#ca", 1234)]
        [InlineData("############1234", true, "###########", 1234)]
        [InlineData("#aaaaaaaaaa#1234", true, "#aaaaaaaaaa", 1234)]
        [InlineData("", false, "", 1234)]
        [InlineData(null, false, "", 1234)]
        [InlineData("123435#123", false, "", 1234)]
        [InlineData("1234567", false, "", 1234)]
        [InlineData("#123456", false, "", 1234)]
        [InlineData("1", false, "", 1234)]
        [InlineData("12345", false, "", 1234)]
        public void SeperateUniqueIdWorks(string displayname, bool shouldParse, string expectedName, short expectedId)
        {

            bool parses = Helpers.TrySeperateDisplayName(ref displayname, out var result);

            Assert.Equal(shouldParse, parses);

            if (shouldParse)
            {
                Assert.Equal(expectedName, result.DisplayName);
                Assert.Equal(expectedId, result.UniqueIdentifier);
            }
        }
    }
}
