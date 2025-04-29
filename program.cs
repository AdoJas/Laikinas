using System;
using System.Collections.Generic;

namespace BoxOptimization
{
    class Program
    {
        static double ObjectiveFunction(double X1, double X2, double X3)
        {
            return -X1 * X2 * X3;
        }

        static double EqualityConstraint(double X1, double X2, double X3) // gi
        {
            return 2 * (X1 * X2 + X2 * X3 + X1 * X3) - 1;
        }

        static double[] InequalityConstraint(double X1, double X2, double X3) // hi
        {
            return new double[]{-X1, -X2, -X3};
        }

        static double PenaltyFunction(double X1, double X2, double X3, double r)
        {
            double objectiveValue = ObjectiveFunction(X1, X2, X3);

            double penalty = Math.Pow(EqualityConstraint(X1, X2, X3), 2); 
            
            foreach(double h in  InequalityConstraint(X1, X2, X3))
            {
                penalty += Math.Pow(Math.Max(0, h), 2);
            }
            
            return objectiveValue + 1.0 / r * penalty;
        }

        static double[] PenaltyFunctionGradient(double X1, double X2, double X3, double r)
        {
            double gradObjectiveX1 = -X2 * X3;
            double gradObjectiveX2 = -X1 * X3;
            double gradObjectiveX3 = -X1 * X2;
            
            double equalityConstraint = EqualityConstraint(X1, X2, X3);
            double gradEqualityX1 = 4 * equalityConstraint * (X2 + X3);
            double gradEqualityX2 = 4 * equalityConstraint * (X1 + X3);
            double gradEqualityX3 = 4 * equalityConstraint * (X1 + X2);
            
            double[] inequalityConstraints = InequalityConstraint(X1, X2, X3);
            double gradInequalityX1 = 0;
            double gradInequalityX2 = 0;
            double gradInequalityX3 = 0;

            if (inequalityConstraints[0] > 0) 
                gradInequalityX1 = -2 * inequalityConstraints[0];
            if (inequalityConstraints[1] > 0)
                gradInequalityX2 = -2 * inequalityConstraints[1];
            if (inequalityConstraints[2] > 0)
                gradInequalityX3 = -2 * inequalityConstraints[2];

            double gradX1 = gradObjectiveX1 + (1.0 / r) * (gradEqualityX1 + gradInequalityX1);
            double gradX2 = gradObjectiveX2 + (1.0 / r) * (gradEqualityX2 + gradInequalityX2);
            double gradX3 = gradObjectiveX3 + (1.0 / r) * (gradEqualityX3 + gradInequalityX3);

            return new double[] { gradX1, gradX2, gradX3 };
        }
        
        public static (double X1, double X2, double X3, int numIterations, int numFunctionEvaluations) GradientDescent(
            double X1, double X2, double X3,
            double learningRate,
            double tolerance,
            double r,
            int maxIterations = 1000)
        {
            int iterations = 0;
            int functionEvaluations = 0;

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                double[] grad = PenaltyFunctionGradient(X1, X2, X3, r);
                functionEvaluations++;

                if (Math.Sqrt(grad[0] * grad[0] + grad[1] * grad[1] + grad[2] * grad[2]) < tolerance) // Sustojam jei gradiento norma pakankamai maza
                {
                    break;
                }

                double newX1 = X1 - learningRate * grad[0];
                double newX2 = X2 - learningRate * grad[1];
                double newX3 = X3 - learningRate * grad[2];
                
                if (Math.Sqrt(Math.Pow(newX1 - X1, 2) + Math.Pow(newX2 - X2, 2) + Math.Pow(newX3 - X3, 2)) < tolerance) // jei pokytis mazesnis nei tolerancija, stabdom
                {
                    break;
                }
                
                X1 = newX1;
                X2 = newX2;
                X3 = newX3;
                
                iterations++;
            }

            return (X1, X2, X3, iterations, functionEvaluations);
        }
        static void ExplorePenaltyMultiplierInfluence(string pointName, double x1, double x2, double x3)
        {
            Console.WriteLine("\n  Baudos funkcijos P(X, r) = f(X) + (1/r) * b(X) reikšmės kintant r:");
            Console.WriteLine("  ------------------------------------------------------------------------------------");
            Console.WriteLine("  r  | Visa Baudos Funkcija P(X,r)");
            Console.WriteLine("  ------------------------------------------------------------------------------------");

            double[] rValuesToExplore = { 10.0, 9.0, 8.0, 7.0, 6.0, 5.0, 4.0, 3.0, 2.0, 1.0, 0.5, 0.1, 0.05, 0.01 };

            foreach (double r in rValuesToExplore)
            {
                double penaltyFunctionValue = PenaltyFunction(x1, x2, x3, r);
                Console.WriteLine($"  {r} | {penaltyFunctionValue}");
            }
        }
        static void Main(string[] args)
        {
            int a = 9, b = 7, c = 7;

            var startPoints = new List<(string Name, double X1, double X2, double X3)>
            {
                ("X0", 0, 0, 0),
                ("X1", 1, 1, 1),
                ("Xm", a / 10.0, b / 10.0, c / 10.0)
            };
            
            ExplorePenaltyMultiplierInfluence(startPoints[1].Name, startPoints[1].X1, startPoints[1].X2, startPoints[1].X3);
            double[] rValues = { 10.0, 9.0, 8.0, 7.0, 6.0, 5.0, 4.0, 3.0, 2.0, 1.0, 0.5, 0.1, 0.05, 0.01 };
            double learningRate = 0.005;
            double tolerance = 1e-6;

            foreach (var (name, initX1, initX2, initX3) in startPoints)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"f(x) = {ObjectiveFunction(initX1, initX2, initX3)}, gi(x) = {EqualityConstraint(initX1, initX2, initX3)}, hi(x) = [{string.Join(", ", InequalityConstraint(initX1, initX2, initX3))}]");
                Console.WriteLine($"\nStarting optimization from {name}:");

                double x1 = initX1, x2 = initX2, x3 = initX3;
                double iterations = 0;
                double functionEvaluations = 0;
                foreach (double r in rValues)
                {
                    var result = GradientDescent(x1, x2, x3, learningRate, tolerance, r);

                    x1 = result.X1;
                    x2 = result.X2;
                    x3 = result.X3;

                    double f = ObjectiveFunction(x1, x2, x3);
                    double penalty = PenaltyFunction(x1, x2, x3, r);
                    Console.WriteLine($"r={r:F2} -> x=({x1:F6}, {x2:F6}, {x3:F6}), Turis={-f:F6}, BaudosFunkcija={penalty:F6}, Iterations={result.numIterations}, Function Evaluations={result.numFunctionEvaluations}");
                    iterations += result.numIterations;
                    functionEvaluations += result.numFunctionEvaluations;
                }
                Console.WriteLine($"Total iterations= {iterations}, Total function evaluations= {functionEvaluations}");
            }
        }
    }
}
