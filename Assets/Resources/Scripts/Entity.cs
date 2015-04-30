using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System;
using MoonSharp.Interpreter;

namespace BulletHell
{
	public class Entity : MonoBehaviour
	{
		public List<string> Tags = new List<string> ();
		protected Dictionary<string, DynValue> callbacks = new Dictionary<string, DynValue> ();

		public void Callback (string name, params DynValue[] args)
		{
			if (callbacks.ContainsKey (name))
				controlScript.Call (callbacks [name], args);

			/*MethodInfo cinfo = this.GetType ().GetMethod (name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic);
			if (cinfo != null)
				cinfo.Invoke (this, (System.Object[])args);*/
		}

		public bool IsEnemy;
		public TextAsset ControlText;
		protected Script controlScript = new Script ();
		protected DynValue controlData = DynValue.NewNil ();
		private Dictionary<string, DynValue> patterns = new Dictionary<string, DynValue> ();

		/*protected void update(System.Object[] dt) {
			print (dt);
		}*/

		public static Player Player {
			get {
				return UnityEngine.Object.FindObjectOfType<Player> ();
			}
		}

		public static List<Fighter> Enemies {
			get {
				List<Fighter> enemies = new List<Fighter> ();
				foreach (Fighter v in UnityEngine.Object.FindObjectsOfType<Fighter> ()) {
					if (v.IsEnemy)
						enemies.Add (v);
				}
				return enemies;
			}
		}

		public static List<Entity> Entities {
			get {
				return new List<Entity> (UnityEngine.Object.FindObjectsOfType<Entity> ());
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
			Callback ("kill");
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
						callbacks [pair.Key.CastToString ()] = pair.Value;
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
			UserData.RegistrationPolicy = MoonSharp.Interpreter.Interop.InteropRegistrationPolicy.Automatic;
			/*UserData.RegisterType<Entity> ();
			UserData.RegisterType<Player> ();
			UserData.RegisterType<Movable> ();
			UserData.RegisterType<Fighter> ();
			UserData.RegisterType<Bullet> ();
			UserData.RegisterType<Weapon> ();
			UserData.RegisterType<Vector2> ();
			UserData.RegisterType<Rigidbody2D> ();
			UserData.RegisterType<List<string>> ();
			UserData.RegisterType<Dictionary<string, Weapon>> ();*/

			controlScript = new Script ();
			
			controlScript.Options.DebugPrint = s => {
				Debug.Log (s);};
			
			controlScript.Globals ["this"] = this;

			/*foreach (MemberInfo info in this.GetType().GetMembers (BindingFlags.Static | BindingFlags.Public)) {
				if (!info.Name.Contains("get_") && !info.Name.Contains("set_")) {
					controlScript.Globals[info.Name] = 
				}
				//info.Invoke(this, null);
			}*/
			
			controlScript.Globals ["statics"] = UserData.CreateStatic (typeof(Entity));

			controlScript.DoString ("setmetatable(_G, {\n" +
				"\t__index = statics\n" +
				"})");

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

		void Awake ()
		{
			Application.targetFrameRate = 60;
		}
		
		public virtual void Start ()
		{
			if (ControlText != default (TextAsset))
				loadControlScript (ControlText.text);	
			Callback ("start");
		}

		public virtual void Update ()
		{
			Callback ("update", DynValue.NewNumber (Time.deltaTime));
		}
	}
}