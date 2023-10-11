using System.Collections.Generic;
using UnityEngine;

[ScriptOrder(-10000)]
public class UndoHistory : MonoBehaviour
{
	private Stack<Stack<AUndoRecord>> _undoStacks = new Stack<Stack<AUndoRecord>>();

	private Stack<Stack<AUndoRecord>> _redoStacks = new Stack<Stack<AUndoRecord>>();

	private Stack<AUndoRecord> _pendingUndoStack;

	private bool _pendingUndoRequest;

	private bool _pendingRedoRequest;

	private bool _suppressAddEntry;

	private HashSet<object> _suppressors;

	protected HashSet<object> suppressors => _suppressors ?? (_suppressors = new HashSet<object>(ReferenceEqualityComparer<object>.Default));

	public bool acceptingEntries
	{
		get
		{
			if (!_suppressAddEntry)
			{
				return suppressors.Count == 0;
			}
			return false;
		}
	}

	private void _Undo()
	{
		_pendingUndoRequest = false;
		if (!HasUndo())
		{
			return;
		}
		Stack<AUndoRecord> stack = _undoStacks.Pop();
		foreach (AUndoRecord item in stack)
		{
			item.Undo();
		}
		_redoStacks.Push(stack.ReverseStack());
	}

	private void _Redo()
	{
		_pendingRedoRequest = false;
		if (!HasRedo())
		{
			return;
		}
		Stack<AUndoRecord> stack = _redoStacks.Pop();
		foreach (AUndoRecord item in stack)
		{
			item.Redo();
		}
		_undoStacks.Push(stack.ReverseStack());
	}

	private void Update()
	{
		if (_pendingUndoRequest && _pendingRedoRequest)
		{
			_pendingUndoRequest = (_pendingRedoRequest = false);
		}
		if (_pendingUndoStack != null)
		{
			_undoStacks.Push(_pendingUndoStack);
			_redoStacks.Clear();
			_pendingUndoStack = null;
		}
		_suppressAddEntry = true;
		if (_pendingUndoRequest)
		{
			_Undo();
		}
		else if (_pendingRedoRequest)
		{
			_Redo();
		}
		_suppressAddEntry = false;
	}

	public void AddEntry(AUndoRecord undoRecord)
	{
		if (acceptingEntries)
		{
			(_pendingUndoStack ?? (_pendingUndoStack = new Stack<AUndoRecord>())).Push(undoRecord);
		}
	}

	public bool HasUndo()
	{
		return _undoStacks.Count > 0;
	}

	public bool Undo()
	{
		return _pendingUndoRequest = HasUndo();
	}

	public bool HasRedo()
	{
		return _redoStacks.Count > 0;
	}

	public bool Redo()
	{
		return _pendingRedoRequest = HasRedo();
	}

	public void SuppressEntries(object obj)
	{
		suppressors.Add(obj);
	}

	public void AllowEntries(object obj)
	{
		suppressors.Remove(obj);
	}
}
