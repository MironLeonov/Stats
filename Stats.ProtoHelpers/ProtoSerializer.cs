using Confluent.Kafka;
using Google.Protobuf;


namespace Stats.ProtoHelpers
{
    public class ProtoSerializer<T> : ISerializer<T> 
        where T : IMessage<T>
    {
        public byte[] Serialize(T data, SerializationContext context) =>
            data.ToByteArray();
    }
}