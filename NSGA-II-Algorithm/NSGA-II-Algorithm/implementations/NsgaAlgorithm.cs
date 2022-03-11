using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSGA_II_Algorithm.interfaces;
using NSGA_II_Algorithm.models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;


namespace NSGA_II_Algorithm.implementations
{
    public class NsgaAlgorithm : INsgaAlgorithm
    {
        private IGeneticOperations _geneticOperations;
        private ICrowdingDistanceSort _crowdingDistanceSort;
        private INonDominatedSort _nonDominatedSort;
        //  private IReadOnlyList<Item> _items;

        public NsgaAlgorithm(double crossoverProbability, double mutationProbability)//The parameters are crossover probabilities Variance probabilities 
        {
            //  _items = items;
            _geneticOperations = new GeneticOperations(crossoverProbability, mutationProbability);
            _crowdingDistanceSort = new CrowdingDistanceSort();
            _nonDominatedSort = new NonDominatedSort();
        }

        /// <summary>
        /// Sort the list of chromosomes in fronts 
        /// </summary>
        /// <param name="chromosomes">List of chromosomes</param>
        /// <returns>List of fronts</returns>
        public List<List<TrainsPlan>> SortByFronts(List<TrainsPlan> trPlans)
        {
            var fronts = _nonDominatedSort.Sort(NonDominatedSortAtom.MapFromChromosomes(trPlans));
            return fronts.Select(NonDominatedSortAtom.MapToChromosomes).ToList();
        }

        /// <summary>
        /// Generate a list of chromosomes.
        /// Repeat nrGenerations the follwing:
        /// </summary>
        /// <param name="nrGenerations">Number of generations for evolution</param>
        /// <param name="populationSize">Population Size of Chromosome</param>
        /// <param name="debug">Display Info Variable</param>
        /// <returns>A list of chromosomes</returns>
        /// This method is the core of the algorithm
        public List<TrainsPlan> Process(int nrGenerations, int populationSize, bool debug = false)//Total number of evolutionary generations Population size
        {
            var trPlans = new List<TrainsPlan>();//Single population
            //Generate initial populations
            for (var i = 0; i < populationSize; i++)
            {
                trPlans.Add(TrainsPlan.RandomGenerateTrainsPlan());
            }
            List<int[]> TBjinhuaguochen = new List<int[]>();
            int tempBestTB = 0;
            List<int[]> TWWTjinhuaguochen = new List<int[]>();
            int tempBestTWWT = 6000000;
            //chromosomes = Chromosome.RemoveDuplicated(chromosomes).Take(Math.Min(populationSize, chromosomes.Count)).ToList();
            //Population Evolution
            for (var currentGeneration = 0; currentGeneration < nrGenerations; ++currentGeneration)//For each generation
            {
                //for (int i = trPlans.Count; i < populationSize; ++i)
                //{
                //    trPlans.Add(TrainsPlan.RandomGenerateTrainsPlan());
                //}

                while (trPlans.Count < populationSize)
                {
                    trPlans.Add(TrainsPlan.RandomGenerateTrainsPlan());
                }

                if (debug)
                    Console.WriteLine($"\n############### GENERATION {currentGeneration + 1}###########");

                //TournamentSelection of Parents
                var selectedPairParents = _geneticOperations.Selection(trPlans);
                //if (debug)
                //    Console.WriteLine($"\nSelected Pair Parents size: {selectedPairParents.Count}");

                //Crossover
                var children = new List<TrainsPlan>();

                foreach (var selectedParents in selectedPairParents)
                {
                    var child = _geneticOperations.Crossover(selectedParents.Item2, selectedParents.Item1);

                    int KinshipIndex = 0;//Proximity Index "Multi-objective Flexible Job Shop Scheduling Based on Proximity Variant NSGA-II Algorithm>
                    for (int i = 0; i < DataSet.nTrain; i++)
                    {
                        if (selectedParents.Item2.Chromosome[i]== selectedParents.Item1.Chromosome[i])
                        {
                            KinshipIndex++;
                        }
                    }
                   
                    child.Item1.MutationProb =0.9* KinshipIndex / DataSet.nTrain;
                    child.Item2.MutationProb = 0.9 * KinshipIndex / DataSet.nTrain;

                   // Console.WriteLine(child.Item1.MutationProb);
                    children.Add(child.Item1);
                    children.Add(child.Item2);
                }

                //if (debug)
                //{
                //    Console.WriteLine($"\nChildren size: {children.Count}");
                //    OperationsPlan.PrintList(children);
                //}

                //Mutation
                children = children.Select(child => _geneticOperations.Mutation(child)).ToList();//Mutate first 


                //New Population


                trPlans.AddRange(children);//Merge again
                //trPlans = TrainsPlan.RemoveDuplicated(trPlans);// Remove duplicates
                //foreach (var item in trPlans)
                //{
                //    Console.WriteLine(item.TotalRun);
                //}
                //chromosomes = Chromosome.RemoveDuplicated(chromosomes).Take(Math.Min(populationSize, chromosomes.Count)).ToList();


                //Non Dominated Sort

                var fronts = _nonDominatedSort.Sort(NonDominatedSortAtom.MapFromChromosomes(trPlans));//Get all fronts fronts


                Console.WriteLine($"\r\nFront 1 front size {fronts[0].Count}");
                var front1 = NonDominatedSortAtom.MapToChromosomes(fronts[0]);//Obtaining the Pareto frontierfront1
                                                                              // TrainsPlan.PrintList(front1);
                                                                              //Console.WriteLine("------------------------");
                                                                              //foreach (var trplan in front1)
                                                                              //{
                                                                              //    Console.WriteLine(string.Join(",", trplan.Chromosome));
                                                                              //}
                Console.WriteLine(front1.Max(x=>x.FunctionOfTotalIncome));
                if (tempBestTB< front1.Max(x => x.FunctionOfTotalIncome))
                {
                    int[] aaa = new int[2];
                    tempBestTB = front1.Max(x => x.FunctionOfTotalIncome);
                    aaa[0] = tempBestTB;
                    aaa[1] = currentGeneration;
                    TBjinhuaguochen.Add(aaa);
                }


                var TotalRun30 = front1.Where(x => x.TotalRun >= DataSet.nTrain);
                if (TotalRun30 .Count()> 0)
                {
                    if (tempBestTWWT > TotalRun30. Min(x => x.FunctionOfTotalWaitTime))
                    {
                        int[] aaa = new int[2];
                        tempBestTWWT = TotalRun30.Min(x => x.FunctionOfTotalWaitTime);
                        aaa[0] = tempBestTWWT;
                        aaa[1] = currentGeneration;
                        TWWTjinhuaguochen.Add(aaa);
                    }
                }

                if (currentGeneration == nrGenerations - 1)
                {
                    var selectedAtomsOfFront1 = _crowdingDistanceSort.Sort(CrowdingDistanceAtom.MapFromChromosomes(front1));//
                    //var newFront1 = new List<TrainsPlan>();
                    //int a = Convert.ToInt32(selectedAtomsOfFront1.Count * 0.7);
                    //newFront1.AddRange(CrowdingDistanceAtom.MapToChromosomes(selectedAtomsOfFront1.Take(a).ToList()));

                    //ToExcel0000(newFront1);
                    //Console.WriteLine("* 0.7-----------------");
                  
                    //foreach (var item in newFront1)
                    //{
                    //    Console.Write(item.FunctionOfTotalIncome.ToString()+',');
                       
                    //}
                    //Console.WriteLine();
                    //foreach (var item in newFront1)
                    //{
                    //    Console.Write(item.FunctionOfTotalWaitTime.ToString() + ',');
                       
                    //}
                    Console.WriteLine();
                    var newFront2 = new List<TrainsPlan>();
                    int b= Convert.ToInt32(selectedAtomsOfFront1.Count );
                    newFront2.AddRange(CrowdingDistanceAtom.MapToChromosomes(selectedAtomsOfFront1.Take(b).ToList()));
                    ToExcel0000(newFront2);
                    Console.WriteLine("-----------------");
                    foreach (var item in newFront2)
                    {
                        Console.Write(item.FunctionOfTotalIncome.ToString() + ',');
                      
                    }
                    Console.WriteLine();
                    foreach (var item in newFront2)
                    {
                        Console.Write(item.FunctionOfTotalWaitTime.ToString() + ',');
                       
                    }
                    Console.WriteLine();
                }

                var newPopulation = new List<TrainsPlan>();

                var currentFront = 0;
                while (currentFront < fronts.Count && newPopulation.Count + (int)fronts[currentFront].Count <= populationSize)
                {

                    newPopulation.AddRange(NonDominatedSortAtom.MapToChromosomes(fronts[currentFront]));//
                    currentFront++;
                }
                //fronts[currentFront]It is a subset of the critical layer classification as stated in the book
                if (currentFront < fronts.Count)//Prevent the appearance of no subset of critical layers 
                {
                    var selectedAtoms = _crowdingDistanceSort.Sort(CrowdingDistanceAtom.MapFromChromosomes(NonDominatedSortAtom.MapToChromosomes(fronts[currentFront])));
                    newPopulation.AddRange(CrowdingDistanceAtom.MapToChromosomes(selectedAtoms.Take(populationSize - newPopulation.Count).ToList()));
                }

                trPlans = newPopulation;
                trPlans = trPlans.Take(Math.Min(populationSize, trPlans.Count)).ToList();
               
            }

            //generationEnd of evolution At this point the number of trPlans may be less than the population size

            var fronts1 = _nonDominatedSort.Sort(NonDominatedSortAtom.MapFromChromosomes(trPlans));

            if (debug)
                for (int i = 0; i < fronts1.Count; ++i)
                {
                    Console.WriteLine($"\r\nFront {i + 1} front size {fronts1[i].Count}");
                    TrainsPlan.PrintList(NonDominatedSortAtom.MapToChromosomes(fronts1[i]));
                    if (i == 0)
                    {
                        int j = 0;
                        foreach (var opePlan in NonDominatedSortAtom.MapToChromosomes(fronts1[i]))
                        {
                            j++;

                            Console.Write("Index" + j + "：");
                            //Console.WriteLine("OpePlan:");
                            //foreach (var ope in opePlan.trainsPlan)
                            //{
                            //    Console.WriteLine(ope);
                            //}//All plans are written out
                            Console.WriteLine("FunctionOfTotalIncome:" + opePlan.FunctionOfTotalIncome);
                            Console.WriteLine("FunctionOfTotalWaitTime:" + opePlan.FunctionOfTotalWaitTime);
                        }

                    }
                }
            Console.WriteLine();
            var newPopulation1 = new List<TrainsPlan>();

            var currentFront1 = 0;
            while (currentFront1 < fronts1.Count && newPopulation1.Count + fronts1[currentFront1].Count <= populationSize)
            {
                newPopulation1.AddRange(NonDominatedSortAtom.MapToChromosomes(fronts1[currentFront1]));
                currentFront1++;
            }

            Console.WriteLine("------TB Evolutionary Process-----");
            foreach (var item in TBjinhuaguochen)
            {
                Console.Write(item[1]+",");               
            }
            Console.WriteLine();
            foreach (var item in TBjinhuaguochen)
            {
                Console.Write(item[0] + ",");
            }
            Console.WriteLine();
            Console.WriteLine("------TWWT Evolutionary Process-----");

            foreach (var item in TWWTjinhuaguochen)
            {
                Console.Write(item[1] + ",");
            }
            Console.WriteLine();
            foreach (var item in TWWTjinhuaguochen)
            {
                Console.Write(item[0] + ",");
            }
            Console.WriteLine();
            return trPlans;
        }

        #region Export to excel
        public static void ToExcel0000(List<TrainsPlan> trPlans)
        {
            //Export the results to an Excel file
            IWorkbook workbook = workbook = new XSSFWorkbook();//Write to xlsx file
            FileStream fs = null;
            IRow row = null;
            ISheet sheet = null;
            ISheet sheet1 = null;
            ICell cell = null;
            ISheet sheet0 = workbook.CreateSheet("Pareto Frontier");//Create a table with the name Pareto Frontier  ;
            trPlans = trPlans.OrderBy(atom => atom.FunctionOfTotalWaitTime).ToList();

            int rowCount0 = trPlans.Count;//
            int columnCount0 =11;
            string[] Lietou0 = new string[11] { " index", "wait time", "income","total volume" ,"total run","total wait demand", "total wait demand's TEU", "direct train","consolidation train","total TEU need transitted","chrosome" };
             
            row = sheet0.CreateRow(0);// 
            for (int col = 0; col < columnCount0; col++)
            {
                cell = row.CreateCell(col);
                cell.SetCellValue(Lietou0[col]);

            }
           
            for (int i = 0; i < rowCount0; i++)
            {
                sheet0.SetColumnWidth(i, 3500);//
                row = sheet0.CreateRow(i + 1);
                cell = row.CreateCell(0);//
                cell.SetCellValue(i);
                cell = row.CreateCell(1);// 
                cell.SetCellValue(trPlans[i].FunctionOfTotalWaitTime);
                cell = row.CreateCell(2);//  
                cell.SetCellValue(trPlans[i].FunctionOfTotalIncome);
                cell = row.CreateCell(3);// 
                cell.SetCellValue(trPlans[i].ActualLoad);
                cell = row.CreateCell(4);// 
                cell.SetCellValue(trPlans[i].TotalRun);
                cell = row.CreateCell(5);// 
                cell.SetCellValue(trPlans[i].TotalWaitDemand);
                cell = row.CreateCell(6);//
                cell.SetCellValue(trPlans[i].TotalWaitDemandTEU);
                cell = row.CreateCell(7);// 
                cell.SetCellValue(trPlans[i].TotalDirectTrain);
                cell = row.CreateCell(8);//
                cell.SetCellValue(trPlans[i].TotalConsolidationTrain);
                cell = row.CreateCell(9);//
                cell.SetCellValue(trPlans[i].TotalTransitDemand);
                cell = row.CreateCell(10);
                cell.SetCellValue(string.Join(",", trPlans[i].Chromosome));
            }


            int sheetIndex = 0;
            foreach (var trPlan in trPlans)
            {
                var trains = trPlan.trainsPlan.Where(x => x.BoolRun == 1).ToList();
                var transDemands = trPlan.demandsPlan.Where(x => x.TrainOfDemand >= 0).OrderBy(x => x.TrainOfDemand).ToList();


                sheet = workbook.CreateSheet("demand information" + sheetIndex);//

                //font color
                IFont font = workbook.CreateFont();                //red
                font.Color = HSSFColor.Red.Index;
                
                ICellStyle cellstyle = workbook.CreateCellStyle();
                
                cellstyle.SetFont(font);



                int rowCount = transDemands.Count; 
                int columnCount = 14;
                string[] Lietou = new string[14] {" demand index","volume","freight station","wait time",
                " bool consolidation","consolidation","consolidation time","train index","CRE start time",
                "border port","border port time","expected time consumed","dest","dest time" };
               
                row = sheet.CreateRow(0);
                for (int col = 0; col < columnCount; col++)
                {
                    cell = row.CreateCell(col);
                    cell.SetCellValue(Lietou[col]);
                    if (col == 2 || col == 5)
                    {
                        cell.CellStyle = cellstyle;
                    }
                }

              
                for (int i = 0; i < rowCount; i++)
                {
                    sheet.SetColumnWidth(i, 3500);
                    row = sheet.CreateRow(i + 1);
                    cell = row.CreateCell(0);
                    cell.SetCellValue(transDemands[i].TransDemand_Id);
                    cell = row.CreateCell(1);
                    cell.SetCellValue(transDemands[i].Volume);
                    cell = row.CreateCell(2);
                    cell.SetCellValue(DataSet.StringArriveTEUStation[transDemands[i].TransDemand_Id]);

                    cell = row.CreateCell(3);
                    cell.SetCellValue(transDemands[i].ArriveTEUStationTime);
                    cell = row.CreateCell(4);
                    cell.SetCellValue(transDemands[i].BoolConsolidation);
                    if (transDemands[i].BoolConsolidation == 1)
                    {
                        cell.CellStyle = cellstyle;
                    }

                    cell = row.CreateCell(5);
                    cell.SetCellValue("-");
                    //  cell.SetCellValue(transDemands[i].ConsolidationCentre);
                    if (transDemands[i].BoolConsolidation == 1)
                    {
                        cell.SetCellValue(DataSet.StringPathOrigin[transDemands[i].PathOfDemand]);
                    }

                    cell = row.CreateCell(6);
                    cell.SetCellValue("-");
                    if (transDemands[i].BoolConsolidation == 1)
                    {
                        cell.SetCellValue(transDemands[i].ArriveConsolidationTime);
                    }

                    cell = row.CreateCell(7);
                    cell.SetCellValue(transDemands[i].TrainOfDemand);
                    cell = row.CreateCell(8);
                    cell.SetCellValue(transDemands[i].StartChinaExpressTime);
                    cell = row.CreateCell(9);
                    cell.SetCellValue(DataSet.StringPathPort[transDemands[i].PathOfDemand]);
                    cell = row.CreateCell(10);
                    cell.SetCellValue(transDemands[i].ArrivePortTime);
                    cell = row.CreateCell(11);
                    cell.SetCellValue(transDemands[i].DueTime);
                    cell = row.CreateCell(12);
                    cell.SetCellValue(DataSet.StringPathDestination[transDemands[i].PathOfDemand]);
                    cell = row.CreateCell(13);
                    cell.SetCellValue(transDemands[i].ArriveDestinationTime);


                }



                sheet1 = workbook.CreateSheet("train information" + sheetIndex);  
                sheetIndex++;
                //font color
                int rowCount1 = trains.Count;
                int columnCount1 = 15;
                string[] Lietou1 = new string[15] { "train index", "actual load", "path", "O", "O", "O  time", "P", "P", "P time", "D", "D", "D time", " direct or consolidation", "bool run", "demands loaded" };
                
                row = sheet1.CreateRow(0);
                for (int col = 0; col < columnCount1; col++)
                {
                    cell = row.CreateCell(col);
                    cell.SetCellValue(Lietou1[col]);
                    if (col == 3 || col == 5)
                    {
                        cell.CellStyle = cellstyle;
                    }
                }


                for (int i = 0; i < rowCount1; i++)
                {
                    sheet1.SetColumnWidth(i, 2500);
                    row = sheet1.CreateRow(i + 1);

                    cell = row.CreateCell(0);
                    cell.SetCellValue(trains[i].TrainId);

                    cell = row.CreateCell(1);
                    cell.SetCellValue(trains[i].ActualLoad);
                    cell = row.CreateCell(2);  
                    cell.SetCellValue(trains[i].PathOfTrain);
                    cell = row.CreateCell(3);  
                    cell.SetCellValue(trains[i].StartTEUStation);
                    cell = row.CreateCell(4);  
                    cell.SetCellValue(DataSet.StringPathOrigin[trains[i].PathOfTrain]);

                    cell = row.CreateCell(5);  
                    cell.SetCellValue(trains[i].StartTime);
                    cell = row.CreateCell(6);  
                    cell.SetCellValue(trains[i].ArrivePort);
                    cell = row.CreateCell(7);  
                    cell.SetCellValue(DataSet.StringPathPort[trains[i].PathOfTrain]);

                    cell = row.CreateCell(8);  
                    cell.SetCellValue(trains[i].ArrivePortTime);
                    cell = row.CreateCell(9);  
                    cell.SetCellValue(trains[i].DestinationOfTrain);
                    cell = row.CreateCell(10);  
                    cell.SetCellValue(DataSet.StringPathDestination[trains[i].PathOfTrain]);
                    cell = row.CreateCell(11);  
                    cell.SetCellValue(trains[i].ArriveDestinationTime);
                    cell = row.CreateCell(12);  
                    cell.SetCellValue(trains[i].BoolConsolidation);
                    if (trains[i].BoolConsolidation == 1)
                    {
                        cell.CellStyle = cellstyle;
                    }
                    cell = row.CreateCell(13);  
                    cell.SetCellValue(trains[i].BoolRun);
                    cell = row.CreateCell(14);  
                    cell.SetCellValue(string.Join(",", trains[i].ActualLoadDemand));



                }



            }
            using (fs = File.OpenWrite(@"NSGA2 CRE Operating plan" + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString().Replace(":", " - ") + ".xlsx"))
            {
                workbook.Write(fs);//write
            }

        }


        #endregion

    }
}
