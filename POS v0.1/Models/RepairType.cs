namespace POS_v0_1.Models
{
    public class RepairType
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public decimal BasePrice { get; set; }
        public string Description { get; set; }

        // Navigation helper for display
        public string CategoryName { get; set; }
    }
}
