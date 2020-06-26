using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;

namespace Small.Tools.Common
{
    /// <summary>
    /// Excel Common
    /// </summary>
    public static class ExcelCommon
    {
        /// <summary>
        /// DataTable To Excel
        /// 单列头导出。
        /// </summary>
        /// <param name="dataTable">需导出数据源</param>
        /// <param name="saveFileFullPath">“Excel”保存绝对路径</param>
        /// <param name="columnHeader">“Excel”列头,格式: "Name","姓名" | colunmnName,列头显示名</param>
        /// <param name="errorMessage">错误信息</param>
        /// <param name="columnHeaderFormater">“Excel”值需转换的格式,"生日","yyyy-mm-dd"| 列头显示名,format</param>
        /// <param name="sheetName">sheet name</param>
        /// <returns>bool</returns>
        public static bool ToExcel(DataTable dataTable, string saveFileFullPath
            , Dictionary<string, string> columnHeader, ref string errorMessage
            , Dictionary<string, string> columnHeaderFormater = null, string sheetName = "sheet")
        {
            try
            {
                if (dataTable == null) throw new Exception("数据源“DataTable”不能为Null。");
                if (string.IsNullOrWhiteSpace(saveFileFullPath)) throw new ArgumentNullException("“Excel” 保存路径不能为空(绝对路径)。");
                if (columnHeader.Count <= 0) throw new Exception("“Excel”列头不能为Null。");

                //保存路径是否存在
                string path = Path.GetDirectoryName(saveFileFullPath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                IWorkbook book = null;
                if (Path.GetExtension(saveFileFullPath).ToLower().Trim() == ".xlsx".ToLower().Trim()) book = new XSSFWorkbook();    //2007 + 版本
                else if (Path.GetExtension(saveFileFullPath).ToLower().Trim() == ".xls".ToLower().Trim()) book = new HSSFWorkbook();    //2003 版本

                if (dataTable.Rows.Count <= 0)
                {
                    errorMessage = "“DataTable”数据条数为: 0";
                    return false;
                }

                if (book != null)
                {
                    var sheet = book.CreateSheet(sheetName);
                    var headerCellStyle = GetCellStyle(book, true);
                    Dictionary<int, ICellStyle> colStyles = new Dictionary<int, ICellStyle>();
                    IRow rowHeader = sheet.CreateRow(0);

                    int index = 0;
                    var headerColNames = new List<string>();

                    foreach (var headher in columnHeader)
                    {
                        ICell headerCell = rowHeader.CreateCell(index);
                        headerCell.SetCellValue(headher.Value);
                        headerCell.CellStyle = headerCellStyle;

                        if (columnHeaderFormater != null && columnHeaderFormater.ContainsKey(headher.Value))
                            colStyles[index] = GetCellStyleWithDataFormat(book, columnHeaderFormater[headher.Value]);
                        else
                            colStyles[index] = GetCellStyle(book);
                        headerColNames.Add(headher.Key);
                        index++;
                    }

                    //填充内容
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        IRow irow = sheet.CreateRow(i + 1);
                        var row = dataTable.Rows[i];
                        for (int j = 0; j < headerColNames.Count; j++)
                        {
                            ICell cell = irow.CreateCell(j);
                            string colName = headerColNames[j];
                            string cellValue = row[colName].ToString();
                            SetCellValue(cell, cellValue, dataTable.Columns[colName].DataType, colStyles);
                        }
                    }
                }

                //保存“Excel”文件
                Save(book, saveFileFullPath);

                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex) { throw new Exception($"“Excel”导出失败,Error:{ex.Message}"); }
        }

        /// <summary>
        /// Excel To DataTable
        /// 默认第一行为标题。
        /// </summary>
        /// <param name="fileFullPath">“Excel”文件绝对路径</param>
        /// <param name="sheetName">sheet Name</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable(string fileFullPath, string sheetName = null)
        {
            if (string.IsNullOrWhiteSpace(fileFullPath)) throw new ArgumentNullException("“Excel” 路径不能为空。");
            if (!File.Exists(fileFullPath)) throw new Exception("“Excel” 文件不存在。");
            FileStream fileStream = null;
            DataTable dataTable = new DataTable();

            IWorkbook workbook = null;
            ISheet sheet = null;
            try
            {
                using (fileStream = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read))
                {
                    if (Path.GetExtension(fileFullPath).ToLower().Trim() == ".xlsx".ToLower().Trim()) workbook = new XSSFWorkbook(fileStream);    //2007 + 版本
                    else if (Path.GetExtension(fileFullPath).ToLower().Trim() == ".xls".ToLower().Trim()) workbook = new HSSFWorkbook(fileStream);    //2003 版本
                    else throw new Exception("导入文件只可是“Excel”。");

                    if (workbook != null)
                    {
                        //sheet
                        if (!string.IsNullOrWhiteSpace(sheetName)) sheet = workbook.GetSheet(sheetName);
                        else sheet = workbook.GetSheetAt(0);

                        if (sheet != null)
                        {
                            dataTable.TableName = sheet.SheetName;
                            var headerRow = sheet.GetRow(0);
                            int cellCount = headerRow.LastCellNum;  //最后一行列数（即为总列数）

                            //获取第一行标题列数据源,转换为dataTable数据源的表格标题名称
                            for (var j = 0; j < cellCount; j++)
                            {
                                var cell = headerRow.GetCell(j);
                                dataTable.Columns.Add(cell.ToString());
                            }

                            //获取Excel表格中除标题以为的所有数据源，转化为dataTable中的表格数据源
                            for (var i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                            {
                                var dataRow = dataTable.NewRow();
                                var row = sheet.GetRow(i);
                                if (row == null) continue; //没有数据的行默认是null　

                                for (int j = row.FirstCellNum; j < cellCount; j++)
                                {
                                    if (row.GetCell(j) != null)//单元格内容非空验证
                                    {
                                        //获取指定的单元格信息
                                        var cell = row.GetCell(j);
                                        dataRow[j] = GetTypeValueByCell(cell, workbook);
                                    }
                                }
                                dataTable.Rows.Add(dataRow);
                            }


                        }
                    }
                }
                return dataTable;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// Excel To DataTable
        /// 第几行开始读取,“headerRowIndex”
        /// </summary>
        /// <param name="fileFullPath">“Excel”文件绝对路径</param>
        /// <param name="sheetIndexOrName">sheet Name Or sheet Index</param>
        /// <param name="headerRowIndex">第几行开始读取</param>
        /// <param name="startColIndex">开始列</param>
        /// <param name="colCount">结束列</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTableByHeaderRow(string fileFullPath, string sheetIndexOrName, int headerRowIndex = 0, short startColIndex = 0, short colCount = 0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileFullPath)) throw new ArgumentNullException("“Excel” 路径不能为空。");
                if (!File.Exists(fileFullPath)) throw new Exception("“Excel” 文件不存在。");
                IWorkbook workbook = null;
                FileStream fileStream = null;

                DataTable dataTable = new DataTable();
                using (fileStream = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read))
                {
                    if (Path.GetExtension(fileFullPath).ToLower().Trim() == ".xlsx".ToLower().Trim()) workbook = new XSSFWorkbook(fileStream);    //2007 + 版本
                    else if (Path.GetExtension(fileFullPath).ToLower().Trim() == ".xls".ToLower().Trim()) workbook = new HSSFWorkbook(fileStream);    //2003 版本
                    else throw new Exception("导入文件只可是“Excel”。");

                    ISheet sheet = GetSheet(workbook, sheetIndexOrName);
                    if (sheet == null) throw new Exception("没有解析到响应“Sheet” 。 ");

                    IRow headerRow = sheet.GetRow(headerRowIndex);
                    int cellFirstNum = (startColIndex > headerRow.FirstCellNum ? startColIndex : headerRow.FirstCellNum);
                    int cellCount = (colCount > 0 && colCount < headerRow.LastCellNum ? colCount : headerRow.LastCellNum);

                    for (int i = cellFirstNum; i < cellCount; i++)
                    {
                        if (headerRow.GetCell(i) == null || headerRow.GetCell(i).StringCellValue.Trim() == "")
                        {
                            //如果遇到第一个空列，则不再继续向后读取
                            cellCount = i;
                            break;
                        }
                        DataColumn dataColumn = new DataColumn(headerRow.GetCell(i).StringCellValue);
                        dataTable.Columns.Add(dataColumn);
                    }

                    for (int i = (headerRowIndex + 1); i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row != null)
                        {
                            List<string> cellValues = new List<string>();
                            for (int j = cellFirstNum; j < cellCount; j++)
                            {
                                if (row.GetCell(j) != null)
                                    cellValues.Add(IsNotNullString(row.GetCell(j)));
                                else
                                    cellValues.Add(string.Empty);
                            }
                            dataTable.Rows.Add(cellValues.ToArray());
                        }
                    }
                }
                return dataTable;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// “Excel IWorkbook” Save
        /// </summary>
        /// <param name="book">IWorkbook</param>
        /// <param name="filePath">保存路径</param>
        /// <returns>string</returns>
        public static string Save(this IWorkbook book, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("“Excel” 保存路径不能为空(绝对路径)。");

            if (File.Exists(filePath))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite)) { book.Write(fileStream); }
            return filePath;
        }

        #region > 自定义列头导出 

        /// <summary>
        /// 先创建行,然后在创建对应的列
        /// 创建“Excel”中指定的行。
        /// </summary>
        /// <param name="sheet">sheet</param>
        /// <param name="rowNum">创建第几行(从0开始)</param>
        /// <param name="rowHeight">行高</param>
        /// <returns>HSSFRow</returns>
        public static HSSFRow CreateRow(ISheet sheet, int rowNum, float rowHeight)
        {
            HSSFRow hssfRow = (HSSFRow)sheet.CreateRow(rowNum); //创建行
            hssfRow.HeightInPoints = rowHeight; //设置列头行高
            return hssfRow;
        }

        /// <summary>
        /// 创建行内指定的单元格
        /// </summary>
        /// <param name="hssfRow">需要创建单元格的行</param>
        /// <param name="cellStyle">单元格样式</param>
        /// <param name="cellNum">创建第几个单元格(从0开始)</param>
        /// <param name="cellValue">给单元格赋值</param>
        /// <returns>HSSFCell</returns>
        public static HSSFCell CreateCells(HSSFRow hssfRow, HSSFCellStyle cellStyle, int cellNum, string cellValue)
        {
            HSSFCell hssfCell = (HSSFCell)hssfRow.CreateCell(cellNum); //创建单元格

            //将样式绑定到单元格
            hssfCell.CellStyle = cellStyle;
            if (!string.IsNullOrWhiteSpace(cellValue))
                hssfCell.SetCellValue(cellValue);   //单元格赋值
            return hssfCell;
        }

        /// <summary>
        /// 行内单元格常用样式设置
        /// </summary>
        /// <param name="workbook">“Excel”文件对象</param>
        /// <param name="hAlignment">水平布局方式</param>
        /// <param name="vAlignment">垂直布局方式</param>
        /// <param name="fontHeightInPoints">字体大小</param>
        /// <param name="isAddBorder">是否需要边框</param>
        /// <param name="boldWeight">字体加粗 (None = 0,Normal = 400，Bold = 700</param>
        /// <param name="fontName">字体（仿宋，楷体，宋体，微软雅黑...与Excel主题字体相对应）</param>
        /// <param name="isAddBorderColor">是否增加边框颜色</param>
        /// <param name="isItalic">是否将文字变为斜体</param>
        /// <param name="isLineFeed">是否自动换行</param>
        /// <param name="isAddCellBackground">是否增加单元格背景颜色</param>
        /// <param name="fillPattern">填充图案样式(FineDots 细点，SolidForeground立体前景，isAddFillPattern=true时存在)</param>
        /// <param name="cellBackgroundColor">单元格背景颜色（当isAddCellBackground=true时存在）</param>
        /// <param name="fontColor">字体颜色</param>
        /// <param name="underlineStyle">下划线样式（无下划线[None],单下划线[Single],双下划线[Double],会计用单下划线[SingleAccounting],会计用双下划线[DoubleAccounting]）</param>
        /// <param name="typeOffset">字体上标下标(普通默认值[None],上标[Sub],下标[Super]),即字体在单元格内的上下偏移量</param>
        /// <param name="isStrikeout">是否显示删除线</param>
        /// <returns></returns>
        public static HSSFCellStyle CreateStyle(HSSFWorkbook workbook, HorizontalAlignment hAlignment, VerticalAlignment vAlignment
            , short fontHeightInPoints, bool isAddBorder, short boldWeight, string fontName = "宋体", bool isAddBorderColor = true
            , bool isItalic = false, bool isLineFeed = false, bool isAddCellBackground = false, FillPattern fillPattern = FillPattern.NoFill
            , short cellBackgroundColor = HSSFColor.Yellow.Index, short fontColor = HSSFColor.Black.Index
            , FontUnderlineType underlineStyle = FontUnderlineType.None, FontSuperScript typeOffset = FontSuperScript.None, bool isStrikeout = false)
        {
            HSSFCellStyle cellStyle = (HSSFCellStyle)workbook.CreateCellStyle(); //创建列头单元格实例样式
            cellStyle.Alignment = hAlignment; //水平居中
            cellStyle.VerticalAlignment = vAlignment; //垂直居中
            cellStyle.WrapText = isLineFeed;//自动换行



            //背景颜色，边框颜色，字体颜色都是使用 HSSFColor属性中的对应调色板索引，关于 HSSFColor 颜色索引对照表，详情参考：https://www.cnblogs.com/Brainpan/p/5804167.html

            //TODO：引用了NPOI后可通过ICellStyle 接口的 FillForegroundColor 属性实现 Excel 单元格的背景色设置，FillPattern 为单元格背景色的填充样式

            //TODO:十分注意，要设置单元格背景色必须是FillForegroundColor和FillPattern两个属性同时设置，否则是不会显示背景颜色
            if (isAddCellBackground)
            {
                cellStyle.FillForegroundColor = cellBackgroundColor;//单元格背景颜色
                cellStyle.FillPattern = fillPattern;//填充图案样式(FineDots 细点，SolidForeground立体前景)
            }


            //是否增加边框
            if (isAddBorder)
            {
                //常用的边框样式 None(没有),Thin(细边框，瘦的),Medium(中等),Dashed(虚线),Dotted(星罗棋布的),Thick(厚的),Double(双倍),Hair(头发)[上右下左顺序设置]
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
            }

            //是否设置边框颜色
            if (isAddBorderColor)
            {
                //边框颜色[上右下左顺序设置]
                cellStyle.TopBorderColor = HSSFColor.DarkGreen.Index;//DarkGreen(黑绿色)
                cellStyle.RightBorderColor = HSSFColor.DarkGreen.Index;
                cellStyle.BottomBorderColor = HSSFColor.DarkGreen.Index;
                cellStyle.LeftBorderColor = HSSFColor.DarkGreen.Index;
            }

            /**
             * 设置相关字体样式
             */
            var cellStyleFont = (HSSFFont)workbook.CreateFont(); //创建字体

            //假如字体大小只需要是粗体的话直接使用下面该属性即可
            //cellStyleFont.IsBold = true;

            cellStyleFont.Boldweight = boldWeight; //字体加粗
            cellStyleFont.FontHeightInPoints = fontHeightInPoints; //字体大小
            cellStyleFont.FontName = fontName;//字体（仿宋，楷体，宋体 ）
            cellStyleFont.Color = fontColor;//设置字体颜色
            cellStyleFont.IsItalic = isItalic;//是否将文字变为斜体
            cellStyleFont.Underline = underlineStyle;//字体下划线
            cellStyleFont.TypeOffset = typeOffset;//字体上标下标
            cellStyleFont.IsStrikeout = isStrikeout;//是否有删除线

            cellStyle.SetFont(cellStyleFont); //将字体绑定到样式
            return cellStyle;
        }

        #endregion

        #region > excel private method

        public static string IsNotNullString(object obj)
        {
            if (obj == null || obj == DBNull.Value)
                return string.Empty;
            return obj.ToString();
        }

        private static ISheet GetSheet(IWorkbook workbook, string sheetIndexOrName = null)
        {
            int sheetIndex = 0;
            ISheet sheet = null;

            if (!string.IsNullOrWhiteSpace(sheetIndexOrName))
            {
                if (int.TryParse(sheetIndexOrName, out sheetIndex))
                    sheet = workbook.GetSheetAt(sheetIndex);
                else
                    sheet = workbook.GetSheet(sheetIndexOrName);
            }

            if (sheet == null)
                sheet = workbook.GetSheetAt(0);
            return sheet;
        }

        /// <summary>
        /// 是否为可空类型
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>bool</returns>
        private static bool IsNullableType(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

        private static void SetCellValue(ICell cell, string value, Type cellType, IDictionary<int, ICellStyle> cellStyle)
        {
            if (IsNullableType(cellType))    //判断是否可空类型
                cellType = cellType.GetGenericArguments()[0];

            string dataFormatValue = string.Empty;
            switch (cellType.ToString())
            {
                //字符串类型
                case "System.String":
                    cell.SetCellType(CellType.String);
                    cell.SetCellValue(value);
                    break;

                //日期类型
                case "System.DateTime":
                    DateTime dateTime = new DateTime();
                    if (DateTime.TryParse(value, out dateTime))
                        cell.SetCellValue(dateTime);
                    else
                        cell.SetCellValue(value);
                    dataFormatValue = "yyyy/mm/dd hh:mm:ss";
                    break;

                //布尔型
                case "System.Boolean":
                    bool boolValue = false;
                    if (bool.TryParse(value, out boolValue))
                    {
                        cell.SetCellType(CellType.Boolean);
                        cell.SetCellValue(boolValue);
                    }
                    else { cell.SetCellValue(value); }
                    break;

                //整型
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Byte":
                    int resultValue = 0;
                    if (int.TryParse(value, out resultValue))
                    {
                        cell.SetCellType(CellType.Numeric);
                        cell.SetCellValue(resultValue);
                    }
                    else { cell.SetCellValue(value); }
                    dataFormatValue = "0";
                    break;

                //浮点型
                case "System.Decimal":
                case "System.Double":
                    double doubleValue = 0;
                    if (double.TryParse(value, out doubleValue))
                    {
                        cell.SetCellType(CellType.Numeric);
                        cell.SetCellValue(doubleValue);
                    }
                    else { cell.SetCellValue(value); }
                    dataFormatValue = "0.00";
                    break;

                //空值处理
                case "System.DBNull":
                    cell.SetCellType(CellType.Blank);
                    cell.SetCellValue(string.Empty);
                    break;
                default:
                    cell.SetCellType(CellType.Unknown);
                    cell.SetCellValue(value);
                    break;
            }

            //没有设置，则采用默认类型格式
            if (!string.IsNullOrEmpty(dataFormatValue) && cellStyle[cell.ColumnIndex].DataFormat <= 0)
            {
                cellStyle[cell.ColumnIndex] = GetCellStyleWithDataFormat(cell.Sheet.Workbook, dataFormatValue);
            }
            cell.CellStyle = cellStyle[cell.ColumnIndex];
            SetColumnWidth(cell.Sheet, cell);
        }
        private static void SetColumnWidth(ISheet sheet, ICell cell)
        {
            const int maxLength = 60 * 256;
            int cellLength = (Encoding.Default.GetBytes(cell.ToString()).Length + 2) * 256;
            if (cellLength > maxLength)  //当单元格内容超过30个中文字符（英语60个字符）宽度，则强制换行
            {
                cellLength = maxLength;
                cell.CellStyle.WrapText = true;
            }
            int colWidth = sheet.GetColumnWidth(cell.ColumnIndex);
            if (colWidth < cellLength)
            {
                sheet.SetColumnWidth(cell.ColumnIndex, cellLength);
            }
        }
        private static ICellStyle GetCellStyle(IWorkbook workbook, bool isHeaderRow = false)
        {
            ICellStyle cellStyle = workbook.CreateCellStyle();
            if (isHeaderRow)
            {
                cellStyle.FillPattern = FillPattern.SolidForeground;
                cellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                IFont font = workbook.CreateFont();
                font.FontHeightInPoints = 11D;
                font.Boldweight = (short)FontBoldWeight.Bold;
                cellStyle.SetFont(font);
            }

            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            return cellStyle;
        }
        private static ICellStyle GetCellStyleWithDataFormat(IWorkbook workbook, string format)
        {
            ICellStyle style = GetCellStyle(workbook);
            var dataFormat = workbook.CreateDataFormat();
            short formatId = -1;
            if (dataFormat is HSSFDataFormat)
            {
                formatId = HSSFDataFormat.GetBuiltinFormat(format);
            }
            if (formatId != -1)
            {
                style.DataFormat = formatId;
            }
            else
            {
                style.DataFormat = dataFormat.GetFormat(format);
            }
            return style;
        }

        /// <summary>
        /// 通过单元格格式获取值
        /// </summary>
        /// <returns>string</returns>
        private static string GetTypeValueByCell(ICell cell, IWorkbook workbook)
        {
            switch (cell.CellType)
            {
                //其他数字类型,在NPOI中数字和日期都属于Numeric类型
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                        return cell.DateCellValue.ToString().Trim();
                    else
                        return cell.NumericCellValue.ToString().Trim();

                //空数据类型
                case CellType.Blank: return "";

                //Boolean
                case CellType.Boolean: return cell.BooleanCellValue.ToString();

                //公式类型
                case CellType.Formula: return new HSSFFormulaEvaluator(workbook).Evaluate(cell).StringValue;

                //其他
                default:
                    return cell.StringCellValue.ToString().Trim();
            }
        }

        #endregion
    }
}
