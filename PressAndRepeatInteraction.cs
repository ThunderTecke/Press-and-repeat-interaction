using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEngine.InputSystem.Editor;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class PressAndRepeatInteraction : IInputInteraction
{
    // Parameters
    public float holdTime = 0.4f;   // Time to consider binding held
    public float repeatTime = 0.2f; // Time to repeat "performed" event when the binding is held
    public bool press = false;      // Option to send "performed" event when the key is pressed

    // Private attributes
    private InputInteractionContext ctx;    // Interaction context
    private float heldTime = 0f;            // Current time held
    private bool firstEventSend = false;    // Memory for the first "performed" event when binding is held
    private float nextEventTime = 0f;       // Time of the next "performed" event
    private bool doingCancel;               // Prevent Stack Overflows

    // Constructor
    static PressAndRepeatInteraction()
    {
        InputSystem.RegisterInteraction<PressAndRepeatInteraction>();
    }

    // Initialization, it calls the constructor on load
    [RuntimeInitializeOnLoadMethod]
    private static void Initialize() { }

    // Process of the interaction
    public void Process(ref InputInteractionContext context)
    {
        // Saving context to use in "OnUpdate" function
        ctx = context;

        // If binding is pressed, then start the process
        if (ctx.phase != InputActionPhase.Performed && ctx.phase != InputActionPhase.Started && ctx.ControlIsActuated(0.5f))
        {
            // Save the context
            ctx.Started();

            // If press option is select, send a "performed" event on the binding press
            if (press)
                ctx.PerformedAndStayStarted();

            //Adding "OnUpdate" fonction to an event call every update, to process the interaction
            InputSystem.onAfterUpdate -= OnUpdate;
            InputSystem.onAfterUpdate += OnUpdate;
        }
    }

    // Process of the interaction execute on each update
    private void OnUpdate()
    {
        // Get the actual value of the binding
        var isActuated = ctx.ControlIsActuated(0.5f);

        // Get the actual phase
        var phase = ctx.phase;

        heldTime += Time.deltaTime;

        // Check if the action is canceled outside of this script, or disable, or just released
        if (phase == InputActionPhase.Canceled || phase == InputActionPhase.Disabled || !ctx.action.actionMap.enabled || (!isActuated && (phase == InputActionPhase.Performed || phase == InputActionPhase.Started)))
        {
            Cancel(ref ctx);
            return;
        }

        // Waiting the hold time to consider the bingind held
        if (heldTime < holdTime)
        {
            return;
        }

        // The hold time is reached, send one time "performed" event. And calculate the next event time
        if (!firstEventSend)
        {
            ctx.PerformedAndStayStarted();
            nextEventTime = heldTime + repeatTime;
            firstEventSend = true;
            return;
        }

        // The next event time is reached, send one more time "performed" event. And calculate the next event time
        if (heldTime >= nextEventTime)
        {
            ctx.PerformedAndStayStarted();
            nextEventTime = heldTime + repeatTime;
        }
    }

    // Reset function
    public void Reset()
    {
        Cancel(ref ctx);
    }

    // Cancelation of the interaction
    private void Cancel(ref InputInteractionContext context)
    {
        if (doingCancel) // Prevent stack overflows, context.Cancel() can result in Reset() eventually being called if multiple input sources trigger this action in the same frame
            return;
        
        // Remove "OnUpdate" from update event
        InputSystem.onAfterUpdate -= OnUpdate;

        // Reset held time, and the memory for the first hold event
        heldTime = 0f;
        firstEventSend = false;

        // Canceled the interaction if it's not already
        if (context.phase != InputActionPhase.Canceled)
        {
            doingCancel = true;
            context.Canceled();
            doingCancel = false;
        }
    }
}

#if UNITY_EDITOR

// This class is used by Unity to have public parameters in the interaction configuration in the action properties
internal class PressAndRepeatInteractionEditor : InputParameterEditor<PressAndRepeatInteraction>
{
    private static GUIContent holdTimeLabel, repeatTimeLabel, pressLabel;

    // Seting the label and description on each parameters
    protected override void OnEnable()
    {
        holdTimeLabel = new GUIContent("Hold Time", "The minimum amount of realtime seconds before the input is considered \"held\".");

        repeatTimeLabel = new GUIContent("Repeating time", "The time between each event while button is \"held\".");

        pressLabel = new GUIContent("Press", "\"Press\" send a \"Performed\" event when the key is pressed. Like normal behaviour");
    }

    // Draw parameters and get changed value
    public override void OnGUI()
    {
        // Press option
        EditorGUILayout.BeginHorizontal();
        target.press = EditorGUILayout.Toggle(pressLabel, target.press);
        EditorGUILayout.EndHorizontal();

        // Hold time
        EditorGUILayout.BeginHorizontal();
        target.holdTime = EditorGUILayout.FloatField(holdTimeLabel, target.holdTime);
        EditorGUILayout.EndHorizontal();

        // Repeat time
        EditorGUILayout.BeginHorizontal();
        target.repeatTime = EditorGUILayout.FloatField(repeatTimeLabel, target.repeatTime);
        EditorGUILayout.EndHorizontal();
    }
}

#endif
