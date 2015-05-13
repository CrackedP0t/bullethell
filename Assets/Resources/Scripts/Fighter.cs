using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace BulletHell {
	public class Fighter : Movable {

		// Public Fields and Properties

		public bool Invulnerable = false;

		public float Health = 50;

		public Dictionary<string, Weapon> Weapons = new Dictionary<string, Weapon> ();

		// Public Methods

		public virtual void Hit(float damage) {
			Callbacks.Call ("hit", DynValue.NewNumber(damage));
			if (!Invulnerable) {
				Health = Health - damage;
				if (Health <= 0)
					Kill ();
			}
		}

		// Private and Protected Methods

		protected override void scanPattern(DynValue pattern) {
			if (pattern.Table.Get ("weapons").CastToBool ()) {
				Dictionary<string, DynValue> weapons = pattern.Table.Get ("weapons").ToObject<Dictionary<string, DynValue>> ();
				foreach (KeyValuePair<string, DynValue> pair in weapons) {
					Weapons.Add(pair.Key, new Weapon(pair.Value.Table, controlScript, this));
				}
			}
			base.scanPattern (pattern);
		}
	}
}