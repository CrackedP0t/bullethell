using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

namespace BulletHell
{
	public class Entity : MonoBehaviour
	{
		public List<string> Tags = new List<string> ();

		protected CallbackDict Callbacks;

		public bool IsEnemy;
		public TextAsset ControlText;
		protected Script controlScript = new Script ();
		protected DynValue controlData = DynValue.NewNil ();
		private Dictionary<string, DynValue> patterns = new Dictionary<string, DynValue> ();

		public static Player Player = null;

		public static List<Entity> Entities = new List<Entity> ();

		public static List<Fighter> Enemies {
			get {
				List<Fighter> enemies = new List<Fighter> ();
				foreach (Entity v in Entities) {
					if (v.IsEnemy && v is Fighter)
						enemies.Add ((Fighter)v);
				}
				return enemies;
			}
		}
		
		public static List<Entity> GetByTag (string tag)
		{
			List<Entity> tagged = new List<Entity> ();
			foreach (Entity entity in Entities) {
				if (entity.Tags.Contains (tag))
					tagged.Add (entity);
			}
			return tagged;
		}

		public static void LoadLevel(string id) {
			Application.LoadLevel (id);
		}

		public static void LoadLevel(int id) {
			Application.LoadLevel (id);
		}

		//----------------------------------------

		public virtual Entity Spawn (string path, float x=0, float y=0)
		{
			GameObject thing = (GameObject)Instantiate (Resources.Load (path), new Vector2 (x, y), Quaternion.Euler (Vector3.zero));
			return thing.GetComponent<Entity> ();
		}

		public void StartPattern (string name)
		{
			if (patterns.ContainsKey (name)) {
				scanPattern (patterns [name]);
			} else {
				Debug.LogError ("There is no pattern named \"" + name + "\"!", this);
			}
		}

		public void Kill ()
		{
			Callbacks.Call ("kill");
			Destroy (gameObject);
		}

		//----------------------------------------

		protected virtual void scanPattern (DynValue pattern)
		{

			if (pattern.Table.Get ("every").CastToBool ()) {
				Dictionary<DynValue, DynValue> every = pattern.Table.Get ("every").ToObject<Dictionary<DynValue, DynValue>> ();
				foreach (KeyValuePair<DynValue, DynValue> pair in every) {
					if (pair.Key.Type == DataType.Number) {
						StartCoroutine (doEvery ((float)pair.Key.CastToNumber () / 1000, () => {
							controlScript.Call (pair.Value);
						}));
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
					if (pair.Key.Type == DataType.Number) {
						StartCoroutine (doAfter ((float)pair.Key.CastToNumber () / 1000, () => {
							controlScript.Call (pair.Value); 
						}));
					} else {
						Debug.LogWarning ("The key \"" + pair.Key.CastToString () + "\" is not a number!", this);
					}
				}
			}

			if (pattern.Table.Get ("on").CastToBool ()) {
				Dictionary<DynValue, DynValue> on = pattern.Table.Get ("on").ToObject<Dictionary<DynValue, DynValue>> ();
				foreach (KeyValuePair<DynValue, DynValue> pair in on) {
					if (pair.Key.Type == DataType.String) {
						Callbacks [pair.Key.CastToString ()].Add (pair.Value);
					} else {
						Debug.LogWarning ("The key \"" + pair.Key.CastToString () + "\" is not a string!", this);
					}
				}
			}

			if (pattern.Table.Get ("tags").CastToBool ())
				Tags = pattern.Table.Get ("tags").ToObject<List<string>> ();
		}

		private void loadControlScript (string text)
		{		
			controlScript.Options.DebugPrint = (s)=>Debug.Log (s, this);


			
			controlScript.Globals ["this"] = this;

			controlScript.Globals ["whatnotg"] = new Action(() => print("whatnot"));

			Table globalMetaTable = new Table (controlScript);
			globalMetaTable ["__index"] = UserData.CreateStatic (typeof(Entity));
			controlScript.Globals.MetaTable = globalMetaTable;

			controlData = controlScript.DoString (text);

			scanPattern (controlData);
		}

		protected IEnumerator doEvery (float seconds, Action action)
		{
			while (true) {
				yield return new WaitForSeconds (seconds);
				action ();
			}
		}
		
		protected IEnumerator doAfter (float seconds, Action action)
		{
			yield return new WaitForSeconds (seconds);
			action ();
		}

		public virtual void Awake ()
		{
			UserData.RegistrationPolicy = MoonSharp.Interpreter.Interop.InteropRegistrationPolicy.Automatic;

			Entities.Add (this);

			controlScript = new Script ();

			Callbacks = new CallbackDict (controlScript);

			Application.targetFrameRate = 60;
		}
		
		public virtual void Start ()
		{
			if (ControlText != default (TextAsset))
				loadControlScript (ControlText.text);

			Callbacks.Call ("start");
		}

		public virtual void Update ()
		{
			Callbacks.Call ("update", DynValue.NewNumber (Time.deltaTime));
		}

		public virtual void OnDestroy() {
			Entities.Remove (this);
		}
	}
}