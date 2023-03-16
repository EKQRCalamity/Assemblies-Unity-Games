using UnityEngine;

public class PatternString
{
	private int mainIndex;

	private int subIndex;

	private char subSubStringSplitter;

	private string[] mainPatternString;

	private string[] subPatternString;

	private string[] subsubPatternString;

	public PatternString(string[] patternString, bool randomizeMain = true, bool randomizeSub = true)
	{
		mainPatternString = patternString;
		mainIndex = (randomizeMain ? Random.Range(0, patternString.Length) : 0);
		subPatternString = mainPatternString[mainIndex].Split(',');
		subIndex = (randomizeSub ? Random.Range(0, subPatternString.Length) : 0);
	}

	public PatternString(string patternString, bool randomizeSub = true)
	{
		mainIndex = 0;
		mainPatternString = new string[1];
		mainPatternString[0] = patternString;
		subPatternString = mainPatternString[0].Split(',');
		subIndex = (randomizeSub ? Random.Range(0, subPatternString.Length) : 0);
	}

	public PatternString(string[] patternString, char subSubStringSplitter, bool randomizeMain = true, bool randomizeSub = true)
	{
		mainPatternString = patternString;
		mainIndex = (randomizeMain ? Random.Range(0, patternString.Length) : 0);
		subPatternString = mainPatternString[mainIndex].Split(',');
		subIndex = (randomizeSub ? Random.Range(0, subPatternString.Length) : 0);
		subsubPatternString = subPatternString[subIndex].Split(subSubStringSplitter);
		this.subSubStringSplitter = subSubStringSplitter;
	}

	public int SubStringLength()
	{
		return subPatternString.Length;
	}

	public void SetMainStringIndex(int value)
	{
		mainIndex = value % mainPatternString.Length;
	}

	public void SetSubStringIndex(int value)
	{
		subIndex = value % subPatternString.Length;
	}

	public int GetMainStringIndex()
	{
		return mainIndex;
	}

	public int GetSubStringIndex()
	{
		return subIndex;
	}

	public char GetSubsubstringLetter(int index)
	{
		return subsubPatternString[index][0];
	}

	public float GetSubsubstringFloat(int index)
	{
		float result = 0f;
		if (Parser.FloatTryParse(subsubPatternString[index], out result))
		{
			return result;
		}
		Debug.LogError("Syntax Error in" + subsubPatternString);
		return result;
	}

	private char GetLetter()
	{
		return subPatternString[subIndex][0];
	}

	public char PopLetter()
	{
		IncrementString();
		return GetLetter();
	}

	public string GetString()
	{
		return subPatternString[subIndex];
	}

	public string PopString()
	{
		IncrementString();
		return GetString();
	}

	public float GetFloat()
	{
		float result = 0f;
		if (Parser.FloatTryParse(subPatternString[subIndex], out result))
		{
			return result;
		}
		Debug.LogError("Syntax Error in" + mainPatternString);
		return result;
	}

	public float PopFloat()
	{
		IncrementString();
		return GetFloat();
	}

	private int GetInt()
	{
		int result = 0;
		if (Parser.IntTryParse(subPatternString[subIndex], out result))
		{
			return result;
		}
		Debug.LogError("Syntax Error in" + mainPatternString);
		return result;
	}

	public int PopInt()
	{
		IncrementString();
		return GetInt();
	}

	public void IncrementString()
	{
		if (subIndex < subPatternString.Length - 1)
		{
			subIndex++;
		}
		else
		{
			mainIndex = (mainIndex + 1) % mainPatternString.Length;
			subIndex = 0;
		}
		subPatternString = mainPatternString[mainIndex].Split(',');
		if (subsubPatternString != null)
		{
			subsubPatternString = subPatternString[subIndex].Split(subSubStringSplitter);
		}
	}
}
