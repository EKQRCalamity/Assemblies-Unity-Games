using Framework.Achievements;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class AchievementElementWidget : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Widgets", true, false, 0)]
	private Image ElementImage;

	[SerializeField]
	[BoxGroup("Widgets", true, false, 0)]
	private Text Header;

	[SerializeField]
	[BoxGroup("Widgets", true, false, 0)]
	private Localize HeaderLoc;

	[SerializeField]
	[BoxGroup("Widgets", true, false, 0)]
	private Text Content;

	[SerializeField]
	[BoxGroup("Widgets", true, false, 0)]
	private Localize ContentLoc;

	[SerializeField]
	[BoxGroup("Widgets", true, false, 0)]
	private GameObject NoHiddenRoot;

	[SerializeField]
	[BoxGroup("Widgets", true, false, 0)]
	private GameObject HiddenRoot;

	[SerializeField]
	[BoxGroup("Colors", true, false, 0)]
	private Color HeaderLocked;

	[SerializeField]
	[BoxGroup("Colors", true, false, 0)]
	private Color ContentLocked;

	[SerializeField]
	[BoxGroup("Colors", true, false, 0)]
	private Color IconLocked;

	[SerializeField]
	[BoxGroup("Colors", true, false, 0)]
	private Color HeaderUnlocked;

	[SerializeField]
	[BoxGroup("Colors", true, false, 0)]
	private Color ContentUnlocked;

	[SerializeField]
	[BoxGroup("Background", true, false, 0)]
	private Sprite BgLocked;

	[SerializeField]
	[BoxGroup("Background", true, false, 0)]
	private Sprite BgUnlocked;

	public void SetData(Achievement achievement)
	{
		HiddenRoot.SetActive(achievement.CurrentStatus == Achievement.Status.HIDDEN);
		NoHiddenRoot.SetActive(achievement.CurrentStatus != Achievement.Status.HIDDEN);
		ElementImage.sprite = achievement.Image;
		HeaderLoc.SetTerm(achievement.GetNameLocalizationTerm());
		ContentLoc.SetTerm(achievement.GetDescLocalizationTerm());
		Header.color = ((achievement.CurrentStatus != 0) ? HeaderUnlocked : HeaderLocked);
		Content.color = ((achievement.CurrentStatus != 0) ? ContentUnlocked : ContentLocked);
		ElementImage.color = ((achievement.CurrentStatus != 0) ? Color.white : IconLocked);
		GetComponent<Image>().sprite = ((achievement.CurrentStatus != Achievement.Status.UNLOCKED) ? BgLocked : BgUnlocked);
	}
}
