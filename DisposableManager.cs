using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PingFunctions
{
	public static class DisposableManager
	{
		private static ConcurrentDictionary<Guid, List<IDisposable>> DisposablesConcurrentDictionary { get; } =
			new ConcurrentDictionary<Guid, List<IDisposable>>();

		public static void DisposeAll(Guid guid)
		{
			if (DisposablesConcurrentDictionary.ContainsKey(guid))
			{
				foreach (var disposable in DisposablesConcurrentDictionary[guid])
				{
					try
					{
						if (disposable != null)
						{
							disposable.Dispose();
						}
					}
					catch (Exception)
					{
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
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
				catch (Exception)
				{
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
				existing.Value.Add(disposable);
			}
			else
			{
				DisposablesConcurrentDictionary.TryAdd(guid, new List<IDisposable> {disposable});
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