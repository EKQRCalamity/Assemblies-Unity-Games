using System;
using System.Collections.Generic;
using FMOD.Studio;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Audio;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Isidora.Audio;

public class IsidoraAudio : EntityAudio
{
	private const string ATTACK_MARKER = "Attack";

	private const string BRIDGE_MARKER = "Bridge";

	private const string PHASE_MARKER = "Phase3";

	public BossAudioSyncHelper bossAudioSync;

	public float lastTimeSpanBetweenBars;

	public float lastBarTime;

	public float lastAttackMarker;

	public int lastAttackMarkerBar;

	public IsidoraBehaviour.ISIDORA_PHASES currentAudioPhase;

	private Dictionary<string, EventInstance> eventRefsByEventId = new Dictionary<string, EventInstance>();

	private const string Isidora_InvisibleDash = "IsidoraInvisibleDash";

	private const string Isidora_SlashAnticipation = "IsidoraSlashAnticipation";

	private const string Isidora_SlashAttack = "IsidoraSlashAttack";

	private const string Isidora_RisingScytheAnticipationFire = "IsidoraRisingScytheAnticipationFire";

	private const string Isidora_RisingScytheAnticipationNoFire = "IsidoraRisingScytheAnticipationNoFire";

	private const string Isidora_RisingScytheSlashFire = "IsidoraRisingScytheSlashFire";

	private const string Isidora_RisingScytheSlashNoFire = "IsidoraRisingScytheSlashNoFire";

	private const string Isidora_FadeSlash = "IsidoraFadeSlash";

	private float timeSinceLevelLoad;

	public event Action<IsidoraAudio> OnBarBegins;

	public event Action<IsidoraAudio> OnNextMarker;

	public event Action<IsidoraAudio> OnAttackMarker;

	private void Awake()
	{
		bossAudioSync = UnityEngine.Object.FindObjectOfType<BossAudioSyncHelper>();
		if ((bool)bossAudioSync)
		{
			bossAudioSync.OnBar += NewBarBegins;
			bossAudioSync.OnMarker += NextMarker;
		}
		currentAudioPhase = IsidoraBehaviour.ISIDORA_PHASES.FIRST;
		Owner = GetComponent<Isidora>();
	}

	public float GetTimeSinceLevelLoad()
	{
		return timeSinceLevelLoad;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		timeSinceLevelLoad += Time.deltaTime;
	}

	private void OnDestroy()
	{
		if ((bool)bossAudioSync)
		{
			bossAudioSync.OnBar -= NewBarBegins;
		}
	}

	private void NextMarker(string marker)
	{
		if (this.OnNextMarker != null)
		{
			this.OnNextMarker(this);
		}
		if (string.Equals(marker, "Attack"))
		{
			if (this.OnAttackMarker != null)
			{
				this.OnAttackMarker(this);
			}
			lastAttackMarker = timeSinceLevelLoad;
			lastAttackMarkerBar = bossAudioSync.LastBar;
		}
		if (string.Equals(marker, "Bridge"))
		{
			currentAudioPhase = IsidoraBehaviour.ISIDORA_PHASES.BRIDGE;
		}
		if (string.Equals(marker, "Phase3"))
		{
			currentAudioPhase = IsidoraBehaviour.ISIDORA_PHASES.SECOND;
		}
	}

	public void SetIsidoraVoice(bool on)
	{
		bossAudioSync.bossfightAudio.SetBossTrackParam("IsidoraVoice", on ? 1 : 0);
	}

	public bool GetIsidoraVoice()
	{
		bossAudioSync.bossfightAudio.GetBossTrackParam("IsidoraVoice").getValue(out var value);
		return value > 0f;
	}

	public void SetSkullsChoir(bool on)
	{
		bossAudioSync.bossfightAudio.SetBossTrackParam("SkullChoir", on ? 1 : 0);
	}

	public void SetPhaseBridge()
	{
		bossAudioSync.bossfightAudio.SetBossTrackParam("BossPhase", 2f);
	}

	public void SetSecondPhase()
	{
		bossAudioSync.bossfightAudio.SetBossTrackParam("BossPhase", 3f);
	}

	private void NewBarBegins()
	{
		lastTimeSpanBetweenBars = timeSinceLevelLoad - lastBarTime;
		lastBarTime = timeSinceLevelLoad;
		if (this.OnBarBegins != null)
		{
			this.OnBarBegins(this);
		}
	}

	public bool IsLastBarValid()
	{
		return bossAudioSync.LastBar % 2 == 1;
	}

	public float GetSingleBarDuration()
	{
		return 2.667f;
	}

	public float GetTimeLeftForCurrentBar()
	{
		return GetSingleBarDuration() - (timeSinceLevelLoad - lastBarTime);
	}

	public float GetTimeUntilNextValidBar()
	{
		float num = GetTimeLeftForCurrentBar();
		if (IsLastBarValid())
		{
			num += GetSingleBarDuration();
		}
		return num;
	}

	public float GetTimeUntilNextAttackAnticipationPeriod()
	{
		return lastAttackMarker + 2f * GetSingleBarDuration() - timeSinceLevelLoad;
	}

	internal void PlayFadeDash()
	{
		PlayOneShot_AUDIO("IsidoraFadeSlash");
	}

	public void PlayInvisibleDash()
	{
		PlayOneShot_AUDIO("IsidoraInvisibleDash");
	}

	public void StopMeleeAudios()
	{
		Debug.Log("<color=red>STOP MELEE AUDIOS</color>");
		Stop_AUDIO("IsidoraSlashAnticipation");
		Stop_AUDIO("IsidoraSlashAttack");
	}

	public void PlayRisingScytheAnticipationLoopAudio()
	{
		Isidora isidora = Owner as Isidora;
		if (isidora.AnimatorInyector.IsScytheOnFire())
		{
			Play_AUDIO("IsidoraRisingScytheAnticipationFire");
		}
		else
		{
			Play_AUDIO("IsidoraRisingScytheAnticipationNoFire");
		}
	}

	public void PlayRisingScytheSlashAudio()
	{
		Isidora isidora = Owner as Isidora;
		if (isidora.AnimatorInyector.IsScytheOnFire())
		{
			Play_AUDIO("IsidoraRisingScytheSlashFire");
		}
		else
		{
			Play_AUDIO("IsidoraRisingScytheSlashNoFire");
		}
	}

	public void PlayOneShot_AUDIO(string eventId, FxSoundCategory category = FxSoundCategory.Attack)
	{
		PlayOneShotEvent(eventId, category);
	}

	public void Play_AUDIO(string eventId)
	{
		if (eventRefsByEventId.TryGetValue(eventId, out var value))
		{
			StopEvent(ref value);
			eventRefsByEventId.Remove(eventId);
		}
		value = default(EventInstance);
		PlayEvent(ref value, eventId, checkSpriteRendererVisible: false);
		eventRefsByEventId[eventId] = value;
	}

	public void Stop_AUDIO(string eventId)
	{
		if (eventRefsByEventId.TryGetValue(eventId, out var value))
		{
			StopEvent(ref value);
			eventRefsByEventId.Remove(eventId);
		}
	}

	public void StopAll()
	{
		Gameplay.GameControllers.Penitent.Penitent penitent = Core.Logic.Penitent;
		penitent.OnDead = (Core.SimpleEvent)Delegate.Remove(penitent.OnDead, new Core.SimpleEvent(StopAll));
		foreach (string key in eventRefsByEventId.Keys)
		{
			EventInstance eventInstance = eventRefsByEventId[key];
			StopEvent(ref eventInstance);
		}
		eventRefsByEventId.Clear();
	}
}
