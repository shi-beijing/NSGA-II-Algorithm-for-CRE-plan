using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NSGA_II_Algorithm.implementations;
using NSGA_II_Algorithm.models;



namespace NSGA_II_CLI
{
    internal class CLI
    {      

        private static void Main(string[] args)
        {
            // PrintTest();

            //var items = GetMockDataItems();
            //Start timing
            string path = "NSGA2-Console-Output" + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString().Replace(":", " - ") + ".txt";
            using (var cc = new ConsoleCopy(path))
            {

                DataSet.getData();
                System.Diagnostics.Stopwatch oTime = new System.Diagnostics.Stopwatch();   //timing obj
                oTime.Start();

                var nsgaAlgorithm = new NsgaAlgorithm(0.9, 0.3);
                var trPlans = nsgaAlgorithm.Process(100, 50, true);
                TrainsPlan.PrintList(trPlans);

                //int whichObject = 2;//=0Represents the minimization time = 1 represents the maximization profit = 2 represents the normalization linearization to a single objective            
                //CplexSolver.LinePlan(whichObject);


                Console.WriteLine("Press Enter ...");
                oTime.Stop(); //end   timing
                              //Output run time.  
                Console.WriteLine("Running time of the program：{0} seconds", oTime.Elapsed.TotalSeconds);
               
            }
            Console.WriteLine("Output completed");
            Console.ReadKey();

        }



        public class ConsoleCopy : IDisposable
        {

            private FileStream m_FileStream;
            private StreamWriter m_FileWriter;

            private readonly TextWriter m_DoubleWriter;
            private readonly TextWriter m_OldOut;

            private class DoubleWriter : TextWriter
            {

                private TextWriter m_One;
                private TextWriter m_Two;

                public DoubleWriter(TextWriter one, TextWriter two)
                {

                    m_One = one;
                    m_Two = two;
                }

                public override Encoding Encoding
                {

                    get
                    {
                        return m_One.Encoding;
                    }
                }

                public override void Flush()
                {

                    m_One.Flush();
                    m_Two.Flush();
                }

                public override void Write(char value)
                {

                    m_One.Write(value);
                    m_Two.Write(value);
                }
            }

            public ConsoleCopy(string path)
            {

                m_OldOut = Console.Out;

                try
                {

                    m_FileStream = File.Create(path);

                    m_FileWriter = new StreamWriter(m_FileStream)
                    {

                        AutoFlush = true
                    };

                    m_DoubleWriter = new DoubleWriter(m_FileWriter, m_OldOut);
                }
                catch (Exception e)
                {

                    Console.WriteLine("Cannot open file for writing");
                    Console.WriteLine(e.Message);
                    return;
                }
                Console.SetOut(m_DoubleWriter);
            }

            public void Dispose()
            {

                Console.SetOut(m_OldOut);

                if (m_FileWriter != null)
                {

                    m_FileWriter.Flush();
                    m_FileWriter.Close();
                    m_FileWriter = null;
                }
                if (m_FileStream != null)
                {

                    m_FileStream.Close();
                    m_FileStream = null;
                }
            }
        }//Let the program log output to the console and TXT.


    }
}