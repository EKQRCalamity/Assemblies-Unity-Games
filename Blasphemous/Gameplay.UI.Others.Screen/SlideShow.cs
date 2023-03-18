using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.Screen;

[RequireComponent(typeof(Image))]
public class SlideShow : MonoBehaviour
{
	private const float MIN_ALPHA = 0.03f;

	private const float MAX_ALPHA = 0.95f;

	public Image[] slides;

	public float changeTime = 10f;

	private int currentSlide;

	private int slideCounter;

	private float timeSinceLast = 1f;

	private Image currentImage;

	private RectTransform rectTransform;

	private float currentAlpha;

	public float timeShowing = 2f;

	private float deltaTimeShowing;

	private bool isShowing;

	private bool isActive;

	private int slidesNumber;

	private int countCycle;

	private bool hasCountCycle;

	public bool IsActive
	{
		get
		{
			return isActive;
		}
		set
		{
			isActive = value;
		}
	}

	private void Awake()
	{
		currentImage = GetComponent<Image>();
		rectTransform = GetComponent<RectTransform>();
		slidesNumber = slides.Length;
		hasCountCycle = false;
		isActive = false;
	}

	private void Start()
	{
		currentImage.sprite = slides[currentSlide].sprite;
		rectTransform.sizeDelta = new Vector2(slides[currentSlide].sprite.textureRect.width, slides[currentSlide].sprite.textureRect.height);
		countCycle = 0;
		slideCounter++;
		currentSlide = slideCounter % slidesNumber;
	}

	private void Update()
	{
		if (isActive)
		{
			Color color = currentImage.color;
			color.a = lerpAlpha();
			currentImage.color = color;
			timeSinceLast += Time.deltaTime;
		}
	}

	private void changeImage()
	{
		currentImage.sprite = slides[currentSlide].sprite;
		rectTransform.sizeDelta = new Vector2(slides[currentSlide].sprite.textureRect.width, slides[currentSlide].sprite.textureRect.height);
		timeSinceLast = 0f;
		slideCounter++;
		currentSlide = slideCounter % slidesNumber;
	}

	private float lerpAlpha()
	{
		if (isShowing)
		{
			deltaTimeShowing += Time.deltaTime;
			if (deltaTimeShowing <= timeShowing)
			{
				return currentAlpha;
			}
			deltaTimeShowing = 0f;
			isShowing = false;
		}
		float t = Mathf.PingPong(Time.time, changeTime) / changeTime;
		currentAlpha = Mathf.Lerp(0f, 1f, t);
		if (currentAlpha < 0.03f && !hasCountCycle)
		{
			hasCountCycle = true;
			countCycle++;
		}
		else if (currentAlpha > 0.95f && hasCountCycle && !isShowing)
		{
			hasCountCycle = false;
			isShowing = true;
		}
		if (countCycle > 0)
		{
			countCycle = 0;
			changeImage();
		}
		return currentAlpha;
	}
}
