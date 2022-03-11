using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NSGA_II_Algorithm.models
{
    public class TrainsPlan //TrainsPlan  
    {



        public List<Train> trainsPlan { get; set; } 
        public List<TransDemand> demandsPlan { get; set; }
        public double MutationProb { get; set; }//Mutation probability Related to inbreeding index                                           

        public List<int> Chromosome { get; set; }
        public int ActualLoad { get; set; }
        public int TotalRun { get; set; }
        public int TotalWaitDemand { get; set; }
        public int TotalWaitDemandTEU { get; set; }
        public int TotalDirectTrain { get; set; }
        public int TotalConsolidationTrain { get; set; }
        public int TotalTransitDemand { get; set; }

        public int FunctionOfTotalWaitTime { get; set; }//Target value - shortest weighted waiting time for all demands at the station. 
        public int FunctionOfTotalIncome { get; set; }

        public TrainsPlan(List<int> chromosome)
        {
            this.trainsPlan = this.GetTrainsPlan(chromosome);
            this.demandsPlan = this.GetDemandsPlan();
            this.TotalRun = trainsPlan.Count(t => t.BoolRun == 1);
            this.TotalWaitDemand = demandsPlan.Count(t => t.TrainOfDemand > -1 && t.ArriveTEUStationTime == 0);
            this.TotalWaitDemandTEU = demandsPlan.Where(t => t.TrainOfDemand > -1 && t.ArriveTEUStationTime == 0).Sum(t => t.Volume);
            this.TotalDirectTrain = trainsPlan.Count(t => t.BoolRun == 1 && t.StartTEUStation > 4);
            this.TotalConsolidationTrain = trainsPlan.Count(t => t.BoolRun == 1 && t.StartTEUStation <= 4);
            this.TotalTransitDemand = demandsPlan.Where(t => t.TrainOfDemand > -1 && t.BoolConsolidation == 1).Sum(t => t.Volume);

            this.FunctionOfTotalWaitTime = CalFunctionOfTotalWaitTime();
            this.FunctionOfTotalIncome = CalFunctionOfTotalIncome();
            this.ActualLoad = CalActualLoad();
            this.Chromosome = chromosome;

        }




        #region  RandomGenerateTrainsPlan	
        public static TrainsPlan RandomGenerateTrainsPlan()
        {
            Random R = new Random(Guid.NewGuid().GetHashCode());
            List<int> chro = new List<int>();
            int[] RunTrainPerD = new int[DataSet.nOverseasDestinations];
            int[] RunTrainPerO = new int[DataSet.nConsolidation];

            for (int tr = 0; tr < DataSet.nTrain; tr++)
            {
                int id = new int();
                int pathId = new int();
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

                            break;
                        }
                    }

                }

                chro.Add(pathId);
            }
           // Console.WriteLine(string.Join(",", chro));

            var trainsPlan = new TrainsPlan(chro);
            //  Program.ToExcel(trainsPlan.demandsPlan, trainsPlan.trainsPlan);//import to excel
            return trainsPlan;
        }
        #endregion

        #region  Heuristic decoding
        public List<Train> GetTrainsPlan(List<int> Chromosome)
        {
            List<Train> trains = new List<Train>();
            List<TransDemand> demandsPool = new List<TransDemand>();
            for (int tr = 0; tr < DataSet.nTrain; tr++)
            {
                Train train = new Train();
                train.TrainId = tr;
                train.ArrivePort = DataSet.TrainPort[tr];
                train.ArrivePortTime = DataSet.TrainPortTime[tr];
                train.PathOfTrain = Chromosome[tr];
                train.StartTEUStation = DataSet.PathOrigin[train.PathOfTrain];
                train.DestinationOfTrain = DataSet.PathDestination[train.PathOfTrain];
                train.StartTime = train.ArrivePortTime - DataSet.time_2[train.StartTEUStation, train.ArrivePort];
                train.ArriveDestinationTime = train.ArrivePortTime + DataSet.time_3[train.ArrivePort, train.DestinationOfTrain];
                train.BoolConsolidation = 0;
                trains.Add(train);
            }
            for (int i = 0; i < DataSet.nTransDemand; i++)
            {
                TransDemand demand = new TransDemand();
                demand.TransDemand_Id = DataSet.DemandId[i];
                demand.ArriveTEUStation = DataSet.ArriveTEUStation[i];
                demand.ArriveTEUStationTime = DataSet.ArriveTEUStationTime[i];
                demand.Volume = DataSet.Volume[i];
                demand.DestinationOfDemand = DataSet.DestinationOfDemand[i];
                demand.DueTime = DataSet.DueTime[i];

                demandsPool.Add(demand);
            }


            List<TransDemand> demandsPoolGenxin = new List<TransDemand>(demandsPool.ToArray()); // copy of demandsPool
                                                                                                //Copy a new requirement pool named "Requirement Pool Update"   
                                                                                                // Iterate over trains to match requirements for trains After a successful match, delete the matched requirements from the requirements pool update.

            var directTrainsOrderByStartTime = trains.Where(x => x.StartTEUStation > 4).OrderBy(x => x.StartTime).ToList();// All direct trains are sorted by departure time to facilitate subsequent allocation of transportation needs in the order of departure
            var consolidationTrainsOrderByStartTime = trains.Where(x => x.StartTEUStation <= 4).OrderBy(x => x.StartTime).ToList();//All assembled trains are sorted according to their departure time to facilitate the subsequent allocation of transport needs in the order of departure
            var trainsOrderByStartTime = directTrainsOrderByStartTime.Concat(consolidationTrainsOrderByStartTime).ToList();// first Direct  then assembly
         
            for (int tr = 0; tr < DataSet.nTrain; tr++)
            {
                List<TransDemand> demandsPoolOfTr = new List<TransDemand>();
                if (trainsOrderByStartTime[tr].PathOfTrain < DataSet.DividePathPoint)//consolidation
                {
                    demandsPoolOfTr = demandsPoolGenxin.Where(x => x.DestinationOfDemand == trainsOrderByStartTime[tr].DestinationOfTrain//Demand dest and train dest are the same
                                                                                                                                      
                    ).ToList();//
                }
                else//dir train
                {
                    demandsPoolOfTr = demandsPoolGenxin.Where(x => x.DestinationOfDemand == trainsOrderByStartTime[tr].DestinationOfTrain // /Demand dest and train dest are the same
                                  && x.ArriveTEUStation == trainsOrderByStartTime[tr].StartTEUStation).ToList();//Direct trains need to ensure that the starting point of demand and the starting point of the train are the same.
                   
                }

                foreach (var demand in demandsPoolOfTr)
                {
                    if (demand.ArriveTEUStation == trainsOrderByStartTime[tr].StartTEUStation)
                    {
                        demand.EarliestTEUStationTime = demand.ArriveTEUStationTime;
                    }
                    else
                    {
                        demand.EarliestTEUStationTime = demand.ArriveTEUStationTime + DataSet.time_1[demand.ArriveTEUStation, trainsOrderByStartTime[tr].StartTEUStation];
                    }
                }
                //  demandsPoolOfTr = demandsPoolOfTr.Where(x => x.EarliestTEUStationTime <= trainsOrderByStartTime[tr].StartTime).OrderByDescending(x => x.Priority).ThenBy(x => x.EarliestTEUStationTime).ToList();

                demandsPoolOfTr = demandsPoolOfTr.Where(x => x.EarliestTEUStationTime <= trainsOrderByStartTime[tr].StartTime).ToList();//

                demandsPoolOfTr = RandomSortList(demandsPoolOfTr);//

                List<int> temp1 = new List<int>();
                foreach (var demand in demandsPoolOfTr)
                {
                    if (trainsOrderByStartTime[tr].ActualLoad < DataSet.MaxMakeUp)
                    {
                        trainsOrderByStartTime[tr].ActualLoad += demand.Volume;

                        if (trainsOrderByStartTime[tr].ActualLoad > DataSet.MaxMakeUp)
                        {
                            trainsOrderByStartTime[tr].ActualLoad -= demand.Volume;//After the maximum load, the demand does not match the train tr
                            continue;
                        }
                        else
                        {
                            temp1.Add(demand.TransDemand_Id);
                          

                        }
                    }
                    else if (trainsOrderByStartTime[tr].ActualLoad == DataSet.MaxMakeUp)//Exactly the maximum number of formations
                    {
                        break;
                    }
                }
                trainsOrderByStartTime[tr].ActualLoadDemand = temp1;

                if (trainsOrderByStartTime[tr].ActualLoad >= DataSet.MinMakeUp)//Traversed the train demand pool to meet the minimum grouping requirement
                {
                    foreach (var id in trainsOrderByStartTime[tr].ActualLoadDemand)
                    {
                        //    if (demand.TrainOfDemand == tr)
                        //    {
                        //        demandsPoolGenxin.Remove(demand);
                        //    }
                        demandsPoolGenxin.Remove(demandsPool[id]);
                    }
                    trainsOrderByStartTime[tr].BoolRun = 1;
                }
                else
                {
                    //foreach (var demand in demandsPoolOfTr)
                    //{
                    //    demand.TrainOfDemand = 1000;
                    //}
                    trainsOrderByStartTime[tr].BoolRun = 0;
                }
            }
            return trains;
        }
        #endregion

        #region   Heuristic decoding  GetDemandsPlan
        public List<TransDemand> GetDemandsPlan()
        {

            List<TransDemand> demandsAfterAssign = new List<TransDemand>();

            for (int i = 0; i < DataSet.nTransDemand; i++)
            {
                TransDemand demand = new TransDemand();
                demand.TransDemand_Id = DataSet.DemandId[i];
                demand.ArriveTEUStation = DataSet.ArriveTEUStation[i];
                demand.ArriveTEUStationTime = DataSet.ArriveTEUStationTime[i];
                demand.Volume = DataSet.Volume[i];
                demand.Income = DataSet.IncomePerTEU[i];
                demand.DestinationOfDemand = DataSet.DestinationOfDemand[i];
                demand.DueTime = DataSet.DueTime[i];
                demand.TrainOfDemand = -1;//Default -1 means no trains matched

                demandsAfterAssign.Add(demand);
            }

            foreach (var tr in trainsPlan)
            {
                if (tr.BoolRun == 1)
                {
                    foreach (var demandId in tr.ActualLoadDemand)
                    {
                        demandsAfterAssign[demandId].TrainOfDemand = tr.TrainId;
                        demandsAfterAssign[demandId].PathOfDemand = tr.PathOfTrain;
                        demandsAfterAssign[demandId].ArriveDestinationTime = tr.ArriveDestinationTime;
                        if (demandsAfterAssign[demandId].ArriveTEUStation == tr.StartTEUStation)
                        {
                            demandsAfterAssign[demandId].ConsolidationCentre = -1;
                            demandsAfterAssign[demandId].ArriveConsolidationTime = -1;
                            demandsAfterAssign[demandId].BoolConsolidation = 0;
                        }
                        else
                        {
                            demandsAfterAssign[demandId].ConsolidationCentre = tr.StartTEUStation;
                            demandsAfterAssign[demandId].ArriveConsolidationTime = demandsAfterAssign[demandId].ArriveTEUStationTime + DataSet.time_1[demandsAfterAssign[demandId].ArriveTEUStation, tr.StartTEUStation];
                            demandsAfterAssign[demandId].BoolConsolidation = 1;
                            tr.BoolConsolidation = 1;
                        }
                        demandsAfterAssign[demandId].StartChinaExpressTime = tr.StartTime;
                        demandsAfterAssign[demandId].ArrivePort = tr.ArrivePort;
                        demandsAfterAssign[demandId].ArrivePortTime = tr.ArrivePortTime;
                    }
                }
            }
            return demandsAfterAssign;
        }
        #endregion




        #region  Calculate the objective function Maximize the benefit
        private int CalFunctionOfTotalIncome()
        {
            int totalFunction = 0;
                       
            foreach (var demand in demandsPlan)
            {
                if (demand.TrainOfDemand >= 0)
                {
                    totalFunction += demand.Volume * demand.Income;//income
                    totalFunction -= DataSet.CostPerHour * demand.Volume * (demand.ArriveDestinationTime - demand.ArriveTEUStationTime + demand.WaitTime);//time cost                    
                    if (demand.BoolConsolidation == 1)
                    {
                        totalFunction -= DataSet.CommonTrainCostPerKMTEU * demand.Volume * DataSet.distance_1[demand.ArriveTEUStation, demand.ConsolidationCentre];//Common  transportation costs in the consolidation process
                    }
                }
            }

            foreach (var train in trainsPlan)
            {
                if (train.BoolRun == 1)
                {
                    
                    totalFunction -= DataSet.ChinaEuTrianCostPerKM * DataSet.PathDistance[train.PathOfTrain];//Variable Costs of CRE Transportation
                    if (train.PathOfTrain < DataSet.DividePathPoint)
                    {
                        totalFunction -= DataSet.CostPerConsolidationTrain;//Consolidation of fixed costs of CRE transport     
                    }
                    else
                    {
                        totalFunction -= DataSet.CostPerDirectTrain;//Direct of fixed costs of CRE transport     
                    }
                }
            }
            return totalFunction;
        }
        #endregion

        #region  Calculate the objective function Minimum waiting time
        private int CalFunctionOfTotalWaitTime()  
        {
            double totalFunction = 0;
        

            foreach (var demand in demandsPlan)
            {
                if (demand.TrainOfDemand >= 0)
                {
                    if (demand.ArriveTEUStation != DataSet.PathOrigin[demand.PathOfDemand])//consoli
                    {
                        totalFunction += DataSet.Priority[demand.TransDemand_Id] * demand.Volume * (demand.StartChinaExpressTime - demand.ArriveTEUStationTime - DataSet.time_1[demand.ArriveTEUStation, demand.ConsolidationCentre]);//Waiting time at the station for the con train demand

                    }
                    else//direct
                    {
                        totalFunction += DataSet.Priority[demand.TransDemand_Id] * demand.Volume * (demand.StartChinaExpressTime - demand.ArriveTEUStationTime);//Waiting time at the station for the direct train demand
                    }
                }
                else//not matched
                {
                    totalFunction += DataSet.Priority[demand.TransDemand_Id] * demand.Volume * DataSet.PenaltyTime;//Weighted penalty time on unmatched

                }
            }



            return (int)totalFunction;
        }

        #endregion


        #region Determining whether to dominate
        public int Dominates(TrainsPlan trainPlan1)
        {


            var thisFunctionOfTotalWaitTime = this.CalFunctionOfTotalWaitTime();
            var thisFunctionOfTotalIncome = this.CalFunctionOfTotalIncome();
            var trainPlan1FunctionOfTotalWaitTime = trainPlan1.CalFunctionOfTotalWaitTime();
            var trainPlan1FunctionOfTotalIncome = trainPlan1.CalFunctionOfTotalIncome();

            
            if ((thisFunctionOfTotalWaitTime <= trainPlan1FunctionOfTotalWaitTime && thisFunctionOfTotalIncome >= trainPlan1FunctionOfTotalIncome) && (thisFunctionOfTotalWaitTime < trainPlan1FunctionOfTotalWaitTime || thisFunctionOfTotalIncome > trainPlan1FunctionOfTotalIncome))
          
            {
                return 1;
            }
            //this dominates trainPlan1
            if ((thisFunctionOfTotalWaitTime >= trainPlan1FunctionOfTotalWaitTime && thisFunctionOfTotalIncome <= trainPlan1FunctionOfTotalIncome) && (thisFunctionOfTotalWaitTime > trainPlan1FunctionOfTotalWaitTime || thisFunctionOfTotalIncome < trainPlan1FunctionOfTotalIncome))
            {
                return -1;
            }//trainPlan1 dominates this
            return 0;//nondominate
        }
        #endregion


        public static void PrintList(List<TrainsPlan> trPlans)
        {
            Console.WriteLine("##### OPERATIONSPLAN LIST ######");
            int i = 0;
            foreach (var trPlan in trPlans)
            {
                i++;
                Console.Write("Index" + i + "：");
                Console.WriteLine("FunctionOfTotalIncome:" + trPlan.FunctionOfTotalIncome);
                Console.WriteLine("FunctionOfTotalWaitTime:" + trPlan.FunctionOfTotalWaitTime);

            }
        }

        public int CalActualLoad()
        {
            int a = new int();

            var trains = trainsPlan.Where(x => x.BoolRun == 1).ToList();
            foreach (var tr in trains)
            {
                a += tr.ActualLoad;
            }
            return a;
        }



        public static List<TrainsPlan> RemoveDuplicated(List<TrainsPlan> trPlans)
        {
            var filteredList = new List<TrainsPlan>();
            Random R = new Random(Guid.NewGuid().GetHashCode());


            foreach (var trplan in trPlans)
            {
                if (filteredList.FindIndex(item => item.IsEqual(trplan)) == -1)
                {
                    filteredList.Add(trplan);
                }
                else
                {
                    
                    if (R.NextDouble() < 0.2)
                    {
                        filteredList.Add(trplan);
                    }

                }
            }
            return filteredList;
        }


        public bool IsEqual(TrainsPlan trPlan)
        {

            
            if (this.Chromosome.SequenceEqual(trPlan.Chromosome))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static List<T> RandomSortList<T>(List<T> ListT)
        {
            Random random = new Random();
            List<T> newList = new List<T>();
            foreach (T item in ListT)
            {
                newList.Insert(random.Next(newList.Count + 1), item);
            }
            return newList;
        }
    }
}
