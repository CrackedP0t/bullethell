using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

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

			DynValue func = DynValue.FromObject (script, obj);

			funcs.Add (func);

			script.Globals ["funcs"] = funcs;

			DynValue value = script.DoString (
				"return function (...)\n" +
				"\tfuncs[" + (funcs.Count - 1).ToString() +"].Invoke(...)\n" +
                "end"
			);

			this [key].Add (value);
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