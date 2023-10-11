using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu]
public class TextMeshProSkin : UISkin
{
	private static TextMeshProSkin _Default;

	public TextMeshProSkin fallbackSkin;

	[Header("Font")]
	public TMP_FontAsset fontAsset;

	public Material fontMaterial;

	[Header("Font Size")]
	public bool setFontSize;

	[HideInInspectorIf("_hideFontSize", false)]
	public bool enableAutoSizing = true;

	[HideInInspectorIf("_hideFontSize", false)]
	public float fontSizeMin = 12f;

	[HideInInspectorIf("_hideFontSize", false)]
	public float fontSizeMax = 24f;

	[Header("Color")]
	public bool setColor;

	[HideInInspectorIf("_hideColor", false)]
	public Color textColor = new Color(0.16f, 0.15f, 0.14f, 1f);

	[Header("Alignment")]
	public bool setAlignment;

	[HideInInspectorIf("_hideAlignment", false)]
	public TextAlignmentOptions alignment = TextAlignmentOptions.Left;

	[Header("Styles")]
	public bool setStyle;

	public FontStyles[] styles;

	[Header("Wrapping")]
	public bool setWrapping;

	[HideInInspectorIf("_hideWrapping", false)]
	public bool enableWordWrapping;

	[HideInInspectorIf("_hideWrapping", false)]
	public TextOverflowModes overflowMode = TextOverflowModes.Ellipsis;

	[Header("Margins")]
	public bool setMargin;

	[HideInInspectorIf("_hideMargin", false)]
	public float marginLeft;

	[HideInInspectorIf("_hideMargin", false)]
	public float marginTop;

	[HideInInspectorIf("_hideMargin", false)]
	public float marginRight;

	[HideInInspectorIf("_hideMargin", false)]
	public float marginBottom;

	[Header("Spacing")]
	public bool setSpacing;

	[HideInInspectorIf("_hideSpacing", false)]
	public float characterSpacing;

	public static TextMeshProSkin Default
	{
		get
		{
			if (!_Default)
			{
				return _Default = Resources.Load<TextMeshProSkin>("UI/Skins/TextMesh Skins/Default");
			}
			return _Default;
		}
	}

	public Vector4 margin => new Vector4(marginLeft, marginTop, marginRight, marginBottom);

	private TextMeshProSkin _GetSkin(Func<TextMeshProSkin, bool> valid)
	{
		HashSet<TextMeshProSkin> hashSet = new HashSet<TextMeshProSkin>();
		TextMeshProSkin textMeshProSkin = this;
		while ((bool)textMeshProSkin.fallbackSkin && !valid(textMeshProSkin) && hashSet.Add(textMeshProSkin.fallbackSkin))
		{
			textMeshProSkin = textMeshProSkin.fallbackSkin;
		}
		return textMeshProSkin;
	}

	public override bool ApplyTo(UnityEngine.Object textObj)
	{
		TextMeshProUGUI textMeshProUGUI = textObj as TextMeshProUGUI;
		if (!textMeshProUGUI || textMeshProUGUI == null)
		{
			return false;
		}
		bool changed = false;
		TextMeshProSkin textMeshProSkin = _GetSkin((TextMeshProSkin skin) => skin.setFontSize);
		if (textMeshProSkin.setFontSize)
		{
			textMeshProUGUI.enableAutoSizing = _SetValue(textMeshProUGUI.enableAutoSizing, textMeshProSkin.enableAutoSizing, ref changed);
			textMeshProUGUI.fontSizeMin = _SetValue(textMeshProUGUI.fontSizeMin, textMeshProSkin.fontSizeMin, ref changed);
			textMeshProUGUI.fontSizeMax = _SetValue(textMeshProUGUI.fontSizeMax, textMeshProSkin.fontSizeMax, ref changed);
			textMeshProUGUI.fontSize = _SetValue(textMeshProUGUI.fontSize, textMeshProSkin.fontSizeMax, ref changed);
		}
		TextMeshProSkin textMeshProSkin2 = _GetSkin((TextMeshProSkin skin) => skin.setColor);
		if (textMeshProSkin2.setColor)
		{
			textMeshProUGUI.color = _SetValue(textMeshProUGUI.color, textMeshProSkin2.textColor, ref changed);
		}
		TextMeshProSkin textMeshProSkin3 = _GetSkin((TextMeshProSkin skin) => skin.fontAsset);
		if ((bool)textMeshProSkin3.fontAsset)
		{
			textMeshProUGUI.font = _SetValue(textMeshProUGUI.font, textMeshProSkin3.fontAsset, ref changed);
		}
		TextMeshProSkin textMeshProSkin4 = _GetSkin((TextMeshProSkin skin) => skin.fontMaterial);
		if ((bool)textMeshProSkin4.fontMaterial)
		{
			textMeshProUGUI.fontMaterial = _SetValue(textMeshProUGUI.fontSharedMaterial, textMeshProSkin4.fontMaterial, ref changed);
		}
		TextMeshProSkin textMeshProSkin5 = _GetSkin((TextMeshProSkin skin) => skin.setAlignment);
		if (textMeshProSkin5.setAlignment)
		{
			textMeshProUGUI.alignment = _SetValue(textMeshProUGUI.alignment, textMeshProSkin5.alignment, ref changed);
		}
		TextMeshProSkin textMeshProSkin6 = _GetSkin((TextMeshProSkin skin) => skin.setStyle && skin.styles != null);
		if (textMeshProSkin6.setStyle && textMeshProSkin6.styles != null)
		{
			FontStyles fontStyles = EnumUtil<FontStyles>.NoFlags;
			FontStyles[] array = textMeshProSkin6.styles;
			foreach (FontStyles fontStyles2 in array)
			{
				fontStyles |= fontStyles2;
			}
			textMeshProUGUI.fontStyle = _SetValue(textMeshProUGUI.fontStyle, fontStyles, ref changed);
		}
		TextMeshProSkin textMeshProSkin7 = _GetSkin((TextMeshProSkin skin) => skin.setWrapping);
		if (textMeshProSkin7.setWrapping)
		{
			textMeshProUGUI.enableWordWrapping = _SetValue(textMeshProUGUI.enableWordWrapping, textMeshProSkin7.enableWordWrapping, ref changed);
			textMeshProUGUI.overflowMode = _SetValue(textMeshProUGUI.overflowMode, textMeshProSkin7.overflowMode, ref changed);
		}
		TextMeshProSkin textMeshProSkin8 = _GetSkin((TextMeshProSkin skin) => skin.setMargin);
		if (textMeshProSkin8.setMargin)
		{
			textMeshProUGUI.margin = _SetValue(textMeshProUGUI.margin, textMeshProSkin8.margin, ref changed);
		}
		TextMeshProSkin textMeshProSkin9 = _GetSkin((TextMeshProSkin skin) => skin.setSpacing);
		if (textMeshProSkin9.setSpacing)
		{
			textMeshProUGUI.characterSpacing = textMeshProSkin9.characterSpacing;
		}
		return changed;
	}
}
