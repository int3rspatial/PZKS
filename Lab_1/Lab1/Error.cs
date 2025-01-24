namespace Lab1
{
    public struct Error
    {
        public int Position { get; }
        public string Message { get; }

        public Error(int position, string message)
        {
            Position = position;
            Message = message;
        }

        public override string ToString()
        {
            //for error without specific position
            if (Position == -1)
            {
                return $"{Message}";
            }

            return $"Position {Position}: {Message}";
        }
    }
}
