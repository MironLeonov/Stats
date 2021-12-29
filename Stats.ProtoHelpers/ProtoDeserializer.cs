using System;
using Confluent.Kafka;
using Google.Protobuf;


namespace Stats.ProtoHelpers
{
    public class ProtoDeserializer<T> : IDeserializer<T>
        where T : IMessage<T>, new()
    {
        private readonly MessageParser<T> _parser;

        public ProtoDeserializer() { this._parser = new MessageParser<T>(() => new T()); }

        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) =>
            this._parser.ParseFrom(data.ToArray());
    }
}