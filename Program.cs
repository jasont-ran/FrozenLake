using System;
using System.Runtime.ExceptionServices;
using Colorful;
using System.Drawing;
using Console = Colorful.Console;
namespace Game1
{
    internal class Program
    {
        static Random rnd = new Random();
        static void Main(string[] args)
        {
            const int height = 4;
            const int width = 4;
            const int goalX = 3;
            const int goalY = 3;
            double gamma = .6;
            double learnRate = 0.1;
            double epsilon = 1;
            int episodes = 1000;

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
                { -1, 0, 0, 1 }
            };

            double[,] QTable = new double[height * height, width];

            QTable = Train(environment, reward, QTable, goalY, goalX, gamma, learnRate, epsilon, episodes);

            Walk(0, 0, environment, QTable);


        }

        static List<(int Y, int X, int ActionState)> GetPossNextStates(int curStateY, int curStateX, char[,] environment)
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
            if (curStateY - 1 >= yUpperBounds && curStateX >= 0) { possNextStates.Add((curStateY - 1, curStateX, 0)); }
            //DOWN = 1
            if (curStateY + 1 <= yLowerBounds && curStateX >= 0) { possNextStates.Add((curStateY + 1, curStateX, 1)); }
         
            return possNextStates;
        }

        static (int, int, int) GetRandNextState(List<(int Y, int X, int ActionState)> possibleStates)
        {
            int count = possibleStates.Count;
            int nextState = rnd.Next(0, count);
            return possibleStates[nextState];
        }

        static double[,] Train(char[,] environment, double[,] reward, double[,] QTable, int goalY, int goalX, double gamma, double learnRate, double epsilon, int episodes)
        {
            double maxEpsilon = 1;
            var minEpsilon = .01;
            var epsilonDecay = 0.001;
            for(int episode = 0; episode < episodes; episode++)
            {
                int curStateY = 0;
                int curStateX = 0;
                bool done = false;
                while (!done)
                {
                    var action = (0, 0, 0);
                    var random = rnd.NextDouble();

                    if (random < epsilon)
                    {
                        var possNextStates = GetPossNextStates(curStateY, curStateX, environment);
                        var nextState = GetRandNextState(possNextStates);
                        action = nextState;
                    }
                    else
                    {
                        var curQState = GetStateForQ(curStateY, curStateX);
                        var nextState = GetMaxNextState(curStateY, curStateX, environment, QTable);
                        action = nextState;
                    }

                    var possNextNextStates = GetPossNextStates(action.Item1, action.Item2, environment);

                    var qState = GetStateForQ(curStateY, curStateX);
                    var qNextState = GetStateForQ(action.Item1, action.Item2);

                    var maxQ = double.MinValue;
                    //get q max of next state poss values
                    for (int i = 0; i < possNextNextStates.Count; i++)
                    {
                        var possStates = possNextNextStates[i];
                        var q = QTable[qNextState, possStates.Item3];
                        if (q > maxQ) { maxQ = q; }
                    }
                    
                    QTable[qState, action.Item3] 
                        = QTable[qState, action.Item3] * (1 - learnRate) + learnRate * (reward[action.Item1, action.Item2] + gamma * maxQ);

                    epsilon = minEpsilon + (maxEpsilon - minEpsilon) * Math.Exp(-epsilonDecay * episode);
                    

                    curStateY = action.Item1;
                    curStateX = action.Item2;
                    if(environment[curStateY, curStateX] == 'h') { done = true; }
                    if(environment[curStateY, curStateX] == 'g') { done = true; }
                }
            }
            return QTable;
        }

        static int GetStateForQ(int curY, int curX)
        {
            return ((curY + 1) * 4) - (4 - (curX + 1)) - 1;
        }

        static (int Y, int X, int ActionState) GetMaxNextState(int curY, int curX, char[,] environment, double[,] Q)
        {
            var possNextStates = GetPossNextStates(curY, curX, environment);
            var max = double.MinValue;
            var maxQState = (0, 0, 0);
            var qState = GetStateForQ(curY, curY);
            foreach (var state in possNextStates)
            {
                var qValue = Q[qState, state.ActionState];
                if(qValue > max) 
                {
                    max = qValue;
                    maxQState = state;
                }
            }

            return maxQState;
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
                        Console.Write("|   ", Color.Cyan);
                        Console.Write("s", Color.LightYellow);
                        Console.Write("   ", Color.Cyan);
                    }
                    else
                    {
                        Console.Write("|   ", Color.Cyan);
                        if (environment[i, j].Equals('h')) Console.Write(environment[i, j], Color.Red);
                        else if (environment[i, j].Equals('g')) Console.Write(environment[i, j], Color.Green);
                        else Console.Write(environment[i, j], Color.White);
                        Console.Write("   ", Color.Cyan);
                    }
                }
                Console.WriteLine();
                for(int k = 0; k < environment.GetLength(1); k++)
                {
                    Console.Write("-------", Color.Cyan);
                };
                Console.WriteLine();
            }

           
        }
        
        static void Walk(int curY, int curX, char[,] environment, double[,] QTable)
        {
            bool goalReached = false;
            bool holeReached = false;
            while(goalReached != true && holeReached != true)
            {
                Thread.Sleep(3000);
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
                if (environment[curY, curX] == 'h')
                {
                    holeReached = true;
                }
                if (environment[curY, curX] == 'g')
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
