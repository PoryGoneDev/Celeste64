
using static Celeste64.Menu;

namespace Celeste64;

public class BadelineChase : Actor, IHaveModels, IHaveSprites, IPickup, ICastPointShadow
{
    public SkinnedModel Model;
    private readonly Hair hair;
	private Color hairColor = 0x9B3FB5;

    public float PickupRadius => 9;

    public float PointShadowAlpha { get; set; }
    private bool drawModel = true;
    private bool drawHair = true;
    private bool drawOrbs = false;
    private float drawOrbsEase = 0;

    public BadelineChase()
    {
        Model = new SkinnedModel(Assets.Models["badeline"]);

        foreach (var mat in Model.Materials)
            mat.Effects = 0.70f;

        PointShadowAlpha = 1;
        LocalBounds = new BoundingBox(Vec3.Zero + Vec3.UnitZ * 4, 8);

        Model.Flags |= ModelFlags.Silhouette;
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

        drawModel = drawHair = false;
        drawOrbs = true;
        drawOrbsEase = 1;
        Audio.Play(Sfx.sfx_revive, Position);
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

        if (drawOrbs)
        {
            drawOrbsEase -= Time.Delta * 2;
            if (drawOrbsEase <= 0)
            {
                drawModel = drawHair = true;
                drawOrbs = false;
            }
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
        if (drawHair)
            populate.Add((this, hair));

        if (drawModel)
            populate.Add((this, Model));
    }

    public void CollectSprites(List<Sprite> populate)
    {
        if (drawOrbs && drawOrbsEase > 0)
        {
            var ease = drawOrbsEase;
            var col = Math.Floor(ease * 10) % 2 == 0 ? hair.Color : Color.White;
            var s = (ease < 0.5f) ? (0.5f + ease) : (Ease.Cube.Out(1 - (ease - 0.5f) * 2));
            for (int i = 0; i < 8; i++)
            {
                var rot = (i / 8f + ease * 0.25f) * MathF.Tau;
                var rad = Ease.Cube.Out(ease) * 16;
                var pos = Position + Vec3.UnitZ * 3 + World.Camera.Left * MathF.Cos(rot) * rad + World.Camera.Up * MathF.Sin(rot) * rad;
                var size = 3 * s;
                populate.Add(Sprite.CreateBillboard(World, pos, "circle", size + 0.5f, Color.Black) with { Post = true });
                populate.Add(Sprite.CreateBillboard(World, pos, "circle", size, col) with { Post = true });
            }
        }
    }
}

