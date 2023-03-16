using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserProfileDisplay : AbstractMonoBehaviour
{
	[SerializeField]
	private Image gamerPic;

	[SerializeField]
	private Text gamerTag;

	[SerializeField]
	private PlayerId player;

	[SerializeField]
	private GameObject root;

	[SerializeField]
	private GameObject switchPromptRoot;

	[SerializeField]
	private bool showForMultipleUsersUnsupported;

	[SerializeField]
	private Sprite defaultAvatarCuphead;

	[SerializeField]
	private Sprite defaultAvatarMugman;

	private OnlineUser currentPicUser;

	private bool isSlotSelect;

	protected override void Awake()
	{
		root.SetActive(value: false);
		gamerPic.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		isSlotSelect = SceneManager.GetActiveScene().name == Scenes.scene_slot_select.ToString();
	}

	private void Update()
	{
		if (OnlineManager.Instance.Interface.SupportsMultipleUsers || (showForMultipleUsersUnsupported && OnlineManager.Instance.Interface.SupportsUserSignIn))
		{
			OnlineUser user = OnlineManager.Instance.Interface.GetUser(player);
			if (user != null)
			{
				root.SetActive(value: true);
				string text = user.Name;
				if (gamerTag.text != text)
				{
					gamerTag.text = text;
				}
				Texture2D profilePic = OnlineManager.Instance.Interface.GetProfilePic(player);
				if (profilePic != null && currentPicUser != user)
				{
					currentPicUser = user;
					Sprite sprite = Sprite.Create(profilePic, new Rect(0f, 0f, profilePic.width, profilePic.height), new Vector2(0.5f, 0.5f));
					gamerPic.sprite = sprite;
					gamerPic.gameObject.SetActive(value: true);
				}
				else if (profilePic == null)
				{
					gamerPic.gameObject.SetActive(value: false);
				}
			}
			else
			{
				root.SetActive(value: false);
			}
		}
		else
		{
			root.SetActive(value: false);
		}
	}
}
