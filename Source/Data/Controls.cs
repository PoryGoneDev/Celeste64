
namespace Celeste64;

public class Controls(Input input, ControlsConfig config, int controllerIndex)
{
	public readonly Input Input = input;
	public readonly int ControllerIndex = controllerIndex;

	public readonly VirtualStick Move = new(input, config.Move, controllerIndex);
	public readonly VirtualStick Camera = new(input, config.Camera, controllerIndex);
	public readonly VirtualAction Jump = new(input, config.Jump, controllerIndex);
	public readonly VirtualAction Dash = new(input, config.Dash, controllerIndex);
	public readonly VirtualAction Climb = new(input, config.Climb, controllerIndex);
	public readonly VirtualAction Pause = new(input, config.Pause, controllerIndex);
	public readonly VirtualAction Confirm = new(input, config.Confirm, controllerIndex);
	public readonly VirtualAction Cancel = new(input, config.Cancel, controllerIndex);

	public readonly (VirtualAction Left, VirtualAction Right, VirtualAction Up, VirtualAction Down) Menu = (
		new(input, config.MenuLeft, controllerIndex),
		new(input, config.MenuRight, controllerIndex),
		new(input, config.MenuUp, controllerIndex),
		new(input, config.MenuDown, controllerIndex)
	);

	public void Consume()
	{
		Jump.ConsumePress();
		Dash.ConsumePress();
		Climb.ConsumePress();
		Confirm.ConsumePress();
		Cancel.ConsumePress();
		Pause.ConsumePress();
	}

	private readonly Dictionary<string, Dictionary<string, string>> prompts = [];

	private string GetControllerName(GamepadProviders pad) => pad switch
	{
		GamepadProviders.PlayStation => "PlayStation 5",
		GamepadProviders.Nintendo => "Nintendo Switch",
		GamepadProviders.Xbox => "Xbox Series",
        _ => "Xbox Series",
    };

    // TODO
/**    public static void ConsumePress()
    {
        Move.ConsumePress();
        Menu.ConsumePress();
        Camera.ConsumePress();
        Jump.ConsumePress();
        Dash.ConsumePress();
        Climb.ConsumePress();
        Confirm.ConsumePress();
        Cancel.ConsumePress();
        Pause.ConsumePress();
    }**/

	private string GetPromptLocation(string name)
	{
		var gamepad = Input.Controllers[ControllerIndex];
		var deviceTypeName = 
			gamepad.Connected ? GetControllerName(gamepad.GamepadProvider) : "PC";

		if (!prompts.TryGetValue(deviceTypeName, out var list))
			prompts[deviceTypeName] = list = [];

		if (!list.TryGetValue(name, out var lookup))
			list[name] = lookup = $"Controls/{deviceTypeName}/{name}";
					
		return lookup;
	}

	public string GetPromptLocation(VirtualAction button)
	{
		// TODO: instead, query the button's actual bindings and look up a
		// texture based on that! no time tho
		if (button == Confirm)
			return GetPromptLocation("confirm");
		else
			return GetPromptLocation("cancel");
	}

	public Subtexture GetPrompt(VirtualAction button)
	{
		return Assets.Subtextures.GetValueOrDefault(GetPromptLocation(button));
	}

	public bool IsUsingNintendo => Input.Controllers[ControllerIndex].GamepadProvider == GamepadProviders.Nintendo;
}
