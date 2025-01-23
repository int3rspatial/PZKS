namespace Lab1
{
    public struct Token
    {
        public TokenType TokenType { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            TokenType = type;
            Value = value;
        }

        public override string ToString() => $"[{TokenType}, {Value}]";
    }
}
