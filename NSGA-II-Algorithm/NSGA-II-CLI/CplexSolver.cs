using System;
using System.Collections.Generic;
using ILOG.Concert;
using ILOG.CPLEX;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;
using System.IO;
using NSGA_II_Algorithm.models;

namespace NSGA_II_CLI
{
    public class CplexSolver
    {

        #region  colve by CPLEX 
        public static void LinePlan(int whichObject)
        {


            Console.WriteLine("Data reading is complete");
            
            Cplex cplexMod1 = new Cplex();

            //Define decision variables
            INumVar[][][] x = new INumVar[DataSet.nTransDemand][][];
            for (int i = 0; i < DataSet.nTransDemand; i++)
            {
                x[i] = new INumVar[DataSet.nTrain][];
                for (int j = 0; j < DataSet.nTrain; j++)
                {
                    x[i][j] = cplexMod1.NumVarArray(DataSet.nPath, 0, 1, NumVarType.Bool);
                }
            }
            INumVar[][] y = new INumVar[DataSet.nTrain][];
            for (int i = 0; i < DataSet.nTrain; i++)
            {
                y[i] = cplexMod1.NumVarArray(DataSet.nPath, 0, 1, NumVarType.Bool);

            }

            //Constraints
            //Number of wagons 

            for (int tr = 0; tr < DataSet.nTrain; tr++)
            {
                ILinearNumExpr expr1 = cplexMod1.LinearNumExpr();
                for (int i = 0; i < DataSet.nTransDemand; i++)
                {
                    for (int p = 0; p < DataSet.nPath; p++)
                    {
                        if (DataSet.DestinationOfDemand[i] == DataSet.PathDestination[p] && DataSet.TrainPort[tr] == DataSet.PathPort[p])
                        {
                            expr1.AddTerm(DataSet.Volume[i], x[i][tr][p]);
                        }
                    }
                }
                cplexMod1.AddLe(expr1, DataSet.MaxMakeUp);
            }

            for (int tr = 0; tr < DataSet.nTrain; tr++)
            {
                for (int p = 0; p < DataSet.nPath; p++)
                {
                    if (DataSet.TrainPort[tr] == DataSet.PathPort[p])
                    {
                        ILinearNumExpr expr1 = cplexMod1.LinearNumExpr();
                        expr1.AddTerm(DataSet.BigM, y[tr][p]);
                        for (int i = 0; i < DataSet.nTransDemand; i++)
                        {
                            if (DataSet.DestinationOfDemand[i] == DataSet.PathDestination[p])
                            {
                                expr1.AddTerm(-DataSet.Volume[i], x[i][tr][p]);
                            }
                        }
                        int a = DataSet.BigM - DataSet.MinMakeUp;
                        cplexMod1.AddLe(expr1, a);
                    }
                }
            }

           
            for (int i = 0; i < DataSet.nTransDemand; i++)
            {
                ILinearNumExpr expr1 = cplexMod1.LinearNumExpr();
                for (int tr = 0; tr < DataSet.nTrain; tr++)
                {
                    for (int p = 0; p < DataSet.nPath; p++)
                    {
                        if (DataSet.DestinationOfDemand[i] == DataSet.PathDestination[p] && DataSet.TrainPort[tr] == DataSet.PathPort[p])
                        {
                            expr1.AddTerm(1, x[i][tr][p]);
                        }
                    }
                }
                cplexMod1.AddLe(expr1, 1);
            }

            //Arbitrary trains, matching only one running path

            for (int tr = 0; tr < DataSet.nTrain; tr++)
            {
                ILinearNumExpr expr1 = cplexMod1.LinearNumExpr();
                for (int p = 0; p < DataSet.nPath; p++)
                {
                    if (DataSet.TrainPort[tr] == DataSet.PathPort[p])
                    {
                        expr1.AddTerm(1, y[tr][p]);
                    }
                }
                cplexMod1.AddEq(expr1, 1);
            }

            //mapping constraint
            for (int tr = 0; tr < DataSet.nTrain; tr++)
            {
                for (int p = 0; p < DataSet.nPath; p++)
                {
                    ILinearNumExpr expr1 = cplexMod1.LinearNumExpr();
                    if (DataSet.TrainPort[tr] == DataSet.PathPort[p])
                    {
                        expr1.AddTerm(999999999, y[tr][p]);
                        for (int i = 0; i < DataSet.nTransDemand; i++)
                        {
                            if (DataSet.DestinationOfDemand[i] == DataSet.PathDestination[p])
                            {
                                expr1.AddTerm(-1, x[i][tr][p]);
                            }
                        }
                    }
                    cplexMod1.AddGe(expr1, 0);
                }
            }


            //time

            for (int i = 0; i < DataSet.nTransDemand; i++)
            {
                for (int p = 0; p < DataSet.nPath; p++)
                {
                    for (int tr = 0; tr < DataSet.nTrain; tr++)
                    {
                        if (DataSet.DestinationOfDemand[i] == DataSet.PathDestination[p] && DataSet.TrainPort[tr] == DataSet.PathPort[p])
                        {
                            if (DataSet.ArriveTEUStation[i] == DataSet.PathOrigin[p])//direct
                            {
                                ILinearNumExpr expr1 = cplexMod1.LinearNumExpr();
                                expr1.AddTerm(DataSet.BigM, x[i][tr][p]);

                                int a = DataSet.TrainPortTime[tr] - DataSet.time_2[DataSet.PathOrigin[p], DataSet.PathPort[p]] - DataSet.ArriveTEUStationTime[i] + DataSet.BigM;
                                cplexMod1.AddLe(expr1, a);
                            }
                            else//con CRE train
                            {
                                if (p < DataSet.DividePathPoint)//The former DividePathPoint path is the path from the rally center
                                {
                                    ILinearNumExpr expr1 = cplexMod1.LinearNumExpr();
                                    expr1.AddTerm(DataSet.BigM, x[i][tr][p]);

                                    int a = DataSet.TrainPortTime[tr] - DataSet.time_2[DataSet.PathOrigin[p], DataSet.PathPort[p]] - DataSet.ArriveTEUStationTime[i] + DataSet.BigM - DataSet.time_1[DataSet.ArriveTEUStation[i], DataSet.PathOrigin[p]];
                                    cplexMod1.AddLe(expr1, a);
                                }
                                else//The path after DividePathPoint is not the path from the assembly center This is not likely to happen
                                {
                                    cplexMod1.AddEq(x[i][tr][p], 0);
                                }
                            }
                        }
                    }
                }
            }


            //Consolidation center processing capacity constraints

            for (int cc = 0; cc < DataSet.nConsolidation; cc++)
            {
                ILinearNumExpr expr1 = cplexMod1.LinearNumExpr();
                for (int p = 0; p < DataSet.nPath; p++)
                {
                    if (DataSet.PathOrigin[p] == cc)
                    {
                        for (int tr = 0; tr < DataSet.nTrain; tr++)
                        {
                            if (DataSet.TrainPort[tr] == DataSet.PathPort[p])
                            {
                                expr1.AddTerm(1, y[tr][p]);
                            }
                        }
                    }
                }
                cplexMod1.AddLe(expr1, DataSet.MaxConsolidationCapacity[cc]);
                cplexMod1.AddGe(expr1, DataSet.MinConsolidationCapacity[cc]);
            }

            //Destination running number constraint

            for (int cc = 0; cc < DataSet.nOverseasDestinations; cc++)
            {
                ILinearNumExpr expr1 = cplexMod1.LinearNumExpr();
                for (int p = 0; p < DataSet.nPath; p++)
                {
                    if (DataSet.PathDestination[p] == cc)
                    {
                        for (int tr = 0; tr < DataSet.nTrain; tr++)
                        {
                            if (DataSet.TrainPort[tr] == DataSet.PathPort[p])
                            {
                                expr1.AddTerm(1, y[tr][p]);
                            }
                        }
                    }
                }
                cplexMod1.AddLe(expr1, DataSet.MaxTrainPerD[cc]);
                cplexMod1.AddGe(expr1, DataSet.MinTrainPerD[cc]);
            }




            //Exclusion of infeasible cases
            for (int tr = 0; tr < DataSet.nTrain; tr++)
            {
                for (int p = 0; p < DataSet.nPath; p++)
                {
                    if (DataSet.TrainPort[tr] != DataSet.PathPort[p])
                    {
                        cplexMod1.AddEq(0, y[tr][p]);
                    }
                }
            }
            //Exclusion of infeasible cases
            for (int tr = 0; tr < DataSet.nTrain; tr++)
            {
                for (int i = 0; i < DataSet.nTransDemand; i++)
                {
                    for (int p = 0; p < DataSet.nPath; p++)
                    {
                        if (DataSet.DestinationOfDemand[i] != DataSet.PathDestination[p] || DataSet.TrainPort[tr] != DataSet.PathPort[p])
                        {
                            cplexMod1.AddEq(x[i][tr][p], 0);
                        }
                    }
                }
            }
            double totalTimePenalty = 0;
            for (int i = 0; i < DataSet.nTransDemand; i++)
            {
                totalTimePenalty += DataSet.Priority[i] * DataSet.Volume[i] * DataSet.PenaltyTime;
            }





            //obj  dexpr float TotalVolume = sum(i in node, j in station, k in port)volume[i] * x[i][j][k];
            ILinearNumExpr TotalVolume = cplexMod1.LinearNumExpr();
            if (whichObject == 0)
            {
                for (int i = 0; i < DataSet.nTransDemand; i++)
                {
                    for (int tr = 0; tr < DataSet.nTrain; tr++)
                    {
                        for (int p = 0; p < DataSet.nPath; p++)
                        {
                            if (DataSet.DestinationOfDemand[i] == DataSet.PathDestination[p] && DataSet.TrainPort[tr] == DataSet.PathPort[p])
                            {
                               
                                TotalVolume.AddTerm(-DataSet.Priority[i] * DataSet.Volume[i] * DataSet.PenaltyTime, x[i][tr][p]);//F_3表示未匹配成功的需求的惩罚时间
                                if (p < DataSet.DividePathPoint && DataSet.ArriveTEUStation[i] != DataSet.PathOrigin[p])//集结列车
                                {
                                    TotalVolume.AddTerm(DataSet.Priority[i] * DataSet.Volume[i] * (DataSet.TrainPortTime[tr] - DataSet.time_1[DataSet.ArriveTEUStation[i], DataSet.PathOrigin[p]] - DataSet.time_2[DataSet.PathOrigin[p], DataSet.PathPort[p]] - DataSet.ArriveTEUStationTime[i]), x[i][tr][p]);//f2
                                }
                                else//直达列车
                                {
                                    TotalVolume.AddTerm(DataSet.Priority[i] * DataSet.Volume[i] * (DataSet.TrainPortTime[tr] - DataSet.time_2[DataSet.PathOrigin[p], DataSet.PathPort[p]] - DataSet.ArriveTEUStationTime[i]), x[i][tr][p]);//f1
                                }
                            }
                        }
                    }
                }

                cplexMod1.AddMinimize(TotalVolume);
            }

            else if (whichObject == 1)
            {
                for (int i = 0; i < DataSet.nTransDemand; i++)
                {
                    for (int tr = 0; tr < DataSet.nTrain; tr++)
                    {
                        for (int p = 0; p < DataSet.nPath; p++)
                        {
                            if (DataSet.DestinationOfDemand[i] == DataSet.PathDestination[p] && DataSet.TrainPort[tr] == DataSet.PathPort[p])
                            {
                                TotalVolume.AddTerm(DataSet.IncomePerTEU[i] * DataSet.Volume[i], x[i][tr][p]);//f1收益
                                TotalVolume.AddTerm(-DataSet.CostPerHour * DataSet.Volume[i] * (DataSet.TrainPortTime[tr] + DataSet.time_3[DataSet.PathPort[p], DataSet.PathDestination[p]] - DataSet.ArriveTEUStationTime[i] + DataSet.WaitTime[i]), x[i][tr][p]);//f2时间成本
                                if (p < DataSet.DividePathPoint && DataSet.ArriveTEUStation[i] != DataSet.PathOrigin[p])//集结费用
                                {
                                    TotalVolume.AddTerm(-DataSet.CommonTrainCostPerKMTEU * DataSet.Volume[i] * DataSet.distance_1[DataSet.ArriveTEUStation[i], DataSet.PathOrigin[p]], x[i][tr][p]);//f3集结成本
                                }
                            }
                        }
                    }
                }

                for (int tr = 0; tr < DataSet.nTrain; tr++)
                {
                    for (int p = 0; p < DataSet.nPath; p++)
                    {
                        if (DataSet.TrainPort[tr] == DataSet.PathPort[p])
                        {
                            if (p < DataSet.DividePathPoint)
                            {
                                TotalVolume.AddTerm(-DataSet.CostPerConsolidationTrain - DataSet.ChinaEuTrianCostPerKM * DataSet.PathDistance[p], y[tr][p]);//f4  集结班列 运输成本       
                            }
                            else
                            {
                                TotalVolume.AddTerm(-DataSet.CostPerDirectTrain - DataSet.ChinaEuTrianCostPerKM * DataSet.PathDistance[p], y[tr][p]);//f4   直达班列运输成本       
                            }

                        }
                    }
                }
                cplexMod1.AddMaximize(TotalVolume);
            }
            else//Normalized and weighted to a single objective
            {
               
                for (int i = 0; i < DataSet.nTransDemand; i++)
                {
                    for (int tr = 0; tr < DataSet.nTrain; tr++)
                    {
                        for (int p = 0; p < DataSet.nPath; p++)
                        {
                            if (DataSet.DestinationOfDemand[i] == DataSet.PathDestination[p] && DataSet.TrainPort[tr] == DataSet.PathPort[p])
                            {
                                
                                TotalVolume.AddTerm(-DataSet.CoeffObjTime * DataSet.Priority[i] * DataSet.Volume[i] * DataSet.PenaltyTime, x[i][tr][p]);//F_3Indicates the penalty time for unsuccessful demand matches
                                if (p < DataSet.DividePathPoint && DataSet.ArriveTEUStation[i] != DataSet.PathOrigin[p])//con train
                                {
                                    TotalVolume.AddTerm(DataSet.CoeffObjTime * DataSet.Priority[i] * DataSet.Volume[i] * (DataSet.TrainPortTime[tr] - DataSet.time_1[DataSet.ArriveTEUStation[i], DataSet.PathOrigin[p]] - DataSet.time_2[DataSet.PathOrigin[p], DataSet.PathPort[p]] - DataSet.ArriveTEUStationTime[i]), x[i][tr][p]);//f2
                                }
                                else//dir train
                                {
                                    TotalVolume.AddTerm(DataSet.CoeffObjTime * DataSet.Priority[i] * DataSet.Volume[i] * (DataSet.TrainPortTime[tr] - DataSet.time_2[DataSet.PathOrigin[p], DataSet.PathPort[p]] - DataSet.ArriveTEUStationTime[i]), x[i][tr][p]);//f1
                                }
                            }
                        }
                    }
                }

           
                for (int i = 0; i < DataSet.nTransDemand; i++)
                {
                    for (int tr = 0; tr < DataSet.nTrain; tr++)
                    {
                        for (int p = 0; p < DataSet.nPath; p++)
                        {
                            if (DataSet.DestinationOfDemand[i] == DataSet.PathDestination[p] && DataSet.TrainPort[tr] == DataSet.PathPort[p])
                            {
                                TotalVolume.AddTerm(DataSet.CoeffObjBenefit * DataSet.IncomePerTEU[i] * DataSet.Volume[i], x[i][tr][p]);//f1Revenue 
                                TotalVolume.AddTerm(-DataSet.CoeffObjBenefit * DataSet.CostPerHour * DataSet.Volume[i] * (DataSet.TrainPortTime[tr] + DataSet.time_3[DataSet.PathPort[p], DataSet.PathDestination[p]] - DataSet.ArriveTEUStationTime[i] + DataSet.WaitTime[i]), x[i][tr][p]);//f2time cost
                                if (p < DataSet.DividePathPoint && DataSet.ArriveTEUStation[i] != DataSet.PathOrigin[p])//
                                {
                                    TotalVolume.AddTerm(-DataSet.CoeffObjBenefit * DataSet.CommonTrainCostPerKMTEU * DataSet.Volume[i] * DataSet.distance_1[DataSet.ArriveTEUStation[i], DataSet.PathOrigin[p]], x[i][tr][p]);//f3 transit cost
                                }
                            }
                        }
                    }
                }

                for (int tr = 0; tr < DataSet.nTrain; tr++)
                {
                    for (int p = 0; p < DataSet.nPath; p++)
                    {
                        if (DataSet.TrainPort[tr] == DataSet.PathPort[p])
                        {
                            if (p < DataSet.DividePathPoint)
                            {
                                TotalVolume.AddTerm(-DataSet.CoeffObjBenefit * (DataSet.CostPerConsolidationTrain + DataSet.ChinaEuTrianCostPerKM * DataSet.PathDistance[p]), y[tr][p]);//f4 con CRE tr cost       
                            }
                            else
                            {
                                TotalVolume.AddTerm(-DataSet.CoeffObjBenefit * (DataSet.CostPerDirectTrain + DataSet.ChinaEuTrianCostPerKM * DataSet.PathDistance[p]), y[tr][p]);//f4  dir CRE tr cost   
                            }

                        }
                    }
                }
                cplexMod1.AddMinimize(TotalVolume);


            }


            //  cplexMod1.SetParam(Cplex.DoubleParam.EpGap, 0.05); // cplex terminate if mip gap hit X
            cplexMod1.SetParam(Cplex.Param.TimeLimit, DataSet.ComputeTime); //max cal  time

            
            List<TransDemand> transDemands = new List<TransDemand>();

            List<Train> trains = new List<Train>();
            // cplexMod1.ExportModel("lpex1.lp");

            
            if (cplexMod1.Solve())//If cplex finds a feasible solution
            {

                Console.WriteLine("CPLEX OBJ VALUE：{0}", cplexMod1.ObjValue);
               

                for (int i = 0; i < DataSet.nTransDemand; i++)
                {
                    TransDemand transDemand = new TransDemand();

                    transDemand.TransDemand_Id = i;
                    transDemand.ArriveTEUStationTime = DataSet.ArriveTEUStationTime[i];
                    transDemand.ArriveTEUStation = DataSet.ArriveTEUStation[i];
                    transDemand.TrainOfDemand = -1;
                    transDemand.Volume = DataSet.Volume[i];
                    transDemand.Income = DataSet.IncomePerTEU[i];
                    transDemand.DestinationOfDemand = DataSet.DestinationOfDemand[i];
                    transDemand.DueTime = DataSet.DueTime[i];
                    transDemand.WaitTime = DataSet.WaitTime[i];
                    for (int tr = 0; tr < DataSet.nTrain; tr++)
                    {
                        for (int p = 0; p < DataSet.nPath; p++)
                        {
                           
                            if (Convert.ToInt32(cplexMod1.GetValue(x[i][tr][p])) == 1)
                            {
                               
                                transDemand.PathOfDemand = p;
                                transDemand.TrainOfDemand = tr;
                                transDemand.ArrivePort = DataSet.TrainPort[tr];
                                transDemand.ArrivePortTime = DataSet.TrainPortTime[tr];

                                transDemand.StartChinaExpressTime = DataSet.TrainPortTime[tr] - DataSet.time_2[DataSet.PathOrigin[p], DataSet.PathPort[p]];
                                transDemand.ArriveDestinationTime = transDemand.StartChinaExpressTime + DataSet.PathTime[p];

                                if (DataSet.ArriveTEUStation[i] == DataSet.PathOrigin[p])//no  consolidation
                                {
                                    transDemand.BoolConsolidation = 0;
                                    transDemand.ConsolidationCentre = 1000;
                                    transDemand.ArriveConsolidationTime = 1000;
                                }
                                else
                                {
                                    transDemand.BoolConsolidation = 1;
                                    transDemand.ConsolidationCentre = DataSet.PathOrigin[p];
                                    transDemand.ArriveConsolidationTime = DataSet.ArriveTEUStationTime[i] + DataSet.time_1[DataSet.ArriveTEUStation[i], DataSet.PathOrigin[p]];
                                }
                               
                            }
                         
                        }

                    }
                    transDemands.Add(transDemand);
                }

                int id = 0;
                for (int tr = 0; tr < DataSet.nTrain; tr++)
                {
                    for (int p = 0; p < DataSet.nPath; p++)
                    {
                        if (Convert.ToInt32(cplexMod1.GetValue(y[tr][p])) == 1)
                        {
                            Train train = new Train();

                            List<int> temp1 = new List<int>();

                            train.TrainId = tr;
                            train.PathOfTrain = p;
                            train.BoolConsolidation = 0;
                            for (int i = 0; i < DataSet.nTransDemand; i++)
                            {
                                if (Convert.ToInt32(cplexMod1.GetValue(x[i][tr][p])) == 1)
                                {
                                    train.ActualLoad += DataSet.Volume[i];
                                    temp1.Add(i);
                                    if (DataSet.ArriveTEUStation[i] != DataSet.PathOrigin[p])
                                    {
                                        train.BoolConsolidation = 1;

                                    }
                                }
                            }
                            train.ActualLoadDemand = temp1;
                            train.StartTEUStation = DataSet.PathOrigin[p];
                            train.StartTime = DataSet.TrainPortTime[tr] - DataSet.time_2[DataSet.PathOrigin[p], DataSet.PathPort[p]];
                            train.ArrivePort = DataSet.TrainPort[tr];
                            train.ArrivePortTime = DataSet.TrainPortTime[tr];

                            train.DestinationOfTrain = DataSet.PathDestination[p];
                            train.ArriveDestinationTime = train.StartTime + DataSet.PathTime[p];
                            trains.Add(train);

                            id++;

                        }

                    }
                }



                foreach (var i in trains)
                {
                    Console.WriteLine(i);
                }
                GenerateDemandExcel(transDemands, trains);

                double totalFunctionBenefit = 0;
   
              

                foreach (var demand in transDemands)
                {
                    if (demand.TrainOfDemand >= 0)
                    {
                        totalFunctionBenefit += demand.Volume * demand.Income;//收入
                        totalFunctionBenefit -= DataSet.CostPerHour * demand.Volume * (demand.ArriveDestinationTime - demand.ArriveTEUStationTime + demand.WaitTime);                
                        if (demand.BoolConsolidation == 1)
                        {
                            totalFunctionBenefit -= DataSet.CommonTrainCostPerKMTEU * demand.Volume * DataSet.distance_1[demand.ArriveTEUStation, demand.ConsolidationCentre];
                        }
                    }
                }

                foreach (var train in trains)
                {
                    if (train.ActualLoad > 1)
                    {

                        totalFunctionBenefit -= DataSet.ChinaEuTrianCostPerKM * DataSet.PathDistance[train.PathOfTrain];
                        if (train.PathOfTrain < DataSet.DividePathPoint)
                        {
                            totalFunctionBenefit -= DataSet.CostPerConsolidationTrain;  
                        }
                        else
                        {
                            totalFunctionBenefit -= DataSet.CostPerDirectTrain;
                        }
                    }
                }
                Console.WriteLine("totalFunctionBenefit  cal again：" + totalFunctionBenefit);

                double totalFunctionWaitTime = 0;
               

                foreach (var demand in transDemands)
                {
                    if (demand.TrainOfDemand >= 0)
                    {
                        if (demand.ArriveTEUStation != DataSet.PathOrigin[demand.PathOfDemand])//
                        {
                            totalFunctionWaitTime += DataSet.Priority[demand.TransDemand_Id] * demand.Volume * (demand.StartChinaExpressTime - demand.ArriveTEUStationTime - DataSet.time_1[demand.ArriveTEUStation, demand.ConsolidationCentre]);

                        }
                        else//直达列车
                        {
                            totalFunctionWaitTime += DataSet.Priority[demand.TransDemand_Id] * demand.Volume * (demand.StartChinaExpressTime - demand.ArriveTEUStationTime);
                        }
                    }
                    else//没有匹配上
                    {
                        totalFunctionWaitTime += DataSet.Priority[demand.TransDemand_Id] * demand.Volume * DataSet.PenaltyTime;

                    }
                }
                Console.WriteLine("totalFunctionWaitTime cal  again：" + totalFunctionWaitTime);

            }
            else
            {
                Console.WriteLine("No feasible solution, please check data !");
            }
        }
        #endregion


        #region Export to excel
        public static void GenerateDemandExcel(List<TransDemand> transDemands, List<Train> trains)
        {
            DataSet ds = new DataSet();
            // Export the results to an Excel file
            IWorkbook workbook = null;
            FileStream fs = null;
            IRow row = null;
            ISheet sheet = null;
            ICell cell = null;
            ISheet sheet1 = null;

   
            workbook = new XSSFWorkbook();
            sheet = workbook.CreateSheet("demand information");
            IFont font = workbook.CreateFont();               
            font.Color = HSSFColor.Red.Index;
            //样式
            ICellStyle cellstyle = workbook.CreateCellStyle();
            //给样式设置字体
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
                //cell.SetCellValue(transDemands[i].ArriveTEUStation);
                cell.SetCellValue(DataSet.StringArriveTEUStation[transDemands[i].TransDemand_Id]);

                cell = row.CreateCell(3);  
                cell.SetCellValue(transDemands[i].ArriveTEUStationTime);
                cell = row.CreateCell(4);  
                cell.SetCellValue(transDemands[i].BoolConsolidation);
                if (transDemands[i].BoolConsolidation == 0)
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
                                         // cell.SetCellValue(transDemands[i].ArrivePort);
                cell.SetCellValue(DataSet.StringPathPort[transDemands[i].PathOfDemand]);
                cell = row.CreateCell(10);  
                cell.SetCellValue(transDemands[i].ArrivePortTime);
                cell = row.CreateCell(11);  
                cell.SetCellValue(transDemands[i].DueTime);
                cell = row.CreateCell(12);  
                                          //cell.SetCellValue(transDemands[i].DestinationOfDemand);
                cell.SetCellValue(DataSet.StringPathDestination[transDemands[i].PathOfDemand]);
                cell = row.CreateCell(13);  
                cell.SetCellValue(transDemands[i].ArriveDestinationTime);
            }



            sheet1 = workbook.CreateSheet("train information");
            int rowCount1 = trains.Count;
            int columnCount1 = 14;
            string[] Lietou1 = new string[14] { "train index", "actual load", "path", "O", "O", "O  time", "P", "P", "P time", "D", "D", "D time", " direct or consolidation", "demands loaded"
                };
            //设置列头  
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
                sheet1.SetColumnWidth(i, 2500);//
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
                cell.SetCellValue(string.Join(",", trains[i].ActualLoadDemand));
            }
            using (fs = File.OpenWrite("CPLEX CRE operating plan" + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString().Replace(":", "-") + ".xlsx"))
           
            {
                workbook.Write(fs);
            }
        }
        #endregion



    }
}
