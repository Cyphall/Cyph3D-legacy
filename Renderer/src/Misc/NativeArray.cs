using System;
using System.Runtime.InteropServices;

namespace Renderer.Misc
{
	public unsafe class NativeArray<T> : IDisposable where T : unmanaged
	{
		private readonly T* _array;
		public uint size { get; }
		
		public NativeArray(uint size, T defaultValue)
		{
			this.size = size;
			_array = (T*) Marshal.AllocHGlobal(new IntPtr(sizeof(T) * size)).ToPointer();
			for (int i = 0; i < size; i++)
			{
				_array[i] = defaultValue;
			}
		}

		public T this[int i]
		{
			get
			{
				if (i >= size) throw new IndexOutOfRangeException("Index is out of bounds");
				if (i < 0) throw new IndexOutOfRangeException("Index cannot be negative");
				return _array[i];
			}
			set
			{
				if (i >= size) throw new IndexOutOfRangeException("Index is out of bounds");
				if (i < 0) throw new IndexOutOfRangeException("Index cannot be negative");
				_array[i] = value;
			}
		}
		
		public static implicit operator IntPtr(NativeArray<T> list)
		{
			return new IntPtr(list._array);
		}
		
		public void Dispose()
		{
			Marshal.FreeHGlobal(new IntPtr(_array));
		}
	}
}