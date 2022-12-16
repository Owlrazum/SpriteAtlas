using System.IO;

using System.Runtime.Serialization.Formatters.Binary;

namespace Orazum.SpriteAtlas.Generation
{
    public class SpriteAtlasSaver
    {
        public void SaveAtlas(SpriteAtlasManaged atlas, string filePath)
        {
            string persistentPath = UnityEngine.Application.persistentDataPath + "/" + filePath;
            int lastIndex = persistentPath.LastIndexOf("/");
            string directoriesPath = persistentPath.Substring(0, persistentPath.Length - (persistentPath.Length - lastIndex));
            Directory.CreateDirectory(directoriesPath);
            using (var stream = File.Open(persistentPath, FileMode.Create))
            {
                BinaryFormatter formatter = new();
                formatter.Serialize(stream, atlas);
            }

            using (var stream = File.Open(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new();
                formatter.Serialize(stream, atlas);
            }
        }

        public SpriteAtlasManaged LoadAtlas(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (var stream = File.Open(filePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new();
                    var atlas = (SpriteAtlasManaged)formatter.Deserialize(stream);
                    return atlas;
                }
            }

            string persistentPath = UnityEngine.Application.persistentDataPath + "/" + filePath;
            using (var stream = File.Open(persistentPath, FileMode.Open))
            {
                BinaryFormatter formatter = new();
                var atlas = (SpriteAtlasManaged)formatter.Deserialize(stream);
                return atlas;
            }
        }
    }
}