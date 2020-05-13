using ImGuiNET;

namespace Cyph3D.UI.Window
{
	public class UIDebug : IUIWindow
	{
		private bool _gbufferDebug;
		
		public void Show()
		{
			if (ImGui.Checkbox("GBuffer Debug View", ref _gbufferDebug))
			{
				Engine.Renderer.Debug = _gbufferDebug;
			}
		}
	}
}