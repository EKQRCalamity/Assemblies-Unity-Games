using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CupheadGlyph : MonoBehaviour
{
	public enum LetterOffset
	{
		Normal,
		Small
	}

	public enum PlatformGlyphType
	{
		Normal,
		TutorialInstruction,
		TutorialInstructionDescend,
		LevelUIInteractionDialogue,
		Shop,
		SwitchWeapon,
		ShmupTutorial,
		Equip,
		OffsetPrompt
	}

	public static readonly char NintendoSwitchUp = '{';

	public static readonly char NintendoSwitchDown = '}';

	public static readonly char NintendoSwitchLeft = '<';

	public static readonly char NintendoSwitchRight = '>';

	public static readonly char PlayStation4Cross = '†';

	public static readonly char PlayStation4Circle = '‡';

	public static readonly char PlayStation4Square = '°';

	public static readonly char PlayStation4Triangle = '~';

	private static readonly int NintendoSwitchFontSize = 24;

	private static readonly Color NintendoSwitchColor = Color.white;

	private static readonly Color NintendoSwitchTutorialInstructionColor = new Color(13f / 51f, 13f / 51f, 13f / 51f, 1f);

	private static readonly Vector2[] letterSpecificOffset = new Vector2[26]
	{
		new Vector2(1.6f, -0.4f),
		new Vector2(1.29f, -0.85f),
		new Vector2(1.3f, -1f),
		new Vector2(1.81f, -1.18f),
		new Vector2(0.9f, -1f),
		new Vector2(0.9f, -1f),
		new Vector2(1.2f, -1f),
		new Vector2(1f, -1f),
		new Vector2(0.8f, -1.2f),
		new Vector2(1.3f, -1.2f),
		new Vector2(0.5f, -1.2f),
		new Vector2(1.1f, -1f),
		new Vector2(0.8f, -1f),
		new Vector2(1.2f, -1.2f),
		new Vector2(1.1f, -1f),
		new Vector2(1.3f, -1.2f),
		new Vector2(1.1f, 0f),
		new Vector2(1.5f, -1.2f),
		new Vector2(1.5f, -1.2f),
		new Vector2(1.3f, -1.8f),
		new Vector2(0.9f, -1.4f),
		new Vector2(1.35f, -1.6f),
		new Vector2(0.6f, -2f),
		new Vector2(0.8f, -1.3f),
		new Vector2(0.95f, -1.8f),
		new Vector2(1.6f, -1f)
	};

	private static readonly Vector2[] letterSpecificSmallOffset = new Vector2[26]
	{
		new Vector2(1.2f, 0f),
		new Vector2(0.5f, -0.2f),
		new Vector2(0.9f, -0.6f),
		new Vector2(1.1f, -0.3f),
		new Vector2(0.32f, -0.27f),
		new Vector2(0.32f, -0.85f),
		new Vector2(0.93f, -0.64f),
		new Vector2(0.64f, -0.56f),
		new Vector2(0.69f, -0.56f),
		new Vector2(0.53f, -0.38f),
		new Vector2(1.01f, -0.38f),
		new Vector2(0.77f, -0.19f),
		new Vector2(0.93f, -0.49f),
		new Vector2(0.79f, -0.67f),
		new Vector2(0.92f, -0.47f),
		new Vector2(1.34f, -0.44f),
		new Vector2(0.97f, 0.63f),
		new Vector2(1.01f, -0.3f),
		new Vector2(0.81f, -0.8f),
		new Vector2(0.48f, -1.02f),
		new Vector2(0.23f, -0.81f),
		new Vector2(0.44f, -0.81f),
		new Vector2(0.94f, -1.36f),
		new Vector2(1.19f, -0.73f),
		new Vector2(1.19f, -0.62f),
		new Vector2(0.89f, -0.62f)
	};

	private static readonly Vector2[] ps4SmallOffset = new Vector2[4]
	{
		new Vector2(0.69f, -0.48f),
		new Vector2(0.97f, -0.38f),
		new Vector2(0.91f, -0.34f),
		new Vector2(1.11f, 0.41f)
	};

	protected Vector2[] ps4NormalOffset = new Vector2[4]
	{
		new Vector2(1.74f, -1f),
		new Vector2(2.27f, -0.97f),
		new Vector2(2.78f, -1.13f),
		new Vector2(1.55f, 0.55f)
	};

	private static readonly Dictionary<int, Vector2> SwitchOffsetMapping = new Dictionary<int, Vector2>
	{
		{
			0,
			new Vector2(-0.97f, -1f)
		},
		{
			1,
			new Vector2(0f, 9.06f)
		},
		{
			2,
			new Vector2(3.24f, 9.06f)
		},
		{
			3,
			new Vector2(-8.3f, -0.8f)
		},
		{
			4,
			new Vector2(0f, 9.06f)
		},
		{
			5,
			new Vector2(-1.61f, -1f)
		},
		{
			6,
			new Vector2(-0.97f, 9.06f)
		},
		{
			7,
			new Vector2(0f, 0f)
		},
		{
			8,
			new Vector2(0f, 6f)
		}
	};

	private static readonly Dictionary<int, Vector2> PlayStation4OffsetMapping = new Dictionary<int, Vector2>
	{
		{
			0,
			new Vector2(1.1f, -1f)
		},
		{
			1,
			new Vector2(0.8f, -0.4f)
		},
		{
			2,
			new Vector2(0.8f, -0.4f)
		},
		{
			3,
			new Vector2(1f, -1f)
		},
		{
			4,
			new Vector2(0.1f, -0.1f)
		},
		{
			5,
			new Vector2(0.3f, -0.35f)
		},
		{
			6,
			new Vector2(0.5f, -0.4f)
		},
		{
			7,
			new Vector2(0.5f, 0f)
		},
		{
			8,
			new Vector2(1.2f, -1.09f)
		}
	};

	protected const float PADDINGH = 25f;

	public int rewiredPlayerId;

	public CupheadButton button;

	[SerializeField]
	private Image glyphSymbolText;

	[SerializeField]
	private Text glyphText;

	[SerializeField]
	private Image glyphSymbolChar;

	[SerializeField]
	private Text glyphChar;

	[SerializeField]
	private RectTransform[] rectTransformTexts;

	[SerializeField]
	protected Vector2 startSize = new Vector2(37f, 37f);

	[SerializeField]
	protected float paddingText = 10.7f;

	[SerializeField]
	private float maxSize;

	[SerializeField]
	private LetterOffset letterOffset;

	[SerializeField]
	private PlatformGlyphType platformGlyphType;

	[SerializeField]
	private CustomLanguageLayout[] glyphLayouts;

	private int initialFontSize;

	private Color initialCharColor;

	private Vector3 initialScale;

	private VerticalWrapMode initialCharWrapMode;

	private Material initialCharMaterial;

	public float preferredWidth => Mathf.Max(glyphText.preferredWidth + paddingText, glyphChar.rectTransform.sizeDelta.y);

	private void Awake()
	{
		initialFontSize = glyphChar.fontSize;
		initialCharColor = glyphChar.color;
		initialScale = base.transform.localScale;
		initialCharWrapMode = glyphChar.verticalOverflow;
		if (platformGlyphType == PlatformGlyphType.TutorialInstruction || platformGlyphType == PlatformGlyphType.TutorialInstructionDescend || platformGlyphType == PlatformGlyphType.Shop || platformGlyphType == PlatformGlyphType.ShmupTutorial)
		{
			initialCharMaterial = glyphChar.material;
		}
	}

	private void Start()
	{
		Init();
		PlayerManager.OnControlsChanged += OnControlsChanged;
		Localization.OnLanguageChangedEvent += OnLanguageChanged;
	}

	private void OnLanguageChanged()
	{
		Init();
	}

	private void OnControlsChanged()
	{
		Init();
	}

	public void Init()
	{
		Localization.Translation translation = CupheadInput.InputDisplayForButton(button, rewiredPlayerId);
		AlignDashInstructions(translation);
		string text = translation.text;
		bool flag = text.Length > 1;
		glyphSymbolText.gameObject.SetActive(flag);
		glyphText.gameObject.SetActive(flag);
		glyphChar.gameObject.SetActive(!flag);
		glyphSymbolChar.gameObject.SetActive(!flag);
		glyphText.text = text;
		glyphChar.text = text;
		glyphText.font = ((translation.fonts == null) ? Localization.Instance.fonts[(int)Localization.language][29].font : translation.fonts.font);
		for (int i = 0; i < rectTransformTexts.Length; i++)
		{
			if (flag)
			{
				float num = preferredWidth;
				if (maxSize > 0f && num > maxSize)
				{
					num = maxSize;
				}
				rectTransformTexts[i].sizeDelta = new Vector2(num, rectTransformTexts[i].sizeDelta.y);
				continue;
			}
			RectTransform component = glyphChar.GetComponent<RectTransform>();
			if (component != null)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(text);
				if (bytes.Length > 0)
				{
					int num2 = bytes[0] - 65;
					if (letterOffset == LetterOffset.Normal)
					{
						if (num2 >= 0 && num2 < letterSpecificOffset.Length)
						{
							component.anchoredPosition = letterSpecificOffset[num2];
						}
						else
						{
							num2 = PS4CharToIndex((char)bytes[0]);
							if (num2 >= 0)
							{
								component.anchoredPosition = ps4NormalOffset[num2];
							}
						}
					}
					else if (num2 >= 0 && num2 < letterSpecificSmallOffset.Length)
					{
						component.anchoredPosition = letterSpecificSmallOffset[num2];
					}
					else
					{
						num2 = PS4CharToIndex((char)bytes[0]);
						if (num2 >= 0)
						{
							component.anchoredPosition = ps4SmallOffset[num2];
						}
					}
				}
			}
			rectTransformTexts[i].sizeDelta = new Vector2(Mathf.Max(preferredWidth, rectTransformTexts[i].sizeDelta.y), rectTransformTexts[i].sizeDelta.y);
		}
		LayoutElement component2 = GetComponent<LayoutElement>();
		if (component2 != null)
		{
			component2.preferredWidth = ((!flag) ? (preferredWidth - paddingText) : preferredWidth);
		}
		if (flag && maxSize > 0f)
		{
			glyphText.resizeTextMaxSize = glyphText.fontSize * 4;
			glyphText.resizeTextForBestFit = true;
			RectTransform component3 = glyphText.GetComponent<RectTransform>();
			component3.sizeDelta *= 4f;
			component3.localScale = Vector3.one * 0.25f;
		}
	}

	private void OnDestroy()
	{
		PlayerManager.OnControlsChanged -= OnControlsChanged;
		Localization.OnLanguageChangedEvent -= OnLanguageChanged;
	}

	private int PS4CharToIndex(char c)
	{
		return -1;
	}

	private int SwitchCharToIndex(char c)
	{
		if (c == NintendoSwitchUp)
		{
			return 0;
		}
		if (c == NintendoSwitchDown)
		{
			return 1;
		}
		if (c == NintendoSwitchLeft)
		{
			return 2;
		}
		if (c == NintendoSwitchRight)
		{
			return 3;
		}
		return -1;
	}

	private void SetSwitchGlyph(bool isSwitchGlyph, RectTransform rectTransform)
	{
		if (isSwitchGlyph)
		{
			glyphSymbolChar.gameObject.SetActive(value: false);
			glyphChar.fontSize = NintendoSwitchFontSize;
			glyphChar.color = NintendoSwitchColor;
			glyphChar.verticalOverflow = VerticalWrapMode.Overflow;
			if (platformGlyphType == PlatformGlyphType.TutorialInstruction || platformGlyphType == PlatformGlyphType.TutorialInstructionDescend)
			{
				glyphChar.material = null;
				glyphChar.color = NintendoSwitchTutorialInstructionColor;
			}
			else if (platformGlyphType == PlatformGlyphType.Shop || platformGlyphType == PlatformGlyphType.ShmupTutorial)
			{
				glyphChar.material = null;
			}
			if (platformGlyphType == PlatformGlyphType.SwitchWeapon)
			{
				base.transform.localScale = Vector3.one;
			}
			if (platformGlyphType == PlatformGlyphType.Equip)
			{
				glyphChar.GetComponent<Shadow>().enabled = true;
				glyphChar.GetComponent<Outline>().enabled = true;
			}
			Vector2 anchoredPosition = SwitchOffsetMapping[(int)platformGlyphType];
			rectTransform.anchoredPosition = anchoredPosition;
		}
		else
		{
			glyphSymbolChar.gameObject.SetActive(value: true);
			glyphChar.fontSize = initialFontSize;
			glyphChar.color = initialCharColor;
			glyphChar.verticalOverflow = initialCharWrapMode;
			if (platformGlyphType == PlatformGlyphType.TutorialInstruction || platformGlyphType == PlatformGlyphType.TutorialInstructionDescend || platformGlyphType == PlatformGlyphType.Shop || platformGlyphType == PlatformGlyphType.ShmupTutorial)
			{
				glyphChar.material = initialCharMaterial;
			}
			if (platformGlyphType == PlatformGlyphType.SwitchWeapon)
			{
				base.transform.localScale = initialScale;
			}
			if (platformGlyphType == PlatformGlyphType.Equip)
			{
				glyphChar.GetComponent<Shadow>().enabled = false;
				glyphChar.GetComponent<Outline>().enabled = false;
			}
		}
	}

	public void AlignDashInstructions(Localization.Translation translation)
	{
		if (glyphLayouts != null)
		{
			bool flag = !translation.text.Equals("Y");
			for (int i = 0; i < glyphLayouts.Length; i++)
			{
				glyphLayouts[i].enabled = flag;
			}
		}
	}
}
