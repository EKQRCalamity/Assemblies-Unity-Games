using System.Collections.Generic;

public struct ListEnumerator<T>
{
	public struct Enumerator
	{
		private List<T>.Enumerator _enumerator;

		public T Current => _enumerator.Current;

		public Enumerator(List<T>.Enumerator enumerator)
		{
			_enumerator = enumerator;
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}
	}

	private readonly List<T> _list;

	public ListEnumerator(List<T> list)
	{
		_list = list;
	}

	public IEnumerable<T> Enumerable()
	{
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_list.GetEnumerator());
	}
}
