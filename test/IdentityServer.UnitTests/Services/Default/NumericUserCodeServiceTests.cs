using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Services;
using Xunit;

namespace IdentityServer.UnitTests.Services.Default
{
    public class NumericUserCodeServiceTests
    {
        [Fact]
        public async Task GenerateAsync_should_have_minimal_duplicates()
        {
            const int count = 500000;
            var taks = new List<Task<string>>(count);
            var sut = new NumericUserCodeService();

            for (var i = 0; i < count; i++) taks.Add(sut.GenerateAsync());

            var codes = await Task.WhenAll(taks);

            // ~ 1 duplicate per 25000 is acceptable
            var duplicates = codes.GroupBy(x => x).Where(x => 2 < x.Count()).Select(x => x.Key).ToList();
            duplicates.Should().BeEmpty();

        }
    }
}