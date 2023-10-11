using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AbilityDeckView))]
public class AbilityDeckViewCreation : MonoBehaviour
{
	public TMP_InputField nameInputField;

	public BoolEvent onDeleteInputEnabledChanged;

	public BoolEvent onNameInputEnabledChanged;

	public StringEvent onCardCountChange;

	public BoolEvent onShowCardCountChange;

	public BoolEvent onCopyInputEnabledChanged;

	private bool _deleteInputEnabled;

	private bool _nameInputEnabled;

	private bool _showCardCount;

	private int _cardCount = -1;

	private bool _copyInputEnabled;

	public bool deleteInputEnabled
	{
		get
		{
			return _deleteInputEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _deleteInputEnabled, value))
			{
				onDeleteInputEnabledChanged?.Invoke(value);
			}
		}
	}

	public bool nameInputEnabled
	{
		get
		{
			return _nameInputEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _nameInputEnabled, value))
			{
				onNameInputEnabledChanged?.Invoke(value);
			}
		}
	}

	public bool showCardCount
	{
		get
		{
			return _showCardCount;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _showCardCount, value))
			{
				onShowCardCountChange?.Invoke(value);
			}
		}
	}

	public int cardCount
	{
		get
		{
			return _cardCount;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _cardCount, value))
			{
				onCardCountChange?.Invoke($"{value} / {30}");
			}
		}
	}

	public bool copyInputEnabled
	{
		get
		{
			return _copyInputEnabled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _copyInputEnabled, value))
			{
				onCopyInputEnabledChanged?.Invoke(value);
			}
		}
	}

	public static event Action<AbilityDeckView> OnDestroyRequested;

	public static event Action<AbilityDeckView> OnCopyRequested;

	public void RequestDestroy()
	{
		AbilityDeckViewCreation.OnDestroyRequested?.Invoke(GetComponent<AbilityDeckView>());
	}

	public void RequestCopy()
	{
		AbilityDeckViewCreation.OnCopyRequested?.Invoke(GetComponent<AbilityDeckView>());
	}
}
