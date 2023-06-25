using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    private static System.Random randomizer = new System.Random();
    public InputField trainName;
    public static EvolutionManager Instance { get; private set; }
    public bool started = false;

    public int PopulationSize = 30;
    public int RestartAfter = 100;
    [SerializeField]
    private bool ElitistSelection = false;

    [SerializeField]
    private uint[] FNNTopology;
    private List<Agent> agents = new List<Agent>();
    public int AgentsAliveCount { get; private set; }
    public event System.Action AllAgentsDied;

    private GeneticAlgorithm geneticAlgorithm;
    public uint GenerationCount { get { return geneticAlgorithm.GenerationCount; } }

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one EvolutionManager in the Scene.");
            return;
        }
        Instance = this;
    }
    public void StartEvolution()
    {
        started =true;
        NeuralNetwork nn = new NeuralNetwork(FNNTopology);
        geneticAlgorithm = new GeneticAlgorithm((uint)nn.WeightCount, (uint)PopulationSize);
       
        geneticAlgorithm.Evaluation = StartEvaluation;

        if (ElitistSelection)
        {
            geneticAlgorithm.Selection = GeneticAlgorithm.DefaultSelectionOperator;
            geneticAlgorithm.Recombination = RandomRecombination;
            geneticAlgorithm.Mutation = MutateAllButBestTwo;
        }
        else
        {
            geneticAlgorithm.Selection = RemainderStochasticSampling;
            geneticAlgorithm.Recombination = RandomRecombination;
            geneticAlgorithm.Mutation = MutateAllButBestTwo;
        }

        AllAgentsDied += geneticAlgorithm.EvaluationFinished;
        if (RestartAfter > 0)
        {
            geneticAlgorithm.TerminationCriterion += CheckGenerationTermination;
            geneticAlgorithm.AlgorithmTerminated += OnGATermination;
        }

        geneticAlgorithm.Start();
    }

    private bool CheckGenerationTermination(IEnumerable<Genotype> currentPopulation)
    {
        return geneticAlgorithm.GenerationCount >= RestartAfter;
    }
    private void OnGATermination(GeneticAlgorithm ga)
    {
        AllAgentsDied -= ga.EvaluationFinished;
    }
    private void StartEvaluation(IEnumerable<Genotype> currentPopulation)
    {
        agents.Clear();
        AgentsAliveCount = 0;

        foreach (Genotype genotype in currentPopulation)
            agents.Add(new Agent(genotype, MathHelper.SoftSignFunction, FNNTopology));

        TrackManager.Instance.SetCarAmount(agents.Count);
        IEnumerator<CarController> carsEnum = TrackManager.Instance.GetCarEnumerator();
        for (int i = 0; i < agents.Count; i++)
        {
            if (!carsEnum.MoveNext())
            {
                Debug.LogError("Cars enum ended before agents.");
                break;
            }

            carsEnum.Current.Agent = agents[i];
            AgentsAliveCount++;
            agents[i].AgentDied += OnAgentDied;
        }

        TrackManager.Instance.Restart();
    }
    private void OnAgentDied(Agent agent)
    {
        AgentsAliveCount--;
        
        if (AgentsAliveCount == 0 && AllAgentsDied != null)
        {
            AllAgentsDied();
        }
            
    }
    private List<Genotype> RemainderStochasticSampling(List<Genotype> currentPopulation)
    {
        List<Genotype> intermediatePopulation = new List<Genotype>();
        foreach (Genotype genotype in currentPopulation)
        {
            if (genotype.Fitness < 1)
                break;
            else
            {
                for (int i = 0; i < (int)genotype.Fitness; i++)
                    intermediatePopulation.Add(new Genotype(genotype.GetParameterCopy()));
            }
        }
        foreach (Genotype genotype in currentPopulation)
        {
            float remainder = genotype.Fitness - (int)genotype.Fitness;
            if (randomizer.NextDouble() < remainder)
                intermediatePopulation.Add(new Genotype(genotype.GetParameterCopy()));
        }

        return intermediatePopulation;
    }
    private List<Genotype> RandomRecombination(List<Genotype> intermediatePopulation, uint newPopulationSize)
    {
        if (intermediatePopulation.Count < 2)
            throw new System.ArgumentException("The intermediate population has to be at least of size 2 for this operator.");

        List<Genotype> newPopulation = new List<Genotype>();
        newPopulation.Add(intermediatePopulation[0]);
        newPopulation.Add(intermediatePopulation[1]);


        while (newPopulation.Count < newPopulationSize)
        {
            int randomIndex1 = randomizer.Next(0, intermediatePopulation.Count), randomIndex2;
            do
            {
                randomIndex2 = randomizer.Next(0, intermediatePopulation.Count);
            } while (randomIndex2 == randomIndex1);

            Genotype offspring1, offspring2;
            GeneticAlgorithm.CompleteCrossover(intermediatePopulation[randomIndex1], intermediatePopulation[randomIndex2],
                GeneticAlgorithm.DefCrossSwapProb, out offspring1, out offspring2);

            newPopulation.Add(offspring1);
            if (newPopulation.Count < newPopulationSize)
                newPopulation.Add(offspring2);
        }

        return newPopulation;
    }

    private void MutateAllButBestTwo(List<Genotype> newPopulation)
    {
        for (int i = 2; i < newPopulation.Count; i++)
        {
            if (randomizer.NextDouble() < GeneticAlgorithm.DefMutationPerc)
                GeneticAlgorithm.MutateGenotype(newPopulation[i], GeneticAlgorithm.DefMutationProb, GeneticAlgorithm.DefMutationAmount);
        }
    }
    private void MutateAll(List<Genotype> newPopulation)
    {
        foreach (Genotype genotype in newPopulation)
        {
            if (randomizer.NextDouble() < GeneticAlgorithm.DefMutationPerc)
                GeneticAlgorithm.MutateGenotype(genotype, GeneticAlgorithm.DefMutationProb, GeneticAlgorithm.DefMutationAmount);
        }
    }

}
