
using static Celeste64.Menu;

namespace Celeste64;

public class BadelineChase : Actor, IHaveModels, IPickup, ICastPointShadow
{
    public SkinnedModel Model;
    private readonly Hair hair;
	private Color hairColor = 0x9B3FB5;

    public float PickupRadius => 12;

    public float PointShadowAlpha { get; set; }

    public BadelineChase()
    {
        Model = new SkinnedModel(Assets.Models["badeline"]);

        foreach (var mat in Model.Materials)
            mat.Effects = 0.70f;

        PointShadowAlpha = 1;
        LocalBounds = new BoundingBox(Vec3.Zero + Vec3.UnitZ * 4, 8);

        Model.Play("Bad.Idle");

		foreach (var mat in Model.Materials)
		{
			if (mat.Name == "Hair")
			{
				mat.Color = hairColor;
				mat.Effects = 0;
			}
            mat.SilhouetteColor = hairColor;
		}

        hair = new()
        {
            Color = hairColor,
			ForwardOffsetPerNode = 0,
            Nodes = 10
        };
	}

    public override void Update()
    {
        base.Update();
		
		// update model
		Model.Transform = 
			Matrix.CreateScale(3) * 
			Matrix.CreateTranslation(0, 0, MathF.Sin(World.GeneralTimer * 2) * 1.0f - 1.5f);

		// update hair
		{
			var hairMatrix = Matrix.Identity;
			foreach (var it in Model.Instance.Armature.LogicalNodes)
				if (it.Name == "Head")
					hairMatrix = it.ModelMatrix * SkinnedModel.BaseTranslation * Model.Transform * Matrix;
			hair.Flags = Model.Flags;
			hair.Forward = -new Vec3(Facing, 0);
			hair.Materials[0].Effects = 0;
			hair.Update(hairMatrix);
		}

    }

    public void Pickup(Player player)
    {
        if (!Game.Instance.IsMidTransition)
        {
            player.Kill();
        }
    }

    public void CollectModels(List<(Actor Actor, Model Model)> populate)
    {
        populate.Add((this, Model));
        populate.Add((this, hair));
    }
}

