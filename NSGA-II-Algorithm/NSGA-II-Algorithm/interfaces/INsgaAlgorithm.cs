using System.Collections.Generic;
using NSGA_II_Algorithm.models;

namespace NSGA_II_Algorithm.interfaces
{
    public interface INsgaAlgorithm
    {
        List<TrainsPlan> Process(int nrGenerations, int populationSize, bool debug = false);
        List<List<TrainsPlan>> SortByFronts(List<TrainsPlan> chromosomes);
    }
}
