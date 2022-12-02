using UnityEngine;
using UnityEngine.Assertions;

using Unity.Mathematics;

using Orazum.TextureAtlas;
using Orazum.Utilities;

public class TestPacking : MonoBehaviour
{
    void Start()
    {
        AtlasPackingByBinaryTree packer = new AtlasPackingByBinaryTree();
        Rectangle rect = packer.Insert(100, 100);
        Assert.IsTrue(math.all(rect.Pos == int2.zero) && math.all(rect.Dims == new int2(100, 100)));
        DebugUtilities.DrawRectangle(rect, 10);
    }
}