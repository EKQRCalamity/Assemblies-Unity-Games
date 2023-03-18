using System.Collections;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.BurntFace;
using Gameplay.GameControllers.Effects.Entity;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

public class BlindBabyGrabManager : MonoBehaviour
{
	[FoldoutGroup("Grab attack settings", 0)]
	public float grabWidth;

	[FoldoutGroup("Grab attack settings", 0)]
	public float grabHeight;

	[FoldoutGroup("Grab attack settings", 0)]
	public float anticipationDuration = 3f;

	[FoldoutGroup("Grab attack settings", 0)]
	public float movementDuration = 2f;

	[FoldoutGroup("Grab attack settings", 0)]
	public ContactFilter2D contactFilter;

	public float followSpeed = 1f;

	public Transform grabPointTransform;

	public WickerWurmAudio Audio;

	public Animator grabWarningAnimator;

	private Animator babyAnimator;

	private Vector2 originPoint;

	private Collider2D attackCollider;

	private Collider2D[] results;

	public MasterShaderEffects babyEffects;

	public MasterShaderEffects motherEffects;

	public float hiddenLerpValue = 0.7f;

	private Tween swayTween;

	public bool moveTowardsPenitent;

	private bool hasGrabbedPenitent;

	private void PlayWarning()
	{
		grabWarningAnimator.SetTrigger("ACTIVATE");
	}

	private void Awake()
	{
		results = new Collider2D[1];
		attackCollider = GetComponentInChildren<Collider2D>();
		babyAnimator = GetComponentInChildren<Animator>();
		hasGrabbedPenitent = false;
	}

	public void PlayDeath()
	{
		StopSway();
		grabWarningAnimator.gameObject.SetActive(value: false);
		base.transform.DOKill();
		StopAllCoroutines();
		babyAnimator.SetTrigger("DEATH");
		base.transform.DOMoveY(base.transform.position.y - 10f, 10f);
	}

	public void StartSway()
	{
		swayTween = base.transform.DOMoveY(base.transform.position.y - 1.5f, 5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
		swayTween.OnComplete(OnSwayComplete);
	}

	private void OnSwayComplete()
	{
		if (moveTowardsPenitent)
		{
			swayTween.Pause();
			Vector2 vector = Core.Logic.Penitent.transform.position - base.transform.position;
			float num = 3f;
			float num2 = num * Mathf.Sign(vector.x);
			base.transform.DOMoveX(base.transform.position.x + num2, 0.5f).OnComplete(OnStepCompleted).SetEase(Ease.InOutQuad);
		}
	}

	private void OnStepCompleted()
	{
		swayTween.Play();
	}

	public void StopSway()
	{
		base.transform.DOKill();
	}

	public void BabyIntro()
	{
		StopSway();
		moveTowardsPenitent = true;
		base.transform.DOMoveY(base.transform.position.y + 11f, 3f).SetEase(Ease.InOutQuad).OnComplete(StartSway);
		babyEffects.TriggerColorizeLerp(1f, hiddenLerpValue, 3f);
		motherEffects.TriggerColorizeLerp(1f, hiddenLerpValue, 3f);
	}

	public void StartGrabAttack()
	{
		StopSway();
		SetCry(shake: true);
		originPoint = base.transform.position;
		StartCoroutine(GrabAttackCoroutine(grabPointTransform));
	}

	public bool HasGrabbedPenitent()
	{
		return hasGrabbedPenitent;
	}

	private void Start()
	{
		StartSway();
	}

	private IEnumerator GrabAttackCoroutine(Transform grabPoint)
	{
		GrabAnticipation(grabPoint);
		float ad = anticipationDuration * 0.75f;
		StartCoroutine(MakeGrabPointFollowPlayer(ad));
		yield return new WaitForSeconds(ad);
		BabyMovesIntoGrab(grabPoint);
		yield return new WaitForSeconds(movementDuration);
		TryGrabPlayer();
	}

	private IEnumerator MakeGrabPointFollowPlayer(float duration)
	{
		float c = 0f;
		while (c < duration)
		{
			c += Time.deltaTime;
			SetGrabPointToPlayerPosition();
			yield return null;
		}
	}

	private void SetGrabPointToPlayerPosition()
	{
		grabPointTransform.position = new Vector3(Core.Logic.Penitent.transform.position.x, grabPointTransform.position.y, grabPointTransform.position.z);
	}

	private IEnumerator GrabSuccessCoroutine()
	{
		hasGrabbedPenitent = true;
		GhostTrailGenerator.AreGhostTrailsAllowed = false;
		HidePlayer();
		PlayGrabAnimation();
		base.transform.DOMoveY(base.transform.position.y + 1.5f, 1.5f).SetEase(Ease.OutQuad);
		float grabAnimationDuration = 3f;
		yield return new WaitForSeconds(grabAnimationDuration);
		KillPlayer();
	}

	private void KillPlayer()
	{
		Debug.Log("KillPlayer()");
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: false);
		Core.Logic.Penitent.Kill();
	}

	private void PlayGrabAnimation()
	{
		Debug.Log("PlayGrabAnimation()");
		Audio.PlayBabyGrab_AUDIO();
		babyAnimator.SetTrigger("EXECUTION");
	}

	private void HidePlayer()
	{
		Debug.Log("HidePlayer()");
		Core.Input.SetBlocker("PLAYER_LOGIC", blocking: true);
		Penitent penitent = Core.Logic.Penitent;
		penitent.SpriteRenderer.enabled = false;
		penitent.Status.CastShadow = false;
	}

	private IEnumerator GrabFailCoroutine()
	{
		hasGrabbedPenitent = false;
		SetCry(shake: true);
		ReturnBabyToIdle();
		yield return new WaitForSeconds(movementDuration * 2f);
		StartSway();
		moveTowardsPenitent = true;
	}

	public void SetCry(bool shake)
	{
		babyAnimator.SetTrigger("CRY");
		Audio.PlayCry_AUDIO();
		if (shake)
		{
			Core.Logic.CameraManager.ProCamera2DShake.Shake(2f, Vector3.down * 3f, 60, 0.2f, 0f, default(Vector3), 0.05f);
		}
	}

	public void ArmRipShake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.5f, Vector3.right * 3f, 60, 0.2f, 0f, default(Vector3), 0.05f);
	}

	private void ReturnBabyToIdle()
	{
		base.transform.DOMove(originPoint, movementDuration * 2f).SetEase(Ease.InOutCubic);
		babyEffects.TriggerColorizeLerp(0f, hiddenLerpValue, movementDuration);
		motherEffects.TriggerColorizeLerp(0f, hiddenLerpValue, movementDuration);
	}

	private void BabyMovesIntoGrab(Transform grabPoint)
	{
		base.transform.DOMove((Vector2)grabPoint.position - attackCollider.offset, movementDuration).SetEase(Ease.InOutCubic);
		babyEffects.TriggerColorizeLerp(hiddenLerpValue, 0f, movementDuration);
		motherEffects.TriggerColorizeLerp(hiddenLerpValue, 0f, movementDuration);
	}

	private void GrabAnticipation(Transform gpoint)
	{
		Vector2 vector = gpoint.position;
		PlayWarning();
		Vector2 vector2 = vector + Vector2.left * grabWidth / 2f;
		Vector2 vector3 = vector + Vector2.right * grabWidth / 2f;
		Debug.DrawLine(vector, vector2, Color.yellow, anticipationDuration);
		Debug.DrawLine(vector + Vector2.up * grabHeight, vector2 + Vector2.up * grabHeight, Color.yellow, anticipationDuration);
		Debug.DrawLine(vector, vector3, Color.yellow, anticipationDuration);
		Debug.DrawLine(vector + Vector2.up * grabHeight, vector3 + Vector2.up * grabHeight, Color.yellow, anticipationDuration);
	}

	private void TryGrabPlayer()
	{
		Vector2 vector = base.transform.position;
		Vector2 vector2 = vector + Vector2.left * grabWidth / 2f;
		Vector2 vector3 = vector + Vector2.right * grabWidth / 2f;
		Debug.DrawLine(vector, vector2, Color.yellow, anticipationDuration);
		Debug.DrawLine(vector + Vector2.up * grabHeight, vector2 + Vector2.up * grabHeight, Color.red, anticipationDuration);
		Debug.DrawLine(vector, vector3, Color.yellow, anticipationDuration);
		Debug.DrawLine(vector + Vector2.up * grabHeight, vector3 + Vector2.up * grabHeight, Color.red, anticipationDuration);
		if (attackCollider.OverlapCollider(contactFilter, results) > 0 && !Core.Logic.Penitent.Status.Invulnerable && (!Core.InventoryManager.IsPrayerEquipped("PR202") || !Core.Logic.Penitent.PrayerCast.IsUsingAbility))
		{
			StartCoroutine(GrabSuccessCoroutine());
		}
		else
		{
			StartCoroutine(GrabFailCoroutine());
		}
	}
}
