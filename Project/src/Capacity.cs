using System.Collections;
using System.Collections.Generic;

namespace Formulas {
	/// <summary>Allows a certain amount of objects to be cached</summary>
	class Capacity<TCategory, TKey, TValue> : IEnumerable<(TCategory, TKey, TValue)> {
		private IDictionary<TCategory, IDictionary<TKey, TValue>> cache = new Dictionary<TCategory, IDictionary<TKey, TValue>>();
		private LinkedList<TCategory> categories = new LinkedList<TCategory>();
		private readonly int max;

		/// <param name="max">Maximum number of categories to store information on at once</param>
		public Capacity(int max) => this.max = max;

		/// <summary>Stores a value related to a category with a key</summary>
		/// <param name="category">Category that the key/value pair is related to</param>
		/// <param name="key">Key for the value</param>
		/// <param name="value">Value at the key</param>
		/// <returns>The value</returns>
		public object Store(TCategory category, TKey key, TValue value) {
			if(!cache.TryGetValue(category, out var map)) {
				if(cache.Count > max) {
					cache.Remove(categories.First.Value);
					categories.RemoveFirst();
				}

				categories.AddLast(category);
				map = cache[category] = new Dictionary<TKey, TValue>();
			}

			return map[key] = value;
		}

		/// <summary>Retrieves a value related to a category with a key</summary>
		/// <param name="category">Category that the key/value pair is related to</param>
		/// <param name="key">Key for the value</param>
		/// <param name="value">Value to assign from retrieval</param>
		/// <categoryparam name="T">Category of the retrieved value</categoryparam>
		/// <returns>True if the capacity contained a value at the key related to the category, false otherwise</returns>
		public bool Retrieve(TCategory category, TKey key, out TValue value) {
			if(cache.TryGetValue(category, out var map) && map.TryGetValue(key, out value)) {
				var node = categories.Find(category);
				categories.Remove(node);
				categories.AddLast(node);

				return true;
			}

			value = default(TValue);
			return false;
		}

		public IEnumerator<(TCategory, TKey, TValue)> GetEnumerator() {
			foreach(var category in categories)
				foreach(var entry in cache[category])
					yield return (category, entry.Key, entry.Value);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}