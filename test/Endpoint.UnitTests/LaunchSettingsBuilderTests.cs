using Endpoint.Application.Builders;
using Xunit;
using static Endpoint.Application.Builders.BuilderFactory;

namespace Endpoint.UnitTests
{
    public class LaunchSettingsBuilderTests
    {
        [Fact]
        public async void Constructor()
        {
            Setup();

            var sut = CreateLaunchSettingsBuilder();
        }

        [Fact]
        public async void Build()
        {
            Setup();

            var sut = CreateLaunchSettingsBuilder();

            sut.SetDirectory(@"C:\Projects\ContactService\src\ContactService.Api\Properties")
                .WithLaunchUrl("")
                .Build();
        }

        private void Setup()
        {

        }

        private static LaunchSettingsBuilder CreateLaunchSettingsBuilder()
            => Create((c, tp, tl, f) => new LaunchSettingsBuilder(c, tp, tl, f));
    }
}
