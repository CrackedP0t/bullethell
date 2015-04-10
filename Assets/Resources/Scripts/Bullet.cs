using UnityEngine;
using System.Collections;
using System;
using MoonSharp.Interpreter;
using BulletHell;

namespace BulletHell {
	public class Bullet : Movable {

		public int Damage = 10;

		public override void Start ()
		{
			base.Start ();
		}

		public override void Update() {
			if (!GetComponent<SpriteRenderer> ().isVisible)
				Destroy (gameObject);
			base.Update ();
		}

		void OnTriggerEnter2D(Collider2D collision)
		{
			Base collisionbase = collision.gameObject.GetComponent<Base> ();
			if (collisionbase.isEnemy != this.isEnemy && collisionbase is Fighter) {
				Fighter collisionsoldier = (Fighter)collisionbase;
				collisionsoldier.Hit(this.Damage);
				Destroy(this.gameObject);
			}
		}

	}
}