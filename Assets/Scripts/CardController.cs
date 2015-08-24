using UnityEngine;
using System;
using System.Collections;

public class CardController : MonoBehaviour {

	private static bool debugMode;
	private static Sprite[] sprites;
	private static string[] spriteNames;
	private static GameController gameController;

	private AudioSource flipCardAudio;

	private enum Suits : int {
		clubs,
		diamonds,
		hearts,
		spades
	};
	
	private enum Ranks : int {
		ace,
		one,
		two,
		three,
		four,
		five,
		six,
		seven,
		eight,
		nine,
		ten,
		jack,
		queen,
		king
	};

	private bool isFlipping = false;
	private bool isFlippingDisabled = false;
	private int suit;
	private int rank;

	public bool isFaceUp = true;
	public float completeRotationAngle = 180f;
	public float maxRisePosition = 0.1f;

	void Awake() {
		// Debug logging switch.
		debugMode = false;

		// Creates a reference to the game manager.
		if (null == gameController) {
			if (debugMode) {
				Debug.Log("Instantiating game controller.");
			}

			gameController = GameObject.Find("Game").GetComponent<GameController>();
		}

		// Loads sound effects.
		if (null == this.flipCardAudio) {
			if (debugMode) {
				Debug.Log("Instantiating audio source.");
			}

			this.flipCardAudio = this.gameObject.GetComponent<AudioSource>();
		}

		// Loads all sprite textures.
		if (null == sprites || null == spriteNames) {
			if (debugMode) {
				Debug.Log("Loading sprites.");
			}

			sprites = Resources.LoadAll<Sprite> ("Textures");
			spriteNames = new string[sprites.Length];

			for (int i = 0; i < spriteNames.Length; i++) {
				spriteNames [i] = sprites [i].name;
			}
		}
	}

	// Use this for initialization
	void Start() {
		if (isFaceUp) {
			this.transform.Rotate(new Vector3(0.0f, 0.0f, 0.0f));
		} else {
			this.transform.Rotate(new Vector3(180.0f, 0.0f, 0.0f));
		}
	}
	
	// Update is called once per frame
	void Update() {
	}

	void OnMouseDown() {
		if (!this.isFlipping && !this.isFlippingDisabled) {
			StartCoroutine(this.flipCardBlocking());
		}
	}

	public void setSuit(int s) {
		this.suit = s;
	}

	public void setRank(int r) {
		this.rank = r;
	}

	public int getSuit() {
		return this.suit;
	}

	public int getRank() {
		return this.rank;
	}

	public void enableFlip() {
		this.isFlippingDisabled = false;
	}

	public void disableFlip() {
		this.isFlippingDisabled = true;
	}

	public void flip() {
		if (!this.isFlipping && !this.isFlippingDisabled) {
			StartCoroutine(this.flipCard());
		}
	}

	public void updateSprite() {
		string textureName = "card-" + Enum.GetName(typeof(Suits), this.suit) + "-" + Enum.GetName(typeof(Ranks), this.rank);
		GameObject cardFace = this.transform.FindChild("CardFace").gameObject;
		cardFace.transform.Rotate(0.0f, 0.0f, 0.0f);
		
		SpriteRenderer renderer = cardFace.GetComponent<SpriteRenderer>();
		if (debugMode) {
			Debug.Log ("Attempting to used texture " + textureName);
		}
		//renderer.sprite = sprites[Array.IndexOf(spriteNames, textureName)];
		renderer.sprite = Resources.Load<Sprite>("Textures/" + textureName);
	}

	IEnumerator flipCardBlocking() {
		yield return StartCoroutine(flipCard());
		gameController.GetComponent<GameController>().matchCard(this.gameObject);
	}

	IEnumerator flipCard() {
		if (debugMode) {
			Debug.Log ("Initializing animation.");
		}

		float currentAngle = 0.0f;
		float partialRotationAngle = 0.0f;
		float animationPercentage = 0.0f;
		float riseProgress = 0.0f;
		float risePosition = 0.0f;

		Vector3 initialPosition = this.gameObject.transform.position;
		
		// Calculates desired final angle after rotation.
		Vector3 initialEulerAngle = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
		if (debugMode) {
			Debug.Log ("Initial card angle: " + initialEulerAngle);
		}
		Vector3 finalEulerAngle = initialEulerAngle;
		finalEulerAngle.y += this.completeRotationAngle;

		if (debugMode) {
			Debug.Log ("Initial card position: " + initialPosition);
		}

		this.isFlipping = true;
		flipCardAudio.Play();
		while (this.isFlipping) {
			// Calculate and perform rotation.
			partialRotationAngle = this.completeRotationAngle * Time.deltaTime * 2.0f;
			transform.Rotate(new Vector3(0.0f, partialRotationAngle, 0.0f));
			currentAngle += partialRotationAngle;

			// Calculate the progress of the animation.
			animationPercentage = currentAngle / completeRotationAngle;
			if (debugMode) {
				Debug.Log("Animation percentage: " + (animationPercentage * 100) + "%");
			}

			// Calculate the position of the card based on animation progress.
			riseProgress = 2 * this.maxRisePosition * animationPercentage;
			risePosition = Mathf.Abs(riseProgress - this.maxRisePosition) - this.maxRisePosition;
			transform.position = new Vector3(initialPosition.x, initialPosition.y, risePosition);

			// Fixes final card position and angle to prevent rounding error accumulation.
			if (currentAngle >= this.completeRotationAngle) {
				transform.eulerAngles = finalEulerAngle;
				transform.position = initialPosition;
				this.isFlipping = false;
			}

			yield return null;
		}

		this.isFaceUp = !this.isFaceUp;
		if (debugMode) {
			Debug.Log ("Final card angle: " + currentAngle);
			Debug.Log ("Final card position: " + transform.position);
			Debug.Log ("Card is now " + (this.isFaceUp ? "face up" : "face down") + ".");
		}
	}
}
