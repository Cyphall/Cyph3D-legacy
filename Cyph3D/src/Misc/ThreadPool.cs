using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using GLFWWindow = OpenToolkit.Windowing.GraphicsLibraryFramework.Window;

namespace Cyph3D.Misc
{
	public unsafe class ThreadPool : IDisposable
	{
		private BlockingCollection<Action> _tasks = new BlockingCollection<Action>();
		private HashSet<Thread> _threads = new HashSet<Thread>();

		public ThreadPool(int threadCount)
		{
			GLFW.WindowHint(WindowHintBool.Visible, false);
			for (int i = 0; i < threadCount; i++)
			{
				Thread thread = new Thread(ThreadProgram)
				{
					IsBackground = true
				};
				thread.Start((IntPtr)GLFW.CreateWindow(1, 1, "", null, Engine.Window));
				_threads.Add(thread);
			}
			GLFW.WindowHint(WindowHintBool.Visible, true);
		}

		public void Schedule(Action task)
		{
			_tasks.Add(task);
		}

		private void ThreadProgram(object o)
		{
			GLFW.MakeContextCurrent((GLFWWindow*)(IntPtr) o);

			while (!_tasks.IsAddingCompleted)
			{
				_tasks.Take().Invoke();
			}
			
			GLFW.MakeContextCurrent(null);
		}

		public void Dispose()
		{
			_tasks.CompleteAdding();
		}
	}
}