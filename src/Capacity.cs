using System;
using System.Collections.Generic;

namespace Formulas {
	/// <summary>Allows a certain amount of objects to be cached</summary>
	public class Capacity {
		/// <summary>Maximum number of types to store information on at once</summary>
		public int max {
			get => _max;
			set {
				if(cache.Count > value)
					for(var i = cache.Count - value - 1; i >= 0; i--) {
						cache.Remove(types.First.Value);
						types.RemoveFirst();
					}

				_max = value;
			}
		}

		private IDictionary<Type, IDictionary<string, object>> cache = new Dictionary<Type, IDictionary<string, object>>();
		private LinkedList<Type> types = new LinkedList<Type>(); //Use a LinkedHashSet equivalent extend for faster finds
		private int _max;

		public Capacity(int max) => _max = max;

		/// <summary>Stores a value related to a type with a key</summary>
		/// <param name="type">Type that the key/value pair is related to</param>
		/// <param name="key">Key for the value</param>
		/// <param name="value">Value at the key</param>
		/// <returns>The value</returns>
		public object Store(Type type, string key, object value) {
			if(!cache.TryGetValue(type, out var map)) {
				if(cache.Count > _max) {
					cache.Remove(types.First.Value);
					types.RemoveFirst();
				}

				types.AddLast(type);
				map = cache[type] = new Dictionary<string, object>();
			}

			return map[key] = value;
		}

		/// <summary>Retrieves a value related to a type with a key</summary>
		/// <param name="type">Type that the key/value pair is related to</param>
		/// <param name="key">Key for the value</param>
		/// <param name="value">Value to assign from retrieval</param>
		/// <typeparam name="T">Type of the retrieved value</typeparam>
		/// <returns>True if the capacity contained a value at the key related to the type, false otherwise</returns>
		public bool Retrieve<T>(Type type, string key, out T value) {
			if(cache.TryGetValue(type, out var map) && map.TryGetValue(key, out var v)) {
				value = (T)v;
				var node = types.Find(type);
				types.Remove(node);
				types.AddLast(node);

				return true;
			}

			value = default(T);

			return false;
		}
	}
}