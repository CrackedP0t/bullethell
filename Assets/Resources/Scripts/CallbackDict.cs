using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace BulletHell
{
	public class CallbackDict
	{
		private Script script = new Script ();

		public CallbackDict (Script newscript)
		{
			script = newscript;
		}

		public void Call(string key, params DynValue[] args) {
			foreach (DynValue v in this[key]) {
				script.Call(v, args);
			}
		}

		private Dictionary<string, List<DynValue>> callbacks = new Dictionary<string, List<DynValue>> ();

		public List<DynValue> this[string key] {
			get {
				if (!callbacks.ContainsKey(key)) {
					callbacks[key] = new List<DynValue> ();
				}
				return callbacks[key];
			}
		}
	}
}