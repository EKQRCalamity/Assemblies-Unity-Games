using TMPro;
using UnityEngine;

public class CharacterCardLockView : MonoBehaviour
{
	private Player _player;

	private void _UpdateUnlockIndex(DataRef<GameData> game)
	{
		int? num = game?.data.GetCharacterUnlockIndex(_player.characterDataRef);
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			GetComponentInChildren<TextMeshProUGUI>().text = valueOrDefault.ToString();
		}
	}

	private void OnEnable()
	{
		ProfilePrefs.OnSelectedGameChange += _UpdateUnlockIndex;
	}

	private void OnDisable()
	{
		ProfilePrefs.OnSelectedGameChange -= _UpdateUnlockIndex;
	}

	public CharacterCardLockView SetData(Player player)
	{
		_player = player;
		_UpdateUnlockIndex(ProfileManager.prefs.selectedGame);
		return this;
	}
}
