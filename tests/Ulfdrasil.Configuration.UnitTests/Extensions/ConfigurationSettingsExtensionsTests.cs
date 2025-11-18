using Microsoft.Extensions.Configuration;
using Xunit;
using AwesomeAssertions;
using Ulfdrasil.Configuration.Extensions;

namespace Ulfdrasil.Configuration.UnitTests.Extensions
{
    public class ConfigurationSettingsExtensionsTests
    {
        private sealed class MySettings
        {
            public string? Foo { get; set; }
        }

        private const string SectionName = nameof(MySettings);

        [Fact]
        public void GetSettings_ReturnsBoundSettings_WhenSectionExists()
        {
            var config = BuildConfig(($"{SectionName}:Foo", "bar"));

            var settings = config.GetSettings<MySettings>();

            settings.Should().NotBeNull();
            settings.Foo.Should().Be("bar");
        }

        [Fact]
        public void GetSettings_ReturnsNull_WhenSectionMissing()
        {
            var config = BuildConfig();

            var settings = config.GetSettings<MySettings>();

            settings.Should().BeNull();
        }

        [Fact]
        public void GetSettings_WithParentSection_ReturnsBoundSettings()
        {
            var config = BuildConfig(("Parent:" + SectionName + ":Foo", "baz"));

            var settings = config.GetSettings<MySettings>("Parent");

            settings.Should().NotBeNull();
            settings.Foo.Should().Be("baz");
        }

        [Fact]
        public void GetRequiredSettings_ReturnsSettings_WhenSectionExists()
        {
            var config = BuildConfig(($"{SectionName}:Foo", "qux"));

            var settings = config.GetRequiredSettings<MySettings>();

            settings.Should().NotBeNull();
            settings.Foo.Should().Be("qux");
        }

        [Fact]
        public void GetRequiredSettings_Throws_WhenSectionMissing()
        {
            var config = BuildConfig();

            Action act = () => config.GetRequiredSettings<MySettings>();

            act.Should()
               .Throw<KeyNotFoundException>()
               .Which.Message.Should()
               .Contain(SectionName);
        }

        [Fact]
        public void GetRequiredSettings_WithParentSection_ReturnsSettings_WhenExists()
        {
            var config = BuildConfig(("Parent:" + SectionName + ":Foo", "parentval"));

            var settings = config.GetRequiredSettings<MySettings>("Parent");

            settings.Should().NotBeNull();
            settings.Foo.Should().Be("parentval");
        }

        [Fact]
        public void GetRequiredSettings_WithParentSection_Throws_WhenMissing()
        {
            var config = BuildConfig();

            Action act = () => config.GetRequiredSettings<MySettings>("Parent");

            act.Should()
               .Throw<KeyNotFoundException>()
               .Which.Message.Should()
               .Contain("Parent:" + SectionName);
        }

        private static IConfiguration BuildConfig(params (string Key, string? Value)[] entries)
        {
            var list = new List<KeyValuePair<string, string?>>(entries.Length);

            foreach (var e in entries)
            {
                list.Add(new KeyValuePair<string, string?>(e.Key, e.Value));
            }

            return new ConfigurationBuilder().AddInMemoryCollection(list).Build();
        }
    }
}