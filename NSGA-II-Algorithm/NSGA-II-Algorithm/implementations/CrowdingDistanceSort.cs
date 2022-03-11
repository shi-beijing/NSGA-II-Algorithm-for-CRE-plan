using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NSGA_II_Algorithm.interfaces;
using NSGA_II_Algorithm.models;

namespace NSGA_II_Algorithm.implementations
{
    public class CrowdingDistanceSort: ICrowdingDistanceSort
    {


        //Calculate the crowding distance of each individual of a certain level classification subset of the front
        public List<CrowdingDistanceAtom> Sort(List<CrowdingDistanceAtom> list)
        {
            //The crowding distance is calculated for each objective
            //First, the crowding distance of each individual under the cost goal is calculated
            var maxByTotalIncome = list.MaxBy(atom => atom.trPlan.FunctionOfTotalIncome).ToList();
            var minByTotalIncome = list.MinBy(atom => atom.trPlan.FunctionOfTotalIncome).ToList();
            var totalIncomeDifference = maxByTotalIncome.First().trPlan.FunctionOfTotalIncome -
                                minByTotalIncome.First().trPlan.FunctionOfTotalIncome;
            //At the maximum bycost, we set their distance infinitely to ensure that they are passed.


            foreach (var atom in maxByTotalIncome)
            {
                atom.CrowdingDistance1 = double.PositiveInfinity;
            }
            foreach (var atom in minByTotalIncome)
            {
                atom.CrowdingDistance1 = double.PositiveInfinity;
            }
            var sortedAtomsByTotalIncome = list.OrderBy(atom => atom.trPlan.FunctionOfTotalIncome).ToList();

            for (var i = 1; i < sortedAtomsByTotalIncome.Count - 1; ++i)
            {
                sortedAtomsByTotalIncome[i].CrowdingDistance1 =
                    (sortedAtomsByTotalIncome[i + 1].trPlan.FunctionOfTotalIncome -
                    sortedAtomsByTotalIncome[i-1 ].trPlan.FunctionOfTotalIncome) / totalIncomeDifference;

            }



            var maxByTotalWaitTime = list.MaxBy(atom => atom.trPlan.FunctionOfTotalWaitTime);
            var minByTotalWaitTime = list.MinBy(atom => atom.trPlan.FunctionOfTotalWaitTime);
            var totalWaitTimeDifference = maxByTotalWaitTime.First().trPlan.FunctionOfTotalWaitTime -
                              minByTotalWaitTime.First().trPlan.FunctionOfTotalWaitTime;
            // In the minimum bytime, we set their distance infinitely to ensure that they are passed.
            foreach (var atom in minByTotalWaitTime)
            {
                atom.CrowdingDistance2 = double.PositiveInfinity;
            }

            foreach (var atom in maxByTotalIncome)
            {
                atom.CrowdingDistance2 = double.PositiveInfinity;
            }

            var sortedAtomsByTotalWaitTime = list.OrderBy(atom => atom.trPlan.FunctionOfTotalWaitTime).ToList();

            for (var i = 1; i < sortedAtomsByTotalWaitTime.Count - 1; ++i)
            {

                sortedAtomsByTotalWaitTime[i].CrowdingDistance2 +=
                    (sortedAtomsByTotalWaitTime[i + 1].trPlan.FunctionOfTotalWaitTime -
                    sortedAtomsByTotalWaitTime[i-1 ].trPlan.FunctionOfTotalWaitTime) / totalWaitTimeDifference;//
            }

            //sum
            foreach (var item in list)
            {
                item.CrowdingDistance = item.CrowdingDistance1 + item.CrowdingDistance2;
            }
            return list.OrderByDescending(atom => atom.CrowdingDistance).ToList();//In descending order, individuals with larger crowding distance are better at the front
        }
    }
}
