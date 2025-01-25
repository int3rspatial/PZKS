using Lab1;
using Lab2;
using Lab3;
using Lab4;
using Lab5;

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
        //tokenizer.PrintTokens();

        tokenizer.CheckForErrors();
        tokenizer.ShowErrors();

        if (tokenizer.Errors.Count > 0)
        {
            prc.Shutdown();
            return;
        }

        //_______________ Lab 2 _______________
        string checkedExpression = tokenizer.TokensToString();
        string expandedExpression = prc.ExpandExpression(checkedExpression);

        tokenizer.TokenizeExpandedExpression(expandedExpression);
        //tokenizer.TokenizeExpandedExpression(checkedExpression); // <-- checked expr, for lab 6

        if (tokenizer.Errors.Count > 0)
        {
            tokenizer.ShowErrors();
            prc.Shutdown();
            return;
        }

        Console.WriteLine("\nExpanded expression --> {0}\n", tokenizer.TokensToString());
        //Console.WriteLine("\nOriginal expression --> {0}\n", tokenizer.TokensToString()); // <-- for lab 6

        Tree tree = new Tree();
        tree.CreateTree(tokenizer.Tokens);
        Console.WriteLine("Expression tree:\n");
        tree.PrintTree(tree.RootNode);

        //_______________ Lab 3 _______________
        string associativeLawExpr = AssociativeLaw.Associate(prc, checkedExpression);

        //_______________ Lab 4 _______________
        List<string> commutativeResults = [];
        tokenizer.Tokenize(checkedExpression);
        CommutativeLaw.Commutate(tokenizer.Tokens, ref commutativeResults);

        //_______________ Lab 5 _______________
        Console.WriteLine("\nExpression for Gantt Chart --> {0}", checkedExpression);
        GanttChart.CreateChart(tree.OperationTree(), true);

        //_______________ Lab 6 _______________
        tokenizer.TokenizeExpandedExpression(associativeLawExpr);
        tree.CreateTree(tokenizer.Tokens);
        Console.WriteLine("\nAssociative Law for Gantt Chart --> {0}", associativeLawExpr);
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
}