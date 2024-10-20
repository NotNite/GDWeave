using GDWeave.Godot;
using GDWeave.Godot.Variants;
using GDWeave.Modding;

namespace WebfishingPlus.Mods;

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

    private const string CreateControllerAction = "gdweave_create_controller_action";

    // Custom
    private const string LookLeft = "gdweave_look_left";
    private const string LookRight = "gdweave_look_right";
    private const string LookUp = "gdweave_look_up";
    private const string LookDown = "gdweave_look_down";

    private const string ZoomControl = "gdweave_zoom_control";

    private const string TabNext = "gdweave_tab_next";
    private const string TabPrevious = "gdweave_tab_previous";

    private const string ToggleSprint = "gdweave_toggle_sprint";
    private const string ToggleWalk = "gdweave_toggle_walk";

    // Movement
    private const string MoveLeft = "move_left";
    private const string MoveRight = "move_right";
    private const string MoveForward = "move_forward";
    private const string MoveBack = "move_back";
    private const string MoveJump = "move_jump";

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

    public class InputRegister : IScriptMod {
        public bool ShouldRun(string path) => path == "res://Scenes/Singletons/globals.gdc";

        public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
            var extendsWaiter = new MultiTokenWaiter([
                t => t.Type is TokenType.PrExtends,
                t => t.Type is TokenType.Newline,
            ], allowPartialMatch: true);
            var readyWaiter = new TokenWaiter(
                t => t.Type is TokenType.Newline && t.AssociatedData is 1,
                waitForReady: true
            );

            foreach (var token in tokens) {
                if (token is IdentifierToken {Name: "_ready"}) readyWaiter.SetReady();

                if (extendsWaiter.Check(token)) {
                    yield return token;
                    foreach (var t in this.CreateControllerActionFunc()) yield return t;
                } else if (readyWaiter.Check(token)) {
                    yield return token;

                    foreach (var t in Register(LookLeft, (int) JoyAxis.RightX, -1)) yield return t;
                    foreach (var t in Register(LookRight, (int) JoyAxis.RightX, 1)) yield return t;
                    foreach (var t in Register(LookUp, (int) JoyAxis.RightY, 1)) yield return t;
                    foreach (var t in Register(LookDown, (int) JoyAxis.RightY, -1)) yield return t;

                    foreach (var t in Register(Bark, (int) JoyButton.FaceUp)) yield return t;
                    foreach (var t in Register(Kiss, (int) JoyButton.FaceRight)) yield return t;

                    foreach (var t in Register(PrimaryAction, (int) JoyButton.TriggerRight)) yield return t;

                    foreach (var t in Register(Freecam, (int) JoyButton.DPadDown)) yield return t;

                    foreach (var t in Register(EscMenu, (int) JoyButton.Start)) yield return t;

                    foreach (var t in Register(TabNext, (int) JoyButton.ShoulderRight)) yield return t;
                    foreach (var t in Register(TabPrevious, (int) JoyButton.ShoulderLeft)) yield return t;
                    foreach (var t in Register(ZoomControl, (int) JoyButton.TriggerLeft)) yield return t;

                    foreach (var t in Register(ToggleSprint, (int) JoyButton.LeftStickIn)) yield return t;
                    foreach (var t in Register(ToggleWalk, (int) JoyButton.RightStickIn)) yield return t;

                    foreach (var t in RegisterRebindable()) yield return t;

                    yield return token;
                } else {
                    yield return token;
                }
            }
        }

        private IEnumerable<Token> CreateControllerActionFunc() {
            // func gdweave_create_controller_action(map, code, axisValue = 0):
            yield return new Token(TokenType.PrFunction);
            yield return new IdentifierToken(CreateControllerAction);
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new IdentifierToken("map");
            yield return new Token(TokenType.Comma);
            yield return new IdentifierToken("code");
            yield return new Token(TokenType.Comma);
            yield return new IdentifierToken("axisValue");
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new RealVariant(0));
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 1);

            // var stickEvent = InputEventJoypadMotion.new()
            yield return new Token(TokenType.PrVar);
            yield return new IdentifierToken("stickEvent");
            yield return new Token(TokenType.OpAssign);
            yield return new IdentifierToken("InputEventJoypadMotion");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("new");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 1);

            // var buttonEvent = InputEventJoypadButton.new()
            yield return new Token(TokenType.PrVar);
            yield return new IdentifierToken("buttonEvent");
            yield return new Token(TokenType.OpAssign);
            yield return new IdentifierToken("InputEventJoypadButton");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("new");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 1);

            // stickEvent.device = -1
            yield return new IdentifierToken("stickEvent");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("device");
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new IntVariant(-1));
            yield return new Token(TokenType.Newline, 1);

            // buttonEvent.device = -1
            yield return new IdentifierToken("buttonEvent");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("device");
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new IntVariant(-1));
            yield return new Token(TokenType.Newline, 1);

            // if not InputMap.has_action("map"): InputMap.add_action("map")
            yield return new Token(TokenType.CfIf);
            yield return new Token(TokenType.OpNot);
            yield return new IdentifierToken("InputMap");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("has_action");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new IdentifierToken("map");
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Colon);
            yield return new IdentifierToken("InputMap");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("add_action");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new IdentifierToken("map");
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 1);

            // if not axisValue == 0:
            yield return new Token(TokenType.CfIf);
            yield return new Token(TokenType.OpNot);
            yield return new IdentifierToken("axisValue");
            yield return new Token(TokenType.OpEqual);
            yield return new ConstantToken(new RealVariant(0));
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 2);

            // stickEvent.axis = code
            yield return new IdentifierToken("stickEvent");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("axis");
            yield return new Token(TokenType.OpAssign);
            yield return new IdentifierToken("code");
            yield return new Token(TokenType.Newline, 2);

            // stickEvent.axis_value = axisValue
            yield return new IdentifierToken("stickEvent");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("axis_value");
            yield return new Token(TokenType.OpAssign);
            yield return new IdentifierToken("axisValue");
            yield return new Token(TokenType.Newline, 1);

            // else: buttonEvent.button_index = button
            yield return new Token(TokenType.CfElse);
            yield return new Token(TokenType.Colon);
            yield return new IdentifierToken("buttonEvent");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("button_index");
            yield return new Token(TokenType.OpAssign);
            yield return new IdentifierToken("code");
            yield return new Token(TokenType.Newline, 1);

            // InputMap.action_add_event(map, stickEvent if not axisValue == 0 else buttonEvent)
            yield return new IdentifierToken("InputMap");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("action_add_event");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new IdentifierToken("map");
            yield return new Token(TokenType.Comma);
            yield return new IdentifierToken("stickEvent");
            yield return new Token(TokenType.CfIf);
            yield return new Token(TokenType.OpNot);
            yield return new IdentifierToken("axisValue");
            yield return new Token(TokenType.OpEqual);
            yield return new ConstantToken(new RealVariant(0));
            yield return new Token(TokenType.CfElse);
            yield return new IdentifierToken("buttonEvent");
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline);
        }
    }

    public class PlayerModifier : IScriptMod {
        public bool ShouldRun(string path) => path == "res://Scenes/Entities/Player/player.gdc";

        private const string UsingController = "gdweave_using_controller";
        private const string WishSprint = "gdweave_wish_sprint";
        private const string WishWalk = "gdweave_wish_walk";
        private const string CurrentHotbar = "gdweave_current_hotbar";

        public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
            var extendsWaiter = new MultiTokenWaiter([
                t => t.Type is TokenType.PrExtends,
                t => t.Type is TokenType.Newline,
            ], allowPartialMatch: true);
            var bind5Waiter = new TokenWaiter(
                t => t.Type is TokenType.Newline && t.AssociatedData is 1,
                waitForReady: true
            );
            var sprintingWaiter = new MultiTokenWaiter([
                t => t is IdentifierToken {Name: "slow_walking"},
                t => t is ConstantToken {Value: StringVariant {Value: "move_walk"}},
                t => t.Type is TokenType.Newline && t.AssociatedData is 1,
            ], allowPartialMatch: true);
            var inputWaiter = new TokenWaiter(
                t => t.Type is TokenType.Newline && t.AssociatedData is 1,
                waitForReady: true
            );
            var processMovementWaiter = new TokenWaiter(
                t => t.Type is TokenType.Newline && t.AssociatedData is 1,
                waitForReady: true
            );
            var rodCastDistWaiter = new MultiTokenWaiter([
                t => t is IdentifierToken {Name: "rod_cast_dist"},
                t => t.Type is TokenType.OpAssignSub,
                t => t is ConstantToken {Value: RealVariant {Value: 0.04}},
                t => t.Type is TokenType.CfIf,
                t => t is IdentifierToken {Name: "Input"},
                t => t.Type is TokenType.Period,
                t => t is IdentifierToken {Name: "is_action_pressed"},
                t => t.Type is TokenType.ParenthesisOpen,
                t => t is ConstantToken {Value: StringVariant {Value: "move_sprint"}},
                t => t.Type is TokenType.ParenthesisClose,
            ]);

            foreach (var token in tokens) {
                if (token is ConstantToken {Value: StringVariant {Value: "bind_5"}}) bind5Waiter.SetReady();
                if (token is IdentifierToken {Name: "_input"}) inputWaiter.SetReady();
                if (token is IdentifierToken {Name: "_process_movement"}) processMovementWaiter.SetReady();

                if (extendsWaiter.Check(token)) {
                    yield return token;
                    foreach (var t in this.PatchSetupPlayerVars()) yield return t;
                } else if (bind5Waiter.Check(token)) {
                    yield return token;
                    foreach (var t in this.PatchLook()) yield return t;
                    foreach (var t in this.PatchZoomControl()) yield return t;
                    foreach (var t in this.PatchHotbarControl()) yield return t;
                } else if (sprintingWaiter.Check(token)) {
                    yield return token;
                    foreach (var t in this.PatchAnalogMovement()) yield return t;
                    foreach (var t in this.PatchToggleMovement()) yield return t;
                } else if (inputWaiter.Check(token)) {
                    yield return token;
                    foreach (var t in this.PatchCheckController()) yield return t;
                } else if (processMovementWaiter.Check(token)) {
                    yield return token;
                    if (Mod.Config.ControllerVibration)
                        foreach (var t in this.PatchLandVibration())
                            yield return t;
                } else if (rodCastDistWaiter.Check(token)) {
                    yield return token;
                    foreach (var t in this.PatchSprintReel()) yield return t;
                } else {
                    yield return token;
                }
            }
        }

        private IEnumerable<Token> PatchSetupPlayerVars() {
            // var UsingController = false
            yield return new Token(TokenType.PrVar);
            yield return new IdentifierToken(UsingController);
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new BoolVariant(false));
            yield return new Token(TokenType.Newline);

            // var CurrentHotbar = -1
            yield return new Token(TokenType.PrVar);
            yield return new IdentifierToken(CurrentHotbar);
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new IntVariant(-1));
            yield return new Token(TokenType.Newline);

            // var WishSprint = false
            yield return new Token(TokenType.PrVar);
            yield return new IdentifierToken(WishSprint);
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new BoolVariant(false));
            yield return new Token(TokenType.Newline);

            // var WishWalk = false
            yield return new Token(TokenType.PrVar);
            yield return new IdentifierToken(WishWalk);
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new BoolVariant(false));
            yield return new Token(TokenType.Newline);
        }

        private IEnumerable<Token> PatchLook() {
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

            // if UsingController:
            yield return new Token(TokenType.CfIf);
            yield return new IdentifierToken(UsingController);
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 2);

            // cam_base.rotation_degrees.y -= inputVar.x * mouseSensVar
            foreach (var t in camBaseRotation) yield return t;
            yield return new Token(TokenType.OpAssignSub);
            yield return new IdentifierToken(inputVar);
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("x");
            yield return new Token(TokenType.OpMul);
            yield return new IdentifierToken(mouseSensVar);
            yield return new Token(TokenType.Newline, 2);

            // cam_pivot.rotation_degrees.x += inputVar.y * mouseSensVar
            foreach (var t in camPivotRotation) yield return t;
            yield return new Token(TokenType.OpAssignAdd);
            yield return new IdentifierToken(inputVar);
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("y");
            yield return new Token(TokenType.OpMul);
            yield return new IdentifierToken(mouseSensVar);
            yield return new Token(TokenType.Newline, 2);

            // cam_pivot.rotation_degrees.x = clamp(cam_pivot.rotation_degrees.x, -80, 80)
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
        }

        private IEnumerable<Token> PatchZoomControl() {
            const string cameraZoom = "camera_zoom";
            const double zoomAmount = 0.2;

            // if UsingController and Input.is_action_pressed(ZoomControl):
            yield return new Token(TokenType.CfIf);
            yield return new IdentifierToken(UsingController);
            yield return new Token(TokenType.OpAnd);
            foreach (var t in this.IsActionPressedShort(ZoomControl)) yield return t;
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 2);

            // if Input.is_action_pressed(TabNext) and not Input.is_action_pressed(TabPrevious): CameraZoom += 0.3
            yield return new Token(TokenType.CfIf);
            foreach (var t in this.IsActionPressedShort(TabNext)) yield return t;
            yield return new Token(TokenType.OpAnd);
            yield return new Token(TokenType.OpNot);
            foreach (var t in this.IsActionPressedShort(TabPrevious)) yield return t;
            yield return new Token(TokenType.Colon);
            yield return new IdentifierToken(cameraZoom);
            yield return new Token(TokenType.OpAssignAdd);
            yield return new ConstantToken(new RealVariant(zoomAmount));
            yield return new Token(TokenType.Newline, 2);

            // if Input.is_action_pressed(TabPrevious) and not Input.is_action_pressed(TabNext): CameraZoom -= 0.3
            yield return new Token(TokenType.CfIf);
            foreach (var t in this.IsActionPressedShort(TabPrevious)) yield return t;
            yield return new Token(TokenType.OpAnd);
            yield return new Token(TokenType.OpNot);
            foreach (var t in this.IsActionPressedShort(TabNext)) yield return t;
            yield return new Token(TokenType.Colon);
            yield return new IdentifierToken(cameraZoom);
            yield return new Token(TokenType.OpAssignSub);
            yield return new ConstantToken(new RealVariant(zoomAmount));
            yield return new Token(TokenType.Newline, 1);
        }

        private IEnumerable<Token> PatchHotbarControl() {
            const string equipHotbar = "_equip_hotbar";
            const int hotbarMin = 0;
            const int hotbarMax = 4;

            // if UsingController and not Input.is_action_pressed(ZoomControl) and not locked and state == STATES.DEFAULT:
            yield return new Token(TokenType.CfIf);
            yield return new IdentifierToken(UsingController);
            yield return new Token(TokenType.OpAnd);
            yield return new Token(TokenType.OpNot);
            foreach (var t in this.IsActionPressedShort(ZoomControl)) yield return t;
            yield return new Token(TokenType.OpAnd);
            yield return new Token(TokenType.OpNot);
            yield return new IdentifierToken("locked");
            yield return new Token(TokenType.OpAnd);
            yield return new IdentifierToken("state");
            yield return new Token(TokenType.OpEqual);
            yield return new IdentifierToken("STATES");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("DEFAULT");
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 2);

            // if Input.is_action_just_pressed(TabNext):
            yield return new Token(TokenType.CfIf);
            foreach (var t in this.IsActionJustPressedShort(TabNext)) yield return t;
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 3);

            // CurrentHotbar += 1
            yield return new IdentifierToken(CurrentHotbar);
            yield return new Token(TokenType.OpAssignAdd);
            yield return new ConstantToken(new IntVariant(1));
            yield return new Token(TokenType.Newline, 3);

            // if CurrentHotbar > hotbarMax: CurrentHotbar = hotbarMin
            yield return new Token(TokenType.CfIf);
            yield return new IdentifierToken(CurrentHotbar);
            yield return new Token(TokenType.OpGreater);
            yield return new ConstantToken(new IntVariant(hotbarMax));
            yield return new Token(TokenType.Colon);
            yield return new IdentifierToken(CurrentHotbar);
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new IntVariant(hotbarMin));
            yield return new Token(TokenType.Newline, 3);

            // equipHotbar(CurrentHotbar)
            yield return new IdentifierToken(equipHotbar);
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new IdentifierToken(CurrentHotbar);
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 2);

            // elif Input.is_action_just_pressed(TabPrevious):
            yield return new Token(TokenType.CfElif);
            foreach (var t in this.IsActionJustPressedShort(TabPrevious)) yield return t;
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 3);

            // CurrentHotbar -= 1
            yield return new IdentifierToken(CurrentHotbar);
            yield return new Token(TokenType.OpAssignSub);
            yield return new ConstantToken(new IntVariant(1));
            yield return new Token(TokenType.Newline, 3);

            // if CurrentHotbar < hotbarMin: CurrentHotbar = hotbarMax
            yield return new Token(TokenType.CfIf);
            yield return new IdentifierToken(CurrentHotbar);
            yield return new Token(TokenType.OpLess);
            yield return new ConstantToken(new IntVariant(hotbarMin));
            yield return new Token(TokenType.Colon);
            yield return new IdentifierToken(CurrentHotbar);
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new IntVariant(hotbarMax));
            yield return new Token(TokenType.Newline, 3);

            // equipHotbar(CurrentHotbar)
            yield return new IdentifierToken(equipHotbar);
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new IdentifierToken(CurrentHotbar);
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 1);
        }

        private IEnumerable<Token> PatchAnalogMovement() {
            Token[] camBaseTransformBasis = [
                new IdentifierToken("cam_base"),
                new(TokenType.Period),
                new IdentifierToken("transform"),
                new(TokenType.Period),
                new IdentifierToken("basis")
            ];

            const string stickInput = "gdweave_stick_input";
            const string direction = "direction";

            // var stickInput = Input.get_vector(MoveRight, MoveLeft, MoveBack, MoveForward)
            yield return new Token(TokenType.PrVar);
            yield return new IdentifierToken(stickInput);
            yield return new Token(TokenType.OpAssign);
            yield return new IdentifierToken("Input");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("get_vector");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new ConstantToken(new StringVariant(MoveRight));
            yield return new Token(TokenType.Comma);
            yield return new ConstantToken(new StringVariant(MoveLeft));
            yield return new Token(TokenType.Comma);
            yield return new ConstantToken(new StringVariant(MoveBack));
            yield return new Token(TokenType.Comma);
            yield return new ConstantToken(new StringVariant(MoveForward));
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 1);

            // if UsingController:
            yield return new Token(TokenType.CfIf);
            yield return new IdentifierToken(UsingController);
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 2);

            // direction = Vector3(stickInput.x, 0, stickInput.y)
            yield return new IdentifierToken(direction);
            yield return new Token(TokenType.OpAssign);
            yield return new Token(TokenType.BuiltInType, (uint?) VariantType.Vector3);
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new IdentifierToken(stickInput);
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("x");
            yield return new Token(TokenType.Comma);
            yield return new ConstantToken(new RealVariant(0));
            yield return new Token(TokenType.Comma);
            yield return new IdentifierToken(stickInput);
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("y");
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 2);

            // direction = -cam_base.transform.basis.xform(direction)
            yield return new IdentifierToken(direction);
            yield return new Token(TokenType.OpAssign);
            yield return new Token(TokenType.OpSub);
            foreach (var t in camBaseTransformBasis) yield return t;
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("xform");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new IdentifierToken(direction);
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.Newline, 1);
        }

        private IEnumerable<Token> PatchToggleMovement() {
            // if UsingController:
            yield return new Token(TokenType.CfIf);
            yield return new IdentifierToken(UsingController);
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 2);

            // if Input.is_action_just_pressed(ToggleSprint):
            yield return new Token(TokenType.CfIf);
            foreach (var t in this.IsActionJustPressedShort(ToggleSprint)) yield return t;
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 3);

            // WishWalk = false
            yield return new IdentifierToken(WishWalk);
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new BoolVariant(false));
            yield return new Token(TokenType.Newline, 3);

            // WishSprint = not WishSprint
            yield return new IdentifierToken(WishSprint);
            yield return new Token(TokenType.OpAssign);
            yield return new Token(TokenType.OpNot);
            yield return new IdentifierToken(WishSprint);
            yield return new Token(TokenType.Newline, 2);

            // elif Input.is_action_just_pressed(ToggleWalk):
            yield return new Token(TokenType.CfElif);
            foreach (var t in this.IsActionJustPressedShort(ToggleWalk)) yield return t;
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 3);

            // WishSprint = false
            yield return new IdentifierToken(WishSprint);
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new BoolVariant(false));
            yield return new Token(TokenType.Newline, 3);

            // WishWalk = !WishWalk
            yield return new IdentifierToken(WishWalk);
            yield return new Token(TokenType.OpAssign);
            yield return new Token(TokenType.OpNot);
            yield return new IdentifierToken(WishWalk);
            yield return new Token(TokenType.Newline, 2);

            // sprinting = WishSprint
            yield return new IdentifierToken("sprinting");
            yield return new Token(TokenType.OpAssign);
            yield return new IdentifierToken(WishSprint);
            yield return new Token(TokenType.Newline, 2);

            // slow_walking = WishWalk
            yield return new IdentifierToken("slow_walking");
            yield return new Token(TokenType.OpAssign);
            yield return new IdentifierToken(WishWalk);
            yield return new Token(TokenType.Newline, 1);
        }

        private IEnumerable<Token> PatchCheckController() {
            // if event is InputEventJoypadButton or event is InputEventJoypadMotion: UsingController = true
            yield return new Token(TokenType.CfIf);
            yield return new IdentifierToken("event");
            yield return new Token(TokenType.PrIs);
            yield return new IdentifierToken("InputEventJoypadButton");
            yield return new Token(TokenType.OpOr);
            yield return new IdentifierToken("event");
            yield return new Token(TokenType.PrIs);
            yield return new IdentifierToken("InputEventJoypadMotion");
            yield return new Token(TokenType.Colon);
            yield return new IdentifierToken(UsingController);
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new BoolVariant(true));
            yield return new Token(TokenType.Newline, 1);

            // Ignore mouse movement for the check, could get annoying on Steam Deck

            // elif not event is InputEventMouseMotion: UsingController = false
            yield return new Token(TokenType.CfElif);
            yield return new Token(TokenType.OpNot);
            yield return new IdentifierToken("event");
            yield return new Token(TokenType.PrIs);
            yield return new IdentifierToken("InputEventMouseMotion");
            yield return new Token(TokenType.Colon);
            yield return new IdentifierToken(UsingController);
            yield return new Token(TokenType.OpAssign);
            yield return new ConstantToken(new BoolVariant(false));
            yield return new Token(TokenType.Newline, 1);
        }

        private IEnumerable<Token> PatchLandVibration() {
            // if is_on_floor() and in_air:
            yield return new Token(TokenType.CfIf);
            yield return new IdentifierToken("is_on_floor");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new Token(TokenType.ParenthesisClose);
            yield return new Token(TokenType.OpAnd);
            yield return new IdentifierToken("in_air");
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 2);

            // if not diving: Input.start_joy_vibration(0, 0.5, 0, 0.15)
            yield return new Token(TokenType.CfIf);
            yield return new Token(TokenType.OpNot);
            yield return new IdentifierToken("diving");
            yield return new Token(TokenType.Colon);
            foreach (var t in VibrateController(0.5, 0, 0.15)) yield return t;
            yield return new Token(TokenType.Newline, 2);

            // else: Input.start_joy_vibration(0, 0, 0.5, 0.25)
            yield return new Token(TokenType.CfElse);
            yield return new Token(TokenType.Colon);
            foreach (var t in VibrateController(0, 0.5, 0.25)) yield return t;
            yield return new Token(TokenType.Newline, 1);
        }

        private IEnumerable<Token> PatchSprintReel() {
            // ... or Input.is_action_pressed(ZoomControl) ...
            yield return new Token(TokenType.OpOr);
            foreach (var t in this.IsActionPressedShort(ZoomControl)) yield return t;
        }

        private IEnumerable<Token> IsActionPressedShort(string action) {
            yield return new IdentifierToken("Input");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("is_action_pressed");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new ConstantToken(new StringVariant(action));
            yield return new Token(TokenType.ParenthesisClose);
        }

        private IEnumerable<Token> IsActionJustPressedShort(string action) {
            yield return new IdentifierToken("Input");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("is_action_just_pressed");
            yield return new Token(TokenType.ParenthesisOpen);
            yield return new ConstantToken(new StringVariant(action));
            yield return new Token(TokenType.ParenthesisClose);
        }
    }

    public class Fishing3Modifier : IScriptMod {
        public bool ShouldRun(string path) => path == "res://Scenes/Minigames/Fishing3/fishing3.gdc";

        public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
            var reelSoundWaiter = new MultiTokenWaiter([
                t => t.Type is TokenType.PrVar,
                t => t is IdentifierToken {Name: "reel_sound"},
                t => t.Type is TokenType.Newline && t.AssociatedData is 1,
            ], allowPartialMatch: true);
            var yankWaiter = new MultiTokenWaiter([
                t => t.Type is TokenType.OpAssignSub,
                t => t is IdentifierToken {Name: "params"},
                t => t.Type is TokenType.BracketOpen,
                t => t is ConstantToken {Value: StringVariant {Value: "damage"}},
                t => t.Type is TokenType.BracketClose,
                t => t.Type is TokenType.Newline && t.AssociatedData is 1,
            ]);

            foreach (var token in tokens) {
                if (reelSoundWaiter.Check(token)) {
                    yield return token;
                    //if (GDWeave.Config.ControllerVibration) foreach (var t in this.PatchReelVibration()) yield return t;
                } else if (yankWaiter.Check(token)) {
                    yield return token;
                    if (Mod.Config.ControllerVibration)
                        foreach (var t in this.PatchYankVibration())
                            yield return t;
                } else {
                    yield return token;
                }
            }
        }

        // TODO: Figure out a way to have both reeling and yanking vibration work together properly
        /*
        private IEnumerable<Token> PatchReelVibration() {
            // this sucks
            // if reeling and main_progress < end_goal and active and thrash_timer <= 0 and not at_yank: Input.start_joy_vibration(0, 0.2, 0, 0.1)
            yield return new Token(TokenType.CfIf);
            yield return new IdentifierToken("reeling");
            yield return new Token(TokenType.OpAnd);
            yield return new IdentifierToken("main_progress");
            yield return new Token(TokenType.OpLess);
            yield return new IdentifierToken("end_goal");
            yield return new Token(TokenType.OpAnd);
            yield return new IdentifierToken("active");
            yield return new Token(TokenType.OpAnd);
            yield return new IdentifierToken("thrash_timer");
            yield return new Token(TokenType.OpLessEqual);
            yield return new ConstantToken(new IntVariant(0));
            yield return new Token(TokenType.OpAnd);
            yield return new Token(TokenType.OpNot);
            yield return new IdentifierToken("at_yank");
            yield return new Token(TokenType.Colon);
            foreach (var t in VibrateController(0.2, 0, 0.1)) yield return t;
            yield return new Token(TokenType.Newline, 1);
        }
        */

        private IEnumerable<Token> PatchYankVibration() {
            // if ys.health <= 0:
            yield return new Token(TokenType.CfIf);
            yield return new IdentifierToken("ys");
            yield return new Token(TokenType.Period);
            yield return new IdentifierToken("health");
            yield return new Token(TokenType.OpLessEqual);
            yield return new ConstantToken(new IntVariant(0));
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 2);

            // Input.start_joy_vibration(0, 0, 0.75, 0.2)
            foreach (var t in VibrateController(0, 0.75, 0.2)) yield return t;
            yield return new Token(TokenType.Newline, 1);

            // else:
            yield return new Token(TokenType.CfElse);
            yield return new Token(TokenType.Colon);
            yield return new Token(TokenType.Newline, 2);

            // Input.start_joy_vibration(0, 0, 0.4, 0.2)
            foreach (var t in VibrateController(0, 0.4, 0.2)) yield return t;
            yield return new Token(TokenType.Newline, 1);
        }
    }

    public class InputRemapButtonModifier : IScriptMod {
        public bool ShouldRun(string path) => path == "res://Scenes/Singletons/OptionsMenu/input_remap_button.gdc";

        public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
            var actionAddEventWaiter = new TokenWaiter(
                t => t.Type is TokenType.Newline && t.AssociatedData is 1,
                waitForReady: true
            );

            foreach (var token in tokens) {
                if (token is IdentifierToken {Name: "action_add_event"}) actionAddEventWaiter.SetReady();

                if (actionAddEventWaiter.Check(token)) {
                    yield return token;
                    foreach (var t in RegisterRebindable(true)) yield return t;
                } else {
                    yield return token;
                }
            }
        }
    }

    private static IEnumerable<Token> BuiltInPrintShort(string txt) {
        yield return new Token(TokenType.BuiltInFunc, (uint?) BuiltinFunction.TextPrint);
        yield return new Token(TokenType.ParenthesisOpen);
        yield return new ConstantToken(new StringVariant(txt));
        yield return new Token(TokenType.ParenthesisClose);
    }

    private static IEnumerable<Token> Register(string map, int code, int axisValue = 0) {
        yield return new IdentifierToken(CreateControllerAction);
        yield return new Token(TokenType.ParenthesisOpen);
        yield return new ConstantToken(new StringVariant(map));
        yield return new Token(TokenType.Comma);
        yield return new ConstantToken(new IntVariant(code));
        if (axisValue != 0) {
            yield return new Token(TokenType.Comma);
            yield return new ConstantToken(new RealVariant(axisValue));
        }
        yield return new Token(TokenType.ParenthesisClose);
        yield return new Token(TokenType.Newline, 1);
    }

    private static IEnumerable<Token> RegisterRebindable(bool external = false) {
        if (external) {
            yield return new IdentifierToken("Globals");
            yield return new Token(TokenType.Period);
        }
        foreach (var t in Register(MoveLeft, (int) JoyAxis.LeftX, -1)) yield return t;
        if (external) {
            yield return new IdentifierToken("Globals");
            yield return new Token(TokenType.Period);
        }
        foreach (var t in Register(MoveRight, (int) JoyAxis.LeftX, 1)) yield return t;
        if (external) {
            yield return new IdentifierToken("Globals");
            yield return new Token(TokenType.Period);
        }
        foreach (var t in Register(MoveForward, (int) JoyAxis.LeftY, -1)) yield return t;
        if (external) {
            yield return new IdentifierToken("Globals");
            yield return new Token(TokenType.Period);
        }
        foreach (var t in Register(MoveBack, (int) JoyAxis.LeftY, 1)) yield return t;

        if (external) {
            yield return new IdentifierToken("Globals");
            yield return new Token(TokenType.Period);
        }
        foreach (var t in Register(MoveJump, (int) JoyButton.FaceDown)) yield return t;
        if (external) {
            yield return new IdentifierToken("Globals");
            yield return new Token(TokenType.Period);
        }
        foreach (var t in Register(Interact, (int) JoyButton.FaceLeft)) yield return t;

        if (external) {
            yield return new IdentifierToken("Globals");
            yield return new Token(TokenType.Period);
        }
        foreach (var t in Register(BaitMenu, (int) JoyButton.DPadUp)) yield return t;
        if (external) {
            yield return new IdentifierToken("Globals");
            yield return new Token(TokenType.Period);
        }
        foreach (var t in Register(EmoteWheel, (int) JoyButton.DPadRight)) yield return t;
        if (external) {
            yield return new IdentifierToken("Globals");
            yield return new Token(TokenType.Period);
        }
        foreach (var t in Register(Build, (int) JoyButton.DPadLeft)) yield return t;

        if (external) {
            yield return new IdentifierToken("Globals");
            yield return new Token(TokenType.Period);
        }
        foreach (var t in Register(MenuOpen, (int) JoyButton.Select)) yield return t;

        yield return new Token(TokenType.Newline, 1);
    }

    private static double CalcVibration(double toCalc) => toCalc * Mod.Config.ControllerVibrationStrength;

    private static IEnumerable<Token> VibrateController(double weak, double strong, double duration) {
        // Input.start_joy_vibration(-1, weak, strong, duration)
        yield return new IdentifierToken("Input");
        yield return new Token(TokenType.Period);
        yield return new IdentifierToken("start_joy_vibration");
        yield return new Token(TokenType.ParenthesisOpen);
        yield return new ConstantToken(new IntVariant(0));
        yield return new Token(TokenType.Comma);
        yield return new ConstantToken(new RealVariant(CalcVibration(weak)));
        yield return new Token(TokenType.Comma);
        yield return new ConstantToken(new RealVariant(CalcVibration(strong)));
        yield return new Token(TokenType.Comma);
        yield return new ConstantToken(new RealVariant(duration));
        yield return new Token(TokenType.ParenthesisClose);
    }
}
