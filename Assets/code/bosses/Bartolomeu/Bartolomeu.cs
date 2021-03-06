﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bartolomeu : Boss {

	[Header("General Variables")]
	[SerializeField] private float appearSpeed = 20;
	[SerializeField] private float movementSpeed = 50;
	[SerializeField] private float swapSpeed = 50;
	[SerializeField] private GameObject upperLimit, lowerLimit;

	[Header("Egg Preferences")]
	[SerializeField] private GameObject eggPrefab;
	[SerializeField] private int attacksBetweenHearts = 2;

	[Header("Tongue Attack Preferences")]
	[SerializeField] private GameObject tongueObject;
	[SerializeField] private float tongueSpeed = 10f;
	[SerializeField] private float tongueMaxSize = 49f;
	[SerializeField] private float tongueWaitTime = 2f;


	[Header("Bomb Attack Preferences")]
	[SerializeField] private GameObject bombPrefab;


	[Header("Animations Preferences")]
	[SerializeField] private float maxScale;
	[SerializeField] private float growingTax;
	[SerializeField] private GameObject safePoint;

	[Header("Appearing Animation")]
	[SerializeField] private GameObject bossTitlePrefab;

	private SpriteRenderer bossRenderer;
	private Animator bossAnimator;

	private SpriteRenderer tongueRenderer;
	private BoxCollider2D tongueCollider;

	private GameObject player;

	private int attacksAmount = 0;

	public bool defeated;
	public bool tongueHit = false;

	private void Start()
	{
		GameObject bossTitle = (GameObject) Instantiate(bossTitlePrefab);
		bossTitle.GetComponent<BossTitle> ().SetBossName ("Bartolomeu");

		bossRenderer = GetComponent<SpriteRenderer> ();
		bossAnimator = GetComponent<Animator> ();

		tongueRenderer = tongueObject.GetComponent<SpriteRenderer> ();
		tongueCollider = tongueObject.GetComponent<BoxCollider2D> ();
		tongueCollider.enabled = false;

		player = GetPlayer();
		StartCoroutine( Appear() );
	}

	public IEnumerator Appear()
	{
		transform.position = safePoint.transform.position;

		yield return new WaitForSeconds(2);

		while(transform.position.x != upperLimit.transform.position.x){
			transform.position = new Vector3(Mathf.MoveTowards(transform.position.x,upperLimit.transform.position.x,movementSpeed*Time.deltaTime),transform.position.y,transform.position.z);
			yield return new WaitForEndOfFrame();
		}

		yield return new WaitForSeconds(1);
		//Texto dizendo "boss: ###"
		yield return Loop();
	}

	private IEnumerator Loop()
	{
		while(!defeated)
		{
			CanDie();
			if(!isActing)
			{
				yield return PlaceEggs ();
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator PlaceEggs ()
	{
		isActing = true;

		int egg_amount = Random.Range(2,4); // Sorteia o número 2 ou 3 para definir quantos ovos ele vai botar
		float offset = (upperLimit.transform.position.y - lowerLimit.transform.position.y) / (float) egg_amount;
	
		DestroyOldEggs ();

		int i = 0;
		for (; i < egg_amount; i++)
		{
			Vector3 newPosition = new Vector3 (upperLimit.transform.position.x, upperLimit.transform.position.y - offset * i, 10f);
			yield return MoveToPosition (transform, newPosition);
			GameObject egg = (GameObject) Instantiate (eggPrefab, newPosition, Quaternion.identity);
			Egg eggScript = egg.GetComponent<Egg> ();
			eggScript.SetEggAmount (egg_amount);
			if (attacksAmount % attacksBetweenHearts == 0)
				eggScript.SetCanSpawnHeart (true);
			else
				eggScript.SetCanSpawnHeart (false);
			egg.transform.SetParent (transform.parent);
		}

		Vector3 bossPosition = new Vector3 (upperLimit.transform.position.x, upperLimit.transform.position.y - offset * i, 10f);
		yield return MoveToPosition (transform, bossPosition);
		GameObject bossEgg = (GameObject) Instantiate (eggPrefab, bossPosition, Quaternion.identity);
		bossEgg.transform.SetParent (transform);
		bossEgg.GetComponent<Egg>().SetBossEgg ();


		// BOSS ENTRA NO OVO
		yield return new WaitForSeconds(1);
		DeactivateRenderer ();

		yield return new WaitForSeconds(1);
		yield return SwapEggs ();
	}

	private IEnumerator SwapEggs ()
	{
		List <Transform> eggs = new List <Transform> ();
		for (int i = 0; i < transform.parent.childCount; i++)
		{
			Transform obj = transform.parent.GetChild (i);
			if (obj.CompareTag ("Egg") || obj.CompareTag ("Boss"))
				eggs.Add (transform.parent.GetChild (i));
		}
			
		// o número de trocas é igual à 5 - quantidade_de_quartos_de_vida_que_o_boss_tem
		int swapAmount = 5 - actualHP/(maxHP/4);

		for (int i = 0; i < swapAmount; i++)
		{

			int firstEgg = Random.Range (0, eggs.Count);
			int secondEgg = firstEgg;
			while (secondEgg == firstEgg)
			{
				secondEgg = Random.Range (0, eggs.Count);
			}


			yield return SwapPositions (eggs[firstEgg], eggs[secondEgg]);
		}

		yield return HatchEggs (eggs);
	}


	private IEnumerator HatchEggs (List <Transform> eggs)
	{
		for (int i = 0; i < eggs.Count; i++)
		{
			if (eggs[i].CompareTag ("Egg"))
			{
				eggs[i].GetComponent<Egg>().Hatch();
			}

			else if (eggs[i].CompareTag ("Boss"))
			{
				eggs[i].GetComponentInChildren<Egg>().Hatch();
			}
		}

		// BOSS ATTACK
		if (Random.Range (0,2) == 0)
			yield return TongueAttack ();
		else
			yield return BombAttack ();
	}

	private IEnumerator TongueAttack ()
	{
		float error = 0.01f;

		tongueCollider.enabled = true;

		// Wait for the boss to hatch from the egg
		yield return new WaitForSeconds (tongueWaitTime);

		float initialWidth = tongueRenderer.size.x;
		Vector2 targetSize = new Vector2 (tongueMaxSize, tongueRenderer.size.y);
		while (tongueRenderer.size.x < tongueMaxSize - error) 
		{
			tongueRenderer.size = Vector2.Lerp (tongueRenderer.size, targetSize, tongueSpeed * Time.deltaTime);
			yield return new WaitForEndOfFrame ();
		}
			
		targetSize = new Vector2 (initialWidth, tongueRenderer.size.y);
		while (tongueRenderer.size.x > initialWidth + error) 
		{
			tongueRenderer.size = Vector2.Lerp (tongueRenderer.size, targetSize, 1.5f*tongueSpeed * Time.deltaTime);
			yield return new WaitForEndOfFrame ();
		}

		if (tongueHit == false) 
		{
			actualHP -= maxHP / 4;
			UpdateHealthBar ();
			tongueHit = false;
		}

		yield return new WaitForSeconds (2f);
		isActing = false;
		tongueCollider.enabled = false;
		attacksAmount++;
	}

	private IEnumerator BombAttack ()
	{
		// Wait for the boss to hatch from the egg
		yield return new WaitForSeconds (tongueWaitTime);

		// Move forward
		Vector3 startPosition = transform.position;
		Vector3 targetPosition = new Vector3 (player.transform.position.x, transform.position.y, transform.position.z);
		yield return MoveToPosition (transform, targetPosition);

		Instantiate (bombPrefab, transform.position, Quaternion.identity);

		// Flip to move back
		bossRenderer.flipX = true;
		yield return MoveToPosition (transform, startPosition);
		bossRenderer.flipX = false;

		yield return new WaitForSeconds (2f);
		isActing = false;
		attacksAmount++;
	}

	private IEnumerator MoveToPosition (Transform objTransform, Vector3 newPosition, float error = 0.01f)
	{
		while ((objTransform.position-newPosition).magnitude > error)
		{
			transform.position = Vector3.MoveTowards (transform.position, newPosition, movementSpeed*Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator SwapPositions (Transform transform_a, Transform transform_b, float error = 0.01f)
	{
		Vector3 initialPos_a = transform_a.position;
		Vector3 initialPos_b = transform_b.position;
		while ((transform_a.position-initialPos_b).magnitude > error)
		{
			transform_a.position = Vector3.MoveTowards (transform_a.position, initialPos_b, swapSpeed*Time.deltaTime);
			transform_b.position = Vector3.MoveTowards (transform_b.position, initialPos_a, swapSpeed*Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}
	}

	public void DeactivateRenderer()
	{
		GetComponent<SpriteRenderer> ().enabled = false;
		tongueRenderer.enabled = false;
	}

	public void ActivateRenderer()
	{
		GetComponent<SpriteRenderer> ().enabled = true;
		tongueRenderer.enabled = true;
	}

	private void DestroyOldEggs ()
	{
		Transform parent = transform.parent;
		for (int i = 0; i < parent.childCount; i++) 
		{
			if (parent.GetChild (i).CompareTag ("Egg")) 
			{
				Destroy (parent.GetChild (i).gameObject);
			}
		}
	}
}
