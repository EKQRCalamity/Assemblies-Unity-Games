using UnityEngine;

public class SaltbakerLevelStrawberryBasket : MonoBehaviour
{
	private const float GRAB_OFFSET = 80f;

	[SerializeField]
	private Animator anim;

	[SerializeField]
	private SpriteRenderer rend;

	[SerializeField]
	private SpriteRenderer saltbakerTopperRend;

	[SerializeField]
	private SaltbakerLevelSaltbaker saltbaker;

	private bool moving;

	private float vel;

	public void StartRunIn(bool sbOnLeft)
	{
		base.transform.position = new Vector3((!sbOnLeft) ? (Level.Current.Left - 300) : (Level.Current.Right + 300), base.transform.position.y);
		base.transform.localScale = new Vector3(sbOnLeft ? 1 : (-1), 1f);
		vel = ((float)(Level.Current.Left + Level.Current.Right) / 2f + 80f * base.transform.localScale.x - base.transform.position.x) / 1.0416666f;
		anim.Play("RunIn");
		moving = true;
	}

	public void GetGrabbed()
	{
		moving = false;
	}

	public void StartRunOut()
	{
		anim.Play("RunOut");
		anim.Update(0f);
		SFX_SALTBAKER_P1_StrawberryBag_CryingRunOff();
		moving = true;
		vel *= 0.8f;
	}

	private void Update()
	{
		if (moving)
		{
			base.transform.position += vel * Vector3.right * CupheadTime.Delta;
			if (Mathf.Abs(base.transform.position.x) > 2000f)
			{
				anim.StopPlayback();
				moving = false;
			}
		}
	}

	private void LateUpdate()
	{
		rend.enabled = saltbakerTopperRend.sprite == null || saltbaker.animator.GetCurrentAnimatorStateInfo(0).IsName("PhaseOneToTwo");
	}

	private void SFX_SALTBAKER_P1_StrawberryBag_CryingRunOff()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p1_strawberrybag_cryingrunoff");
	}
}
