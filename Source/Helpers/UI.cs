
namespace Celeste64;

public static class UI
{
	public const float IconSize = 30 * Game.RelativeScale;
	public const float PromptSize = 28 * Game.RelativeScale;

	public static void Text(Batcher batch, string text, in Vec2 at, in Vec2 justify, in Color color)
	{
		var font = Language.Current.SpriteFont;
		for (int x = -1; x <= 1; x++)
			for (int y = -1; y <= 3; y++)
				batch.Text(font, text, at + new Vec2(x, y), justify, Color.Black);
		batch.Text(font, text, at, justify, color);
	}

	public static void Icon(Batcher batch, string icon, string label, in Vec2 at, float align = 0)
	{
		Icon(batch, Assets.Subtextures.GetValueOrDefault(icon), label, at, align);
	}
	
	public static void Icon(Batcher batch, Subtexture icon, string label, in Vec2 at, float align = 0)
	{
		var pos = at;
		var size = IconSize;
		var iconAdvance = size * 0.7f;
		
		if (align > 0)
		{
			var font = Language.Current.SpriteFont;
			pos.X -= (font.WidthOf(label) + iconAdvance) * align;
		}

		for (int x = -1; x <= 1; x++)
			for (int y = -1; y <= 1; y++)
				if (x != 0 || y != 0)
				batch.ImageFit(icon, new Rect(pos.X + x, pos.Y + y, size, size), Vec2.One * 0.50f, Color.Black, false, false);
		batch.ImageFit(icon, new Rect(pos.X, pos.Y, size, size), Vec2.One * 0.50f, Color.White, false, false);
		
		Text(batch, label, new Vec2(pos.X + iconAdvance, pos.Y + size / 2), new Vec2(0, 0.5f), Color.White);
	}

	public static void Timer(Batcher batch, TimeSpan time, in Vec2 at, float align = 0)
	{
		string str;
		if ((int)time.TotalHours > 0)
			str = $"{((int)time.TotalHours):00}:{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds:000}";
		else
			str = $"{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds:000}";
		Icon(batch, "icon_stopwatch", str, at, align);
	}

	public static void Strawberries(Batcher batch, int count, int req, in Vec2 at, float align = 0)
	{
		Icon(batch, "icon_strawberry", $"x{count:00}/{req:00}  ", at, align);
	}

	public static void Deaths(Batcher batch, int count, in Vec2 at, float align = 0)
	{
		Icon(batch, "icon_skull", $"x{count:000}", at, align);
    }

    public static void DeathLinks(Batcher batch, int count, int req, in Vec2 at, float align = 0)
    {
        Icon(batch, "icon_skull", $"x{count:00}/{req:00}", at, align);
    }

    public static void Items(Batcher batch, in Vec2 at, float align = 0)
    {
		Vec2 pos = at;
		if (Save.CurrentRecord.GetFlag("Breakables") == 0)
		{
			Icon(batch, "Breakables_Grey", "", pos, align);
		}
		else
        {
            Icon(batch, "Breakables_Filled", "", pos, align);
        }
        pos.X += (UI.IconSize + 8);
        if (Save.CurrentRecord.GetFlag("Cassette") == 0)
        {
            Icon(batch, "Cassettes_Grey", "", pos, align);
        }
        else
        {
            Icon(batch, "Cassettes_Filled", "", pos, align);
        }
        pos.X -= (UI.IconSize + 8);
        pos.Y += (UI.IconSize + 8);
        if (Save.CurrentRecord.GetFlag("Coin") == 0)
        {
            Icon(batch, "Coins_Grey", "", pos, align);
        }
        else
        {
            Icon(batch, "Coins_Filled", "", pos, align);
        }
        pos.X += (UI.IconSize + 8);
        if (Save.CurrentRecord.GetFlag("DashRefill") == 0)
        {
            Icon(batch, "Dash_Grey", "", pos, align);
        }
        else
        {
            Icon(batch, "Dash_Filled", "", pos, align);
        }
        pos.X -= (UI.IconSize + 8);
        pos.Y += (UI.IconSize + 8);
        if (Save.CurrentRecord.GetFlag("DoubleDashRefill") == 0)
        {
            Icon(batch, "DoubleDash_Grey", "", pos, align);
        }
        else
        {
            Icon(batch, "DoubleDash_Filled", "", pos, align);
        }
        pos.X += (UI.IconSize + 8);
        if (Save.CurrentRecord.GetFlag("Feather") == 0)
        {
            Icon(batch, "Feather_Grey", "", pos, align);
        }
        else
        {
            Icon(batch, "Feather_Filled", "", pos, align);
        }
        pos.X -= (UI.IconSize + 8);
        pos.Y += (UI.IconSize + 8);
        if (Save.CurrentRecord.GetFlag("Spring") == 0)
        {
            Icon(batch, "Springs_Grey", "", pos, align);
        }
        else
        {
            Icon(batch, "Springs_Filled", "", pos, align);
        }
        pos.X += (UI.IconSize + 8);
        if (Save.CurrentRecord.GetFlag("TrafficBlock") == 0)
        {
            Icon(batch, "Traffic_Grey", "", pos, align);
        }
        else
        {
            Icon(batch, "Traffic_Filled", "", pos, align);
        }
        if (Game.Instance.ArchipelagoManager.MoveShuffle)
        {
            pos.X -= (UI.IconSize + 8);
            pos.Y += (UI.IconSize + 8);
            if (Save.CurrentRecord.GetFlag("Grounded Dash") == 0)
            {
                Icon(batch, "GroundDashGrey", "", pos, align);
            }
            else
            {
                Icon(batch, "GroundDash", "", pos, align);
            }
            pos.X += (UI.IconSize + 8);
            if (Save.CurrentRecord.GetFlag("Air Dash") == 0)
            {
                Icon(batch, "AirDashGrey", "", pos, align);
            }
            else
            {
                Icon(batch, "AirDash", "", pos, align);
            }
            pos.X -= (UI.IconSize + 8);
            pos.Y += (UI.IconSize + 8);
            if (Save.CurrentRecord.GetFlag("Skid Jump") == 0)
            {
                Icon(batch, "SkidJumpGrey", "", pos, align);
            }
            else
            {
                Icon(batch, "SkidJump", "", pos, align);
            }
            pos.X += (UI.IconSize + 8);
            if (Save.CurrentRecord.GetFlag("Climb") == 0)
            {
                Icon(batch, "ClimbGrey", "", pos, align);
            }
            else
            {
                Icon(batch, "Climb", "", pos, align);
            }
        }

        if (Game.Instance.ArchipelagoManager.BadelinesDisabled)
        {
            Icon(batch, "BadelineDisabled", "", new Vector2(100, 10), align);
        }
    }

    public static void Prompt(Batcher batch, VirtualButton button, string label, in Vec2 at, out float width, float align = 0)
	{
		var pos = at;
		var icon = Controls.GetPrompt(button);
		var size = PromptSize;
		var iconAdvance = size;
		var font = Language.Current.SpriteFont;
		width = (font.WidthOf(label) + iconAdvance);

		if (align > 0)
			pos.X -= width * align;

		for (int x = -1; x <= 1; x++)
			for (int y = -1; y <= 1; y++)
				if (x != 0 || y != 0)
				batch.ImageFit(icon, new Rect(pos.X + x, pos.Y + y, size, size), Vec2.One * 0.50f, Color.Black, false, false);
		batch.ImageFit(icon, new Rect(pos.X, pos.Y, size, size), Vec2.One * 0.50f, Color.White, false, false);
		
		Text(batch, label, new Vec2(pos.X + iconAdvance, pos.Y + size / 2), new Vec2(0, 0.5f), Color.White);
	}
}
