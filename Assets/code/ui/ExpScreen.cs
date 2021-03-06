﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExpScreen : MonoBehaviour {

	[Header("Items List")]
	[SerializeField]private List<GameObject> colItems;
	[SerializeField]private int exp;

	[Header("UI Objects")]
	[SerializeField]private Slider myExpSlider;
	[SerializeField]private GameObject myGrid;
	[SerializeField]private GameObject colItemPrefab;
	[SerializeField]private Text score;
	[SerializeField]private Text newItemInGallery;

	[SerializeField]private GameObject bPlayAgain;
	[SerializeField]private GameObject bMenu;

	[SerializeField]private GameObject highScoreText;

	[Header("Saving Values")]
	[SerializeField] private SO_PlayerData playerData;
	[SerializeField] private int level;
	[SerializeField] private List<int> expToLevel;

	int biggestScore = 0;

	private int indexInList;

	private void Start(){
		newItemInGallery.gameObject.SetActive(false);
		GetCollectedItems();
		StartCoroutine( BeginScreen() );
	}
	private void GetCollectedItems(){
		colItems = GameObject.Find("LevelManager").GetComponent<LevelManager>().collectedItems;
	}
	private GameObject ReadNextItem(){
		if(indexInList + 1 <colItems.Count){
			indexInList++;
			return colItems[indexInList];
		}else{
			return null;
		}
	}
	private int GetValue(GameObject item){
		return item.GetComponent<Gem>().GetValue();
	}
	private string GetName(GameObject item){
		return item.name;
	}
	private IEnumerator AddOnExp(int value){
		while(value > 0){
			value -= 2;
			exp += 2;
			if(exp > expToLevel[level+1]){
				playerData.collectedItems[level-1] = true;
				level++;
				print("LEVEL UP: "+level);
				newItemInGallery.gameObject.SetActive(true);
				exp = 0;
				//MUDA ITEMS E LEVEL
				playerData.level = level;
			}
			myExpSlider.value = (float)exp/expToLevel[level+1];
			yield return new WaitForEndOfFrame();
		}
	}
	private IEnumerator BeginScreen(){
		score.gameObject.SetActive(true);
		var currentScore = GameObject.Find("LevelManager").GetComponent<LevelManager>().totalMoney;
		score.text = currentScore.ToString();

		if(currentScore > PlayerPrefs.GetInt("bestScore",0) ){
			highScoreText.SetActive(true);
			PlayerPrefs.SetInt("bestScore",currentScore);
		}else{
			highScoreText.SetActive(false);
		}

		exp = PlayerPrefs.GetInt("exp",0);
		level = PlayerPrefs.GetInt("level",1);

		indexInList = 0;
		while(true){
			GameObject actualItem = ReadNextItem();
			if(actualItem == null) {
				break;
			}else if(actualItem.GetComponent<Gem>() != null){
				yield return AddOnExp(actualItem.GetComponent<Gem>().GetValue());
			}else{
				yield return AddOnExp(0);
			}
		}
		bMenu.SetActive(true);
		bPlayAgain.SetActive(true);

		PlayerPrefs.SetInt("exp",exp);
		PlayerPrefs.SetInt("level", level);
	}
}
