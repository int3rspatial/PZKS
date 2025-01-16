using Lab_1;

internal class Program
{
    private static void Main(string[] args)
    {
        PythonRuntimeControler prc = new PythonRuntimeControler();

        //_______________ Lab 1 _______________
        string? expression = null;
        while (expression == null || expression == string.Empty)
        {
            Console.Write("\nEnter your expression -> ");
            expression = Console.ReadLine();
        }

        Tokenizer tokenizer = new Tokenizer();
        tokenizer.Tokenize(expression);
        List<Error> errors = tokenizer.CheckForErrors();

        //tokenizer.PrintTokens();

        ShowErrors(errors, expression);
        if(errors.Count > 0) { prc.Shutdown(); return; }

        //_______________ Lab 2 _______________

        string checkedExpression = tokenizer.TokensToString();
        string expandedExpression = prc.ExpandExpression(checkedExpression);

        expandedExpression = expandedExpression.Replace(" ", "");
        tokenizer.TokenizeExpandedExpression(checkedExpression); // <-- checked

        if (tokenizer.Errors.Count > 0)
        {
            ShowErrors(tokenizer.Errors, expandedExpression);
            prc.Shutdown();
            return;
        }

        //Console.WriteLine("\nExpanded expression --> {0}\n", tokenizer.TokensToString());
        Console.WriteLine("\nOriginal expression --> {0}\n", tokenizer.TokensToString());

        Tree tree = new Tree();
        tree.CreateTree(tokenizer.Tokens); // <-- checked
        Console.WriteLine("Expression tree:\n");
        tree.PrintTree(tree.RootNode);

        //_______________ Lab 3 _______________
        (int nodes, string expr) = AssociativeLaw.Associate(prc, checkedExpression);

        //_______________ Lab 4 _______________
        List<string> commutativeResults = new List<string>();
        tokenizer.Tokenize(checkedExpression);
        CommutativeLaw.Commutate(tokenizer.Tokens, ref commutativeResults);

        //_______________ Lab 5 _______________
        Console.WriteLine("\nExpression for Gantt Chart --> {0}", checkedExpression);
        GanttChart.CreateChart(tree.OperationTree(), true);

        tokenizer.TokenizeExpandedExpression(expr);
        tree.CreateTree(tokenizer.Tokens);
        Console.WriteLine("\nAssociative Law for Gantt Chart --> {0}", expr);
        GanttChart.CreateChart(tree.OperationTree());

        Console.WriteLine("\n >> Gantt charts for commutative law <<\n");
        foreach (var item in commutativeResults)
        {
            Console.WriteLine("\nExpression --> {0}", item);
            tokenizer.TokenizeExpandedExpression(item);
            tree.CreateTree(tokenizer.Tokens);
            GanttChart.CreateChart(tree.OperationTree());
            Console.WriteLine();
        }

        prc.Shutdown();
    }

    public static void ShowErrors(List<Error> errors, string expression)
    {
        if (errors.Count > 0)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("\nError list:");
            Console.BackgroundColor = ConsoleColor.Black;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Number of errors: {0}", errors.Count);
            Console.ForegroundColor = ConsoleColor.White;

            foreach (var item in errors)
            {
                if (item.Position != -1)
                {
                    Console.WriteLine("\n----------------------------------------");
                    Console.WriteLine(expression);

                    (int left, int top) = Console.GetCursorPosition();
                    Console.SetCursorPosition(item.Position, top);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("^");
                    Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine(item.ToString());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("--> {0}", item.ToString());
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nExpression successfully passed syntax validation");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}