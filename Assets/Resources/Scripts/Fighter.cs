using UnityEngine;
using System.Collections;
using System;
using BulletHell;
using MoonSharp.Interpreter;

namespace BulletHell {
	public class Fighter : Movable {

		//=== Public Fields and Properties ===//

		public int Health = 50;

		public bool CanFire = true;
		
		public int FireRate = 500;

		//=== Private and Protected Fields and Properties ===//
		
		protected DynValue weapons = new DynValue();

		protected DynValue weaponFunction = new DynValue();

		private string _weaponname = "";
		public string Weapon {
			get {
				return _weaponname;
			}
			set {
				_weaponname = value;
				weaponFunction = weapons.Table.Get (_weaponname).Table.Get ("attack");
				FireRate = (int)weapons.Table.Get (_weaponname).Table.Get ("rate").CastToNumber();
			}
		}

		//=== Public Methods ===//

		public void Hit(int damage) {
			Health = Health - damage;
			if (Health <= 0) {
				Kill ();
			}
		}

		//=== Private and Protected methods ===//

		protected override void scanPattern(DynValue pattern) {
			if (pattern.Table.Get ("weapons").CastToBool ()) {
				weapons = pattern.Table.Get ("weapons");
			}
			base.scanPattern (pattern);
		}

		IEnumerator startFiring() {
			while (true) {
				yield return new WaitForSeconds ((float)FireRate / 1000);
				if (weaponFunction.Type == DataType.Function && CanFire)
					controlScript.Call (weaponFunction);
			}
		}

		//=== Unity Specific Methods ===//
		public override void Start () {
			base.Start ();
			StartCoroutine(startFiring ());
		}

	}
}