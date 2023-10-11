public class ProtoProperty<T> where T : class
{
	private readonly string _filePath;

	private T _value;

	private bool _isDirty;

	private T value
	{
		get
		{
			T val = _value;
			if (val == null)
			{
				T obj = IOUtil.LoadFromBytesBackup<T>(this) ?? ConstructorCache<T>.Constructor();
				T val2 = obj;
				_value = obj;
				val = val2;
			}
			return val;
		}
	}

	public T read => value;

	public T write
	{
		get
		{
			if (!(_isDirty = true))
			{
				return null;
			}
			return value;
		}
	}

	public ProtoProperty(string filePath)
	{
		_filePath = filePath;
	}

	private void _Save()
	{
		_isDirty = false;
		if (_value != null)
		{
			IOUtil.WriteToFileBackup(_value, this);
		}
	}

	public void SaveChanges()
	{
		if (_isDirty)
		{
			_Save();
		}
	}

	public void Delete()
	{
		IOUtil.DeleteSafe(this);
	}

	public static implicit operator ProtoProperty<T>(string filePath)
	{
		return new ProtoProperty<T>(filePath);
	}

	public static implicit operator string(ProtoProperty<T> protoProperty)
	{
		return protoProperty?._filePath;
	}
}
