using UnityEngine;

public class SnowCultLevelSnowballExplosion : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private Animator animator;

	public void Init(Vector3 pos, SnowCultLevelSnowball.Size size, SnowCultLevelYeti main)
	{
		base.transform.position = pos;
		switch (size)
		{
		case SnowCultLevelSnowball.Size.Large:
			animator.Play("Large");
			break;
		case SnowCultLevelSnowball.Size.Medium:
			animator.Play((main.GetMediumExplosion() != 0) ? "MediumB" : "MediumA");
			break;
		case SnowCultLevelSnowball.Size.Small:
			switch (main.GetSmallExplosion())
			{
			case 0:
				animator.Play("SmallA");
				break;
			case 1:
				animator.Play("SmallB");
				break;
			case 2:
				animator.Play("SmallC");
				break;
			}
			break;
		}
	}

	private void Update()
	{
		if (!rend.enabled)
		{
			this.Recycle();
		}
	}
}
