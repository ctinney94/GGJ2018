﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour {

	bool triggered;
	public int level_index;
	public Renderer m_circletransition; 
	public Transform m_background;

	void OnTriggerEnter2D(Collider2D c)
	{
		PlayerPrefs.SetInt("Level"+level_index,1);//1 means complete!
		GameStateManager.instance.ChangeState(GameStateManager.GameStates.STATE_LEVELCOMPLETE);
		StartCoroutine(EndSequence());
	}
	void Start()
	{
		StartCoroutine(CircleTransition(true));
	}
	void Update()
	{

		if (GameStateManager.instance.m_state==GameStateManager.GameStates.STATE_GAMEPLAY)
		{
			Vector3 pos=Camera.main.transform.position;
			pos.z=0;
			m_background.transform.position=pos;
		}
		else if (GameStateManager.instance.m_state==GameStateManager.GameStates.STATE_GAMEPLAY)
		{
			//if jump button
			//Exit to overworld
			//StartCoroutine(CircleTransition(false));
		}
	}

	IEnumerator CircleTransition(bool inout)
	{
		if (inout) yield return new WaitForSeconds(1);
		float lerpy=0;
		while (lerpy<1)
		{
			lerpy+=Time.deltaTime;
			m_circletransition.transform.position=Player.instance.transform.position;
			m_circletransition.material.SetFloat("_SliceAmount",inout ? lerpy : 1-lerpy);
       		yield return new WaitForEndOfFrame();
		}
	}

	//Zoom in on player, maybe change sprite to a thumbs up ting
	IEnumerator EndSequence()
	{
		float lerpy=0;
		while (lerpy<1)
		{
			lerpy+=Time.deltaTime;
			Vector3 pos=Vector3.Lerp(Camera.main.transform.position,Player.instance.transform.position,lerpy);
			pos.z=-10;
			Camera.main.transform.position=pos;
			Camera.main.orthographicSize=Mathf.Lerp(5,1.5f,lerpy);
        	yield return new WaitForEndOfFrame();
		}
	}

}