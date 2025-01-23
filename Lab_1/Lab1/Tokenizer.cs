using NCalc;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Lab1
{
    public class Tokenizer
    {
        private List<Token> _tokens;
        private List<Error> _errors;

        private Regex _operationSymbolsRegex;
        private Regex _varRegex;
        private Regex _constantRegex;

        public List<Token> Tokens
        {
            get { return _tokens; }
        }

        public List<Error> Errors
        {
            get { return _errors; }
        }

        public Tokenizer()
        {
            _tokens = new List<Token>();
            _errors = new List<Error>();

            _operationSymbolsRegex = new Regex("[()+*/-]");
            _varRegex = new Regex("[a-zA-Z_]");
            _constantRegex = new Regex("[0-9.]");
        }

        public void Tokenize(string expression)
        {
            _tokens.Clear();
            _errors.Clear();
            expression = expression.Replace(" ", "");
            for (int i = 0; i < expression.Length; i++)
            {
                string exprSymBuf = expression[i].ToString();

                if (expression[i] == '+' || expression[i] == '-')
                {
                    _tokens.Add(new Token(TokenType.AddSubtractOperation, exprSymBuf));
                }
                else if (expression[i] == '*' || expression[i] == '/')
                {
                    _tokens.Add(new Token(TokenType.MultiplyDivideOperation, exprSymBuf));
                }
                else if (expression[i] == '(')
                {
                    _tokens.Add(new Token(TokenType.OpeningBracket, exprSymBuf));
                }
                else if (expression[i] == ')')
                {
                    _tokens.Add(new Token(TokenType.ClosingBracket, exprSymBuf));
                }
                else if (_varRegex.IsMatch(exprSymBuf))
                {
                    string variable = exprSymBuf;
                    int k = i;

                    for (int j = i + 1; j < expression.Length; j++)
                    {
                        if (_operationSymbolsRegex.IsMatch(expression[j].ToString()))
                        {
                            break;
                        }
                        else if (!_varRegex.IsMatch(expression[j].ToString()) && !_constantRegex.IsMatch(expression[j].ToString()))
                        {
                            _errors.Add(new Error(j, "invalid variable name"));
                        }
                        variable += expression[j].ToString();
                        k = j;
                    }

                    i = k;

                    _tokens.Add(new Token(TokenType.Variable, variable));
                }
                else if (_constantRegex.IsMatch(exprSymBuf))
                {
                    string constant = exprSymBuf;
                    int k = i;

                    for (int j = i + 1; j < expression.Length; j++)
                    {
                        if (_operationSymbolsRegex.IsMatch(expression[j].ToString()))
                        {
                            break;
                        }
                        k = j;
                        constant += expression[j].ToString();
                    }

                    i = k;

                    _tokens.Add(new Token(TokenType.Constant, constant));
                }
                else
                {
                    _errors.Add(new Error(i, $"invalid symbol \'{expression[i]}\'"));
                    _tokens.Add(new Token(TokenType.InvalidSymbol, exprSymBuf));
                }
            }
        }

        public void TokenizeExpandedExpression(string expression)
        {
            _tokens.Clear();
            _errors.Clear();

            Tokenize(expression);

            int zeroDivErrors = 0;
            for (int i = 0; i < _tokens.Count; i++)
            {
                //exponent
                if (_tokens[i].TokenType == TokenType.MultiplyDivideOperation &&
                    _tokens[i - 1].TokenType == TokenType.MultiplyDivideOperation)
                {
                    _tokens[i - 1] = new Token(TokenType.Exponent, "^");
                    _tokens.RemoveAt(i);
                }

                //unary operations
                if (i == 0)
                {
                    if (_tokens[i].TokenType == TokenType.AddSubtractOperation)
                    {
                        TokenType tokenType = TokenType.InvalidSymbol;
                        if (_tokens[i + 1].TokenType == TokenType.Constant)
                        {
                            tokenType = TokenType.UnaryOpConstant;
                        }
                        else if (_tokens[i + 1].TokenType == TokenType.Variable)
                        {
                            tokenType = TokenType.UnaryOpVariable;
                        }
                        _tokens[i] = new Token(tokenType,
                            $"{_tokens[i].Value}{_tokens[i + 1].Value}");
                        _tokens.RemoveAt(i + 1);
                    }
                }
                else if (_tokens[i].TokenType == TokenType.AddSubtractOperation)
                {
                    if (_tokens[i - 1].TokenType == TokenType.OpeningBracket)
                    {
                        TokenType tokenType = TokenType.InvalidSymbol;
                        if (_tokens[i + 1].TokenType == TokenType.Constant)
                        {
                            tokenType = TokenType.UnaryOpConstant;
                        }
                        else if (_tokens[i + 1].TokenType == TokenType.Variable)
                        {
                            tokenType = TokenType.UnaryOpVariable;
                        }
                        _tokens[i] = new Token(tokenType,
                            $"{_tokens[i].Value}{_tokens[i + 1].Value}");
                        _tokens.RemoveAt(i + 1);
                    }
                }

                //zero division
                if (_tokens[i].TokenType == TokenType.Variable)
                {
                    if (_tokens[i].Value == "zoo")
                    {
                        zeroDivErrors++;
                    }
                    else if (_tokens[i].Value == "nan")
                    {
                        _errors.Add(new Error(-1, "Division by zero error, 0/0 type"));
                    }
                }
            }

            if (zeroDivErrors > 0)
                _errors.Add(new Error(-1, $"[{zeroDivErrors}] division by zero, 1/0 type"));

            //not fully solved equations (fractions)
            if (_tokens.Count == 3)
            {
                if (_tokens[0].TokenType == TokenType.Constant &&
                    _tokens[1].TokenType == TokenType.MultiplyDivideOperation &&
                    _tokens[2].TokenType == TokenType.Constant)
                {
                    double result = (double)new Expression(expression).Evaluate();
                    _tokens.Clear();
                    _tokens.Add(new Token(TokenType.Constant, result.ToString()));
                }
            }
        }

        public List<Error> CheckForErrors()
        {
            int position = 0;
            List<string> brackets = new List<string>();

            for (int i = 0; i < _tokens.Count; i++)
            {
                switch (_tokens[i].TokenType)
                {
                    case TokenType.AddSubtractOperation when i == _tokens.Count - 1:
                        {
                            _errors.Add(new Error(position, "end of expression after operator, variable or constant expected"));
                            break;
                        }
                    case TokenType.AddSubtractOperation:
                        {
                            position++;
                            switch (_tokens[i + 1].TokenType)
                            {
                                case TokenType.AddSubtractOperation:
                                    {
                                        _errors.Add(new Error(position, $"operator \'{_tokens[i + 1].Value}\' after another operator"));
                                        break;
                                    }
                                case TokenType.MultiplyDivideOperation:
                                    {
                                        _errors.Add(new Error(position, $"operator \'{_tokens[i + 1].Value}\' after another operator"));
                                        break;
                                    }
                                case TokenType.ClosingBracket:
                                    {
                                        _errors.Add(new Error(position, $"closing bracket after operator, variable or constant expected"));
                                        break;
                                    }
                                case TokenType.InvalidSymbol:
                                    {
                                        _errors.Add(new Error(position, $"invalid symbol after '{_tokens[i].Value}' operator"));
                                        break;
                                    }
                            }
                            break;
                        }
                    case TokenType.MultiplyDivideOperation when i == 0:
                        {
                            _errors.Add(new Error(position, $"{_tokens[i].Value} operator at the beginning of the expression"));
                            position++;
                            break;
                        }
                    case TokenType.MultiplyDivideOperation when i == _tokens.Count - 1:
                        {
                            _errors.Add(new Error(position, "end of expression after operator, variable or constant expected"));
                            break;
                        }
                    case TokenType.MultiplyDivideOperation:
                        {
                            position++;
                            switch (_tokens[i + 1].TokenType)
                            {
                                case TokenType.AddSubtractOperation:
                                    {
                                        _errors.Add(new Error(position, $"operator \'{_tokens[i + 1].Value}\' after another operator"));
                                        break;
                                    }
                                case TokenType.MultiplyDivideOperation:
                                    {
                                        _errors.Add(new Error(position, $"operator \'{_tokens[i + 1].Value}\' after another operator"));
                                        break;
                                    }
                                case TokenType.ClosingBracket:
                                    {
                                        _errors.Add(new Error(position, $"closing bracket after operator, variable or constant expected"));
                                        break;
                                    }
                                case TokenType.InvalidSymbol:
                                    {
                                        _errors.Add(new Error(position, $"invalid symbol after '{_tokens[i].Value}' operator"));
                                        break;
                                    }
                            }
                            break;
                        }
                    case TokenType.Variable when i < _tokens.Count - 1:
                        {
                            position += _tokens[i].Value.Length;
                            if (_tokens[i + 1].TokenType == TokenType.OpeningBracket)
                            {
                                _errors.Add(new Error(position, "opening bracket after variable, operator expected"));
                            }
                            break;
                        }
                    case TokenType.Constant:
                        {
                            if (!Double.TryParse(_tokens[i].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                            {
                                _errors.Add(new Error(position, $"invalid constant \'{_tokens[i].Value}\'"));
                            }

                            position += _tokens[i].Value.Length;

                            if (i < _tokens.Count - 1)
                            {
                                if (_tokens[i + 1].TokenType == TokenType.OpeningBracket)
                                {
                                    _errors.Add(new Error(position, "opening bracket after constant, operator expected"));
                                }
                            }

                            break;
                        }
                    case TokenType.OpeningBracket when i == _tokens.Count - 1:
                        {
                            _errors.Add(new Error(position, "end of expression after opening bracket"));
                            brackets.Add(_tokens[i].Value);
                            break;
                        }
                    case TokenType.OpeningBracket:
                        {
                            position++;
                            switch (_tokens[i + 1].TokenType)
                            {
                                case TokenType.MultiplyDivideOperation:
                                    {
                                        _errors.Add(new Error(position, $"operator \'{_tokens[i + 1].Value}\' after opening bracket"));
                                        break;
                                    }
                                case TokenType.ClosingBracket:
                                    {
                                        _errors.Add(new Error(position, "closing bracket after opening one, variable or constant expected"));
                                        break;
                                    }
                                case TokenType.InvalidSymbol:
                                    {
                                        _errors.Add(new Error(position, $"invalid symbol after opening bracket"));
                                        break;
                                    }
                            }
                            brackets.Add(_tokens[i].Value);

                            break;
                        }
                    case TokenType.ClosingBracket when i == 0:
                        {
                            _errors.Add(new Error(position, "closing bracket at the beginning of the expression"));
                            brackets.Add(_tokens[i].Value);
                            position++;
                            break;
                        }
                    case TokenType.ClosingBracket:
                        {
                            if (i < _tokens.Count - 1)
                            {
                                position++;
                                switch (_tokens[i + 1].TokenType)
                                {
                                    case TokenType.OpeningBracket:
                                        {
                                            _errors.Add(new Error(position, "opening bracket after closing one, operator expected"));
                                            break;
                                        }
                                    case TokenType.Variable:
                                        {
                                            _errors.Add(new Error(position, "variable after closing bracket, operator expected"));
                                            break;
                                        }
                                    case TokenType.Constant:
                                        {
                                            _errors.Add(new Error(position, "constant after closing bracket, operator expected"));
                                            break;
                                        }
                                    case TokenType.InvalidSymbol:
                                        {
                                            _errors.Add(new Error(position, $"invalid symbol after closing brackets"));
                                            break;
                                        }
                                }
                            }
                            brackets.Add(_tokens[i].Value);
                            break;
                        }
                    case TokenType.InvalidSymbol:
                        position++;
                        break;
                }
            }

            if (brackets.Count > 0)
            {
                if (brackets[0] == ")" || brackets[brackets.Count - 1] == "(")
                {
                    _errors.Add(new Error(-1, "Wrong order of brackets."));
                }
            }

            int opnBracket = 0, closingBracket = 0;
            foreach (string bracket in brackets)
            {
                if (bracket == "(")
                    opnBracket++;
                else
                    closingBracket++;
            }

            if (opnBracket > closingBracket)
            {
                _errors.Add(new Error(-1, "Not enough closing brackets."));
            }
            else if (closingBracket > opnBracket)
            {
                _errors.Add(new Error(-1, "Not enough opening brackets."));
            }

            _errors = _errors.OrderBy(x => x.Position).ToList();

            return _errors;
        }

        public string TokensToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Token token in _tokens)
            {
                sb.Append(token.Value);
            }
            return sb.ToString();
        }

        public void PrintTokens()
        {
            foreach (var token in _tokens)
            {
                Console.WriteLine(token.ToString());
            }
        }
    }
}
