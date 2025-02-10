# Neuro Godot SDK Usage

There is an example of a Tic Tac Toe game implemented with the Neuro API, which you can find [here](./examples/).

## Sending Contexts

For sending context messages, you can use the `Context.send(message: String, silent: bool)` function. 

## Creating Custom Actions

In order to create a custom action, you need to extend the `NeuroAction` class.

You will need to implement the `_get_name`, `_get_description` and `_get_schema` functions, as well as the `_validate_action` and `_execute_action` functions.

If your action is not going to be used in an `ActionWindow`, you can pass `null` to that parameter in the base constructor. Otherwise, you should take in an `ActionWindow` parameter in your own constructor, and pass that down.

The `_validate_action` method should validate the incoming data from json, and make sure that it's correct, and it should also perform any kind of initial verifications and finding objects. For example, in Inscryption, checking that the card that Neuro is trying to play is valid, and that there is enough bones for it, as well as finding the actual Card object and saving that as state. At the end you should return either `ExecutionResult.success()` or `ExecutionResult.failure(message: string)`. 

In order to pass state or context between the `_validate_action` and `_execute_action` functions, you can use the `state` dictionary parameter.

The `_execute_action` function should fully perform what Neuro requested. By this point, the action result has already been sent, so you need to try your best to execute it. If it's not possible anymore, you need to fail silently.

### Code Sample

```py
class_name JudgeAction
extends NeuroAction

var _judgeGame: JudgeGame

# This action will always be part of an action window, so we pass that as a parameter
func _init(window: ActionWindow, judgeGame: JudgeGame) -> void:
    super(window)
    _judgeGame = judgeGame

func _get_name() -> String:
    return "judge"

func _get_description() -> String:
    return "Decide if the defendant is innocent or guilty."

func _get_schema() -> Dictionary:
    return JsonUtils.wrap_schema({
        "verdict": {
            "enum": ["innocent", "guilty"]
        }
    })

func _validate_action(data: IncomingData, state: Dictionary) -> ExecutionResult:
    var verdict := data.get_string("verdict") # If Neuro makes this parameter null, this will return an empty string instead.
    match verdict:
        "innocent":
            state["func"] = _judgeGame.say_innocent
            return ExecutionResult.success()
        "guilty":
            state["func"] = _judgeGame.say_guilty
            return ExecutionResult.success()
        "":
            return ExecutionResult.failure("Action failed. Missing required parameter 'verdict'.")
        _:
            return ExecutionResult.failure("Action failed. Invalid parameter 'verdict'.")

func _execute_action(state: Dictionary) -> void:
    var func := state["func"]
    func()
```

## Registered Actions

For registering semi-permanent actions, you can use the function `NeuroActionHandler.register_actions(actions: Array[NeuroAction])`.

Afterwards, if you want to unregister them, you can call `NeuroActionHandler.unregister_actions(actions: Array[NeuroAction])`. Removal is done by checking the name of the action, so even if you pass in a different instance of the same action, it will still be removed. REMEMBER, the name is a unique identifier.

> [!Caution]  
> The Godot SDK currently handles overriding actions with the same name differently than the Neuro API.  
> The Neuro API will ignore any attempts at registering an action with the same name as an already registered action, even if the schema or description is different.  
> The Neuro Godot SDK will always override the existing action with the new one.  
> This needs to be fixed eventually.

### Code Sample

```py
func _init() -> void:
    NeuroActionHandler.register_actions([LookAtAction.new()])

func _exit_tree() -> void:
    NeuroActionHandler.unregister_actions([LookAtAction.new()])

# --------------------------------
# where LookAtAction._init is defined as
func _init():
    # The action is never part of an action window, so we pass down null
    super(null)
    # ...
```

## Action Windows

For using ephemeral actions, such as in a turn-based game during the player's turn, you can use the `ActionWindow` class.

Create an instance using `ActionWindow.new(parent: Node)`. Be careful with what parent you choose, because if that node is destroyed, the window will be automatically ended.

After you have finished setting up your action window, you can call `ActionWindow.register()` to register it with Neuro. This will make it immutable and register all of the actions over the websocket.

An `ActionWindow` can be in one of 4 possible states:
- `Building`: This window has just been created and is currently being setup.
- `Registered`: This window has been registered and is now immutable and waiting for a response.
- `Forced`: An action force has been sent for this window.
- `Ended`: This window has successfuly received an action and is waiting to be destroyed.

> [!Important]  
> Since the API doesn't support multiple `actions/force` messages at the same time, you need to make sure you don't have multiple action windows in the `Forced` state at the same time.  
> Easiest way to prevent this is to only ever have one active action window at a time.

### Adding a Context

If you want to add a context message to an action window, you can use the function `ActionWindow.set_context(message: String, silent: bool)`. This will send the context message when the window is registered. Use this to pass in state relevant to the available actions.

### Forcing

If you want the action window to be forced (i.e. Neuro must perform an action as soon as she can), you can use the function `ActionWindow.set_force(...)`. This allows you to pass in various parameters that dictate when the action force for the window should be sent.

### Ending

If you want the action window to end programmatically, you can call `ActionWindow.set_end(...)` to describe when that should happen.

When an action window is ended, all of the actions that were registered with it are unregistered automatically.

An action window will be automatically ended when an action is received and is executed successfully.

### Adding Actions

Using the `ActionWindow.add_action(action: NeuroAction)` method, you can add an action to the window. This will add this action as one of the possible responses that Neuro can pick from.

Each action window can register any number of actions, but only one of them will be returned by Neuro. This just asks Neuro to pick one of them basically.

> [!Caution]
> The Godot SDK currently handles overriding actions with the same name differently than the Neuro API.  
> The Neuro API will ignore any attempts at registering an action with the same name as an already registered action, even if the schema or description is different.  
> The Neuro Godot SDK will always override the existing action with the new one.  
> This needs to be fixed eventually.

### Code Sample

This code is taken from the Tic Tac Toe example [here](./examples/tic_tac_toe.gd).

```py
func player_play_in_cell(cell: BaseButton) -> void:
    # ...

    if not check_win():
        var actionWindow := ActionWindow.new(self)
        # 0 seconds forces the action immediately
        actionWindow.set_force(0, "It is your turn. Please place an O.", "", false)
        actionWindow.add_action(PlayOAction.new(actionWindow, self))
        actionWindow.register()
    else:
        # ...
```

### Sequence Diagram

```mermaid
sequenceDiagram
box rgba(255,0,0,0.1) Server
	participant API as Neuro API
end
box rgba(0,255,0,0.1) Client
	participant SDK as Neuro SDK
    participant Game
end
Game ->> SDK: ActionWindow.set_force(...)<br>ActionWindow.register()
SDK ->> API: actions/register
opt If forced
    SDK ->> API: actions/force
end
API ->> SDK: action
activate API
SDK ->> Game: NeuroAction._validate_action(...)
alt Success
    Game ->> SDK: return ExecutionResult.success(...)
    SDK ->> API: actions/unregister
    SDK ->> API: action/result
    deactivate API
    SDK ->> Game: NeuroAction._execute_action(...)
else Failure
    activate API
    Game ->> SDK: return ExecutionResult.failure(...)
    SDK ->> API: action/result
    deactivate API
    opt If forced
        Note over API: The API is responsible for retrying<br>failed action forces, but this is a<br>problem and will likely be changed<br>in the future. See issue #35;14.
        API ->> SDK: action
        activate API
        SDK ->> Game: NeuroAction._validate_action(...)
        Note over API, Game: Repeats as necessary
        deactivate API
    end
end
```

## Signals

There a few signals built-in to the `Websocket` autoload you can use to know when Neuro connects or disconnects:

* `Websocket.connected`

* `Websocket.failed_to_connect(error: Error)`

* `Websocket.disconnected(code: Code)`


Keep in mind the `connected` signal is emitted as soon as the `Websocket` autoload can connect, which may be before non-autoload nodes are ready. If you need to check the connection after this point, you can simply check the `websocket_is_connected` bool.

Since the Websocket has an autoload class name, you can connect to it from any script:

```py
# Some Node
# ...
func _ready() -> void:
    if Websocket.websocket_is_connected:
        neuro_connected()

    Websocket.disconnected.connect(neuro_disconnected)
    
func neuro_connected() -> void:
    print("Neuro is connected! Yipee!")

	# Enable related features

func neuro_disconnected(code: int) -> void:
    push_warning("Neuro disconnected with code %d!" % code)

    # Disable related features

func neuro_failed_to_connect(err: Error) -> void:
    push_warning("Neuro failed to connect with error: %s!" % str(err))
```