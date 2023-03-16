using UnityEngine;
using UnityEngine.UI;

public class LevelHUDPlayerSuperCard : AbstractMonoBehaviour
{
	private const float SPEED = 10f;

	private const float Y_DIFF = -30f;

	[SerializeField]
	private Image image;

	private bool initialized;

	private float current;

	private float max;

	private Vector3 start;

	private Vector3 end;

	private Vector3 target;

	private void Start()
	{
		end = base.transform.localPosition;
		start = end + new Vector3(0f, -30f, 0f);
	}

	private void Update()
	{
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		if (initialized)
		{
			target = Vector3.Lerp(start, end, current / max);
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, target, (float)CupheadTime.Delta * 10f);
			image.fillAmount = Mathf.Lerp(image.fillAmount, current / max, (float)CupheadTime.Delta * 10f);
		}
	}

	public void Init(PlayerId playerId, float exCost)
	{
		max = exCost;
		switch (playerId)
		{
		case PlayerId.PlayerOne:
			base.animator.SetInteger("Player", PlayerManager.player1IsMugman ? 1 : 0);
			break;
		case PlayerId.PlayerTwo:
			base.animator.SetInteger("Player", (!PlayerManager.player1IsMugman) ? 1 : 0);
			break;
		}
		initialized = true;
	}

	public void SetAmount(float amount)
	{
		current = Mathf.Clamp(amount, 0f, max);
	}

	public void SetSuper(bool super)
	{
		base.animator.SetBool("Super", super);
	}

	public void SetEx(bool ex)
	{
		base.animator.SetBool("Ex", ex);
	}
}
