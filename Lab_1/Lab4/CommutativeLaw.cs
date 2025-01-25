using System.Text;
using Lab1;

namespace Lab4
{
    public static class CommutativeLaw
    {
        public static void Commutate(List<Token> tokens, ref List<string> result)
        {
            List<Token> subExpressions = DivideIntoSubExpressions(tokens);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n >>> Commutative transformations <<<");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Number of subexpressions --> {0}", subExpressions.Count);

            Console.WriteLine("Expression parts:");
            foreach (Token token in subExpressions)
            {
                Console.BackgroundColor = ConsoleColor.DarkMagenta;
                Console.Write(token.Value);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");
            }
            Console.Write(" \n");

            if (subExpressions.Count > 20)
            {
                Console.WriteLine("\nThere are too many permutations, and I cannot even compute their number!");
                return;
            }

            long numberOfPermutations = Factorial(subExpressions.Count);

            long Factorial(long number)
            {
                if (number != 1)
                    return number * Factorial(number - 1);
                return 1;
            }

            Console.WriteLine("\nNumber of permutations --> {0}! = {1}", subExpressions.Count, numberOfPermutations);

            if (numberOfPermutations > 25)
                return;

            StringBuilder sb = new StringBuilder();

            List<string> variants = new List<string>();
            var permutationsList = Permute(subExpressions);
            foreach (var item in permutationsList)
            {
                foreach (var token in item)
                {
                    sb.Append(token.Value);
                }

                if (sb.ToString().StartsWith('+'))
                    sb.Remove(0, 1);

                variants.Add(sb.ToString());
                sb.Clear();
            }

            Console.WriteLine("Commutation variants:");
            for (int i = 0; i < variants.Count; i++)
            {
                Console.Write($" #{i + 1} >> ");
                Console.WriteLine(variants[i]);
            }

            result = variants;
        }
        private static List<Token> DivideIntoSubExpressions(List<Token> tokens)
        {
            if (tokens[0].TokenType != TokenType.UnaryOpVariable ||
                tokens[0].TokenType != TokenType.UnaryOpConstant)
            {
                tokens.Insert(0, new Token(TokenType.AddSubtractOperation, "+"));
            }

            List<Token> subExpressions = [];
            StringBuilder sb = new StringBuilder("");
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].TokenType == TokenType.AddSubtractOperation)
                {
                    sb.Append(tokens[i].Value);

                    int j = i + 1, brackets = 0;
                    while (tokens[j].TokenType != TokenType.AddSubtractOperation || brackets != 0)
                    {
                        sb.Append(tokens[j].Value);

                        if (tokens[j].TokenType == TokenType.OpeningBracket)
                            brackets++;
                        else if (tokens[j].TokenType == TokenType.ClosingBracket)
                            brackets--;

                        if (j == tokens.Count - 1)
                            break;

                        j++;
                    }

                    subExpressions.Add(new Token(TokenType.SubExpression, sb.ToString()));
                    sb.Clear();
                    i = j - 1;
                }
            }

            tokens.RemoveAt(0);

            return subExpressions;
        }
        public static IEnumerable<IEnumerable<T>> Permute<T>(this IEnumerable<T> sequence)
        {
            if (sequence == null)
            {
                yield break;
            }

            var list = sequence.ToList();

            if (!list.Any())
            {
                yield return Enumerable.Empty<T>();
            }
            else
            {
                var startingElementIndex = 0;

                foreach (var startingElement in list)
                {
                    var index = startingElementIndex;
                    var remainingItems = list.Where((e, i) => i != index);

                    foreach (var permutationOfRemainder in remainingItems.Permute())
                    {
                        yield return permutationOfRemainder.Prepend(startingElement);
                    }

                    startingElementIndex++;
                }
            }
        }
    }
}
