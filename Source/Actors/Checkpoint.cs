
using Archipelago.MultiClient.Net.Packets;
using System;
using System.Diagnostics;

namespace Celeste64;

public class Checkpoint : Actor, IHaveModels, IPickup, IHaveSprites
{
	public readonly string Name;
	public SkinnedModel ModelOff;
	public SkinnedModel ModelOff_2;
	public SkinnedModel ModelOn;

	private float tWiggle = 0.0f;

	public Checkpoint(string name)
	{
		Name = name;
		LocalBounds = new BoundingBox(Vec3.Zero, 8);
		ModelOff = new(Assets.Models["flag_off"]);
		ModelOff.Play("Idle");
		ModelOff.Transform = Matrix.CreateScale(0.2f);

        ModelOff_2 = new(Assets.Models["flag_off_2"]);
        ModelOff_2.Play("Idle");
        ModelOff_2.Transform = Matrix.CreateScale(0.2f);

        ModelOn = new(Assets.Models["flag_on"]);
		ModelOn.Play("Idle");
		ModelOn.Transform = Matrix.CreateScale(0.2f);

		CurrentModel = ModelOn;

    }

	public float PickupRadius => 16;

	public SkinnedModel CurrentModel;

	public override void Added()
	{
		// if we're the spawn checkpoint, shift us so the player isn't on top
		if (IsCurrent())
			Position -= Vec3.UnitY * 8;
	}

	public override void Update()
	{
		if (IsCurrent())
		{
			if (CurrentModel != ModelOn)
            {
                Audio.Play(Sfx.sfx_checkpoint, Position);

                tWiggle = 1;
            }
			CurrentModel = ModelOn;
		}
		else if (IsLocationChecked())
        {
            CurrentModel = ModelOff_2;
        }
		else
        {
            CurrentModel = ModelOff;
        }

        Calc.Approach(ref tWiggle, 0, Time.Delta / 0.7f);
        CurrentModel.Update();
    }

	public void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		populate.Add((this, CurrentModel));
	}

	public bool IsCurrent()
    {
        return World.Entry.CheckPoint == Name;
    }

	public bool IsLocationChecked()
    {
        return Save.CurrentRecord.GetFlag(Name) != 0;
    }

    public bool HaveItem()
    {
        return Save.CurrentRecord.GetFlag("Item_" + Name) != 0;
    }

	public void Pickup(Player player)
	{
		Save.CurrentRecord.SetFlag(Name);
		World.AddCheckpointToHistory(Name);

		if (World.Entry.Submap)
		{
			World.Entry = World.Entry with { CheckPoint = Name };
		}
	}

	public void CollectSprites(List<Sprite> populate)
	{
		var haloPos = Position + Vec3.UnitZ * 16;
		var haloCol = new Color(IsCurrent() ? 0x7fde46 : (IsLocationChecked() ? 0x545cfc : 0xdf5ab4)) * .4f;
		populate.Add(Sprite.CreateBillboard(World, haloPos, "gradient", 12, haloCol * 0.40f));

		if (tWiggle > 0)
		{
			populate.Add(Sprite.CreateBillboard(World, haloPos, "ring", tWiggle * tWiggle * 40, haloCol) with { Post = true });
			populate.Add(Sprite.CreateBillboard(World, haloPos, "ring", tWiggle * 50, haloCol) with { Post = true });
		}
	}
}
