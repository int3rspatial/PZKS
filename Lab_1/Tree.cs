namespace Lab_1
{
    public class Tree
    {
        private List<TokenType> _operators;
        public int NodeQuantity { get; private set; }
        public TreeNode RootNode { get; set; }

        private Dictionary<int, List<TreeNode>> _operations;

        public Tree()
        {
            _operators = new List<TokenType> {
                TokenType.AddSubtractOperation,
                TokenType.MultiplyDivideOperation,
                TokenType.Exponent
            };
            RootNode = new TreeNode();
            NodeQuantity = 0;
            _operations = new Dictionary<int, List<TreeNode>>();
        }

        public Dictionary<int, List<TreeNode>> OperationTree()
        {
            _operations.Clear();
            if (RootNode.IsOperation)
            {
                _operations.Add(0, new List<TreeNode>());
            }
            TraverseTree(RootNode, level:0);
            return _operations;
        }

        private void TraverseTree(TreeNode node, int level)
        {
            if (node != null)
            {
                if (!node.IsOperation)
                    return;

                if(!_operations.TryGetValue(level, out _))
                {
                    _operations.Add(level, new List<TreeNode>());
                }
                node.NodeLevel = level;
                _operations[level].Add(node);


                TraverseTree(node.LeftChild!, ++level);
                level--;

                TraverseTree(node.RightChild!, ++level);
            }
        }

        public void CreateTree(List<Token> tokens)
        {
            NodeQuantity = 0;
            RootNode = GenerateNode(tokens);
        }

        public void PrintTree(TreeNode node, int level = 0)
        {
            if (node.RightChild != null)
                PrintTree(node.RightChild, level + 1);
            Console.WriteLine(new string(' ', 6 * level) + $"-> [{node.Symbol}]");
            if (node.LeftChild != null)
                PrintTree(node.LeftChild, level + 1);
        }

        private TreeNode GenerateNode(List<Token> tokens)
        {
            List<Token> inputTokens = new List<Token>(tokens);
            OpenBrackets(inputTokens);

            foreach (TokenType operators in _operators)
            {
                int operatorIndex = AnalyzeOperators(operators, inputTokens);
                if (operatorIndex != -1)
                {
                    TreeNode node = new TreeNode(inputTokens[operatorIndex]);
                    NodeQuantity++;
                    TreeNode leftChild = GenerateNode(inputTokens.GetRange(0, operatorIndex));
                    TreeNode rightChild = GenerateNode(inputTokens.GetRange(operatorIndex + 1, inputTokens.Count - (operatorIndex + 1)));

                    node.LeftChild = leftChild;
                    node.RightChild = rightChild;

                    return node;
                }
            }
            NodeQuantity++;
            return new TreeNode(inputTokens[0].Value);
        }

        private void OpenBrackets(List<Token> tokens)
        {
            int openingBrackets = 0, closingBrackets = 0;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].TokenType == TokenType.OpeningBracket)
                    openingBrackets++;
                else if (tokens[i].TokenType == TokenType.ClosingBracket)
                    closingBrackets++;

                if (openingBrackets - closingBrackets == 0 && i != tokens.Count - 1)
                {
                    return;
                }
            }

            if (openingBrackets - closingBrackets == 0 && 
                tokens[0].TokenType == TokenType.OpeningBracket &&
                tokens[tokens.Count - 1].TokenType == TokenType.ClosingBracket)
            {
                tokens.RemoveAt(tokens.Count - 1);
                tokens.RemoveAt(0);
            }
        }

        private int AnalyzeOperators(TokenType currentOperators, List<Token> tokens)
        {
            List<int> operators = new List<int>();
            int brackets = 0;

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].TokenType == TokenType.OpeningBracket)
                    brackets++;
                else if (tokens[i].TokenType == TokenType.ClosingBracket)
                    brackets--;
                else if (tokens[i].TokenType == currentOperators && brackets == 0)
                    operators.Add(i);
            }

            if (operators.Count > 0)
            {
                int middleOperatorIndex = Convert.ToInt32(Math.Floor((double)operators.Count / 2));
                return operators[middleOperatorIndex];
            }

            return -1;
        }
    }
}
