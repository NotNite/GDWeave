using GDWeave.Parser;
using GDWeave.Parser.Variants;

namespace GDWeave.Mods;

public class ControllerInput {
    private enum JoyAxis {
        LeftX = 0,
        LeftY = 1,
        RightX = 2,
        RightY = 3
    }

    private enum JoyButton {
        LeftStickIn = 8,
        RightStickIn = 9,
        FaceUp = 3,
        FaceRight = 1,
        FaceDown = 0,
        FaceLeft = 2,
        Select = 10,
        Start = 11,
        DPadUp = 12,
        DPadDown = 13,
        DPadLeft = 14,
        DPadRight = 15,
        ShoulderLeft = 4,
        ShoulderRight = 5,
        TriggerLeft = 6,
        TriggerRight = 7
    }

    // Custom
    private const string LookLeft = "gdweave_look_left";
    private const string LookRight = "gdweave_look_right";
    private const string LookUp = "gdweave_look_up";
    private const string LookDown = "gdweave_look_down";

    private const string ZoomControl = "gdweave_zoom_control";

    private const string TabNext = "gdweave_tab_next";
    private const string TabPrevious = "gdweave_tab_previous";

    // Movement
    private const string MoveLeft = "move_left";
    private const string MoveRight = "move_right";
    private const string MoveForward = "move_forward";
    private const string MoveBack = "move_back";
    private const string MoveJump = "move_jump";
    private const string MoveSprint = "move_sprint";
    private const string MoveWalk = "move_walk";

    // Actions
    private const string Interact = "interact";
    private const string Kiss = "kiss";
    private const string Bark = "bark";
    private const string PrimaryAction = "primary_action";

    // Menus
    private const string MenuOpen = "menu_open";
    private const string BaitMenu = "bait_menu";
    private const string EmoteWheel = "emote_wheel";
    private const string Build = "build";

    // Misc
    private const string Freecam = "freecam";
    private const string EscMenu = "esc_menu";

    public class InputRegister : ScriptMod {
        public override bool ShouldRun(string path) => path == "res://Scenes/Singletons/globals.gdc";

        public override IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
            var readyWaiter = new TokenWaiter(
                t => t.Type is TokenType.Newline && t.AssociatedData is 1,
                waitForReady: true
            );

            foreach (var token in tokens) {
                if (token is IdentifierToken {Name: "_ready"}) readyWaiter.SetReady();

                if (readyWaiter.Check(token)) {
                    yield return new Token(TokenType.Newline, 1);

                    foreach (var t in this.HandleStick(LookLeft, JoyAxis.RightX, -1)) yield return t;
                    foreach (var t in this.HandleStick(LookRight, JoyAxis.RightX, 1)) yield return t;
                    foreach (var t in this.HandleStick(LookUp, JoyAxis.RightY, 1)) yield return t;
                    foreach (var t in this.HandleStick(LookDown, JoyAxis.RightY, -1)) yield return t;

                    foreach (var t in this.HandleStick(MoveLeft, JoyAxis.LeftX, -1)) yield return t;
                    foreach (var t in this.HandleStick(MoveRight, JoyAxis.LeftX, 1)) yield return t;
                    foreach (var t in this.HandleStick(MoveForward, JoyAxis.LeftY, -1)) yield return t;
                    foreach (var t in this.HandleStick(MoveBack, JoyAxis.LeftY, 1)) yield return t;

                    foreach (var t in this.HandleButton(MoveSprint, JoyButton.LeftStickIn)) yield return t;
                    foreach (var t in this.HandleButton(MoveWalk, JoyButton.RightStickIn)) yield return t;

                    foreach (var t in this.HandleButton(Bark, JoyButton.FaceUp)) yield return t;
                    foreach (var t in this.HandleButton(Kiss, JoyButton.FaceRight)) yield return t;
                    foreach (var t in this.HandleButton(MoveJump, JoyButton.FaceDown)) yield return t;
                    foreach (var t in this.HandleButton(Interact, JoyButton.FaceLeft)) yield return t;

                    foreach (var t in this.HandleButton(PrimaryAction, JoyButton.TriggerRight)) yield return t;

                    foreach (var t in this.HandleButton(BaitMenu, JoyButton.DPadUp)) yield return t;
                    foreach (var t in this.HandleButton(EmoteWheel, JoyButton.DPadRight)) yield return t;
                    foreach (var t in this.HandleButton(Freecam, JoyButton.DPadDown)) yield return t;
                    foreach (var t in this.HandleButton(Build, JoyButton.DPadLeft)) yield return t;

                    foreach (var t in this.HandleButton(MenuOpen, JoyButton.Select)) yield return t;
                    foreach (var t in this.HandleButton(EscMenu, JoyButton.Start)) yield return t;

                    yield return new Token(TokenType.Newline, 1);
                } else {
                    yield return token;
                }
            }
        }

        private IEnumerable<Token> HandleStick(string map, JoyAxis axis, float axisValue) {
            foreach (var t in this.AddAction(map)) yield return t;
            foreach (var t in this.CreateStickEvent(map, axis, axisValue)) yield return t;
            foreach (var t in this.AddActionEvent(map, map)) yield return t;
        }

        private IEnumerable<Token> HandleButton(string map, JoyButton button) {
            foreach (var t in this.AddAction(map)) yield return t;
            foreach (var t in this.CreateButtonEvent(map, button)) yield return t;
            foreach (var t in this.AddActionEvent(map, map)) yield return t;
        }

        private IEnumerable<Token> AddAction(string map) {
            // InputMap.add_action(map)
            yield return new IdentifierToken("InputMap");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("add_action");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new ConstantToken(new StringVariant(map));
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 1);
        }

        private IEnumerable<Token> CreateStickEvent(string varName, JoyAxis axis, float axisValue) {
            // var varName = InputEventJoypadMotion.new()
            yield return new Token(TokenType.PrVar);
            yield return new IdentifierToken(varName);
            yield return new Token(TokenType.OpAssign);
            yield return new IdentifierToken("InputEventJoypadMotion");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("new");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 1);

            // varName.axis = axis
            yield return new IdentifierToken(varName);
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("axis");
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new IntVariant((int)axis));
            yield return new Token(TokenType.Newline, 1);

            // varName.axis_value = axisValue
            yield return new IdentifierToken(varName);
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("axis_value");
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new RealVariant(axisValue));
            yield return new Token(TokenType.Newline, 1);

            // varName.device = -1
            yield return new IdentifierToken(varName);
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("device");
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new IntVariant(-1));
            yield return new Token(TokenType.Newline, 1);
        }

        private IEnumerable<Token> CreateButtonEvent(string varName, JoyButton button) {
            // var varName = InputEventJoypadButton.new()
            yield return new Token(TokenType.PrVar);
            yield return new IdentifierToken(varName);
            yield return new Token(TokenType.OpAssign);
            yield return new IdentifierToken("InputEventJoypadButton");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("new");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 1);

            // varName.button_index = button
            yield return new IdentifierToken(varName);
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("button_index");
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new IntVariant((int)button));
            yield return new Token(TokenType.Newline, 1);

            // varName.device = -1
            yield return new IdentifierToken(varName);
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("device");
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new IntVariant(-1));
            yield return new Token(TokenType.Newline, 1);
        }

        private IEnumerable<Token> AddActionEvent(string actionName, string varName) {
            // InputMap.action_add_event(actionName, varName)
            yield return new IdentifierToken("InputMap");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("action_add_event");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new ConstantToken(new StringVariant(actionName));
            yield return new Token(TokenType.Comma);
            yield return new IdentifierToken(varName);
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 1);
        }
    }

    public class PlayerModifier : ScriptMod {
        public override bool ShouldRun(string path) => path == "res://Scenes/Entities/Player/player.gdc";

        public override IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
            var inputWaiter = new TokenWaiter(
                t => t.Type is TokenType.Newline && t.AssociatedData is 1,
                waitForReady: true
            );
            foreach (var token in tokens) {
                if (token is IdentifierToken {Name: "_get_input"}) inputWaiter.SetReady();

                if (inputWaiter.Check(token)) {
                    yield return new Token(TokenType.Newline, 1);

                    const string inputVar = "gdweave_look_vec";
                    const string mouseSensVar = "mouse_sens";

                    // var inputVar = Input.get_vector(LookLeft, LookRight, LookUp, LookDown)
                    yield return new Token(TokenType.PrVar);
                    yield return new IdentifierToken(inputVar);
                    yield return new Token(TokenType.OpAssign);
                    yield return new IdentifierToken("Input");
                    yield return new Token(TokenType.Period);
                    yield return new IdentifierToken("get_vector");
                    yield return new Token(TokenType.ParenthesisOpen);
                    yield return new ConstantToken(new StringVariant(LookLeft));
                    yield return new Token(TokenType.Comma);
                    yield return new ConstantToken(new StringVariant(LookRight));
                    yield return new Token(TokenType.Comma);
                    yield return new ConstantToken(new StringVariant(LookUp));
                    yield return new Token(TokenType.Comma);
                    yield return new ConstantToken(new StringVariant(LookDown));
                    yield return new Token(TokenType.ParenthesisClose);
                    yield return new Token(TokenType.Newline, 1);

                    // var mouseSensVar = OptionsMenu.mouse_sens * 40
                    yield return new Token(TokenType.PrVar);
                    yield return new IdentifierToken(mouseSensVar);
                    yield return new Token(TokenType.OpAssign);
                    yield return new IdentifierToken("OptionsMenu");
                    yield return new Token(TokenType.Period);
                    yield return new IdentifierToken("mouse_sens");
                    yield return new Token(TokenType.OpMul, 1);
                    yield return new ConstantToken(new RealVariant(40f)); // https://xkcd.com/221/
                    yield return new Token(TokenType.Newline, 1);

                    Token[] camBaseRotation = [
                        new IdentifierToken("cam_base"),
                        new Token(TokenType.Period),
                        new IdentifierToken("rotation_degrees"),
                        new Token(TokenType.Period),
                        new IdentifierToken("y")
                    ];
                    Token[] camPivotRotation = [
                        new IdentifierToken("cam_pivot"),
                        new Token(TokenType.Period),
                        new IdentifierToken("rotation_degrees"),
                        new Token(TokenType.Period),
                        new IdentifierToken("x")
                    ];

                    /*yield return new Token(TokenType.BuiltInFunc, (uint?) BuiltinFunction.TextPrint);
                    yield return new Token(TokenType.ParenthesisOpen);
                    yield return new IdentifierToken(inputVar);
                    yield return new Token(TokenType.ParenthesisClose);
                    yield return new Token(TokenType.Newline, 1);*/

                    // cam_base.rotation_degrees.y -= inputVar.x * mouseSensVar
                    foreach (var t in camBaseRotation) yield return t;
                    yield return new Token(TokenType.OpAssignSub);
                    yield return new IdentifierToken(inputVar);
                    yield return new Token(TokenType.Period);
                    yield return new IdentifierToken("x");
                    yield return new Token(TokenType.OpMul);
                    yield return new IdentifierToken(mouseSensVar);
                    yield return new Token(TokenType.Newline, 1);

                    // cam_pivot.rotation_degrees.x += inputVar.y * mouseSensVar
                    foreach (var t in camPivotRotation) yield return t;
                    yield return new Token(TokenType.OpAssignAdd);
                    yield return new IdentifierToken(inputVar);
                    yield return new Token(TokenType.Period);
                    yield return new IdentifierToken("y");
                    yield return new Token(TokenType.OpMul);
                    yield return new IdentifierToken(mouseSensVar);
                    yield return new Token(TokenType.Newline, 1);

                    // cam_pivot.rotation_degrees.x = clamp(cam_pivot.rotation_degrees.x, - 80, 80)
                    foreach (var t in camPivotRotation) yield return t;
                    yield return new Token(TokenType.OpAssign);
                    yield return new Token(TokenType.BuiltInFunc, (uint?) BuiltinFunction.LogicClamp);
                    yield return new Token(TokenType.ParenthesisOpen);
                    foreach (var t in camPivotRotation) yield return t;
                    yield return new Token(TokenType.Comma);
                    yield return new ConstantToken(new IntVariant(-80));
                    yield return new Token(TokenType.Comma);
                    yield return new ConstantToken(new IntVariant(80));
                    yield return new Token(TokenType.ParenthesisClose);
                    yield return new Token(TokenType.Newline, 1);
                } else {
                    yield return token;
                }
            }
        }
    }
}
