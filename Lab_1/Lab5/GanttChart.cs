using Lab2;

namespace Lab5
{
    public static class GanttChart
    {
        private static Dictionary<int, TreeNode> _memoryReads;
        private static List<string> _firstLayer;
        private static List<string> _secondLayer;
        private static Dictionary<int, TreeNode> _memoryWrites;
        private static List<TreeNode> _operationOrder;

        //Symbol for unavailable (n/a) clock cycles
        private static string _naSymbol;
        static GanttChart()
        {
            _memoryReads = new();
            _firstLayer = new();
            _secondLayer = new();
            _memoryWrites = new();
            _operationOrder = new();
            _naSymbol = "#";
        }
        public static void CreateChart(Dictionary<int, List<TreeNode>> operations, bool isPrint = false)
        {
            /*out of lists of operations for each layer of the tree
            we make one list with all of them, sorted by operation cost*/
            foreach (var item in operations.Reverse())
            {
                var sortedNodes = item.Value.OrderBy(x => x.OperationCost);
                int mostExpensiveOp = sortedNodes.Max(x => x.OperationCost);
                int leastExpensiveOp = sortedNodes.Min(x => x.OperationCost);
                if (leastExpensiveOp * 4 < mostExpensiveOp || leastExpensiveOp * 3 == mostExpensiveOp)
                {
                    sortedNodes = sortedNodes.OrderByDescending(x => x.OperationCost);
                }
                foreach (var node in sortedNodes)
                {
                    _operationOrder.Add(node);
                }
            }

            for (int i = 0; i < _operationOrder.Count; i++)
            {
                if (i == 0)
                {
                    _memoryReads.Add(i, _operationOrder[i]);
                    _firstLayer.Add(_naSymbol);
                    _secondLayer.Add(_naSymbol);

                    _firstLayer.AddRange(Enumerable.Range(0, _operationOrder[i].OperationCost)
                        .Select(x => $"{i}{_operationOrder[i].Symbol}").ToList());

                    _secondLayer.AddRange(Enumerable.Range(0, _operationOrder[i].OperationCost)
                        .Select(x => _naSymbol).ToList());
                    _secondLayer.AddRange(Enumerable.Range(0, _operationOrder[i].OperationCost)
                        .Select(x => $"{i}{_operationOrder[i].Symbol}").ToList());

                    _memoryWrites.Add(_secondLayer.Count, _operationOrder[i]);

                    i++;
                }

                if (_operationOrder[i].NodeLevel != _operationOrder[i - 1].NodeLevel)
                {
                    int lastWrite = LastChildrenWrite(_operationOrder[i]);

                    if (lastWrite == _firstLayer.Count || lastWrite == _firstLayer.Count - 1)
                    {
                        _firstLayer.AddRange(Enumerable.Range(0, _operationOrder[i - 1].OperationCost)
                            .Select(x => _naSymbol).ToList());

                        if (lastWrite == _firstLayer.Count - 1)
                        {
                            _firstLayer.Add(_naSymbol);
                            _secondLayer.Add(_naSymbol);
                        }

                        SimpleAddition(i, _operationOrder[i]);
                    }
                    else if (lastWrite == _secondLayer.Count)
                    {
                        _firstLayer.AddRange(Enumerable.Range(0, _operationOrder[i - 1].OperationCost + 2).Select(x => _naSymbol).ToList());
                        _secondLayer.AddRange(Enumerable.Range(0, 2).Select(x => _naSymbol).ToList());

                        SimpleAddition(i, _operationOrder[i]);
                    }
                    else if (lastWrite == -1 || lastWrite < _firstLayer.Count)
                    {
                        ComplexAddition(i, _operationOrder[i], _operationOrder[i - 1]);
                    }
                }

                if (_operationOrder[i].NodeLevel == _operationOrder[i - 1].NodeLevel)
                {
                    int lastWrite = LastChildrenWrite(_operationOrder[i]);
                    if (lastWrite == _firstLayer.Count - 1)
                    {
                        _firstLayer.AddRange(Enumerable.Range(0, _operationOrder[i - 1].OperationCost)
                            .Select(x => _naSymbol).ToList());

                        SimpleAddition(i, _operationOrder[i]);
                    }
                    else if (lastWrite == _firstLayer.Count)
                    {
                        _firstLayer.AddRange(Enumerable.Range(0, _operationOrder[i - 1].OperationCost)
                            .Select(x => _naSymbol).ToList());

                        //if (!_memoryReads.TryGetValue(_firstLayer.Count - 1, out _))
                        //{
                        //_firstLayer.Add($"{i}#");
                        //_secondLayer.Add($"{i}#");
                        //}

                        SimpleAddition(i, _operationOrder[i]);
                    }
                    else
                    {
                        ComplexAddition(i, _operationOrder[i], _operationOrder[i - 1]);
                    }
                }

            }

            if (isPrint)
            {
                PrintChart();
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Performance metrics:");
            Console.WriteLine("Sequential execution time: {0}", OneLayerWorkTime() * 2 + 2);
            Console.WriteLine("Execution time: {0}", _memoryWrites.Last().Key + 1);
            AccelerationFactor();
            EfficiencyFactor();
            Console.ForegroundColor = ConsoleColor.White;

            _memoryReads.Clear();
            _firstLayer.Clear();
            _secondLayer.Clear();
            _memoryWrites.Clear();
            _operationOrder.Clear();
        }
        private static void EfficiencyFactor()
        {
            double workTime = OneLayerWorkTime() * 2;
            int execTime = _memoryWrites.Last().Key + 1;
            double unusedComputeNodes = (execTime - _firstLayer.Count) + (execTime - _secondLayer.Count);
            double factor = Math.Round(workTime / (execTime * 2 - unusedComputeNodes), 2);
            Console.WriteLine("Efficiency factor: {0}", factor);
        }
        private static void AccelerationFactor()
        {
            double sequentialTime = (OneLayerWorkTime() + 1) * 2;
            double factor = Math.Round(sequentialTime / (_memoryWrites.Last().Key + 1), 2);
            Console.WriteLine("Acceleration factor: {0}", factor);
        }
        private static double OneLayerWorkTime()
        {
            double workTime = 0;
            foreach (TreeNode item in _operationOrder)
            {
                workTime += item.OperationCost;
            }
            return workTime;
        }
        private static void PrintChart()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n >>> Gantt chart (kind of) <<<\n");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Layers:");
            PrettyOutput(GroupOperations(_firstLayer), 1);
            PrettyOutput(GroupOperations(_secondLayer), 2);

            Console.WriteLine();
        }
        private static void PrettyOutput(List<string> layer, int layerNumber)
        {
            Console.Write($"{layerNumber} -> ");
            foreach (string item in layer)
            {
                if (item.Contains('#'))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write(item);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (item.Contains('+'))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(item);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (item.Contains('-'))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(item);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (item.Contains('*'))
                {
                    Console.BackgroundColor = ConsoleColor.DarkMagenta;
                    Console.Write(item);
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                else if (item.Contains('/'))
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write(item);
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.Write(' ');
            }
            Console.ResetColor();
            Console.Write("\n");
        }
        private static List<string> GroupOperations(List<string> layer)
        {
            List<string> output = new List<string>();
            int number = 0;
            string buf = "#";
            foreach (string item in layer)
            {
                if (item != buf)
                {
                    if (number > 1)
                    {
                        output.Add(MakeOperationBlock(number, buf));
                        number = 0;
                    }
                    else if (number == 1)
                    {
                        if (buf == "#")
                        {
                            output.Add($"###");
                        }
                        else
                        {
                            output.Add(MakeOperationBlock(number, buf));
                        }
                        number = 0;
                    }
                    buf = item;
                    number++;
                }
                else if (item == _naSymbol)
                {
                    output.Add($"###");
                }
                else if (item == buf)
                {
                    number++;
                }
            }
            if (number > 1)
            {
                output.Add(MakeOperationBlock(number, buf));
            }
            else if (number == 1)
            {
                output.Add(MakeOperationBlock(number, buf));
            }
            return output;
        }
        private static string MakeOperationBlock(int reps, string op)
        {
            if (reps == 1)
            {
                if (op.Length == 2)
                    return $"{op} ";
                else
                    return op;
            }

            int inputLength = op.Length == 2 ? op.Length + 1 : op.Length;
            int emptySpaceLength = inputLength * reps + reps - 1 - 2 - inputLength;
            int leftSubPart = Convert.ToInt32(Math.Floor((double)emptySpaceLength / 2));
            int rightSubPart = emptySpaceLength - leftSubPart;
            leftSubPart = op.Length == 2 ? leftSubPart + 1 : leftSubPart;

            return "[" + new string(' ', leftSubPart) + op + new string(' ', rightSubPart) + "]";
        }
        private static void ComplexAddition(int index, TreeNode currentNode, TreeNode previousNode)
        {
            _memoryReads.Add(_firstLayer.Count - 1, currentNode);

            _firstLayer.AddRange(Enumerable.Range(0, currentNode.OperationCost)
                .Select(x => $"{index}{currentNode.Symbol}").ToList());

            if (currentNode.OperationCost < previousNode.OperationCost)
            {
                _firstLayer.AddRange(Enumerable.Range(0, previousNode.OperationCost - currentNode.OperationCost)
                    .Select(x => _naSymbol).ToList());
            }

            if (currentNode.OperationCost > previousNode.OperationCost)
            {
                _secondLayer.AddRange(Enumerable.Range(0, currentNode.OperationCost - previousNode.OperationCost)
                    .Select(x => _naSymbol).ToList());
            }

            _secondLayer.AddRange(Enumerable.Range(0, currentNode.OperationCost)
                .Select(x => $"{index}{currentNode.Symbol}").ToList());

            _memoryWrites.Add(_secondLayer.Count, currentNode);
        }
        private static void SimpleAddition(int index, TreeNode currentNode)
        {
            _memoryReads.Add(_firstLayer.Count - 1, currentNode);

            _firstLayer.AddRange(Enumerable.Range(0, currentNode.OperationCost)
                .Select(x => $"{index}{currentNode.Symbol}").ToList());

            _secondLayer.AddRange(Enumerable.Range(0, currentNode.OperationCost)
                .Select(x => _naSymbol).ToList());

            _secondLayer.AddRange(Enumerable.Range(0, currentNode.OperationCost)
                .Select(x => $"{index}{currentNode.Symbol}").ToList());

            _memoryWrites.Add(_secondLayer.Count, currentNode);
        }
        private static int LastChildrenWrite(TreeNode node)
        {
            int leftChildWI, rightChildWI;//left and right child write indexes
            leftChildWI = rightChildWI = -1;
            if (_memoryWrites.Any(x => x.Value.Equals(node.LeftChild)))
            {
                leftChildWI = _memoryWrites.First(x => x.Value.Equals(node.LeftChild)).Key;
            }
            if (_memoryWrites.Any(x => x.Value.Equals(node.RightChild)))
            {
                rightChildWI = _memoryWrites.First(x => x.Value.Equals(node.RightChild)).Key;
            }

            int lastWrite;
            if (leftChildWI == -1 && rightChildWI == -1)
            {
                lastWrite = -1;
            }
            else if (leftChildWI == -1)
            {
                lastWrite = rightChildWI;
            }
            else if (rightChildWI == -1)
            {
                lastWrite = leftChildWI;
            }
            else
            {
                lastWrite = leftChildWI > rightChildWI ? leftChildWI : rightChildWI;
            }

            return lastWrite;
        }
    }
}
