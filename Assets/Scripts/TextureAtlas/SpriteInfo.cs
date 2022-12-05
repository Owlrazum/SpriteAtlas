namespace Orazum.SpriteAtlas
{
    [System.Serializable]
    public struct SpriteInfo
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

#if !UNITY_EDITOR
    public static void SaveAtlas(string filePath, YourSpriteAtlas atlas)
    {
        // implement this
    }

    public static YourSpriteAtlas LoadAtlas(string filePath)
    {
        // implement this
    }

    public static string[] GetSpriteNames(YourSpriteAtlas atlas)
    {
        // implement this 
    }

    public static SpriteInfo GetSpriteByName(YourSpriteAtlas atlas, string name)
    {
        // implement this
    }
#endif
}