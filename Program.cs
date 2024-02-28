using System;
using System.Runtime.ExceptionServices;
namespace Game1
{
    internal class Program
    {
        static Random rnd = new Random(1);
        static void Main(string[] args)
        {
            const int height = 4;
            const int width = 4;
            char[,] environment = new char[height, width]
            {
                { 'f', 'f', 'f', 'f' },
                { 'f', 'h', 'f', 'h' },
                { 'f', 'f', 'f', 'h' },
                { 'h', 'f', 'f', 'g' }
            };

            double[,] reward = new double[height, width]
            {
                { 0, 0, 0, 0, },
                { 0, -.1, 0, -.1},
                { 0, 0, 0, -.1},
                { -.1, 0, 0, 1}
            };
            /*
            
            Q TABLE 
                ACTIONS = UP, DOWN, LEFT, RIGHT
         S   |-----------------------------------
         T   |   
         A   |   
         T   |   
         E   |   
         S   |   

            */

            double[,] Q = new double[height * height, width];


            Console.WriteLine(Q.ToString());

            PrintMap(environment, 0, 3);

        }

        static List<(int , int)> GetPossNextStates(int curStateY, int curStateX, char[,] environment)
        {
            List<(int, int)> possNextStates = new List<(int, int)>();
            int xLeftBounds = 0;
            int xRightBounds = environment.GetLength(0) - 1;
            int yUpperBounds = 0;
            int yLowerBounds = environment.GetLength(1) - 1;


            if (curStateY >= 0 && curStateX - 1 >= xLeftBounds) { possNextStates.Add((curStateY, curStateX - 1)); }
            if (curStateY >= 0 && curStateX + 1 <= xRightBounds) { possNextStates.Add((curStateY, curStateX + 1)); }
            if (curStateY - 1 >= yUpperBounds && curStateX >= 0) { possNextStates.Add((curStateY - 1, curStateX)); }
            if (curStateY + 1 <= yLowerBounds && curStateX >= 0) { possNextStates.Add((curStateY + 1, curStateX)); }
         

            return possNextStates;
        }

        static (int, int) GetRandNextState(int curStateY, int curStateX, char[,] environment)
        {
            var possNextStates = GetPossNextStates(curStateY, curStateX, environment);
            int count = possNextStates.Count;
            int nextState = rnd.Next(0, count);
            return possNextStates[nextState];
        }

        static void Train(char[,] environment, double[,] reward, double[,] Q, int goalY, int goalX, double gamma, double learnRate, int episodes)
        {
            for(int episode = 0; episode < episodes; episode++)
            {
                int curStateY = 0;
                int curStateX = 0;
                while (true)
                {
                    var nextState = GetRandNextState(curStateY, curStateX, environment);
                    var possNextStates = GetPossNextStates(nextState.Item1, nextState.Item2, environment);
                    PrintMap(environment, curStateY, curStateX);


                    curStateY = nextState.Item1;
                    curStateX = nextState.Item2;
                    if(environment[curStateY, curStateX] == 'h') { break; }
                    if(environment[curStateY, curStateX] == 'g') { break; }
                }
            }
        }

        static void PrintMap(char[,] environment, int curY, int curX)
        {
            Console.Clear();
            for(int i = 0; i < environment.GetLength(0); i++)
            {
                for(int j = 0;  j < environment.GetLength(1); j++)
                {
                    if(i == curY && j == curX)
                    {
                        Console.Write("s" + " ");
                    }
                    else
                    {
                        Console.Write(environment[i, j] + " ");
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
