using System;
using System.Collections.Generic;
using UnityEngine;

namespace TMPro;

[Serializable]
[ExecuteInEditMode]
public class TMP_Settings : ScriptableObject
{
	private static TMP_Settings s_Instance;

	[SerializeField]
	private bool m_enableWordWrapping;

	[SerializeField]
	private bool m_enableKerning;

	[SerializeField]
	private bool m_enableExtraPadding;

	[SerializeField]
	private bool m_enableTintAllSprites;

	[SerializeField]
	private bool m_warningsDisabled;

	[SerializeField]
	private TMP_FontAsset m_defaultFontAsset;

	[SerializeField]
	private List<TMP_FontAsset> m_fallbackFontAssets;

	[SerializeField]
	private TMP_SpriteAsset m_defaultSpriteAsset;

	[SerializeField]
	private TMP_StyleSheet m_defaultStyleSheet;

	public static bool enableWordWrapping => instance.m_enableWordWrapping;

	public static bool enableKerning => instance.m_enableKerning;

	public static bool enableExtraPadding => instance.m_enableExtraPadding;

	public static bool enableTintAllSprites => instance.m_enableTintAllSprites;

	public static bool warningsDisabled => instance.m_warningsDisabled;

	public static TMP_FontAsset defaultFontAsset => instance.m_defaultFontAsset;

	public static List<TMP_FontAsset> fallbackFontAssets => instance.m_fallbackFontAssets;

	public static TMP_SpriteAsset defaultSpriteAsset => instance.m_defaultSpriteAsset;

	public static TMP_StyleSheet defaultStyleSheet => instance.m_defaultStyleSheet;

	public static TMP_Settings instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = Resources.Load("TMP Settings") as TMP_Settings;
			}
			return s_Instance;
		}
	}

	public static TMP_Settings LoadDefaultSettings()
	{
		if (s_Instance == null)
		{
			TMP_Settings tMP_Settings = Resources.Load("TMP Settings") as TMP_Settings;
			if (tMP_Settings != null)
			{
				s_Instance = tMP_Settings;
			}
		}
		return s_Instance;
	}

	public static TMP_Settings GetSettings()
	{
		if (instance == null)
		{
			return null;
		}
		return instance;
	}

	public static TMP_FontAsset GetFontAsset()
	{
		if (instance == null)
		{
			return null;
		}
		return instance.m_defaultFontAsset;
	}

	public static TMP_SpriteAsset GetSpriteAsset()
	{
		if (instance == null)
		{
			return null;
		}
		return instance.m_defaultSpriteAsset;
	}

	public static TMP_StyleSheet GetStyleSheet()
	{
		if (instance == null)
		{
			return null;
		}
		return instance.m_defaultStyleSheet;
	}
}
