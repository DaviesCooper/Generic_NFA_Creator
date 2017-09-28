using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace tileRead.Datastructures.StateMachine
{
    /// <summary>
    /// interface for generating states.
    /// Because states can be general things (i.e. locations or colour values etc.) we need this to be generic
    /// </summary>
    public interface State : IEquatable<State>
    {
        //Used so we can identify states
        int GetHashCode();
        //Useful for debugging
        string ToString();
    }

    public class Transition<T> : IEquatable<Transition<T>> where T :  new()
    {
        public T transitionValue;
        private HashSet<State> nextStates;

        /// <summary>
        /// checks if the transition value is the same
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Transition<T> other)
        {
            if (other == null)
                return false;
            //check if they are literally the same object
            if (ReferenceEquals(this, other))
                return true;
            if (other.transitionValue.Equals(transitionValue))
                return true;
            return false;
        }

        public Transition() : this(new T()){}
        public Transition(T value)
        {
            transitionValue = value;
            nextStates = new HashSet<State>();
        }
        public Transition(T value, State next)
        {
            transitionValue = value;
            nextStates = new HashSet<State>();
            nextStates.Add(next);
        }

        public void addTransition(State next)
        {
            nextStates.Add(next);
        }

        public List<State> getStatesAsList()
        {
            return nextStates.ToList();
        }
        public State[] getStatesAsArray()
        {
            return nextStates.ToArray();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class TransitionState<T> : IEquatable<TransitionState<T>> where T : new()
    {

        //The state we are "coming" from
        public State currentState;
        //The transition values from this state
        public HashSet<Transition<T>> transitions;


        public TransitionState(State cur)
        {
            currentState = cur;
            transitions = new HashSet<Transition<T>>();
        }

        /// <summary>
        /// public constructor generates an empty state list for the next possible states
        /// </summary>
        /// <param name="cur"></param>
        /// <param name="tVal"></param>
        public TransitionState(State cur, T tVal)
        {
            currentState = cur;
            transitions = new HashSet<Transition<T>>();
            transitions.Add(new Transition<T>(tVal));
        }

        public TransitionState(State cur, T tVal, State fin)
        {
            currentState = cur;
            transitions = new HashSet<Transition<T>>();
            transitions.Add(new Transition<T>(tVal, fin));
        }

        /// <summary>
        /// Add a new transition to this current state transition.
        /// This is essentially what makes this an NFA right here.
        /// </summary>
        /// <param name="newTrans"></param>
        public void addTransition(T transitionVal, State nextState)
        {
            if(transitions.Contains(new Transition<T>(transitionVal)))
            {
                transitions.First(x => x.transitionValue.Equals(transitionVal)).addTransition(nextState);
            }
            else
            {
                transitions.Add(new Transition<T>(transitionVal, nextState));
            }
        }

        /// <summary>
        /// Call to get the list of states that this state transition is capable of producing
        /// </summary>
        /// <returns></returns>
        public State[] getNextStates()
        {
            if(transitions == null)
            {
                return new State[0];
            }
            HashSet<State> retVal = new HashSet<State>();
            foreach(Transition<T> trans in transitions)
            {
                foreach(State s in trans.getStatesAsList())
                {
                    retVal.Add(s);
                }
            }
            return retVal.ToArray();
        }


        /// <summary>
        /// Returns all possible T values that can be passed in to cause this state to transition.
        /// </summary>
        /// <returns></returns>
        public T[] getPossibleTransitionValue()
        {
            if(transitions == null)
            {
                return new T[0];
            }
            T[] retVal = new T[transitions.Count];
            int count = 0;
            foreach(Transition<T> trans in transitions)
            {
                retVal[count] = trans.transitionValue;
                count++;
            }
            return retVal;
        }

        /// <summary>
        /// Returns all states that a possible transition might generate (because this is an nfa)
        /// </summary>
        /// <returns></returns>
        public State[] getNextStatesForTransition(T transitionValue)
        {
            if (transitions == null)
            {
                return new State[0];
            }
            HashSet<State> retVal = new HashSet<State>();    
            //filtering for transition value
            foreach (Transition<T> trans in transitions.Where(x => x.transitionValue.Equals(transitionValue)))
            {
                foreach (State s in trans.getStatesAsList())
                {
                    retVal.Add(s);
                }
            }
            return retVal.ToArray();
        }

        /// <summary>
        /// If the state this transition starts at is that same as the state the passed in transition starts from
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(TransitionState<T> other)
        {
            if (other == null)
                return false;
            //check if they are literally the same object
            if (ReferenceEquals(this, other))
                return true;
            if (other.currentState.Equals(currentState))
                return true;
            return false;
        }

        /// <summary>
        /// A very functional programming way of string joining
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}