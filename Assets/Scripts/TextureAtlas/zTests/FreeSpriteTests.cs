using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;

namespace Orazum.SpriteAtlas.Tests
{
    class FreeSpriteTests
    {
        [Test]
        public void Simple()
        {
            FreeSprite s1 = Create(new int2(0, 0), new int2(100, 100));
            FreeSprite s2 = Create(new int2(101, 0), new int2(100, 100));

            Assert.IsTrue(s1.IsAdjacent(s2) && s2.IsAdjacent(s1));

            s2.Move(new int2(1, 0));
            Assert.IsFalse(s1.IsAdjacent(s2) && s2.IsAdjacent(s1));
        }

        [Test]
        public void Pair()
        {
            var s1 = Create(new int2(50, 100), new int2(200, 50));
            var s2 = Create(new int2(100, 151), new int2(50, 200));
            Assert.IsTrue(s1.IsAdjacent(s2) && s2.IsAdjacent(s1));

            s2.Move(new int2(150, 0));
            Assert.IsTrue(s1.IsAdjacent(s2) && s2.IsAdjacent(s1));

            var s3 = s2.Clone();
            s3.Move(new int2(100000, 0));
            Assert.IsFalse(s1.IsAdjacent(s3) && s3.IsAdjacent(s1));

            s3 = s2.Clone();
            s3.Move(new int2(0, 1));
            Assert.IsFalse(s1.IsAdjacent(s3) && s2.IsAdjacent(s3));
        }

        FreeSprite Create(int2 pos, int2 dims)
        {
            return new(pos, dims, new bool2(false, false));
        }

    }

    static class Extension
    {
        public static void Move(this FreeSprite sprite, int2 delta)
        {
            sprite.SpriteData.Pos += delta;
        }

        public static FreeSprite Clone(this FreeSprite sprite)
        {
            return new(sprite.Pos, sprite.Dims, sprite.IsBorderingAtlas);
        }
    }
}