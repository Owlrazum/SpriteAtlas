using System.IO;

using System.Runtime.Serialization.Formatters.Binary;

namespace Orazum.SpriteAtlas.Generation
{
    public class SpriteAtlasSaver
    {
        public void SaveAtlas(SpriteAtlasManaged atlas, string filePath)
        {
            using (var stream = File.Open(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new();
                formatter.Serialize(stream, atlas);
            }
        }

        public SpriteAtlasManaged LoadAtlas(string filePath)
        {
            using (var stream = File.Open(filePath, FileMode.Open))
            { 
                BinaryFormatter formatter = new();
                var atlas = (SpriteAtlasManaged)formatter.Deserialize(stream);
                return atlas;
            }
        }
    }
}