using System;
using System.Collections.Generic;
using System.Linq;
using tileRead.Datastructures.StateMachine;
using tileReadTest.Datastructures;

namespace tileRead.Datastructures
{
    public class NFAException : Exception
    {
        public NFAException()
            : base() { }

        public NFAException(string message)
            : base(message) { }

        public NFAException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public NFAException(string message, Exception innerException)
            : base(message, innerException) { }

        public NFAException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }


    /// <summary>
    /// An NFA machine.
    /// (Q, Sigma, Delta, q0, F)
    /// (set of all states, input symbols, transition functions, start state, final state(singular because our data only HAS one final state)
    /// </summary>
    /// <typeparam name="T">Sigma</typeparam>
    public class StateMachine<T> where T : new()
    {
        #region Delegate Definitions
        /// <summary>
        /// The Delta of the NFA machine
        /// Delegate for creating the transition table.
        /// Given a set of entries, defines how to generate states for the entries as well as the state transitions and the relationship between the two
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public delegate HashSet<TransitionState<T>> generateTransitionTableFromEntries(Object[] entries);

        /// <summary>
        /// Delegate for creating States from entries
        /// used for generating (q ∈ Q)
        /// </summary>
        /// <param name="toConvert"></param>
        /// <returns></returns>
        public delegate State generateStateFromEntry(Object toConvert);

        /// <summary>
        /// Delegate for deciding how to choose between multiple posible states correspondiong to a state transitions state change
        /// </summary>
        /// <param name="possibleOuts">the possible states to transition to</param>
        /// <returns></returns>
        public delegate State tieBreak(State[] possibleOuts);
        #endregion

        //The only public var as its the only one we will be referencing for our visualizations
        public State currentState;
        private State previousState;
        //q0
        private State startState;
        //F
        private State finalState;
        public HashSet<TransitionState<T>> transitionTable;


        /// <summary>
        /// Creates the machine.
        /// Works in a very functional programming way in that it has two higher order functions being used as arguments
        /// </summary>
        /// <param name="entries">the array of all entries the machine is to be generated from</param>
        /// <param name="tableGenerator">a delegate methid</param>
        /// <param name="stateGenerator"></param>
        public StateMachine(Entry[] entries, ref generateTransitionTableFromEntries tableGenerator, ref generateStateFromEntry stateGenerator)
        {
            transitionTable = tableGenerator(entries);
            startState = stateGenerator(entries[0]);
            finalState = stateGenerator(entries[entries.Length - 1]);
        }

        /// <summary>
        /// Places our current state as the start state
        /// </summary>
        public void StartMachine()
        {
            currentState = startState;
        }

        /// <summary>
        /// Get the transitionstate structure which contains all the transition structures possible for this state
        /// </summary>
        /// <returns></returns>
        private TransitionState<T> getTransitionForCurrentState()
        {
            if (currentState == null)
            {
                throw new NFAException("There does not exist a current state currently");
            }
            //returns all elements in the hashset which have the same current state as us
            return transitionTable.FirstOrDefault(x => x.currentState == currentState);
        }

        /// <summary>
        /// returns all possible transition values that cause this state to transition to another.
        /// If a value does not cause this state to change, it will not be returned
        /// </summary>
        /// <returns></returns>
        private T[] getPossibleTransitionValuesForCurrentState()
        {
            try
            {
                TransitionState<T> trans = getTransitionForCurrentState();
                return trans.getPossibleTransitionValue();
            }
            catch (Exception)
            {
                throw new NFAException("It is not possible to transition out of our current state");
            }
        }

        /// <summary>
        /// gets the group of next states for the current state given a transition value
        /// </summary>
        /// <param name="transitionValue"></param>
        /// <returns></returns>
        private State[] GetNextStates(T transitionValue)
        {
            TransitionState<T> transitions = getTransitionForCurrentState();
            State[] states = transitions.getNextStatesForTransition(transitionValue);
            if (states.Length < 1)
                throw new NFAException("There did not exist a transition of " + transitionValue.ToString() + " for the current state");
            else
                return states;
        }

        /// <summary>
        /// Takes the machine from its current state to the next state based on this input.
        /// Because this is an NFA, there is a delegate here for how the user would like 
        /// to choose between multiple states for the next state should multiple states
        /// be an option for the given input.
        /// Returns true if a final state has been reached
        /// </summary>
        /// <param name="input"></param>
        public bool Transition(T input, ref tieBreak tieBreakers)
        {
            //set our previous state to be this one
            previousState = currentState;
            //and use our delegate to decide how we want to choose our next state
            currentState = tieBreakers(GetNextStates(input));
            return checkIfFinalState();
        }

        /// <summary>
        /// Used for determining if we have reached the final state
        /// </summary>
        /// <returns></returns>
        private bool checkIfFinalState()
        {
            return finalState.Equals(currentState);
        }
    }
}
