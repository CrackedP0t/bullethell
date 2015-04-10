using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System;
using MoonSharp.Interpreter;

namespace BulletHell {
	public class Base : MonoBehaviour {

		public static void thing() {
			print ("thing");
		}

		public string hi {
		get {
			return "hi";
		}
	}

		public bool isEnemy;
		public TextAsset ControlText;

		protected Script controlScript;
		protected DynValue controlData;

		protected Table globals;

		private Dictionary<string, DynValue> patterns = new Dictionary<string, DynValue>();

		public static Player Player {
			get {
				return UnityEngine.Object.FindObjectOfType<Player>();
			}
		}

		public static List<Fighter> Enemies {
			get {
				List<Fighter> enemies = new List<Fighter>();
				foreach (Fighter v in UnityEngine.Object.FindObjectsOfType<Fighter>()) {
					if (v.isEnemy)
						enemies.Add(v);
				}
				return enemies;
			}
		}

		//----------------------------------------

		public virtual Base Spawn(string path, float x=0, float y=0) {
			GameObject thing = (GameObject)Instantiate (Resources.Load (path), new Vector2(x, y), Quaternion.Euler(Vector3.zero));
			return thing.GetComponent<Base> ();
		}

		public void StartPattern(string name) {
			if (patterns.ContainsKey (name)) {
				scanPattern (patterns [name]);
			} else {
				Debug.LogError ("There is no pattern named \"" + name + "\"!", this);
			}
		}

		public void Kill() {
			Destroy (this.gameObject);
		}

		//----------------------------------------

		protected virtual void scanPattern (DynValue pattern) {

			if (pattern.Table.Get ("every").CastToBool ()) {
				Dictionary<DynValue, DynValue> every = pattern.Table.Get ("every").ToObject<Dictionary<DynValue, DynValue>> ();
				foreach (KeyValuePair<DynValue, DynValue> pair in every) {
					if (pair.Key.Type == DataType.Number) {
						StartCoroutine (doEvery ((float)pair.Key.CastToNumber () / 1000, () => {
							controlScript.Call (pair.Value); }));
					} else {
						Debug.LogWarning ("The key \"" + pair.Key.CastToString () + "\" is not a number!", this);
					}
				}
			}

			if (pattern.Table.Get ("patterns").CastToBool ()) {
				Dictionary<DynValue, DynValue> subpatterns = pattern.Table.Get ("patterns").ToObject<Dictionary<DynValue, DynValue>> ();
				foreach (KeyValuePair<DynValue, DynValue> pair in subpatterns) {
					if (pair.Key.Type == DataType.String) {
						patterns [pair.Key.CastToString ()] = subpatterns [pair.Key];
					} else {
						Debug.LogWarning ("The key \"" + pair.Key.CastToString () + "\" is not a string!", this);
					}
				}
			}
			
			if (pattern.Table.Get ("at").CastToBool ()) {
				Dictionary<DynValue, DynValue> at = pattern.Table.Get ("at").ToObject<Dictionary<DynValue, DynValue>> ();
				foreach (KeyValuePair<DynValue, DynValue> pair in at) {
					if (pair.Key.CastToString () == "start") {
						controlScript.Call (pair.Value);
					} else if (pair.Key.Type == DataType.Number) {
						StartCoroutine (doAfter ((float)pair.Key.CastToNumber () / 1000, () => {
							controlScript.Call (pair.Value); }));
					} else {
						Debug.LogWarning ("The key \"" + pair.Key.CastToString () + "\" is not \"start\" or a number!", this);
					}
				}
			}
		}

		protected void loadControlScript(string text) {
			UserData.RegisterType<Base> ();
			UserData.RegisterType<Player> ();
			UserData.RegisterType<Movable> ();
			UserData.RegisterType<Fighter> ();
			UserData.RegisterType<Bullet> ();
			UserData.RegisterType<Vector2> ();
			UserData.RegisterType<Rigidbody2D> ();
			
			controlScript = new Script ();
			
			controlScript.Options.DebugPrint = s=>{Debug.Log(s);};
			
			controlScript.Globals ["this"] = this;

			/*foreach (MemberInfo info in this.GetType().GetMembers (BindingFlags.Static | BindingFlags.Public)) {
				if (!info.Name.Contains("get_") && !info.Name.Contains("set_")) {
					controlScript.Globals[info.Name] = 
				}
				//info.Invoke(this, null);
			}*/

			controlScript.Globals["bullethell"] = this.GetType();
			
			controlData = controlScript.DoString (text);
			
			scanPattern ((DynValue)controlData);
		}

		protected IEnumerator doEvery(float seconds, Action action) {
			while (true) {
				yield return new WaitForSeconds (seconds);
				action ();
			}
		}
		
		protected IEnumerator doAfter (float seconds, Action action) {
			yield return new WaitForSeconds (seconds);
			action ();
		}

		void Awake() {
			Application.targetFrameRate = 60;
		}
		
		public virtual void Start () {
			if (ControlText != default(TextAsset))
				loadControlScript (ControlText.text);			            
		}

		public virtual void Update () {

		}
	}
}
