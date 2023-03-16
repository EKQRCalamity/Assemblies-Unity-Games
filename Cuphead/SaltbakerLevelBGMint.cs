using UnityEngine;

public class SaltbakerLevelBGMint : MonoBehaviour
{
	[SerializeField]
	private Transform startPos;

	[SerializeField]
	private Transform nextPos;

	[SerializeField]
	private Animator anim;

	public void StartAnimation(int which)
	{
		anim.Play(which.ToString(), 0, Random.Range(0f, 0.5f));
	}

	private void AniEvent_JumpDown()
	{
		base.transform.position += nextPos.position - startPos.position;
		if (base.transform.position.y < -1000f)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
