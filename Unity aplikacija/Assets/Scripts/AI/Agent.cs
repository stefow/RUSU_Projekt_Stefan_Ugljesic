using System;
using System.Collections.Generic;

/// Class that combines a genotype and a feedforward neural network (FNN).
[Serializable]
public class Agent : IComparable<Agent>
{
    /// The underlying genotype of this agent.
    public Genotype Genotype{get;private set;}
    /// The feedforward neural network which was constructed from the genotype of this agent.
    public NeuralNetwork FNN{get;private set;}

    private bool isAlive = false;
    /// Whether this agent is currently alive (actively participating in the simulation).
    public bool IsAlive
    {
        get { return isAlive; }
        private set
        {
            if (isAlive != value)
            {
                isAlive = value;

                if (!isAlive && AgentDied != null)
                    AgentDied(this);
            }
        }
    }
    /// Event for when the agent died (stopped participating in the simulation).
    public event Action<Agent> AgentDied;

    /// Initialises a new agent from given genotype, constructing a new feedfoward neural network from
    /// the parameters of the genotype.
    public Agent(Genotype genotype, NeuralLayer.ActivationFunction defaultActivation, params uint[] topology)
    {
        IsAlive = false;
        this.Genotype = genotype;
        FNN = new NeuralNetwork(topology);
        foreach (NeuralLayer layer in FNN.Layers)
            layer.NeuronActivationFunction = defaultActivation;

        //Check if topology is valid
        if (FNN.WeightCount != genotype.ParameterCount)
            throw new ArgumentException("The given genotype's parameter count must match the neural network topology's weight count.");

        //Construct FNN from genotype
        IEnumerator<float> parameters = genotype.GetEnumerator();
        foreach (NeuralLayer layer in FNN.Layers) //Loop over all layers
        {
            for (int i = 0; i < layer.Weights.GetLength(0); i++) //Loop over all nodes of current layer
            {
                for (int j = 0; j < layer.Weights.GetLength(1); j++) //Loop over all nodes of next layer
                {
                    layer.Weights[i,j] = parameters.Current;
                    parameters.MoveNext();
                }
            }
        }
    }
    
    /// Resets this agent to be alive again.
    public void Reset()
    {
        Genotype.Evaluation = 0;
        Genotype.Fitness = 0;
        IsAlive = true;
    }

    /// Kills this agent (sets IsAlive to false).
    public void Kill()
    {
        IsAlive = false;
    }

    /// Compares this agent to another agent, by comparing their underlying genotypes.
    public int CompareTo(Agent other)
    {
        return this.Genotype.CompareTo(other.Genotype);
    }
}

