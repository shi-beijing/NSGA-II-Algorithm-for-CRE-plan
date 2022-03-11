using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSGA_II_Algorithm.models
{
    /// <summary>
    /// Model used in NonDominatedSort containing a chromosome, dominationCount
    /// and the list of dominates
    /// </summary>
    public class NonDominatedSortAtom
    {
        private TrainsPlan _trPlan;
        private int _dominationCount;
        private List<NonDominatedSortAtom> _dominates;

        public NonDominatedSortAtom(TrainsPlan trPlan)
        {
            _trPlan = trPlan;
            _dominationCount = 0;
            _dominates = new List<NonDominatedSortAtom>();
        }

        public TrainsPlan trPlan
        {
            get => _trPlan;
            set => _trPlan = value;
        }

        public int DominationCount
        {
            get => _dominationCount;
            set => _dominationCount = value;
        }

        public List<NonDominatedSortAtom> Dominates
        {
            get => _dominates;
            set => _dominates = value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"NonDominatedSortAtom:\n");
            sb.Append($"\t{trPlan}\n");
            sb.Append($"\tDominationCount: {_dominationCount}");
            return sb.ToString();
        }

        public static List<NonDominatedSortAtom> MapFromChromosomes(List<TrainsPlan> list)//Convert chromosome's list to NonDominatedSortAtom's list
        {
            var a = list.Select(trPlan => new NonDominatedSortAtom(trPlan)).ToList(); 
            return a;
        }

        public static List<TrainsPlan> MapToChromosomes(List<NonDominatedSortAtom> list)//Convert NonDominatedSortAtom's list to chromosome's list
        {
            return list.Select(atom => atom.trPlan).ToList();
        }
    }
}
