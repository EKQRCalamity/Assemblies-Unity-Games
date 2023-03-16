using TMPro;
using UnityEngine;

public class MapBasicStartUI : AbstractMapSceneStartUI
{
	public Animator Animator;

	public TMP_Text Title;

	[SerializeField]
	private RectTransform cursor;

	[Header("Options")]
	[SerializeField]
	private RectTransform enter;

	public static MapBasicStartUI Current { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		Current = this;
	}

	private void OnDestroy()
	{
		if (Current == this)
		{
			Current = null;
		}
	}

	private void UpdateCursor()
	{
		cursor.transform.position = enter.transform.position;
		cursor.sizeDelta = new Vector2(enter.sizeDelta.x + 30f, enter.sizeDelta.y + 20f);
	}

	private void Update()
	{
		UpdateCursor();
		if (base.CurrentState == State.Active)
		{
			CheckInput();
		}
	}

	private void CheckInput()
	{
		if (base.Able)
		{
			if (GetButtonDown(CupheadButton.Cancel))
			{
				Out();
			}
			if (GetButtonDown(CupheadButton.Accept))
			{
				LoadLevel();
			}
		}
	}

	public new void In(MapPlayerController playerController)
	{
		base.In(playerController);
		if (Animator != null)
		{
			Animator.SetTrigger("ZoomIn");
			AudioManager.Play("world_map_level_menu_open");
		}
		InitUI(level);
	}

	public void InitUI(string level)
	{
		TranslationElement translationElement = Localization.Find(level);
		if (translationElement != null)
		{
			Title.GetComponent<LocalizationHelper>().ApplyTranslation(translationElement);
			if (Localization.language == Localization.Languages.Japanese)
			{
				Title.lineSpacing = 0f;
			}
			else
			{
				Title.lineSpacing = 17.46f;
			}
		}
	}
}
