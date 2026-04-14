namespace SecurIT_Memory.Models
{
    public class Card
    {
        public int Id { get; private set; }
        public string ImagePath { get; private set; }
        public CardState CurrentState { get; set; }

        public Card(int id, string imagePath)
        {
            Id = id;
            ImagePath = imagePath;
            CurrentState = CardState.Hidden;
        }
    }
}