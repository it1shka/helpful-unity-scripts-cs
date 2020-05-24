using System;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] layers;
    private float[][] neurons;
    private float[][][] weights;

    public float fitness;
    public NeuralNetwork(int[] layers)
    {
        this.layers = new int[layers.Length];
        for (var i = 0; i < layers.Length; i++)
            this.layers[i] = layers[i];
        InitNeurons();
        InitWeights();
    }

    public NeuralNetwork(NeuralNetwork originalNN)
    {
        this.layers = new int[originalNN.layers.Length];
        for (var i = 0; i < this.layers.Length; i++)
            this.layers[i] = originalNN.layers[i];
        InitNeurons();
        InitWeights();
        CopyWeights(originalNN.weights);
    }

    private void CopyWeights(float[][][] originalWeights)
    {
        for (var i = 0; i < weights.Length; i++)
        {
            for (var j = 0; j < weights[i].Length; j++)
            {
                for (var k = 0; k < weights[i][j].Length; k++)
                {
                    this.weights[i][j][k] = originalWeights[i][j][k];
                }
            }
        }
    }

    private void InitNeurons()
    {
        var neuronsList = new List<float[]>();
        foreach (var i in layers)
        {
            neuronsList.Add(new float[i]);
        }
        neurons = neuronsList.ToArray();
    }

    private void InitWeights()
    {
        var weightsList = new List<float[][]>();
        for (var layerNumber = 1; layerNumber < layers.Length; layerNumber++)
        {
            var layerWeightsList = new List<float[]>();
            var prevLayerCount = layers[layerNumber - 1];
            var curLayerCount = layers[layerNumber];
            for (var neuron = 0; neuron < curLayerCount; neuron++)
            {
                var curWeights = new float[prevLayerCount];
                for (var curWeight = 0; curWeight < prevLayerCount; curWeight++)
                {
                    curWeights[curWeight] = UnityEngine.Random.Range(-.5f, .5f);
                }
                layerWeightsList.Add(curWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }

        weights = weightsList.ToArray();
    }

    public float[] FeedForward(float[] input)
    {
        for (var i = 0; i < input.Length; i++)
        {
            neurons[0][i] = input[i];
        }

        for (var layer = 1; layer < layers.Length; layer++)
        {
            for (var neuron = 0; neuron < neurons[layer].Length; neuron++)
            {
                var value = 0f;
                for (var prevNeuron = 0; prevNeuron < neurons[layer - 1].Length; prevNeuron++)
                {
                    value += weights[layer - 1][neuron][prevNeuron] * neurons[layer - 1][prevNeuron];
                }
                neurons[layer][neuron] = (float)Math.Tanh(value);
            }
        }

        return neurons[neurons.Length - 1];
    }

    public void Mutate()
    {
        for (var i = 0; i < weights.Length; i++)
        {
            for (var j = 0; j < weights[i].Length; j++)
            {
                for (var k = 0; k < weights[i][j].Length; k++)
                {
                    var cur = weights[i][j][k];

                    var rnd = UnityEngine.Random.Range(0f, 100f);

                    if (rnd <= 2f)
                        cur *= -1f;
                    else if (rnd <= 4f)
                        cur = UnityEngine.Random.Range(-.5f, .5f);
                    else if (rnd <= 6f)
                        cur *= UnityEngine.Random.Range(0f, 1f);
                    else if (rnd <= 8f)
                        cur *= UnityEngine.Random.Range(0f, 1f) + 1f;

                    weights[i][j][k] = cur;
                }
            }
        }
    }

    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;
        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else return 0;
    }
}