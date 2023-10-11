using System.Collections.Generic;

public struct DictionaryValueEnumerator<K, V>
{
	public struct Enumerator
	{
		private Dictionary<K, V>.Enumerator _enumerator;

		public V Current => _enumerator.Current.Value;

		public Enumerator(Dictionary<K, V>.Enumerator enumerator)
		{
			_enumerator = enumerator;
		}

		public bool MoveNext()
		{
			return _enumerator.MoveNext();
		}
	}

	private readonly Dictionary<K, V> _dictionary;

	public DictionaryValueEnumerator(Dictionary<K, V> dictionary)
	{
		_dictionary = dictionary;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_dictionary.GetEnumerator());
	}
}
