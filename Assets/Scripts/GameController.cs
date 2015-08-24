using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour {

	private static bool debugMode;

	private System.Diagnostics.Stopwatch timer;
	private GameObject card1;
	private bool victory;
	private int matchedPairs;
	private int victoryCondition;

	public Text victoryText;
	public Text gameTimerText;
	public AudioSource correctAudio;
	public AudioSource victoryAudio;

	void Awake() {
		// Debug logging switch.
		debugMode = false;
	}

	// Use this for initialization
	void Start() {
		this.card1 = null;
		this.victory = false;
		this.matchedPairs = 0;
		this.victoryCondition = 0;
		this.victoryText.text = "";
		this.gameTimerText.text = "";
		this.timer = new System.Diagnostics.Stopwatch();
	}
	
	// Update is called once per frame
	void Update() {
		this.displayTimer();
	}

	void setVictoryText() {
		this.victoryText.text = "You win!";
	}

	void clearVictoryText() {
		this.victoryText.text = "";
	}

	public bool getVictory() {
		return this.victory;
	}

	public void initializeGame(int numberOfCardPairs) {
		this.victory = false;
		this.victoryCondition = numberOfCardPairs;
		this.matchedPairs = 0;
		this.clearVictoryText();
		this.timer.Reset();
		this.timer.Start();
	}

	public void matchCard(GameObject card) {
		if (null == this.card1) {
			if (debugMode) {
				Debug.Log ("No card to match.  Adding card.");
			}

			this.card1 = card;
		} else {
			if (debugMode) {
				Debug.Log ("Previous card found.  Comparing cards.");
			}

			CardController prevCardController = this.card1.GetComponent<CardController>();
			CardController nextCardController = card.GetComponent<CardController>();

			// Disable interaction with cards being matched.
			prevCardController.disableFlip();
			nextCardController.disableFlip();

			int prevCardSuit = prevCardController.getSuit();
			int prevCardRank = prevCardController.getRank();
			int nextCardSuit = nextCardController.getSuit();
			int nextCardRank = nextCardController.getRank();

			if ((nextCardSuit != prevCardSuit) || (nextCardRank != prevCardRank)) {
				if (debugMode) {
					Debug.Log("Cards do not match.");
				}

				// Flip cards back over.
				prevCardController.enableFlip();
				nextCardController.enableFlip();
				prevCardController.flip();
				nextCardController.flip();
			} else {
				this.correctAudio.Play();

				if (debugMode) {
					Debug.Log("Cards match.");
				}

				this.matchedPairs++;
			}

			// Reset stored card.
			this.card1 = null;
		}

		if (this.victoryCondition > 0 &&
			this.matchedPairs == this.victoryCondition) {
			this.timer.Stop();
			this.victory = true;
			this.setVictoryText();
			this.victoryAudio.Play();
		}
	}

	void displayTimer() {
		long timeElapsed = this.timer.ElapsedMilliseconds;

		string minutes = string.Format("{0:0#}", timeElapsed / 60000);
		string seconds = string.Format("{0:0#}", timeElapsed / 1000);
		string hundredths = string.Format("{0:0#}", (timeElapsed / 10) % 100);
		
		this.gameTimerText.text = minutes + ":" + seconds + ":" + hundredths;
	}
}
