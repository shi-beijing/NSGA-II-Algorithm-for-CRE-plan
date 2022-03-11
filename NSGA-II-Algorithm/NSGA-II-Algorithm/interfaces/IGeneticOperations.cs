using System;
using System.Collections.Generic;
using NSGA_II_Algorithm.models;

namespace NSGA_II_Algorithm.interfaces
{
    public interface IGeneticOperations
    {
        Tuple<TrainsPlan, TrainsPlan> Crossover(TrainsPlan parent1, TrainsPlan parent2);
        TrainsPlan Mutation(TrainsPlan opePlan);
        TrainsPlan TournamentSelection(List<TrainsPlan> opePlans);
        List<Tuple<TrainsPlan, TrainsPlan>> Selection(List<TrainsPlan> opePlans);
    }
}