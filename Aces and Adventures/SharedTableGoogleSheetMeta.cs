using System;
using UnityEngine;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = MetadataType.SharedTableData)]
public class SharedTableGoogleSheetMeta : IMetadata
{
	[SerializeField]
	protected GoogleUtil.Sheets.WrapStrategy _textWrapMode;

	[SerializeField]
	protected bool _checkProperNouns;

	[SerializeField]
	protected OutOfDateTranslationHandling _outOfDateTranslationHandling;

	[SerializeField]
	protected bool _forcePush;

	public GoogleUtil.Sheets.WrapStrategy textWrapMode => _textWrapMode;

	public bool checkProperNouns => _checkProperNouns;

	public OutOfDateTranslationHandling? outOfDateTranslationHandling => _outOfDateTranslationHandling.GetValue();

	public bool ForcePush()
	{
		if (_forcePush)
		{
			return !(_forcePush = false);
		}
		return false;
	}
}
