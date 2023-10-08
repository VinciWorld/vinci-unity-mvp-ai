using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersSelection : MonoBehaviour {

	public GameObject character1;
	public GameObject character2;
	public GameObject character3;
	public GameObject character4;
	public GameObject character5;
	public GameObject character6;


	void setCharacter01 (){
		onActiveFalse ();
		character1.active=true;
	}
	void setCharacter02 (){
		onActiveFalse ();
		character2.active=true;
	}
	void setCharacter03 (){
		onActiveFalse ();
		character3.active=true;
	}
	void setCharacter04 (){
		onActiveFalse ();
		character4.active=true;
	}
	void setCharacter05 (){
		onActiveFalse ();
		character5.active=true;
	}
	void setCharacter06 (){
		onActiveFalse ();
		character6.active=true;
	}

	void onActiveFalse()
	{
		character1.active=false;
		character2.active=false;
		character3.active=false;
		character4.active=false;
		character5.active=false;
		character6.active=false;

	}
}
