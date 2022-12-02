using Unity.Mathematics;

public struct Rectangle
{
    public int2 Pos;
    public int2 Dims;

    public Rectangle(int2 pos, int2 dims)
    {
        Pos = pos;
        Dims = dims;
    }

    public override string ToString()
    {
        return $"Pos is {Pos}, Dims is {Dims}";
    }
}