using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class NeuralNetwork : ISerializable
{
    public NeuralLayer[] Layers { get; private set; }
    public uint[] Topology { get; private set; }
    public int WeightCount { get; private set; }
    public NeuralNetwork(params uint[] topology)
    {
        this.Topology = topology;

        WeightCount = 0;
        for (int i = 0; i < topology.Length - 1; i++)
            WeightCount += (int)((topology[i] + 1) * topology[i + 1]);

        Layers = new NeuralLayer[topology.Length - 1];
        for (int i = 0; i < Layers.Length; i++)
            Layers[i] = new NeuralLayer(topology[i], topology[i + 1]);
    }
    public double[] ProcessInputs(double[] inputs)
    {
        if (inputs.Length != Layers[0].NeuronCount)
            throw new ArgumentException("Given inputs do not match network input amount.");

        double[] outputs = inputs;
        foreach (NeuralLayer layer in Layers)
            outputs = layer.ProcessInputs(outputs);

        return outputs;
    }

    public void SetRandomWeights(double minValue, double maxValue)
    {
        if (Layers != null)
        {
            foreach (NeuralLayer layer in Layers)
                layer.SetRandomWeights(minValue, maxValue);
        }
    }

    public NeuralNetwork GetTopologyCopy()
    {
        NeuralNetwork copy = new NeuralNetwork(this.Topology);
        for (int i = 0; i < Layers.Length; i++)
            copy.Layers[i].NeuronActivationFunction = this.Layers[i].NeuronActivationFunction;

        return copy;
    }

    public NeuralNetwork DeepCopy()
    {
        NeuralNetwork newNet = new NeuralNetwork(this.Topology);
        for (int i = 0; i < this.Layers.Length; i++)
            newNet.Layers[i] = this.Layers[i].DeepCopy();

        return newNet;
    }

    public override string ToString()
    {
        string output = "";

        for (int i = 0; i < Layers.Length; i++)
            output += "Layer " + i + ":\n" + Layers[i].ToString();

        return output;
    }

    public void SaveModel(string filePath)
    {
        FileStream fileStream = new FileStream(filePath, FileMode.Create);

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, this);
        }
        finally
        {
            fileStream.Close();
        }
    }

    public static NeuralNetwork LoadModel(string filePath)
    {
        FileStream fileStream = new FileStream(filePath, FileMode.Open);
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            NeuralNetwork neuralNetwork = (NeuralNetwork)formatter.Deserialize(fileStream);

            return neuralNetwork;
        }
        finally
        {
            fileStream.Close();
        }
    }

    protected NeuralNetwork(SerializationInfo info, StreamingContext context)
    {
        Topology = (uint[])info.GetValue("Topology", typeof(uint[]));

        WeightCount = 0;
        for (int i = 0; i < Topology.Length - 1; i++)
            WeightCount += (int)((Topology[i] + 1) * Topology[i + 1]);

        Layers = new NeuralLayer[Topology.Length - 1];
        for (int i = 0; i < Layers.Length; i++)
            Layers[i] = (NeuralLayer)info.GetValue($"Layer{i}", typeof(NeuralLayer));
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Topology", Topology);
        for (int i = 0; i < Layers.Length; i++)
            info.AddValue($"Layer{i}", Layers[i]);
    }
}
