using System;
using System.Runtime.InteropServices;

namespace Renderer
{
	public unsafe class NativeList<T> : IDisposable where T : unmanaged
	{
		private T* _array;
		public uint size { get; private set; }
		private uint _capacity;
		
		public NativeList(uint initialCapacity = 1)
		{
			_capacity = initialCapacity;
			_array = (T*) Marshal.AllocHGlobal(new IntPtr(sizeof(T) * _capacity)).ToPointer();
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
		
		public static implicit operator IntPtr(NativeList<T> list)
		{
			return new IntPtr(list._array);
		}

		public void add(params T[] elements)
		{
			foreach (T element in elements)
			{
				if (size == _capacity)
				{
					_capacity *= 2;
					_array = (T*) Marshal.ReAllocHGlobal(new IntPtr(_array), new IntPtr(sizeof(T) * _capacity)).ToPointer();
				}

				_array[size] = element;
				size++;
			}
		}
		
		public void Dispose()
		{
			Marshal.FreeHGlobal(new IntPtr(_array));
		}
	}
}