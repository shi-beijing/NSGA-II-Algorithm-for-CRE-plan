using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSGA_II_Algorithm.models
{
    /// <summary>
    /// Model used in CrowdingSort containing a chromosome and the crowding distance
    /// </summary>
    public class CrowdingDistanceAtom
    {
        private TrainsPlan _trPlan;
        private double _crowdingDistance;
        private double _crowdingDistance1; //Crowding distance under goal 1
        private double _crowdingDistance2;//Crowding distance under goal 2

        public CrowdingDistanceAtom(TrainsPlan trPlan)
        {
            _trPlan = trPlan;
            _crowdingDistance = 0;
        }

        public TrainsPlan trPlan
        {
            get => _trPlan;
            set => _trPlan = value;
        }

        public double CrowdingDistance
        {
            get => _crowdingDistance;
            set => _crowdingDistance = value;
        }

        public double CrowdingDistance1
        {
            get => _crowdingDistance1;
            set => _crowdingDistance1 = value;
        }
        public double CrowdingDistance2
        {
            get => _crowdingDistance2;
            set => _crowdingDistance2 = value;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"CrowdingDistanceAtom:\n");
            sb.Append($"\t{trPlan}\n");
            sb.Append($"\tCrowding Distance: {_crowdingDistance}");
            return sb.ToString();
        }

        public static List<CrowdingDistanceAtom> MapFromChromosomes(List<TrainsPlan> list)
        {
            return list.Select(trPlan => new CrowdingDistanceAtom(trPlan)).ToList();
        }

        public static List<TrainsPlan> MapToChromosomes(List<CrowdingDistanceAtom> list)
        {
            return list.Select(crowd => crowd.trPlan).ToList();
        }
    }
}
