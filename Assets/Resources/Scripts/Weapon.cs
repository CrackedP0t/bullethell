using UnityEngine;
using MoonSharp.Interpreter;
using System;
using System.Collections;

namespace BulletHell
{
	public class Weapon
	{
		public Weapon (DynValue dyndata, Script script, MonoBehaviour parent) {
			if (dyndata.Type == DataType.Table) {
				Table data = dyndata.Table;

				if (data.Get ("rate").Type == DataType.Number) {
					Rate = (float)data.Get ("rate").CastToNumber() / 1000;
				} else {
					Debug.LogError ("The field \"rate\" must be a number!");
				}

				if (data.Get ("attack").Type == DataType.Function) {
					Attack = ()=>script.Call (data.Get ("attack"));
				} else {
					Debug.LogError ("The field \"rate\" must be a number!");
				}

				parent.StartCoroutine(startFiring());
			} else {
				Debug.LogError ("Weapons can only be constructed from tables!");
			}
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

