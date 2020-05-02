using System;
using System.Runtime.InteropServices;

namespace Renderer.Misc
{
	public unsafe class NativeArray<T> : IDisposable where T : unmanaged
	{
		private readonly T* _array;
		public uint Count { get; }
		public uint ByteSize => Count * (uint)sizeof(T);
		
		public NativeArray(int count, T defaultValue = default)
		{
			Count = (uint)count;
			_array = (T*) Marshal.AllocHGlobal(new IntPtr(sizeof(T) * Count)).ToPointer();
			for (int i = 0; i < Count; i++)
			{
				_array[i] = defaultValue;
			}
		}

		public T this[int i]
		{
			get
			{
				if (i >= Count) throw new IndexOutOfRangeException("Index is out of bounds");
				if (i < 0) throw new IndexOutOfRangeException("Index cannot be negative");
				return _array[i];
			}
			set
			{
				if (i >= Count) throw new IndexOutOfRangeException("Index is out of bounds");
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