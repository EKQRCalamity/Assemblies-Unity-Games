using UnityEngine;

public class ChessQueenLevelLooseMouse : MonoBehaviour
{
	[SerializeField]
	private Animator anim;

	[SerializeField]
	private ChessQueenLevelQueen queen;

	private bool won;

	private float jumpSwitchTime;

	private GameObject activeCannonball;

	private void Start()
	{
		jumpSwitchTime = Random.Range(2f, 3f);
	}

	private void Update()
	{
		anim.SetBool("Right", queen.transform.position.x > -200f);
		if (!won && activeCannonball == null)
		{
			jumpSwitchTime -= CupheadTime.Delta;
			if (jumpSwitchTime < 0f)
			{
				anim.SetBool("Jump", !anim.GetBool("Jump"));
				jumpSwitchTime = ((!anim.GetBool("Jump")) ? Random.Range(3f, 4f) : Random.Range(0.5f, 2f));
			}
		}
	}

	public void HitQueen()
	{
		anim.SetTrigger("HitQueen");
		anim.SetBool("Jump", value: true);
		jumpSwitchTime = Random.Range(2f, 3f);
	}

	public void CannonFired(GameObject cannonBall)
	{
		anim.SetBool("Jump", value: false);
		activeCannonball = cannonBall;
		jumpSwitchTime = Random.Range(3f, 4f);
	}

	public void Win()
	{
		anim.SetBool("Jump", value: true);
		won = true;
	}
}
