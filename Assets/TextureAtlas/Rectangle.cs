using Unity.Mathematics;

struct Rectangle
{
    public int2 Pos;
    public int2 Dims;

    public Rectangle(int2 pos, int2 dims)
    {
        Pos = pos;
        Dims = dims;
    }
}