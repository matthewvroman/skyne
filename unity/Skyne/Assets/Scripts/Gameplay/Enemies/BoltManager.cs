﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltManager : Enemy 
{
	void Start () 
	{
		// Parent class Start()
		EnemyParentStart(); 
	}

	void Update () 
	{
		// Parent class Update()
		if (EnemyParentUpdate())
		{
			EnemyChildUpdate(); 
		}
	}

	void EnemyChildUpdate()
	{

	}

	public void ChildUpdate ()
	{

	}
}