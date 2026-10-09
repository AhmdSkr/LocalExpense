using LocalExpense.Models;

namespace LocalExpense.Cli;

/// <summary>Deterministic, lived-in sample data: a few years of salary, rent, groceries, coffee and the occasional surprise.</summary>
internal static class SeedGenerator
{
    private static readonly string[] GroceryNotes = ["Weekly shop", "Farmers market", "Corner store", "Bakery run", "Bulk rice & lentils", "Late-night snacks"];
    private static readonly string[] CoffeeNotes = ["Coffee, oat milk", "Flat white", "Café crème", "Cold brew", "Espresso, double"];
    private static readonly string[] DiningNotes = ["Pizza \"Da Luigi\"", "Sushi night", "Crème brûlée & friends", "+1 birthday dinner", "Falafel, extra tahini", "Ramen with Sam"];
    private static readonly string[] TransportNotes = ["Metro top-up", "Taxi home", "-50% off-peak pass", "Bike repair", "Ride share"];

    /// <summary>
    /// Newest first, as the app shows them. With three months or more, the month in the middle of the span has no
    /// transactions at all, so zero-filling and gaps get exercised; shorter spans have no middle to leave empty.
    /// </summary>
    public static List<Transaction> Generate(DateOnly end, int months, int randomSeed)
    {
        var rnd = new Random(randomSeed);
        var list = new List<Transaction>();
        var firstMonth = new DateOnly(end.Year, end.Month, 1).AddMonths(-(months - 1));
        var gap = months >= 3 ? months / 2 : -1;

        decimal Amount(double min, double max) => Math.Round((decimal)(min + rnd.NextDouble() * (max - min)), 2);
        string Pick(string[] notes) => notes[rnd.Next(notes.Length)];

        for (var m = 0; m < months; m++)
        {
            if (m == gap)
            {
                continue;
            }

            var month = firstMonth.AddMonths(m);
            var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
            DateOnly Day(int d) => new(month.Year, month.Month, Math.Min(d, daysInMonth));
            void Add(DateOnly date, decimal amount, string category, string? note)
            {
                if (date <= end)
                {
                    list.Add(new Transaction { Date = date, AmountMinor = Money.ToMinor(amount), Category = category, Note = note });
                }
            }

            Add(Day(25), m >= 18 ? 3450m : 3200m, "Salary", "Salary - Acme Corp");
            if (month.Month == 12)
            {
                Add(Day(20), 1500m, "Salary", "Year-end bonus");
            }

            if (rnd.Next(4) == 0)
            {
                Add(Day(rnd.Next(5, 28)), Amount(250, 900), "Freelance", "Side gig: logo + brand sheet");
            }

            Add(Day(1), m >= 12 ? -1200m : -1150m, "Housing", "Rent - flat 4B");
            Add(Day(rnd.Next(5, 13)), -Amount(55, 140), "Utilities", "Electricity, water");
            Add(Day(14), -39.99m, "Utilities", "Fibre 300 Mbps");
            Add(Day(18), -24.00m, "Utilities", "Phone plan");
            Add(Day(9), -15.49m, "Entertainment", "Streaming");
            Add(Day(3), -29.00m, "Health", "Gym membership - \"Iron Works\"");

            for (var i = rnd.Next(5, 9); i > 0; i--)
            {
                Add(Day(rnd.Next(1, 29)), -Amount(18, 96), "Groceries", Pick(GroceryNotes));
            }

            for (var i = rnd.Next(4, 10); i > 0; i--)
            {
                Add(Day(rnd.Next(1, 29)), -Amount(3.2, 6.4), "Coffee", Pick(CoffeeNotes));
            }

            for (var i = rnd.Next(2, 5); i > 0; i--)
            {
                Add(Day(rnd.Next(1, 29)), -Amount(14, 70), "Dining", Pick(DiningNotes));
            }

            for (var i = rnd.Next(2, 5); i > 0; i--)
            {
                Add(Day(rnd.Next(1, 29)), -Amount(2.5, 45), "Transport", Pick(TransportNotes));
            }

            if (rnd.Next(4) == 0)
            {
                Add(Day(rnd.Next(1, 29)), -Amount(20, 120), "Health", "Pharmacy");
            }

            if (rnd.Next(3) == 0)
            {
                Add(Day(rnd.Next(1, 29)), -Amount(25, 180), "Shopping", "Clothes");
            }

            if (rnd.Next(2) == 0)
            {
                Add(Day(rnd.Next(1, 29)), -Amount(9, 30), "Entertainment", "Cinema + popcorn");
            }

            if (month.Month == 7)
            {
                Add(Day(2), -Amount(400, 900), "Travel", "Flights to Lisbon");
            }

            if (month.Month == 8)
            {
                Add(Day(31), -Amount(300, 700), "Travel", "Hotel, 5 nights");
            }

            if (m == 8)
            {
                Add(Day(11), -1299.00m, "Shopping", "Laptop: 14\" ultrabook");
            }

            if (m == 11)
            {
                Add(Day(28), -85.50m, "Housing", "Moving day:\nboxes, tape, van rental");
            }

            if (m == 20)
            {
                Add(Day(16), -Amount(300, 650), "Health", "Dentist: root canal");
            }

            if (m == 22)
            {
                Add(Day(1), 34.99m, "Shopping", "Refund: headphones");
            }
        }

        var sorted = list.OrderByDescending(t => t.Date).ThenBy(t => t.AmountMinor).ToList();
        for (var i = 0; i < sorted.Count; i++)
        {
            sorted[i].Id = sorted.Count - i;
        }

        return sorted;
    }
}
