using ClosedXML.Excel;
using OpenSpeed.Core.Models;

namespace OpenSpeed.Core.Export
{
  public static class SpeedResultExporter
  {
    public static void Export(
      string path,
      IEnumerable<SpeedStepMeasurement> steps,
      string stepHeader,
      string forwardHeader,
      string backwardHeader)
    {
      using var workbook = new XLWorkbook();
      var worksheet = workbook.AddWorksheet("Speeds");

      worksheet.Cell(1, 1).Value = stepHeader;
      worksheet.Cell(1, 2).Value = forwardHeader;
      worksheet.Cell(1, 3).Value = backwardHeader;
      worksheet.Row(1).Style.Font.Bold = true;

      var row = 2;
      foreach (var step in steps.OrderBy(s => s.SpeedStep))
      {
        worksheet.Cell(row, 1).Value = step.SpeedStep;
        if (step.ForwardPass is { } forward)
          worksheet.Cell(row, 2).Value = forward;
        if (step.BackwardPass is { } backward)
          worksheet.Cell(row, 3).Value = backward;
        row++;
      }

      worksheet.Columns().AdjustToContents();
      workbook.SaveAs(path);
    }
  }
}
