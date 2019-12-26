using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.ExcelHelper
{
    /// <summary>
    ///  使用 IO 
    /// </summary>
    class IOExportCSV
    {

        public static void ExportToCSV(DataSet ds, string tableName, bool containColumName, string fileName)
        {
            string csvStr = ConverDataSet2CSV(ds, tableName, containColumName);
            if (csvStr == "") return;
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            //将string转换成byte[]
            byte[] csvArray = System.Text.Encoding.ASCII.GetBytes(csvStr.ToCharArray(), 0, csvStr.Length - 1);
            fs.Write(csvArray, 0, csvStr.Length - 1);
            fs.Close();
        }


        /// <summary>
        /// 将指定的数据集中指定的表转换成CSV字符串
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static string ConverDataSet2CSV(DataSet ds, string tableName, bool containColumName)
        {
            //首先判断数据集中是否包含指定的表
            if (ds == null || !ds.Tables.Contains(tableName))
            {
                return "";
            }
            string csvStr = "";
            //下面写出数据
            DataTable tb = ds.Tables[tableName];
            //写表名
            //csvStr += tb.TableName + "\n";
            //第一步：写出列名
            if (containColumName)
            {
                foreach (DataColumn column in tb.Columns)
                {
                    csvStr += "\"" + column.ColumnName + "\"" + ",";
                }
                //去掉最后一个","
                csvStr = csvStr.Remove(csvStr.LastIndexOf(","), 1);
                csvStr += "\n";
            }
            //第二步：写出数据
            foreach (DataRow row in tb.Rows)
            {
                foreach (DataColumn column in tb.Columns)
                {
                    csvStr += "\"" + row[column].ToString() + "\"" + ",";
                }
                csvStr = csvStr.Remove(csvStr.LastIndexOf(","), 1);
                csvStr += "\n";
            }
            return csvStr;
        }
    }
}
