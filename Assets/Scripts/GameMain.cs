using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMain : MonoBehaviour {	

	static public GameMain instance ;

	public Transform CardDataRoot ;
	public List<CCardData> CardData ;

	// decide card outgiving order
	public List<GameObject> CardSlotList ;

	public Queue<CCardData> CardPool;
	public List<CCardData> CommunityCardPool;

	public CPlayer Player01 ;
	public CPlayer Player02 ;

	void Awake () {

		instance = this;

		CardData = new List<CCardData> ();
		CardPool = new Queue<CCardData> ();
//		CommunityCardPool = new List<CCardData> ();

		GetCardData ();
	}

	void GetCardData () {
		CCardData [] cardDataArray = CardDataRoot.GetComponentsInChildren<CCardData> ();
		foreach (CCardData data in cardDataArray) {
			CardData.Add (data);
		}
	}

	// Use this for initialization
	void Start () {
		RestartRound ();
	}

	// when play again button click
	public void RestartRound () {
		Shuffle ();
		DrawCards ();
		ShowDown ();
	}

	// Randomize Card Pool
	void Shuffle () {
		
//		CommunityCardPool.Clear ();
		List<CCardData> tempCardList = new List<CCardData> ();

		foreach ( CCardData card in CardData ) {
			tempCardList.Add (card);
		}

		int shuffleCount = Random.Range (CardData.Count , CardData.Count * 2);

		for ( int i = 0; i < shuffleCount ; i++ ) {
			int swapIndex1 = Random.Range (0, CardData.Count);
			int swapIndex2 = Random.Range (0, CardData.Count);
			CCardData temp = tempCardList [swapIndex1];
			tempCardList [swapIndex1] = tempCardList [swapIndex2];
			tempCardList [swapIndex2] = temp;
		}

		CardPool.Clear ();
		foreach ( CCardData card in tempCardList ) {
			CardPool.Enqueue (card);
		}

	}

	// draw a card from card pool
	public CCardData DrawCard () {
		return CardPool.Dequeue () ;
	}

	const int MaxCommunityCards = 5;
	void DrawCards () {		
		// draw cards flow 
		DrawCommunityCardSet ();
		Player01.DrawCards ();
		Player02.DrawCards ();
	}

	void DrawCommunityCardSet () {
		for ( int i = 0 ; i < MaxCommunityCards ; ++ i )
			CommunityCardPool [ i ].SetData ( DrawCard () );
	}

	public Text TextComparerStatus ;
	// Card Set Comparer
	void ShowDown () {

		Player01.GetBestCardCombination ( CommunityCardPool.ToArray () );
		Player02.GetBestCardCombination ( CommunityCardPool.ToArray () );

		string displayString = "";
		if (Player01.GetBestCardWeight () == Player02.GetBestCardWeight()) {
			displayString = "Draw";
		} else {
			if (Player01.GetBestCardWeight() > Player02.GetBestCardWeight())
				displayString = "P1 Win ! ";
			else
				displayString = "P2 Win ! ";
		}
		TextComparerStatus.text = displayString;

	}

	List<CCardData> CardPoolList = new List<CCardData> ();

	public List<CCardData> DebugCardSetRSF ;
	public List<CCardData> DebugCardSetSF ;

	// Debug Function
	void SetCommunityCardSetAsTemplate ( List<CCardData> cardSetTemplate ) {		

		Shuffle ();
		CardPoolList.Clear ();

		while (CardPool.Count > 0)
			CardPoolList.Add (CardPool.Dequeue ());

		int cardIndex = 0;
		foreach (CCardData card in CommunityCardPool) {
			card.SetData (cardSetTemplate [cardIndex]);
			cardIndex++;
		}

		// search card and remove from pool
		foreach ( CCardData card1 in cardSetTemplate ) {
			foreach ( CCardData card2 in CardPoolList ) {
				if (card1.GetWeight () == card2.GetWeight ()) {
					CardPoolList.Remove (card2);
					break;
				}
			}
		}

		CardPool.Clear ();

		foreach (CCardData card in CardPoolList) {
			CardPool.Enqueue (card);
		}

		Player01.DrawCards ();
		Player02.DrawCards ();

		ShowDown ();

	}

	// RSF = Royal Straigth Flush
	public void SetCommunityCardSetAsRSF () {
		SetCommunityCardSetAsTemplate ( DebugCardSetRSF );
	}

	// SF = Straigth Flush
	public void SetCommunityCardSetAsSF () {
		SetCommunityCardSetAsTemplate ( DebugCardSetSF );
	}

	// Update is called once per frame
	void Update () {
		
		if (Input.GetKeyDown (KeyCode.Space)) {
			RestartRound ();
		}

		if (Input.GetKeyDown (KeyCode.R)) {
			SetCommunityCardSetAsRSF ();
		}

		if (Input.GetKeyDown (KeyCode.S)) {
			SetCommunityCardSetAsSF ();
		}

		if (Input.GetKeyDown (KeyCode.Q)) {
			Player01.GetBestCardCombination (CommunityCardPool.ToArray ());
		}

		if (Input.GetKeyDown (KeyCode.E)) {
			Player02.GetBestCardCombination (CommunityCardPool.ToArray ());
		}

	}

}

