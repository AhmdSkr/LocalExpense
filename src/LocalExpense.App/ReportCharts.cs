using System.Globalization;
using LocalExpense.Models;
using LocalExpense.Services;
using ScottPlot;
using ScottPlot.Panels;
using ScottPlot.TickGenerators;
using ScottPlot.WinForms;
using Color = ScottPlot.Color;

namespace LocalExpense;

/// <summary>
/// Draws the Reports charts on ScottPlot controls. Buckets sit at positions 0, 1, 2, ... so the axis labels come from
/// <see cref="ChartData"/> (invariant, never the culture's calendar) rather than from a date axis. Series differ by marker and line style
/// or fill, not only by colour, and the colours avoid red against green; the numbers are in the table view.
/// </summary>
internal static class ReportCharts
{
    // Wide enough for the longest label of each granularity, with a gap.
    private const int DayLabelWidth = 46;
    private const int MonthLabelWidth = 66;
    private const int YearLabelWidth = 44;

    private static readonly Color IncomeColor = Colors.SteelBlue;
    private static readonly Color ExpensesColor = Colors.DarkOrange;
    private static readonly Color NetColor = Colors.Black;

    public static void ShowLines(FormsPlot control, ChartData data, Granularity granularity)
    {
        var plot = Prepare(control, data, granularity);
        var xs = Positions(data);

        AddLine(plot, xs, data.IncomeMinor, Strings.LegendIncome, IncomeColor, MarkerShape.FilledCircle, LinePattern.Solid);
        AddLine(plot, xs, data.ExpensesMinor, Strings.LegendExpenses, ExpensesColor, MarkerShape.FilledSquare, LinePattern.Dashed);
        AddLine(plot, xs, data.NetMinor, Strings.LegendNet, NetColor, MarkerShape.FilledDiamond, LinePattern.Dotted);

        plot.ShowLegend(Edge.Bottom);
        plot.Axes.AutoScale();
        control.Refresh();
    }

    public static void ShowBars(FormsPlot control, ChartData data, Granularity granularity)
    {
        var plot = Prepare(control, data, granularity);

        // Two bars per bucket, side by side: income on the left in solid blue, expenses on the right with diagonal stripes in orange.
        var income = new List<Bar>();
        var expenses = new List<Bar>();
        for (var i = 0; i < data.Count; i++)
        {
            income.Add(new Bar { Position = i - 0.2, Value = ToMajor(data.IncomeMinor[i]), Size = 0.38, FillColor = IncomeColor });
            expenses.Add(new Bar { Position = i + 0.2, Value = ToMajor(data.ExpensesMinor[i]), Size = 0.38, FillColor = ExpensesColor, FillHatch = new ScottPlot.Hatches.Striped(ScottPlot.Hatches.StripeDirection.DiagonalUp), FillHatchColor = Colors.White });
        }

        plot.Add.Bars(income);
        plot.Add.Bars(expenses);
        plot.Legend.ManualItems.Add(new LegendItem { LabelText = Strings.LegendIncome, FillColor = IncomeColor });
        plot.Legend.ManualItems.Add(new LegendItem { LabelText = Strings.LegendExpenses, FillColor = ExpensesColor, FillHatch = new ScottPlot.Hatches.Striped(ScottPlot.Hatches.StripeDirection.DiagonalUp), FillHatchColor = Colors.White });
        plot.ShowLegend(Edge.Bottom);
        plot.Axes.Margins(bottom: 0);   // bars start at zero; both series are never negative
        plot.Axes.AutoScale();
        control.Refresh();
    }

    // Clears the previous chart and sets up the axes: a bucket label at every Nth position, money on the left, no mouse panning or zooming.
    private static Plot Prepare(FormsPlot control, ChartData data, Granularity granularity)
    {
        var plot = control.Plot;
        plot.Clear();
        plot.Legend.ManualItems.Clear();
        plot.HideLegend();

        // ShowLegend(Edge) adds a new legend panel on every call and Clear() leaves the old ones, so drop them before redrawing.
        foreach (var panel in plot.Axes.GetPanels().OfType<LegendPanel>())
        {
            plot.Axes.Remove(panel);
        }

        var labelWidth = granularity switch { Granularity.Day => DayLabelWidth, Granularity.Month => MonthLabelWidth, _ => YearLabelWidth };
        var step = ChartData.LabelStep(data.Count, Math.Max(1, control.Width / labelWidth));
        var ticks = Enumerable.Range(0, data.Count).Where(i => i % step == 0).Select(i => new Tick(i, data.Labels[i])).ToArray();
        plot.Axes.Bottom.TickGenerator = new NumericManual(ticks);
        plot.Axes.Left.TickGenerator = new NumericAutomatic { LabelFormatter = FormatMoneyTick };
        plot.Axes.Margins(horizontal: 0.04);

        control.UserInputProcessor.Disable();
        return plot;
    }

    private static void AddLine(Plot plot, double[] xs, IReadOnlyList<long> minor, string name, Color color, MarkerShape marker, LinePattern pattern)
    {
        var line = plot.Add.Scatter(xs, minor.Select(ToMajor).ToArray());
        line.LegendText = name;
        line.Color = color;
        line.MarkerShape = marker;
        line.MarkerSize = 7;
        line.LineWidth = 2;
        line.LinePattern = pattern;
    }

    private static double[] Positions(ChartData data) => Enumerable.Range(0, data.Count).Select(i => (double)i).ToArray();

    private static double ToMajor(long minor) => (double)Money.ToMajor(minor);

    // Axis ticks are whole currency units, e.g. -$1,500, always formatted the same way whatever the culture.
    private static string FormatMoneyTick(double value) =>
        (value < 0 ? "-" : "") + Money.Symbol + Math.Abs(value).ToString("N0", CultureInfo.InvariantCulture);
}
