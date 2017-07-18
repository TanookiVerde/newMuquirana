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

	[Header("Saving Values")]
	[SerializeField] private int level;
	[SerializeField] private List<int> expToLevel;

	private int indexInList;

	private void Start(){
		GetCollectedItems();
		StartCoroutine(BeginScreen());
	}
	private void GetCollectedItems(){
		colItems = GameObject.Find("Player").GetComponent<LevelManager>().collectedItems;
	}
	private GameObject ReadNextItem(){
		if(indexInList + 1 == colItems.Count ){
			return null;
		}
		return colItems[++indexInList];
	}
	private int GetValue(GameObject item){
		return item.GetComponent<Gem>().GetValue();
	}
	private string GetName(GameObject item){
		return item.name;
	}
	private void NewColItemInGrid(GameObject item){
		string itemName = item.name;
		for(int i = 0; i < myGrid.transform.childCount;i++){
			if(myGrid.transform.GetChild(i).name == itemName){
				myGrid.transform.GetChild(i).GetChild(2).GetChild(0).GetComponent<Text>().text = (int.Parse(myGrid.transform.GetChild(i).GetChild(2).GetChild(0).GetComponent<Text>().text) + 1 ).ToString();
				return;
			}
		}
		GameObject temp = Instantiate(colItemPrefab,Vector3.zero,Quaternion.identity,myGrid.transform);
		temp.transform.GetChild(0).GetComponent<Image>().sprite = item.GetComponent<SpriteRenderer>().sprite;
		temp.transform.GetChild(1).GetComponent<Text>().text = itemName;
		temp.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = 1.ToString();

		temp.name = itemName;
		return;
	}
	private IEnumerator AddOnExp(int value){
		while(value > 0){
			value--;
			exp++;
			if(exp > expToLevel[level+1]){
				level++;
				exp = 0;
			}
			myExpSlider.value = (float)exp/500;
			yield return new WaitForEndOfFrame();
		}
		PlayerPrefs.SetInt("level", level);
	}
	private IEnumerator BeginScreen(){
		exp = PlayerPrefs.GetInt("exp");
		level = PlayerPrefs.GetInt("level");
		while(true){
			GameObject actualItem = ReadNextItem();
			if(actualItem == null) break;
			NewColItemInGrid(actualItem);
			yield return AddOnExp(actualItem.GetComponent<Gem>().GetValue());
			yield return new WaitForSeconds(0.25f);
		}
		PlayerPrefs.SetInt("exp",exp);
	}
}
