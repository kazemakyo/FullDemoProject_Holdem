using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public enum E_CardCombinationType {		
	[Description("Undefine")]					Undefine = -1 ,
	[Description("Rayal Straigth Flush")]	RoyalStraigthFlush = 10 , 
	[Description("Straigth Flush")]			StraigthFlush = 9 , 
	[Description("Four of a Kind")]			FourOfaKind = 8 , 
	[Description("Full House")]				FullHouse = 7 , 
	[Description("Flush")]						Flush = 6 , 
	[Description("Straight")]					Straight = 5 , 
	[Description("Three of a Kind")]		ThreeOfAKind = 4 , 
	[Description("Two Pair")]					TwoPair = 3 , 
	[Description("One Pair")]					OnePair = 2 , 
	[Description("High Card")]				HighCard = 1 , 
}

public class CardCombinationData {

	const int MaxHandCardCount  = 5 ;
	// factor must bigger than biggest value ( ACE_value )
	const float weightFactor = 16;

	float divider1 = Mathf.Pow (weightFactor, 1f);
	float divider2 = Mathf.Pow (weightFactor, 2f);
	float divider3 = Mathf.Pow (weightFactor, 3f);
	float divider4 = Mathf.Pow (weightFactor, 4f);

	public E_CardCombinationType type = E_CardCombinationType.Undefine ;
	public float [] cardSetValueArray = new float[MaxHandCardCount];

	public void SetData ( E_CardCombinationType _type , CCardData [] cards ) {
		type = _type;
		for ( int i = 0 ; i < cardSetValueArray.Length ; ++i )
			cardSetValueArray [i] = (float)cards [i].Value ;
	}

	// total weight = type weight + value weight
	public float GetWeight () {
		return (int)type * weightFactor + GetValueWeight ();
	}

	// important algorithms
	float GetValueWeight () {
		switch (type) {
		case E_CardCombinationType.RoyalStraigthFlush:
		case E_CardCombinationType.StraigthFlush:
		case E_CardCombinationType.Straight:
			// lead card value decide card weight
			return cardSetValueArray [0] ;
		case E_CardCombinationType.FourOfaKind:
			// fifth card value decide card weight
			return cardSetValueArray [4] ;
		case E_CardCombinationType.FullHouse:
			return cardSetValueArray [0] + cardSetValueArray [3]/divider1 ;
		case E_CardCombinationType.ThreeOfAKind:
			return cardSetValueArray [0] + cardSetValueArray [3]/divider1 + cardSetValueArray [4]/divider2 ;
		case E_CardCombinationType.TwoPair:
			return cardSetValueArray [0] + cardSetValueArray [2]/divider1 + cardSetValueArray [4]/divider2 ;
		case E_CardCombinationType.OnePair:
			return cardSetValueArray [0] + cardSetValueArray [2]/divider1 + cardSetValueArray [3]/divider2 + cardSetValueArray [4]/divider3 ;
		case E_CardCombinationType.Flush:
		case E_CardCombinationType.HighCard:
			return cardSetValueArray [0] + cardSetValueArray [1]/divider1 + cardSetValueArray [2]/divider2 + cardSetValueArray [3]/divider3 + cardSetValueArray [4]/divider4 ;
		default:
			return 0;
		}
	}

}
public class CPlayer : MonoBehaviour {

	const int ACE_value = 14;
	const int MaxHandCardCount  = 5 ;

	public List<CCardData> HandCards ;
	public List<CCardData> CurrentBestCardList ;
	public CardCombinationData CurrentBestCardCombination;

	public Text TextBestCardForm ;

	// Use this for initialization
	void Start () {

	}

	public void DrawCards () {
		foreach (CCardData card in HandCards)
			card.SetData (GameMain.instance.DrawCard ());
	}

	public float GetBestCardWeight () {
		return CurrentBestCardCombination.GetWeight ();
	}

	int CardComparer ( CCardData card1 , CCardData card2 ) {
		if (card1.GetWeight () < card2.GetWeight ())
			return 1;
		if (card1.GetWeight () == card2.GetWeight ())
			return 0;
		if (card1.GetWeight () > card2.GetWeight ())
			return -1;
		return -1;
	}

	E_CardCombinationType BestPatternMatch ( CCardData [] sources ) {
		return E_CardCombinationType.Undefine;
	}

	List<CCardData> SortList ( bool showDebugLog , List<CCardData> sources ) {
		
		if (showDebugLog) {
			Debug.Log (" --- before sort --- ");
			foreach (CCardData data in sources)
				Debug.Log ("value : " + data.Value + " , weight : " + data.GetWeight ());
		}

		// sort card list by weight
		sources.Sort (CardComparer);

		if (showDebugLog) {
			Debug.Log (" --- after sort --- ");
			foreach (CCardData data in sources)
				Debug.Log ("value : " + data.Value + " , weight : " + data.GetWeight ());
			Debug.Log (" --- end sort --- ");
		}

		return sources;
	}

	List<CCardData> setTestValue ( List<CCardData> sources , List<int> testData ) {
		int index = 0;
		foreach (CCardData data in sources) {
			data.Value = testData [index];
			++index;
		}
		return sources;
	}

	bool StraightCheck ( List<CCardData> cards , out int startIndex ) {
		
		int maxContinuousCount = 0;
		int continuousCounter = 0;
		startIndex = 0 ;
		bool hasStartIndex_continuous = false;

		int [] subValueArray = new int [ cards.Count - 1 ] ;

		for (int i = 0; i < cards.Count - 1; ++i) {

			subValueArray [i] = cards [i].Value - cards [i + 1].Value;

			if (subValueArray [i] == 1) {

				continuousCounter++;

				if (hasStartIndex_continuous == false) {
					if (continuousCounter >= maxContinuousCount) {
						startIndex = i;
						hasStartIndex_continuous = true;
					}
				}

			} else {
				continuousCounter = 0;
				hasStartIndex_continuous = false;
			}

			// save max counter
			if (continuousCounter > maxContinuousCount) {
				maxContinuousCount = continuousCounter;
			}

		}

		return maxContinuousCount >= 4 ;

	}

	CardType FlushTypeCheck ( List<CCardData> cards  ) {
		
		int spadeCount = 0;
		int heartCount = 0;
		int diamondCount = 0;
		int cludCount = 0;

		CardType result = CardType.Undefine;

		foreach (CCardData card in cards) {
			if (card.Type == CardType.Spade)
				spadeCount++;
			if (card.Type == CardType.Heart)
				heartCount++;
			if (card.Type == CardType.Diamond)
				diamondCount++;
			if (card.Type == CardType.Club)
				cludCount++;
		}

		if (spadeCount >= MaxHandCardCount )
			result = CardType.Spade;
		if (heartCount >= MaxHandCardCount)
			result = CardType.Heart;
		if (diamondCount >= MaxHandCardCount)
			result = CardType.Diamond;
		if (cludCount >= MaxHandCardCount)
			result = CardType.Club;

		return result;
	}

	// Arrange cards , 7 pick 5 , Main Algorithm
	public CardCombinationData GetBestCardCombination ( CCardData [] sources ) {

		CardCombinationData result = new CardCombinationData () ;
		List<CCardData> newSet = new List<CCardData> ();

		foreach (CCardData data in sources)
			newSet.Add (data);

		foreach ( CCardData data in HandCards )
			newSet.Add (data);		
		
		newSet = SortList ( false , newSet ) ;

		// run best pattern match algorithm
		bool isFlush = false;
		bool isStraight = false;
		bool isFourOfaKind = false;

		// Straight check 
		int startIndex_continuous = 0;
		isStraight = StraightCheck (newSet, out startIndex_continuous);

		// the same card special cehck method
		int [] valueCounter = new int[16] ;

		// zero fields
		for (int i = 0 ; i < valueCounter.Length ; ++ i)
			valueCounter [ i ] = 0 ;

		foreach (CCardData card in newSet) {
			valueCounter [card.Value]++;
		}

		// check each value counter
		// check Four of a Kind , Three of a kind , and pair count
		int valueIndex = 0 ;
		int arrayIndex = -1;
		int fourOfaKindIndex = 0;

		List<int> ThreeOfaKindIndexList = new List<int> ();
		List<int> PairIndexList = new List<int> ();
		List<int> newSetValueList = new List<int> ();

		foreach (CCardData card in newSet)
			newSetValueList.Add (card.Value);

		foreach ( int count in valueCounter ) {

			arrayIndex = newSetValueList.IndexOf (valueIndex);

			if ( count == 4 ) {
				isFourOfaKind = true;
				fourOfaKindIndex = arrayIndex;
			}
			
			if ( count == 3 )
				ThreeOfaKindIndexList.Add (arrayIndex);

			if ( count == 2 )
				PairIndexList.Add (arrayIndex);
			
			valueIndex++;
		}

		// Flush Check
		CardType flushType = FlushTypeCheck ( newSet );
		isFlush = flushType != CardType.Undefine ;

		// -------------------------------------------------------------------------------------

		// default set as High Card (Zitch)
		result.type = E_CardCombinationType.HighCard ;

		// one pair check
		if (PairIndexList.Count == 1)
			result.type = E_CardCombinationType.OnePair;
		
		// two pair check
		if (PairIndexList.Count >= 2)
			result.type = E_CardCombinationType.TwoPair;

		// three of kind check
		if (ThreeOfaKindIndexList.Count == 1)
			result.type = E_CardCombinationType.ThreeOfAKind;

		// Straight check
		if (isStraight)
			result.type = E_CardCombinationType.Straight;

		// Flush check
		if (isFlush)
			result.type = E_CardCombinationType.Flush;

		// Full house check , situation 1
		if ( ThreeOfaKindIndexList.Count == 2 )
			result.type = E_CardCombinationType.FullHouse;

		// Full house check , situation 2
		if (ThreeOfaKindIndexList.Count == 1 && PairIndexList.Count >= 1)
			result.type = E_CardCombinationType.FullHouse;

		// Four of a kind check
		if (isFourOfaKind)
			result.type = E_CardCombinationType.FourOfaKind;	

		
		// pick card
		List<CCardData> pickedCardList = new List<CCardData> () ;
		Queue<CCardData> valueSetQueue = new Queue<CCardData>() ;

		// need reverse once , for the right order of the pair ( small -> big => big -> small )
		PairIndexList.Reverse ();

		// very low chance to get two ThreeOfaKind set , still need Reverse
		if (ThreeOfaKindIndexList.Count >= 2)
			ThreeOfaKindIndexList.Reverse ();

		int pair1Index = -1;
		int pair2Index = -1;

		switch (result.type) {
		case E_CardCombinationType.Straight:
			
			for (int i = startIndex_continuous; i < startIndex_continuous + MaxHandCardCount; ++i)
				pickedCardList.Add (newSet [i]);
			
			// check flush again after pick ( check hand card only )
			if ( flushType != CardType.Undefine ) {
				for (int i = startIndex_continuous; i < startIndex_continuous + MaxHandCardCount ; ++i) {
					if (newSet [i].Type == flushType)
						result.type = E_CardCombinationType.StraigthFlush ;
				}
			}

			// Royal straigth flush check
			if ( result.type == E_CardCombinationType.StraigthFlush )
				if ( newSet [ startIndex_continuous ] .Value == ACE_value )
					result.type = E_CardCombinationType.RoyalStraigthFlush;
			
			break;

		case E_CardCombinationType.FourOfaKind:

				pickedCardList.Add (newSet [fourOfaKindIndex]);
				pickedCardList.Add (newSet [fourOfaKindIndex + 1 ]);
				pickedCardList.Add (newSet [fourOfaKindIndex + 2 ]);
				pickedCardList.Add (newSet [fourOfaKindIndex + 3 ]);

				foreach (CCardData card in newSet)
				if (card.Value != newSet [fourOfaKindIndex].Value)
						valueSetQueue.Enqueue (card);

				pickedCardList.Add (valueSetQueue.Dequeue ());

			break;
		case E_CardCombinationType.FullHouse:
			
			int threeOfaKindIndex = ThreeOfaKindIndexList [0];
			int pairIndex = -1;

			if (PairIndexList.Count > 0)
				pairIndex = PairIndexList [0];
			else
				pairIndex = ThreeOfaKindIndexList [1];

			pickedCardList.Add (newSet [threeOfaKindIndex]);
			pickedCardList.Add (newSet [threeOfaKindIndex + 1]);
			pickedCardList.Add (newSet [threeOfaKindIndex + 2]);
			pickedCardList.Add (newSet [pairIndex]);
			pickedCardList.Add (newSet [pairIndex + 1]);

			break;
		case E_CardCombinationType.Flush:

			List<CCardData> flushList = new List<CCardData> ();

			foreach (CCardData card in newSet) {
				if (card.Type == flushType)
					flushList.Add (card);
			}

			// check Straight again in flush list
			int startIndex_flush = 0 ;
			if ( StraightCheck ( flushList , out startIndex_flush ) ) {

				for (int i = startIndex_flush; i < startIndex_flush + MaxHandCardCount; ++i)
					pickedCardList.Add (flushList [i]);

				if ( pickedCardList [ startIndex_flush ].Value == ACE_value )
					result.type = E_CardCombinationType.RoyalStraigthFlush;
				else
					// straigth flush check
					result.type = E_CardCombinationType.StraigthFlush;
			}

			break;
		case E_CardCombinationType.ThreeOfAKind:
			
			int startIndex = ThreeOfaKindIndexList [ 0 ] ;
			pickedCardList.Add (newSet [startIndex]);
			pickedCardList.Add (newSet [startIndex + 1]);
			pickedCardList.Add (newSet [startIndex + 2 ]);

			foreach (CCardData card in newSet)
				if (card.Value != newSet [startIndex].Value)
					valueSetQueue.Enqueue (card);

			pickedCardList.Add (valueSetQueue.Dequeue ());
			pickedCardList.Add (valueSetQueue.Dequeue ());

			break;
		case E_CardCombinationType.TwoPair:

			pair1Index = PairIndexList [0];
			pair2Index = PairIndexList [1];

			pickedCardList.Add (newSet [pair1Index]);
			pickedCardList.Add (newSet [pair1Index + 1]);
			pickedCardList.Add (newSet [pair2Index]);
			pickedCardList.Add (newSet [pair2Index + 1]);

			foreach (CCardData card in newSet)
				if (card.Value != newSet [pair1Index].Value && card.Value != newSet [pair2Index].Value)
					valueSetQueue.Enqueue (card); 
			
			pickedCardList.Add (valueSetQueue.Dequeue ());

			break;
		case E_CardCombinationType.OnePair:
			
			pair1Index = PairIndexList [0];

			pickedCardList.Add (newSet [pair1Index]);
			pickedCardList.Add (newSet [pair1Index + 1]);

			foreach (CCardData card in newSet)
				if (card.Value != newSet [pair1Index].Value)
					valueSetQueue.Enqueue (card);
			
			pickedCardList.Add (valueSetQueue.Dequeue ());
			pickedCardList.Add (valueSetQueue.Dequeue ());
			pickedCardList.Add (valueSetQueue.Dequeue ());

			break;
		case E_CardCombinationType.HighCard:
			
			for ( int i = 0 ; i < MaxHandCardCount ; i ++ )
				pickedCardList.Add ( newSet [i] );
			
			break;
		default:
			break;
		}

		if (pickedCardList.Count == MaxHandCardCount) {
			
			result.SetData ( result.type , pickedCardList.ToArray() ) ;

			for ( int i = 0 ; i < MaxHandCardCount ; i ++ )
				CurrentBestCardList[i].SetData (pickedCardList [i]);

			CurrentBestCardCombination = result ;
			TextBestCardForm.text = result.type.GetDescription ();

			return result ;

		} else {
			// something error
			Debug.LogWarning ( "GetBestCardCombination : pickedCardList count not match MaxHandCardCount ! " ) ;
			return null ;
		}


	}

	// Update is called once per frame
	void Update () {

	}

}