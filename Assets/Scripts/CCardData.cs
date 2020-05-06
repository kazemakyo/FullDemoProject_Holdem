using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CardType {
	Spade = 4 , 
	Heart= 3 , 
	Diamond = 2 , 
	Club = 1 , 
	Undefine = 5 ,
	Count = 6
}

public class CCardData : MonoBehaviour {

	public CardType Type; // flower type
	public int Value;
	public Sprite DisplayCardSprite ;

	// Use this for initialization
	void Start () {
		
	}

	public int GetWeight () {
		return Value * 4 + (int)Type ;
	}

	public void SetData ( CardType _type , int _value , Sprite _sp ) {
		Type = _type;
		Value = _value;
		DisplayCardSprite = _sp;
	}

	public void SetData ( CCardData data ) {
		Type = data.Type;
		Value = data.Value;
		DisplayCardSprite = data.DisplayCardSprite;
		GetComponent<Image> ().sprite = DisplayCardSprite;
	}

	// Update is called once per frame
	void Update () {
		
	}

}
