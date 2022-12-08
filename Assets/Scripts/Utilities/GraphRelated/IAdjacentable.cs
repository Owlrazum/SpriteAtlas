namespace Orazum.Graphs
{
    public interface IAdjacentable<T>
    {
        public bool IsAdjacent(T other);
        public int Id { get; set; }
    }
}