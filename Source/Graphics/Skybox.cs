using System.Runtime.InteropServices;

namespace Celeste64;

public class Skybox
{
	public readonly GraphicsDevice GraphicsDevice;
	public readonly Texture Texture;
	private readonly Mesh<SpriteVertex> mesh;
	private readonly Material material = new(Assets.Shaders["Sprite"]);

	public Skybox(Texture texture)
	{
		GraphicsDevice = texture.GraphicsDevice;
		Texture = texture;

		var block = Texture.Width / 4;
		
		var u = new Subtexture(Texture, new Rect(block * 0, block * 0, block, block));
		var d = new Subtexture(Texture, new Rect(block * 0, block * 2, block, block));
		var n = new Subtexture(Texture, new Rect(block * 0, block * 1, block, block));
		var e = new Subtexture(Texture, new Rect(block * 1, block * 1, block, block));
		var s = new Subtexture(Texture, new Rect(block * 2, block * 1, block, block));
		var w = new Subtexture(Texture, new Rect(block * 3, block * 1, block, block));

		var v0 = new Vec3(-1, -1, 1);
		var v1 = new Vec3(1, -1, 1);
		var v2 = new Vec3(1, 1, 1);
		var v3 = new Vec3(-1, 1, 1);
		var v4 = new Vec3(-1, -1, -1);
		var v5 = new Vec3(1, -1, -1);
		var v6 = new Vec3(1, 1, -1);
		var v7 = new Vec3(-1, 1, -1);
		
		var verts = new List<SpriteVertex>();
		var indices = new List<int>();

		AddFace(verts, indices, v0, v1, v2, v3, u.TexCoords[3], u.TexCoords[2], u.TexCoords[1], u.TexCoords[0]);
		AddFace(verts, indices, v7, v6, v5, v4, d.TexCoords[3], d.TexCoords[2], d.TexCoords[1], d.TexCoords[0]);
		AddFace(verts, indices, v4, v5, v1, v0, n.TexCoords[2], n.TexCoords[3], n.TexCoords[0], n.TexCoords[1]);
		AddFace(verts, indices, v6, v7, v3, v2, s.TexCoords[2], s.TexCoords[3], s.TexCoords[0], s.TexCoords[1]);
		AddFace(verts, indices, v0, v3, v7, v4, e.TexCoords[0], e.TexCoords[1], e.TexCoords[2], e.TexCoords[3]);
		AddFace(verts, indices, v5, v6, v2, v1, w.TexCoords[2], w.TexCoords[3], w.TexCoords[0], w.TexCoords[1]);

		mesh = new(texture.GraphicsDevice);
		mesh.SetVertices(CollectionsMarshal.AsSpan(verts));
		mesh.SetIndices(CollectionsMarshal.AsSpan(indices));
	}

	private static void AddFace(List<SpriteVertex> verts, List<int> indices, in Vec3 a, in Vec3 b, in Vec3 c, in Vec3 d, in Vec2 v0, in Vec2 v1, in Vec2 v2, in Vec2 v3)
	{
		int n = verts.Count;

		verts.Add(new SpriteVertex(a, v0, Color.White));
		verts.Add(new SpriteVertex(b, v1, Color.White));
		verts.Add(new SpriteVertex(c, v2, Color.White));
		verts.Add(new SpriteVertex(d, v3, Color.White));

		indices.Add(n + 0);indices.Add(n + 1);indices.Add(n + 2);
		indices.Add(n + 0);indices.Add(n + 2);indices.Add(n + 3);
	}

	public void Render(in Camera camera, in Matrix transform, float size)
	{
		var mat = Matrix.CreateScale(size) * transform * camera.ViewProjection;
		
		material.Vertex.SetUniformBuffer(new UniformBuffers.SpriteVertex(mat));
		material.Fragment.SetUniformBuffer(new UniformBuffers.SpriteFragment(camera));
		material.Fragment.Samplers[0] = new(Texture, new TextureSampler(TextureFilter.Linear, TextureWrap.Clamp, TextureWrap.Clamp));

		GraphicsDevice.Draw(new(camera.Target, mesh, material)
		{
			DepthTestEnabled = false,
			DepthWriteEnabled = false,
			DepthCompare = DepthCompare.Always,
			CullMode = CullMode.Back
		});
	}
}
