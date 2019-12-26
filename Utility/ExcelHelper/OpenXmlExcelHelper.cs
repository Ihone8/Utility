using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
namespace Utility.ExcelHelper
{
    public sealed class OpenXmlExcelHelper
    {
        /*
         * excel Object structure
         * SpreadsheetDocument
         *   》WorkbookPart
	     *       》WorksheetPart
		 *           》Worksheet
		 *            》SheetData
	     *       》WorksheetPart
		 *          》Worksheet
		 *	            》SheetData1
	     *       》Workbook
		 *           》Sheets
		 *	            》Sheet
         */
        public static void Create(string filename, DataSet ds)
        {
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(filename, SpreadsheetDocumentType.Workbook);

            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            Workbook workbook = new Workbook();
            Sheets sheets = new Sheets();

            #region create more sheet 

            //create more sheet
            for (int s = 0; s < ds.Tables.Count; s++)
            {
                DataTable dt = ds.Tables[s];
                var tname = dt.TableName;

                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                Worksheet worksheet = new Worksheet();
                SheetData sheetData = new SheetData();

                //create sheet 
                Sheet sheet = new Sheet()
                {
                    // WorksheetPart
                    Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = UInt32Value.FromUInt32((uint)s + 1),
                    Name = tname
                };
                sheets.Append(sheet);

                #region 创建sheet 行
                Row row;
                uint rowIndex = 1;
                //add header
                row = new Row()
                {
                    RowIndex = UInt32Value.FromUInt32(rowIndex++)
                };
                sheetData.Append(row);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    Cell newCell = new Cell();
                    newCell.CellValue = new CellValue(dt.Columns[i].ColumnName);
                    newCell.DataType = new EnumValue<CellValues>(CellValues.String);
                    row.Append(newCell);
                }
                //add content
                object val = null;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    row = new Row()
                    {
                        RowIndex = UInt32Value.FromUInt32(rowIndex++)
                    };
                    sheetData.Append(row);

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        Cell newCell = new Cell();
                        val = dt.Rows[i][j];
                        newCell.CellValue = new CellValue(val.ToString());
                        newCell.DataType = new EnumValue<CellValues>(CellValues.String);

                        row.Append(newCell);
                    }

                }
                #endregion

                worksheet.Append(sheetData);
                worksheetPart.Worksheet = worksheet;
                worksheetPart.Worksheet.Save();
            }
            #endregion

            workbook.Append(sheets);
            workbookpart.Workbook = workbook;

            workbookpart.Workbook.Save();
            spreadsheetDocument.Close();
        }

        public static DataTable GetSheet(string filename, string sheetName)
        {
            DataTable dt = new DataTable();
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(filename, false))
            {
                WorkbookPart wbPart = document.WorkbookPart;
                //find sheet by sheetName
                Sheet sheet = wbPart
                    .Workbook
                    .Descendants<Sheet>()
                    .Where(s => s.Name == sheetName)
                    .FirstOrDefault();

                if (sheet == null)
                {
                    throw new ArgumentException("can`t find " + sheetName + " sheet page");
                }


                SharedStringTablePart sharedStringTablePart = wbPart
                    .GetPartsOfType<SharedStringTablePart>()
                    .FirstOrDefault();
                SharedStringTable sharedStringTable = null;
                if (sharedStringTablePart != null)
                    sharedStringTable = sharedStringTablePart.SharedStringTable;
                #region create datatable


                Func<Row, int> addTabColumn = (r) =>
                {

                    foreach (Cell c in r.Elements<Cell>())
                    {
                        dt.Columns.Add(GetCellVal(c, sharedStringTable));
                    }
                    return dt.Columns.Count;
                };
                //add row
                Action<Row> addTabRow = (r) =>
                {
                    DataRow dr = dt.NewRow();
                    int colIndex = 0;
                    int colCount = dt.Columns.Count;

                    foreach (Cell c in r.Elements<Cell>())
                    {
                        if (colIndex >= colCount)
                            break;
                        dr[colIndex++] = GetCellVal(c, sharedStringTable);
                    }
                    dt.Rows.Add(dr);
                };
                #endregion



                WorksheetPart worksheetPart
                    = wbPart.GetPartById(sheet.Id) as WorksheetPart;

                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();


                foreach (Row r in sheetData.Elements<Row>())
                {

                    if (r.RowIndex == 1)
                    {
                        addTabColumn(r);
                        continue;
                    }

                    addTabRow(r);
                }

            }
            return dt;
        }

        static string GetCellVal(Cell cell, SharedStringTable sharedStringTable)
        {
            var val = cell.InnerText;

            if (cell.DataType != null)
            {
                switch (cell.DataType.Value)
                {

                    case CellValues.SharedString:
                        if (sharedStringTable != null)
                            val = sharedStringTable
                                .ElementAt(int.Parse(val))
                                .InnerText;
                        break;
                    default:
                        val = string.Empty;
                        break;
                }

            }
            return val;
        }
    }
}
