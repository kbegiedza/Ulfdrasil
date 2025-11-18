using AwesomeAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ulfdrasil.Configuration.Extensions;
using Xunit;

namespace Ulfdrasil.Configuration.UnitTests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        private class MySettings
        {
            public string? Foo { get; set; }
        }

        [Fact]
        public void AddSettings_BindsAndRegistersSingleton()
        {
            // Arrange
            const string expectedFooValue = "bar";
            var builder = new HostApplicationBuilder();

            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{nameof(MySettings)}:Foo"] = expectedFooValue
            });

            // Act
            builder.AddSettings<MySettings>();

            var provider = builder.Services.BuildServiceProvider();
            var settings = provider.GetRequiredService<MySettings>();

            // Assert
            settings.Should().NotBeNull();
            settings.Foo.Should().Be(expectedFooValue);
        }

        [Fact]
        public void AddSettings_ThrowsWhenSectionMissing()
        {
            // Arrange
            var builder = new HostApplicationBuilder();

            // Act & Assert
            System.Action act = () => builder.AddSettings<MySettings>();

            act.Should().Throw<KeyNotFoundException>()
               .Which.Message.Should().Contain(nameof(MySettings));
        }
    }
}