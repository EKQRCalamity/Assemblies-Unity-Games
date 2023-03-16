using System;
using System.Collections;
using UnityEngine;

public class AirplaneLevelPlayerPlane : LevelProperties.Airplane.Entity
{
	private const float PUFF_DELAY_L = 1f;

	private const float PUFF_DELAY_R = 0.8f;

	private const float AUTO_MOVE_MAX_X_DIST = 50f;

	private const float AUTO_MOVE_MAX_Y_DIST = 5f;

	private const float AUTO_MOVE_MAX_Y_END_SPEED = 2f;

	private const float AUTO_MOVE_MAX_X_SPEED = 400f;

	private const float AUTO_MOVE_MAX_Y_SPEED = 100f;

	private const float AUTO_ACCEL_X = 5f;

	private const float AUTO_ACCEL_Y = 3f;

	private const float MIN_TILT_DIFFERENCE = 0.1f;

	private const float TILT_ATTENUATION = 0.15f;

	private const float ACCEL_SPEED = 4.1f;

	private const float BOUNCE_TIME = 1.2f;

	private const float BOUNCE_DIST = 12f;

	private const float RISE_DIST = 9f;

	private const float RISE_RATE = 0.1f;

	private const float BOUNCE_RATE = 0.5f;

	private const float DAMP_ON_BOUNDARY_COLLIDE = 12.5f;

	[SerializeField]
	private float pitchIncreaseFactor = 0.5f;

	[SerializeField]
	private float pitchIncreaseFactorHighSpeed = 0.5f;

	[SerializeField]
	private MinMax volume = new MinMax(0.25f, 0.5f);

	[SerializeField]
	private MinMax volumeHighSpeed = new MinMax(0.25f, 0.5f);

	private float cachedHighSpeedVolume = 1E-06f;

	[SerializeField]
	private float highSpeedVolumeIncreaseRate = 1f;

	[SerializeField]
	private float highSpeedVolumeDecreaseRate = 0.25f;

	[SerializeField]
	private float volumeHighSpeedSpeedFloor = 0.5f;

	[SerializeField]
	private Transform edgeLeft;

	[SerializeField]
	private Transform edgeRight;

	[SerializeField]
	private Transform airplane1;

	[SerializeField]
	private Transform tiltable;

	[SerializeField]
	private Transform[] planeParts;

	[SerializeField]
	private float[] planePartAngleRanges;

	[SerializeField]
	private Vector2[] planePartPosOffsets;

	[SerializeField]
	private Effect planePuffFX;

	[SerializeField]
	private Transform[] planePuffPos;

	private AbstractPlayerController player1;

	private AbstractPlayerController player2;

	private bool p1IsColliding;

	private bool p2IsColliding;

	private Vector3 tiltableBasePos;

	private float maxParallaxX;

	public bool autoX;

	public bool autoY;

	public Vector3 autoDest;

	private float autoTiltTime;

	private bool autoTilt;

	private float minX;

	private float maxX;

	private float rotationDist;

	private float rotationVal;

	private float p1contactTime;

	private float p2contactTime;

	private bool[] playerInSuper = new bool[2];

	private bool[] restorePlayerPos = new bool[2];

	private float[] playerRelativePosAtSuperStart = new float[2];

	private float[] puffTimer = new float[2];

	private Vector3 moveSpeed;

	private Coroutine autoMoveCoroutine;

	private float lastNormalizedSpeed;

	private int updateCount;

	public override void LevelInit(LevelProperties.Airplane properties)
	{
		base.LevelInit(properties);
		tiltableBasePos = tiltable.localPosition;
		maxParallaxX = CupheadLevelCamera.Current.Bounds.xMax - properties.CurrentState.plane.endScreenOffset;
		rotationDist = Vector3.Distance(edgeLeft.position, edgeRight.position);
		rotationVal = rotationDist / 2f;
		StartCoroutine(handle_player_move_cr());
		StartCoroutine(handle_tilt_cr());
		puffTimer[0] = 1f;
		puffTimer[1] = 0.8f;
		SFX_DOGFIGHT_PlayerPlane_Loop();
		SFX_DOGFIGHT_PlayerPlane_HighSpeed_Loop();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (player1 != null)
		{
			LevelPlayerWeaponManager component = player1.gameObject.GetComponent<LevelPlayerWeaponManager>();
			component.OnSuperStart -= StartP1Super;
			component.OnSuperEnd -= EndP1Super;
			component.OnExStart -= StartP1Super;
			component.OnExEnd -= EndP1Super;
		}
		if (player2 != null)
		{
			LevelPlayerWeaponManager component2 = player2.gameObject.GetComponent<LevelPlayerWeaponManager>();
			component2.OnSuperStart -= StartP2Super;
			component2.OnSuperEnd -= EndP2Super;
			component2.OnExStart -= StartP2Super;
			component2.OnExEnd -= EndP2Super;
		}
		WORKAROUND_NullifyFields();
	}

	private void Update()
	{
		if (((AirplaneLevel)Level.Current).Rotating)
		{
			if (playerInSuper[0])
			{
				restorePlayerPos[0] = true;
			}
			if (playerInSuper[1])
			{
				restorePlayerPos[1] = true;
			}
		}
		if (player1 == null)
		{
			player1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
			if (player1 != null)
			{
				LevelPlayerWeaponManager component = player1.gameObject.GetComponent<LevelPlayerWeaponManager>();
				component.OnSuperStart += StartP1Super;
				component.OnSuperEnd += EndP1Super;
				component.OnExStart += StartP1Super;
				component.OnExEnd += EndP1Super;
			}
		}
		if (player2 == null)
		{
			player2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
			if (player2 != null)
			{
				LevelPlayerWeaponManager component2 = player2.gameObject.GetComponent<LevelPlayerWeaponManager>();
				component2.OnSuperStart += StartP2Super;
				component2.OnSuperEnd += EndP2Super;
				component2.OnExStart += StartP2Super;
				component2.OnExEnd += EndP2Super;
			}
		}
		if (player1 != null)
		{
			p1IsColliding = player1.transform.parent == airplane1.transform;
			player1.transform.SetEulerAngles(null, null, 0f);
		}
		else
		{
			p1IsColliding = false;
		}
		if (player2 != null)
		{
			p2IsColliding = player2.transform.parent == airplane1.transform;
			player2.transform.SetEulerAngles(null, null, 0f);
		}
		else
		{
			p2IsColliding = false;
		}
		autoTiltTime = Mathf.Clamp(autoTiltTime + (float)CupheadTime.Delta * ((!autoX || !autoTilt) ? (-1f) : 3f), 0f, 1f);
		for (int i = 0; i < puffTimer.Length; i++)
		{
			puffTimer[i] -= CupheadTime.Delta;
			if (puffTimer[i] <= 0f)
			{
				puffTimer[i] += ((i != 0) ? 0.8f : 1f);
				Effect effect = planePuffFX.Create(planePuffPos[i].position);
				effect.transform.SetEulerAngles(null, null, (i != 0) ? 30 : (-30));
			}
		}
	}

	private void StartP1Super()
	{
		playerInSuper[0] = true;
		playerRelativePosAtSuperStart[0] = player1.transform.position.y - base.transform.position.y;
	}

	private void EndP1Super()
	{
		playerInSuper[0] = false;
		if (restorePlayerPos[0])
		{
			player1.transform.position = new Vector3(player1.transform.position.x, base.transform.position.y + playerRelativePosAtSuperStart[0]);
		}
		restorePlayerPos[0] = false;
	}

	private void StartP2Super()
	{
		playerInSuper[1] = true;
		playerRelativePosAtSuperStart[1] = player2.transform.position.y - base.transform.position.y;
	}

	private void EndP2Super()
	{
		playerInSuper[1] = false;
		if (restorePlayerPos[1])
		{
			player2.transform.position = new Vector3(player1.transform.position.x, base.transform.position.y + playerRelativePosAtSuperStart[1]);
		}
		restorePlayerPos[1] = false;
	}

	public void AutoMoveToPos(Vector3 pos, bool controlTilt = true, bool holdForYToReleaseX = true)
	{
		if (autoMoveCoroutine != null)
		{
			StopCoroutine(autoMoveCoroutine);
		}
		autoTilt = controlTilt;
		autoDest = pos;
		autoX = true;
		autoY = true;
		autoMoveCoroutine = StartCoroutine(auto_move_to_pos_cr(holdForYToReleaseX));
	}

	private IEnumerator auto_move_to_pos_cr(bool holdForYToReleaseX)
	{
		if (Mathf.Abs(autoDest.y - base.transform.position.y) > 50f)
		{
			moveSpeed.y = Mathf.Sign(base.transform.position.y - autoDest.y) * 100f;
		}
		float maxYSpeed = 100f;
		float yDir = Mathf.Sign(autoDest.y - base.transform.position.y);
		YieldInstruction wait = new WaitForFixedUpdate();
		while (autoX || autoY)
		{
			if (!CupheadTime.IsPaused())
			{
				if (autoX)
				{
					float num = ((!(Mathf.Abs(autoDest.x - base.transform.position.x) < 100f)) ? 1f : 0.5f);
					moveSpeed.x = Mathf.Clamp(moveSpeed.x + Mathf.Sign(autoDest.x - base.transform.position.x) * 5f * num, -400f, 400f);
					MoveAirplane();
					if (Mathf.Abs(base.transform.position.x - autoDest.x) < 50f && Mathf.Abs(base.transform.position.y - autoDest.y) < (float)((!holdForYToReleaseX) ? 1000 : 20))
					{
						autoX = false;
						moveSpeed.x = Mathf.Clamp(moveSpeed.x, base.properties.CurrentState.plane.speedAtMaxTilt.min, base.properties.CurrentState.plane.speedAtMaxTilt.max);
					}
				}
				if (autoY)
				{
					float num2 = ((!(Mathf.Abs(autoDest.y - base.transform.position.y) < 50f)) ? 1f : 0.5f);
					moveSpeed.y = Mathf.Clamp(moveSpeed.y + Mathf.Sign(autoDest.y - base.transform.position.y) * 3f * num2, 0f - maxYSpeed, maxYSpeed);
					if (yDir != Mathf.Sign(autoDest.y - base.transform.position.y))
					{
						maxYSpeed *= 0.99f;
					}
					if (Mathf.Abs(base.transform.position.y - autoDest.y) < 5f && moveSpeed.y < 2f)
					{
						autoY = false;
						moveSpeed.y = 0f;
						base.transform.position = new Vector3(base.transform.position.x, autoDest.y);
					}
				}
			}
			yield return wait;
		}
		autoX = false;
	}

	private void HandleDip()
	{
		p1contactTime = ((!p1IsColliding) ? 0f : Mathf.Clamp(p1contactTime + (float)CupheadTime.Delta, 0f, 1.2f));
		p2contactTime = ((!p2IsColliding) ? 0f : Mathf.Clamp(p2contactTime + (float)CupheadTime.Delta, 0f, 1.2f));
		float num = Mathf.Clamp(Mathf.Sin(p1contactTime / 1.2f * (float)Math.PI) * 12f + Mathf.Sin(p2contactTime / 1.2f * (float)Math.PI) * 12f, 0f, 12f);
		if (!p1IsColliding && !p2IsColliding)
		{
			tiltable.localPosition = Vector3.Lerp(tiltable.transform.localPosition, tiltableBasePos + Vector3.up * 9f, 0.1f);
		}
		else
		{
			tiltable.localPosition = Vector3.Lerp(tiltable.transform.localPosition, tiltableBasePos + Vector3.down * num, 0.5f);
		}
	}

	private float GetDestRotationVal()
	{
		LevelProperties.Airplane.Plane plane = base.properties.CurrentState.plane;
		float num = 0f;
		float num2 = Vector3.Distance(edgeRight.position, Vector3.Lerp(edgeLeft.position, edgeRight.position, Mathf.InverseLerp(plane.speedAtMaxTilt.min, plane.speedAtMaxTilt.max, moveSpeed.x)));
		if (p1IsColliding && p2IsColliding && player1 != null && player2 != null)
		{
			num = Vector3.Distance(Vector3.Lerp(player1.transform.position, player2.transform.position, 0.5f), edgeRight.position);
		}
		else
		{
			AbstractPlayerController abstractPlayerController = null;
			if (p1IsColliding && player1 != null)
			{
				abstractPlayerController = player1;
			}
			else if (p2IsColliding && player2 != null)
			{
				abstractPlayerController = player2;
			}
			num = ((!(abstractPlayerController != null)) ? num2 : Vector3.Distance(edgeRight.position, abstractPlayerController.transform.position));
		}
		return Mathf.Lerp(num, num2, autoTiltTime);
	}

	private IEnumerator handle_tilt_cr()
	{
		LevelProperties.Airplane.Plane p = base.properties.CurrentState.plane;
		float destRotationVal = rotationVal;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			if (!CupheadTime.IsPaused())
			{
				if (!p1IsColliding && !p2IsColliding && moveSpeed.x == 0f)
				{
					Mathf.Lerp(destRotationVal, Vector3.Distance(edgeRight.position, base.transform.position), 0.05f);
				}
				else
				{
					destRotationVal = GetDestRotationVal();
				}
				if (Mathf.Abs(destRotationVal - rotationVal) > 0.1f)
				{
					rotationVal = Mathf.Lerp(rotationVal, destRotationVal, 0.15f);
				}
				else
				{
					rotationVal = destRotationVal;
				}
				tiltable.transform.SetEulerAngles(null, null, p.tiltAngle.GetFloatAt(rotationVal / rotationDist));
			}
			yield return wait;
		}
	}

	private IEnumerator handle_player_move_cr()
	{
		LevelProperties.Airplane.Plane p = base.properties.CurrentState.plane;
		float destMoveSpeed3 = 0f;
		bool goingLeft = false;
		YieldInstruction wait = new WaitForFixedUpdate();
		while (true)
		{
			if (!CupheadTime.IsPaused())
			{
				while (autoX)
				{
					yield return null;
				}
				goingLeft = moveSpeed.x < 0f;
				if (!p1IsColliding && !p2IsColliding)
				{
					if (moveSpeed.x < 0f && goingLeft)
					{
						moveSpeed.x += p.decelerationAmount;
					}
					else if (moveSpeed.x > 0f && !goingLeft)
					{
						moveSpeed.x -= p.decelerationAmount;
					}
					destMoveSpeed3 = moveSpeed.x;
				}
				else
				{
					destMoveSpeed3 = 0f - p.speedAtMaxTilt.GetFloatAt(rotationVal / rotationDist);
					if (Mathf.Abs(destMoveSpeed3 - moveSpeed.x) > 4.1f)
					{
						moveSpeed.x += Mathf.Sign(destMoveSpeed3 - moveSpeed.x) * 4.1f;
					}
					else
					{
						moveSpeed.x = destMoveSpeed3;
					}
				}
				MoveAirplane();
			}
			yield return wait;
		}
	}

	public void SetXRange(float min, float max)
	{
		minX = min;
		maxX = max;
	}

	private void SetPartAngles()
	{
		float num = base.transform.position.x / maxParallaxX;
		for (int i = 0; i < planeParts.Length; i++)
		{
			planeParts[i].SetLocalEulerAngles(null, null, Mathf.LerpUnclamped(0f, planePartAngleRanges[i], num));
			planeParts[i].SetLocalPosition(Mathf.LerpUnclamped(0f, planePartPosOffsets[i].x, num), Mathf.Lerp(0f, planePartPosOffsets[i].y, Mathf.Abs(num)));
		}
	}

	private void MoveAirplane()
	{
		if (CupheadTime.IsPaused())
		{
			return;
		}
		HandleDip();
		SetPartAngles();
		base.transform.position += Vector3.up * moveSpeed.y * CupheadTime.FixedDelta;
		base.transform.position += Vector3.right * moveSpeed.x * CupheadTime.FixedDelta;
		if (!autoX)
		{
			if (base.transform.position.x < minX && moveSpeed.x < 0f)
			{
				moveSpeed.x *= 12.5f * CupheadTime.FixedDelta;
				AutoMoveToPos(new Vector3(minX + 50f, base.transform.position.y), controlTilt: false, holdForYToReleaseX: false);
			}
			if (base.transform.position.x > maxX && moveSpeed.x > 0f)
			{
				moveSpeed.x *= 12.5f * CupheadTime.FixedDelta;
				AutoMoveToPos(new Vector3(maxX - 50f, base.transform.position.y), controlTilt: false, holdForYToReleaseX: false);
			}
		}
		updateCount++;
		if (updateCount % 5 == 0)
		{
			UpdateSound();
		}
	}

	private void UpdateSound()
	{
		float num = Mathf.Abs(moveSpeed.x) / base.properties.CurrentState.plane.speedAtMaxTilt.max;
		if (Mathf.Abs(num - lastNormalizedSpeed) < 0.01f)
		{
			return;
		}
		lastNormalizedSpeed = num;
		AudioManager.ChangeSFXPitch("sfx_dlc_dogfight_playerplane_loop", 1f + num * pitchIncreaseFactor, 0f);
		AudioManager.FadeSFXVolume("sfx_dlc_dogfight_playerplane_loop", volume.GetFloatAt(num), 0f);
		float floatAt = volumeHighSpeed.GetFloatAt(Mathf.InverseLerp(volumeHighSpeedSpeedFloor, 1f, num));
		if (floatAt > cachedHighSpeedVolume)
		{
			cachedHighSpeedVolume += highSpeedVolumeIncreaseRate * CupheadTime.FixedDelta;
			if (cachedHighSpeedVolume > floatAt)
			{
				cachedHighSpeedVolume = floatAt;
			}
		}
		if (floatAt < cachedHighSpeedVolume)
		{
			cachedHighSpeedVolume -= highSpeedVolumeDecreaseRate * CupheadTime.FixedDelta;
			if (cachedHighSpeedVolume < floatAt)
			{
				cachedHighSpeedVolume = floatAt;
			}
		}
		AudioManager.ChangeSFXPitch("sfx_dlc_dogfight_playerplane_highspeed_loop", 1f + num * pitchIncreaseFactorHighSpeed, 0f);
		AudioManager.FadeSFXVolume("sfx_dlc_dogfight_playerplane_highspeed_loop", cachedHighSpeedVolume, 0f);
	}

	private void SFX_DOGFIGHT_PlayerPlane_Loop()
	{
		AudioManager.PlayLoop("sfx_dlc_dogfight_playerplane_loop");
		emitAudioFromObject.Add("sfx_dlc_dogfight_playerplane_loop");
		AudioManager.FadeSFXVolumeLinear("sfx_dlc_dogfight_playerplane_loop", 0.25f, 3f);
	}

	private void SFX_DOGFIGHT_PlayerPlane_HighSpeed_Loop()
	{
		AudioManager.PlayLoop("sfx_dlc_dogfight_playerplane_highspeed_loop");
		emitAudioFromObject.Add("sfx_dlc_dogfight_playerplane_highspeed_loop");
		AudioManager.FadeSFXVolumeLinear("sfx_dlc_dogfight_playerplane_highspeed_loop", 0.6f, 3f);
	}

	private void SFX_DOGFIGHT_PlayerPlane_StopLoop()
	{
		AudioManager.Stop("sfx_dlc_dogfight_playerplane_loop");
	}

	private void AnimationEvent_SFX_DOGFIGHT_PlayerPlane_CanteenCheer()
	{
		AudioManager.Play("sfx_dlc_dogfight_p2_pilotclap");
	}

	private void WORKAROUND_NullifyFields()
	{
		volume = null;
		volumeHighSpeed = null;
		edgeLeft = null;
		edgeRight = null;
		airplane1 = null;
		tiltable = null;
		planeParts = null;
		planePartAngleRanges = null;
		planePartPosOffsets = null;
		planePuffFX = null;
		planePuffPos = null;
		player1 = null;
		player2 = null;
		playerInSuper = null;
		restorePlayerPos = null;
		playerRelativePosAtSuperStart = null;
		puffTimer = null;
		autoMoveCoroutine = null;
	}
}
