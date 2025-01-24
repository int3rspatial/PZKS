using Lab1;
using Lab2;

namespace Lab3
{
    public static class AssociativeLaw
    {
        private enum StepType
        {
            Original, Expand, Apart, Simplify, SeparateVars, Condense, ApartSepvarsTogether, Collect
        }

        private static Tokenizer _tokenizer;
        private static Tree _tree;
        private static int _nodeQuantity;
        private static string _bestExpression;
        private static string _mostCommonVar;

        private static List<string> _mostCommonVars;

        static AssociativeLaw()
        {
            _tokenizer = new Tokenizer();
            _tree = new Tree();
            _nodeQuantity = 0;
            _bestExpression = "";
            _mostCommonVar = "";

            _mostCommonVars = new List<string>();
        }
        public static string Associate(PythonRuntimeControler prc, string expression)
        {
            List<StepType> steps = new List<StepType>()
            {
                StepType.Original, StepType.Expand, StepType.Apart,
                StepType.Simplify, StepType.SeparateVars, StepType.Condense,
                StepType.ApartSepvarsTogether, StepType.Collect
            };
            Dictionary<string, int> dict = new Dictionary<string, int>();

            foreach (StepType step in steps)
            {
                switch (step)
                {
                    case StepType.Original:
                        {
                            _nodeQuantity = expression.Length + 1;
                            TokenizeAndBuildTree(expression);
                            foreach (Token token in _tokenizer.Tokens)
                            {
                                if (token.TokenType == TokenType.Variable)
                                {
                                    if (dict.TryGetValue(token.Value, out _))
                                    {
                                        dict[token.Value]++;
                                    }
                                    else
                                    {
                                        dict.Add(token.Value, 1);
                                    }
                                }
                                else if (token.TokenType == TokenType.UnaryOpVariable)
                                {
                                    string buf = token.Value[1..];
                                    if (dict.TryGetValue(buf, out _))
                                    {
                                        dict[buf]++;
                                    }
                                    else
                                    {
                                        dict.Add(buf, 1);
                                    }
                                }
                            }
                            var sortedDict = dict.OrderByDescending(x => x.Value);
                            _mostCommonVar = sortedDict.First().Key;
                            _mostCommonVars = sortedDict.Where(x => x.Value != 1).Select(x => x.Key).ToList();
                            break;
                        }
                    case StepType.Expand:
                        {
                            TokenizeAndBuildTree(prc.ExpandExpression(expression));
                            break;
                        }
                    case StepType.Apart:
                        {
                            TokenizeAndBuildTree(prc.ApartVarInExpression(expression, _mostCommonVar));
                            break;
                        }
                    case StepType.Simplify:
                        {
                            TokenizeAndBuildTree(prc.SimplifyExpression(expression));
                            break;
                        }
                    case StepType.SeparateVars:
                        {
                            TokenizeAndBuildTree(prc.SeparateVarsInExpression(expression));
                            break;
                        }
                    case StepType.Condense:
                        {
                            TokenizeAndBuildTree(prc.CondenseExpression(expression));
                            break;
                        }
                    case StepType.ApartSepvarsTogether:
                        {
                            TokenizeAndBuildTree(prc.ApartSepvarsTogether(expression, _mostCommonVar));
                            break;
                        }
                    case StepType.Collect:
                        {
                            foreach (var item in _mostCommonVars)
                            {
                                TokenizeAndBuildTree(prc.CollectVarsInExpression(expression, item));
                            }
                            break;
                        }
                }
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n >>> Associative transformation <<<");
            Console.ForegroundColor = ConsoleColor.White;

            _bestExpression = _bestExpression.Replace("**", "^");
            Console.Write("Result --> {0}\n", _bestExpression);

            return _bestExpression;
        }

        private static void TokenizeAndBuildTree(string expression)
        {
            _tokenizer.TokenizeExpandedExpression(expression.Replace(" ", ""));
            _tree.CreateTree(_tokenizer.Tokens);
            if (_tree.NodeQuantity < _nodeQuantity)
            {
                _bestExpression = expression;
                _nodeQuantity = _tree.NodeQuantity;
            }
        }
    }
}
