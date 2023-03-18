using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.Map;

public class MapRenderer
{
	private class SpriteTransform
	{
		public Sprite BaseSprite;

		public bool FlipX;

		public bool FlipY;

		public string Key = string.Empty;
	}

	private class UIData
	{
		public Image image;

		public Color originalColor;
	}

	private const string RootName = "RootRenderer";

	private float CellSizeX;

	private float CellSizeY;

	private RectTransform markRoot;

	private Dictionary<string, SpriteTransform> spriteTransforms = new Dictionary<string, SpriteTransform>();

	private Dictionary<CellKey, Image> CachedImages = new Dictionary<CellKey, Image>();

	private Dictionary<CellKey, UIData> cellsUI = new Dictionary<CellKey, UIData>();

	public MapRendererConfig Config { get; private set; }

	public RectTransform Root { get; private set; }

	public Vector3 Center
	{
		get
		{
			return Root.localPosition;
		}
		set
		{
			MoveCenterTo(value);
		}
	}

	public MapRenderer(MapRendererConfig config, Transform parent, string name)
	{
		Config = config;
		string name2 = "RootRenderer_" + name;
		Root = (RectTransform)parent.Find(name2);
		if (Root == null)
		{
			Root = CreateRectTranform(parent, name2);
			Root.localRotation = Quaternion.identity;
			Root.localScale = Vector3.one;
			Root.localPosition = Vector3.zero;
		}
		Sprite sprite = Config.Sprites.Values.First();
		CellSizeX = sprite.rect.width;
		CellSizeY = sprite.rect.height;
		spriteTransforms.Clear();
		cellsUI.Clear();
	}

	public CellKey GetCenterCell()
	{
		return new CellKey(Mathf.RoundToInt((0f - Root.localPosition.x) / CellSizeX), Mathf.RoundToInt((0f - Root.localPosition.y) / CellSizeY));
	}

	public void SetCenterCell(CellKey cellKey)
	{
		if (cellKey != null)
		{
			Root.localPosition = GetPosition(cellKey) * -1f;
		}
	}

	public void MoveCenter(Vector3 movement)
	{
		CellKey cellKey = new CellKey(Mathf.RoundToInt((0f - (Root.localPosition.x + movement.x)) / CellSizeX), Mathf.RoundToInt((0f - (Root.localPosition.y + movement.y)) / CellSizeY));
		if (cellKey.X < Config.minCell.x || cellKey.X > Config.maxCell.x)
		{
			movement.x = 0f;
		}
		if (cellKey.Y < Config.minCell.y || cellKey.Y > Config.maxCell.y)
		{
			movement.y = 0f;
		}
		Root.localPosition += movement;
	}

	public void MoveCenterTo(Vector3 position)
	{
		CellKey cellKey = new CellKey(Mathf.RoundToInt((0f - position.x) / CellSizeX), Mathf.RoundToInt((0f - position.y) / CellSizeY));
		if (cellKey.X >= Config.minCell.x && cellKey.X <= Config.maxCell.x && cellKey.Y >= Config.minCell.y && cellKey.Y <= Config.maxCell.y)
		{
			Root.localPosition = position;
		}
	}

	public void Render(List<CellData> revealedCells, List<CellKey> secrets, Dictionary<CellKey, List<MapData.MarkType>> marks, CellKey playerCell)
	{
		Dictionary<CellKey, Image> dictionary = new Dictionary<CellKey, Image>();
		foreach (CellData revealedCell in revealedCells)
		{
			SpriteTransform sprite = GetSprite(revealedCell.Walls, revealedCell.Doors);
			if (sprite.BaseSprite == null)
			{
				Debug.LogError("Map renderer sprite null KEY:" + sprite.Key + " CELL:" + revealedCell.CellKey.X + "_" + revealedCell.CellKey.Y + "  ZONE:" + revealedCell.ZoneId.GetKey());
				continue;
			}
			Image image = null;
			if (CachedImages.ContainsKey(revealedCell.CellKey) && !secrets.Contains(revealedCell.CellKey))
			{
				image = CachedImages[revealedCell.CellKey];
				CachedImages.Remove(revealedCell.CellKey);
			}
			else
			{
				RectTransform rectTransform = CreateRectTranform(Root, revealedCell.CellKey.X + "_" + revealedCell.CellKey.Y);
				rectTransform.localRotation = Quaternion.identity;
				rectTransform.localScale = new Vector3((!sprite.FlipX) ? 1f : (-1f), (!sprite.FlipY) ? 1f : (-1f), 1f);
				rectTransform.localPosition = GetPosition(revealedCell.CellKey);
				rectTransform.sizeDelta = new Vector2(sprite.BaseSprite.rect.width, sprite.BaseSprite.rect.height);
				image = rectTransform.gameObject.AddComponent<Image>();
			}
			dictionary[revealedCell.CellKey] = image;
			image.sprite = sprite.BaseSprite;
			image.material = Config.SpriteMaterial;
			if (revealedCell.CellKey.Equals(playerCell))
			{
				image.color = Config.PlayerBackgoundColor;
			}
			else
			{
				image.color = Config.GetZoneColor(revealedCell.ZoneId);
			}
			UIData uIData = new UIData();
			uIData.originalColor = image.color;
			uIData.image = image;
			cellsUI[revealedCell.CellKey] = uIData;
		}
		foreach (KeyValuePair<CellKey, Image> cachedImage in CachedImages)
		{
			Object.Destroy(cachedImage.Value.gameObject);
		}
		CachedImages = new Dictionary<CellKey, Image>(dictionary);
		if (markRoot != null)
		{
			Object.Destroy(markRoot.gameObject);
		}
		markRoot = CreateRectTranform(Root, "Marks");
		markRoot.localRotation = Quaternion.identity;
		markRoot.localScale = Vector3.one;
		markRoot.localPosition = Vector3.zero;
		markRoot.gameObject.SetActive(value: true);
		UpdateMarks(marks);
	}

	public void UpdateMarks(Dictionary<CellKey, List<MapData.MarkType>> marks)
	{
		foreach (Transform item in markRoot)
		{
			Object.Destroy(item.gameObject);
		}
		foreach (KeyValuePair<CellKey, List<MapData.MarkType>> mark in marks)
		{
			Sprite value = null;
			int num = 0;
			foreach (MapData.MarkType item2 in mark.Value)
			{
				if (Config.Marks.TryGetValue(item2, out value) && value != null)
				{
					RectTransform rectTransform = CreateRectTranform(markRoot, mark.Key.X + "_" + mark.Key.Y + "_" + num);
					rectTransform.localRotation = Quaternion.identity;
					rectTransform.localScale = Vector3.one;
					rectTransform.localPosition = GetPosition(mark.Key);
					rectTransform.sizeDelta = new Vector2(value.rect.width, value.rect.height);
					Image image = rectTransform.gameObject.AddComponent<Image>();
					image.sprite = value;
				}
				num++;
			}
		}
	}

	public void ToogleMarks()
	{
		markRoot.gameObject.SetActive(!markRoot.gameObject.activeSelf);
	}

	public void SetVisibleMarks(bool visible)
	{
		markRoot.gameObject.SetActive(visible);
	}

	public void SetSelected(List<CellKey> cells, bool selected)
	{
		foreach (CellKey cell in cells)
		{
			if (cellsUI.ContainsKey(cell))
			{
				if (selected)
				{
					cellsUI[cell].image.material = Config.SpriteMaterialSelected;
				}
				else
				{
					cellsUI[cell].image.material = Config.SpriteMaterial;
				}
			}
		}
	}

	public void ResetSelection()
	{
		foreach (UIData value in cellsUI.Values)
		{
			value.image.material = Config.SpriteMaterial;
		}
	}

	public Vector2 GetPosition(CellKey cellKey)
	{
		return new Vector2(CellSizeX * (float)cellKey.X, CellSizeY * (float)cellKey.Y);
	}

	private RectTransform CreateRectTranform(Transform parent, string name)
	{
		GameObject gameObject = new GameObject(name, typeof(RectTransform));
		gameObject.transform.SetParent(parent);
		return (RectTransform)gameObject.transform;
	}

	private SpriteTransform GetSprite(bool[] walls, bool[] doors)
	{
		SpriteTransform spriteTransform = new SpriteTransform();
		string text = string.Empty;
		foreach (EditorMapCellData.CellSide item in MapRendererConfig.spriteKeyOrder)
		{
			string text2 = "_";
			int num = (int)item;
			if (doors[num])
			{
				text2 = "D";
			}
			else if (walls[num])
			{
				text2 = "W";
			}
			text += text2;
		}
		spriteTransform.Key = text;
		if (Config.Sprites.ContainsKey(text))
		{
			spriteTransform.BaseSprite = Config.Sprites[text];
		}
		else if (spriteTransforms.ContainsKey(text))
		{
			spriteTransform = spriteTransforms[text];
		}
		else
		{
			string key = text.Substring(2, 1) + text.Substring(1, 1) + text.Substring(0, 1) + text.Substring(3, 1);
			if (Config.Sprites.ContainsKey(key))
			{
				spriteTransform.FlipX = true;
				spriteTransform.BaseSprite = Config.Sprites[key];
				spriteTransforms[key] = spriteTransform;
			}
			else
			{
				string key2 = text.Substring(0, 1) + text.Substring(3, 1) + text.Substring(2, 1) + text.Substring(1, 1);
				if (Config.Sprites.ContainsKey(key2))
				{
					spriteTransform.FlipY = true;
					spriteTransform.BaseSprite = Config.Sprites[key2];
					spriteTransforms[key2] = spriteTransform;
				}
				else
				{
					string key3 = text.Substring(2, 1) + text.Substring(3, 1) + text.Substring(0, 1) + text.Substring(1, 1);
					if (Config.Sprites.ContainsKey(key3))
					{
						spriteTransform.FlipY = true;
						spriteTransform.FlipX = true;
						spriteTransform.BaseSprite = Config.Sprites[key3];
						spriteTransforms[key3] = spriteTransform;
					}
					else
					{
						Debug.LogError("Renderer config: Can find sprite for cell " + text);
					}
				}
			}
		}
		return spriteTransform;
	}
}
