using UnityEngine;
using System.Collections.Generic;
using BulletHell;

namespace BulletHell {
	public class Movable : Entity {

		public float Speed = 0;
		public float Rotation = 0;
		public bool Frozen = false;
		public Vector2 Position{
			get {
				return body.position;
			}
			set {
				body.position = value;
			}
		}

		[HideInInspector]
		public Movable Target = null;

		private Rigidbody2D body = new Rigidbody2D ();

		//---------------------------------------

		public override Entity Spawn(string path, float x=0, float y=0) {
			GameObject thing = (GameObject)Instantiate (Resources.Load (path), new Vector2(x + Position.x, y + Position.y), Quaternion.Euler(Vector3.zero));
			return thing.GetComponent<Entity> ();
		}

		public void SetPosition(float x, float y) {
			Position = new Vector2(x, y);
		}
		
		public Vector2 GetPosition() {
			return Position;
		}
		
		public void SetVelocity(Vector2 newvelocity) {
			Rotation = Mathf.Atan2 (newvelocity.y, newvelocity.x) * Mathf.Rad2Deg;
			//Speed = Mathf.Sqrt (Mathf.Pow (newvelocity.x, 2) + Mathf.Pow (newvelocity.y, 2));
			updateBody ();
		}
		
		public Vector2 GetVelocity() {
			return body.velocity;
		}

		public Fighter ClosestEnemy {
			get {
				List<Fighter> enemies = Enemies;
				if (enemies.Count > 0) {
					Fighter closestenemy = enemies[0];
					for(int i = 1; i < enemies.Count; i++) {
						if (Vector3.Distance(Position, enemies[i].Position) < Vector3.Distance(Position, closestenemy.Position)) {
							closestenemy = enemies[i];
						}
				 	}
					return closestenemy;
				}
				else {
					return null;
				}
	    	}
	    }

		public void AimAt(Vector2 target) {
			Rotation = Mathf.Atan2 (target.y - Position.y, target.x - Position.x) * Mathf.Rad2Deg;
		}

//		public void AimAt(Movable target) {
//			
//		}

		//-------------------------------------
		
		private void updateBody() {
			body.velocity = (!Frozen
			                 	? new Vector2(Mathf.Cos (Rotation * Mathf.Deg2Rad) * Speed, Mathf.Sin(Rotation * Mathf.Deg2Rad) * Speed)
			                 : Vector2.zero);
		}

		public override void Awake() {
			base.Awake ();
			this.body = GetComponent<Rigidbody2D> ();
		}

		public override void Start () {
			base.Start ();
		}

		public override void Update () {
			if (Target) {
				AimAt(Target.Position);
			}
			updateBody ();
			base.Update ();
		}
	}
}