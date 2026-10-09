using System.Globalization;
using LocalExpense.Models;
using LocalExpense.Services;

namespace LocalExpense;

/// <summary>
/// Modal reports window. A period is chosen with three combos: the kind (month, year or whole timeline) and, depending on the kind,
/// a year and a month. The first entry of the month combo is "Last month" and, in year mode, of the year combo "Last year": both are
/// rolling windows ending today, not calendar periods. The summary shows income, expenses and net for the chosen period, and the table below
/// it breaks the net down by category and by day, month or year (see <see cref="PeriodBucketer.GetGranularity"/>). The View combo swaps that table
/// for a line chart (income, expenses, net) or a bar chart (income and expenses) of the same period. Everything comes from one query.
/// </summary>
public partial class ReportsForm : Form
{
    // The order of the kind combo's items.
    private enum Kind
    {
        Month,
        Year,
        Timeline,
    }

    // The order of the view combo's items.
    private enum View
    {
        Table,
        LineChart,
        BarChart,
    }

    private enum Source
    {
        Kind,
        Year,
        Month,
    }

    private readonly TransactionService? service;
    private readonly List<int> years = [];
    private readonly LatestQuery loads = new();
    private int? chosenYear;
    private bool updatingSelection;
    private DateRange? bounds;
    private Font? totalRowFont;
    private ChartData? lineData;
    private ChartData? barData;
    private Granularity lineGranularity;
    private Granularity barGranularity;

    // Designer path: no service, so nothing is loaded. Production composes the overload below via DI.
    public ReportsForm()
    {
        InitializeComponent();
    }

    public ReportsForm(TransactionService service) : this()
    {
        this.service = service;
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (service is not null)
        {
            await this.TryAsync(InitializeAsync);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        loads.Cancel();
        totalRowFont?.Dispose();
        base.OnFormClosed(e);
    }

    private async void kindCombo_SelectedIndexChanged(object? sender, EventArgs e) =>
        await OnSelectionChangedAsync(Source.Kind);

    private async void yearCombo_SelectedIndexChanged(object? sender, EventArgs e) =>
        await OnSelectionChangedAsync(Source.Year);

    private async void monthCombo_SelectedIndexChanged(object? sender, EventArgs e) =>
        await OnSelectionChangedAsync(Source.Month);

    // Only swaps what is shown; the data is already loaded, so there is no query.
    private void viewCombo_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!updatingSelection && service is not null)
        {
            ApplyView();
        }
    }

    private Kind SelectedKind => (Kind)Math.Max(0, kindCombo.SelectedIndex);

    private View SelectedView => (View)Math.Max(0, viewCombo.SelectedIndex);

    // Reads the data bounds once (this window is modal, so the data cannot change under it), fills the combos and shows the default.
    private async Task InitializeAsync()
    {
        summaryPanel.Visible = false;
        viewPanel.Visible = false;
        noDataLabel.Visible = false;

        bounds = await service!.GetDateBoundsAsync();
        years.Clear();
        if (bounds is { } b)
        {
            years.AddRange(PeriodBucketer.GetYears(b).Reverse());
        }

        updatingSelection = true;
        try
        {
            kindCombo.Items.AddRange([Strings.PeriodMonth, Strings.PeriodYear, Strings.PeriodTimeline]);
            kindCombo.SelectedIndex = (int)Kind.Month;
            monthCombo.Items.Add(Strings.LastMonth);
            monthCombo.Items.AddRange(Enumerable.Range(1, 12).Select(MonthName).ToArray<object>());
            monthCombo.SelectedIndex = 0;
            viewCombo.Items.AddRange([Strings.ViewTable, Strings.ViewLineChart, Strings.ViewBarChart]);
            viewCombo.SelectedIndex = (int)View.Table;
            FillYears(Kind.Month);
            UpdateEnabledState();
        }
        finally
        {
            updatingSelection = false;
        }

        await RefreshAsync();
    }

    private async Task OnSelectionChangedAsync(Source source)
    {
        if (updatingSelection || service is null)
        {
            return;
        }

        // Changing the kind or the month entry rebuilds or enables other combos; their events must not each trigger a query.
        updatingSelection = true;
        try
        {
            if (source == Source.Year)
            {
                chosenYear = SelectedYear();
            }
            else if (source == Source.Kind)
            {
                FillYears(SelectedKind);
            }

            UpdateEnabledState();
        }
        finally
        {
            updatingSelection = false;
        }

        await this.TryAsync(RefreshAsync);
    }

    // Month mode lists real years only; year mode puts "Last year" first. A year the user picked earlier is selected again when it is listed;
    // otherwise the default is "Last year" in year mode and the newest year in month mode.
    private void FillYears(Kind kind)
    {
        yearCombo.Items.Clear();
        if (kind == Kind.Year)
        {
            yearCombo.Items.Add(Strings.LastYear);
        }

        yearCombo.Items.AddRange(years.Select(y => y.ToString(CultureInfo.InvariantCulture)).ToArray<object>());
        var index = chosenYear is { } year ? years.IndexOf(year) : -1;
        if (index >= 0)
        {
            yearCombo.SelectedIndex = index + YearItemOffset(kind);
        }
        else if (yearCombo.Items.Count > 0)
        {
            yearCombo.SelectedIndex = 0;
        }
    }

    private static int YearItemOffset(Kind kind) => kind == Kind.Year ? 1 : 0;

    // The year that is selected, or null when none is or the entry is "Last year".
    private int? SelectedYear()
    {
        var index = yearCombo.SelectedIndex - YearItemOffset(SelectedKind);
        return index >= 0 && index < years.Count ? years[index] : null;
    }

    private void UpdateEnabledState()
    {
        var kind = SelectedKind;
        var lastMonth = monthCombo.SelectedIndex <= 0;
        monthCombo.Enabled = kind == Kind.Month;
        yearCombo.Enabled = kind == Kind.Year || (kind == Kind.Month && !lastMonth);
    }

    // Null when the selection does not name a period (no data to choose a year from).
    private Period? SelectedPeriod()
    {
        switch (SelectedKind)
        {
            case Kind.Timeline:
                return Period.Timeline;
            case Kind.Year:
                if (yearCombo.SelectedIndex == 0)
                {
                    return Period.LastTwelveMonths;
                }

                return SelectedYear() is { } year ? Period.ForYear(year) : null;
            default:
                if (monthCombo.SelectedIndex <= 0)
                {
                    return Period.LastMonth;
                }

                return SelectedYear() is { } y ? Period.ForMonth(y, monthCombo.SelectedIndex) : null;
        }
    }

    // A new query cancels the one in flight, and only the newest result is shown.
    private async Task RefreshAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var period = SelectedPeriod();
        var range = period is { } p ? PeriodBucketer.GetRange(p, today, bounds) : null;
        if (period is not { } selected || range is not { } r)
        {
            loads.Cancel();
            ShowNoData(null);
            return;
        }

        if (await loads.RunAsync(ct => service!.GetDailyTotalsByCategoryAsync(r.From, r.To, ct)) is not { } rows)
        {
            return;
        }

        var totals = new PeriodTotals(rows.Sum(x => x.Totals.IncomeMinor), rows.Sum(x => x.Totals.ExpensesMinor));
        if (!totals.HasData)
        {
            ShowNoData(r);
            return;
        }

        // The table and the bar chart switch to yearly buckets for a long timeline; the line chart stays monthly.
        var granularity = PeriodBucketer.GetGranularity(selected, r, allowYearly: true);
        var buckets = PeriodBucketer.GetBuckets(r, granularity);
        var table = ReportBuilder.ByCategory(buckets, rows);
        var bar = ChartData.From(ReportBuilder.TotalsByBucket(buckets, rows), granularity);

        var lineGran = PeriodBucketer.GetGranularity(selected, r, allowYearly: false);
        var lineBuckets = lineGran == granularity ? buckets : PeriodBucketer.GetBuckets(r, lineGran);
        var line = ChartData.From(ReportBuilder.TotalsByBucket(lineBuckets, rows), lineGran);

        barData = bar;
        barGranularity = granularity;
        lineData = line;
        lineGranularity = lineGran;
        ShowReport(r, totals, table, granularity);
    }

    private void ShowReport(DateRange range, PeriodTotals totals, CategoryTable table, Granularity granularity)
    {
        rangeLabel.Text = RangeText(range);
        incomeValue.Text = Money.Format(totals.IncomeMinor);
        expensesValue.Text = Money.Format(totals.ExpensesMinor);
        netValue.Text = Money.Format(totals.NetMinor);

        netValue.ForeColor = FormHelpers.AmountColor(totals.NetMinor);
        FillTable(table, granularity);
        summaryPanel.Visible = true;
        viewPanel.Visible = true;
        noDataLabel.Visible = false;
        ApplyView();
    }

    // Shows the chosen view of the loaded data. Charts are drawn when they are shown, once the panel has its final size, because the axis labels
    // are thinned to the width available.
    private void ApplyView()
    {
        var view = SelectedView;
        categoryGrid.Visible = view == View.Table;
        linePlot.Visible = view == View.LineChart;
        barPlot.Visible = view == View.BarChart;
        viewPanel.PerformLayout();

        switch (view)
        {
            case View.Table:
                FitColumns();

                // A freshly filled grid would otherwise highlight its first cell.
                categoryGrid.CurrentCell = null;
                categoryGrid.ClearSelection();
                break;
            case View.LineChart when lineData is { } line:
                ReportCharts.ShowLines(linePlot, line, lineGranularity);
                break;
            case View.BarChart when barData is { } bar:
                ReportCharts.ShowBars(barPlot, bar, barGranularity);
                break;
        }
    }

    private void ShowNoData(DateRange? range)
    {
        rangeLabel.Text = range is { } r ? RangeText(r) : string.Empty;
        summaryPanel.Visible = false;
        viewPanel.Visible = false;
        noDataLabel.Visible = true;
    }

    // Columns are Category, one per bucket, Total; only the first and last exist in the designer. Every cell holds the net of a category
    // in a bucket (income minus expenses), signed, with an en dash for none. The last row sums each column.
    private void FillTable(CategoryTable table, Granularity granularity)
    {
        categoryGrid.SuspendLayout();
        try
        {
            categoryGrid.Rows.Clear();
            for (var i = categoryGrid.Columns.Count - 2; i >= 1; i--)
            {
                categoryGrid.Columns.RemoveAt(i);
            }

            foreach (var bucket in table.Buckets)
            {
                categoryGrid.Columns.Insert(totalColumn.Index, new DataGridViewTextBoxColumn
                {
                    HeaderText = PeriodBucketer.Label(bucket, granularity),
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight },
                });
            }

            foreach (var row in table.Rows)
            {
                AddTableRow(row.Category, row.NetMinor, row.TotalMinor, bold: false);
            }

            AddTableRow(Strings.TotalRow, table.BucketNetMinor, table.TotalMinor, bold: true);
        }
        finally
        {
            categoryGrid.ResumeLayout();
        }
    }

    // Period and Total columns share the width the Category column leaves, so a few columns fill the grid. Each keeps at least the width its
    // content needs, so many columns overflow instead and the grid scrolls sideways. Weights are equal so spare width is shared evenly.
    private void FitColumns()
    {
        for (var i = 1; i < categoryGrid.Columns.Count; i++)
        {
            var column = categoryGrid.Columns[i];
            column.MinimumWidth = column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            column.FillWeight = 100;
        }
    }

    private void AddTableRow(string label, IReadOnlyList<long> netByBucket, long total, bool bold)
    {
        var values = new object[netByBucket.Count + 2];
        values[0] = label;
        for (var i = 0; i < netByBucket.Count; i++)
        {
            values[i + 1] = AmountText(netByBucket[i]);
        }

        values[^1] = AmountText(total);

        var row = categoryGrid.Rows[categoryGrid.Rows.Add(values)];
        for (var i = 0; i < netByBucket.Count; i++)
        {
            StyleAmount(row.Cells[i + 1], netByBucket[i]);
        }

        StyleAmount(row.Cells[^1], total);
        if (bold)
        {
            row.DefaultCellStyle.Font = totalRowFont ??= new Font(categoryGrid.Font, FontStyle.Bold);
        }
    }

    private static string AmountText(long minor) => minor == 0 ? "\u2013" : Money.Format(minor);

    private static void StyleAmount(DataGridViewCell cell, long minor) =>
        cell.Style.ForeColor = FormHelpers.AmountColor(minor);

    private static string RangeText(DateRange range) =>
        string.Format(Strings.PeriodRange,
            range.From.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            range.To.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

    // Gregorian names whatever the culture's default calendar is, since periods are Gregorian months.
    private static string MonthName(int month)
    {
        var culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        culture.DateTimeFormat.Calendar = new GregorianCalendar();
        return culture.DateTimeFormat.GetMonthName(month);
    }
}
