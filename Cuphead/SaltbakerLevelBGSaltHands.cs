using UnityEngine;

public class SaltbakerLevelBGSaltHands : MonoBehaviour
{
	[SerializeField]
	private Vector2[] positions;

	[SerializeField]
	private Animator anim;

	[SerializeField]
	private SpriteRenderer[] rends;

	private int positionCounter;

	public void Play()
	{
		base.transform.position = positions[positionCounter];
		base.transform.localScale = new Vector3((positionCounter == 0) ? 1 : (-1), (positionCounter != 0) ? 1.02f : 1f);
		for (int i = 0; i < rends.Length; i++)
		{
			rends[i].sortingOrder = ((positionCounter != 0) ? 650 : 850) + i * 5;
		}
		anim.Play("SaltHands");
		SFX_SALTB_Bouncer_MakeBouncer();
		positionCounter = 1 - positionCounter;
	}

	private void SFX_SALTB_Bouncer_MakeBouncer()
	{
		AudioManager.Play("sfx_dlc_saltbaker_p3_hands_makebouncer");
	}
}
