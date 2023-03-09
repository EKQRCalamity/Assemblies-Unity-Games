using Rewired;
using UnityEngine;

public class PlayerInput : AbstractMonoBehaviour
{
	public enum Axis
	{
		X,
		Y
	}

	private AbstractPlayerController player;

	private bool canRotateInput;

	private Transform cameraTransform;

	public PlayerId playerId { get; private set; }

	public bool IsDead
	{
		get
		{
			if (player != null)
			{
				return player.IsDead;
			}
			return false;
		}
	}

	public Player actions { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		player = GetComponent<AbstractPlayerController>();
	}

	private void Start()
	{
		if (Level.Current != null && Level.Current.CameraRotates)
		{
			canRotateInput = true;
			cameraTransform = Camera.main.transform;
		}
	}

	public void Init(PlayerId playerId)
	{
		this.playerId = playerId;
		actions = PlayerManager.GetPlayerInput(playerId);
	}

	public override void StopAllCoroutines()
	{
	}

	public int GetAxisInt(Axis axis, bool crampedDiagonal = false, bool duckMod = false)
	{
		Vector2 vector = new Vector2(actions.GetAxis(0), actions.GetAxis(1));
		if (canRotateInput)
		{
			if (SettingsData.Data.rotateControlsWithCamera)
			{
				vector = cameraTransform.rotation * vector;
			}
			else if (Mathf.Abs(cameraTransform.rotation.eulerAngles.z - 180f) <= 1f)
			{
				vector = cameraTransform.rotation * vector;
			}
		}
		float magnitude = vector.magnitude;
		float num = ((!crampedDiagonal) ? 0.38268f : 0.5f);
		if (magnitude < 0.375f)
		{
			return 0;
		}
		float num2 = ((axis != 0) ? vector.y : vector.x) / magnitude;
		if (num2 > num)
		{
			return 1;
		}
		if (num2 < ((!duckMod) ? (0f - num) : (-0.705f)))
		{
			return -1;
		}
		return 0;
	}

	public float GetAxis(Axis axis)
	{
		if (axis == Axis.X)
		{
			return actions.GetAxis(0);
		}
		return actions.GetAxis(1);
	}

	public bool GetButton(CupheadButton button)
	{
		return actions.GetButton((int)button);
	}
}
