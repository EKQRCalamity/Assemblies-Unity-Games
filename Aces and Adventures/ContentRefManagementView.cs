using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ContentRefManagementView : MonoBehaviour
{
	private static ResourceBlueprint<GameObject> _Blueprint = "UI/Content/ContentRefManagementView";

	public RectTransform dependentsContainer;

	public RectTransform creatorTooltipTransform;

	[Header("Events")]
	public StringEvent onNameChange;

	public StringEvent onCreatorChange;

	public ColorEvent onCreatorColorChange;

	public StringEvent onDependantsCountChange;

	public BoolEvent onCanDeleteChange;

	public BoolEvent onCanCreateDependentViewsChange;

	public BoolEvent onIsRecursiveChange;

	public BoolEvent onIsUsedByDefaultsChange;

	public SpriteEvent onContentTypeSpriteChange;

	public ContentRef contentRef { get; private set; }

	public static ContentRefManagementView Create(ContentRef contentRef, Transform parent = null)
	{
		return Pools.Unpool(_Blueprint, parent, setActive: false).GetComponent<ContentRefManagementView>()._SetData(contentRef);
	}

	private ContentRefManagementView _SetData(ContentRef cRef)
	{
		contentRef = cRef;
		base.gameObject.SetActive(value: true);
		onNameChange.Invoke($"{cRef.name} <size=75%><i>{cRef.specificTypeFriendly}");
		ContentCreatorType creatorType = cRef.creatorType;
		onCreatorChange.Invoke(EnumUtil.FriendlyName(creatorType));
		onCreatorColorChange.Invoke(creatorType.GetTint());
		int dependentsCount = ContentRefManager.Instance.GetDependentsCount(cRef);
		onDependantsCountChange.Invoke(dependentsCount.ToString());
		onCanDeleteChange.Invoke(ContentRefManager.Instance.CanBeDeleted(cRef));
		onCanCreateDependentViewsChange.Invoke(dependentsCount > 0);
		onIsUsedByDefaultsChange.Invoke(ContentRefManager.Instance.IsUsedByDefaults(cRef));
		TooltipCreator.CreateTextTooltip(creatorTooltipTransform, "Last Updated: " + $"{MathUtil.FromUnixEpoch(cRef.lastUpdateTime).ToLocalTime():f}", beginShowTimer: false, 0.2f, backgroundEnabled: true, TextAlignmentOptions.Center, 0f, 12f, TooltipDirection.Vertical);
		onContentTypeSpriteChange.Invoke(ContentRefManager.ContentTypeSpriteMap[cRef.contentType]);
		return this;
	}

	private void OnDisable()
	{
		contentRef = null;
		CollapseFitter componentInChildren = GetComponentInChildren<CollapseFitter>();
		if ((bool)componentInChildren)
		{
			componentInChildren.ForceClose();
		}
		dependentsContainer.SetChildrenActive(active: false);
	}

	private void OnEnable()
	{
		onIsRecursiveChange.Invoke(base.gameObject.GetComponentsInParentsPooled<ContentRefManagementView>(includeInactive: false, includeSelf: false).AsEnumerable().Any((ContentRefManagementView cRef) => ContentRef.Equal(cRef.contentRef, contentRef)));
	}

	public void View()
	{
		UIUtil.CreateContentSearchPopup(contentRef, delegate(ContentRef cRef)
		{
			cRef.OnEditRequest(base.transform, delegate
			{
				ContentRefManager.Instance.Refresh();
			});
		}, base.transform, contentRef.name, EnumUtil<ContentCreatorType>.ConvertToFlag<ContentCreatorTypeFlags>(contentRef.creatorType));
	}

	public void Delete()
	{
		string name = contentRef.name + " (" + contentRef.specificTypeFriendly + ")";
		string deleteButton = "Delete " + name;
		string deepDeleteButton = "Deep Delete...";
		string[] array = ((!contentRef.isDataRef) ? new string[2] { "Cancel", deleteButton } : new string[3] { "Cancel", deleteButton, deepDeleteButton });
		string title = $"Delete {name}";
		GameObject mainContent = UIUtil.CreateMessageBox($"Are you certain you wish to delete <nobr><b><u>{name}</u></b></nobr>? This process cannot be undone.", TextAlignmentOptions.Left, 32, 600, 300, 24f);
		Transform parent = base.transform;
		string[] buttons = array;
		UIUtil.CreatePopup(title, mainContent, null, null, null, null, null, null, displayCloseButton: true, blockAllRaycasts: true, null, delegate(string s)
		{
			if (s == deleteButton)
			{
				contentRef.Delete();
				ContentRefManager.Instance.Refresh();
			}
			else if (s == deepDeleteButton)
			{
				HashSet<ContentRef.Key> allDependencies;
				HashSet<ContentRef> contentRefsToBeDeleted = ContentRef.DeepDelete(contentRef, ContentRefManager.Instance.dependentsCache, out allDependencies);
				foreach (ContentRef item in contentRefsToBeDeleted)
				{
					allDependencies.Remove(item);
				}
				UIUtil.CreatePopup("<color=red>Deep Delete</color> " + name, UIUtil.CreateLogTextBox("<color=red>You are about to <b>delete</b> [" + name + "] and <i><b>ALL</b></i> of its unique dependencies.\nThis process <b>cannot be undone</b>.</color>\n<b>The following " + contentRefsToBeDeleted.Count + " item(s) will be deleted:</b>\n" + contentRefsToBeDeleted.OrderByComparer(ContentRefDeepDeleteComparer.Default).ToStringSmart((ContentRef c) => "<size=75%>[" + EnumUtil.FriendlyName(c.creatorType) + "] " + c.specificTypeFriendly + ":</size> " + c.friendlyName, "\n") + "\n<b>The following " + allDependencies.Count + " item(s) will not be deleted due to having other dependents:</b>\n" + allDependencies.Select(ContentRef.GetByKey<ContentRef>).OrderByComparer(ContentRefDeepDeleteComparer.Default).ToStringSmart((ContentRef c) => "<size=75%>[" + EnumUtil.FriendlyName(c.creatorType) + "] " + c.specificTypeFriendly + ":</size> " + c.friendlyName, "\n")), null, parent: base.transform, buttons: new string[2] { "Cancel", "Confirm Deep Delete" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string b)
				{
					if (!(b != "Confirm Deep Delete"))
					{
						foreach (ContentRef item2 in contentRefsToBeDeleted)
						{
							item2.Delete();
						}
						ContentRefManager.Instance.Refresh();
					}
				});
			}
		}, null, parent, null, null, buttons);
	}

	public void CreateDependentViews()
	{
		if (dependentsContainer.childCount != 0)
		{
			return;
		}
		foreach (ContentRef dependent in ContentRefManager.Instance.GetDependents(contentRef))
		{
			Create(dependent, dependentsContainer);
		}
	}
}
