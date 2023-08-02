namespace Pathfinding.Data
{
    public interface IBinaryHeap<in TKey, T>
    {
        void Enqueue(T item);
        T Dequeue();
        void Clear();
        bool TryGet(TKey key, out T value);
        void Modify(T value);
        int Count { get; }
    }
}