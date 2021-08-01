using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public abstract class BaseState : MonoBehaviour
{
#if UNITY_EDITOR
    [HideInInspector]
    public bool editorShow;
#endif
    public virtual bool ShouldSwitch { get; protected set; }
    protected bool IsActive { get; private set; }
    public AIBrain Brain { get; private set; }
    public Unit Unit => Brain.Unit;
    public void Initialize(AIBrain brain)
    {
        Brain = brain;
    }
    public virtual void OnEnter()
    {
        ShouldSwitch = false;
        IsActive = true;
    }

    public virtual void OnExit()
    {
        IsActive = false;
    }

    public virtual void HandleUpdate()
    {

    }

    protected virtual void OnDrawGizmos()
    {
    }
}

public class AIBrain : MonoBehaviour
{
#if UNITY_EDITOR
    [HideInInspector]
    public bool editorShowStates;
    public int EditorCurrentStateIndex => currentStateIndex;
#endif

    [HideInInspector]
    public List<BaseState> states;

    [SerializeField]
    private Unit unit;
    [SerializeField]
    private Transform target;

    public Unit Unit => unit;
    public Transform Target
    {
        get => target;
        set => target = value;
    }

    private int currentStateIndex;
    private BaseState CurrentState => states[currentStateIndex];
    [HideInInspector]
    public UnityEvent onCurrentStateChanged = new UnityEvent();
    public int CurrentStateIndex { get => currentStateIndex; set => currentStateIndex = value; }

    private void Start()
    {
        foreach (var state in states)
        {
            state.Initialize(this);
        }
        if (states.Count > 0)
        {
            CurrentState.OnEnter();
        }
    }

    private void Update()
    {
        if (states.Count <= 0)
        {
            return;
        }
        CurrentState.HandleUpdate();
        if (CurrentState.ShouldSwitch)
        {
            AdvanceState();
        }
    }

    private void AdvanceState()
    {
        CurrentState.OnExit();
        currentStateIndex++;
        if (currentStateIndex >= states.Count)
        {
            currentStateIndex = 0;
        }
        onCurrentStateChanged.Invoke();
        CurrentState.OnEnter();
    }
}
