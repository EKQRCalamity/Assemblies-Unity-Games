using System.Collections;
using UnityEngine;

public class FlyingBirdLevelNurses : AbstractCollidableObject
{
	private const string Regular = "R";

	private const string Parry = "P";

	[SerializeField]
	private AbstractProjectile pillPrefab;

	[SerializeField]
	private Transform shootRightPosRoot;

	[SerializeField]
	private Transform shootLeftPosRoot;

	[SerializeField]
	private GameObject spitFXLeft;

	[SerializeField]
	private GameObject spitFXRight;

	private bool leftSideShooting;

	private int attackIndex;

	private PlayerId target;

	private string[] pinkPattern;

	private int pinkIndex;

	public Transform[] nurses;

	private LevelProperties.FlyingBird.Nurses properties;

	public void InitNurse(LevelProperties.FlyingBird.Nurses properties)
	{
		nurses = base.transform.GetChildTransforms();
		Transform[] array = nurses;
		foreach (Transform transform in array)
		{
			transform.gameObject.SetActive(value: true);
		}
		leftSideShooting = Random.Range(-1, 1) >= 0;
		this.properties = properties;
		attackIndex = Random.Range(0, properties.attackCount.Split(',').Length);
		pinkPattern = properties.pinkString.Split(',');
		pinkIndex = 0;
		Transform[] array2 = nurses;
		foreach (Transform transform2 in array2)
		{
			if (transform2.GetComponent<Collider2D>() != null)
			{
				transform2.GetComponent<Collider2D>().enabled = true;
			}
		}
		StartCoroutine(attack_cr());
	}

	private IEnumerator attack_cr()
	{
		bool shootLeft = Rand.Bool();
		bool multiplayer = ((PlayerManager.GetPlayer(PlayerId.PlayerTwo) != null) ? true : false);
		while (true)
		{
			int max = Parser.IntParse(properties.attackCount.Split(',')[attackIndex]);
			for (int i = 0; i < max; i++)
			{
				if (i != 0)
				{
					yield return CupheadTime.WaitForSeconds(this, properties.attackRepeatDelay);
				}
				if (shootLeft)
				{
					base.animator.SetBool("ANurseATK", value: true);
				}
				else
				{
					base.animator.SetBool("BNurseATK", value: true);
				}
				shootLeft = !shootLeft;
				if (multiplayer)
				{
					target++;
					if (target > PlayerId.PlayerTwo)
					{
						target = PlayerId.PlayerOne;
					}
				}
			}
			leftSideShooting = !leftSideShooting;
			yield return CupheadTime.WaitForSeconds(this, properties.attackMainDelay);
			attackIndex++;
			if (attackIndex >= properties.attackCount.Split(',').Length)
			{
				attackIndex = 0;
			}
		}
	}

	private void ShootLeft()
	{
		spitFXLeft.SetActive(value: false);
		AbstractProjectile abstractProjectile = pillPrefab.Create(shootLeftPosRoot.position + base.transform.up.normalized * 0.1f);
		abstractProjectile.GetComponent<FlyingBirdLevelNursePill>().InitPill(properties, target, pinkPattern[pinkIndex] == "P");
		pinkIndex = (pinkIndex + 1) % pinkPattern.Length;
		base.animator.SetBool("ANurseATK", value: false);
		spitFXLeft.SetActive(value: true);
	}

	private void ShootRight()
	{
		spitFXRight.SetActive(value: false);
		AbstractProjectile abstractProjectile = pillPrefab.Create(shootRightPosRoot.position + base.transform.up.normalized * 0.1f);
		abstractProjectile.GetComponent<FlyingBirdLevelNursePill>().InitPill(properties, target, pinkPattern[pinkIndex] == "P");
		pinkIndex = (pinkIndex + 1) % pinkPattern.Length;
		base.animator.SetBool("BNurseATK", value: false);
		spitFXRight.SetActive(value: true);
	}

	private void ShootSFX()
	{
		AudioManager.Play("nurse_attack");
		emitAudioFromObject.Add("nurse_attack");
	}

	public void Die()
	{
		StopAllCoroutines();
	}
}
