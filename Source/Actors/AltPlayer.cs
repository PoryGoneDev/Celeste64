
using static Celeste64.Menu;

namespace Celeste64;

/// <summary>
/// </summary>
public class AltPlayer : Actor, IHaveModels, ICastPointShadow
{
	public Vec3 ModelScale = Vec3.One;
	public SkinnedModel Model;
	public readonly Hair Hair = new();
	public float PointShadowAlpha { get; set; } = 1.0f;


	public AltPlayer()
    {
        PointShadowAlpha = 1.0f;
		LocalBounds = new BoundingBox(new Vec3(0, 0, 10), 10);
		UpdateOffScreen = true;


		// setup model
		{
			Model = new(Assets.Models["player"]);
			Model.SetBlendDuration("Idle", "Dash", 0.05f);
			Model.SetBlendDuration("Idle", "Run", 0.2f);
			Model.SetBlendDuration("Run", "Skid", .125f);
			Model.SetLooping("Dash", false);
			Model.Flags |= ModelFlags.Silhouette;
			Model.Play("Idle");

			Model.MakeMaterialsUnique();

			foreach (var mat in Model.Materials)
				mat.Effects = 0.60f;
		}

		SetHairColor(0xdb2c00);
	}


	public override void LateUpdate()
	{
		// update model
		{
			Calc.Approach(ref ModelScale.X, 1, Time.Delta / .8f);
			Calc.Approach(ref ModelScale.Y, 1, Time.Delta / .8f);
			Calc.Approach(ref ModelScale.Z, 1, Time.Delta / .8f);

			Model.Update();
			Model.Transform = Matrix.CreateScale(ModelScale * 3);
		}

		// hair
		{
			var hairMatrix = Matrix.Identity;

			foreach (var it in Model.Instance.Armature.LogicalNodes)
			{
				if (it.Name == "Head")
				{
					hairMatrix = it.ModelMatrix * SkinnedModel.BaseTranslation * Model.Transform * Matrix;
				}
			}

			Hair.Flags = Model.Flags;
			Hair.Forward = -new Vec3(Facing, 0);
			Hair.Squish = ModelScale;
			Hair.Materials[0].Effects = 0;
			Hair.Update(hairMatrix);
		}
	}


	public void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		if ((World.Camera.Position - (Position + Vec3.UnitZ * 8)).LengthSquared() > World.Camera.NearPlane * World.Camera.NearPlane)
		{
			populate.Add((this, Hair));

			populate.Add((this, Model));
		}
	}

    public void SetHairColor(Color color)
    {
        foreach (var mat in Model.Materials)
        {
            if (mat.Name == "Hair")
            {
                mat.Color = color;
                mat.Effects = 0;
            }
            mat.SilhouetteColor = color;
        }

        Hair.Color = color;
        Hair.Nodes = 10;
    }
}
