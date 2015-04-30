using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BulletHell;
using MoonSharp.Interpreter;

namespace BulletHell {
	public class Player : Fighter {

		public override void Start () {
			IsEnemy = false;
			base.Start ();
		}

		public override void  Update () {
			Frozen = Input.GetAxis ("Horizontal") == 0 && Input.GetAxis ("Vertical") == 0;

			foreach (KeyValuePair<string, Weapon> pair in Weapons) {
				pair.Value.CanFire = (Input.GetButton("Fire"));
			}

			SetVelocity (new Vector2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical")) * Speed);

			base.Update ();
		}
	}
}