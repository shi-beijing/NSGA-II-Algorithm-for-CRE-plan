using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSGA_II_Algorithm.models
{
    public class Train
    {
        public int TrainId { get; set; }
        public int BoolRun { get; set; }

        public int PathOfTrain { get; set; }
        public int StartTime { get; set; }



        public int ActualLoad { get; set; }
        public List<int> ActualLoadDemand { get; set; }
        public int StartTEUStation { get; set; }

        public int ArrivePort { get; set; }
        public int ArrivePortTime { get; set; }

        public int DestinationOfTrain { get; set; }
        public int ArriveDestinationTime { get; set; }

        public int BoolConsolidation { get; set; }//Whether the train is a transit train 0 means direct 1 means consolidation train
        public override string ToString()
        {
            return string.Format("TrainId: {0},ActualLoad: {1},PathOfTrain: {2},StartTEUStation: {3},StartTime: {4},ArrivePort: {5},ArrivePortTime: {6},DestinationOfTrain: {7},ArriveDestinationTime: {8},BoolConsolidation: {9},BoolRun: {10}",
                                 TrainId, ActualLoad, PathOfTrain, StartTEUStation, StartTime, ArrivePort, ArrivePortTime, DestinationOfTrain, ArriveDestinationTime, BoolConsolidation, BoolRun);
        }

    }
}
