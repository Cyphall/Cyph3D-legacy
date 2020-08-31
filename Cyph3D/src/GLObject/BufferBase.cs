using System;

namespace Cyph3D.GLObject
{
	public abstract class BufferBase : IDisposable
	{
		protected int _id = -1;
		
		public static implicit operator int(BufferBase buffer) => buffer._id;

		public void Dispose()
		{
			if (_id == -1) return;
			
			DeleteBuffer();
			_id = -1;
		}
		
		protected abstract void DeleteBuffer();
	}
}