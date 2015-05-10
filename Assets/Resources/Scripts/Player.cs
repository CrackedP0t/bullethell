using System.Collections.Generic;
using UnityEngine;

namespace BulletHell {
	public class Player : Fighter {
		public override void Awake() {
			base.Awake ();

			if (Entity.Player)
				Debug.LogError ("There may only be one Player!", this);

			Entity.Player = this;
		}

		public override void Start () {
			base.Start ();

			IsEnemy = false;
		}

		public override void  Update () {
			Frozen = Input.GetAxis ("Horizontal") == 0 && Input.GetAxis ("Vertical") == 0;

			foreach (KeyValuePair<string, Weapon> pair in Weapons)
				pair.Value.CanFire = (Input.GetButton("Fire"));

			SetVelocity (new Vector2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical")) * Speed);

			base.Update ();
		}
	}
}