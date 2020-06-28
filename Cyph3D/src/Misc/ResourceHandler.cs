using System;
using System.Collections.Generic;

namespace Cyph3D.Misc
{
	public class ResourceHandler<T> : IDisposable where T : IDisposable
	{
		public delegate void ResourceCallback(T resource);

		public T Resource { get; private set; }
		private Queue<ResourceCallback> _pendingCallbacks = new Queue<ResourceCallback>();

		public void AddCallback(ResourceCallback callback)
		{
			if (Resource == null)
			{
				_pendingCallbacks.Enqueue(callback);
			}
			else
			{
				callback.Invoke(Resource);
			}
		}

		public void ValidateLoading(T resource)
		{
			Resource = resource;

			while (_pendingCallbacks.TryDequeue(out ResourceCallback callback))
			{
				callback.Invoke(Resource);
			}
		}

		public void Dispose()
		{
			Resource?.Dispose();
		}
	}
}