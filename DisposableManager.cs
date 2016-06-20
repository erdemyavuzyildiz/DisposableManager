using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PingTest
{
	public static class DisposableManager
	{
		public static ConcurrentDictionary<Guid, List<IDisposable>> DisposablesConcurrentDictionary { get; set; } =
			new ConcurrentDictionary<Guid, List<IDisposable>>();

		public static void DisposeAll(Guid guid)
		{
			if (DisposablesConcurrentDictionary.ContainsKey(guid))
			{
				foreach (var disposable in DisposablesConcurrentDictionary[guid])
				{
					try
					{
						disposable.Dispose();
					}
					catch (Exception)
					{
					}
				}
				DisposablesConcurrentDictionary.Clear();
			}

			foreach (var webdriver in DisposablesConcurrentDictionary.SelectMany(x => x.Value))
			{
				try
				{
					webdriver.Dispose();
				}
				catch (Exception)
				{
				}
			}
			DisposablesConcurrentDictionary.Clear();
		}

		public static void DisposeAll()
		{
			foreach (var disposable in DisposablesConcurrentDictionary.SelectMany(x => x.Value))
			{
				try
				{
					disposable.Dispose();
				}
				catch (Exception)
				{
				}
			}
			DisposablesConcurrentDictionary.Clear();
		}

		public static void Add(Guid guid, IDisposable disposable)
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
		}

		public static IDisposable GetNewObject<T>(T webDriverType, params object[] objects) where T : IDisposable
		{
			var driver = (IDisposable) Activator.CreateInstance(typeof(T), objects);
			Add(Guid.NewGuid(), driver);

			return driver;
		}
	}
}