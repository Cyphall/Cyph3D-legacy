using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using GLFWWindow = OpenToolkit.Windowing.GraphicsLibraryFramework.Window;

namespace Cyph3D.Misc
{
	public unsafe class ThreadPool : IDisposable
	{
		private BlockingCollection<Action> _tasks = new BlockingCollection<Action>();
		private HashSet<Thread> _threads = new HashSet<Thread>();

		public ThreadPool(int threadCount)
		{
			for (int i = 0; i < threadCount; i++)
			{
				Thread thread = new Thread(ThreadProgram)
				{
					IsBackground = true
				};
				thread.Start();
				_threads.Add(thread);
			}
		}

		public void Schedule(Action task)
		{
			_tasks.Add(task);
		}

		private void ThreadProgram()
		{
			while (!_tasks.IsAddingCompleted)
			{
				_tasks.Take().Invoke();
			}
		}

		public void Dispose()
		{
			_tasks.CompleteAdding();
		}
	}
}