using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DeckController : MonoBehaviour {

	private static bool debugMode;
	private static GameController gameController;

	private AudioSource shuffleDeckAudio;

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

	private struct Card {
		public int suit;
		public int rank;

		public Card(int s, int r) {
			suit = s;
			rank = r;
		}
	}

	private GameObject cardObject;
	private Vector3 boardOrigin;

	// Number of cards in a row/column for the game.
	// Game has a square card distribution, so only one dimension should be required.
	public int boardCardWidth = 4;
	// Spacing between cards on the X-axis.
	public float boardCardSpacingX = 0.5f;
	// Spacing between cards on the Y-axis.
	public float boardCardSpacingY = 1.0f;

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

		// Loadings sound effects.
		if (null == this.shuffleDeckAudio) {
			if (debugMode) {
				Debug.Log("Instantiating audio source.");
			}
			
			this.shuffleDeckAudio = this.gameObject.GetComponent<AudioSource>();
		}

		// Associates the card prefab.
		if (null == this.cardObject) {
			this.cardObject = (GameObject)Resources.Load("Prefabs/Card");
		}

		if (debugMode) {
			Debug.Log ("Card object set to " + this.cardObject);
		}

		// Sets origin coordinates for card distribution.
		this.boardOrigin = GameObject.Find("Anchor").transform.position;

		if (debugMode) {
			Debug.Log ("Set board origin " + this.boardOrigin);
		}
	}

	// Use this for initialization
	void Start() {
		this.dealCards();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown() {
		if (gameController.getVictory()) {
			this.dealCards();
		}
	}

	// Selects and places cards, then initializes the game.
	void dealCards() {
		// Remove any existing cards on the board.
		GameObject[] dealtCards = GameObject.FindGameObjectsWithTag("DealtCard");
		foreach (GameObject dealtCard in dealtCards) {
			Destroy(dealtCard);
		}

		int numberOfCards = this.boardCardWidth * this.boardCardWidth;
		int numberOfCardPairs = numberOfCards / 2;
		Card[] selectedCards = new Card[numberOfCards];
		
		string[] suitNames = Enum.GetNames(typeof(Suits));
		string[] rankNames = Enum.GetNames(typeof(Ranks));
		int deckSize = suitNames.Length * rankNames.Length;
		
		// Cards should all have the same initial facing (face-down).
		Vector3 cardRotation = new Vector3(180.0f, 0.0f, 0.0f);
		
		// Selects cards suits/ranks to be used in the game.
		System.Random rng = new System.Random();
		
		// Creates list of available card positions on the board.
		List<int> cardPlacements = new List<int>(numberOfCards);
		for (int i = 0; i < numberOfCards; i++) {
			cardPlacements.Add(i);
		}
		
		// Creates list of available cards to select from.
		List<Card> availableCards = new List<Card>(deckSize);
		for (int suit = 0; suit < suitNames.Length; suit++) {
			for (int rank = 0; rank < rankNames.Length; rank++) {
				Card card = new Card(suit, rank);
				availableCards.Add(card);
			}
		}

		shuffleDeckAudio.Play();
		for (int i = 0; i < numberOfCardPairs; i++) {
			// Selects an available card.
			int availableCardIndex = rng.Next(availableCards.Count);
			selectedCards[i] = availableCards[availableCardIndex];
			availableCards.RemoveAt(availableCardIndex);
			
			// Places two copies of each selected card on the board.
			for (int j = 0; j < 2; j++) {
				// Selects a random place for the next card.
				int cardPlacementIndex = rng.Next(cardPlacements.Count);
				int cardPlacement = cardPlacements[cardPlacementIndex];
				cardPlacements.RemoveAt(cardPlacementIndex);
				
				// Calculates the coordinates of the selected place.
				float cardPositionX = this.boardOrigin.x + ((float)cardPlacement % this.boardCardWidth) * this.boardCardSpacingX;
				float cardPositionY = this.boardOrigin.y - (float)Math.Floor((double)cardPlacement / this.boardCardWidth) * this.boardCardSpacingY;
				Vector3 cardPosition = new Vector3(cardPositionX, cardPositionY, this.cardObject.transform.position.z);
				
				// Generates a card object at the selected position.
				GameObject card = Instantiate(this.cardObject) as GameObject;
				// Set the deck as the parent for the card.
				card.transform.position = cardPosition;
				card.transform.Rotate(cardRotation);
				card.GetComponent<CardController>().setSuit(selectedCards[i].suit);
				card.GetComponent<CardController>().setRank(selectedCards[i].rank);
				card.GetComponent<CardController>().updateSprite();
				
				if (debugMode) {
					Debug.Log("Instantiating card at position " + cardPlacement + ", coodinates " + cardPosition);
				}
			}
		}
		
		// Initializes game after cards are dealt.
		gameController.initializeGame(numberOfCardPairs);
	}
}