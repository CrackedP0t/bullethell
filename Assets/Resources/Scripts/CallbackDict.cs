using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace BulletHell
{
	public class CallbackDict
	{
		private List<DynValue> funcs = new List<DynValue> ();

		private Script script;

		public CallbackDict (Script newscript)
		{
			script = newscript;
		}

		public void Call(string key, params DynValue[] args) {
			foreach (DynValue v in this[key]) {
				script.Call(v, args);
			}
		}

		public void SetObject(string key, object obj) {
			// I feel ashamed of this code

			funcs.Add (DynValue.FromObject (script, obj));

			script.Globals ["funcs"] = funcs;

			this [key].Add (script.DoString (
				"return function (...)\n" +
				"\tfuncs[" + (funcs.Count - 1).ToString() +"].Invoke(...)\n" +
				"end"
			));
		}

		private Dictionary<string, List<DynValue>> callbacks = new Dictionary<string, List<DynValue>> ();

		public List<DynValue> this[string key] {
			get {
				if (!callbacks.ContainsKey(key))
					callbacks[key] = new List<DynValue> ();

				return callbacks[key];
			}
		}
	}
}