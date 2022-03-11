using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSGA_II_Algorithm.models
{
    public class TransDemand
    {
        public int TransDemand_Id { get; set; }
        public int Volume { get; set; }
        public int DueTime { get; set; }

        public int Income { get; set; }

        public int WaitTime { get; set; }
        public int ArriveTEUStation { get; set; }
        public int ArriveTEUStationTime { get; set; }

        public int EarliestTEUStationTime { get; set; }
        public int StartChinaExpressTime { get; set; }
        public int ArrivePortTime { get; set; }

        public int ArrivePort { get; set; }

        public int TrainOfDemand { get; set; }

        public int PathOfDemand { get; set; }

        public int BoolConsolidation { get; set; }
        public int ConsolidationCentre { get; set; }
        public int ArriveConsolidationTime { get; set; }

        public int DestinationOfDemand { get; set; }
        public int ArriveDestinationTime { get; set; }
        public double  Priority { get; set; }
        public override string ToString()
        {
            return string.Format("TransDemand_Id: {0},Volume: {1},ArriveTEUStation: {2},ArriveTEUStationTime: {3},  " +
                " BoolConsolidation: {4},ConsolidationCentre: {5},ArriveConsolidationTime: {6},TrainOfDemand: {7},StartChinaExpressTime: {8}," +
                "ArrivePort: {9},ArrivePortTime: {10},DueTime: {11},DestinationOfDemand: {12},ArriveDestinationTime: {13}",
                TransDemand_Id, Volume, ArriveTEUStation, ArriveTEUStationTime,
                BoolConsolidation, ConsolidationCentre, ArriveConsolidationTime, TrainOfDemand, StartChinaExpressTime,
                ArrivePort, ArrivePortTime, DueTime, DestinationOfDemand, ArriveDestinationTime);
        }

    }
}
