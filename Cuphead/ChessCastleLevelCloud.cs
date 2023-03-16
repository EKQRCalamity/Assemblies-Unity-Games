using System.Collections;
using UnityEngine;

public class ChessCastleLevelCloud : AbstractPausableComponent
{
	[SerializeField]
	private MinMax speedRange;

	private ChessCastleLevel castle;

	private float speed;

	private float speedMultiplier = 1f;

	private bool wasRotating;

	private Coroutine speedRampCoroutine;

	public void Initialize(ChessCastleLevel castle)
	{
		this.castle = castle;
	}

	private void Start()
	{
		base.animator.SetInteger("Version", Random.Range(0, 13));
		base.animator.Update(0f);
		Bounds bounds = new Bounds(Vector3.zero, new Vector3(1280f, 720f, 0f) / Level.Current.CameraSettings.zoom);
		Vector2 vector = GetComponent<SpriteRenderer>().sprite.bounds.size;
		base.transform.position = new Vector3(bounds.max.x + vector.x * 0.5f + 30f, Random.Range(bounds.min.y + vector.y * 0.5f + 30f, bounds.max.y - vector.y * 0.5f - 30f), Random.Range(0f, 1f));
		speed = speedRange.RandomFloat();
		StartCoroutine(destroy_cr());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		castle = null;
	}

	private void Update()
	{
		if (castle.rotating)
		{
			if (speedRampCoroutine != null)
			{
				StopCoroutine(speedRampCoroutine);
				speedRampCoroutine = null;
			}
			speedMultiplier = castle.rotationMultiplier;
		}
		else if (wasRotating)
		{
			speedRampCoroutine = StartCoroutine(speedRamp_cr());
		}
		wasRotating = castle.rotating;
		float num = speed * speedMultiplier;
		Vector3 position = base.transform.position;
		position.x -= num * (float)CupheadTime.Delta;
		base.transform.position = position;
	}

	private IEnumerator speedRamp_cr()
	{
		float elapsedTime = 0f;
		while (elapsedTime < 0.35f)
		{
			yield return null;
			elapsedTime += Time.deltaTime;
			speedMultiplier = Mathf.Lerp(castle.rotationMultiplier, 1f, elapsedTime / 0.35f);
		}
		speedMultiplier = 1f;
		speedRampCoroutine = null;
	}

	private IEnumerator destroy_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 10f);
		Object.Destroy(base.gameObject);
	}
}
