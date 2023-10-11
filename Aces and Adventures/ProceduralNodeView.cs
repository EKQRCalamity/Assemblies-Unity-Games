using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PointerClick3D), typeof(PointerOver3D), typeof(RectTransform))]
public class ProceduralNodeView : MonoBehaviour
{
	private static Dictionary<ProceduralNodeType, Sprite> _SpriteMap;

	private static readonly ResourceBlueprint<GameObject> Blueprint = "GameState/Procedural/ProceduralNodeView";

	private static readonly ResourceBlueprint<GameObject> EditorBlueprint = "GameState/Procedural/ProceduralNodeEditorView";

	public BoolEvent onActiveChange;

	public BoolEvent onSelectableChange;

	public BoolEvent onSelectedChange;

	public BoolEvent onCompletedChange;

	public BoolEvent onLastSelectedChange;

	public BoolEvent onHighlightedChange;

	public BoolEvent onCustomHighlightedChange;

	public BoolEvent onMarkedChange;

	public SpriteEvent onSpriteChange;

	public FloatEvent onScaleChange;

	public IntEvent onSkinChange;

	protected ProceduralMapView _graphView;

	private RectTransform _rect;

	private static Dictionary<ProceduralNodeType, Sprite> SpriteMap => _SpriteMap ?? (_SpriteMap = ReflectionUtil.CreateEnumResourceMap<ProceduralNodeType, Sprite>("GameState/Procedural/NodeType"));

	public ProceduralNode node { get; private set; }

	public ProceduralGraph graph => _graphView?.map?.graph;

	public GameState state => view.state;

	public GameStateView view => _graphView?.view;

	protected uint _nodeId => node?.id ?? 0;

	public RectTransform rect => this.CacheComponent(ref _rect);

	public float radius => new Rect3D(rect).size.x * 0.5f;

	public bool shouldBeHighlighted
	{
		get
		{
			if (!node.isSelected && !node.isSelectable)
			{
				return node.highlighted;
			}
			return true;
		}
	}

	public static ProceduralNodeView Create(ProceduralMapView graphView, ProceduralNode startNode, Transform parent, bool isEditor = false)
	{
		return Pools.Unpool(isEditor ? EditorBlueprint : Blueprint, parent).GetComponent<ProceduralNodeView>().SetData(graphView, startNode);
	}

	private IEnumerable<ATarget> _GetCardTooltips()
	{
		return node.data.cards.GenerateCards(state);
	}

	private void _OnStateChange(ProceduralNode.State previousState, ProceduralNode.State state)
	{
		onActiveChange?.Invoke(node.isActive);
		onSelectableChange?.Invoke(node.isSelectable);
		onSelectedChange?.Invoke(node.isSelected);
		onCompletedChange?.Invoke(node.isCompleted);
		onLastSelectedChange?.Invoke(node.isLastSelected);
		onCustomHighlightedChange?.Invoke(node.customHighlighted && node.isActive);
		onMarkedChange?.Invoke(node.marked && node.isActive && !node.isSelected);
		onHighlightedChange?.Invoke(shouldBeHighlighted);
	}

	protected virtual void _OnPointerEnter(PointerEventData eventData)
	{
		ProceduralNode proceduralNode = node;
		if (proceduralNode == null || !proceduralNode.hasBeenGenerated)
		{
			return;
		}
		node.isPointerOver = true;
		ProceduralMapDeckLayout obj = _graphView.deck as ProceduralMapDeckLayout;
		if ((object)obj == null || obj.dragPlane?.isDragging != true)
		{
			if (node.type.ShowCardTooltips(state.parameters.viewMapNodes || node.isCompleted || ProfileManager.options.devModeEnabled) && node.data.hasCards)
			{
				view.ShowCardsAsTooltip(_GetCardTooltips, rect);
			}
			else
			{
				ProjectedTooltipFitter.Create(node.type.LocalizeName().Localize(), base.gameObject, GameStateView.Instance?.tooltipCanvas, TooltipAlignment.BottomCenter, 0, TooltipOptionType.Map);
			}
			node.highlighted = true;
		}
	}

	protected virtual void _OnPointerExit(PointerEventData eventData)
	{
		ProceduralNode proceduralNode = node;
		if (proceduralNode != null && proceduralNode.hasBeenGenerated)
		{
			SignalPointerExit();
			node.isPointerOver = false;
			view.HideCardsShownAsTooltip();
		}
	}

	protected virtual void _OnClick(PointerEventData eventData)
	{
		_graphView?.SignalNodeClick(node, eventData);
	}

	protected virtual void _OnRightClick(PointerEventData eventData)
	{
		node.marked = !node.marked;
	}

	protected virtual void _OnMiddleClick(PointerEventData eventData)
	{
		_graphView.CustomHighlightNodesOfType(node.type, !node.customHighlighted);
	}

	protected virtual void Start()
	{
		PointerOver3D component = GetComponent<PointerOver3D>();
		component.OnEnter.AddListener(_OnPointerEnter);
		component.OnExit.AddListener(_OnPointerExit);
		PointerClick3D component2 = GetComponent<PointerClick3D>();
		component2.OnClick.AddListener(_OnClick);
		component2.OnRightClick.AddListener(_OnRightClick);
		component2.OnMiddleClick.AddListener(_OnMiddleClick);
	}

	protected virtual void OnDisable()
	{
		if (node != null)
		{
			node.onStateChange -= _OnStateChange;
		}
		_graphView = null;
		node = null;
	}

	public void UpdatePosition()
	{
		base.transform.localPosition = _graphView.ToVector3Position(node.position);
	}

	public void UpdateNodeType()
	{
		onSpriteChange?.Invoke(SpriteMap[node.type]);
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, node.type.Size());
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, node.type.Size());
	}

	public virtual ProceduralNodeView SetData(ProceduralMapView graphToSet, ProceduralNode nodeToSet)
	{
		_graphView = graphToSet;
		node = nodeToSet;
		nodeToSet.onStateChange += _OnStateChange;
		_OnStateChange(nodeToSet.state, nodeToSet.state);
		UpdatePosition();
		UpdateNodeType();
		onSkinChange?.Invoke((int)graphToSet.map.data.skin);
		return this;
	}

	public void SetUniformScale(float scale)
	{
		if (base.transform.localScale.x != scale)
		{
			base.transform.localScale = new Vector3(scale, scale, scale);
			onScaleChange?.Invoke(scale);
		}
	}

	public void SignalPointerOver()
	{
		_OnPointerEnter(null);
	}

	public void SignalPointerExit()
	{
		ProjectedTooltipFitter.Finish(base.gameObject);
		node.highlighted = false;
	}
}
