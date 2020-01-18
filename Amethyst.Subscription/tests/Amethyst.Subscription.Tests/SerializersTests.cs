using System.Text;
using System.Text.Json;
using Amethyst.Subscription.Serializers;
using Amethyst.Subscription.Tests.Fakes;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace Amethyst.Subscription.Tests
{
    public sealed class SerializersTests
    {
        [Fact(Skip = "Constructor deserialization will be implemented in .net 5")]
        public void JsonSerializing()
        {
            var fixture = new Fixture();
            var e = fixture.Create<RedEvent>();

            var serializer = new JsonEventDeserializer<RedEvent>();

            var bytes = JsonSerializer.SerializeToUtf8Bytes(e);

            var deserialized = serializer.Deserialize(bytes);

            deserialized.Event.Should().BeEquivalentTo(e);
        }

        [Fact]
        public void Json_CaseInsensitive()
        {
            var json = @"{ ""propA"":22047748656944,""propB"":281179222,""propC"":4,""propD"":4}";

            var serializer = new JsonEventDeserializer<CustomModel>();

            var result = (CustomModel) serializer.Deserialize(Encoding.UTF8.GetBytes(json)).Event;

            result.PropA.Should().Be(22047748656944);
            result.PropB.Should().Be(281179222);
            result.PropC.Should().Be(4);
        }
    }
}