using UnityEngine;
using MoonSharp.Interpreter;
using System;
using System.Collections;

namespace BulletHell
{
	public class Weapon
	{
		public Weapon (Table data, Script script, Movable parent) {
				if (data.Get ("rate").Type == DataType.Number) {
					Rate = (float)data.Get ("rate").CastToNumber() / 1000;
				} else {
					Debug.LogError ("The field \"rate\" must be a number!");
				}

				if (data.Get ("attack").Type == DataType.Function) {
					Attack = ()=>script.Call (data.Get ("attack"));
				} else {
					Debug.LogError ("The field \"attack\" must be a function!");
				}

				parent.StartCoroutine(startFiring());
		}

		private IEnumerator startFiring() {
			while (true) {
				yield return new WaitForSeconds(Rate);
				if (Activated && CanFire)
					Attack();
			}
		}

		public bool CanFire = true;

		public bool Activated = false;

		public float Rate;

		public Action Attack;
	}
}

