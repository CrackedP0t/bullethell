using UnityEngine;
using System.Collections;
using BulletHell;
using MoonSharp.Interpreter;

namespace BulletHell {
	public class Player : Fighter {

		public override void Start () {
			this.isEnemy = false;
			base.Start ();
		}

		public override void  Update () {
			Frozen = Input.GetAxis ("Horizontal") == 0 && Input.GetAxis ("Vertical") == 0;

			if (!CanFire && Input.GetAxis ("Fire") > 0 && weaponFunction != default(DynValue)) {
				controlScript.Call(weaponFunction);
			}

			CanFire = Input.GetAxis ("Fire") > 0;

			SetVelocity (new Vector2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical")) * Speed);

			base.Update ();
		}
	}
}