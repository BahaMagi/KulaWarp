using System;
using UnityEngine;
using System.Collections.Generic;

public class StateMachine
{
    public State      currentState { get; private set; }
    public State      defaultState { get; private set; }
    public GameObject owner;

    private List<State> states; // @TODO check if this list is really needed...

    public StateMachine(GameObject o)
    {
        owner        = o;
        currentState = null;
        states       = new List<State>();
        defaultState = null;
    }

    public void ChangeState(State newState)
    {
        if (currentState != null)
            currentState.OnExitState(newState);

        State tmp    = currentState;
        currentState = newState;
        currentState.OnEnterState(tmp);
    }

    public void Update()
    {
        if (currentState != null)
            currentState.UpdateState();
    }

    public void AddState(State s)
    {
        states.Add(s);
    }

    public void Reset()
    {
        // Change State to s without(!) calling OnExitState of the state that was left. 
        State tmp    = currentState;
        //// // Testing with OnExitState. Can't remember why I took it out...
        tmp.OnExitState(defaultState);
        ////
        currentState = defaultState;
        currentState.OnEnterState(tmp);
    }

    public void SetDefaultState(State s)
    {
        defaultState = s;
    }
}

public abstract class State
{
    public StateMachine     sm;
    public List<Transition> transitions;
    public int              stateName;

    public abstract void OnEnterState(State from);
    public abstract void OnExitState(State to);
    public abstract void UpdateState();

    public State(StateMachine statemachine)
    {
        sm = statemachine;
        transitions = new List<Transition>();
    }
   
    public void AddTransition(Transition t)
    {
        transitions.Add(t);
    }

    public void AddTransition(State target, Func<bool> condition)
    {
        transitions.Add(new Transition(this, target, condition));
    }

    public void AddTransition(State target)
    {
        transitions.Add(new Transition(this, target));
    }

    public void CheckTransitions()
    {
        foreach (Transition t in transitions)
            if (t.condition())
                t.Trigger();
    }
}

public class Transition
{
    public State        from, to;
    public StateMachine sm;
    public Func<bool>   condition;

    // Transitio based on a condition
    public Transition(State oldState, State newState, Func<bool> cond)
    {
        sm        = oldState.sm;
        condition = cond;
        from      = oldState;
        to        = newState;
    }

    // Transition based on triggering
    public Transition(State oldState, State newState)
    {
        sm        = oldState.sm;
        condition = (() => false);
        from      = oldState;
        to        = newState;
    }

    public void Trigger()
    {
        sm.ChangeState(to);
    }
}