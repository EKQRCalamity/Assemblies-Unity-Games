using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class GameStepLevelUp : GameStep
{
	public static ResourceBlueprint<GameObject> CardLock = "GameState/CardLockView";

	public const float HERO_ABILITY_DRAW_TIME = 0.3f;

	private LocalizedString _loggedMessage;

	protected IdDeck<HeroDeckPile, Ability> heroDeck => base.state.player.heroDeck;

	public override bool canSafelyCancelStack => true;

	private void _OnAbilityClick(HeroDeckPile pile, Ability card)
	{
		if (pile != HeroDeckPile.SelectionHand || base.finished)
		{
			return;
		}
		if (card.view.hasOffset)
		{
			base.view.LogError(LevelUpMessages.CollectManaToUnlock.Localize(), base.state.player.audio.character.error.cannotUseNow);
			return;
		}
		_ClearGlows();
		base.view.ClearMessage();
		base.state.player.abilityDeck.Transfer(card, card.actPile);
		_ClearSelectionHand();
		if (card.isTrait)
		{
			base.state.player.AddTrait(card);
		}
		base.finished = true;
	}

	private void _ClearSelectionHand()
	{
		foreach (Ability card in heroDeck.GetCards(HeroDeckPile.SelectionHand))
		{
			PointerOver3D pointerOver = card.view.pointerOver;
			bool flag2 = (card.view.pointerDrag.enabled = true);
			pointerOver.enabled = flag2;
		}
		heroDeck.TransferPile(HeroDeckPile.SelectionHand, HeroDeckPile.Discard);
		base.view.heroDeckLayout.RestoreLayoutToDefault(HeroDeckPile.SelectionHand);
	}

	protected override void OnEnable()
	{
		heroDeck.layout.onPointerClick += _OnAbilityClick;
		if (_loggedMessage != null)
		{
			base.view.LogMessage(_loggedMessage);
		}
		MapCompassView instance = MapCompassView.Instance;
		if ((object)instance != null)
		{
			instance.canBeActiveWhileCancelIsActive = true;
		}
	}

	protected override IEnumerator Update()
	{
		AbilityData.Category? drawCategory = heroDeck.NextInPile()?.data.category;
		bool isUnrestrictedTrait = base.state.traitRuleset == TraitRuleset.Unrestricted && (drawCategory?.IsTrait() ?? false);
		int drawCount = (isUnrestrictedTrait ? 9 : heroDeck.GetCards(HeroDeckPile.Draw).Count((Ability a) => a.data.category == drawCategory));
		if (isUnrestrictedTrait)
		{
			base.view.heroDeckLayout.SetLayout(HeroDeckPile.SelectionHand, base.view.heroDeckLayout.selectionHandUnrestricted);
		}
		if (base.state.encounterNumber > 0 && drawCount > 0)
		{
			yield return AppendStep(new GameStepProjectileMedia(ContentRef.Defaults.media.adventureLevelUp[base.state.parameters.numberOfLevelUpgrades], base.state.player, base.state.player));
			base.state.Heal(new ActionContext(base.state.player, null, base.state.player), base.state.parameters.levelUpHealAmount);
		}
		GameStepLevelUp gameStepLevelUp = this;
		IdDeck<HeroDeckPile, Ability> idDeck = heroDeck;
		float wait = ((drawCount <= 1) ? 0f : (0.1f / (float)((!isUnrestrictedTrait) ? 1 : 2)));
		yield return gameStepLevelUp.AppendStep(idDeck.DrawStep(drawCount, null, null, null, wait));
		if (heroDeck.Count(HeroDeckPile.SelectionHand) == 0)
		{
			Cancel();
			yield break;
		}
		if (base.state.encounterNumber == 0 && heroDeck.Count(HeroDeckPile.SelectionHand) == 1)
		{
			Ability ability = heroDeck.GetCards(HeroDeckPile.SelectionHand).First();
			if (ability != null && ability.data.category == AbilityData.Category.HeroAbility)
			{
				base.view.dofShifter.RemoveTarget(ability.view.transform);
				foreach (float item in Wait(0.3f))
				{
					_ = item;
					yield return null;
				}
				_OnAbilityClick(HeroDeckPile.SelectionHand, ability);
				yield break;
			}
		}
		VoiceManager.Instance.Play(base.state.player.view.transform, base.state.player.audio.character.level, interrupt: true);
		foreach (Ability card in heroDeck.GetCards(HeroDeckPile.SelectionHand))
		{
			card.view.RequestGlow(this, Colors.TARGET);
		}
		AbilityData.Category category = heroDeck.NextInPile(HeroDeckPile.SelectionHand).data.category;
		if (category.IsTrait() && base.state.traitRuleset == TraitRuleset.Standard)
		{
			using PoolKeepItemHashSetHandle<DataRef<AbilityData>> poolKeepItemHashSetHandle = base.state.player.GetLockedLevelUpAbilities();
			foreach (Ability card2 in heroDeck.GetCards(HeroDeckPile.SelectionHand))
			{
				if (poolKeepItemHashSetHandle.Contains(card2.dataRef))
				{
					PointerOver3D pointerOver = card2.view.pointerOver;
					bool flag2 = (card2.view.pointerDrag.enabled = false);
					pointerOver.enabled = flag2;
					Quaternion quaternion = Quaternion.Euler(0f, 0f, 180f);
					GameObject gameObject = Object.Instantiate(CardLock.value, card2.view.transform, worldPositionStays: false);
					gameObject.transform.localRotation = quaternion;
					int? unlockLevelForAbility = base.state.player.characterData.GetUnlockLevelForAbility(card2.dataRef);
					if (unlockLevelForAbility.HasValue)
					{
						int valueOrDefault = unlockLevelForAbility.GetValueOrDefault();
						gameObject.GetComponentInChildren<TextMeshProUGUI>().text = valueOrDefault.ToString();
					}
					card2.view.offsets.Add(Matrix4x4.TRS(base.view.heroDeckLayout.selectionHand.layoutTarget.transform.up * -0.00375f, quaternion, Vector3.one));
					card2.view.RequestGlow(this, Colors.FAILURE);
				}
			}
		}
		base.view.LogMessage(_loggedMessage = (isUnrestrictedTrait ? AdventureTutorial.SelectTrait.Localize() : category.LocalizeLevelUp()));
		while (!base.finished)
		{
			yield return null;
		}
	}

	protected override void OnDisable()
	{
		heroDeck.layout.onPointerClick -= _OnAbilityClick;
		base.view.ClearMessage();
		MapCompassView instance = MapCompassView.Instance;
		if ((object)instance != null)
		{
			instance.canBeActiveWhileCancelIsActive = false;
		}
	}

	protected override void OnDestroy()
	{
		_ClearSelectionHand();
	}
}
