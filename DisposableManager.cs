using System;
using System.Collections.Concurrent;
using System.Linq;

namespace PingFunctions
{
	public static class DisposableManager
	{
		private static ConcurrentDictionary<Guid, ConcurrentDictionary<IDisposable, byte>> DisposablesConcurrentDictionary {
			get; } = new ConcurrentDictionary<Guid, ConcurrentDictionary<IDisposable, byte>>();

		public static void DisposeAll(Guid guid)
		{
			if (DisposablesConcurrentDictionary.ContainsKey(guid))
			{
				foreach (var disposable in DisposablesConcurrentDictionary[guid])
				{
					try
					{
						if (disposable.Key != null)
						{
							disposable.Key.Dispose();
						}
					}
					catch (Exception)
					{
						// ignored
					}
				}
				DisposablesConcurrentDictionary.Clear();
			}

			DisposablesConcurrentDictionary.Clear();
		}

		public static void DisposeAll()
		{
			foreach (var disposable in DisposablesConcurrentDictionary.SelectMany(x => x.Value))
			{
				try
				{
					if (disposable.Key != null)
					{
						disposable.Key.Dispose();
					}
				}
				catch (Exception)
				{
					// ignored
				}
			}
			DisposablesConcurrentDictionary.Clear();
		}

		public static Guid Add(IDisposable disposable)
		{
			return Add(disposable, Guid.NewGuid());
		}

		public static Guid Add(IDisposable disposable, Guid guid)
		{
			if (DisposablesConcurrentDictionary.ContainsKey(guid))
			{
				var existing = DisposablesConcurrentDictionary.SingleOrDefault(x => x.Key == guid);
				existing.Value.TryAdd(disposable, 0);
			}
			else
			{
				var newDict = new ConcurrentDictionary<IDisposable, byte>();
				newDict.TryAdd(disposable, 0);

				DisposablesConcurrentDictionary.TryAdd(guid, newDict);
			}

			return guid;
		}

		public static IDisposable GetNewObject<T>(T webDriverType, params object[] objects) where T : IDisposable
		{
			var driver = (IDisposable) Activator.CreateInstance(typeof(T), objects);
			Add(driver, Guid.NewGuid());

			return driver;
		}
	}
}