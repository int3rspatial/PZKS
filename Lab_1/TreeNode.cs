namespace Lab_1
{
    public class TreeNode
    {
        public bool IsOperation { get; set; }
        public Operation OperationType { get; set; }
        public int NodeLevel { get; set; }
        public int OperationCost { get; set; }

        public string Symbol { get; set; }
        public TreeNode? LeftChild { get; set; }
        public TreeNode? RightChild { get; set; }

        public TreeNode(string symbol = "")
        {
            Symbol = symbol;
            LeftChild = null;
            RightChild = null;
            IsOperation = false;
            OperationType = Operation.None;
            NodeLevel = 0;
            OperationCost = 0;
        }
        public TreeNode(Token token)
        {
            LeftChild = RightChild = null;
            Symbol = token.Value;
            switch (token.TokenType)
            {
                case TokenType.AddSubtractOperation:
                    {
                        IsOperation = true;
                        if (token.Value == "+")
                        {
                            OperationType = Operation.Add;
                        }
                        else if (token.Value == "-")
                        {
                            OperationType = Operation.Subtract;
                        }
                        SetOperationCost();
                        break;
                    }
                case TokenType.MultiplyDivideOperation:
                    {
                        IsOperation = true;
                        if (token.Value == "*")
                        {
                            OperationType = Operation.Multiply;
                        }
                        else if (token.Value == "/")
                        {
                            OperationType = Operation.Divide;
                        }
                        SetOperationCost();
                        break;
                    }
                default:
                    IsOperation = false;
                    OperationType = Operation.None;
                    break;
            }
        }

        private void SetOperationCost()
        {
            // +,- = 1; * = 3; / = 10
            switch (OperationType)
            {
                case Operation.Add:
                    OperationCost = 1;
                    break;
                case Operation.Subtract:
                    OperationCost = 1;
                    break;
                case Operation.Multiply:
                    OperationCost = 3;
                    break;
                case Operation.Divide:
                    OperationCost = 10;
                    break;
            }
        }
    }
}
