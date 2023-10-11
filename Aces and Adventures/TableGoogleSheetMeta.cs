using System;
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = MetadataType.StringTable)]
public class TableGoogleSheetMeta : IMetadata
{
	[SerializeField]
	[ShowOnly]
	protected int _sheetId;

	[SerializeField]
	[HideInInspector]
	protected string _pushHash;

	[SerializeField]
	[HideInInspector]
	protected string _pullHash;

	[SerializeReference]
	[HideInInspector]
	protected List<StringTableColumn> _columns;

	private Sheet _sheet;

	public int sheetId
	{
		get
		{
			return _sheetId;
		}
		set
		{
			_sheetId = value;
		}
	}

	public string pushHash
	{
		get
		{
			return _pushHash;
		}
		set
		{
			_pushHash = value;
		}
	}

	public string pullHash
	{
		get
		{
			return _pullHash;
		}
		set
		{
			_pullHash = value;
		}
	}

	public List<StringTableColumn> columns
	{
		get
		{
			return _columns ?? (_columns = new List<StringTableColumn>());
		}
		set
		{
			_columns = value;
		}
	}

	public Sheet sheet
	{
		get
		{
			return _sheet;
		}
		set
		{
			_sheet = value;
		}
	}

	public TableGoogleSheetMeta()
	{
	}

	public TableGoogleSheetMeta(int sheetId)
	{
		_sheetId = sheetId;
	}

	public void ClearHashes()
	{
		string text2 = (pullHash = "");
		pushHash = text2;
	}

	public static implicit operator bool(TableGoogleSheetMeta meta)
	{
		if (meta != null)
		{
			return meta._sheetId != 0;
		}
		return false;
	}

	public static implicit operator int(TableGoogleSheetMeta meta)
	{
		return meta?._sheetId ?? 0;
	}
}
