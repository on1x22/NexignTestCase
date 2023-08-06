namespace RockPaperScissors.Domain
{
    public static class ComputerPlayer
    {
        public static string GetTurn()
        {
            var random = new Random();
            var values = new List<string>{
                "камень",
                "ножницы",
                "бумага"};
            int index = random.Next(values.Count);

            return values[index];
        }
    }
}
