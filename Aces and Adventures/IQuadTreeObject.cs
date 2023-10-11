public interface IQuadTreeObject<T>
{
	void AddToPartition(ushort x, ushort y);

	void RemoveFromTree(QuadTree<T> tree);
}
