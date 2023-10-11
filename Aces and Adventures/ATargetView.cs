using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ATargetView : CardLayoutElement, IProjectileExtrema
{
	private static TextBuilder _Builder;

	private static readonly Dictionary<ATarget, ATargetView> _Map = new Dictionary<ATarget, ATargetView>();

	private static readonly Dictionary<object, PoolKeepItemDictionaryHandle<ATargetView, GlowTags>> _OwnedGlowRequests = new Dictionary<object, PoolKeepItemDictionaryHandle<ATargetView, GlowTags>>();

	[SerializeField]
	protected ColorEvent _onGlowColorChange;

	[SerializeField]
	protected BoolEvent _onGlowEnabledChange;

	[SerializeField]
	protected BoolEvent _onFrontIsVisibleChange;

	public CardTargetTransforms cardTargets;

	private ATarget _target;

	private Dictionary<object, Color> _glowRequests;

	private bool? _frontIsVisible;

	protected static TextBuilder Builder => _Builder ?? (_Builder = new TextBuilder(clearOnToString: true));

	protected ATarget target
	{
		get
		{
			return _target;
		}
		set
		{
			if (_target != value)
			{
				_OnTargetSet(_target, value);
			}
		}
	}

	public override ATarget card
	{
		get
		{
			return target;
		}
		protected set
		{
			target = value;
		}
	}

	public GameStateView view => GameStateView.Instance;

	private Dictionary<object, Color> glowRequests => _glowRequests ?? (_glowRequests = new Dictionary<object, Color>());

	public bool hasGlow => !_glowRequests.IsNullOrEmpty();

	public bool frontIsVisible
	{
		get
		{
			return _frontIsVisible.GetValueOrDefault();
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _frontIsVisible, value))
			{
				_onFrontIsVisibleChange?.Invoke(value);
			}
		}
	}

	public Transform this[CardTarget cardTarget] => cardTargets?[cardTarget] ?? base.transform;

	public static ATargetView GetView(ATarget target)
	{
		return _Map.GetValueOrDefault(target);
	}

	public static void ClearGlowRequestsFrom<T>(object glowRequestOwner, GlowTags tags = (GlowTags)0, bool areExceptTags = false) where T : ATarget
	{
		PoolKeepItemDictionaryHandle<ATargetView, GlowTags> valueOrDefault = _OwnedGlowRequests.GetValueOrDefault(glowRequestOwner);
		if (valueOrDefault == null)
		{
			return;
		}
		foreach (KeyValuePair<ATargetView, GlowTags> item in valueOrDefault.value.EnumeratePairsSafe())
		{
			if (item.Key.target is T)
			{
				item.Key.ReleaseGlow(glowRequestOwner, tags, areExceptTags);
			}
		}
	}

	public static void ClearAllGlowRequests(GlowTags tags = (GlowTags)0)
	{
		if (_OwnedGlowRequests.Count == 0)
		{
			return;
		}
		foreach (object item in _OwnedGlowRequests.EnumerateKeysSafe())
		{
			ClearGlowRequestsFrom<ATarget>(item, tags);
		}
	}

	public static void ClearAllGlowRequestsExcept(GlowTags tags)
	{
		if (_OwnedGlowRequests.Count == 0)
		{
			return;
		}
		foreach (object item in _OwnedGlowRequests.EnumerateKeysSafe())
		{
			ClearGlowRequestsFrom<ATarget>(item, tags, areExceptTags: true);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		foreach (object item in glowRequests.EnumerateKeysSafe())
		{
			ReleaseGlow(item);
		}
	}

	protected virtual void OnDestroy()
	{
		if (!UJobManager.IsQuitting)
		{
			target?.Unregister();
			target = null;
		}
	}

	private void _OnTargetSet(ATarget oldTarget, ATarget newTarget)
	{
		if (oldTarget != null && GetView(oldTarget) == this)
		{
			_Map.Remove(oldTarget);
		}
		if (newTarget != null)
		{
			_Map[newTarget] = this;
		}
		_target = newTarget;
		_OnTargetChange(oldTarget, newTarget);
	}

	protected virtual void _OnTargetChange(ATarget oldTarget, ATarget newTarget)
	{
	}

	public virtual void ShowTooltips()
	{
	}

	public virtual void HideTooltips()
	{
	}

	public ATargetView SetData(ATarget data)
	{
		target = data;
		return this;
	}

	public void RequestGlow(object requestedBy, Color color, GlowTags tags = (GlowTags)0)
	{
		if (color.a <= 0f)
		{
			ReleaseGlow(requestedBy);
			return;
		}
		if (glowRequests.Count == 0)
		{
			_onGlowEnabledChange?.Invoke(arg0: true);
		}
		_onGlowColorChange?.Invoke(color);
		glowRequests[requestedBy] = color;
		if (!_OwnedGlowRequests.ContainsKey(requestedBy))
		{
			_OwnedGlowRequests.Add(requestedBy, Pools.UseKeepItemDictionary<ATargetView, GlowTags>());
		}
		_OwnedGlowRequests[requestedBy][this] = tags;
	}

	public void ReleaseGlow(object requestedBy, GlowTags tags = (GlowTags)0, bool areExceptTags = false)
	{
		if (tags != 0)
		{
			PoolKeepItemDictionaryHandle<ATargetView, GlowTags> valueOrDefault = _OwnedGlowRequests.GetValueOrDefault(requestedBy);
			if (valueOrDefault != null)
			{
				GlowTags glowTags = valueOrDefault.value.GetValueOrDefault(this) & tags;
				if (areExceptTags ? ((byte)glowTags != 0) : (glowTags != tags))
				{
					return;
				}
			}
		}
		if (glowRequests.Remove(requestedBy))
		{
			PoolKeepItemDictionaryHandle<ATargetView, GlowTags> poolKeepItemDictionaryHandle = _OwnedGlowRequests[requestedBy];
			if (poolKeepItemDictionaryHandle.Remove(this) && poolKeepItemDictionaryHandle.Count == 0 && _OwnedGlowRequests.Remove(requestedBy))
			{
				Pools.Repool(poolKeepItemDictionaryHandle);
			}
			if (glowRequests.Count == 0)
			{
				_onGlowEnabledChange?.Invoke(arg0: false);
			}
			else
			{
				_onGlowColorChange?.Invoke(glowRequests.Values.Last());
			}
		}
	}

	public void RequestGlow(object requestedBy, Color color, bool request, GlowTags tags = (GlowTags)0)
	{
		if (request)
		{
			RequestGlow(requestedBy, color, tags);
		}
		else
		{
			ReleaseGlow(requestedBy, tags);
		}
	}

	public void ReleaseOwnedGlowRequests(GlowTags tags = (GlowTags)0)
	{
		ReleaseOwnedGlowRequestsFor<ATarget>(tags);
	}

	public void ReleaseOwnedGlowRequestsFor<T>(GlowTags tags = (GlowTags)0) where T : ATarget
	{
		ClearGlowRequestsFrom<T>(this, tags);
	}

	public void DestroyCard()
	{
		base.deck?.DestroyCard(card);
	}

	public void RepoolCard()
	{
		base.deck?.RepoolCard(card);
	}

	public Transform GetTargetForProjectile(CardTarget cardTarget)
	{
		return this[cardTarget];
	}

	[SpecialName]
	Transform IProjectileExtrema.get_transform()
	{
		return base.transform;
	}
}
