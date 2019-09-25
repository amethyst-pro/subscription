using System.Text;
using Amethyst.Subscription.Serializers;
using Amethyst.Subscription.Tests.Fakes;
using AutoFixture;
using FluentAssertions;
using Utf8Json;
using Utf8Json.Resolvers;
using Xunit;

namespace Amethyst.Subscription.Tests
{
    public class SerializersTests
    {
        [Fact]
        public void JsonSerializing()
        {
            var fixture = new Fixture();
            var e = fixture.Create<RedEvent>();

            var serializer = new JsonEventDeserializer<RedEvent>();

            var bytes = JsonSerializer.Serialize(e);

            var deserialized = serializer.Deserialize(bytes);

            deserialized.Event.Should().BeEquivalentTo(e);
        }


        [Fact]
        public void Json_CaseInsensitive()
        {
            var json = @"{ ""prop_a"":22047748656944,""prop_b"":281179222,""prop_c"":4,""prop_d"":4}";

            var serializer = new JsonEventDeserializer<CustomModel>(StandardResolver.SnakeCase);

            var result = (CustomModel)serializer.Deserialize(Encoding.UTF8.GetBytes(json)).Event;

            result.PropA.Should().Be(22047748656944);
            result.PropB.Should().Be(281179222);
            result.PropC.Should().Be(4);

        }

    }
}