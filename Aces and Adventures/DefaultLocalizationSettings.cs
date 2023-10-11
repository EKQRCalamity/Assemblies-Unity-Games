using System;
using UnityEngine;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = MetadataType.Locale)]
public class DefaultLocalizationSettings : IMetadata
{
	[SerializeField]
	protected OutOfDateTranslationHandling _outOfDateTranslationHandling;

	[SerializeField]
	protected bool _forcePush;

	[SerializeField]
	protected bool _pullOnly;

	public OutOfDateTranslationHandling? outOfDateTranslationHandling => _outOfDateTranslationHandling.GetValue();

	public bool pullOnly => _pullOnly;

	public bool forcePush => _forcePush;

	public bool ForcePush()
	{
		if (_forcePush)
		{
			return !(_forcePush = false);
		}
		return false;
	}
}
