using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BulletHell;
using MoonSharp.Interpreter;

namespace BulletHell {
	public class Fighter : Movable {

		// Public Fields and Properties

		public int Health = 50;

		public Dictionary<string, Weapon> Weapons = new Dictionary<string, Weapon> ();

		// Private and Protected Fields and Properties

		// Public Methods

		public void Hit(int damage) {
			Health = Health - damage;
			if (Health <= 0) {
				Kill ();
			}
		}

		// Private and Protected Methods

		protected override void scanPattern(DynValue pattern) {
			if (pattern.Table.Get ("weapons").CastToBool ()) {
				Dictionary<string, DynValue> weapons = pattern.Table.Get ("weapons").ToObject<Dictionary<string, DynValue>> ();
				foreach (KeyValuePair<string, DynValue> pair in weapons) {
					Weapons.Add(pair.Key, new Weapon(pair.Value, controlScript, this));
				}
			}
			base.scanPattern (pattern);
		}

		// Unity Specific Methods

		public override void Start () {
			base.Start ();
		}

	}
}