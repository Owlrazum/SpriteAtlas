namespace Orazum.SpriteAtlas
{
    [System.Serializable]
    public struct SpriteInfo
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public override string ToString()
        {
            return $"Pos:{X} {Y}, Dimensions:{Width} {Height}";
        }
    }
}