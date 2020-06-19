using ImGuiNET;

namespace Cyph3D.Extension
{
	public static class ImGuiExtension
	{
		public static unsafe bool IsValid(this ImGuiPayloadPtr payload)
		{
			return payload.NativePtr != null;
		}
	}
}