using System.Collections;
using UnityEngine;

public class RetroArcadeMissileMan : RetroArcadeEnemy
{
	[SerializeField]
	private RetroArcadeMissile missilePrefab;

	[SerializeField]
	private Transform shootRootLeft;

	[SerializeField]
	private Transform shootRootRight;

	[SerializeField]
	private Transform pivotPoint;

	private const float MAX_X_POS = 80f;

	private LevelProperties.RetroArcade properties;

	private RetroArcadeMissile missile;

	public void LevelInit(LevelProperties.RetroArcade properties)
	{
		this.properties = properties;
		hp = properties.CurrentState.missile.hp;
	}

	public void StartMissile()
	{
		base.gameObject.SetActive(value: true);
		missile = Object.Instantiate(missilePrefab);
		missile.Init(shootRootLeft.position, -90f, properties.CurrentState.missile, pivotPoint.position);
		StartCoroutine(move_cr());
		StartCoroutine(fire_missiles_cr());
	}

	private IEnumerator move_cr()
	{
		float time = properties.CurrentState.missile.manMoveTime;
		bool movingRight = Rand.Bool();
		while (true)
		{
			float t = 0f;
			float start = base.transform.position.x;
			float end = ((!movingRight) ? (-80f) : 80f);
			while (t < time)
			{
				TransformExtensions.SetPosition(x: EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, start, end, t / time), transform: base.transform);
				t += (float)CupheadTime.Delta;
				yield return null;
			}
			base.transform.SetPosition(end);
			movingRight = !movingRight;
			yield return null;
		}
	}

	private IEnumerator fire_missiles_cr()
	{
		string[] dirString = properties.CurrentState.missile.directionString.Split(',');
		int dirIndex = Random.Range(0, dirString.Length);
		bool onRight2 = false;
		while (true)
		{
			onRight2 = dirString[dirIndex] == "R";
			yield return CupheadTime.WaitForSeconds(this, properties.CurrentState.missile.timerRelease.RandomFloat());
			yield return null;
			missile.StartCircle(onRight2, pivotPoint.position);
			dirIndex = (dirIndex + 1) % dirString.Length;
		}
	}

	public override void Dead()
	{
		base.Dead();
		StopAllCoroutines();
		Object.Destroy(missile.gameObject);
		properties.DealDamageToNextNamedState();
		Object.Destroy(base.gameObject);
	}
}
