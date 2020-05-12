using GlmSharp;

namespace Renderer.Misc
{
	public class ActiveTransform : Transform
	{
		public vec3 Velocity { get; set; }
		public vec3 AngularVelocity { get; set; }

		public ActiveTransform(string name = null, Transform parent = null, vec3? position = null, vec3? rotation = null, vec3? scale = null, vec3? velocity = null, vec3? angularVelocity = null)
		{
			Name = name ?? "Object";
			Parent = parent;
			Position = position ?? vec3.Zero;
			Rotation = rotation ?? vec3.Zero;
			Scale = scale ?? vec3.Ones;
			Velocity = velocity ?? vec3.Zero;
			AngularVelocity = angularVelocity ?? vec3.Zero;
			
			MatrixChanged();
		}

		public void Update(double deltaTime)
		{
			Position += Velocity * (float)deltaTime;
			Rotation += AngularVelocity * (float)deltaTime;
		}
	}
}