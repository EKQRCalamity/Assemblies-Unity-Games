using UnityEngine;

public class ADeckView : ATargetView
{
	private static readonly int OPEN_ID = Animator.StringToHash("Open");

	[Header("Deck")]
	public ACardLayout cardLayout;

	public StringEvent onNameChange;

	public MaterialEvent onMaterialChange;

	public StringEvent onDescriptionChange;

	private Animator _animator;

	public ADeck deckObject
	{
		get
		{
			return base.target as ADeck;
		}
		set
		{
			base.target = value;
		}
	}

	protected Animator animator => this.CacheComponentInChildren(ref _animator);

	public static CardLayoutElement Create(ADeck deck, Transform parent = null)
	{
		return Pools.Unpool(deck.blueprint, parent).GetComponent<ATargetView>().SetData(deck);
	}

	protected override void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
		if (deckObject != null)
		{
			onNameChange?.InvokeLocalized(this, () => deckObject?.contentRef.friendlyName);
			onDescriptionChange?.InvokeLocalized(this, () => deckObject?.contentRef.GetDescription());
		}
	}

	public void Open()
	{
		CardAdditionalFoleySoundPack cardAdditionalFoleySoundPack = GameStateView.Instance?.foleySoundPack;
		cardAdditionalFoleySoundPack?.openDeck.Play(base.transform, ACardLayout.Random, cardAdditionalFoleySoundPack.mixerGroup);
		animator.SetBool(OPEN_ID, value: true);
	}

	public void Close()
	{
		CardAdditionalFoleySoundPack cardAdditionalFoleySoundPack = GameStateView.Instance?.foleySoundPack;
		cardAdditionalFoleySoundPack?.closeDeck.Play(base.transform, ACardLayout.Random, cardAdditionalFoleySoundPack.mixerGroup);
		animator.SetBool(OPEN_ID, value: false);
	}
}
