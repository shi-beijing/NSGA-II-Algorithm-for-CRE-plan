using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSGA_II_Algorithm.models
{
    public class Path
    {
        public int Path_Id { get; set; }//path index
        public int PathOrigin { get; set; }
        public int PathPort { get; set; }
        public int PathDestination { get; set; }


        public int PathCost { get; set; }//

        public int PathTime { get; set; }//

        public int BoolConsolidation { get; set; }

    }
}
