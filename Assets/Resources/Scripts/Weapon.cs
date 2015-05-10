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

		private Movable parent;
		
		public Weapon (int rate, Action attack) {
			Rate = rate;
			Attack = attack;
			
			start ();
		}

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
			parent.StartCoroutine (startFiring());
		}

		private IEnumerator startFiring() {
			while (true) {
				yield return new WaitForSeconds((float)Rate / 1000);
				if (Activated && CanFire)
					Attack();
			}
		}
	}
}