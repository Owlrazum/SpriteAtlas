using UnityEngine;
using Unity.Mathematics;

namespace Orazum.TextureAtlas
{
    [System.Serializable]
    class TextureInfo
    {
        int2 m_Pos;
        Texture2D m_Texture;

        public TextureInfo(int2 pos, Texture2D texture)
        {
            m_Pos = pos;
            m_Texture = texture;
        }

        public SpriteInfo GetSpriteInfo()
        {
            return new SpriteInfo()
            {
                X = m_Pos.x,
                Y = m_Pos.y,
                Width = m_Texture.width,
                Height = m_Texture.height
            };
        }

        public string GetName()
        {
            return m_Texture.name;
        }
    }
}
