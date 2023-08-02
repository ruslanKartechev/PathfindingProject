using System.Collections.Generic;
using System.Linq;

namespace Pathfinding.Data
{
    public class BinaryHeapNodes : IBinaryHeap<GridCoord2, PathNode>
    {
        private readonly IDictionary<GridCoord2, int> map;
        private readonly IList<PathNode> collection;
        private readonly IComparer<PathNode> comparer;
        
        public BinaryHeapNodes(IComparer<PathNode> comparer)
        {
            this.comparer = comparer;
            collection = new List<PathNode>();
            map = new Dictionary<GridCoord2, int>();
        }

        public int Count => collection.Count;

        public void Clear()
        {
            collection.Clear();
            map.Clear();
        }		

        public void Enqueue(PathNode item)
        {
            collection.Add(item);
            var i = collection.Count - 1;
            map[item.Position] = i;
            while(i > 0)
            {
                var j = (i - 1) / 2;
                
                if (comparer.Compare(collection[i], collection[j]) <= 0)
                    break;

                Swap(i, j);
                i = j;
            }
        }
	
        private void Swap(int i, int j)
        {
            (collection[i], collection[j]) = (collection[j], collection[i]);
            map[collection[i].Position] = i;
            map[collection[j].Position] = j;
        }

        public PathNode Dequeue()
        {
            if (collection.Count == 0) return default;
          
            var result = collection.First();
            RemoveRoot();
            map.Remove(result.Position);
            return result;
        }
	
        private void RemoveRoot()
        {
            collection[0] = collection.Last();
            map[collection[0].Position] = 0;
            collection.RemoveAt(collection.Count - 1);

            // Sort nodes from top to bottom.
            var i = 0;
            while(true)
            {
                var largest = LargestIndex(i);
                if (largest == i)
                    return;

                Swap(i, largest);
                i = largest;
            }
        }
	
        private int LargestIndex(int i)
        {
            var leftInd = 2 * i + 1;
            var rightInd = 2 * i + 2;
            var largest = i;

            if (leftInd < collection.Count 
                && comparer.Compare(collection[leftInd], collection[largest]) > 0)
                largest = leftInd;

            if (rightInd < collection.Count 
                && comparer.Compare(collection[rightInd], collection[largest]) > 0) 
                largest = rightInd;
            
            return largest;
        }

        public bool TryGet(GridCoord2 key, out PathNode value)
        {
            if (!map.TryGetValue(key, out int index))
            {
                value = default;
                return false;
            }
            
            value = collection[index];
            return true;
        }

        public void Modify(PathNode value)
        {
            if (map.TryGetValue(value.Position, out var index) == false)
                throw new KeyNotFoundException(nameof(value));
            
            collection[index] = value;
        }
    }
}