using System;
using System.Collections.Generic;
using System.Linq;
using NSGA_II_Algorithm.interfaces;
using NSGA_II_Algorithm.models;

namespace NSGA_II_Algorithm.implementations
{
    /// <summary>
    /// Class for genetic operations like:
    /// - selection
    /// - mutation
    /// - crossover
    /// - tournament selection    
    /// </summary>
    /// 

    class GeneticOperations : IGeneticOperations
    {
        // private readonly IReadOnlyList<Item> _items;




        private readonly double _crossoverProbability;
        private readonly double _mutationProbability;

        private readonly Random rnd = new Random(new System.DateTime().Millisecond);

        public GeneticOperations(double crossoverProbability, double mutationProbability)
        {
            _crossoverProbability = crossoverProbability;
            _mutationProbability = mutationProbability;
            // _items = items;
        }

        /// <summary>
        /// Select a slice index between 0..parents.genes.size and do the crossover between them
        /// </summary>
        /// <param name="parent1">Parent one chromosome</param>
        /// <param name="parent2">Parent one chromosome</param>
        /// <returns>New cross-over-ed chromosome</returns>
        public Tuple<TrainsPlan, TrainsPlan> Crossover(TrainsPlan parent1, TrainsPlan parent2)
        {
            Random R = new Random();
            //
            //one-point crossover
            int S_Point = R.Next(0,DataSet.nTrain);//crossover point
          

            List<int> s1 =new List<int> ();
            for (int i = 0; i < S_Point; i++)
            {
                s1.Add ( parent1.Chromosome[i]);
            }
            for (int i = S_Point; i < DataSet.nTrain; i++)
            {
                s1.Add(  parent2.Chromosome[i]);
            }
            var child1 = new TrainsPlan(RepairChro(s1));


            List<int> s2 = new List<int>();
            for (int i = 0; i < S_Point; i++)
            {
                s2.Add(parent2.Chromosome[i]);
            }
            for (int i = S_Point; i < DataSet.nTrain; i++)
            {
                s2.Add(parent1.Chromosome[i]);
            }
            var child2 = new TrainsPlan(RepairChro(s2));

            return Tuple.Create(child1, child2);
        }

        /// <summary>
        /// Iterate through each gene of the chromosome and mutates it.
        /// </summary>
        /// <param name="item">Chromosome to be mutated</param>
        /// <returns>A mutated chromosome with the specific probability for each gene</returns>
        public TrainsPlan Mutation(TrainsPlan opePlan)
        {
            Random R = new Random(Guid.NewGuid().GetHashCode());
            var s = opePlan.Chromosome;

            for (int i = 0; i < s.Count; i++)
            {
                if (opePlan.trainsPlan[i].BoolRun != 1)//There is a high probability that the train will be mutated when the train is not running. If the chromosomal position remains unchanged, the excellent gene will be retained. / / this mutation step is rarely performed in the later stage, because most individuals run in full number
                {
                    int id = R.Next(0, DataSet.AlternatePath[DataSet.TrainPort[i]].Count);
                    int temp = DataSet.AlternatePath[DataSet.TrainPort[i]][id];
                    s[i] = temp;
                }
                double a = R.NextDouble();

                //  if (a < _mutationProbability)
                if (a < opePlan.MutationProb)
                {
                    int id = R.Next(0, DataSet.AlternatePath[DataSet.TrainPort[i]].Count);
                    int temp = DataSet.AlternatePath[DataSet.TrainPort[i]][id];
                    s[i] = temp;
                }


            }
            return new TrainsPlan(RepairChro(s));
        }

        /// <summary>
        /// This function Select a chromosome using the tournament method
        /// </summary>
        /// <param name="chromosomes">List of chromosomes</param>
        /// <returns>A chromosome selected using tournament method</returns>
        public TrainsPlan TournamentSelection(List<TrainsPlan> opePlans)
        {
            var idx1 = rnd.Next(0, opePlans.Count);
            var idx2 = rnd.Next(0, opePlans.Count);

            var dominate = opePlans[idx1].Dominates(opePlans[idx2]);
            switch (dominate)
            {
                case 1://idx1 dominate idx2
                    return opePlans[idx1];
                case -1://idx2 dominate idx1
                    return opePlans[idx2];
                default:
                    return rnd.NextDouble() < 0.5 ? opePlans[idx1] : opePlans[idx2];
                    //                   
            }
        }
       

        /// <summary>
        /// This functions return a list of pair of parents of the same length as the input list using tournament method
        /// </summary>
        /// <param name="chromosomes">List of chromosomes</param>
        /// <returns>A list of pairs of selected parents using the tournament method</returns>
        public List<Tuple<TrainsPlan, TrainsPlan>> Selection(List<TrainsPlan> opePlans)
        {
            List<TrainsPlan> opePlans1 = new List<TrainsPlan>();


            if ((opePlans.Count(x => x.TotalRun >= 30) / opePlans.Count)>0.3)//The elite strategy is innovated. When the number of individuals with 30 rows reaches 30% of the population, the individual pool selected in the championship takes all individuals with 30 rows.
            {
                 opePlans1 = opePlans.Where(x => x.TotalRun >= 30).ToList();
            }
            else
            {
                opePlans1 = opePlans;
            }
            var result = new List<Tuple<TrainsPlan, TrainsPlan>>();

            for (var i = 0; i < 0.5*opePlans.Count; ++i)//Only 0.5 times the population number of parent individuals are needed, and each individual pair can produce two offspring individuals
            {
                result.Add(Tuple.Create(TournamentSelection(opePlans1), TournamentSelection(opePlans1)));
            }
            return result;
        }

        #region Correct chromosomes that do not meet the requirements for the number of rows in the destination and consolidation center
        public static List<int> RepairChro(List<int> chromosome)
        {
            List<int> chro = new List<int>();
            int[] RunTrainPerD = new int[DataSet.nOverseasDestinations];
            int[] RunTrainPerO = new int[DataSet.nConsolidation];
            Random R = new Random(Guid.NewGuid().GetHashCode());
            for (int tr = 0; tr < DataSet.nTrain; tr++)
            {
                int id = new int();
                int pathId = new int();
                while (true)
                {
                    pathId = chromosome[tr];
                    if (pathId < DataSet.DividePathPoint)//Consolidation trains have quantitative requirements for destinations and originating consolidation centers
                    {//Meet the number of consolidation trains required for both destination and originating consolidation centers
                        if (RunTrainPerD[DataSet.PathDestination[pathId]] + 1 <= DataSet.MaxTrainPerD[DataSet.PathDestination[pathId]] && RunTrainPerO[DataSet.PathOrigin[pathId]] + 1 <= DataSet.MaxConsolidationCapacity[DataSet.PathOrigin[pathId]])
                        {
                            RunTrainPerD[DataSet.PathDestination[pathId]]++;
                            RunTrainPerO[DataSet.PathOrigin[pathId]]++;
                            break;
                        }
                        else//Does not meet the number of consolidation trains required for both destination and originating consolidation centers
                        {

                            while (true)
                            {
                                id = R.Next(0, DataSet.AlternatePath[DataSet.TrainPort[tr]].Count);//The index of the selected path The id is the index of the alternative set of the port path The element of the alternative set of the real path
                                pathId = DataSet.AlternatePath[DataSet.TrainPort[tr]][id];
                                if (pathId < DataSet.DividePathPoint)//Consolidation trains have quantitative requirements for destinations and originating consolidation centers
                                {
                                    if (RunTrainPerD[DataSet.PathDestination[pathId]] + 1 <= DataSet.MaxTrainPerD[DataSet.PathDestination[pathId]] && RunTrainPerO[DataSet.PathOrigin[pathId]] + 1 <= DataSet.MaxConsolidationCapacity[DataSet.PathOrigin[pathId]])
                                    {
                                        RunTrainPerD[DataSet.PathDestination[pathId]]++;
                                        RunTrainPerO[DataSet.PathOrigin[pathId]]++;
                                        break;
                                    }
                                }
                                else//Direct train Only the number of destinations is required
                                {
                                    if (RunTrainPerD[DataSet.PathDestination[pathId]] + 1 <= DataSet.MaxTrainPerD[DataSet.PathDestination[pathId]])
                                    {
                                        RunTrainPerD[DataSet.PathDestination[pathId]]++;

                                        break;//Jumping out of the inner while loop
                                    }
                                }

                            }

                            break;//Jumping out of the outer while loop
                        }
                    }
                    else//Direct train Only the number of destinations is required
                    {
                        if (RunTrainPerD[DataSet.PathDestination[pathId]] + 1 <= DataSet.MaxTrainPerD[DataSet.PathDestination[pathId]])
                        {
                            RunTrainPerD[DataSet.PathDestination[pathId]]++;
                            break;
                        }
                        else//Does not meet the number of consolidation trains required for both destination and originating consolidation centers
                        {
                            while (true)
                            {
                                id = R.Next(0, DataSet.AlternatePath[DataSet.TrainPort[tr]].Count);//
                                pathId = DataSet.AlternatePath[DataSet.TrainPort[tr]][id];
                                if (pathId < DataSet.DividePathPoint)//Consolidation trains have quantitative requirements for destinations and originating consolidation centers
                                {
                                    if (RunTrainPerD[DataSet.PathDestination[pathId]] + 1 <= DataSet.MaxTrainPerD[DataSet.PathDestination[pathId]] && RunTrainPerO[DataSet.PathOrigin[pathId]] + 1 <= DataSet.MaxConsolidationCapacity[DataSet.PathOrigin[pathId]])
                                    {
                                        RunTrainPerD[DataSet.PathDestination[pathId]]++;
                                        RunTrainPerO[DataSet.PathOrigin[pathId]]++;
                                        break;
                                    }
                                }
                                else//Direct train Only the number of destinations is required
                                {
                                    if (RunTrainPerD[DataSet.PathDestination[pathId]] + 1 <= DataSet.MaxTrainPerD[DataSet.PathDestination[pathId]])
                                    {
                                        RunTrainPerD[DataSet.PathDestination[pathId]]++;
                                        break;
                                    }
                                }

                            }
                            break;
                        }
                    }
                }
                chro.Add(pathId);
            }
            return chro;
        }
        #endregion
    }
}
