using EmployeeAnalyticsAPI.DataL;
using EmployeeAnalyticsAPI.DTOs;
using EmployeeAnalyticsAPI.Interface;
using EmployeeAnalyticsAPI.Models;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace EmployeeAnalyticsAPI.Services
{
    public class EmployeeReportService(AppDbContext context) : IEmployeeReportService
    {
        private readonly AppDbContext _context = context;

        public async Task<EmployeeReportDto?> GenerateEmployeeReportByIdAsync(
            int employeeId, CancellationToken ct = default)
        {
            var emp = await _context.EmployeesTble
                 .Include(e => e.Department)
                 .Include(e => e.SalariesTble)
                 .Include(e => e.EmployeeDetails)
                 .FirstOrDefaultAsync(e => e.Id == employeeId, ct);

            return emp == null ? null : MapToDto(emp);
        }

        public async Task<byte[]> GenerateEmployeeReportPdfByIdAsync(
            int employeeId, CancellationToken ct = default)
        {
            var report = await GenerateEmployeeReportByIdAsync(employeeId, ct)
               ?? throw new KeyNotFoundException(
                   $"Employee {employeeId} not found.");

            return GenerateSinglePdf(report);
        }

        public async Task<List<EmployeeReportDto>> GenerateEmployeeReportsAsync(
            CancellationToken ct = default)
        {
           var employees=_context.EmployeesTble
                .Include(e => e.Department)
                .Include(e => e.SalariesTble)
                .Include(e => e.EmployeeDetails)
                .ToList();
            var reports = employees.Select(MapToDto).ToList();
            return reports;
        }

        public async Task<byte[]> GenerateEmployeeReportsPdfAsync(
            CancellationToken ct = default)
        {
            var reports=await GenerateEmployeeReportsAsync(ct);
            if(!reports.Any())
                throw new InvalidOperationException(
                    "No employee reports available to generate PDF.");
            return GenerateAllPdf(reports);
        }
        private static EmployeeReportDto MapToDto(
           Employee emp)
        {
            var latestSalary = emp.SalariesTble
                .OrderByDescending(s => s.EffectiveFrom)
                .FirstOrDefault();

            var details=emp.EmployeeDetails?.FirstOrDefault();

            var gross = latestSalary?.Amount ?? 0;
            var basic = gross * 0.40m;
            var hra = gross * 0.20m;
            var pfDeduct = basic * 0.12m;
            var net = gross - pfDeduct;

            return new EmployeeReportDto
            {
                EmployeeId = emp.Id,    
                FullName = $"{emp.FirstName} {emp.LastName}",
                Department = emp.Department?.DName,
                HireDate = emp.HireDate,
                CurrentSalary = gross,
                BasicSalary = basic,
                HRA = hra,
                PFDeduction = pfDeduct,
                NetSalary = net,
                BankName = details?.BankName,
                AccountNumber =details?.AccountNumber,
                UANNumber = details?.UANNumber,
                PFNumber = details?.PFNumber,
                PanNumber = details?.PanNumber,
                AadharNumber = details?.AadharNumber,
                Address = details?.Address,
                City = details?.City,
                State = details?.State,
                GeneratedAt = DateTime.UtcNow
            };
        }
        private static byte[] GenerateSinglePdf(EmployeeReportDto r)
        {
            using var doc = new PdfDocument();
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            double w = page.Width.Point;

            // ── Fonts ────────────────────────────────────────────
            var fTitle = new XFont("Arial", 16, XFontStyle.Bold);
            var fBold = new XFont("Arial", 11, XFontStyle.Bold);
            var fNormal = new XFont("Arial", 10, XFontStyle.Regular);
            var fSmall = new XFont("Arial", 8, XFontStyle.Regular);

            // ── Colors ───────────────────────────────────────────
            var cBlue = XColor.FromArgb(26, 35, 126);
            var cLight = XColor.FromArgb(232, 234, 246);
            var cGray = XColor.FromArgb(100, 100, 100);

            double y = 30;

            // ── Header ───────────────────────────────────────────
            gfx.DrawRectangle(
                new XSolidBrush(cGray), 0, y, w, 60);
            gfx.DrawString(
                "AshishIT Solutions Pvt. Ltd.",
                new XFont("Arial", 15, XFontStyle.Bold),
                XBrushes.White,
                new XRect(0, y + 8, w, 25),
                XStringFormats.TopCenter);
            gfx.DrawString(
                "EMPLOYEE Details REPORT",
                new XFont("Arial", 11, XFontStyle.Regular),
                XBrushes.White,
                new XRect(0, y + 33, w, 20),
                XStringFormats.TopCenter);
            y += 75;

            // ── Name Banner ──────────────────────────────────────
            gfx.DrawRectangle(
                new XSolidBrush(cLight), 30, y, w - 60, 34);
            gfx.DrawString(
                $"{r.FullName}",
                fTitle,
                new XSolidBrush(cGray),
                new XRect(30, y + 7, w - 60, 25),
                XStringFormats.TopCenter);
            y += 48;

            // ── Section Helper ───────────────────────────────────
            void DrawSection(string title)
            {
                gfx.DrawRectangle(
                    new XSolidBrush(cGray), 30, y, w - 60, 24);
                gfx.DrawString(
                    title, fBold, XBrushes.White,
                    new XRect(40, y + 5, w - 60, 20),
                    XStringFormats.TopLeft);
                y += 28;
            }

            // ── Row Helper ───────────────────────────────────────
            int rowIdx = 0;
            void DrawRow(string label, string? value)
            {
                var bg = rowIdx % 2 == 0
                    ? new XSolidBrush(cLight)
                    : new XSolidBrush(XColors.White);

                gfx.DrawRectangle(bg, 30, y, w - 60, 22);
                gfx.DrawString(
                    label, fNormal,
                    new XSolidBrush(cGray),
                    new XRect(40, y + 4, 190, 18),
                    XStringFormats.TopLeft);
                gfx.DrawString(
                    value ?? "N/A", fNormal,
                    new XSolidBrush(XColors.Black),
                    new XRect(235, y + 4, w - 265, 18),
                    XStringFormats.TopLeft);
                y += 24;
                rowIdx++;
            }

            // ── 1. Personal Information ──────────────────────────
            rowIdx = 0;
            DrawSection("1.  Personal Information");
            DrawRow("Employee ID", r.EmployeeId.ToString());
            DrawRow("Full Name", r.FullName);
            DrawRow("Department", r.Department);
            DrawRow("Date of Join", r.HireDate.ToString("yyyy mm dd"));
            DrawRow("PAN Number", r.PanNumber);
            DrawRow("Aadhar Number", r.AadharNumber);
            DrawRow("Address",
                $"{r.Address ?? "N/A"}, " +
                $"{r.City ?? ""}, {r.State ?? ""}");
            y += 10;

            // ── 2. Salary Breakdown ──────────────────────────────
            rowIdx = 0;
            DrawSection("2.  Salary Breakdown");
            DrawRow("Gross Salary (CTC)",
                r.CurrentSalary.ToString("C2"));
            DrawRow("Basic Salary (40%)",
                r.BasicSalary.ToString("C2"));
            DrawRow("HRA (20%)",
                r.HRA.ToString("C2"));
            DrawRow("PF Deduction (12% of Basic)",
                r.PFDeduction.ToString("C2"));
            DrawRow("Net Salary (Take Home)",
                r.NetSalary.ToString("C2"));
            y += 10;

            // ── 3. PF and UAN Details ────────────────────────────
            rowIdx = 0;
            DrawSection("3.  PF and UAN Details");
            DrawRow("PF Number", r.PFNumber);
            DrawRow("UAN Number", r.UANNumber);
            y += 10;

            // ── 4. Bank Details ──────────────────────────────────
            rowIdx = 0;
            DrawSection("4.  Bank Details");
            DrawRow("Bank Name", r.BankName);
            DrawRow("Account Number", r.AccountNumber);
            y += 16;

            // ── Footer ───────────────────────────────────────────
            gfx.DrawLine(XPens.LightGray, 30, y, w - 30, y);
            y += 8;
            gfx.DrawString(
                $"Generated: {r.GeneratedAt:dd MMM yyyy HH:mm} UTC" +
                $"  |  AshishIT Solutions Pvt. Ltd." +
                $"  |  Confidential",
                fSmall,
                new XSolidBrush(cGray),
                new XRect(30, y, w - 60, 14),
                XStringFormats.TopCenter);

            using var ms = new MemoryStream();
            doc.Save(ms, false);
            return ms.ToArray();
        }
        private static byte[] GenerateAllPdf(
            List<EmployeeReportDto> reports)
        {
            using var doc = new PdfDocument();

            // ── Summary Page ─────────────────────────────────────
            var sumPage = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(sumPage);
            double w = sumPage.Width.Point;

            var fBold = new XFont("Arial", 10, XFontStyle.Bold);
            var fNormal = new XFont("Arial", 9, XFontStyle.Regular);
            var fSmall = new XFont("Arial", 8, XFontStyle.Regular);
            var cBlue = XColor.FromArgb(26, 35, 126);
            var cLight = XColor.FromArgb(232, 234, 246);
            var cGray = XColor.FromArgb(100, 100, 100);

            // Header
            gfx.DrawRectangle(
                new XSolidBrush(cGray), 0, 30, w, 60);
            gfx.DrawString(
                "AshishIT Solutions Pvt. Ltd.",
                new XFont("Arial", 15, XFontStyle.Bold),
                XBrushes.White,
                new XRect(0, 38, w, 25),
                XStringFormats.TopCenter);
            gfx.DrawString(
                "ALL EMPLOYEES REPORT — SUMMARY",
                new XFont("Arial", 11, XFontStyle.Regular),
                XBrushes.White,
                new XRect(0, 62, w, 20),
                XStringFormats.TopCenter);

            double y = 105;
            gfx.DrawString(
                $"Total Employees: {reports.Count}   |   " +
                $"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC",
                fSmall,
                new XSolidBrush(cGray),
                new XRect(30, y, w - 60, 16),
                XStringFormats.TopLeft);
            y += 22;

            // Table header
            double[] cols = { 35, 130, 100, 85, 85, 85, 80 };
            string[] headers =
            {
                "ID", "Name", "Department",
                "Gross", "PF", "Net", "Bank"
            };

            gfx.DrawRectangle(
                new XSolidBrush(cGray), 30, y, w - 60, 22);
            double cx = 35;
            foreach (var (h, cw) in headers.Zip(cols))
            {
                gfx.DrawString(h, fBold, XBrushes.White,
                    new XRect(cx, y + 4, cw, 16),
                    XStringFormats.TopLeft);
                cx += cw;
            }
            y += 24;

            // Table rows
            int idx = 0;
            foreach (var r in reports)
            {
                if (y > sumPage.Height.Point - 60) break;

                var bg = idx % 2 == 0
                    ? new XSolidBrush(cLight)
                    : new XSolidBrush(XColors.White);

                gfx.DrawRectangle(bg, 30, y, w - 60, 20);
                cx = 35;

                var vals = new[]
                {
                    r.EmployeeId.ToString(),
                    r.FullName?.Length > 16
                        ? r.FullName[..16] + "…" : r.FullName,
                    r.Department?.Length > 12
                        ? r.Department[..12] + "…" : r.Department,
                    r.CurrentSalary.ToString("N0"),
                    r.PFDeduction.ToString("N0"),
                    r.NetSalary.ToString("N0"),
                    r.BankName?.Length > 10
                        ? r.BankName[..10] + "…" : r.BankName,
                };

                foreach (var (v, cw) in vals.Zip(cols))
                {
                    gfx.DrawString(
                        v ?? "N/A", fNormal,
                        new XSolidBrush(XColors.Black),
                        new XRect(cx, y + 3, cw, 16),
                        XStringFormats.TopLeft);
                    cx += cw;
                }
                y += 22;
                idx++;
            }

            // Individual pages per employee
            foreach (var r in reports)
            {
                var bytes = GenerateSinglePdf(r);
                using var ms2 = new MemoryStream(bytes);
                var tmpDoc = PdfReader.Open(
                    ms2, PdfDocumentOpenMode.Import);
                doc.AddPage(tmpDoc.Pages[0]);
            }

            using var ms = new MemoryStream();
            doc.Save(ms, false);
            return ms.ToArray();
        }
    }
}
