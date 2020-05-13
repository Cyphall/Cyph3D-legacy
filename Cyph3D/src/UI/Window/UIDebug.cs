using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public static class UIDebug
	{
		private static bool _gbufferDebug;
		
		public static void Show()
		{
			if (ImGui.Checkbox("GBuffer Debug View", ref _gbufferDebug))
			{
				Engine.Renderer.Debug = _gbufferDebug;
			}
		}
	}
}