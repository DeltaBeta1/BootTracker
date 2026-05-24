using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BootTracker.Models;

namespace BootTracker.Services
{
    public static class ExcelExportService
    {
        public static void ExportRecords(List<BootRecord> records, string filePath)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            xml.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            xml.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            xml.AppendLine("<Styles>");
            xml.AppendLine("<Style ss:ID=\"h\"><Font ss:Bold=\"1\"/><Interior ss:Color=\"#4472C4\" ss:Pattern=\"Solid\"/></Style>");
            xml.AppendLine("</Styles>");
            xml.AppendLine("<Worksheet ss:Name=\"Boot Records\"><Table>");

            xml.AppendLine("<Row>");
            foreach (var h in new[] { "ID", "使用者", "事由", "审批人", "开机时间" })
            {
                xml.AppendLine("<Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">" + EscapeXml(h) + "</Data></Cell>");
            }
            xml.AppendLine("</Row>");

            foreach (var r in records)
            {
                xml.AppendLine("<Row>");
                xml.AppendLine("<Cell><Data ss:Type=\"Number\">" + r.Id + "</Data></Cell>");
                xml.AppendLine("<Cell><Data ss:Type=\"String\">" + EscapeXml(r.UserName) + "</Data></Cell>");
                xml.AppendLine("<Cell><Data ss:Type=\"String\">" + EscapeXml(r.Reason) + "</Data></Cell>");
                xml.AppendLine("<Cell><Data ss:Type=\"String\">" + EscapeXml(r.Approver) + "</Data></Cell>");
                xml.AppendLine("<Cell><Data ss:Type=\"String\">" + r.BootTime + "</Data></Cell>");
                xml.AppendLine("</Row>");
            }

            xml.AppendLine("</Table></Worksheet></Workbook>");
            File.WriteAllText(filePath, xml.ToString(), new UTF8Encoding(true));
        }

        public static void ExportStats(List<DayStats> dayData, List<UserStats> userData, string filePath)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            xml.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            xml.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            xml.AppendLine("<Styles><Style ss:ID=\"h\"><Font ss:Bold=\"1\"/><Interior ss:Color=\"#4472C4\" ss:Pattern=\"Solid\"/></Style></Styles>");

            xml.AppendLine("<Worksheet ss:Name=\"Daily Stats\"><Table>");
            xml.AppendLine("<Row><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">Date</Data></Cell><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">Count</Data></Cell></Row>");
            foreach (var d in dayData)
            {
                xml.AppendLine("<Row><Cell><Data ss:Type=\"String\">" + d.Day + "</Data></Cell><Cell><Data ss:Type=\"Number\">" + d.Count + "</Data></Cell></Row>");
            }
            xml.AppendLine("</Table></Worksheet>");

            xml.AppendLine("<Worksheet ss:Name=\"User Stats\"><Table>");
            xml.AppendLine("<Row><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">User</Data></Cell><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">Count</Data></Cell></Row>");
            foreach (var u in userData)
            {
                xml.AppendLine("<Row><Cell><Data ss:Type=\"String\">" + EscapeXml(u.UserName) + "</Data></Cell><Cell><Data ss:Type=\"Number\">" + u.Count + "</Data></Cell></Row>");
            }
            xml.AppendLine("</Table></Worksheet>");

            xml.AppendLine("</Workbook>");
            File.WriteAllText(filePath, xml.ToString(), new UTF8Encoding(true));
        }

        public static void ExportStats(List<DayStats> dayData, List<UserStats> userData, List<BootRecord> records, string filePath)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            xml.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            xml.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            xml.AppendLine("<Styles><Style ss:ID=\"h\"><Font ss:Bold=\"1\"/><Interior ss:Color=\"#4472C4\" ss:Pattern=\"Solid\"/></Style></Styles>");

            // Sheet 1: Daily Stats
            xml.AppendLine("<Worksheet ss:Name=\"Daily Stats\"><Table>");
            xml.AppendLine("<Row><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">Date</Data></Cell><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">Count</Data></Cell></Row>");
            foreach (var d in dayData)
                xml.AppendLine("<Row><Cell><Data ss:Type=\"String\">" + d.Day + "</Data></Cell><Cell><Data ss:Type=\"Number\">" + d.Count + "</Data></Cell></Row>");
            xml.AppendLine("</Table></Worksheet>");

            // Sheet 2: User Stats
            xml.AppendLine("<Worksheet ss:Name=\"User Stats\"><Table>");
            xml.AppendLine("<Row><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">User</Data></Cell><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">Count</Data></Cell></Row>");
            foreach (var u in userData)
                xml.AppendLine("<Row><Cell><Data ss:Type=\"String\">" + EscapeXml(u.UserName) + "</Data></Cell><Cell><Data ss:Type=\"Number\">" + u.Count + "</Data></Cell></Row>");
            xml.AppendLine("</Table></Worksheet>");

            // Sheet 3: Detail Records
            xml.AppendLine("<Worksheet ss:Name=\"Detail Records\"><Table>");
            xml.AppendLine("<Row>");
            foreach (var h in new[] { "序号", "使用人", "开机时间", "事由", "审批人" })
                xml.AppendLine("<Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">" + h + "</Data></Cell>");
            xml.AppendLine("</Row>");

            int seq = 0;
            foreach (var r in records)
            {
                seq++;
                xml.AppendLine("<Row>");
                xml.AppendLine("<Cell><Data ss:Type=\"Number\">" + seq + "</Data></Cell>");
                xml.AppendLine("<Cell><Data ss:Type=\"String\">" + EscapeXml(r.UserName) + "</Data></Cell>");
                xml.AppendLine("<Cell><Data ss:Type=\"String\">" + r.BootTime + "</Data></Cell>");
                xml.AppendLine("<Cell><Data ss:Type=\"String\">" + EscapeXml(r.Reason) + "</Data></Cell>");
                xml.AppendLine("<Cell><Data ss:Type=\"String\">" + EscapeXml(r.Approver) + "</Data></Cell>");
                xml.AppendLine("</Row>");
            }
            xml.AppendLine("</Table></Worksheet>");

            xml.AppendLine("</Workbook>");
            File.WriteAllText(filePath, xml.ToString(), new UTF8Encoding(true));
        }

        public static void ExportCompare(List<CompareItem> data, string filePath)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            xml.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            xml.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            xml.AppendLine("<Styles>");
            xml.AppendLine("<Style ss:ID=\"h\"><Font ss:Bold=\"1\"/><Interior ss:Color=\"#4472C4\" ss:Pattern=\"Solid\"/></Style>");
            xml.AppendLine("<Style ss:ID=\"red\"><Interior ss:Color=\"#FFC7CE\" ss:Pattern=\"Solid\"/></Style>");
            xml.AppendLine("</Styles>");
            xml.AppendLine("<Worksheet ss:Name=\"Compare\"><Table>");
            xml.AppendLine("<Row><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">Date</Data></Cell><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">System Boots</Data></Cell><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">Recorded</Data></Cell><Cell ss:StyleID=\"h\"><Data ss:Type=\"String\">Match</Data></Cell></Row>");

            foreach (var d in data)
            {
                var style = d.Match ? "" : " ss:StyleID=\"red\"";
                xml.AppendLine("<Row" + style + ">");
                xml.AppendLine("<Cell><Data ss:Type=\"String\">" + d.Day + "</Data></Cell>");
                xml.AppendLine("<Cell><Data ss:Type=\"Number\">" + d.SystemCount + "</Data></Cell>");
                xml.AppendLine("<Cell><Data ss:Type=\"Number\">" + d.RecordedCount + "</Data></Cell>");
                xml.AppendLine("<Cell><Data ss:Type=\"String\">" + (d.Match ? "Yes" : "No") + "</Data></Cell>");
                xml.AppendLine("</Row>");
            }

            xml.AppendLine("</Table></Worksheet></Workbook>");
            File.WriteAllText(filePath, xml.ToString(), new UTF8Encoding(true));
        }

        private static string EscapeXml(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                    .Replace("\"", "&quot;").Replace("'", "&apos;");
        }
    }
}
