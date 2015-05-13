using System;
using System.Collections;
using UnityEngine;
using MoonSharp.Interpreter;

namespace BulletHell
{
	public class Weapon
	{
		public bool CanFire = true;
		
		public bool Activated = false;
		
		public int Rate;
		
		public Action Attack = ()=>{};
		
		public Weapon (int rate, Action attack) {
			Rate = rate;
			Attack = attack;
			
			start ();
		}

		private Movable parent;

		private int totalTime = 0;

		public Weapon (Table data, Script script, Movable newparent) {
			parent = newparent;

			if (data.Get ("rate").Type == DataType.Number)
				Rate = (int)data.Get ("rate").CastToNumber();
			else
				Debug.LogError ("The field \"rate\" must be a number!", parent);

			if (data.Get ("attack").Type == DataType.Function)
				Attack = () => script.Call (data.Get ("attack"));
			else
				Debug.LogError ("The field \"attack\" must be a function!", parent);

			start ();
		}

		private void start() {
			parent.Callbacks.SetObject ("update", new Action (()=>{
				totalTime += (int)(Time.deltaTime * 1000);

				for (int i = 0; i < (int)(totalTime / Rate) /*&& Activated && CanFire*/; i++)
					Attack();

				totalTime = totalTime % Rate;
			}));
		}
	}
}