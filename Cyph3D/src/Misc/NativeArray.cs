﻿using System;
using System.Runtime.InteropServices;

namespace Cyph3D.Misc
{
	public unsafe class NativeArray<T> : IDisposable where T : unmanaged
	{
		private readonly T* _array;
		public int Count { get; }
		public int ByteSize => Count * sizeof(T);
		
		public NativeArray(int count, T defaultValue = default)
		{
			Count = count;
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