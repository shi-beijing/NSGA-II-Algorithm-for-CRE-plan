using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;

namespace NSGA_II_Algorithm.models
{
    public static class Excel
    {
        /// <summary>  
        /// Import excel to datatable  
        /// </summary>  
        /// <param name="filePath">excelpath</param>  

        /// <returns>返回datatable</returns>  
        public static List<DataTable> ExcelToDataTable(int sheetNumber)
        {
            List<DataTable> dataTables = new List<DataTable>();
            for (int sheetIndex = 0; sheetIndex < sheetNumber; sheetIndex++)
            {
           
                string filePath = "large scale dataset.xlsx";
               
                DataTable dataTable = null;
                FileStream fs = null;
                DataColumn column = null;
                DataRow dataRow = null;
                IWorkbook workbook = null;
                ISheet sheet = null;
                IRow row = null;
                ICell cell = null;
                int startRow = 0;
                try
                {
                    using (fs = File.OpenRead(filePath))
                    {
                        workbook = new XSSFWorkbook(fs);
                        if (workbook != null)
                        {
                            sheet = workbook.GetSheetAt(sheetIndex);//
                            dataTable = new DataTable();
                            if (sheet != null)
                            {
                                int rowCount = sheet.LastRowNum;// 
                                if (rowCount > 0)
                                {
                                    IRow firstRow = sheet.GetRow(0);//
                                    int cellCount = firstRow.LastCellNum;//

                      
                                    startRow = 1;
                                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                    {
                                        cell = firstRow.GetCell(i);
                                        if (cell != null)
                                        {
                                            if (cell.StringCellValue != null)
                                            {
                                                column = new DataColumn(cell.StringCellValue);
                                                dataTable.Columns.Add(column);
                                            }
                                        }
                                    }
                                    
                                    for (int i = startRow; i <= rowCount; ++i)
                                    {
                                        row = sheet.GetRow(i);
                                        if (row == null) continue;

                                        dataRow = dataTable.NewRow();
                                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                                        {
                                            cell = row.GetCell(j);
                                            if (cell == null)
                                            {
                                                dataRow[j] = "";
                                            }
                                            else
                                            {
                                                //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)  
                                                switch (cell.CellType)
                                                {
                                                    case CellType.Blank:
                                                        dataRow[j] = "";
                                                        break;
                                                    case CellType.Numeric:
                                                        // short format = cell.CellStyle.DataFormat;
                                                      
                                                        //if (format == 14 || format == 31 || format == 57 || format == 58)
                                                        //    dataRow[j] = cell.DateCellValue;
                                                        //else
                                                        dataRow[j] = cell.NumericCellValue;
                                                        break;
                                                    case CellType.String:
                                                        dataRow[j] = cell.StringCellValue;
                                                        break;
                                                    case CellType.Formula:
                                                        dataRow[j] = cell.NumericCellValue;
                                                        break;
                                                }
                                            }
                                        }
                                        dataTable.Rows.Add(dataRow);
                                    }
                                }
                            }
                        }
                    }
                    dataTables.Add(dataTable);

                }
                catch (Exception)
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                    return null;
                }

            }
            return dataTables;

        }
    }
}
