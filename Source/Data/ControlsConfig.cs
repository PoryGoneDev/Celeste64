using System.Text.Json;
using System.Text.Json.Serialization;

namespace Celeste64;

public class ControlsConfig
{
	public const string FILTER_NINTENDO = "NINTENDO";
	public const string FILTER_XBOX = "XBOX";
	public const string FILTER_MOUSE_CAMERA = "MOUSE_CAMERA";
	public const string FILTER_KEYBOARD_CAMERA = "KEYBOARD_CAMERA";

	public const string FileName = "controls.json";

	public ActionBinding Jump { get; private set; } = new ActionBinding()
		.Add(Keys.C, Keys.Space)
		.Add([FILTER_MOUSE_CAMERA], MouseButtons.Left)
		.Add(Buttons.South, Buttons.North);

	public ActionBinding Dash { get; private set; } = new ActionBinding()
		.Add(Keys.X)
		.Add([FILTER_MOUSE_CAMERA], MouseButtons.Right)
		.Add(Buttons.West, Buttons.East);

	public ActionBinding Climb { get; private set; } = new ActionBinding()
		.Add(Keys.Z,Keys.V,Keys.LeftShift, Keys.RightShift)
		.Add(Buttons.LeftShoulder,Buttons.RightShoulder)
		.Add(Axes.LeftTrigger, 1, 0.4f)
		.Add(Axes.RightTrigger, 1, 0.4f);

	public ActionBinding Confirm { get; private set; } = new ActionBinding()
		.Add(Keys.C)
		.Add([FILTER_MOUSE_CAMERA], MouseButtons.Left)
		.Add([FILTER_XBOX], Buttons.South)
		.Add([FILTER_NINTENDO], Buttons.East);

	public ActionBinding Cancel { get; private set; } = new ActionBinding()
		.Add(Keys.X)
		.Add([FILTER_MOUSE_CAMERA], MouseButtons.Right)
		.Add([FILTER_XBOX], Buttons.East)
		.Add([FILTER_NINTENDO], Buttons.South);

	public ActionBinding Pause { get; private set; } = new ActionBinding()
		.Add(Keys.Enter, Keys.Escape)
		.Add(Buttons.Start, Buttons.Select, Buttons.Back);

	public StickBinding Move { get; private set; } = new StickBinding(0.35f, AxisBinding.Overlaps.TakeNewer)
		.AddWasd([FILTER_MOUSE_CAMERA])
		.AddArrowKeys([FILTER_KEYBOARD_CAMERA])
		.AddLeftJoystick();

	public StickBinding Camera { get; private set; } = new StickBinding(0.35f, AxisBinding.Overlaps.TakeNewer)
		.AddMouseMotion([FILTER_MOUSE_CAMERA])
		.AddWasd([FILTER_KEYBOARD_CAMERA])
		.AddRightJoystick();

	public ActionBinding MenuLeft { get; private set; } = new ActionBinding()
		.Add(Keys.Left, Keys.A)
        .Add(Buttons.Left)
		.AddLeftJoystickLeft();

	public ActionBinding MenuRight { get; private set; } = new ActionBinding()
		.Add(Keys.Right, Keys.D)
        .Add(Buttons.Right)
		.AddLeftJoystickRight();

	public ActionBinding MenuUp { get; private set; } = new ActionBinding()
		.Add(Keys.Up, Keys.W)
        .Add(Buttons.Up)
		.AddLeftJoystickUp();

	public ActionBinding MenuDown { get; private set; } = new ActionBinding()
		.Add(Keys.Down, Keys.S)
        .Add(Buttons.Down)
		.AddLeftJoystickDown();
}

[JsonSourceGenerationOptions(
	WriteIndented = true,
	UseStringEnumConverter = true,
	AllowTrailingCommas = true
)]
[JsonSerializable(typeof(ControlsConfig))]
internal partial class ControlsConfigContext : JsonSerializerContext {}
