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
            const int goalX = 3;
            const int goalY = 3;
            double gamma = .6;
            double learnRate = 0.1;
            int episodes = 1000000000;

            //DOWN, UP, LEFT, RIGHT
            char[,] environment = new char[height, width]
            {
                { 'f', 'f', 'f', 'f' },
                { 'f', 'h', 'f', 'h' },
                { 'f', 'f', 'f', 'h' },
                { 'h', 'f', 'f', 'g' }
            };

            double[,] reward = new double[height, width]
            {
                { 0, 0, 0, 0 },
                { 0, -1, 0, -1 },
                { 0, 0, 0, -1 },
                { -1, 0, 0, 10 }
            };

            double[,] QTable = new double[height * height, width];

            QTable = Train(environment, reward, QTable, goalY, goalX, gamma, learnRate, episodes);


            PrintQ(QTable);


            List<List<(int, int, int)>> bruh = new List<List<(int, int, int)>>();
            List<int> list = new List<int>();
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    bruh.Add(GetPossNextStates(i, j, environment));
                    list.Add((GetStateForQ(i, j)));
                }
            }

            Walk(0, 0, environment, QTable);



            //PrintMap(environment, 0, 3);

        }

        static List<(int, int, int)> GetPossNextStates(int curStateY, int curStateX, char[,] environment)
        {
            List<(int, int, int)> possNextStates = new List<(int, int, int)>();
            int xLeftBounds = 0;
            int xRightBounds = environment.GetLength(0) - 1;
            int yUpperBounds = 0;
            int yLowerBounds = environment.GetLength(1) - 1;

            //LEFT = 2
            if (curStateY >= 0 && curStateX - 1 >= xLeftBounds) { possNextStates.Add((curStateY, curStateX - 1, 2)); }
            //RIGHT = 3
            if (curStateY >= 0 && curStateX + 1 <= xRightBounds) { possNextStates.Add((curStateY, curStateX + 1, 3)); }
            //UP = 0
            if (curStateY - 1 >= 0 && curStateX >= 0) { possNextStates.Add((curStateY - 1, curStateX, 0)); }
            //DOWN = 1
            if (curStateY + 1 <= yLowerBounds && curStateX >= 0) { possNextStates.Add((curStateY + 1, curStateX, 1)); }
         
            return possNextStates;
        }

        static (int, int, int) GetRandNextState(List<(int, int, int)> possibleStates)
        {
            int count = possibleStates.Count;
            int nextState = rnd.Next(0, count);
            return possibleStates[nextState];
        }

        static double[,] Train(char[,] environment, double[,] reward, double[,] QTable, int goalY, int goalX, double gamma, double learnRate, int episodes)
        {
            for(int episode = 0; episode < episodes; episode++)
            {
                int curStateY = 0;
                int curStateX = 0;
                while (true)
                {
                    var possNextStates = GetPossNextStates(curStateY, curStateX, environment);
                    var nextState = GetRandNextState(possNextStates);

                    var possNextNextStates = GetPossNextStates(nextState.Item1, nextState.Item2, environment);

                    var qState = GetStateForQ(curStateY, curStateX);
                    var qNextState = GetStateForQ(nextState.Item1, nextState.Item2);

                    var maxQ = double.MinValue;
                    //get q max of next state poss values
                    for (int i = 0; i < possNextNextStates.Count; i++)
                    {
                        var possStates = possNextNextStates[i];
                        var q = QTable[qNextState, possStates.Item3];
                        if (q > maxQ) { maxQ = q; }
                    }
                    
                    QTable[qState, nextState.Item3] 
                        = QTable[qState, nextState.Item3] * (1 - learnRate) + learnRate * (reward[nextState.Item1, nextState.Item2] + gamma * maxQ);

                    curStateY = nextState.Item1;
                    curStateX = nextState.Item2;
                    if(environment[curStateY, curStateX] == 'h') { break; }
                    if(environment[curStateY, curStateX] == 'g') { break; }
                }
            }
            return QTable;
        }

        static int GetStateForQ(int curY, int curX)
        {
            return ((curY + 1) * 4) - (4 - (curX + 1)) - 1;
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

        
        static void Walk(int curY, int curX, char[,] environment, double[,] QTable)
        {
            bool goalReached = false;
            while(goalReached != true)
            {
                Thread.Sleep(200);
                var curStateQ = GetStateForQ(curY, curX);
                var possNextStates = GetPossNextStates(curY, curX, environment);
                var maxQ = double.MinValue;
                int actionToTake = 0;

                foreach(var state in possNextStates)
                {
                    var curAction = state.Item3;
                    if (QTable[curStateQ, curAction] > maxQ)
                    {
                        maxQ = QTable[curStateQ, curAction];
                        actionToTake = curAction;
                    }
                }
                
                if (actionToTake == 0) { curY--; }
                if (actionToTake == 1) { curY++; }
                if (actionToTake == 2) { curX--; }
                if (actionToTake == 3) { curX++; }

                PrintMap(environment, curY, curX);

                if (curY == 3 && curX == 3)
                {
                    goalReached = true;
                }
            }
            PrintQ(QTable);

        }
        static void PrintQ(double[,] QTable)
        {
            for (int i = 0; i < QTable.GetLength(0); i++)
            {
                Console.Write("STATE : " + i + "| ");
                for (int j = 0; j < QTable.GetLength(1); j++)
                {
                    Console.Write(QTable[i, j] + ", ");
                }
                Console.WriteLine();
            }
        }
    }
}
