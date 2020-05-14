using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public static class UIDebug
	{
		private static bool _gbufferDebug;
		private static bool _showDemoWindow;
		
		public static void Show()
		{
			if (ImGui.Checkbox("GBuffer Debug View", ref _gbufferDebug))
			{
				Engine.Renderer.Debug = _gbufferDebug;
			}

			ImGui.Checkbox("Show Demo Window", ref _showDemoWindow);
			
			if (_showDemoWindow)
				ImGui.ShowDemoWindow();
		}
	}
}