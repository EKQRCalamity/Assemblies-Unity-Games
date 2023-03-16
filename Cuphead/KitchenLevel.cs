using System;
using System.Collections;
using UnityEngine;

public class KitchenLevel : Level
{
	private LevelProperties.Kitchen properties;

	private const int DIALOGUER_VAR_ID = 23;

	[Header("Boss Info")]
	[SerializeField]
	private Sprite _bossPortrait;

	[SerializeField]
	[Multiline]
	private string _bossQuote;

	[SerializeField]
	private GameObject beforeGettingIngredients;

	[SerializeField]
	private GameObject afterGettingIngredients;

	[SerializeField]
	private SpriteRenderer[] sunbeams;

	[SerializeField]
	private float sunbeamCycleSpeed = 2f;

	[SerializeField]
	private Animator saltbakerShadow;

	[SerializeField]
	private Transform triggerEndGame;

	private bool trapDoorOpen;

	[SerializeField]
	private SpriteRenderer trapDoorOverlay;

	private bool forceUnlockSaltbakerBattle;

	[SerializeField]
	private GameObject kitchenBG;

	[SerializeField]
	private GameObject basementBG;

	[SerializeField]
	private Material playerBasementMaterial;

	[SerializeField]
	private Transform[] torchPositions;

	public override Levels CurrentLevel => Levels.Kitchen;

	public override Scenes CurrentScene => Scenes.scene_level_kitchen;

	public override Sprite BossPortrait => _bossPortrait;

	public override string BossQuote => _bossQuote;

	protected override void PartialInit()
	{
		properties = LevelProperties.Kitchen.GetMode(base.mode);
		properties.OnStateChange += base.zHack_OnStateChanged;
		properties.OnBossDeath += base.zHack_OnWin;
		base.timeline = properties.CreateTimeline(base.mode);
		goalTimes = properties.goalTimes;
		properties.OnBossDamaged += base.timeline.DealDamage;
		base.PartialInit();
	}

	protected override void Start()
	{
		base.Start();
		CheckIfBossesCompleted();
		StartCoroutine(check_camera_cr());
		StartCoroutine(cycle_sunbeams_cr());
		beforeGettingIngredients.SetActive(!trapDoorOpen);
		afterGettingIngredients.SetActive(trapDoorOpen);
		AddDialoguerEvents();
		PlayerManager.OnPlayerJoinedEvent += SetPlayerBasementMaterial;
	}

	protected override void OnDestroy()
	{
		RemoveDialoguerEvents();
		PlayerManager.OnPlayerJoinedEvent -= SetPlayerBasementMaterial;
		base.OnDestroy();
	}

	private void SetPlayerBasementMaterial(PlayerId p)
	{
		if (!basementBG.activeInHierarchy)
		{
			return;
		}
		SpriteRenderer[] componentsInChildren = PlayerManager.GetPlayer(p).GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if (spriteRenderer.material.name == "Sprites-Default (Instance)" || (spriteRenderer.sharedMaterial.name == "ChaliceRecolor (Instance)" && spriteRenderer.sharedMaterial.GetFloat("_RecolorFactor") == 0f))
			{
				spriteRenderer.material = playerBasementMaterial;
				spriteRenderer.color = new Color(0.7137255f, 0.4862745f, 11f / 85f);
			}
		}
	}

	public void AddDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent += OnDialoguerMessageEvent;
	}

	public void RemoveDialoguerEvents()
	{
		Dialoguer.events.onMessageEvent -= OnDialoguerMessageEvent;
	}

	private void OnDialoguerMessageEvent(string message, string metadata)
	{
		if (message == "MetSaltbaker")
		{
			PlayerData.SaveCurrentFile();
		}
	}

	private void CheckIfBossesCompleted()
	{
		if (PlayerData.Data.CheckLevelsHaveMinDifficulty(Level.worldDLCBossLevels, Mode.Normal))
		{
			trapDoorOpen = true;
			StartCoroutine(check_trigger_cr());
		}
		else
		{
			trapDoorOpen = false;
		}
	}

	protected override void OnLevelStart()
	{
		if (Dialoguer.GetGlobalFloat(23) == 1f)
		{
			AudioManager.Play("sfx_dlc_bakery_doorenter");
		}
		if (trapDoorOpen)
		{
			AudioManager.StartBGMAlternate(1);
		}
		else if (PlayerData.Data.pianoAudioEnabled)
		{
			AudioManager.StartBGMAlternate(2);
		}
		else
		{
			AudioManager.PlayBGM();
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		if (triggerEndGame != null)
		{
			Vector2 vector = new Vector2(triggerEndGame.position.x, triggerEndGame.position.y + 1000f);
			Vector2 vector2 = new Vector2(triggerEndGame.position.x, triggerEndGame.position.y - 1000f);
			Gizmos.DrawLine(vector, vector2);
		}
	}

	private IEnumerator check_trigger_cr()
	{
		AbstractPlayerController player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		bool hasntPassed = true;
		while (hasntPassed)
		{
			if (player1.transform.position.x >= triggerEndGame.position.x)
			{
				hasntPassed = false;
			}
			if (player2 != null && player2.transform.position.x >= triggerEndGame.position.x)
			{
				hasntPassed = false;
			}
			yield return null;
		}
		PlayerManager.playerWasChalice[0] = player1.stats.isChalice;
		PlayerManager.playerWasChalice[1] = player2 != null && player2.stats.isChalice;
		if (Level.CurrentMode == Mode.Easy)
		{
			Level.SetCurrentMode(Mode.Normal);
		}
		Cutscene.Load(Scenes.scene_level_saltbaker, Scenes.scene_cutscene_dlc_saltbaker_prebattle, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade);
	}

	private IEnumerator check_camera_cr()
	{
		camera.mode = CupheadLevelCamera.Mode.Relative;
		bool inPit = false;
		AbstractPlayerController player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
		AbstractPlayerController player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
		float lastP1YPos = 0f;
		float lastP2YPos = 0f;
		while (!inPit)
		{
			player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
			player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
			if (player2 != null && !player2.IsDead)
			{
				inPit = player2.transform.position.y < -400f && player1.transform.position.y < -400f;
				if (!inPit)
				{
					if (player1.transform.position.y < -400f)
					{
						player1.gameObject.SetActive(value: false);
					}
					if (player2.transform.position.y < -400f)
					{
						player2.gameObject.SetActive(value: false);
					}
				}
			}
			else
			{
				inPit = player1.transform.position.y < -400f;
			}
			if (player1 != null && Mathf.Sign(player1.transform.position.y + 208f) != Mathf.Sign(lastP1YPos + 208f))
			{
				SpriteRenderer[] componentsInChildren = player1.GetComponentsInChildren<SpriteRenderer>();
				foreach (SpriteRenderer spriteRenderer in componentsInChildren)
				{
					spriteRenderer.sortingLayerName = ((!(player1.transform.position.y < -208f)) ? "Player" : "Enemies");
				}
				lastP1YPos = player1.transform.position.y;
			}
			if (player2 != null && Mathf.Sign(player2.transform.position.y + 208f) != Mathf.Sign(lastP2YPos + 208f))
			{
				SpriteRenderer[] componentsInChildren2 = player2.GetComponentsInChildren<SpriteRenderer>();
				foreach (SpriteRenderer spriteRenderer2 in componentsInChildren2)
				{
					spriteRenderer2.sortingLayerName = ((!(player2.transform.position.y < -208f)) ? "Player" : "Enemies");
				}
				lastP2YPos = player1.transform.position.y;
			}
			yield return null;
		}
		kitchenBG.SetActive(value: false);
		basementBG.SetActive(value: true);
		AudioManager.FadeSFXVolume("sfx_dlc_bakery_basementamb_loop", 0.0001f, 0.0001f);
		AudioManager.PlayLoop("sfx_dlc_bakery_basementamb_loop");
		AudioManager.PlayLoop("sfx_dlc_bakery_basementtorch_loop");
		afterGettingIngredients.SetActive(value: false);
		CupheadLevelCamera.Current.ChangeHorizontalBounds(740, 3500);
		Level.Current.SetBounds(680, 6860, null, null);
		CupheadLevelCamera.Current.ChangeCameraMode(CupheadLevelCamera.Mode.Lerp);
		CupheadLevelCamera.Current.LERP_SPEED = 5f;
		CupheadLevelCamera.Current.SetPosition(new Vector3(-100f, 0f));
		player1.transform.position = new Vector3(-500f, 800f);
		player1.gameObject.SetActive(value: true);
		if (player2 != null && !player2.IsDead)
		{
			player2.transform.position = new Vector3(-400f, 800f);
			player2.gameObject.SetActive(value: true);
		}
		foreach (AbstractPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (allPlayer != null)
			{
				SetPlayerBasementMaterial(allPlayer.id);
				SpriteRenderer[] componentsInChildren3 = allPlayer.GetComponentsInChildren<SpriteRenderer>();
				foreach (SpriteRenderer spriteRenderer3 in componentsInChildren3)
				{
					spriteRenderer3.sortingLayerName = "Player";
				}
			}
		}
		AudioManager.StartBGMAlternate(0);
		AudioManager.FadeSFXVolume("sfx_dlc_bakery_basementamb_loop", 0.5f, 1f);
		while (CupheadLevelCamera.Current.transform.position.x < 2320f)
		{
			HandleTorchSFX();
			yield return null;
		}
		saltbakerShadow.SetTrigger("Continue");
		CupheadLevelCamera.Current.ChangeCameraMode(CupheadLevelCamera.Mode.Platforming);
		AudioManager.Play("sfx_dlc_saltbaker_evilbasementlaugh");
		while (CupheadLevelCamera.Current.transform.position.x < 2800f)
		{
			HandleTorchSFX();
			yield return null;
		}
		saltbakerShadow.SetTrigger("Continue");
	}

	private void HandleTorchSFX()
	{
		float num = float.MaxValue;
		float num2 = 0f;
		Transform[] array = torchPositions;
		foreach (Transform transform in array)
		{
			foreach (AbstractPlayerController allPlayer in PlayerManager.GetAllPlayers())
			{
				if (allPlayer != null)
				{
					float num3 = Mathf.Abs(allPlayer.center.x - transform.position.x);
					if (num3 < num)
					{
						num2 = Mathf.Sign(transform.position.x - allPlayer.center.x);
						num = num3;
					}
				}
			}
		}
		float num4 = Mathf.InverseLerp(320f, 0f, num);
		float value = num2 * (1f - num4);
		AudioManager.FadeSFXVolume("sfx_dlc_bakery_basementtorch_loop", Mathf.Lerp(0.01f, 0.8f, num4), 0.0001f);
		AudioManager.Pan("sfx_dlc_bakery_basementtorch_loop", value);
	}

	private IEnumerator cycle_sunbeams_cr()
	{
		float t = 0f;
		while (true)
		{
			for (int i = 0; i < 3; i++)
			{
				sunbeams[i].color = new Color(1f, 1f, 1f, (Mathf.Sin((float)i * ((float)Math.PI * 2f / 3f) + t) + 1f) / 2f);
			}
			t += (float)CupheadTime.Delta * sunbeamCycleSpeed;
			yield return null;
		}
	}
}
