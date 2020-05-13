namespace Cyph3D
{
	internal static class Program
	{
		private static void Main()
		{
			Engine.Init();

			Engine.Run();

			Engine.Shutdown();
		}
	}
}