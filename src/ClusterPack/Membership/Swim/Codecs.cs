using System;
using System.Buffers;

namespace ClusterPack.Membership.Swim
{
    internal sealed class SwimCodec
    {
        public void Encode<TBufferWriter>(ISwimProtocol message, ref TBufferWriter writer)
            where TBufferWriter: IBufferWriter<byte>
        {
            throw new NotImplementedException();
        }

        public ISwimProtocol Decode(ReadOnlySequence<byte> payload)
        {
            throw new NotImplementedException();
        }
    }
}