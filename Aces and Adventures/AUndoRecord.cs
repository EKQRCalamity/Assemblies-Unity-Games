public abstract class AUndoRecord
{
	public enum EntryType : byte
	{
		Add,
		Remove
	}

	public virtual EntryType entryType => EntryType.Add;

	private void _Do(EntryType entryType)
	{
		if (entryType == EntryType.Add)
		{
			_Add();
		}
		else
		{
			_Remove();
		}
	}

	protected virtual void _Add()
	{
	}

	protected virtual void _Remove()
	{
	}

	public virtual void Undo()
	{
		_Do(entryType.Opposite());
	}

	public virtual void Redo()
	{
		_Do(entryType);
	}
}
