# Neuro Unity SDK Usage

There is an example of a Tic Tac Toe game implemented with the Neuro API, which you can find [here](./Assets/Examples).

## Sending Contexts

For sending context messages, you can use the `static void Context.Send(string message, bool silent)` method.

## Creating Custom Actions

In order to create a custom action, you can extend either the `NeuroAction` or `NeuroAction<T>` class. The difference is explained below.

You will need to implement the `Name`, `Description` and `Schema` of the action that you are creating, as well as a `Validate` and `Execute` method.

The `Validate` method should validate the incoming data from json, and make sure that it's correct, and it should also perform any kind of initial verifications and finding objects. For example, in Inscryption, checking that the card that Neuro is trying to play is valid, and that there is enough bones for it, as well as finding the actual Card object and saving that as state. At the end you should return either `ExecutionResult.Success()` or `ExecutionResult.Failure(string message)`. 

In order to pass state or context between the `Validate` and `Execute` methods, you can use the `parsedData` out parameter. This will be passed to the `Execute` method when it is called. The type of the `parsedData` parameter is the type parameter of the `NeuroAction<T>` class. If you have no context to pass, you can use the class without the generic type parameter. If your context is a value-type such as a struct or a primitive type like `int` or `bool`, you should use `NeuroActionS<T>` instead.

The `Execute` method should fully perform what Neuro requested. By this point, the action result has already been sent, so you need to try your best to execute it. If it's not possible anymore, you need to fail silently.

### Code Sample

```cs
// We extend NeuroAction<Button> because the state we pass between validation and execution is a Button object
public class JudgeAction : NeuroAction<Button>
{
    private readonly JudgeGame _judgeGame;

    public JudgeAction(JudgeGame judgeGame)
    {
        _judgeGame = judgeGame;
    }

    public override string Name => "judge";
    protected override string Description => "Decide if the defendant is innocent or guilty.";

    protected override JsonSchema Schema => QJS.WrapObject(new Dictionary<string, JsonSchema>
    {
        ["verdict"] = QJS.Enum(new string[] { "innocent", "guilty" })
    });

    protected override ExecutionResult Validate(ActionJData actionData, out Button? button)
    {
        // Please watch out for nullability - actionData.Data can be null if Neuro sends it like that
        string? verdict = actionData.Data?["verdict"]?.Value<string>();

        switch (verdict)
        {
            case "innocent":
                button = _judgeGame.InnocentButton;
                return ExecutionResult.Success();

            case "guilty":
                button = _judgeGame.GuiltyButton;
                return ExecutionResult.Success();

            case null:
                button = null;
                return ExecutionResult.Failure("Action failed. Missing required parameter 'verdict'.");

            default:
                button = null;
                return ExecutionResult.Failure("Action failed. Invalid parameter 'verdict'.");
        }
    }

    protected override void Execute(Button? button)
    {
        Button.Press();
    }
}
```

## Registered Actions

For registering semi-permanent actions, you can use the method `static void NeuroActionHandler.RegisterActions(INeuroAction[] actions)`.

Afterwards, if you want to unregister them, you can call `static void NeuroActionHandler.UnregisterActions(INeuroAction[] actions)`. Removal is done by checking the name of the action, so even if you pass in a different instance of the same action, it will still be removed. REMEMBER, the name is a unique identifier.

There's also `static void NeuroActionHandler.UnregisterActions(string[] actionNames)` which makes more sense I guess, the above function is just for convenience.

> [!Caution]  
> The Unity SDK currently handles overriding actions with the same name differently than the Neuro API.  
> The Neuro API will ignore any attempts at registering an action with the same name as an already registered action, even if the schema or description is different.  
> The Neuro Unity SDK will always override the existing action with the new one.  
> This needs to be fixed eventually.

### Code Sample

```cs
public void OnSceneChanged(string sceneName)
{
    if (sceneName == "game")
        NeuroActionHandler.RegisterActions(new LookAtAction());
    else
        NeuroActionHandler.UnregisterActions("look_at");
}

// where LookAtAction is defined as
public class LookAtAction: NeuroAction
{
    public LookAtAction()
    {
    }

    // ...
}
```

## Action Windows

For using ephemeral actions, such as in a turn-based game during the player's turn, you can use the `ActionWindow` class.

Create an instance using `static ActionWindow ActionWindow.Create(GameObject parent)`. Be careful with what parent you choose, because if that object is destroyed, the window will be automatically ended.

After you have finished setting up your action window, you can call `void ActionWindow.Register()` to register it with Neuro. This will make it immutable and register all of the actions over the websocket.

The methods of the `ActionWindow` class are designed to be used with method chaining.
`Register()` terminates the chain, as calling any methods on the action window after that would result in an error.

An `ActionWindow` can be in one of 4 possible states:
- `Building`: This window has just been created and is currently being setup.
- `Registered`: This window has been registered and is now immutable and waiting for a response.
- `Forced`: An action force has been sent for this window.
- `Ended`: This window has successfuly received an action and is waiting to be destroyed.

> [!Important]  
> Since the API doesn't support multiple `actions/force` messages at the same time, you need to make sure you don't have multiple action windows in the `Forced` state at the same time.  
> Easiest way to prevent this is to only ever have one active action window at a time.

### Adding a Context

If you want to add a context message to an action window, you can use the method `ActionWindow ActionWindow.SetContext(string message, bool silent)`. This will send the context message when the window is registered. Use this to pass in state relevant to the available actions.

### Forcing

If you want the action window to be forced (i.e. Neuro must perform an action as soon as she can), you can use one of the methods `ActionWindow ActionWindow.SetForce(...)`. This allows you to pass in various parameters that dictate when the action force for the window should be sent.

### Ending

If you want the action window to end programmatically, you can call one of the `ActionWindow ActionWindow.SetEnd(...)` methods to describe when that should happen.

When an action window is ended, all of the actions that were registered with it are unregistered automatically.

An action window will be automatically ended when an action is received and is executed successfully.

### Adding Actions

Using the `ActionWindow ActionWindow.AddAction(INeuroAction action)` method, you can add an action to the window. This will add this action as one of the possible responses that Neuro can pick from.

Each action window can register any number of actions, but only one of them will be returned by Neuro. This just asks Neuro to pick one of them basically.

> [!Caution]
> The Unity SDK currently handles overriding actions with the same name differently than the Neuro API.  
> The Neuro API will ignore any attempts at registering an action with the same name as an already registered action, even if the schema or description is different.  
> The Neuro Unity SDK will always override the existing action with the new one.  
> This needs to be fixed eventually.

### Code Sample

This code is taken from the Tic Tac Toe example [here](./Assets/Examples/TicTacToe.cs).

```cs
public void PlayerPlayInCell(GameObject cell)
{
    // ...

    if (!CheckWin())
    {
        ActionWindow.Create(gameObject);
            // 0 seconds forces the action immediately
            .SetForce(0, "It is your turn. Please place an O.", "", false);
            .AddAction(new PlayOAction(this));
            .Register();
    }
    else
    {
        // ...
    }
}
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
Game ->> SDK: ActionWindow.SetForce(...)<br>ActionWindow.Register()
SDK ->> API: actions/register
opt If forced
    SDK ->> API: actions/force
end
API ->> SDK: action
activate API
SDK ->> Game: NeuroAction.Validate(...)
alt Success
    Game ->> SDK: return ExecutionResult.Success(...)
    SDK ->> API: actions/unregister
    SDK ->> API: action/result
    deactivate API
    SDK ->> Game: NeuroAction.Execute(...)
else Failure
    activate API
    Game ->> SDK: return ExecutionResult.Failure(...)
    SDK ->> API: action/result
    deactivate API
    opt If forced
        Note over API: The API is responsible for retrying<br>failed action forces, but this is a<br>problem and will likely be changed<br>in the future. See issue #35;14.
        API ->> SDK: action
        activate API
        SDK ->> Game: NeuroAction.Validate
        Note over API, Game: Repeats as necessary
        deactivate API
    end
end
```
