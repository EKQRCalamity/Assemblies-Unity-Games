using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class StringIntFlags
{
	public delegate void OnFlagValueChange(string flag, int previousValue, int newValue);

	public static bool DEBUG;

	[ProtoMember(1)]
	private Dictionary<string, int> _flags = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, int> flags
	{
		get
		{
			return _flags ?? (_flags = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
		}
		set
		{
			_flags = value;
		}
	}

	public int count
	{
		get
		{
			if (_flags == null)
			{
				return 0;
			}
			return _flags.Count;
		}
	}

	public event OnFlagValueChange onValueChange;

	public int GetFlagValue(string flag)
	{
		if (!flags.ContainsKey(flag))
		{
			return 0;
		}
		return flags[flag];
	}

	public bool CheckFlag(string flag, int value, FlagCheckType checkType)
	{
		return checkType.Check(GetFlagValue(flag), value);
	}

	public DictionaryPairEnumerator<string, int> GetFlagValues()
	{
		return flags.EnumeratePairs();
	}

	public void AdjustFlagValue(string flag, int value, FlagSetType setType, bool signalChange = true)
	{
		if (setType == FlagSetType.AddToCurrentValue)
		{
			AddToFlagValue(flag, value, signalChange);
		}
		else
		{
			SetFlagValue(flag, value, signalChange);
		}
	}

	public void AddToFlagValue(string flag, int delta, bool signalChange = true)
	{
		if (!flags.ContainsKey(flag))
		{
			flags.Add(flag, 0);
		}
		flags[flag] += delta;
		if (signalChange && this.onValueChange != null)
		{
			this.onValueChange(flag, flags[flag] - delta, flags[flag]);
		}
		_ = DEBUG;
	}

	public void SetFlagValue(string flag, int setValue, bool signalChange = true)
	{
		int flagValue = GetFlagValue(flag);
		flags[flag] = setValue;
		if (signalChange && this.onValueChange != null)
		{
			this.onValueChange(flag, flagValue, setValue);
		}
		_ = DEBUG;
	}

	public override string ToString()
	{
		return flags.ToStringSmart((KeyValuePair<string, int> pair) => $"({pair.Key}, {pair.Value})", "\n");
	}
}
