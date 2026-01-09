using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlarmMonitor.Data;
using AlarmMonitor.Models;
using System.Linq;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace AlarmMonitor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlarmsController : ControllerBase
    {
        private readonly AlarmContext _context;

        public AlarmsController(AlarmContext context)
        {
            _context = context;
            // PDF License for Community use
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private IQueryable<AlarmEvent> ApplyFilters(
            string? sourceName = null,
            int? severity = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.AlarmEvents.AsQueryable();

            if (!string.IsNullOrEmpty(sourceName))
            {
                query = query.Where(a => a.SourceName != null && a.SourceName.Contains(sourceName));
            }

            if (severity.HasValue)
            {
                query = query.Where(a => a.Severity == severity.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.EventTimeStamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.EventTimeStamp <= toDate.Value);
            }

            return query;
        }

        // GET: api/Alarms
        [HttpGet]
        public async Task<ActionResult<object>> GetAlarmEvents(
            [FromQuery] DateTime? since = null,
            [FromQuery] string? sourceName = null,
            [FromQuery] int? severity = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null)
        {
            // If 'since' is provided, return unpaginated results for real-time polling
            if (since.HasValue)
            {
                var newAlarms = await _context.AlarmEvents
                    .Where(a => a.EventTimeStamp.HasValue && a.EventTimeStamp.Value > since.Value)
                    .OrderBy(a => a.EventTimeStamp)
                    .ToListAsync();
                return Ok(newAlarms);
            }

            var query = ApplyFilters(sourceName, severity, fromDate, toDate);

            // Pagination logic
            var page = pageNumber ?? 1;
            var size = pageSize ?? 10;

            if (page < 1) page = 1;
            if (size < 1) size = 10;
            if (size > 10) size = 10;

            var totalCount = await query.CountAsync();
            var alarms = await query
                .OrderByDescending(a => a.EventTimeStamp)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return Ok(new PaginatedResponse<AlarmEvent>
            {
                Data = alarms,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            });
        }

        // GET: api/Alarms/export/excel
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery] string? sourceName = null,
            [FromQuery] int? severity = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var alarms = await ApplyFilters(sourceName, severity, fromDate, toDate)
                .OrderByDescending(a => a.EventTimeStamp)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Alarm History");
                var currentRow = 1;

                // Header
                worksheet.Cell(currentRow, 1).Value = "Timestamp";
                worksheet.Cell(currentRow, 2).Value = "Message";
                worksheet.Cell(currentRow, 3).Value = "Severity";
                worksheet.Cell(currentRow, 4).Value = "Source";
                worksheet.Cell(currentRow, 5).Value = "Category";
                worksheet.Cell(currentRow, 6).Value = "Status";

                var headerRange = worksheet.Range(1, 1, 1, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.SlateGray;
                headerRange.Style.Font.FontColor = XLColor.White;

                foreach (var alarm in alarms)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = alarm.EventTimeStamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "—";
                    worksheet.Cell(currentRow, 2).Value = alarm.Message ?? "—";
                    worksheet.Cell(currentRow, 3).Value = alarm.Severity?.ToString() ?? "0";
                    worksheet.Cell(currentRow, 4).Value = alarm.SourceName ?? "—";
                    worksheet.Cell(currentRow, 5).Value = alarm.EventCategory ?? "—";
                    worksheet.Cell(currentRow, 6).Value = (alarm.Active == true ? "ACTIVE" : "RESOLVED");
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AlarmReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        // GET: api/Alarms/export/pdf
        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPdf(
            [FromQuery] string? sourceName = null,
            [FromQuery] int? severity = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var alarms = await ApplyFilters(sourceName, severity, fromDate, toDate)
                .OrderByDescending(a => a.EventTimeStamp)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("ALARM HISTORY REPORT").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken3);
                            col.Item().Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}").FontSize(9).FontColor(Colors.Grey.Medium);
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Timestamp
                            columns.RelativeColumn(4); // Message
                            columns.RelativeColumn(1); // Sev
                            columns.RelativeColumn(2); // Source
                            columns.RelativeColumn(2); // Status
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Timestamp");
                            header.Cell().Element(CellStyle).Text("Message");
                            header.Cell().Element(CellStyle).Text("Sev");
                            header.Cell().Element(CellStyle).Text("Source");
                            header.Cell().Element(CellStyle).Text("Status");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold())
                                                .PaddingVertical(5)
                                                .BorderBottom(1)
                                                .BorderColor(Colors.Black);
                            }
                        });

                        foreach (var alarm in alarms)
                        {
                            table.Cell().Element(BodyStyle).Text(alarm.EventTimeStamp?.ToString("yyyy-MM-dd HH:mm") ?? "—");
                            table.Cell().Element(BodyStyle).Text(alarm.Message ?? "—");
                            table.Cell().Element(BodyStyle).Text(alarm.Severity?.ToString() ?? "0");
                            table.Cell().Element(BodyStyle).Text(alarm.SourceName ?? "—");
                            table.Cell().Element(BodyStyle).Text(alarm.Active == true ? "ACTIVE" : "RESOLVED");

                            static IContainer BodyStyle(IContainer container)
                            {
                                return container.BorderBottom(1)
                                                .BorderColor(Colors.Grey.Lighten2)
                                                .PaddingVertical(5);
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            using (var stream = new MemoryStream())
            {
                document.GeneratePdf(stream);
                return File(stream.ToArray(), "application/pdf", $"AlarmReport_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
        }
    }
}
