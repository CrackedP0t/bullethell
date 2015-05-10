using System.Collections;
using UnityEngine;
using MoonSharp.Interpreter;

namespace BulletHell {
	public class Bullet : Movable {

		public int Damage = 10;

		public override void Update() {
			if (!GetComponent<SpriteRenderer> ().isVisible)
				Kill ();
			base.Update ();
		}

		void OnTriggerEnter2D(Collider2D collision)
		{
			Callbacks.Call ("collision", DynValue.FromObject(controlScript, collision));

			Entity collisionentity = collision.gameObject.GetComponent<Entity> ();
			if (collisionentity.IsEnemy != IsEnemy && collisionentity is Fighter) {
				Fighter collisionfighter = (Fighter)collisionentity;
				collisionfighter.Hit(Damage);
				Kill ();
			}
		}

	}
}