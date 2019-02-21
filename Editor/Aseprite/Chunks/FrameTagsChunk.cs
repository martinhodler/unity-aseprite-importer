using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Aseprite.Chunks
{
    public enum LoopAnimation : byte
    {
        Forward = 0,
        Reverse = 1,
        PingPong = 2,
    }


    public class FrameTag
    {
        public ushort FrameFrom { get; private set; }
        public ushort FrameTo { get; private set; }
        public LoopAnimation Animation { get; private set; }
        private byte[] ForFuture { get; set; } // 8 Bytes
        public Color TagColor { get; set; } // 3 Bytes
        // 1 Extra Byte
        public string TagName { get; private set; }

        public FrameTag(BinaryReader reader)
        {
            FrameFrom = reader.ReadUInt16();
            FrameTo = reader.ReadUInt16();
            Animation = (LoopAnimation)reader.ReadByte();
            ForFuture = reader.ReadBytes(8);

            byte[] colorBytes = reader.ReadBytes(3);
            TagColor = new Color((colorBytes[0] / 255f), (colorBytes[1] / 255f), (colorBytes[2] / 255f));

            reader.ReadByte(); // Extra byte (zero)

            ushort nameLength = reader.ReadUInt16();
            TagName = Encoding.Default.GetString(reader.ReadBytes(nameLength));
        }
    }

    public class FrameTagsChunk : Chunk
    {
        public ushort TagCount { get; private set; }
        private byte[] ForFuture { get; set; } // 8 Bytes
        
        public List<FrameTag> Tags { get; private set; }

        public FrameTagsChunk(uint length, BinaryReader reader) : base(length, ChunkType.FrameTags)
        {
            TagCount = reader.ReadUInt16();
            ForFuture = reader.ReadBytes(8);

            Tags = new List<FrameTag>();

            for (int i = 0; i < TagCount; i++)
            {
                Tags.Add(new FrameTag(reader));
            }
        }
    }
}
