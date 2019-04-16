namespace Podwoozka.Models
{
    public class RaceItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }

        public string Owner { get; set; }
    }
}