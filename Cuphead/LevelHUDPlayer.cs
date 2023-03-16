using System.Collections;
using RektTransform;
using UnityEngine;
using UnityEngine.Serialization;

public class LevelHUDPlayer : AbstractPausableComponent
{
	[SerializeField]
	private LevelHUDPlayerHealth health;

	[SerializeField]
	private LevelHUDPlayerSuper super;

	[Space(10f)]
	[SerializeField]
	private RectTransform weaponRoot;

	[SerializeField]
	[FormerlySerializedAs("weaponPrefab")]
	private LevelHUDWeapon weaponIconPrefab;

	[SerializeField]
	private CanvasGroup weaponSwitchNotification;

	public float weaponSwitchWobbleSpeed = 1f;

	public float weaponSwitchWobbleScale = 1f;

	private RectTransform weaponSwitchTransform;

	private Vector2 weaponSwitchStartPosition;

	public AbstractPlayerController player { get; private set; }

	public void Init(AbstractPlayerController player, bool startAtOneHealth = false)
	{
		this.player = player;
		if (player.id == PlayerId.PlayerTwo)
		{
			SetupPlayerTwo();
		}
		player.stats.OnHealthChangedEvent += OnHealthChanged;
		player.stats.OnSuperChangedEvent += OnSuperChanged;
		player.stats.OnWeaponChangedEvent += OnWeaponChanged;
		health.Init(this);
		if (startAtOneHealth)
		{
			health.OnHealthChanged(1);
		}
		super.Init(this);
		if (player as PlanePlayerController != null)
		{
			weaponSwitchNotification.gameObject.SetActive(PlayerData.Data.Loadouts.GetPlayerLoadout(player.id).MustNotifySwitchSHMUPWeapon && !Level.IsTowerOfPowerMain);
		}
		else
		{
			weaponSwitchNotification.gameObject.SetActive(PlayerData.Data.Loadouts.GetPlayerLoadout(player.id).MustNotifySwitchRegularWeapon && !Level.IsTowerOfPowerMain);
		}
		weaponSwitchNotification.alpha = 1f;
		weaponSwitchTransform = weaponSwitchNotification.GetComponent<RectTransform>();
		weaponSwitchStartPosition = weaponSwitchTransform.anchoredPosition;
	}

	private void SetupPlayerTwo()
	{
		base.gameObject.name = "Mugman";
		Vector3 localPosition = base.transform.localPosition;
		localPosition.x *= -1f;
		base.rectTransform.SetAnchors(new RektTransform.MinMax(new Vector2(1f, 0f), new Vector2(1f, 0f)));
		base.rectTransform.pivot = new Vector2(1f, 0f);
		base.transform.localPosition = localPosition;
		localPosition = health.rectTransform.localPosition;
		localPosition.x *= -1f;
		health.rectTransform.localPosition = localPosition;
		weaponRoot.localPosition = localPosition;
		localPosition = super.rectTransform.localPosition;
		localPosition.x *= -1f;
		super.rectTransform.SetScale(-1f);
		super.rectTransform.localPosition = localPosition;
	}

	private void OnHealthChanged(int health, PlayerId playerId)
	{
		this.health.OnHealthChanged(health);
	}

	private void OnSuperChanged(float super, PlayerId playerId, bool playEffect)
	{
		this.super.OnSuperChanged(super);
	}

	private void OnWeaponChanged(Weapon weapon)
	{
		weaponIconPrefab.Create(weaponRoot, weapon);
		if (weaponSwitchNotification.gameObject.activeSelf)
		{
			bool flag = PlayerData.Data.Loadouts.GetPlayerLoadout(player.id).MustNotifySwitchSHMUPWeapon || PlayerData.Data.Loadouts.GetPlayerLoadout(player.id).MustNotifySwitchRegularWeapon;
			if (player as PlanePlayerController != null)
			{
				PlayerData.Data.Loadouts.GetPlayerLoadout(player.id).MustNotifySwitchSHMUPWeapon = false;
			}
			else
			{
				PlayerData.Data.Loadouts.GetPlayerLoadout(player.id).MustNotifySwitchRegularWeapon = false;
			}
			if (flag)
			{
				PlayerData.SaveCurrentFile();
			}
			StartCoroutine(FadeOutSwitchNotification(0.4f));
		}
	}

	private void Update()
	{
		if (weaponSwitchNotification.gameObject.activeSelf)
		{
			weaponSwitchTransform.anchoredPosition = weaponSwitchStartPosition + Vector2.up * Mathf.Sin(Time.time * weaponSwitchWobbleSpeed) * weaponSwitchWobbleScale;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		health = null;
		super = null;
	}

	private IEnumerator FadeOutSwitchNotification(float overTime)
	{
		if (overTime > 0f)
		{
			float startAlpha = weaponSwitchNotification.alpha;
			float timeSpent = 0f;
			while (timeSpent < overTime)
			{
				weaponSwitchNotification.alpha = Mathf.Lerp(startAlpha, 0f, timeSpent / overTime);
				timeSpent += Time.deltaTime;
				yield return null;
			}
			weaponSwitchNotification.gameObject.SetActive(value: false);
		}
	}
}
