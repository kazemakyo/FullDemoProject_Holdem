using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMain : MonoBehaviour {	

	public Transform CardDataRoot ;
	public List<CCardData> CardData ;

	// decide card outgiving order
	public List<GameObject> CardSlotList ;

	public Queue<CCardData> CardPool;
	public List<CCardData> CommunityCardPool;

	public CPlayer Player01 ;
	public CPlayer Player02 ;

	void Awake () {
		
		CardData = new List<CCardData> ();
		CardPool = new Queue<CCardData> ();
		CommunityCardPool = new List<CCardData> ();

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
		
		CommunityCardPool.Clear ();
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
	CCardData DrawCard () {
		return CardPool.Dequeue () ;
	}

	const int MaxCommunityCards = 5;
	void DrawCards () {
		
		// draw cards flow , no implement now
		DrawCommunityCardSet ();
		Player01.DrawCards ();
		Player02.DrawCards ();

		// easy draw cards flow 
		foreach (GameObject card in CardSlotList) {
			card.GetComponent<CCardData>().SetData ( DrawCard () );
		}

		int index = 0;
		foreach (GameObject card in CardSlotList) {
			if (index >= MaxCommunityCards )
				break;
			CommunityCardPool.Add ( card.GetComponent<CCardData>() );
			index++;
		}

	}

	void DrawCommunityCardSet () {

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

	// Update is called once per frame
	void Update () {
		
		if (Input.GetKeyDown (KeyCode.Space)) {
			RestartRound ();
		}

		if (Input.GetKeyDown (KeyCode.Q)) {
			Player01.GetBestCardCombination (CommunityCardPool.ToArray ());
		}

		if (Input.GetKeyDown (KeyCode.E)) {
			Player02.GetBestCardCombination (CommunityCardPool.ToArray ());
		}

	}

}

