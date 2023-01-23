using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lib.Common.Database;

namespace RetirementApp
{
    public partial class Form1 : Form
    {
        Database m_objRxVectorDatabase;
        Dictionary<int, double> m_dictWinningNumbers;  //dictionary for each number and that number's "score"
        Dictionary<int, double> m_dictPB;  //dictionary specifically for the "power" ball

        DataSet m_dataSetRetireData = new DataSet();
        DataTable m_dtRetireData = new DataTable();

        string m_strPopupMessage;

        public Form1()
        {
            InitializeComponent();
            m_objRxVectorDatabase = null;

            m_dataSetRetireData.DataSetName = "NewDataSet";
            m_dtRetireData.TableName = "Table1";
            m_dtRetireData.Columns.Add("GName", typeof(string));
            m_dtRetireData.Columns.Add("DDate", typeof(DateTime));
            m_dtRetireData.PrimaryKey = new DataColumn[] { m_dtRetireData.Columns["GName"], m_dtRetireData.Columns["DDate"] };
            m_dtRetireData.Columns.Add("N1", typeof(int));
            m_dtRetireData.Columns.Add("N2", typeof(int));
            m_dtRetireData.Columns.Add("N3", typeof(int));
            m_dtRetireData.Columns.Add("N4", typeof(int));
            m_dtRetireData.Columns.Add("N5", typeof(int));
            m_dtRetireData.Columns.Add("N6", typeof(int));
            m_dtRetireData.Columns.Add("PB", typeof(int));
            m_dataSetRetireData.Tables.Add(m_dtRetireData);
            m_dataSetRetireData.ReadXml("F:\\C_Drive\\data\\retire\\RetireData.xml");
            m_strPopupMessage = string.Empty;
        }

        private void btnGenerateBadger5_Click(object sender, EventArgs e)
        {
            //save the current cursor
            Cursor saved_cursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            m_dictWinningNumbers = new Dictionary<int, double>();
            txtWinningNumbers.Text = string.Empty;
            txtB5WinningNumbersBasedOnPastAverageScores.Text = string.Empty;
            txtB5MostOverdueNumbers.Text = string.Empty;
            DateTime dtDrawDate = dtpDrawDate.Value;
            dtDrawDate = new DateTime(dtDrawDate.Year, dtDrawDate.Month, dtDrawDate.Day);
            int iDaysOfHistoryToConsider = 9999;
            if (txtDaysOfHistoryToConsider.Text.Trim() != string.Empty)
            {
                iDaysOfHistoryToConsider = Convert.ToInt32(txtDaysOfHistoryToConsider.Text);
            }
            DateTime dtStartDate = dtpDrawDate.Value.AddDays(-1 * iDaysOfHistoryToConsider);
            dtStartDate = new DateTime(dtStartDate.Year, dtStartDate.Month, dtStartDate.Day);

            try
            {
                if (m_objRxVectorDatabase == null)
                {
                    string strDBEnvironment = "NEWLumicera-DEV";
                    string strDBName = "RxVector";
                    m_objRxVectorDatabase = Database.Create(strDBName, strDBEnvironment);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = saved_cursor;
                MessageBox.Show("The following error occurred while trying to connect to the database:" + Environment.NewLine + "     " + ex.Message);
                return;
            }


            string strSQL = string.Empty;
            string strGName = "MB";
            List<IDbDataParameter> parms = new List<IDbDataParameter>();

            ////TEMP - get all data from database table and write it out to an xml file...
            //strSQL = "SELECT * FROM dbo.Temp_retireData ORDER BY DDate DESC";
            //DataTable dt99 = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
            ////Save data to disk
            //DataSet dataSet = new DataSet();
            //dataSet.Tables.Add(dt99);
            //dataSet.WriteXml("F:\\C_Drive\\data\\retire\\RetireData.xml");

            //TEMP - read in all the data from our XML file
            //DataSet m_dataSetRetireData = new DataSet();
            //DataTable m_dtRetireData = new DataTable();
            //m_dataSetRetireData.DataSetName = "NewDataSet";
            //m_dtRetireData.TableName = "Table1";
            //m_dtRetireData.Columns.Add("GName", typeof(string));
            //m_dtRetireData.Columns.Add("DDate", typeof(DateTime));
            //m_dtRetireData.PrimaryKey = new DataColumn[] { m_dtRetireData.Columns["GName"], m_dtRetireData.Columns["DDate"] };
            //m_dtRetireData.Columns.Add("N1", typeof(int));
            //m_dtRetireData.Columns.Add("N2", typeof(int));
            //m_dtRetireData.Columns.Add("N3", typeof(int));
            //m_dtRetireData.Columns.Add("N4", typeof(int));
            //m_dtRetireData.Columns.Add("N5", typeof(int));
            //m_dtRetireData.Columns.Add("N6", typeof(int));
            //m_dtRetireData.Columns.Add("PB", typeof(int));
            //m_dataSetRetireData.Tables.Add(m_dtRetireData);
            //m_dataSetRetireData.ReadXml("F:\\C_Drive\\data\\retire\\RetireData.xml");


            //first just get the count of occurrences for each number
            if (radioBadger5.Checked == true)
                strGName = "B5";
            strSQL = "SELECT * FROM dbo.Temp_RetireData WITH(NOLOCK) WHERE Gname = '" + strGName + "'";
            strSQL += " AND DDate >= :StartDate AND DDate < :DDate ";
            //strSQL += " AND MONTH(DDate) = " + dtDrawDate.Month.ToString();
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":StartDate", DbType.DateTime, dtStartDate));
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
            try
            {
                //DataTable dt = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
                //Use LINQ to get a DataTable of the Badger5 (or Megabucks) data between the requested dates
                var B5Results = from myRow in m_dtRetireData.AsEnumerable()
                                     where myRow.Field<string>("GName") == strGName 
                                     && myRow.Field<DateTime>("DDate") >= dtStartDate 
                                     && myRow.Field<DateTime>("DDate") < dtDrawDate
                                     select myRow;
                DataTable dt = B5Results.Any() ? B5Results.CopyToDataTable() : null;
                if (dt == null || dt.Rows.Count < 1)
                {
                    this.Cursor = saved_cursor;
                    MessageBox.Show("No rows were returned from Temp_RetireData table.");
                    return;
                }
                foreach (DataRow dr in dt.Rows)
                {
                    int iNum1 = Convert.ToInt32(dr["N1"]);
                    int iNum2 = Convert.ToInt32(dr["N2"]);
                    int iNum3 = Convert.ToInt32(dr["N3"]);
                    int iNum4 = Convert.ToInt32(dr["N4"]);
                    int iNum5 = Convert.ToInt32(dr["N5"]);
                    int iNum6 = 0;
                    if (strGName == "MB")
                        iNum6 = Convert.ToInt32(dr["N6"]);

                    if (m_dictWinningNumbers.ContainsKey(iNum1))
                        m_dictWinningNumbers[iNum1]++;
                    else
                        m_dictWinningNumbers.Add(iNum1, 1);
                    if (m_dictWinningNumbers.ContainsKey(iNum2))
                        m_dictWinningNumbers[iNum2]++;
                    else
                        m_dictWinningNumbers.Add(iNum2, 1);
                    if (m_dictWinningNumbers.ContainsKey(iNum3))
                        m_dictWinningNumbers[iNum3]++;
                    else
                        m_dictWinningNumbers.Add(iNum3, 1);
                    if (m_dictWinningNumbers.ContainsKey(iNum4))
                        m_dictWinningNumbers[iNum4]++;
                    else
                        m_dictWinningNumbers.Add(iNum4, 1);
                    if (m_dictWinningNumbers.ContainsKey(iNum5))
                        m_dictWinningNumbers[iNum5]++;
                    else
                        m_dictWinningNumbers.Add(iNum5, 1);
                    if (strGName == "MB")
                    {
                        if (m_dictWinningNumbers.ContainsKey(iNum6))
                            m_dictWinningNumbers[iNum6]++;
                        else
                            m_dictWinningNumbers.Add(iNum6, 1);
                    }
                }
                //next, for each occurrences of a number in N1, get number of times it was paired with N2, N3, N4, N5
                //then do the same for each number in N2, how many times it was paired with N1, N3, N4, N5
                //then for N3 how many times it was paired with N1, N2, N4, N5, and so on
                CalcNumberPairs(strGName, "N1", dtDrawDate, iDaysOfHistoryToConsider);
                CalcNumberPairs(strGName, "N2", dtDrawDate, iDaysOfHistoryToConsider);
                CalcNumberPairs(strGName, "N3", dtDrawDate, iDaysOfHistoryToConsider);
                CalcNumberPairs(strGName, "N4", dtDrawDate, iDaysOfHistoryToConsider);
                CalcNumberPairs(strGName, "N5", dtDrawDate, iDaysOfHistoryToConsider);
                if (strGName == "MB")
                    CalcNumberPairs(strGName, "N6", dtDrawDate, iDaysOfHistoryToConsider);

                //adjust the score of each number based on average number of days between draws of the number
                AdjustScoresForAverageDaysBetweenDraws(strGName, dtDrawDate, iDaysOfHistoryToConsider);//DateTime.Today);

                //get the 5 winning numbers (with the highest probability score!)
                var myList = m_dictWinningNumbers.ToList();
                //myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value)); //sort ascending
                myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); //sort descending (highest first)
                int iCounter = 0;
                string strWinningNumbers = string.Empty;
                foreach (KeyValuePair<int, double> kp in myList)
                {
                    iCounter++;
                    if (strWinningNumbers != String.Empty)
                        strWinningNumbers += ", " + kp.Key.ToString();
                    else
                        strWinningNumbers = kp.Key.ToString();
                    //if (iCounter >= 12)  //print out the top 12 numbers for now until we hone in on right algorithm
                    //    break;
                    if (strGName == "MB" && iCounter >= 6)
                    {
                        break;
                    }
                    else if (strGName != "MB" && iCounter >= 5)
                        break;
                }

                //calc most overdue numbers (that haven't been picked in a long time)
                List<KeyValuePair<int, int>> listOverdueNums = new List<KeyValuePair<int, int>>();
                CalcNumbersWithGreatestGapSinceLastPicked(myList, listOverdueNums, dtDrawDate, strGName, false);
                iCounter = 0;
                string strOverdueNumbers = string.Empty;
                listOverdueNums.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); //sort descending (highest first)
                for (int ii = 0; ii < listOverdueNums.Count; ii++)
                {
                    iCounter++;
                    KeyValuePair<int, int> kp = listOverdueNums[ii];
                    if (strOverdueNumbers != String.Empty)
                        strOverdueNumbers += ", " + kp.Key.ToString();
                    else
                        strOverdueNumbers = kp.Key.ToString();
                    if (strGName == "MB" && iCounter >= 6)
                    {
                        break;
                    }
                    else if (strGName != "MB" && iCounter >= 5)  //print out the top 5 numbers
                    {
                        break;
                    }
                }
                //this commented out logic just shows the numbers with LOWEST score (not necessarily most overdue numbers)
                //for (int ii = myList.Count-1; ii > 0; ii--)
                //{
                //    iCounter++;
                //    KeyValuePair<int, double> kp = myList[ii];
                //    if (strOverdueNumbers != String.Empty)
                //        strOverdueNumbers += ", " + kp.Key.ToString();
                //    else
                //        strOverdueNumbers = kp.Key.ToString();
                //    if (iCounter >= 5)  //print out the top 5 numbers
                //        break;
                //}
                txtB5MostOverdueNumbers.Text = strOverdueNumbers;
                if (checkBoxB5DontPickAllHighestProbability.Checked == false)
                {
                    txtWinningNumbers.Text = strWinningNumbers;
                }
                else
                {
                    //don't just pick highest probability numbers.  Go back through the history of winning numbers
                    //and pick the winners based on where they typically scored in the past.
                    Dictionary<int, int> dictArraySlotWinCount = new Dictionary<int, int>();  //values are arraySlot, NumberOfTimesAWinner
                    foreach (DataRow dr in dt.Rows)
                    {
                        int iNum1 = Convert.ToInt32(dr["N1"]);
                        int iNum2 = Convert.ToInt32(dr["N2"]);
                        int iNum3 = Convert.ToInt32(dr["N3"]);
                        int iNum4 = Convert.ToInt32(dr["N4"]);
                        int iNum5 = Convert.ToInt32(dr["N5"]);
                        int iNum6 = 0;
                        for (int iArraySlot = 0; iArraySlot < myList.Count; iArraySlot++)
                        {
                            KeyValuePair<int, double> kp = myList[iArraySlot];
                            if (kp.Key == iNum1 || kp.Key == iNum2 || kp.Key == iNum3 ||
                                kp.Key == iNum4 || kp.Key == iNum5)
                            {
                                if (dictArraySlotWinCount.ContainsKey(iArraySlot))
                                    dictArraySlotWinCount[iArraySlot]++;
                                else
                                    dictArraySlotWinCount.Add(iArraySlot, 1);
                            }
                            if (strGName == "MB" && kp.Key == iNum6)
                            {
                                if (dictArraySlotWinCount.ContainsKey(iArraySlot))
                                    dictArraySlotWinCount[iArraySlot]++;
                                else
                                    dictArraySlotWinCount.Add(iArraySlot, 1);
                            }
                        }
                    }
                    //now at this point, dictArraySlotWinCount has the most common array slot winners 
                    //so sort that to get the top 5 array slots.
                    var myArraySlotList = dictArraySlotWinCount.ToList();
                    myArraySlotList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); //sort descending (highest first)
                    iCounter = 0;
                    strWinningNumbers = string.Empty;
                    foreach (KeyValuePair<int, int> slotkp in myArraySlotList)
                    {
                        iCounter++;
                        if (strWinningNumbers != String.Empty)
                            strWinningNumbers += ", " + myList[slotkp.Key].Key.ToString();
                        else
                            strWinningNumbers = myList[slotkp.Key].Key.ToString();
                        //if (iCounter >= 12)  //print out the top 12 numbers for now until we hone in on right algorithm
                        //    break;
                        if (strGName == "MB" && iCounter >= 6)
                        {
                            break;
                        }
                        else if (strGName != "MB" && iCounter >= 5)
                        {
                            break;
                        }
                    }
                    txtWinningNumbers.Text = strWinningNumbers;

                    //m_dictWinningNumbers.OrderByDescending(x => x.Value) results.OrderByDescending(x => x.Value).Skip(1).First().Key;
                }//end of else (to not pick strictly by highest probability)

                //now pick winning numbers based on the average score of past winning numbers
                string strTempNums = PickWinnersBasedOnAverageScoreOfPreviousWinners(strGName, dtHistoricalNumbers: dt, listScoredNumbers: myList);
                txtB5WinningNumbersBasedOnPastAverageScores.Text = strTempNums;

                //last thing to do is pick the numbers based on algorithm analysis
                string[] winningNumbers = this.txtWinningNumbers.Text.Split(',');
                string[] overdueNumbers = this.txtB5MostOverdueNumbers.Text.Split(',');
                string[] scoredNumbers = this.txtB5WinningNumbersBasedOnPastAverageScores.Text.Split(',');
                int[] intsWinningNumbers = winningNumbers.Select(int.Parse).ToArray();  //use LINQ to convert our string array to int array
                int[] intOverdueNumbers = overdueNumbers.Select(int.Parse).ToArray();
                int[] intScoredNumbers = scoredNumbers.Select(int.Parse).ToArray();
                List<int> listAlgorithmicNumbers = new List<int>();
                if (strGName == "MB")
                {
                    listAlgorithmicNumbers.Add(intsWinningNumbers[2]);  //22x
                    if (listAlgorithmicNumbers.Contains(intScoredNumbers[0]) == false)
                        listAlgorithmicNumbers.Add(intScoredNumbers[0]);  //21x
                    if (listAlgorithmicNumbers.Contains(intsWinningNumbers[5]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[5]);  //18x
                    if (listAlgorithmicNumbers.Contains(intOverdueNumbers[1]) == false)
                        listAlgorithmicNumbers.Add(intOverdueNumbers[1]);  //19x
                    if (listAlgorithmicNumbers.Contains(intsWinningNumbers[0]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[0]);
                    if (listAlgorithmicNumbers.Contains(intOverdueNumbers[0]) == false)
                        listAlgorithmicNumbers.Add(intOverdueNumbers[0]);
                    if (listAlgorithmicNumbers.Count < 6 && listAlgorithmicNumbers.Contains(intsWinningNumbers[1]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[1]);
                    if (listAlgorithmicNumbers.Count < 6 && listAlgorithmicNumbers.Contains(intScoredNumbers[1]) == false)
                        listAlgorithmicNumbers.Add(intScoredNumbers[1]);
                    if (listAlgorithmicNumbers.Count < 6 && listAlgorithmicNumbers.Contains(intScoredNumbers[5]) == false)
                        listAlgorithmicNumbers.Add(intScoredNumbers[5]);
                    if (listAlgorithmicNumbers.Count < 6 && listAlgorithmicNumbers.Contains(intsWinningNumbers[4]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[4]);
                    if (listAlgorithmicNumbers.Count < 6 && listAlgorithmicNumbers.Contains(intsWinningNumbers[3]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[3]);
                }
                else
                {
                    listAlgorithmicNumbers.Add(intsWinningNumbers[0]);
                    if (listAlgorithmicNumbers.Contains(intScoredNumbers[3]) == false)
                        listAlgorithmicNumbers.Add(intScoredNumbers[3]);
                    if (listAlgorithmicNumbers.Contains(intsWinningNumbers[2]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[2]);
                    if (listAlgorithmicNumbers.Contains(intOverdueNumbers[2]) == false)
                        listAlgorithmicNumbers.Add(intOverdueNumbers[2]);
                    if (listAlgorithmicNumbers.Contains(intScoredNumbers[1]) == false)
                        listAlgorithmicNumbers.Add(intScoredNumbers[1]);
                    if ((listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[1]) == false) ||
                        (strGName == "MB" && listAlgorithmicNumbers.Count < 6 && listAlgorithmicNumbers.Contains(intOverdueNumbers[1]) == false))
                        listAlgorithmicNumbers.Add(intOverdueNumbers[1]);
                    if ((listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[4]) == false) ||
                        (strGName == "MB" && listAlgorithmicNumbers.Count < 6 && listAlgorithmicNumbers.Contains(intOverdueNumbers[4]) == false))
                        listAlgorithmicNumbers.Add(intOverdueNumbers[4]);
                    if ((listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[3]) == false) ||
                        (strGName == "MB" && listAlgorithmicNumbers.Count < 6 && listAlgorithmicNumbers.Contains(intsWinningNumbers[3]) == false))
                        listAlgorithmicNumbers.Add(intsWinningNumbers[3]);
                    if ((listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[1]) == false) ||
                        (strGName == "MB" && listAlgorithmicNumbers.Count < 6 && listAlgorithmicNumbers.Contains(intsWinningNumbers[1]) == false))
                        listAlgorithmicNumbers.Add(intsWinningNumbers[1]);
                    if ((listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[2]) == false) ||
                        (strGName == "MB" && listAlgorithmicNumbers.Count < 6 && listAlgorithmicNumbers.Contains(intScoredNumbers[2]) == false))
                        listAlgorithmicNumbers.Add(intScoredNumbers[2]);
                    if ((listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[0]) == false) ||
                        (strGName == "MB" && listAlgorithmicNumbers.Count < 6 && listAlgorithmicNumbers.Contains(intScoredNumbers[0]) == false))
                        listAlgorithmicNumbers.Add(intOverdueNumbers[0]);
                }


                listAlgorithmicNumbers.Sort(); //sort the list of algorithmic numbers just to make form easier to fill out
                txtB5AlgorithmicNumbers.Text = string.Empty;
                foreach (int i in listAlgorithmicNumbers)
                {
                    if (txtB5AlgorithmicNumbers.Text == string.Empty)
                        txtB5AlgorithmicNumbers.Text = i.ToString();
                    else
                        txtB5AlgorithmicNumbers.Text += ", " + i.ToString();
                }
            }
            catch (Exception ex)
            {
                this.Cursor = saved_cursor;
                MessageBox.Show("Error while querying database - the message from the system was: " + Environment.NewLine + "     " + ex.Message);
            }

            this.Cursor = saved_cursor;

        }

        void CalcNumbersWithGreatestGapSinceLastPicked(List<KeyValuePair<int, double>> listPickedNumbers, List<KeyValuePair<int, int>> listOverdueNums, DateTime dtDrawDate, string strGName, bool bCalcPBOnly)
        {
            List<IDbDataParameter> parms = new List<IDbDataParameter>();
            
            foreach (KeyValuePair<int, double> kvp in listPickedNumbers)
            {
                EnumerableRowCollection<DataRow> rcResults;
                parms.Clear();
                string strSQL = "SELECT * FROM dbo.Temp_RetireData ";
                strSQL += " WHERE GName = :GName ";
                strSQL += " AND DDate < :DDate ";
                parms.Add(m_objRxVectorDatabase.CreateInParameter(":GName", DbType.String, strGName));
                parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
                if (bCalcPBOnly == true)
                {
                    strSQL += " AND PB = :PB ";
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":PB", DbType.Int32, kvp.Key));
                    rcResults = from myRow in m_dtRetireData.AsEnumerable()
                                .OrderByDescending(r => r.Field<DateTime>("DDate")) 
                                where myRow.Field<string>("GName") == strGName
                                && myRow.Field<DateTime>("DDate") < dtDrawDate
                                && myRow.Field<int>("PB") == kvp.Key
                                select myRow;
                }
                else
                {
                    if (strGName == "MB")
                    {
                        strSQL += " AND (N1 = :N1 OR N2 = :N2 OR N3 = :N3 OR N4 = :N4 OR N5 = :N5 OR N6 = :N6) ";
                    }
                    else
                    {
                        strSQL += " AND (N1 = :N1 OR N2 = :N2 OR N3 = :N3 OR N4 = :N4 OR N5 = :N5) ";
                    }
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":N1", DbType.Int32, kvp.Key));
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":N2", DbType.Int32, kvp.Key));
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":N3", DbType.Int32, kvp.Key));
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":N4", DbType.Int32, kvp.Key));
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":N5", DbType.Int32, kvp.Key));
                    if (strGName == "MB")
                    {
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":N6", DbType.Int32, kvp.Key));
                        rcResults = from myRow in m_dtRetireData.AsEnumerable()
                                    .OrderByDescending(r => r.Field<DateTime>("DDate"))
                                    where myRow.Field<string>("GName") == strGName
                                    && myRow.Field<DateTime>("DDate") < dtDrawDate
                                    && (myRow.Field<int>("N1") == kvp.Key || myRow.Field<int>("N2") == kvp.Key || myRow.Field<int>("N3") == kvp.Key || myRow.Field<int>("N4") == kvp.Key || myRow.Field<int>("N5") == kvp.Key || myRow.Field<int>("N6") == kvp.Key)
                                    select myRow;
                    }
                    else
                    {
                        rcResults = from myRow in m_dtRetireData.AsEnumerable()
                                    .OrderByDescending(r => r.Field<DateTime>("DDate"))
                                    where myRow.Field<string>("GName") == strGName
                                    && myRow.Field<DateTime>("DDate") < dtDrawDate
                                    && (myRow.Field<int>("N1") == kvp.Key || myRow.Field<int>("N2") == kvp.Key || myRow.Field<int>("N3") == kvp.Key || myRow.Field<int>("N4") == kvp.Key || myRow.Field<int>("N5") == kvp.Key)
                                    select myRow;
                    }
                }
                strSQL += " ORDER BY DDate DESC";
                //DataTable dt = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
                DataTable dt = rcResults.Any() ? rcResults.CopyToDataTable() : null;


                int iNumDaysSinceLastPicked = 1000;
                if (dt != null && dt.Rows.Count > 0)
                {
                    DateTime dtPriorPickDate = Convert.ToDateTime(dt.Rows[0]["DDate"]);
                    iNumDaysSinceLastPicked = (dtDrawDate - dtPriorPickDate).Days;
                }
                listOverdueNums.Add(new KeyValuePair<int, int>(kvp.Key, iNumDaysSinceLastPicked));
            }
        }

        string PickWinnersBasedOnAverageScoreOfPreviousWinners(string strGName, DataTable dtHistoricalNumbers, List<KeyValuePair<int, double>> listScoredNumbers, bool bPowerballOnly = false)
        {
            //make a copy of the scored list since we're going to manipulate it
            List<KeyValuePair<int, double>> listCopyScoredList = new List<KeyValuePair<int, double>>(listScoredNumbers);

            string strRet = string.Empty;
            double dAvgN1Score = 0;
            double dAvgN2Score = 0;
            double dAvgN3Score = 0;
            double dAvgN4Score = 0;
            double dAvgN5Score = 0;
            double dAvgN6Score = 0;

            if (bPowerballOnly == false)
            {
                foreach (DataRow dr in dtHistoricalNumbers.Rows)
                {
                    int iNum1 = Convert.ToInt32(dr["N1"]);
                    int iNum2 = Convert.ToInt32(dr["N2"]);
                    int iNum3 = Convert.ToInt32(dr["N3"]);
                    int iNum4 = Convert.ToInt32(dr["N4"]);
                    int iNum5 = Convert.ToInt32(dr["N5"]);
                    int iNum6 = strGName == "MB" ? Convert.ToInt32(dr["N6"]) : 0;
                    foreach (KeyValuePair<int, double> kp in listScoredNumbers)
                    {
                        if (kp.Key == iNum1)
                            dAvgN1Score += kp.Value;
                        if (kp.Key == iNum2)
                            dAvgN2Score += kp.Value;
                        if (kp.Key == iNum3)
                            dAvgN3Score += kp.Value;
                        if (kp.Key == iNum4)
                            dAvgN4Score += kp.Value;
                        if (kp.Key == iNum5)
                            dAvgN5Score += kp.Value;
                        if (strGName == "MB" && kp.Key == iNum6)
                            dAvgN6Score += kp.Value;
                    }
                }
                dAvgN1Score = dAvgN1Score / dtHistoricalNumbers.Rows.Count;
                dAvgN2Score = dAvgN2Score / dtHistoricalNumbers.Rows.Count;
                dAvgN3Score = dAvgN3Score / dtHistoricalNumbers.Rows.Count;
                dAvgN4Score = dAvgN4Score / dtHistoricalNumbers.Rows.Count;
                dAvgN5Score = dAvgN5Score / dtHistoricalNumbers.Rows.Count;
                if (strGName == "MB")
                    dAvgN6Score = dAvgN6Score / dtHistoricalNumbers.Rows.Count;

                //now we know the average score of winning numbers, so find the numbers closest to those averages
                int iPreviousArraySlot = 0;
                double dPreviousScoreDifference = 99999999.99;
                for (int iiArraySlot = 0; iiArraySlot < listCopyScoredList.Count; iiArraySlot++)
                {
                    KeyValuePair<int, double> kp = listCopyScoredList[iiArraySlot];
                    double dScore = kp.Value;
                    if (Math.Abs(dAvgN1Score - dScore) < dPreviousScoreDifference)
                    {
                        //this score is closer so keep looking
                        dPreviousScoreDifference = Math.Abs(dAvgN1Score - dScore);
                        iPreviousArraySlot = iiArraySlot;
                    }
                }
                strRet += listCopyScoredList[iPreviousArraySlot].Key.ToString() + ", ";
                listCopyScoredList.RemoveAt(iPreviousArraySlot);

                //N2
                iPreviousArraySlot = 0;
                dPreviousScoreDifference = 99999999.99;
                for (int iiArraySlot = 0; iiArraySlot < listCopyScoredList.Count; iiArraySlot++)
                {
                    KeyValuePair<int, double> kp = listCopyScoredList[iiArraySlot];
                    double dScore = kp.Value;
                    if (Math.Abs(dAvgN2Score - dScore) < dPreviousScoreDifference)
                    {
                        //this score is closer so keep looking
                        dPreviousScoreDifference = Math.Abs(dAvgN2Score - dScore);
                        iPreviousArraySlot = iiArraySlot;
                    }
                }
                strRet += listCopyScoredList[iPreviousArraySlot].Key.ToString() + ", ";
                listCopyScoredList.RemoveAt(iPreviousArraySlot);

                //N3
                iPreviousArraySlot = 0;
                dPreviousScoreDifference = 99999999.99;
                for (int iiArraySlot = 0; iiArraySlot < listCopyScoredList.Count; iiArraySlot++)
                {
                    KeyValuePair<int, double> kp = listCopyScoredList[iiArraySlot];
                    double dScore = kp.Value;
                    if (Math.Abs(dAvgN3Score - dScore) < dPreviousScoreDifference)
                    {
                        //this score is closer so keep looking
                        dPreviousScoreDifference = Math.Abs(dAvgN3Score - dScore);
                        iPreviousArraySlot = iiArraySlot;
                    }
                }
                strRet += listCopyScoredList[iPreviousArraySlot].Key.ToString() + ", ";
                listCopyScoredList.RemoveAt(iPreviousArraySlot);

                //N4
                iPreviousArraySlot = 0;
                dPreviousScoreDifference = 99999999.99;
                for (int iiArraySlot = 0; iiArraySlot < listCopyScoredList.Count; iiArraySlot++)
                {
                    KeyValuePair<int, double> kp = listCopyScoredList[iiArraySlot];
                    double dScore = kp.Value;
                    if (Math.Abs(dAvgN4Score - dScore) < dPreviousScoreDifference)
                    {
                        //this score is closer so keep looking
                        dPreviousScoreDifference = Math.Abs(dAvgN4Score - dScore);
                        iPreviousArraySlot = iiArraySlot;
                    }
                }
                strRet += listCopyScoredList[iPreviousArraySlot].Key.ToString() + ", ";
                listCopyScoredList.RemoveAt(iPreviousArraySlot);

                //N5
                iPreviousArraySlot = 0;
                dPreviousScoreDifference = 99999999.99;
                for (int iiArraySlot = 0; iiArraySlot < listCopyScoredList.Count; iiArraySlot++)
                {
                    KeyValuePair<int, double> kp = listCopyScoredList[iiArraySlot];
                    double dScore = kp.Value;
                    if (Math.Abs(dAvgN5Score - dScore) < dPreviousScoreDifference)
                    {
                        //this score is closer so keep looking
                        dPreviousScoreDifference = Math.Abs(dAvgN5Score - dScore);
                        iPreviousArraySlot = iiArraySlot;
                    }
                }
                strRet += listCopyScoredList[iPreviousArraySlot].Key.ToString();
                listCopyScoredList.RemoveAt(iPreviousArraySlot);

                if (strGName == "MB")
                {
                    strRet += ", ";
                    //N6
                    iPreviousArraySlot = 0;
                    dPreviousScoreDifference = 99999999.99;
                    for (int iiArraySlot = 0; iiArraySlot < listCopyScoredList.Count; iiArraySlot++)
                    {
                        KeyValuePair<int, double> kp = listCopyScoredList[iiArraySlot];
                        double dScore = kp.Value;
                        if (Math.Abs(dAvgN6Score - dScore) < dPreviousScoreDifference)
                        {
                            //this score is closer so keep looking
                            dPreviousScoreDifference = Math.Abs(dAvgN6Score - dScore);
                            iPreviousArraySlot = iiArraySlot;
                        }
                    }
                    strRet += listCopyScoredList[iPreviousArraySlot].Key.ToString();
                    listCopyScoredList.RemoveAt(iPreviousArraySlot);
                }

            }//end of if (this IS NOT a powerball-only run)
            else  //do the calculation only for the powerball
            {
                foreach (DataRow dr in dtHistoricalNumbers.Rows)
                {
                    int iNum1 = Convert.ToInt32(dr["PB"]);
                    foreach (KeyValuePair<int, double> kp in listScoredNumbers)
                    {
                        if (kp.Key == iNum1)
                            dAvgN1Score += kp.Value;
                    }
                }
                dAvgN1Score = dAvgN1Score / dtHistoricalNumbers.Rows.Count;

                //now we know the average score of winning numbers, so find the numbers closest to those averages
                int iPreviousArraySlot = 0;
                double dPreviousScoreDifference = 99999999.99;
                for (int iiArraySlot = 0; iiArraySlot < listCopyScoredList.Count; iiArraySlot++)
                {
                    KeyValuePair<int, double> kp = listCopyScoredList[iiArraySlot];
                    double dScore = kp.Value;
                    if (Math.Abs(dAvgN1Score - dScore) < dPreviousScoreDifference)
                    {
                        //this score is closer so keep looking
                        dPreviousScoreDifference = Math.Abs(dAvgN1Score - dScore);
                        iPreviousArraySlot = iiArraySlot;
                    }
                }
                strRet += listCopyScoredList[iPreviousArraySlot].Key.ToString() + ", ";
                listCopyScoredList.RemoveAt(iPreviousArraySlot);
            }

            return strRet;
        }

        void AdjustScoresForAverageDaysBetweenDraws(string strGName, DateTime dtDrawDate, int iDaysOfHistoryToConsider, bool bAdjustPowerballOnly = false)
        {
            List<IDbDataParameter> parms = new List<IDbDataParameter>();

            int iMinKey = 0;
            int iMaxKey = 0;
            if (bAdjustPowerballOnly == true)
            {
                iMinKey = m_dictPB.Keys.Min();
                iMaxKey = m_dictPB.Keys.Max();
            }
            else
            {
                iMinKey = m_dictWinningNumbers.Keys.Min();
                iMaxKey = m_dictWinningNumbers.Keys.Max();
            }
            DateTime dtStartDate = dtDrawDate.AddDays(-1 * iDaysOfHistoryToConsider);

            for (int iKeyNum = iMinKey; iKeyNum <= iMaxKey;  iKeyNum++)// in m_dictWinningNumbers.Keys)
            {
                DataTable dt;
                EnumerableRowCollection<DataRow> rcResults;
                int iNumRows = 0;
                parms.Clear();
                string strSQL = "SELECT COUNT(*) AS num_rows FROM dbo.Temp_RetireData ";
                strSQL += " WHERE GName = :GName ";
                //strSQL += " AND MONTH(DDate) = " + dtDrawDate.Month.ToString();
                //strSQL += " AND DDate BETWEEN :StartDate AND :DDate ";
                strSQL += " AND DDate >= :StartDate AND DDate < :DDate ";
                parms.Add(m_objRxVectorDatabase.CreateInParameter(":GName", DbType.String, strGName));
                parms.Add(m_objRxVectorDatabase.CreateInParameter(":StartDate", DbType.DateTime, dtStartDate));
                parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
                if (bAdjustPowerballOnly == true)
                {
                    strSQL += " AND PB = :PB ";
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":PB", DbType.Int32, iKeyNum));
                    iNumRows = m_dtRetireData.AsEnumerable()
                               .Count(myRow => myRow.Field<string>("GName") == strGName &&
                                               myRow.Field<DateTime>("DDate") >= dtStartDate &&
                                               myRow.Field<DateTime>("DDate") < dtDrawDate &&
                                               myRow.Field<int>("PB") == iKeyNum);
                }
                else
                {
                    if (strGName == "MB")
                        strSQL += " AND (N1 = :N1 OR N2 = :N2 OR N3 = :N3 OR N4 = :N4 OR N5 = :N5 OR N6 = :N6) ";
                    else
                        strSQL += " AND (N1 = :N1 OR N2 = :N2 OR N3 = :N3 OR N4 = :N4 OR N5 = :N5) ";
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":N1", DbType.Int32, iKeyNum));
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":N2", DbType.Int32, iKeyNum));
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":N3", DbType.Int32, iKeyNum));
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":N4", DbType.Int32, iKeyNum));
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":N5", DbType.Int32, iKeyNum));
                    if (strGName == "MB")
                    {
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":N6", DbType.Int32, iKeyNum));
                    }

                    if (strGName == "MB")
                    {
                        iNumRows = m_dtRetireData.AsEnumerable()
                                   .Count(myRow => myRow.Field<string>("GName") == strGName &&
                                          myRow.Field<DateTime>("DDate") >= dtStartDate &&
                                          myRow.Field<DateTime>("DDate") < dtDrawDate &&
                                          (myRow.Field<int>("N1") == iKeyNum || myRow.Field<int>("N2") == iKeyNum || myRow.Field<int>("N3") == iKeyNum || myRow.Field<int>("N4") == iKeyNum || myRow.Field<int>("N5") == iKeyNum || myRow.Field<int>("N6") == iKeyNum));
                    }
                    else
                    {
                        iNumRows = m_dtRetireData.AsEnumerable()
                                   .Count(myRow => myRow.Field<string>("GName") == strGName &&
                                          myRow.Field<DateTime>("DDate") >= dtStartDate &&
                                          myRow.Field<DateTime>("DDate") < dtDrawDate &&
                                          (myRow.Field<int>("N1") == iKeyNum || myRow.Field<int>("N2") == iKeyNum || myRow.Field<int>("N3") == iKeyNum || myRow.Field<int>("N4") == iKeyNum || myRow.Field<int>("N5") == iKeyNum));
                    }
                }
                //dt = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
                //int iNumRows = Convert.ToInt32(dt.Rows[0]["num_rows"]);
                if (iNumRows > 1)
                {
                    DateTime? dtTempMinDate = null;
                    DateTime? dtTempMaxDate = null;
                    int iDaysDiff = 0;

                    parms.Clear();
                    strSQL = "SELECT MaxDate, MinDate, DATEDIFF(day, MinDate, MaxDate) AS DaysDiff ";
                    strSQL += " FROM (SELECT MAX(DDate) AS MaxDate, MIN(DDate) AS MinDate ";
                    strSQL += "       FROM dbo.Temp_RetireData ";
                    strSQL += "       WHERE GName = :GName ";
                    //strSQL += "       AND DDate BETWEEN :StartDate AND :DDate ";
                    strSQL += "       AND DDate >= :StartDate AND DDate < :DDate ";
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":GName", DbType.String, strGName));
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":StartDate", DbType.DateTime, dtStartDate));
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
                    if (bAdjustPowerballOnly == true)
                    {
                        strSQL += "       AND PB = :PB) b";
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":PB", DbType.Int32, iKeyNum));
                        var innerquery = from myRow in m_dtRetireData.AsEnumerable()
                                         where myRow.Field<string>("GName") == strGName &&
                                               myRow.Field<DateTime>("DDate") >= dtStartDate &&
                                               myRow.Field<DateTime>("DDate") < dtDrawDate && 
                                               myRow.Field<int>("PB") == iKeyNum
                                         group myRow by true into r
                                         select new
                                         {
                                             MinDate = r.Min(z => z.Field<DateTime>("DDate")),
                                             MaxDate = r.Max(z => z.Field<DateTime>("DDate"))
                                         };
                        if (innerquery.Any())
                        {
                            dtTempMinDate = innerquery.ElementAt(0).MinDate;
                            dtTempMaxDate = innerquery.ElementAt(0).MaxDate;
                            iDaysDiff = (innerquery.ElementAt(0).MinDate - innerquery.ElementAt(0).MaxDate).Days;
                        }
                    }
                    else
                    {
                        if (strGName == "MB")
                        {
                            strSQL += "       AND (N1 = :N1 OR N2 = :N2 OR N3 = :N3 OR N4 = :N4 OR N5 = :N5 OR N6 = :N6))) b";
                        }
                        else
                        {
                            strSQL += "       AND (N1 = :N1 OR N2 = :N2 OR N3 = :N3 OR N4 = :N4 OR N5 = :N5)) b";
                        }
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":N1", DbType.Int32, iKeyNum));
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":N2", DbType.Int32, iKeyNum));
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":N3", DbType.Int32, iKeyNum));
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":N4", DbType.Int32, iKeyNum));
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":N5", DbType.Int32, iKeyNum));
                        if (strGName == "MB")
                        {
                            parms.Add(m_objRxVectorDatabase.CreateInParameter(":N6", DbType.Int32, iKeyNum));
                        }

                        if (strGName == "MB")
                        {
                            var innerquery = from myRow in m_dtRetireData.AsEnumerable()
                                             where myRow.Field<string>("GName") == strGName &&
                                                   myRow.Field<DateTime>("DDate") >= dtStartDate &&
                                                   myRow.Field<DateTime>("DDate") < dtDrawDate &&
                                                  (myRow.Field<int>("N1") == iKeyNum || myRow.Field<int>("N2") == iKeyNum || myRow.Field<int>("N3") == iKeyNum || myRow.Field<int>("N4") == iKeyNum || myRow.Field<int>("N5") == iKeyNum || myRow.Field<int>("N6") == iKeyNum)
                                             group myRow by true into r
                                             select new
                                             {
                                                 MinDate = r.Min(z => z.Field<DateTime>("DDate")),
                                                 MaxDate = r.Max(z => z.Field<DateTime>("DDate"))
                                             };
                            if (innerquery.Any())
                            {
                                dtTempMinDate = innerquery.ElementAt(0).MinDate;
                                dtTempMaxDate = innerquery.ElementAt(0).MaxDate;
                                iDaysDiff = (innerquery.ElementAt(0).MaxDate - innerquery.ElementAt(0).MinDate).Days;
                                if (iDaysDiff < 0)
                                    iDaysDiff *= -1;
                            }
                        }
                        else
                        {
                            var innerquery = from myRow in m_dtRetireData.AsEnumerable()
                                             where myRow.Field<string>("GName") == strGName &&
                                                   myRow.Field<DateTime>("DDate") >= dtStartDate &&
                                                   myRow.Field<DateTime>("DDate") < dtDrawDate &&
                                                  (myRow.Field<int>("N1") == iKeyNum || myRow.Field<int>("N2") == iKeyNum || myRow.Field<int>("N3") == iKeyNum || myRow.Field<int>("N4") == iKeyNum || myRow.Field<int>("N5") == iKeyNum)
                                             group myRow by true into r
                                             select new
                                             {
                                                 MinDate = r.Min(z => z.Field<DateTime>("DDate")),
                                                 MaxDate = r.Max(z => z.Field<DateTime>("DDate"))
                                             };
                            if (innerquery.Any())
                            {
                                dtTempMinDate = innerquery.ElementAt(0).MinDate;
                                dtTempMaxDate = innerquery.ElementAt(0).MaxDate;
                                iDaysDiff = (innerquery.ElementAt(0).MaxDate - innerquery.ElementAt(0).MinDate).Days;
                                if (iDaysDiff < 0)
                                    iDaysDiff *= -1;
                            }
                        }
                    }
                    //dt = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
                    //if (dt.Rows.Count > 0)
                    if (dtTempMinDate != null && dtTempMaxDate != null)
                    {
                        //double dAvgDaysBetweenDraws = Convert.ToDouble(dt.Rows[0]["DaysDiff"]) / iNumRows;
                        double dAvgDaysBetweenDraws = Convert.ToDouble(iDaysDiff) / iNumRows;

                        if (dAvgDaysBetweenDraws > 0)
                        {
                            //get the most recent day this number was drawn
                            strSQL = " SELECT DATEDIFF(day, DDate, '" + dtDrawDate.ToString("yyyy/MM/dd") + "') AS DaysSinceLastDrawn ";
                            strSQL += " FROM (SELECT TOP(1) DDate FROM dbo.Temp_RetireData ";
                            strSQL += "       WHERE GName = :GName ";
                            //strSQL += "       AND DDate BETWEEN :StartDate AND :DDate ";
                            strSQL += "       AND DDate >= :StartDate AND DDate < :DDate ";
                            parms.Clear();
                            parms.Add(m_objRxVectorDatabase.CreateInParameter(":GName", DbType.String, strGName));
                            parms.Add(m_objRxVectorDatabase.CreateInParameter(":StartDate", DbType.DateTime, dtStartDate));
                            parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
                            if (bAdjustPowerballOnly == true)
                            {
                                strSQL += "       AND PB = :PB ";
                                parms.Add(m_objRxVectorDatabase.CreateInParameter(":PB", DbType.Int32, iKeyNum));
                                rcResults = from myRow in m_dtRetireData.AsEnumerable()
                                            .OrderByDescending(r => r.Field<DateTime>("DDate"))
                                            where myRow.Field<string>("GName") == strGName && 
                                                  myRow.Field<DateTime>("DDate") >= dtStartDate && 
                                                  myRow.Field<DateTime>("DDate") < dtDrawDate &&
                                                  myRow.Field<int>("PB") == iKeyNum
                                            select myRow;
                            }
                            else
                            {
                                if (strGName == "MB")
                                {
                                    strSQL += "       AND (N1 = :N1 OR N2 = :N2 OR N3 = :N3 OR N4 = :N4 OR N5 = :N5 OR N6 = :N6) ";
                                }
                                else
                                {
                                    strSQL += "       AND (N1 = :N1 OR N2 = :N2 OR N3 = :N3 OR N4 = :N4 OR N5 = :N5) ";
                                }
                                parms.Add(m_objRxVectorDatabase.CreateInParameter(":N1", DbType.Int32, iKeyNum));
                                parms.Add(m_objRxVectorDatabase.CreateInParameter(":N2", DbType.Int32, iKeyNum));
                                parms.Add(m_objRxVectorDatabase.CreateInParameter(":N3", DbType.Int32, iKeyNum));
                                parms.Add(m_objRxVectorDatabase.CreateInParameter(":N4", DbType.Int32, iKeyNum));
                                parms.Add(m_objRxVectorDatabase.CreateInParameter(":N5", DbType.Int32, iKeyNum));
                                if (strGName == "MB")
                                {
                                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":N6", DbType.Int32, iKeyNum));
                                    rcResults = from myRow in m_dtRetireData.AsEnumerable()
                                                .OrderByDescending(r => r.Field<DateTime>("DDate"))
                                                where myRow.Field<string>("GName") == strGName &&
                                                      myRow.Field<DateTime>("DDate") >= dtStartDate &&
                                                      myRow.Field<DateTime>("DDate") < dtDrawDate &&
                                                      (myRow.Field<int>("N1") == iKeyNum || myRow.Field<int>("N2") == iKeyNum || myRow.Field<int>("N3") == iKeyNum || myRow.Field<int>("N4") == iKeyNum || myRow.Field<int>("N5") == iKeyNum || myRow.Field<int>("N6") == iKeyNum)
                                                select myRow;
                                }
                                else
                                {
                                    rcResults = from myRow in m_dtRetireData.AsEnumerable()
                                                .OrderByDescending(r => r.Field<DateTime>("DDate"))
                                                where myRow.Field<string>("GName") == strGName &&
                                                      myRow.Field<DateTime>("DDate") >= dtStartDate &&
                                                      myRow.Field<DateTime>("DDate") < dtDrawDate &&
                                                      (myRow.Field<int>("N1") == iKeyNum || myRow.Field<int>("N2") == iKeyNum || myRow.Field<int>("N3") == iKeyNum || myRow.Field<int>("N4") == iKeyNum || myRow.Field<int>("N5") == iKeyNum)
                                                select myRow;
                                }
                            }
                            strSQL += "       ORDER BY DDate DESC) b";
                            //dt = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
                            dt = rcResults.Any() ? rcResults.CopyToDataTable() : null;
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                //double dDaysSinceLastDrawn = Convert.ToDouble(dt.Rows[0]["DaysSinceLastDrawn"]);
                                DateTime dtTempDate1 = Convert.ToDateTime(dt.Rows[0]["DDate"]);
                                double dDaysSinceLastDrawn = (dtDrawDate - dtTempDate1).TotalDays;
                                if (dDaysSinceLastDrawn < 0)
                                    dDaysSinceLastDrawn = Convert.ToDouble(dDaysSinceLastDrawn * -1);
                                double dMultiplicationFactorForScore = dDaysSinceLastDrawn / dAvgDaysBetweenDraws;
                                if (dMultiplicationFactorForScore > 1.0)
                                {
                                    //we only use this factor here to increase the chances if the number is in a "dry spell" where 
                                    //it hasn't been picked in more than its average number of days.
                                    if (bAdjustPowerballOnly == true)
                                    {
                                        //m_dictPB[iKeyNum] *= dMultiplicationFactorForScore;
                                        m_dictPB[iKeyNum] += dMultiplicationFactorForScore;
                                    }
                                    else
                                    {
                                        //m_dictWinningNumbers[iKeyNum] *= dMultiplicationFactorForScore;
                                        m_dictWinningNumbers[iKeyNum] += dMultiplicationFactorForScore;
                                    }
                                }
                            }

                        }//end of if (average days between draws in > 0

                    }//end of if (to get the average number of days between drawing this number)

                }//end of if (this number has been picked more than 1 time)
            }//end of for loop to go through our dictionary of numbers)
        }

        void CalcNumberPairs(string strGname, string strPrimaryNumColumnName, DateTime dtDrawDate, int iDaysOfHistoryToConsider)
        {
            DateTime dtStartDate = dtDrawDate.AddDays(-1 * iDaysOfHistoryToConsider);
            List<IDbDataParameter> parms = new List<IDbDataParameter>();
            string strSQL = "SELECT " + strPrimaryNumColumnName + ", COUNT(" + strPrimaryNumColumnName + ") AS Num_occurrences ";
            strSQL += " FROM dbo.Temp_RetireData ";
            strSQL += " WHERE GName = :GName ";
            //strSQL += " AND MONTH(DDate) = " + dtDrawDate.Month.ToString();
            //strSQL += " AND DDate BETWEEN :StartDate AND :DDate ";
            strSQL += " AND DDate >= :StartDate AND DDate < :DDate ";
            strSQL += " GROUP BY " + strPrimaryNumColumnName;
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":GName", DbType.String, strGname));
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":StartDate", DbType.DateTime, dtStartDate));
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
            //var counts = m_dtRetireData.GroupBy(x => x.ColumnId)
            //          .Select(g => new { g.Key, Count = g.Count() });
            var rcResults2 = from myRow in m_dtRetireData.AsEnumerable()
                        //.GroupBy(r => r.Field<int>(strPrimaryNumColumnName))
                        where myRow.Field<string>("GName") == strGname && 
                              myRow.Field<DateTime>("DDate") >= dtStartDate && 
                              myRow.Field<DateTime>("DDate") < dtDrawDate
                        group myRow by myRow.Field<int>(strPrimaryNumColumnName) into g
                        select new { strPrimaryNumColumnName = g.Key, Num_occurrences = g.Count() };
            //DataTable dt = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
            DataTable dt2 = m_objRxVectorDatabase.ToDataTable(rcResults2.ToList());
            dt2.Columns["strPrimaryNumColumnName"].ColumnName = strPrimaryNumColumnName;  //have to upadte the column name because of linq...
            //TODO - debug this to make sure my linq query is really correct.  Debugged it and it seems correct...the ordering is different but that shouldn't matter.
            int iCounter = 0;
            foreach (DataRow dr in dt2.Rows)
            {
                iCounter++;
                if (iCounter >= 18)
                {
                    string strJunk123 = "set breakpoint";
                }
                    
                int iN1 = Convert.ToInt32(dr[strPrimaryNumColumnName]);
                if (strPrimaryNumColumnName.ToUpper() == "N1".ToUpper())
                {
                    CalcNumberCombinations(strGname, iN1, "N1", "N2", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N1", "N3", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N1", "N4", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N1", "N5", dtDrawDate, iDaysOfHistoryToConsider);
                    if (strGname == "MB")
                        CalcNumberCombinations(strGname, iN1, "N1", "N6", dtDrawDate, iDaysOfHistoryToConsider);
                }
                else if (strPrimaryNumColumnName.ToUpper() == "N2".ToUpper())
                {
                    CalcNumberCombinations(strGname, iN1, "N2", "N1", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N2", "N3", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N2", "N4", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N2", "N5", dtDrawDate, iDaysOfHistoryToConsider);
                    if (strGname == "MB")
                        CalcNumberCombinations(strGname, iN1, "N2", "N6", dtDrawDate, iDaysOfHistoryToConsider);
                }
                else if (strPrimaryNumColumnName.ToUpper() == "N3".ToUpper())
                {
                    CalcNumberCombinations(strGname, iN1, "N3", "N1", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N3", "N2", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N3", "N4", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N3", "N5", dtDrawDate, iDaysOfHistoryToConsider);
                    if (strGname == "MB")
                        CalcNumberCombinations(strGname, iN1, "N3", "N6", dtDrawDate, iDaysOfHistoryToConsider);
                }
                else if (strPrimaryNumColumnName.ToUpper() == "N4".ToUpper())
                {
                    CalcNumberCombinations(strGname, iN1, "N4", "N1", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N4", "N2", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N4", "N3", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N4", "N5", dtDrawDate, iDaysOfHistoryToConsider);
                    if (strGname == "MB")
                        CalcNumberCombinations(strGname, iN1, "N4", "N6", dtDrawDate, iDaysOfHistoryToConsider);
                }
                else if (strPrimaryNumColumnName.ToUpper() == "N5".ToUpper())
                {
                    CalcNumberCombinations(strGname, iN1, "N5", "N1", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N5", "N2", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N5", "N3", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N5", "N4", dtDrawDate, iDaysOfHistoryToConsider);
                    if (strGname == "MB")
                        CalcNumberCombinations(strGname, iN1, "N5", "N6", dtDrawDate, iDaysOfHistoryToConsider);
                }
                else if (strPrimaryNumColumnName.ToUpper() == "N6".ToUpper())
                {
                    CalcNumberCombinations(strGname, iN1, "N6", "N1", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N6", "N2", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N6", "N3", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "N6", "N4", dtDrawDate, iDaysOfHistoryToConsider);
                    if (strGname == "MB")
                        CalcNumberCombinations(strGname, iN1, "N6", "N5", dtDrawDate, iDaysOfHistoryToConsider);
                }
                else if (strPrimaryNumColumnName.ToUpper() == "PB".ToUpper())
                {
                    CalcNumberCombinations(strGname, iN1, "PB", "N1", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "PB", "N2", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "PB", "N3", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "PB", "N4", dtDrawDate, iDaysOfHistoryToConsider);
                    CalcNumberCombinations(strGname, iN1, "PB", "N5", dtDrawDate, iDaysOfHistoryToConsider);
                }
            }
        }

        private void CalcNumberCombinations(string strGName, int iNum1, string strNum1ColumnName, string strNum2ColumnName, DateTime dtDrawDate, int iDaysOfHistoryToConsider)
        {
            DateTime dtStartDate = dtDrawDate.AddDays(-1 * iDaysOfHistoryToConsider);
            List<IDbDataParameter> parms = new List<IDbDataParameter>();
            string strSQL = "SELECT " + strNum1ColumnName + ", COUNT(" + strNum1ColumnName + ") AS Num_occurrences, ";
            strSQL += strNum2ColumnName + ", COUNT(" + strNum2ColumnName + ") AS Num_occurrences2 ";
            strSQL += " FROM dbo.Temp_RetireData ";
            strSQL += " WHERE " + strNum1ColumnName + " = :" + strNum1ColumnName;
            //strSQL += " AND MONTH(DDate) = " + dtDrawDate.Month.ToString();
            //strSQL += " AND DDate BETWEEN :StartDate AND :DDate ";
            strSQL += " AND DDate >= :StartDate AND DDate < :DDate ";
            strSQL += " AND GName = :GName ";
            strSQL += " GROUP BY " + strNum1ColumnName + ", " + strNum2ColumnName;
            strSQL += " ORDER BY Num_occurrences DESC ";
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":" + strNum1ColumnName, DbType.Int32, iNum1));
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":StartDate", DbType.DateTime, dtStartDate));
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":GName", DbType.String, strGName));

            var rcResults2 = from myRow in m_dtRetireData.AsEnumerable()
                             where myRow.Field<string>("GName") == strGName &&
                                   myRow.Field<DateTime>("DDate") >= dtStartDate &&
                                   myRow.Field<DateTime>("DDate") < dtDrawDate && 
                                   myRow.Field<int>(strNum1ColumnName) == iNum1
                             group myRow by ( myRow.Field<int>(strNum1ColumnName), myRow.Field<int>(strNum2ColumnName) ) into g
                             select new { strNum1ColumnName = g.Key.Item1, Num_occurrences = g.Count(), strNum2ColumnName = g.Key.Item2, Num_occurrences2 = g.Count() };
            DataTable dt2 = m_objRxVectorDatabase.ToDataTable(rcResults2.ToList());

            //DataTable dt2 = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
            dt2.Columns["strNum1ColumnName"].ColumnName = strNum1ColumnName;  //have to upadte the column name because of linq...
            dt2.Columns["strNum2ColumnName"].ColumnName = strNum2ColumnName;  //have to upadte the column name because of linq...
            //todo - debug this to make sure LINQ works.  Debugged and this too looks good!
            foreach (DataRow dr2 in dt2.Rows)
            {
                int iN2 = Convert.ToInt32(dr2[strNum2ColumnName]);
                if (strNum1ColumnName != "PB")
                {
                    m_dictWinningNumbers[iNum1] += Convert.ToInt32(dr2["Num_occurrences"]);
                    m_dictWinningNumbers[iN2] += Convert.ToInt32(dr2["Num_occurrences2"]);
                }
                else
                {
                    m_dictPB[iNum1] += Convert.ToInt32(dr2["Num_occurrences"]);
                }
            }
        }


        private void btnGeneratePowerball_Click(object sender, EventArgs e)
        {
            //save the current cursor
            Cursor saved_cursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            m_dictWinningNumbers = new Dictionary<int, double>();
            m_dictPB = new Dictionary<int, double>();
            txtPowerballWinningNumbers.Text = string.Empty;
            txtPowerball.Text = string.Empty;
            DateTime dtDrawDate = dtpPowerballDrawDate.Value;
            dtDrawDate = new DateTime(dtDrawDate.Year, dtDrawDate.Month, dtDrawDate.Day);
            int iDaysOfHistoryToConsider = 9999;
            if (txtPowerballDaysOfHistoryToConsider.Text.Trim() != string.Empty)
            {
                iDaysOfHistoryToConsider = Convert.ToInt32(txtPowerballDaysOfHistoryToConsider.Text);
            }
            DateTime dtStartDate = dtpPowerballDrawDate.Value.AddDays(-1 * iDaysOfHistoryToConsider);
            dtStartDate = new DateTime(dtStartDate.Year, dtStartDate.Month, dtStartDate.Day);

            try
            {
                string strDBEnvironment = "NEWLumicera-DEV";
                string strDBName = "RxVector";
                m_objRxVectorDatabase = Database.Create(strDBName, strDBEnvironment);
            }
            catch (Exception ex)
            {
                this.Cursor = saved_cursor;
                MessageBox.Show("The following error occurred while trying to connect to the database:" + Environment.NewLine + "     " + ex.Message);
                return;
            }


            string strGName = "MM";
            string strSQL = string.Empty;
            List<IDbDataParameter> parms = new List<IDbDataParameter>();
            //first just get the count of occurrences for each number
            if (radioPowerball.Checked == true)
                strGName = "PB";
            strSQL = "SELECT * FROM dbo.Temp_RetireData WITH(NOLOCK) WHERE Gname = '" + strGName + "'";
            //strSQL += " AND DDate BETWEEN :StartDate AND :DDate ";
            strSQL += " AND DDate >= :StartDate AND DDate < :DDate ";
            //strSQL += " AND MONTH(DDate) = " + dtDrawDate.Month.ToString();
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":StartDate", DbType.DateTime, dtStartDate));
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
            try
            {
                //Use LINQ to get a DataTable of the Powerball or MegaMillions data between the requested dates
                var PBResults = from myRow in m_dtRetireData.AsEnumerable()
                                where myRow.Field<string>("GName") == strGName
                                && myRow.Field<DateTime>("DDate") >= dtStartDate
                                && myRow.Field<DateTime>("DDate") < dtDrawDate
                                select myRow;
                DataTable dt = PBResults.Any() ? PBResults.CopyToDataTable() : null;
                //DataTable dt = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
                if (dt != null && dt.Rows.Count < 1)
                {
                    this.Cursor = saved_cursor;
                    MessageBox.Show("No rows were returned from Temp_RetireData table.");
                    return;
                }
                foreach (DataRow dr in dt.Rows)
                {
                    int iNum1 = Convert.ToInt32(dr["N1"]);
                    int iNum2 = Convert.ToInt32(dr["N2"]);
                    int iNum3 = Convert.ToInt32(dr["N3"]);
                    int iNum4 = Convert.ToInt32(dr["N4"]);
                    int iNum5 = Convert.ToInt32(dr["N5"]);
                    int iPB = Convert.ToInt32(dr["PB"]);
                    if (m_dictWinningNumbers.ContainsKey(iNum1))
                        m_dictWinningNumbers[iNum1]++;
                    else
                        m_dictWinningNumbers.Add(iNum1, 1);
                    if (m_dictWinningNumbers.ContainsKey(iNum2))
                        m_dictWinningNumbers[iNum2]++;
                    else
                        m_dictWinningNumbers.Add(iNum2, 1);
                    if (m_dictWinningNumbers.ContainsKey(iNum3))
                        m_dictWinningNumbers[iNum3]++;
                    else
                        m_dictWinningNumbers.Add(iNum3, 1);
                    if (m_dictWinningNumbers.ContainsKey(iNum4))
                        m_dictWinningNumbers[iNum4]++;
                    else
                        m_dictWinningNumbers.Add(iNum4, 1);
                    if (m_dictWinningNumbers.ContainsKey(iNum5))
                        m_dictWinningNumbers[iNum5]++;
                    else
                        m_dictWinningNumbers.Add(iNum5, 1);
                    if (m_dictPB.ContainsKey(iPB))
                        m_dictPB[iPB]++;
                    else
                        m_dictPB.Add(iPB, 1);
                }
                //next, for each occurrences of a number in N1, get number of times it was paired with N2, N3, N4, N5
                //then do the same for each number in N2, how many times it was paired with N1, N3, N4, N5
                //then for N3 how many times it was paired with N1, N2, N4, N5, and so on
                CalcNumberPairs(strGName, "N1", dtDrawDate, iDaysOfHistoryToConsider);
                CalcNumberPairs(strGName, "N2", dtDrawDate, iDaysOfHistoryToConsider);
                CalcNumberPairs(strGName, "N3", dtDrawDate, iDaysOfHistoryToConsider);
                CalcNumberPairs(strGName, "N4", dtDrawDate, iDaysOfHistoryToConsider);
                CalcNumberPairs(strGName, "N5", dtDrawDate, iDaysOfHistoryToConsider);

                //adjust the score of each number based on average number of days between draws of the number
                AdjustScoresForAverageDaysBetweenDraws(strGName, dtDrawDate, iDaysOfHistoryToConsider);//DateTime.Today);


                //now do those same calculations only for the powerball number
                CalcNumberPairs(strGName, "PB", dtDrawDate, iDaysOfHistoryToConsider);
                AdjustScoresForAverageDaysBetweenDraws(strGName, dtDrawDate, iDaysOfHistoryToConsider, true);

                //get the 5 winning numbers (with the highest probability score!)
                var myList = m_dictWinningNumbers.ToList();
                //myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value)); //sort ascending
                myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); //sort descending (highest first)

                //sort the powerball dictionary highest to lowest
                var myPBList = m_dictPB.ToList();
                myPBList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

                int iCounter = 0;
                string strWinningNumbers = string.Empty;
                foreach (KeyValuePair<int, double> kp2 in myList)
                {
                    iCounter++;
                    if (strWinningNumbers != String.Empty)
                        strWinningNumbers += ", " + kp2.Key.ToString();
                    else
                        strWinningNumbers = kp2.Key.ToString();
                    //if (iCounter >= 12)  //print out the top 12 numbers for now until we hone in on right algorithm
                    //    break;
                    if (iCounter >= 5)
                        break;
                }
                txtPowerballWinningNumbers.Text = strWinningNumbers;

                //calc most overdue numbers (that haven't been picked in a long time)
                List<KeyValuePair<int, int>> listOverdueNums = new List<KeyValuePair<int, int>>();
                CalcNumbersWithGreatestGapSinceLastPicked(myList, listOverdueNums, dtDrawDate, strGName, false);
                iCounter = 0;
                string strOverdueNumbers = string.Empty;
                listOverdueNums.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); //sort descending (highest first)
                for (int ii = 0; ii < listOverdueNums.Count; ii++)
                {
                    iCounter++;
                    KeyValuePair<int, int> kp = listOverdueNums[ii];
                    if (strOverdueNumbers != String.Empty)
                        strOverdueNumbers += ", " + kp.Key.ToString();
                    else
                        strOverdueNumbers = kp.Key.ToString();
                    if (iCounter >= 5)  //print out the top 5 numbers
                        break;
                }
                txtPBMostOverdueNumbers.Text = strOverdueNumbers;

                //now do most overdue numbers - commented out since it's actually LOWEST score numbers, not most overdue
                //iCounter = 0;
                //string strOverdueNumbers = string.Empty;
                //for (int ii = myList.Count-1; ii > 0; ii--)
                //{
                //    iCounter++;
                //    KeyValuePair<int, double> kp3 = myList[ii];
                //    if (strOverdueNumbers != String.Empty)
                //        strOverdueNumbers += ", " + kp3.Key.ToString();
                //    else
                //        strOverdueNumbers = kp3.Key.ToString();
                //    if (iCounter >= 5)  //print out the top 5 numbers
                //        break;
                //}
                //txtPBMostOverdueNumbers.Text = strOverdueNumbers;


                //now do most likely powerball (or megaball)
                iCounter = 0;
                string strWinningPB = string.Empty;
                foreach (KeyValuePair<int, double> kp2 in myPBList)
                {
                    iCounter++;
                    if (strWinningPB != String.Empty)
                        strWinningPB += ", " + kp2.Key.ToString();
                    else
                        strWinningPB = kp2.Key.ToString();
                    if (iCounter >= 1)
                        break;
                    //if (iCounter >= 3)  //print out the top 3 numbers for now until we hone in on right algorithm
                    //    break;
                }
                txtPowerball.Text = strWinningPB;

                //now do most overdue powerball (or megaball)
                listOverdueNums.Clear();
                CalcNumbersWithGreatestGapSinceLastPicked(myPBList, listOverdueNums, dtDrawDate, strGName, true);
                listOverdueNums.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); //sort descending (highest first)
                txtPBOverduePowerball.Text = listOverdueNums[0].Key.ToString();
                //do most overdue powerball - commented out since that's lowest score powerball, not most overdue
                //KeyValuePair<int, double> kp4 = myPBList[myPBList.Count - 1];
                //txtPBOverduePowerball.Text = kp4.Key.ToString();

                if (checkBoxPBDontPickAllHighestProbability.Checked == true)
                {
                    //don't just pick highest probability numbers.  Go back through the history of winning numbers
                    //and pick the winners based on where they typically scored in the past.
                    Dictionary<int, int> dictArraySlotWinCount = new Dictionary<int, int>();  //values are arraySlot, NumberOfTimesAWinner
                    foreach (DataRow dr in dt.Rows)
                    {
                        int iNum1 = Convert.ToInt32(dr["N1"]);
                        int iNum2 = Convert.ToInt32(dr["N2"]);
                        int iNum3 = Convert.ToInt32(dr["N3"]);
                        int iNum4 = Convert.ToInt32(dr["N4"]);
                        int iNum5 = Convert.ToInt32(dr["N5"]);
                        for (int iArraySlot = 0; iArraySlot < myList.Count; iArraySlot++)
                        {
                            KeyValuePair<int, double> kp = myList[iArraySlot];
                            if (kp.Key == iNum1 || kp.Key == iNum2 || kp.Key == iNum3 ||
                                kp.Key == iNum4 || kp.Key == iNum5)
                            {
                                if (dictArraySlotWinCount.ContainsKey(iArraySlot))
                                    dictArraySlotWinCount[iArraySlot]++;
                                else
                                    dictArraySlotWinCount.Add(iArraySlot, 1);
                            }
                        }
                    }
                    //now at this point, dictArraySlotWinCount has the most common array slot winners 
                    //so sort that to get the top 5 array slots.
                    var myArraySlotList = dictArraySlotWinCount.ToList();
                    myArraySlotList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); //sort descending (highest first)
                    iCounter = 0;
                    strWinningNumbers = string.Empty;
                    foreach (KeyValuePair<int, int> slotkp in myArraySlotList)
                    {
                        iCounter++;
                        if (strWinningNumbers != String.Empty)
                            strWinningNumbers += ", " + myList[slotkp.Key].Key.ToString();
                        else
                            strWinningNumbers = myList[slotkp.Key].Key.ToString();
                        //if (iCounter >= 12)  //print out the top 12 numbers for now until we hone in on right algorithm
                        //    break;
                        if (iCounter >= 5)
                            break;
                    }
                    txtPowerballWinningNumbers.Text = strWinningNumbers;

                    //m_dictWinningNumbers.OrderByDescending(x => x.Value) results.OrderByDescending(x => x.Value).Skip(1).First().Key;
                }//end of else (to not pick strictly by highest probability)

                //now pick winning numbers based on the average score of past winning numbers
                string strTempNums = PickWinnersBasedOnAverageScoreOfPreviousWinners(strGName, dtHistoricalNumbers: dt, listScoredNumbers: myList);
                txtPBWinningNumbersBasedOnPastAverageScores.Text = strTempNums;

                strTempNums = PickWinnersBasedOnAverageScoreOfPreviousWinners(strGName, dtHistoricalNumbers: dt, listScoredNumbers: myPBList, bPowerballOnly: true);
                strTempNums = strTempNums.Replace(",", "");
                txtPowerballWinnerBasedOnPastAverageScore.Text = strTempNums;

                //last thing to do is pick the numbers based on algorithm analysis
                string[] winningNumbers = this.txtPowerballWinningNumbers.Text.Split(',');
                string[] overdueNumbers = this.txtPBMostOverdueNumbers.Text.Split(',');
                string[] scoredNumbers = this.txtPBWinningNumbersBasedOnPastAverageScores.Text.Split(',');
                int[] intsWinningNumbers = winningNumbers.Select(int.Parse).ToArray();  //use LINQ to convert our string array to int array
                int[] intOverdueNumbers = overdueNumbers.Select(int.Parse).ToArray();
                int[] intScoredNumbers = scoredNumbers.Select(int.Parse).ToArray();
                List<int> listAlgorithmicNumbers = new List<int>();
                if (strGName == "MM")
                {
                    listAlgorithmicNumbers.Add(intScoredNumbers[1]);
                    if (listAlgorithmicNumbers.Contains(intOverdueNumbers[0]) == false)
                        listAlgorithmicNumbers.Add(intOverdueNumbers[0]);
                    if (listAlgorithmicNumbers.Contains(intOverdueNumbers[3]) == false)
                        listAlgorithmicNumbers.Add(intOverdueNumbers[3]);
                    if (listAlgorithmicNumbers.Contains(intsWinningNumbers[1]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[1]);
                    if (listAlgorithmicNumbers.Contains(intScoredNumbers[2]) == false)
                        listAlgorithmicNumbers.Add(intScoredNumbers[2]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[0]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[0]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[4]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[4]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[4]) == false)
                        listAlgorithmicNumbers.Add(intScoredNumbers[4]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[2]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[2]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[3]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[3]);
                }
                else //assume powerball
                {
                    //this is the 365 day history algorithm after linq improvements
                    listAlgorithmicNumbers.Add(intsWinningNumbers[1]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[2]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[2]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[0]) == false)
                        listAlgorithmicNumbers.Add(intScoredNumbers[0]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[3]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[3]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[2]) == false)
                        listAlgorithmicNumbers.Add(intOverdueNumbers[2]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[2]) == false)
                        listAlgorithmicNumbers.Add(intOverdueNumbers[2]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[1]) == false)  //added (moved) this one up in algorithm on 6-1-2021
                        listAlgorithmicNumbers.Add(intScoredNumbers[1]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[4]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[4]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[0]) == false)
                        listAlgorithmicNumbers.Add(intsWinningNumbers[0]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[4]) == false)
                        listAlgorithmicNumbers.Add(intOverdueNumbers[4]);
                    if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[2]) == false)
                        listAlgorithmicNumbers.Add(intScoredNumbers[2]);


                    //this is the full history algorithm after linq improvements
                    //listAlgorithmicNumbers.Add(intsWinningNumbers[1]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[2]) == false)
                    //    listAlgorithmicNumbers.Add(intsWinningNumbers[2]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[2]) == false)
                    //    listAlgorithmicNumbers.Add(intOverdueNumbers[2]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[2]) == false)
                    //    listAlgorithmicNumbers.Add(intScoredNumbers[2]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[3]) == false)
                    //    listAlgorithmicNumbers.Add(intScoredNumbers[3]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[2]) == false)
                    //    listAlgorithmicNumbers.Add(intOverdueNumbers[2]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[0]) == false)
                    //    listAlgorithmicNumbers.Add(intsWinningNumbers[0]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[4]) == false)  //added (moved) this one up in algorithm on 6-1-2021
                    //    listAlgorithmicNumbers.Add(intScoredNumbers[4]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[4]) == false)
                    //    listAlgorithmicNumbers.Add(intOverdueNumbers[4]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[2]) == false)
                    //    listAlgorithmicNumbers.Add(intsWinningNumbers[2]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[1]) == false)
                    //    listAlgorithmicNumbers.Add(intScoredNumbers[1]);

                    //listAlgorithmicNumbers.Add(intsWinningNumbers[4]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[0]) == false)
                    //    listAlgorithmicNumbers.Add(intOverdueNumbers[0]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[2]) == false)
                    //    listAlgorithmicNumbers.Add(intOverdueNumbers[2]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[1]) == false)
                    //    listAlgorithmicNumbers.Add(intScoredNumbers[1]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[4]) == false)
                    //    listAlgorithmicNumbers.Add(intScoredNumbers[4]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[4]) == false)
                    //    listAlgorithmicNumbers.Add(intsWinningNumbers[4]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[0]) == false)
                    //    listAlgorithmicNumbers.Add(intScoredNumbers[0]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intScoredNumbers[3]) == false)  //added (moved) this one up in algorithm on 6-1-2021
                    //    listAlgorithmicNumbers.Add(intScoredNumbers[3]);

                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[0]) == false)
                    //    listAlgorithmicNumbers.Add(intsWinningNumbers[0]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[4]) == false)
                    //    listAlgorithmicNumbers.Add(intOverdueNumbers[4]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intOverdueNumbers[2]) == false)
                    //    listAlgorithmicNumbers.Add(intOverdueNumbers[2]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[2]) == false)
                    //    listAlgorithmicNumbers.Add(intsWinningNumbers[2]);
                    //if (listAlgorithmicNumbers.Count < 5 && listAlgorithmicNumbers.Contains(intsWinningNumbers[1]) == false)
                    //    listAlgorithmicNumbers.Add(intsWinningNumbers[1]);
                }

                listAlgorithmicNumbers.Sort(); //sort the list of algorithmic numbers just to make form easier to fill out
                txtPBAlgorithmicNumbers.Text = string.Empty;
                iCounter = 0;
                foreach (int i in listAlgorithmicNumbers)
                {
                    iCounter++;
                    if (txtPBAlgorithmicNumbers.Text == string.Empty)
                        txtPBAlgorithmicNumbers.Text = i.ToString();
                    else
                        txtPBAlgorithmicNumbers.Text += ", " + i.ToString();
                    if (iCounter >= 5)
                        break;
                }
                if (strGName == "MM")
                    txtPBAlgorithmicPowerball.Text = txtPowerballWinnerBasedOnPastAverageScore.Text;//txtPBOverduePowerball.Text;
                else
                    txtPBAlgorithmicPowerball.Text = txtPowerball.Text;
                if (strGName == "MM")
                {
                    txtPBAlgorithmRules.Text = "MB=scored (8x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N1 = 2nd scored(16x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N2 = 1st overdue(15x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N3 = 4th overdue(15x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N4 = 2nd winning(13x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N5 = 3rd scored(11x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N6 = 1st winning(11x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "rest from winning if needed";
                    //txtPBAlgorithmRules.Text = "MB=most overdue is MB (11x)" + Environment.NewLine;
                    //txtPBAlgorithmRules.Text += "N1 = 5th winning(17x)" + Environment.NewLine;
                    //txtPBAlgorithmRules.Text += "N2 = 5th scored(15x)" + Environment.NewLine;
                    //txtPBAlgorithmRules.Text += "N3 = 1st overdue(15x)" + Environment.NewLine;
                    //txtPBAlgorithmRules.Text += "N4 = 1st scored(14x)" + Environment.NewLine;
                    //txtPBAlgorithmRules.Text += "N5 = 2nd scored(14x)" + Environment.NewLine;
                    //txtPBAlgorithmRules.Text += "N6 = 5th overdue(12x)" + Environment.NewLine;
                    //txtPBAlgorithmRules.Text += "rest from winning if needed";
                }
                else //assume powerball
                {
                    txtPBAlgorithmRules.Text = "PB=winning PB (16x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N1 = 5th winning(22x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N2 = 1st overdue(17x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N3 = 3rd overdue(17x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N4 = 2nd scored(17x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N5 = 5th scored(16x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N6 = 4th scored(19x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N7 = 5th overdue(13x)" + Environment.NewLine;
                    txtPBAlgorithmRules.Text += "N8 = 3rdover,winning";
                }
            }
            catch (Exception ex)
            {
                this.Cursor = saved_cursor;
                MessageBox.Show("Error while querying database - the message from the system was: " + Environment.NewLine + "     " + ex.Message);
            }

            this.Cursor = saved_cursor;

        }

        private void radioPowerball_CheckedChanged(object sender, EventArgs e)
        {
            if (radioPowerball.Checked == true)
            {
                this.lblPBOverduePowerball.Text = "Powerball";
                this.lblPowerball.Text = "Powerball";
                this.lblPowerballWinnerBasedOnPastAverageScore.Text = "Powerball";
                this.lblPBAlgorithmicPowerball.Text = "Powerball";
                groupBoxPowerball.Text = "Powerball";
                btnGeneratePowerball.Text = "Gen Powerball";
            }
            else
            {
                this.lblPBOverduePowerball.Text = "Megaball";
                this.lblPowerball.Text = "Megaball";
                this.lblPowerballWinnerBasedOnPastAverageScore.Text = "Megaball";
                this.lblPBAlgorithmicPowerball.Text = "Megaball";
                groupBoxPowerball.Text = "Mega Millons";
                btnGeneratePowerball.Text = "Gen MegaMillions";
            }
        }

        private void AnalyzeAlgorithms(string strGName, object sender, EventArgs e)
        {
            //save the current cursor
            Cursor saved_cursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            m_strPopupMessage = string.Empty;

            try
            {
                if (m_objRxVectorDatabase == null)
                {
                    string strDBEnvironment = "NEWLumicera-DEV";
                    string strDBName = "RxVector";
                    m_objRxVectorDatabase = Database.Create(strDBName, strDBEnvironment);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = saved_cursor;
                MessageBox.Show("The following error occurred while trying to connect to the database:" + Environment.NewLine + "     " + ex.Message);
                return;
            }

            if (strGName == "B5")
            {
                txtB5TotalPlays.Text = string.Empty;
                lblB5WinningNumberAlgorithmAnalysisResults.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x";
                lblB5OverdueNumbersAlgorithmAnalysis.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x";
                lblB5PastScoreNumbersAlgorithmAnalysis.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x";
                lblB5WinningNumberPositionAlgorithmAnalysisResults.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x";
                lblB5OverdueNumbersPositionAlgorithmAnalysis.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x";
                lblB5PastScoreNumbersPositionAlgorithmAnalysis.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x";
            }
            else if (strGName == "MB")
            {
                txtB5TotalPlays.Text = string.Empty;
                lblB5WinningNumberAlgorithmAnalysisResults.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x, 6 0x";
                lblB5OverdueNumbersAlgorithmAnalysis.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x, 6 0x";
                lblB5PastScoreNumbersAlgorithmAnalysis.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x, 6 0x";
                lblB5WinningNumberPositionAlgorithmAnalysisResults.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x, 6th 0x";
                lblB5OverdueNumbersPositionAlgorithmAnalysis.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x, 6th 0x";
                lblB5PastScoreNumbersPositionAlgorithmAnalysis.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x, 6th 0x";
            }
            else if (strGName == "PB" || strGName == "MM")
            {
                txtPBTotalPlays.Text = string.Empty;
                lblPBWinningNumberAlgorithmAnalysisResults.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x";
                lblPBOverdueNumbersAlgorithmAnalysis.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x";
                lblPBPastScoreNumbersAlgorithmAnalysis.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x";
                lblPBWeeklyWinningNumberAlgorithmAnalysisResults.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x";
                lblPBWeeklyOverdueNumbersAlgorithmAnalysis.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x";
                lblPBWeeklyPastScoreNumbersAlgorithmAnalysis.Text = "0 6x, 1 5x, 2 4x, 3 3x, 4 2x, 5 1x";
                if (strGName == "PB")
                {
                    lblPBWinningNumberPositionAlgorithmAnalysisResults.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x,  PB 0x";
                    lblPBOverdueNumbersPositionAlgorithmAnalysis.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x,  PB 0x";
                    lblPBPastScoreNumbersPositionAlgorithmAnalysis.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x,  PB 0x";
                }
                else
                {
                    lblPBWinningNumberPositionAlgorithmAnalysisResults.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x,  MB 0x";
                    lblPBOverdueNumbersPositionAlgorithmAnalysis.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x,  MB 0x";
                    lblPBPastScoreNumbersPositionAlgorithmAnalysis.Text = "1st 0x, 2nd 0x, 3rd 0x, 4th 0x, 5th 0x,  MB 0x";
                }
            }

            int iWinningNumbersPos1Right = 0;
            int iWinningNumbersPos2Right = 0;
            int iWinningNumbersPos3Right = 0;
            int iWinningNumbersPos4Right = 0;
            int iWinningNumbersPos5Right = 0;
            int iWinningNumbersPos6Right = 0;
            int iOverdueNumbersPos1Right = 0;
            int iOverdueNumbersPos2Right = 0;
            int iOverdueNumbersPos3Right = 0;
            int iOverdueNumbersPos4Right = 0;
            int iOverdueNumbersPos5Right = 0;
            int iOverdueNumbersPos6Right = 0;
            int iScoredNumbersPos1Right = 0;
            int iScoredNumbersPos2Right = 0;
            int iScoredNumbersPos3Right = 0;
            int iScoredNumbersPos4Right = 0;
            int iScoredNumbersPos5Right = 0;
            int iScoredNumbersPos6Right = 0;
            int iAlgorithmNumbersPos1Right = 0;
            int iAlgorithmNumbersPos2Right = 0;
            int iAlgorithmNumbersPos3Right = 0;
            int iAlgorithmNumbersPos4Right = 0;
            int iAlgorithmNumbersPos5Right = 0;
            int iAlgorithmNumbersPos6Right = 0;
            int iWinningNumbersPBRight = 0;
            int iOverdueNumbersPBRight = 0;
            int iScoredNumbersPBRight = 0;
            int iAlgorithmNumbersPBRight = 0;
            int iTotalTimesWinningNums0Right = 0;
            int iTotalTimesOverdueNums0Right = 0;
            int iTotalTimesScoredNums0Right = 0;
            int iTotalTimesAlgorithmNums0Right = 0;
            int iTotalTimesWinningNumsAtLeast1Right = 0;
            int iTotalTimesWinningNumsAtLeast2Right = 0;
            int iTotalTimesWinningNumsAtLeast3Right = 0;
            int iTotalTimesWinningNumsAtLeast4Right = 0;
            int iTotalTimesWinningNumsAtLeast5Right = 0;
            int iTotalTimesWinningNumsAtLeast6Right = 0;
            int iTotalTimesOverdueNumsAtLeast1Right = 0;
            int iTotalTimesOverdueNumsAtLeast2Right = 0;
            int iTotalTimesOverdueNumsAtLeast3Right = 0;
            int iTotalTimesOverdueNumsAtLeast4Right = 0;
            int iTotalTimesOverdueNumsAtLeast5Right = 0;
            int iTotalTimesOverdueNumsAtLeast6Right = 0;
            int iTotalTimesScoredNumsAtLeast1Right = 0;
            int iTotalTimesScoredNumsAtLeast2Right = 0;
            int iTotalTimesScoredNumsAtLeast3Right = 0;
            int iTotalTimesScoredNumsAtLeast4Right = 0;
            int iTotalTimesScoredNumsAtLeast5Right = 0;
            int iTotalTimesScoredNumsAtLeast6Right = 0;
            int iTotalTimesAlgorithmNumsAtLeast1Right = 0;
            int iTotalTimesAlgorithmNumsAtLeast2Right = 0;
            int iTotalTimesAlgorithmNumsAtLeast3Right = 0;
            int iTotalTimesAlgorithmNumsAtLeast4Right = 0;
            int iTotalTimesAlgorithmNumsAtLeast5Right = 0;
            int iTotalTimesAlgorithmNumsAtLeast6Right = 0;
            int iTotalDollarsWonWinningNumbers = 0;
            int iTotalDollarsWonOverdueNumbers = 0;
            int iTotalDollarsWonScoredNumbers = 0;
            int iTotalDollarsWonAlgorithmNumbers = 0;

            int iWeeklyWinningNumbersPBRight = 0;
            int iWeeklyOverdueNumbersPBRight = 0;
            int iWeeklyScoredNumbersPBRight = 0;
            int iWeeklyAlgorithmNumbersPBRight = 0;
            int iWeeklyTotalTimesWinningNums0Right = 0;
            int iWeeklyTotalTimesOverdueNums0Right = 0;
            int iWeeklyTotalTimesScoredNums0Right = 0;
            int iWeeklyTotalTimesAlgorithmNums0Right = 0;
            int iWeeklyTotalTimesWinningNumsAtLeast1Right = 0;
            int iWeeklyTotalTimesWinningNumsAtLeast2Right = 0;
            int iWeeklyTotalTimesWinningNumsAtLeast3Right = 0;
            int iWeeklyTotalTimesWinningNumsAtLeast4Right = 0;
            int iWeeklyTotalTimesWinningNumsAtLeast5Right = 0;
            int iWeeklyTotalTimesWinningNumsAtLeast6Right = 0;
            int iWeeklyTotalTimesOverdueNumsAtLeast1Right = 0;
            int iWeeklyTotalTimesOverdueNumsAtLeast2Right = 0;
            int iWeeklyTotalTimesOverdueNumsAtLeast3Right = 0;
            int iWeeklyTotalTimesOverdueNumsAtLeast4Right = 0;
            int iWeeklyTotalTimesOverdueNumsAtLeast5Right = 0;
            int iWeeklyTotalTimesOverdueNumsAtLeast6Right = 0;
            int iWeeklyTotalTimesScoredNumsAtLeast1Right = 0;
            int iWeeklyTotalTimesScoredNumsAtLeast2Right = 0;
            int iWeeklyTotalTimesScoredNumsAtLeast3Right = 0;
            int iWeeklyTotalTimesScoredNumsAtLeast4Right = 0;
            int iWeeklyTotalTimesScoredNumsAtLeast5Right = 0;
            int iWeeklyTotalTimesScoredNumsAtLeast6Right = 0;
            int iWeeklyTotalTimesAlgorithmNumsAtLeast1Right = 0;
            int iWeeklyTotalTimesAlgorithmNumsAtLeast2Right = 0;
            int iWeeklyTotalTimesAlgorithmNumsAtLeast3Right = 0;
            int iWeeklyTotalTimesAlgorithmNumsAtLeast4Right = 0;
            int iWeeklyTotalTimesAlgorithmNumsAtLeast5Right = 0;
            int iWeeklyTotalTimesAlgorithmNumsAtLeast6Right = 0;
            int iWeeklyTotalDollarsWonWinningNumbers = 0;
            int iWeeklyTotalDollarsWonOverdueNumbers = 0;
            int iWeeklyTotalDollarsWonScoredNumbers = 0;
            int iWeeklyTotalDollarsWonAlgorithmNumbers = 0;
            int iWeeklyRepeatCountForWinningNumbers = 0;
            string strWeeklyWinningNumbers = string.Empty;
            string strWeeklyOverdueNumbers = string.Empty;
            string strWeeklyScoredNumbers = string.Empty;
            string strWeeklyAlgorithmNumbers = string.Empty;
            string strWeeklyPowerball = string.Empty;
            string strWeeklyOverduePowerball = string.Empty;
            string strWeeklyPowerballWinnerBasedOnPastAverageScore = string.Empty;
            string strWeeklyPBAlgorithmicPowerball = string.Empty;

            string strSQL = string.Empty;
            List<IDbDataParameter> parms = new List<IDbDataParameter>();

            //first thing we want to do is see exactly how much historical data 
            //we have to work with and then calculate using half of it.
            strSQL = "SELECT min(ddate) AS 'MinDate', max(ddate) AS 'MaxDate' ";
            strSQL += " FROM dbo.Temp_RetireData ";
            strSQL += " WHERE gname = :GName";
            parms.Clear();
            parms.Add(m_objRxVectorDatabase.CreateInParameter(":GName", DbType.String, strGName));
            //parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
            var innerquery = from myRow in m_dtRetireData.AsEnumerable()
                             where myRow.Field<string>("GName") == strGName 
                             group myRow by true into r
                             select new
                             {
                                 MinDate = r.Min(z => z.Field<DateTime>("DDate")),
                                 MaxDate = r.Max(z => z.Field<DateTime>("DDate"))
                             };
            try
            {
                //DataTable dt = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
                //if (dt.Rows.Count < 1)
                if (innerquery.Any() == false)
                {
                    this.Cursor = saved_cursor;
                    MessageBox.Show("No rows were returned from Temp_RetireData table.");
                    return;
                }
                //DateTime dtMinDate = Convert.ToDateTime(dt.Rows[0]["MinDate"]);
                //DateTime dtMaxDate = Convert.ToDateTime(dt.Rows[0]["MaxDate"]);
                DateTime dtMinDate = innerquery.ElementAt(0).MinDate;
                DateTime dtMaxDate = innerquery.ElementAt(0).MaxDate;
                dtMinDate = new DateTime(dtMinDate.Year, dtMinDate.Month, dtMinDate.Day);  //just to get rid of time
                dtMaxDate = new DateTime(dtMaxDate.Year, dtMaxDate.Month, dtMaxDate.Day);
                int iB5TotalPlays = 0;
                int iNumDaysOfHistoricalData = (dtMaxDate - dtMinDate).Days;
                int iDaysToLookBack = (iNumDaysOfHistoricalData / 2) - 1; //1 fewer day just to make sure we don't run out of history

                //also see if user only wants to look back for X days before considering all history
                int iUserEnteredNumOfDaysToLookBack = 0;
                if (strGName == "B5" || strGName == "MB")
                {
                    if (txtDaysOfHistoryToConsider.Text.Trim() == string.Empty)
                        txtDaysOfHistoryToConsider.Text = iDaysToLookBack.ToString();
                    else
                    {
                        iUserEnteredNumOfDaysToLookBack = Convert.ToInt32(txtDaysOfHistoryToConsider.Text);
                        if (iUserEnteredNumOfDaysToLookBack <= iDaysToLookBack)
                        {
                            txtDaysOfHistoryToConsider.Text = iUserEnteredNumOfDaysToLookBack.ToString();
                            iDaysToLookBack = iUserEnteredNumOfDaysToLookBack;
                        }
                    }
                }
                else
                {
                    if (txtPowerballDaysOfHistoryToConsider.Text.Trim() == string.Empty)
                        txtPowerballDaysOfHistoryToConsider.Text = iDaysToLookBack.ToString();
                    else
                    {
                        iUserEnteredNumOfDaysToLookBack = Convert.ToInt32(txtPowerballDaysOfHistoryToConsider.Text);
                        if (iUserEnteredNumOfDaysToLookBack <= iDaysToLookBack)
                        {
                            txtPowerballDaysOfHistoryToConsider.Text = iUserEnteredNumOfDaysToLookBack.ToString();
                            iDaysToLookBack = iUserEnteredNumOfDaysToLookBack;
                        }
                    }
                }

                //Figure out our next draw date.  Easy for daily draws, for non-daily draws 
                //like PB and MM we have to see when the next historical draw date even occurred.
                DateTime dtDrawDate = dtMaxDate.AddDays(iDaysToLookBack * -1);
                if (strGName == "PB" || strGName == "MM" || strGName == "MB")
                {
                    strSQL = "SELECT DDate FROM dbo.Temp_RetireData ";
                    strSQL += " WHERE GName = :GName AND DDate > :DDate ";
                    strSQL += " ORDER BY DDate ";
                    parms.Clear();
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":GName", DbType.String, strGName));
                    parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
                    var PBResults = from myRow in m_dtRetireData.AsEnumerable()
                                    .OrderBy(r => r.Field<DateTime>("DDate"))
                                    where myRow.Field<string>("GName") == strGName &&
                                          myRow.Field<DateTime>("DDate") > dtDrawDate
                                    select myRow;
                    DataTable dt2 = PBResults.Any() ? PBResults.CopyToDataTable() : null;
                    //DataTable dt2 = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
                    //TODO - should double-check that order is correct here with linq...
                    if (dt2.Rows.Count < 1)
                    {
                        dtDrawDate = dtMaxDate.AddDays(1);  //should never happen...no data
                    }
                    else
                    {
                        dtDrawDate = Convert.ToDateTime(dt2.Rows[0]["DDate"]);
                        dtDrawDate = new DateTime(dtDrawDate.Year, dtDrawDate.Month, dtDrawDate.Day);  //just to get rid of time
                    }
                }

                while (dtDrawDate <= dtMaxDate)
                {
                    if (dtDrawDate == new DateTime(2022, 11, 7))
                    {
                        string strCaseyBreak = "set breakpoint here";
                    }
                    if (strGName == "B5" || strGName == "MB")
                    {
                        dtpDrawDate.Value = dtDrawDate;
                        btnGenerateBadger5_Click(sender, e);  //calc winning numbers for draw date
                    }
                    else
                    {
                        dtpPowerballDrawDate.Value = dtDrawDate;
                        btnGeneratePowerball_Click(sender, e);
                    }


                    if (((strGName == "B5" || strGName == "MB") && 
                         txtB5WinningNumbersBasedOnPastAverageScores.Text != string.Empty &&
                         txtB5MostOverdueNumbers.Text != string.Empty &&
                         txtWinningNumbers.Text != string.Empty) || 
                        (txtPBWinningNumbersBasedOnPastAverageScores.Text != string.Empty &&
                         txtPBMostOverdueNumbers.Text != string.Empty &&
                         txtPowerballWinningNumbers.Text != string.Empty))
                    {
                        //Valid numbers were determined, so save these picked numbers into our "weekly" strings if 
                        //our weekly repeat count is up.
                        if (strGName.ToUpper() == "MM" || strGName.ToUpper() == "PB")
                        {
                            if (strWeeklyAlgorithmNumbers == string.Empty || iWeeklyRepeatCountForWinningNumbers >= Convert.ToInt32(txtPBKeepNumbersForXPicks.Text))
                            {
                                iWeeklyRepeatCountForWinningNumbers = 0; //reset since we're picking new numbers for the week
                                strWeeklyAlgorithmNumbers = txtPBAlgorithmicNumbers.Text;
                                strWeeklyOverdueNumbers = txtPBMostOverdueNumbers.Text;
                                strWeeklyScoredNumbers = txtPBWinningNumbersBasedOnPastAverageScores.Text;
                                strWeeklyWinningNumbers = txtPowerballWinningNumbers.Text;
                                strWeeklyPowerball = txtPowerball.Text;
                                strWeeklyOverduePowerball = txtPBOverduePowerball.Text;
                                strWeeklyPowerballWinnerBasedOnPastAverageScore = txtPowerballWinnerBasedOnPastAverageScore.Text;
                                strWeeklyPBAlgorithmicPowerball = txtPBAlgorithmicPowerball.Text;
                            }
                        }
                        else  //badger5 or megabucks
                        {
                            if (strWeeklyAlgorithmNumbers == string.Empty || iWeeklyRepeatCountForWinningNumbers >= Convert.ToInt32(txtB5KeepNumbersForXPicks.Text))
                            {
                                iWeeklyRepeatCountForWinningNumbers = 0; //reset since we're picking new numbers for the week
                                strWeeklyAlgorithmNumbers = txtB5AlgorithmicNumbers.Text;
                                strWeeklyOverdueNumbers = txtB5MostOverdueNumbers.Text;
                                strWeeklyScoredNumbers = txtB5WinningNumbersBasedOnPastAverageScores.Text;
                                strWeeklyWinningNumbers = txtWinningNumbers.Text;
                            }
                        }

                        //Examine what computer picked vs. what was actualy picked on this day.
                        strSQL = "SELECT * FROM dbo.Temp_RetireData ";
                        strSQL += " WHERE gname = :GName ";
                        strSQL += " AND CAST(DDate AS DATE) = :DDate";
                        parms.Clear();
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":GName", DbType.String, strGName));
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
                        var PBResults2 = from myRow in m_dtRetireData.AsEnumerable()
                                        where myRow.Field<string>("GName") == strGName &&
                                              myRow.Field<DateTime>("DDate").Year == dtDrawDate.Year &&
                                              myRow.Field<DateTime>("DDate").Month == dtDrawDate.Month &&
                                              myRow.Field<DateTime>("DDate").Day == dtDrawDate.Day
                                        select myRow;
                        DataTable dt = PBResults2.Any() ? PBResults2.CopyToDataTable() : null;
                        //dt = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
                        if (dt.Rows.Count > 0)
                        {
                            string[] winningNumbers;
                            string[] weeklyWinningNumbers;
                            if (strGName == "B5" || strGName == "MB")
                            {
                                winningNumbers = this.txtWinningNumbers.Text.Split(',');
                                weeklyWinningNumbers = strWeeklyWinningNumbers.Split(',');
                            }
                            else
                            {
                                winningNumbers = this.txtPowerballWinningNumbers.Text.Split(',');
                                weeklyWinningNumbers = strWeeklyWinningNumbers.Split(',');
                            }

                            string[] overdueNumbers;
                            string[] weeklyOverdueNumbers;
                            if (strGName == "B5" || strGName == "MB")
                            {
                                overdueNumbers = this.txtB5MostOverdueNumbers.Text.Split(',');
                                weeklyOverdueNumbers = strWeeklyOverdueNumbers.Split(',');
                            }
                            else
                            {
                                overdueNumbers = this.txtPBMostOverdueNumbers.Text.Split(',');
                                weeklyOverdueNumbers = strWeeklyOverdueNumbers.Split(',');
                            }

                            string[] scoredNumbers;
                            string[] weeklyScoredNumbers;
                            if (strGName == "B5" || strGName == "MB")
                            {
                                scoredNumbers = this.txtB5WinningNumbersBasedOnPastAverageScores.Text.Split(',');
                                weeklyScoredNumbers = strWeeklyScoredNumbers.Split(',');
                            }
                            else
                            {
                                scoredNumbers = this.txtPBWinningNumbersBasedOnPastAverageScores.Text.Split(',');
                                weeklyScoredNumbers = strWeeklyScoredNumbers.Split(',');
                            }

                            string[] algorithmNumbers;
                            string[] weeklyAlgorithmNumbers;
                            if (strGName == "B5" || strGName == "MB")
                            {
                                //this.txtB5AlgorithmicNumbers.Text = "-1, -2, -3, -4, -5"; //todo - remove this once algorithm for badger5 is determined
                                algorithmNumbers = this.txtB5AlgorithmicNumbers.Text.Split(',');
                                weeklyAlgorithmNumbers = strWeeklyAlgorithmNumbers.Split(',');
                            }
                            else
                            {
                                algorithmNumbers = this.txtPBAlgorithmicNumbers.Text.Split(',');
                                weeklyAlgorithmNumbers = strWeeklyAlgorithmNumbers.Split(',');
                            }

                            int[] intsWinningNumbers = winningNumbers.Select(int.Parse).ToArray();  //use LINQ to convert our string array to int array
                            int[] intOverdueNumbers = overdueNumbers.Select(int.Parse).ToArray();
                            int[] intScoredNumbers = scoredNumbers.Select(int.Parse).ToArray();
                            int[] intAlgorithmNumbers = algorithmNumbers.Select(int.Parse).ToArray();
                            int[] weeklyIntsWinningNumbers = weeklyWinningNumbers.Select(int.Parse).ToArray();
                            int[] weeklyIntOverdueNumbers = weeklyOverdueNumbers.Select(int.Parse).ToArray();
                            int[] weeklyIntScoredNumbers = weeklyScoredNumbers.Select(int.Parse).ToArray();
                            int[] weeklyIntAlgorithmNumbers = weeklyAlgorithmNumbers.Select(int.Parse).ToArray();
                            int iWinningNumbersRight = 0;
                            int iOverdueNumbersRight = 0;
                            int iScoredNumbersRight = 0;
                            int iAlgorithmNumbersRight = 0;
                            int iWeeklyWinningNumbersRight = 0;
                            int iWeeklyOverdueNumbersRight = 0;
                            int iWeeklyScoredNumbersRight = 0;
                            int iWeeklyAlgorithmNumbersRight = 0;
                            bool bWinningNumbersPBRight = false;
                            bool bOverdueNumbersPBRight = false;
                            bool bScoredNumbersPBRight = false;
                            bool bAlgorithmNumbersPBRight = false;
                            bool bWeeklyWinningNumbersPBRight = false;
                            bool bWeeklyOverdueNumbersPBRight = false;
                            bool bWeeklyScoredNumbersPBRight = false;
                            bool bWeeklyAlgorithmNumbersPBRight = false;
                            //use LINQ's extension method "contains" to see if the number is in our array of numbers
                            if (intsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N1"])))
                                iWinningNumbersRight++;
                            if (intsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N2"])))
                                iWinningNumbersRight++;
                            if (intsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N3"])))
                                iWinningNumbersRight++;
                            if (intsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N4"])))
                                iWinningNumbersRight++;
                            if (intsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N5"])))
                                iWinningNumbersRight++;
                            if (strGName == "MB" && intsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N6"])))
                                iWinningNumbersRight++;
                            if (weeklyIntsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N1"])))
                                iWeeklyWinningNumbersRight++;
                            if (weeklyIntsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N2"])))
                                iWeeklyWinningNumbersRight++;
                            if (weeklyIntsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N3"])))
                                iWeeklyWinningNumbersRight++;
                            if (weeklyIntsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N4"])))
                                iWeeklyWinningNumbersRight++;
                            if (weeklyIntsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N5"])))
                                iWeeklyWinningNumbersRight++;
                            if (strGName == "MB" && weeklyIntsWinningNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N6"])))
                                iWeeklyWinningNumbersRight++;

                            if (intOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N1"])))
                                iOverdueNumbersRight++;
                            if (intOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N2"])))
                                iOverdueNumbersRight++;
                            if (intOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N3"])))
                                iOverdueNumbersRight++;
                            if (intOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N4"])))
                                iOverdueNumbersRight++;
                            if (intOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N5"])))
                                iOverdueNumbersRight++;
                            if (strGName == "MB" && intOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N6"])))
                                iOverdueNumbersRight++;
                            if (weeklyIntOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N1"])))
                                iWeeklyOverdueNumbersRight++;
                            if (weeklyIntOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N2"])))
                                iWeeklyOverdueNumbersRight++;
                            if (weeklyIntOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N3"])))
                                iWeeklyOverdueNumbersRight++;
                            if (weeklyIntOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N4"])))
                                iWeeklyOverdueNumbersRight++;
                            if (weeklyIntOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N5"])))
                                iWeeklyOverdueNumbersRight++;
                            if (strGName == "MB" && weeklyIntOverdueNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N6"])))
                                iWeeklyOverdueNumbersRight++;

                            if (intScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N1"])))
                                iScoredNumbersRight++;
                            if (intScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N2"])))
                                iScoredNumbersRight++;
                            if (intScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N3"])))
                                iScoredNumbersRight++;
                            if (intScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N4"])))
                                iScoredNumbersRight++;
                            if (intScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N5"])))
                                iScoredNumbersRight++;
                            if (strGName == "MB" && intScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N6"])))
                                iScoredNumbersRight++;
                            if (weeklyIntScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N1"])))
                                iWeeklyScoredNumbersRight++;
                            if (weeklyIntScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N2"])))
                                iWeeklyScoredNumbersRight++;
                            if (weeklyIntScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N3"])))
                                iWeeklyScoredNumbersRight++;
                            if (weeklyIntScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N4"])))
                                iWeeklyScoredNumbersRight++;
                            if (weeklyIntScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N5"])))
                                iWeeklyScoredNumbersRight++;
                            if (strGName == "MB" && weeklyIntScoredNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N6"])))
                                iWeeklyScoredNumbersRight++;

                            if (intAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N1"])))
                                iAlgorithmNumbersRight++;
                            if (intAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N2"])))
                                iAlgorithmNumbersRight++;
                            if (intAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N3"])))
                                iAlgorithmNumbersRight++;
                            if (intAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N4"])))
                                iAlgorithmNumbersRight++;
                            if (intAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N5"])))
                                iAlgorithmNumbersRight++;
                            if (strGName == "MB" && intAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N6"])))
                                iAlgorithmNumbersRight++;
                            if (weeklyIntAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N1"])))
                                iWeeklyAlgorithmNumbersRight++;
                            if (weeklyIntAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N2"])))
                                iWeeklyAlgorithmNumbersRight++;
                            if (weeklyIntAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N3"])))
                                iWeeklyAlgorithmNumbersRight++;
                            if (weeklyIntAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N4"])))
                                iWeeklyAlgorithmNumbersRight++;
                            if (weeklyIntAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N5"])))
                                iWeeklyAlgorithmNumbersRight++;
                            if (strGName == "MB" && weeklyIntAlgorithmNumbers.Contains(Convert.ToInt32(dt.Rows[0]["N6"])))
                                iWeeklyAlgorithmNumbersRight++;

                            if (iWinningNumbersRight == 0)
                                iTotalTimesWinningNums0Right++;
                            if (iWinningNumbersRight >= 1)
                                iTotalTimesWinningNumsAtLeast1Right++;
                            if (iWinningNumbersRight >= 2)
                                iTotalTimesWinningNumsAtLeast2Right++;
                            if (iWinningNumbersRight >= 3)
                                iTotalTimesWinningNumsAtLeast3Right++;
                            if (iWinningNumbersRight >= 4)
                                iTotalTimesWinningNumsAtLeast4Right++;
                            if (iWinningNumbersRight >= 5)
                                iTotalTimesWinningNumsAtLeast5Right++;
                            if (strGName == "MB" && iWinningNumbersRight >= 6)
                                iTotalTimesWinningNumsAtLeast6Right++;
                            if (iWeeklyWinningNumbersRight == 0)
                                iWeeklyTotalTimesWinningNums0Right++;
                            if (iWeeklyWinningNumbersRight >= 1)
                                iWeeklyTotalTimesWinningNumsAtLeast1Right++;
                            if (iWeeklyWinningNumbersRight >= 2)
                                iWeeklyTotalTimesWinningNumsAtLeast2Right++;
                            if (iWeeklyWinningNumbersRight >= 3)
                                iWeeklyTotalTimesWinningNumsAtLeast3Right++;
                            if (iWeeklyWinningNumbersRight >= 4)
                                iWeeklyTotalTimesWinningNumsAtLeast4Right++;
                            if (iWeeklyWinningNumbersRight >= 5)
                                iWeeklyTotalTimesWinningNumsAtLeast5Right++;
                            if (strGName == "MB" && iWeeklyWinningNumbersRight >= 6)
                                iWeeklyTotalTimesWinningNumsAtLeast6Right++;

                            if (iOverdueNumbersRight == 0)
                                iTotalTimesOverdueNums0Right++;
                            if (iOverdueNumbersRight >= 1)
                                iTotalTimesOverdueNumsAtLeast1Right++;
                            if (iOverdueNumbersRight >= 2)
                                iTotalTimesOverdueNumsAtLeast2Right++;
                            if (iOverdueNumbersRight >= 3)
                                iTotalTimesOverdueNumsAtLeast3Right++;
                            if (iOverdueNumbersRight >= 4)
                                iTotalTimesOverdueNumsAtLeast4Right++;
                            if (iOverdueNumbersRight >= 5)
                                iTotalTimesOverdueNumsAtLeast5Right++;
                            if (strGName == "MB" && iOverdueNumbersRight >= 6)
                                iTotalTimesOverdueNumsAtLeast6Right++;
                            if (iWeeklyOverdueNumbersRight == 0)
                                iWeeklyTotalTimesOverdueNums0Right++;
                            if (iWeeklyOverdueNumbersRight >= 1)
                                iWeeklyTotalTimesOverdueNumsAtLeast1Right++;
                            if (iWeeklyOverdueNumbersRight >= 2)
                                iWeeklyTotalTimesOverdueNumsAtLeast2Right++;
                            if (iWeeklyOverdueNumbersRight >= 3)
                                iWeeklyTotalTimesOverdueNumsAtLeast3Right++;
                            if (iWeeklyOverdueNumbersRight >= 4)
                                iWeeklyTotalTimesOverdueNumsAtLeast4Right++;
                            if (iWeeklyOverdueNumbersRight >= 5)
                                iWeeklyTotalTimesOverdueNumsAtLeast5Right++;
                            if (strGName == "MB" && iWeeklyOverdueNumbersRight >= 6)
                                iWeeklyTotalTimesOverdueNumsAtLeast6Right++;

                            if (iScoredNumbersRight == 0)
                                iTotalTimesScoredNums0Right++;
                            if (iScoredNumbersRight >= 1)
                                iTotalTimesScoredNumsAtLeast1Right++;
                            if (iScoredNumbersRight >= 2)
                                iTotalTimesScoredNumsAtLeast2Right++;
                            if (iScoredNumbersRight >= 3)
                                iTotalTimesScoredNumsAtLeast3Right++;
                            if (iScoredNumbersRight >= 4)
                                iTotalTimesScoredNumsAtLeast4Right++;
                            if (iScoredNumbersRight >= 5)
                                iTotalTimesScoredNumsAtLeast5Right++;
                            if (strGName == "MB" && iScoredNumbersRight >= 6)
                                iTotalTimesScoredNumsAtLeast6Right++;
                            if (iWeeklyScoredNumbersRight == 0)
                                iWeeklyTotalTimesScoredNums0Right++;
                            if (iWeeklyScoredNumbersRight >= 1)
                                iWeeklyTotalTimesScoredNumsAtLeast1Right++;
                            if (iWeeklyScoredNumbersRight >= 2)
                                iWeeklyTotalTimesScoredNumsAtLeast2Right++;
                            if (iWeeklyScoredNumbersRight >= 3)
                                iWeeklyTotalTimesScoredNumsAtLeast3Right++;
                            if (iWeeklyScoredNumbersRight >= 4)
                                iWeeklyTotalTimesScoredNumsAtLeast4Right++;
                            if (iWeeklyScoredNumbersRight >= 5)
                                iWeeklyTotalTimesScoredNumsAtLeast5Right++;
                            if (strGName == "MB" && iWeeklyScoredNumbersRight >= 6)
                                iWeeklyTotalTimesScoredNumsAtLeast6Right++;

                            if (iAlgorithmNumbersRight == 0)
                                iTotalTimesAlgorithmNums0Right++;
                            if (iAlgorithmNumbersRight >= 1)
                                iTotalTimesAlgorithmNumsAtLeast1Right++;
                            if (iAlgorithmNumbersRight >= 2)
                                iTotalTimesAlgorithmNumsAtLeast2Right++;
                            if (iAlgorithmNumbersRight >= 3)
                                iTotalTimesAlgorithmNumsAtLeast3Right++;
                            if (iAlgorithmNumbersRight >= 4)
                                iTotalTimesAlgorithmNumsAtLeast4Right++;
                            if (iAlgorithmNumbersRight >= 5)
                                iTotalTimesAlgorithmNumsAtLeast5Right++;
                            if (strGName == "MB" && iAlgorithmNumbersRight >= 6)
                                iTotalTimesAlgorithmNumsAtLeast6Right++;
                            if (iWeeklyAlgorithmNumbersRight == 0)
                                iWeeklyTotalTimesAlgorithmNums0Right++;
                            if (iWeeklyAlgorithmNumbersRight >= 1)
                                iWeeklyTotalTimesAlgorithmNumsAtLeast1Right++;
                            if (iWeeklyAlgorithmNumbersRight >= 2)
                                iWeeklyTotalTimesAlgorithmNumsAtLeast2Right++;
                            if (iWeeklyAlgorithmNumbersRight >= 3)
                                iWeeklyTotalTimesAlgorithmNumsAtLeast3Right++;
                            if (iWeeklyAlgorithmNumbersRight >= 4)
                                iWeeklyTotalTimesAlgorithmNumsAtLeast4Right++;
                            if (iWeeklyAlgorithmNumbersRight >= 5)
                                iWeeklyTotalTimesAlgorithmNumsAtLeast5Right++;
                            if (strGName == "MB" && iWeeklyAlgorithmNumbersRight >= 6)
                                iWeeklyTotalTimesAlgorithmNumsAtLeast6Right++;

                            //also do some quick number position analysis (e.g. how many times was our 1st number picked vs 2nd number picked, etc)
                            if ((intsWinningNumbers[0] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intsWinningNumbers[0] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intsWinningNumbers[0] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intsWinningNumbers[0] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intsWinningNumbers[0] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intsWinningNumbers[0] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iWinningNumbersPos1Right++;
                            if ((intsWinningNumbers[1] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intsWinningNumbers[1] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intsWinningNumbers[1] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intsWinningNumbers[1] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intsWinningNumbers[1] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intsWinningNumbers[1] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iWinningNumbersPos2Right++;
                            if ((intsWinningNumbers[2] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intsWinningNumbers[2] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intsWinningNumbers[2] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intsWinningNumbers[2] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intsWinningNumbers[2] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intsWinningNumbers[2] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iWinningNumbersPos3Right++;
                            if ((intsWinningNumbers[3] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intsWinningNumbers[3] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intsWinningNumbers[3] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intsWinningNumbers[3] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intsWinningNumbers[3] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intsWinningNumbers[3] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iWinningNumbersPos4Right++;
                            if ((intsWinningNumbers[4] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intsWinningNumbers[4] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intsWinningNumbers[4] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intsWinningNumbers[4] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intsWinningNumbers[4] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intsWinningNumbers[4] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iWinningNumbersPos5Right++;
                            if (strGName == "MB")
                            {
                                if (intsWinningNumbers[5] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                    intsWinningNumbers[5] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                    intsWinningNumbers[5] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                    intsWinningNumbers[5] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                    intsWinningNumbers[5] == Convert.ToInt32(dt.Rows[0]["N5"]) ||
                                    intsWinningNumbers[5] == Convert.ToInt32(dt.Rows[0]["N6"]))
                                    iWinningNumbersPos6Right++;
                            }
                            if (strGName == "PB" || strGName == "MM")
                            {
                                if (Convert.ToInt32(txtPowerball.Text) == Convert.ToInt32(dt.Rows[0]["PB"]))
                                {
                                    iWinningNumbersPBRight++;
                                    bWinningNumbersPBRight = true;
                                }
                                if (Convert.ToInt32(strWeeklyPowerball) == Convert.ToInt32(dt.Rows[0]["PB"]))
                                {
                                    iWeeklyWinningNumbersPBRight++;
                                    bWeeklyWinningNumbersPBRight = true;
                                }
                            }

                            if ((intOverdueNumbers[0] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intOverdueNumbers[0] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intOverdueNumbers[0] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intOverdueNumbers[0] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intOverdueNumbers[0] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intOverdueNumbers[0] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iOverdueNumbersPos1Right++;
                            if ((intOverdueNumbers[1] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intOverdueNumbers[1] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intOverdueNumbers[1] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intOverdueNumbers[1] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intOverdueNumbers[1] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intOverdueNumbers[1] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iOverdueNumbersPos2Right++;
                            if ((intOverdueNumbers[2] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intOverdueNumbers[2] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intOverdueNumbers[2] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intOverdueNumbers[2] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intOverdueNumbers[2] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intOverdueNumbers[2] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iOverdueNumbersPos3Right++;
                            if ((intOverdueNumbers[3] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intOverdueNumbers[3] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intOverdueNumbers[3] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intOverdueNumbers[3] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intOverdueNumbers[3] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intOverdueNumbers[3] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iOverdueNumbersPos4Right++;
                            if ((intOverdueNumbers[4] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intOverdueNumbers[4] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intOverdueNumbers[4] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intOverdueNumbers[4] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intOverdueNumbers[4] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intOverdueNumbers[4] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iOverdueNumbersPos5Right++;
                            if (strGName == "MB")
                            {
                                if (intOverdueNumbers[5] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                    intOverdueNumbers[5] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                    intOverdueNumbers[5] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                    intOverdueNumbers[5] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                    intOverdueNumbers[5] == Convert.ToInt32(dt.Rows[0]["N5"]) ||
                                    intOverdueNumbers[5] == Convert.ToInt32(dt.Rows[0]["N6"]))
                                    iOverdueNumbersPos6Right++;
                            }
                            if (strGName == "PB" || strGName == "MM")
                            {
                                if (Convert.ToInt32(txtPBOverduePowerball.Text) == Convert.ToInt32(dt.Rows[0]["PB"]))
                                {
                                    iOverdueNumbersPBRight++;
                                    bOverdueNumbersPBRight = true;
                                }
                                if (Convert.ToInt32(strWeeklyOverduePowerball) == Convert.ToInt32(dt.Rows[0]["PB"]))
                                {
                                    iWeeklyOverdueNumbersPBRight++;
                                    bWeeklyOverdueNumbersPBRight = true;
                                }
                            }

                            if ((intScoredNumbers[0] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intScoredNumbers[0] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intScoredNumbers[0] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intScoredNumbers[0] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intScoredNumbers[0] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intScoredNumbers[0] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iScoredNumbersPos1Right++;
                            if ((intScoredNumbers[1] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intScoredNumbers[1] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intScoredNumbers[1] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intScoredNumbers[1] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intScoredNumbers[1] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intScoredNumbers[1] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iScoredNumbersPos2Right++;
                            if ((intScoredNumbers[2] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intScoredNumbers[2] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intScoredNumbers[2] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intScoredNumbers[2] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intScoredNumbers[2] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intScoredNumbers[2] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iScoredNumbersPos3Right++;
                            if ((intScoredNumbers[3] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intScoredNumbers[3] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intScoredNumbers[3] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intScoredNumbers[3] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intScoredNumbers[3] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intScoredNumbers[3] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iScoredNumbersPos4Right++;
                            if ((intScoredNumbers[4] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intScoredNumbers[4] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intScoredNumbers[4] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intScoredNumbers[4] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intScoredNumbers[4] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intScoredNumbers[4] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iScoredNumbersPos5Right++;
                            if (strGName == "MB")
                            {
                                if (intScoredNumbers[5] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                    intScoredNumbers[5] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                    intScoredNumbers[5] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                    intScoredNumbers[5] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                    intScoredNumbers[5] == Convert.ToInt32(dt.Rows[0]["N5"]) ||
                                    intScoredNumbers[5] == Convert.ToInt32(dt.Rows[0]["N6"]))
                                    iScoredNumbersPos6Right++;
                            }
                            if (strGName == "PB" || strGName == "MM")
                            {
                                if (Convert.ToInt32(txtPowerballWinnerBasedOnPastAverageScore.Text) == Convert.ToInt32(dt.Rows[0]["PB"]))
                                {
                                    iScoredNumbersPBRight++;
                                    bScoredNumbersPBRight = true;
                                }
                                if (Convert.ToInt32(strWeeklyPowerballWinnerBasedOnPastAverageScore) == Convert.ToInt32(dt.Rows[0]["PB"]))
                                {
                                    iWeeklyScoredNumbersPBRight++;
                                    bWeeklyScoredNumbersPBRight = true;
                                }
                            }

                            if ((intAlgorithmNumbers[0] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intAlgorithmNumbers[0] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intAlgorithmNumbers[0] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intAlgorithmNumbers[0] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intAlgorithmNumbers[0] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intAlgorithmNumbers[0] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iAlgorithmNumbersPos1Right++;
                            if ((intAlgorithmNumbers[1] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intAlgorithmNumbers[1] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intAlgorithmNumbers[1] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intAlgorithmNumbers[1] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intAlgorithmNumbers[1] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intAlgorithmNumbers[1] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iAlgorithmNumbersPos2Right++;
                            if ((intAlgorithmNumbers[2] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intAlgorithmNumbers[2] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intAlgorithmNumbers[2] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intAlgorithmNumbers[2] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intAlgorithmNumbers[2] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intAlgorithmNumbers[2] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iAlgorithmNumbersPos3Right++;
                            if ((intAlgorithmNumbers[3] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intAlgorithmNumbers[3] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intAlgorithmNumbers[3] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intAlgorithmNumbers[3] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intAlgorithmNumbers[3] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intAlgorithmNumbers[3] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iAlgorithmNumbersPos4Right++;
                            if ((intAlgorithmNumbers[4] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                 intAlgorithmNumbers[4] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                 intAlgorithmNumbers[4] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                 intAlgorithmNumbers[4] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                 intAlgorithmNumbers[4] == Convert.ToInt32(dt.Rows[0]["N5"])) || 
                                (strGName == "MB" && intAlgorithmNumbers[4] == Convert.ToInt32(dt.Rows[0]["N6"])))
                                iAlgorithmNumbersPos5Right++;
                            if (strGName == "MB")
                            {
                                if (intAlgorithmNumbers[5] == Convert.ToInt32(dt.Rows[0]["N1"]) ||
                                    intAlgorithmNumbers[5] == Convert.ToInt32(dt.Rows[0]["N2"]) ||
                                    intAlgorithmNumbers[5] == Convert.ToInt32(dt.Rows[0]["N3"]) ||
                                    intAlgorithmNumbers[5] == Convert.ToInt32(dt.Rows[0]["N4"]) ||
                                    intAlgorithmNumbers[5] == Convert.ToInt32(dt.Rows[0]["N5"]) ||
                                    intAlgorithmNumbers[5] == Convert.ToInt32(dt.Rows[0]["N6"]))
                                    iAlgorithmNumbersPos6Right++;
                            }
                            if (strGName == "PB" || strGName == "MM")
                            {
                                if (Convert.ToInt32(txtPBAlgorithmicPowerball.Text) == Convert.ToInt32(dt.Rows[0]["PB"]))
                                {
                                    iAlgorithmNumbersPBRight++;
                                    bAlgorithmNumbersPBRight = true;
                                }
                                if (Convert.ToInt32(strWeeklyPBAlgorithmicPowerball) == Convert.ToInt32(dt.Rows[0]["PB"]))
                                {
                                    iWeeklyAlgorithmNumbersPBRight++;
                                    bWeeklyAlgorithmNumbersPBRight = true;
                                }
                            }


                            //check our computer picked numbers to see if they have EVER been the right numbers in all of the history 
                            //we have for the game...
                            EnumerableRowCollection<DataRow> wasWinningEverRightResults;
                            DataTable dtJunk123;
                            if (strGName != "MM")
                            {
                                wasWinningEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                             where myRow.Field<string>("GName") == strGName &&
                                                             myRow.Field<Int32>("N1") == intsWinningNumbers[0] &&
                                                             myRow.Field<Int32>("N2") == intsWinningNumbers[1] &&
                                                             myRow.Field<Int32>("N3") == intsWinningNumbers[2] &&
                                                             myRow.Field<Int32>("N4") == intsWinningNumbers[3]
                                                             select myRow;
                                dtJunk123 = wasWinningEverRightResults.Any() ? wasWinningEverRightResults.CopyToDataTable() : null;
                                if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                                {
                                    m_strPopupMessage += "Generated WINNING picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 4 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                                }
                            }
                            wasWinningEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                         where myRow.Field<string>("GName") == strGName &&
                                                         myRow.Field<Int32>("N1") == intsWinningNumbers[0] &&
                                                         myRow.Field<Int32>("N2") == intsWinningNumbers[1] &&
                                                         myRow.Field<Int32>("N3") == intsWinningNumbers[2] &&
                                                         myRow.Field<Int32>("N4") == intsWinningNumbers[3] &&
                                                         myRow.Field<Int32>("N5") == intsWinningNumbers[4]
                                                         select myRow;
                            dtJunk123 = wasWinningEverRightResults.Any() ? wasWinningEverRightResults.CopyToDataTable() : null;
                            if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                            {
                                m_strPopupMessage += "Generated WINNING picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 5 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                            }
                            if (strGName == "MM")
                            {
                                wasWinningEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                             where myRow.Field<string>("GName") == strGName &&
                                                             myRow.Field<Int32>("N1") == intsWinningNumbers[0] &&
                                                             myRow.Field<Int32>("N2") == intsWinningNumbers[1] &&
                                                             myRow.Field<Int32>("N3") == intsWinningNumbers[2] &&
                                                             myRow.Field<Int32>("N4") == intsWinningNumbers[3] &&
                                                             myRow.Field<Int32>("N5") == intsWinningNumbers[4] &&
                                                             myRow.Field<Int32>("N6") == intsWinningNumbers[5]
                                                             select myRow;
                                dtJunk123 = wasWinningEverRightResults.Any() ? wasWinningEverRightResults.CopyToDataTable() : null;
                                if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                                {
                                    m_strPopupMessage += "Generated WINNING picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 6 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                                }
                            }
                            EnumerableRowCollection<DataRow> wasOverdueEverRightResults;
                            if (strGName != "MM")
                            {
                                wasOverdueEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                             where myRow.Field<string>("GName") == strGName &&
                                                             myRow.Field<Int32>("N1") == intOverdueNumbers[0] &&
                                                             myRow.Field<Int32>("N2") == intOverdueNumbers[1] &&
                                                             myRow.Field<Int32>("N3") == intOverdueNumbers[2] &&
                                                             myRow.Field<Int32>("N4") == intOverdueNumbers[3]
                                                             select myRow;
                                dtJunk123 = wasOverdueEverRightResults.Any() ? wasOverdueEverRightResults.CopyToDataTable() : null;
                                if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                                {
                                    m_strPopupMessage += "Generated OVERDUE picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 4 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                                }
                            }
                            wasOverdueEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                         where myRow.Field<string>("GName") == strGName &&
                                                         myRow.Field<Int32>("N1") == intOverdueNumbers[0] &&
                                                         myRow.Field<Int32>("N2") == intOverdueNumbers[1] &&
                                                         myRow.Field<Int32>("N3") == intOverdueNumbers[2] &&
                                                         myRow.Field<Int32>("N4") == intOverdueNumbers[3] &&
                                                         myRow.Field<Int32>("N5") == intOverdueNumbers[4]
                                                         select myRow;
                            dtJunk123 = wasOverdueEverRightResults.Any() ? wasOverdueEverRightResults.CopyToDataTable() : null;
                            if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                            {
                                m_strPopupMessage += "Generated OVERDUE picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 5 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                            }
                            if (strGName == "MM")
                            {
                                wasOverdueEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                             where myRow.Field<string>("GName") == strGName &&
                                                             myRow.Field<Int32>("N1") == intOverdueNumbers[0] &&
                                                             myRow.Field<Int32>("N2") == intOverdueNumbers[1] &&
                                                             myRow.Field<Int32>("N3") == intOverdueNumbers[2] &&
                                                             myRow.Field<Int32>("N4") == intOverdueNumbers[3] &&
                                                             myRow.Field<Int32>("N5") == intOverdueNumbers[4] &&
                                                             myRow.Field<Int32>("N6") == intOverdueNumbers[5]
                                                             select myRow;
                                dtJunk123 = wasOverdueEverRightResults.Any() ? wasOverdueEverRightResults.CopyToDataTable() : null;
                                if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                                {
                                    m_strPopupMessage += "Generated OVERDUE picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 6 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                                }
                            }
                            EnumerableRowCollection<DataRow> wasScoredEverRightResults;
                            if (strGName != "MM")
                            {
                                wasScoredEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                             where myRow.Field<string>("GName") == strGName &&
                                                             myRow.Field<Int32>("N1") == intScoredNumbers[0] &&
                                                             myRow.Field<Int32>("N2") == intScoredNumbers[1] &&
                                                             myRow.Field<Int32>("N3") == intScoredNumbers[2] &&
                                                             myRow.Field<Int32>("N4") == intScoredNumbers[3]
                                                             select myRow;
                                dtJunk123 = wasScoredEverRightResults.Any() ? wasScoredEverRightResults.CopyToDataTable() : null;
                                if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                                {
                                    m_strPopupMessage += "Generated SCORED picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 4 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                                }
                            }
                            wasScoredEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                         where myRow.Field<string>("GName") == strGName &&
                                                         myRow.Field<Int32>("N1") == intScoredNumbers[0] &&
                                                         myRow.Field<Int32>("N2") == intScoredNumbers[1] &&
                                                         myRow.Field<Int32>("N3") == intScoredNumbers[2] &&
                                                         myRow.Field<Int32>("N4") == intScoredNumbers[3] &&
                                                         myRow.Field<Int32>("N5") == intScoredNumbers[4]
                                                         select myRow;
                            dtJunk123 = wasScoredEverRightResults.Any() ? wasScoredEverRightResults.CopyToDataTable() : null;
                            if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                            {
                                m_strPopupMessage += "Generated SCORED picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 5 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                            }
                            if (strGName == "MM")
                            {
                                wasScoredEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                             where myRow.Field<string>("GName") == strGName &&
                                                             myRow.Field<Int32>("N1") == intScoredNumbers[0] &&
                                                             myRow.Field<Int32>("N2") == intScoredNumbers[1] &&
                                                             myRow.Field<Int32>("N3") == intScoredNumbers[2] &&
                                                             myRow.Field<Int32>("N4") == intScoredNumbers[3] &&
                                                             myRow.Field<Int32>("N5") == intScoredNumbers[4] &&
                                                             myRow.Field<Int32>("N6") == intScoredNumbers[5]
                                                             select myRow;
                                dtJunk123 = wasScoredEverRightResults.Any() ? wasScoredEverRightResults.CopyToDataTable() : null;
                                if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                                {
                                    m_strPopupMessage += "Generated SCORED picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 6 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                                }
                            }
                            EnumerableRowCollection<DataRow> wasAlgorithmEverRightResults;
                            if (strGName != "MM")
                            {
                                wasAlgorithmEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                            where myRow.Field<string>("GName") == strGName &&
                                                            myRow.Field<Int32>("N1") == intAlgorithmNumbers[0] &&
                                                            myRow.Field<Int32>("N2") == intAlgorithmNumbers[1] &&
                                                            myRow.Field<Int32>("N3") == intAlgorithmNumbers[2] &&
                                                            myRow.Field<Int32>("N4") == intAlgorithmNumbers[3]
                                                            select myRow;
                                dtJunk123 = wasAlgorithmEverRightResults.Any() ? wasAlgorithmEverRightResults.CopyToDataTable() : null;
                                if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                                {
                                    m_strPopupMessage += "Generated ALGORITHM picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 4 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                                }
                            }
                            wasAlgorithmEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                        where myRow.Field<string>("GName") == strGName &&
                                                        myRow.Field<Int32>("N1") == intAlgorithmNumbers[0] &&
                                                        myRow.Field<Int32>("N2") == intAlgorithmNumbers[1] &&
                                                        myRow.Field<Int32>("N3") == intAlgorithmNumbers[2] &&
                                                        myRow.Field<Int32>("N4") == intAlgorithmNumbers[3] &&
                                                        myRow.Field<Int32>("N5") == intAlgorithmNumbers[4]
                                                        select myRow;
                            dtJunk123 = wasAlgorithmEverRightResults.Any() ? wasAlgorithmEverRightResults.CopyToDataTable() : null;
                            if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                            {
                                m_strPopupMessage += "Generated ALGORITHM picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 5 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                            }
                            if (strGName == "MM")
                            {
                                wasAlgorithmEverRightResults = from myRow in m_dtRetireData.AsEnumerable()
                                                            where myRow.Field<string>("GName") == strGName &&
                                                            myRow.Field<Int32>("N1") == intAlgorithmNumbers[0] &&
                                                            myRow.Field<Int32>("N2") == intAlgorithmNumbers[1] &&
                                                            myRow.Field<Int32>("N3") == intAlgorithmNumbers[2] &&
                                                            myRow.Field<Int32>("N4") == intAlgorithmNumbers[3] &&
                                                            myRow.Field<Int32>("N5") == intAlgorithmNumbers[4] &&
                                                            myRow.Field<Int32>("N6") == intAlgorithmNumbers[5]
                                                            select myRow;
                                dtJunk123 = wasAlgorithmEverRightResults.Any() ? wasAlgorithmEverRightResults.CopyToDataTable() : null;
                                if (dtJunk123 != null && dtJunk123.Rows.Count > 0)
                                {
                                    m_strPopupMessage += "Generated ALGORITHM picks for " + dtDrawDate.ToString("ddd MM-dd-yyyy") + " resulted in 6 numbers that matched actual numbers " + dtJunk123.Rows.Count.ToString() + " times, from actual date " + Convert.ToDateTime(dtJunk123.Rows[0]["DDate"]).ToString("ddd MM-dd-yyyy") + Environment.NewLine;
                                }
                            }



                            //While we are here, do some calculations to determine if our numbers 
                            //would have won any money.
                            if (strGName == "MM" || strGName == "PB")
                            {
                                if (bWinningNumbersPBRight == true && iWinningNumbersRight == 5)
                                    iTotalDollarsWonWinningNumbers = int.MaxValue;  //jackpot!!
                                else if (bWinningNumbersPBRight == false && iWinningNumbersRight == 5)
                                    iTotalDollarsWonWinningNumbers += 1000000;  //won a million!
                                else if (bWinningNumbersPBRight == true && iWinningNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonWinningNumbers += 10000;  //won 10,000!
                                    else
                                        iTotalDollarsWonWinningNumbers += 50000;  //won 50,000!
                                }
                                else if (bWinningNumbersPBRight == false && iWinningNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonWinningNumbers += 500;
                                    else
                                        iTotalDollarsWonWinningNumbers += 100;
                                }
                                else if (bWinningNumbersPBRight == true && iWinningNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonWinningNumbers += 200;
                                    else
                                        iTotalDollarsWonWinningNumbers += 100;
                                }
                                else if (bWinningNumbersPBRight == false && iWinningNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonWinningNumbers += 10;
                                    else
                                        iTotalDollarsWonWinningNumbers += 7;
                                }
                                else if (bWinningNumbersPBRight == true && iWinningNumbersRight == 2)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonWinningNumbers += 10;
                                    else
                                        iTotalDollarsWonWinningNumbers += 7;
                                }
                                else if (bWinningNumbersPBRight == true && iWinningNumbersRight == 1)
                                    iTotalDollarsWonWinningNumbers += 4;
                                else if (bWinningNumbersPBRight == true)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonWinningNumbers += 2;
                                    else
                                        iTotalDollarsWonWinningNumbers += 4;
                                }
                                //weekly
                                if (bWeeklyWinningNumbersPBRight == true && iWeeklyWinningNumbersRight == 5)
                                    iWeeklyTotalDollarsWonWinningNumbers = int.MaxValue;  //jackpot!!
                                else if (bWeeklyWinningNumbersPBRight == false && iWeeklyWinningNumbersRight == 5)
                                    iWeeklyTotalDollarsWonWinningNumbers += 1000000;  //won a million!
                                else if (bWeeklyWinningNumbersPBRight == true && iWeeklyWinningNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonWinningNumbers += 10000;  //won 10,000!
                                    else
                                        iWeeklyTotalDollarsWonWinningNumbers += 50000;  //won 50,000!
                                }
                                else if (bWeeklyWinningNumbersPBRight == false && iWeeklyWinningNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonWinningNumbers += 500;
                                    else
                                        iWeeklyTotalDollarsWonWinningNumbers += 100;
                                }
                                else if (bWeeklyWinningNumbersPBRight == true && iWeeklyWinningNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonWinningNumbers += 200;
                                    else
                                        iWeeklyTotalDollarsWonWinningNumbers += 100;
                                }
                                else if (bWeeklyWinningNumbersPBRight == false && iWeeklyWinningNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonWinningNumbers += 10;
                                    else
                                        iWeeklyTotalDollarsWonWinningNumbers += 7;
                                }
                                else if (bWeeklyWinningNumbersPBRight == true && iWeeklyWinningNumbersRight == 2)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonWinningNumbers += 10;
                                    else
                                        iWeeklyTotalDollarsWonWinningNumbers += 7;
                                }
                                else if (bWeeklyWinningNumbersPBRight == true && iWeeklyWinningNumbersRight == 1)
                                    iWeeklyTotalDollarsWonWinningNumbers += 4;
                                else if (bWeeklyWinningNumbersPBRight == true)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonWinningNumbers += 2;
                                    else
                                        iWeeklyTotalDollarsWonWinningNumbers += 4;
                                }

                                if (bOverdueNumbersPBRight == true && iOverdueNumbersRight == 5)
                                    iTotalDollarsWonOverdueNumbers = int.MaxValue;  //jackpot!!
                                else if (bOverdueNumbersPBRight == false && iOverdueNumbersRight == 5)
                                    iTotalDollarsWonOverdueNumbers += 1000000;  //won a million!
                                else if (bOverdueNumbersPBRight == true && iOverdueNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonOverdueNumbers += 10000;  //won 10,000!
                                    else
                                        iTotalDollarsWonOverdueNumbers += 50000;  //won 50,000!
                                }
                                else if (bOverdueNumbersPBRight == false && iOverdueNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonOverdueNumbers += 500;
                                    else
                                        iTotalDollarsWonOverdueNumbers += 100;
                                }
                                else if (bOverdueNumbersPBRight == true && iOverdueNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonOverdueNumbers += 200;
                                    else
                                        iTotalDollarsWonOverdueNumbers += 100;
                                }
                                else if (bOverdueNumbersPBRight == false && iOverdueNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonOverdueNumbers += 10;
                                    else
                                        iTotalDollarsWonOverdueNumbers += 7;
                                }
                                else if (bOverdueNumbersPBRight == true && iOverdueNumbersRight == 2)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonOverdueNumbers += 10;
                                    else
                                        iTotalDollarsWonOverdueNumbers += 7;
                                }
                                else if (bOverdueNumbersPBRight == true && iOverdueNumbersRight == 1)
                                    iTotalDollarsWonOverdueNumbers += 4;
                                else if (bOverdueNumbersPBRight == true)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonOverdueNumbers += 2;
                                    else
                                        iTotalDollarsWonOverdueNumbers += 4;
                                }

                                //weekly
                                if (bWeeklyOverdueNumbersPBRight == true && iWeeklyOverdueNumbersRight == 5)
                                    iWeeklyTotalDollarsWonOverdueNumbers = int.MaxValue;  //jackpot!!
                                else if (bWeeklyOverdueNumbersPBRight == false && iWeeklyOverdueNumbersRight == 5)
                                    iWeeklyTotalDollarsWonOverdueNumbers += 1000000;  //won a million!
                                else if (bWeeklyOverdueNumbersPBRight == true && iWeeklyOverdueNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonOverdueNumbers += 10000;  //won 10,000!
                                    else
                                        iWeeklyTotalDollarsWonOverdueNumbers += 50000;  //won 50,000!
                                }
                                else if (bWeeklyOverdueNumbersPBRight == false && iWeeklyOverdueNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonOverdueNumbers += 500;
                                    else
                                        iWeeklyTotalDollarsWonOverdueNumbers += 100;
                                }
                                else if (bWeeklyOverdueNumbersPBRight == true && iWeeklyOverdueNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonOverdueNumbers += 200;
                                    else
                                        iWeeklyTotalDollarsWonOverdueNumbers += 100;
                                }
                                else if (bWeeklyOverdueNumbersPBRight == false && iWeeklyOverdueNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonOverdueNumbers += 10;
                                    else
                                        iWeeklyTotalDollarsWonOverdueNumbers += 7;
                                }
                                else if (bWeeklyOverdueNumbersPBRight == true && iWeeklyOverdueNumbersRight == 2)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonOverdueNumbers += 10;
                                    else
                                        iWeeklyTotalDollarsWonOverdueNumbers += 7;
                                }
                                else if (bWeeklyOverdueNumbersPBRight == true && iWeeklyOverdueNumbersRight == 1)
                                    iWeeklyTotalDollarsWonOverdueNumbers += 4;
                                else if (bWeeklyOverdueNumbersPBRight == true)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonOverdueNumbers += 2;
                                    else
                                        iWeeklyTotalDollarsWonOverdueNumbers += 4;
                                }

                                if (bScoredNumbersPBRight == true && iScoredNumbersRight == 5)
                                    iTotalDollarsWonScoredNumbers = int.MaxValue;  //jackpot!!
                                else if (bScoredNumbersPBRight == false && iScoredNumbersRight == 5)
                                    iTotalDollarsWonScoredNumbers += 1000000;  //won a million!
                                else if (bScoredNumbersPBRight == true && iScoredNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonScoredNumbers += 10000;  //won 10,000!
                                    else
                                        iTotalDollarsWonScoredNumbers += 50000;  //won 50,000!
                                }
                                else if (bScoredNumbersPBRight == false && iScoredNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonScoredNumbers += 500;
                                    else
                                        iTotalDollarsWonScoredNumbers += 100;
                                }
                                else if (bScoredNumbersPBRight == true && iScoredNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonScoredNumbers += 200;
                                    else
                                        iTotalDollarsWonScoredNumbers += 100;
                                }
                                else if (bScoredNumbersPBRight == false && iScoredNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonScoredNumbers += 10;
                                    else
                                        iTotalDollarsWonScoredNumbers += 7;
                                }
                                else if (bScoredNumbersPBRight == true && iScoredNumbersRight == 2)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonScoredNumbers += 10;
                                    else
                                        iTotalDollarsWonScoredNumbers += 7;
                                }
                                else if (bScoredNumbersPBRight == true && iScoredNumbersRight == 1)
                                    iTotalDollarsWonScoredNumbers += 4;
                                else if (bScoredNumbersPBRight == true)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonScoredNumbers += 2;
                                    else
                                        iTotalDollarsWonScoredNumbers += 4;
                                }

                                //weekly
                                if (bWeeklyScoredNumbersPBRight == true && iWeeklyScoredNumbersRight == 5)
                                    iWeeklyTotalDollarsWonScoredNumbers = int.MaxValue;  //jackpot!!
                                else if (bWeeklyScoredNumbersPBRight == false && iWeeklyScoredNumbersRight == 5)
                                    iWeeklyTotalDollarsWonScoredNumbers += 1000000;  //won a million!
                                else if (bWeeklyScoredNumbersPBRight == true && iWeeklyScoredNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonScoredNumbers += 10000;  //won 10,000!
                                    else
                                        iWeeklyTotalDollarsWonScoredNumbers += 50000;  //won 50,000!
                                }
                                else if (bWeeklyScoredNumbersPBRight == false && iWeeklyScoredNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonScoredNumbers += 500;
                                    else
                                        iWeeklyTotalDollarsWonScoredNumbers += 100;
                                }
                                else if (bWeeklyScoredNumbersPBRight == true && iWeeklyScoredNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonScoredNumbers += 200;
                                    else
                                        iWeeklyTotalDollarsWonScoredNumbers += 100;
                                }
                                else if (bWeeklyScoredNumbersPBRight == false && iWeeklyScoredNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonScoredNumbers += 10;
                                    else
                                        iWeeklyTotalDollarsWonScoredNumbers += 7;
                                }
                                else if (bWeeklyScoredNumbersPBRight == true && iWeeklyScoredNumbersRight == 2)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonScoredNumbers += 10;
                                    else
                                        iWeeklyTotalDollarsWonScoredNumbers += 7;
                                }
                                else if (bWeeklyScoredNumbersPBRight == true && iWeeklyScoredNumbersRight == 1)
                                    iWeeklyTotalDollarsWonScoredNumbers += 4;
                                else if (bWeeklyScoredNumbersPBRight == true)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonScoredNumbers += 2;
                                    else
                                        iWeeklyTotalDollarsWonScoredNumbers += 4;
                                }

                                if (bAlgorithmNumbersPBRight == true && iAlgorithmNumbersRight == 5)
                                    iTotalDollarsWonAlgorithmNumbers = int.MaxValue;  //jackpot!!
                                else if (bAlgorithmNumbersPBRight == false && iAlgorithmNumbersRight == 5)
                                    iTotalDollarsWonAlgorithmNumbers += 1000000;  //won a million!
                                else if (bAlgorithmNumbersPBRight == true && iAlgorithmNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonAlgorithmNumbers += 10000;  //won 10,000!
                                    else
                                        iTotalDollarsWonAlgorithmNumbers += 50000;  //won 50,000!
                                }
                                else if (bAlgorithmNumbersPBRight == false && iAlgorithmNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonAlgorithmNumbers += 500;
                                    else
                                        iTotalDollarsWonAlgorithmNumbers += 100;
                                }
                                else if (bAlgorithmNumbersPBRight == true && iAlgorithmNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonAlgorithmNumbers += 200;
                                    else
                                        iTotalDollarsWonAlgorithmNumbers += 100;
                                }
                                else if (bAlgorithmNumbersPBRight == false && iAlgorithmNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonAlgorithmNumbers += 10;
                                    else
                                        iTotalDollarsWonAlgorithmNumbers += 7;
                                }
                                else if (bAlgorithmNumbersPBRight == true && iAlgorithmNumbersRight == 2)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonAlgorithmNumbers += 10;
                                    else
                                        iTotalDollarsWonAlgorithmNumbers += 7;
                                }
                                else if (bAlgorithmNumbersPBRight == true && iAlgorithmNumbersRight == 1)
                                    iTotalDollarsWonAlgorithmNumbers += 4;
                                else if (bAlgorithmNumbersPBRight == true)
                                {
                                    if (strGName == "MM")
                                        iTotalDollarsWonAlgorithmNumbers += 2;
                                    else
                                        iTotalDollarsWonAlgorithmNumbers += 4;
                                }

                                //weekly
                                if (bWeeklyAlgorithmNumbersPBRight == true && iWeeklyAlgorithmNumbersRight == 5)
                                    iWeeklyTotalDollarsWonAlgorithmNumbers = int.MaxValue;  //jackpot!!
                                else if (bWeeklyAlgorithmNumbersPBRight == false && iWeeklyAlgorithmNumbersRight == 5)
                                    iWeeklyTotalDollarsWonAlgorithmNumbers += 1000000;  //won a million!
                                else if (bWeeklyAlgorithmNumbersPBRight == true && iWeeklyAlgorithmNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 10000;  //won 10,000!
                                    else
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 50000;  //won 50,000!
                                }
                                else if (bWeeklyAlgorithmNumbersPBRight == false && iWeeklyAlgorithmNumbersRight == 4)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 500;
                                    else
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 100;
                                }
                                else if (bWeeklyAlgorithmNumbersPBRight == true && iWeeklyAlgorithmNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 200;
                                    else
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 100;
                                }
                                else if (bWeeklyAlgorithmNumbersPBRight == false && iWeeklyAlgorithmNumbersRight == 3)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 10;
                                    else
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 7;
                                }
                                else if (bWeeklyAlgorithmNumbersPBRight == true && iWeeklyAlgorithmNumbersRight == 2)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 10;
                                    else
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 7;
                                }
                                else if (bWeeklyAlgorithmNumbersPBRight == true && iWeeklyAlgorithmNumbersRight == 1)
                                    iWeeklyTotalDollarsWonAlgorithmNumbers += 4;
                                else if (bWeeklyAlgorithmNumbersPBRight == true)
                                {
                                    if (strGName == "MM")
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 2;
                                    else
                                        iWeeklyTotalDollarsWonAlgorithmNumbers += 4;
                                }

                            }
                            else if (strGName == "MB")
                            {
                                if (iWinningNumbersRight == 6)
                                    iTotalDollarsWonWinningNumbers = int.MaxValue;  //jackpot!!
                                else if (iWinningNumbersRight == 5)
                                    iTotalDollarsWonWinningNumbers += 500;
                                else if (iWinningNumbersRight == 4)
                                    iTotalDollarsWonWinningNumbers += 30;
                                else if (iWinningNumbersRight == 3)
                                    iTotalDollarsWonWinningNumbers += 2;
                                //weekly
                                if (iWeeklyWinningNumbersRight == 6)
                                    iWeeklyTotalDollarsWonWinningNumbers = int.MaxValue;  //jackpot!!
                                else if (iWeeklyWinningNumbersRight == 5)
                                    iWeeklyTotalDollarsWonWinningNumbers += 500;
                                else if (iWeeklyWinningNumbersRight == 4)
                                    iWeeklyTotalDollarsWonWinningNumbers += 30;
                                else if (iWeeklyWinningNumbersRight == 3)
                                    iWeeklyTotalDollarsWonWinningNumbers += 2;

                                if (iOverdueNumbersRight == 6)
                                    iTotalDollarsWonOverdueNumbers = int.MaxValue;  //jackpot!!
                                else if (iOverdueNumbersRight == 5)
                                    iTotalDollarsWonOverdueNumbers += 500;
                                else if (iOverdueNumbersRight == 4)
                                    iTotalDollarsWonOverdueNumbers += 30;
                                else if (iOverdueNumbersRight == 3)
                                    iTotalDollarsWonOverdueNumbers += 2;
                                //weekly
                                if (iWeeklyOverdueNumbersRight == 6)
                                    iWeeklyTotalDollarsWonOverdueNumbers = int.MaxValue;  //jackpot!!
                                else if (iWeeklyOverdueNumbersRight == 5)
                                    iWeeklyTotalDollarsWonOverdueNumbers += 500;
                                else if (iWeeklyOverdueNumbersRight == 4)
                                    iWeeklyTotalDollarsWonOverdueNumbers += 30;
                                else if (iWeeklyOverdueNumbersRight == 3)
                                    iWeeklyTotalDollarsWonOverdueNumbers += 2;

                                if (iScoredNumbersRight == 6)
                                    iTotalDollarsWonScoredNumbers = int.MaxValue;  //jackpot!!
                                else if (iScoredNumbersRight == 5)
                                    iTotalDollarsWonScoredNumbers += 500;
                                else if (iScoredNumbersRight == 4)
                                    iTotalDollarsWonScoredNumbers += 30;
                                else if (iScoredNumbersRight == 3)
                                    iTotalDollarsWonScoredNumbers += 2;
                                //weekly
                                if (iWeeklyScoredNumbersRight == 6)
                                    iWeeklyTotalDollarsWonScoredNumbers = int.MaxValue;  //jackpot!!
                                else if (iWeeklyScoredNumbersRight == 5)
                                    iWeeklyTotalDollarsWonScoredNumbers += 500;
                                else if (iWeeklyScoredNumbersRight == 4)
                                    iWeeklyTotalDollarsWonScoredNumbers += 30;
                                else if (iWeeklyScoredNumbersRight == 3)
                                    iWeeklyTotalDollarsWonScoredNumbers += 2;

                                if (iAlgorithmNumbersRight == 6)
                                    iTotalDollarsWonAlgorithmNumbers = int.MaxValue;  //jackpot!!
                                else if (iAlgorithmNumbersRight == 5)
                                    iTotalDollarsWonAlgorithmNumbers += 500;
                                else if (iAlgorithmNumbersRight == 4)
                                    iTotalDollarsWonAlgorithmNumbers += 30;
                                else if (iAlgorithmNumbersRight == 3)
                                    iTotalDollarsWonAlgorithmNumbers += 2;
                                //weekly
                                if (iWeeklyAlgorithmNumbersRight == 6)
                                    iWeeklyTotalDollarsWonAlgorithmNumbers = int.MaxValue;  //jackpot!!
                                else if (iWeeklyAlgorithmNumbersRight == 5)
                                    iWeeklyTotalDollarsWonAlgorithmNumbers += 500;
                                else if (iWeeklyAlgorithmNumbersRight == 4)
                                    iWeeklyTotalDollarsWonAlgorithmNumbers += 30;
                                else if (iWeeklyAlgorithmNumbersRight == 3)
                                    iWeeklyTotalDollarsWonAlgorithmNumbers += 2;
                            }
                            else if (strGName == "B5")
                            {
                                if (iWinningNumbersRight == 5)
                                    iTotalDollarsWonWinningNumbers = int.MaxValue;  //jackpot!!
                                else if (iWinningNumbersRight == 4)
                                    iTotalDollarsWonWinningNumbers += 50;
                                else if (iWinningNumbersRight == 3)
                                    iTotalDollarsWonWinningNumbers += 2;
                                else if (iWinningNumbersRight == 2)
                                    iTotalDollarsWonWinningNumbers += 1;
                                //weekly
                                if (iWeeklyWinningNumbersRight == 5)
                                    iWeeklyTotalDollarsWonWinningNumbers = int.MaxValue;  //jackpot!!
                                else if (iWeeklyWinningNumbersRight == 4)
                                    iWeeklyTotalDollarsWonWinningNumbers += 50;
                                else if (iWeeklyWinningNumbersRight == 3)
                                    iWeeklyTotalDollarsWonWinningNumbers += 2;
                                else if (iWeeklyWinningNumbersRight == 2)
                                    iWeeklyTotalDollarsWonWinningNumbers += 1;


                                if (iOverdueNumbersRight == 5)
                                    iTotalDollarsWonOverdueNumbers = int.MaxValue;  //jackpot!!
                                else if (iOverdueNumbersRight == 4)
                                    iTotalDollarsWonOverdueNumbers += 50;
                                else if (iOverdueNumbersRight == 3)
                                    iTotalDollarsWonOverdueNumbers += 2;
                                else if (iOverdueNumbersRight == 2)
                                    iTotalDollarsWonOverdueNumbers += 1;
                                //weekly
                                if (iWeeklyOverdueNumbersRight == 5)
                                    iWeeklyTotalDollarsWonOverdueNumbers = int.MaxValue;  //jackpot!!
                                else if (iWeeklyOverdueNumbersRight == 4)
                                    iWeeklyTotalDollarsWonOverdueNumbers += 50;
                                else if (iWeeklyOverdueNumbersRight == 3)
                                    iWeeklyTotalDollarsWonOverdueNumbers += 2;
                                else if (iWeeklyOverdueNumbersRight == 2)
                                    iWeeklyTotalDollarsWonOverdueNumbers += 1;


                                if (iScoredNumbersRight == 5)
                                    iTotalDollarsWonScoredNumbers = int.MaxValue;  //jackpot!!
                                else if (iScoredNumbersRight == 4)
                                    iTotalDollarsWonScoredNumbers += 50;
                                else if (iScoredNumbersRight == 3)
                                    iTotalDollarsWonScoredNumbers += 2;
                                else if (iScoredNumbersRight == 2)
                                    iTotalDollarsWonScoredNumbers += 1;
                                //weekly
                                if (iWeeklyScoredNumbersRight == 5)
                                    iWeeklyTotalDollarsWonScoredNumbers = int.MaxValue;  //jackpot!!
                                else if (iWeeklyScoredNumbersRight == 4)
                                    iWeeklyTotalDollarsWonScoredNumbers += 50;
                                else if (iWeeklyScoredNumbersRight == 3)
                                    iWeeklyTotalDollarsWonScoredNumbers += 2;
                                else if (iWeeklyScoredNumbersRight == 2)
                                    iWeeklyTotalDollarsWonScoredNumbers += 1;

                                if (iAlgorithmNumbersRight == 5)
                                    iTotalDollarsWonAlgorithmNumbers = int.MaxValue;  //jackpot!!
                                else if (iAlgorithmNumbersRight == 4)
                                    iTotalDollarsWonAlgorithmNumbers += 50;
                                else if (iAlgorithmNumbersRight == 3)
                                    iTotalDollarsWonAlgorithmNumbers += 2;
                                else if (iAlgorithmNumbersRight == 2)
                                    iTotalDollarsWonAlgorithmNumbers += 1;
                                //weekly
                                if (iWeeklyAlgorithmNumbersRight == 5)
                                    iWeeklyTotalDollarsWonAlgorithmNumbers = int.MaxValue;  //jackpot!!
                                else if (iWeeklyAlgorithmNumbersRight == 4)
                                    iWeeklyTotalDollarsWonAlgorithmNumbers += 50;
                                else if (iWeeklyAlgorithmNumbersRight == 3)
                                    iWeeklyTotalDollarsWonAlgorithmNumbers += 2;
                                else if (iWeeklyAlgorithmNumbersRight == 2)
                                    iWeeklyTotalDollarsWonAlgorithmNumbers += 1;
                            }
                            if (strGName == "B5" || strGName == "MB")
                            {
                                txtB5DollarsWonWinningNumbers.Text = iTotalDollarsWonWinningNumbers.ToString("C0");
                                txtB5DollarsWonOverdueNumbers.Text = iTotalDollarsWonOverdueNumbers.ToString("C0");
                                txtB5DollarsWonScoredNumbers.Text = iTotalDollarsWonScoredNumbers.ToString("C0");
                                txtB5DollarsWonAlgorithmNumbers.Text = iTotalDollarsWonAlgorithmNumbers.ToString("C0");

                                txtWeeklyB5DollarsWonWinningNumbers.Text = iWeeklyTotalDollarsWonWinningNumbers.ToString("C0");
                                txtWeeklyB5DollarsWonOverdueNumbers.Text = iWeeklyTotalDollarsWonOverdueNumbers.ToString("C0");
                                txtWeeklyB5DollarsWonScoredNumbers.Text = iWeeklyTotalDollarsWonScoredNumbers.ToString("C0");
                                txtWeeklyB5DollarsWonAlgorithmNumbers.Text = iWeeklyTotalDollarsWonAlgorithmNumbers.ToString("C0");
                            }
                            else
                            {
                                txtPBDollarsWonWinningNumbers.Text = iTotalDollarsWonWinningNumbers.ToString("C0");
                                txtPBDollarsWonOverdueNumbers.Text = iTotalDollarsWonOverdueNumbers.ToString("C0");
                                txtPBDollarsWonScoredNumbers.Text = iTotalDollarsWonScoredNumbers.ToString("C0");
                                txtPBDollarsWonAlgorithmNumbers.Text = iTotalDollarsWonAlgorithmNumbers.ToString("C0");

                                txtPBWeeklyDollarsWonWinningNumbers.Text = iWeeklyTotalDollarsWonWinningNumbers.ToString("C0");
                                txtPBWeeklyDollarsWonOverdueNumbers.Text = iWeeklyTotalDollarsWonOverdueNumbers.ToString("C0");
                                txtPBWeeklyDollarsWonScoredNumbers.Text = iWeeklyTotalDollarsWonScoredNumbers.ToString("C0");
                                txtPBWeeklyDollarsWonAlgorithmNumbers.Text = iWeeklyTotalDollarsWonAlgorithmNumbers.ToString("C0");
                            }

                            iB5TotalPlays++;
                            iWeeklyRepeatCountForWinningNumbers++;
                        }//end of if (we got the actual winning numbers for this draw date)
                    }//end of if (computer calculated winning numbers)

                    if (strGName == "B5")
                    {
                        dtDrawDate = dtDrawDate.AddDays(1);
                        dtDrawDate = new DateTime(dtDrawDate.Year, dtDrawDate.Month, dtDrawDate.Day);  //just to get rid of the time
                    }
                    else //assume it is PB or MM or MB which are not daily draws  if (strGName == "PB" || strGName == "MM")
                    {
                        strSQL = "SELECT DDate FROM dbo.Temp_RetireData ";
                        strSQL += " WHERE GName = :GName AND DDate > :DDate ";
                        strSQL += " ORDER BY DDate ";
                        parms.Clear();
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":GName", DbType.String, strGName));
                        parms.Add(m_objRxVectorDatabase.CreateInParameter(":DDate", DbType.DateTime, dtDrawDate));
                        var PBResults3 = from myRow in m_dtRetireData.AsEnumerable()
                                        .OrderBy(r => r.Field<DateTime>("DDate"))
                                        where myRow.Field<string>("GName") == strGName &&
                                              myRow.Field<DateTime>("DDate") > dtDrawDate
                                        select myRow;
                        DataTable dt2 = PBResults3.Any() ? PBResults3.CopyToDataTable() : null;
                        //DataTable dt2 = m_objRxVectorDatabase.ExecuteDataTable_InlineSql(strSQL, parms);
                        if (dt2 == null || dt2.Rows.Count < 1)
                        {
                            dtDrawDate = dtMaxDate.AddDays(1);  //hit the end of available data
                            dtDrawDate = new DateTime(dtDrawDate.Year, dtDrawDate.Month, dtDrawDate.Day);  //just to get rid of the time
                        }
                        else
                        {
                            dtDrawDate = Convert.ToDateTime(dt2.Rows[0]["DDate"]);
                            dtDrawDate = new DateTime(dtDrawDate.Year, dtDrawDate.Month, dtDrawDate.Day);  //just to get rid of the time
                        }
                    }

                }//end of while to go through each possible draw day

                if (strGName == "B5")
                {
                    txtB5TotalPlays.Text = iB5TotalPlays.ToString();
                    lblB5WinningNumberAlgorithmAnalysisResults.Text = "0 " + iTotalTimesWinningNums0Right.ToString() + "x, 1 " + iTotalTimesWinningNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesWinningNumsAtLeast2Right.ToString() + "x, 3 " + iTotalTimesWinningNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesWinningNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesWinningNumsAtLeast5Right.ToString() + "x";
                    lblB5OverdueNumbersAlgorithmAnalysis.Text = "0 " + iTotalTimesOverdueNums0Right.ToString() + "x, 1 " + iTotalTimesOverdueNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesOverdueNumsAtLeast2Right + "x, 3 " + iTotalTimesOverdueNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesOverdueNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesOverdueNumsAtLeast5Right.ToString() + "x";
                    lblB5PastScoreNumbersAlgorithmAnalysis.Text = "0 " + iTotalTimesScoredNums0Right.ToString() + "x, 1 " + iTotalTimesScoredNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesScoredNumsAtLeast2Right.ToString() + "x, 3 " + iTotalTimesScoredNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesScoredNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesScoredNumsAtLeast5Right.ToString() + "x";
                    lblB5AlgorithmicAnalysis.Text = "0 " + iTotalTimesAlgorithmNums0Right.ToString() + "x, 1 " + iTotalTimesAlgorithmNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesAlgorithmNumsAtLeast2Right.ToString() + "x, 3 " + iTotalTimesAlgorithmNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesAlgorithmNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesAlgorithmNumsAtLeast5Right.ToString() + "x";

                    lblWeeklyB5WinningNumberAlgorithmAnalysisResults.Text = "0 " + iWeeklyTotalTimesWinningNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesWinningNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesWinningNumsAtLeast2Right.ToString() + "x, 3 " + iWeeklyTotalTimesWinningNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesWinningNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesWinningNumsAtLeast5Right.ToString() + "x";
                    lblWeeklyB5OverdueNumberAlgorithmAnalysisResults.Text = "0 " + iWeeklyTotalTimesOverdueNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesOverdueNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesOverdueNumsAtLeast2Right + "x, 3 " + iWeeklyTotalTimesOverdueNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesOverdueNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesOverdueNumsAtLeast5Right.ToString() + "x";
                    lblWeeklyB5PastScoreNumbersAlgorithmAnalysis.Text = "0 " + iWeeklyTotalTimesScoredNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesScoredNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesScoredNumsAtLeast2Right.ToString() + "x, 3 " + iWeeklyTotalTimesScoredNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesScoredNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesScoredNumsAtLeast5Right.ToString() + "x";
                    lblWeeklyB5AlgorithmicAnalysis.Text = "0 " + iWeeklyTotalTimesAlgorithmNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesAlgorithmNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesAlgorithmNumsAtLeast2Right.ToString() + "x, 3 " + iWeeklyTotalTimesAlgorithmNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesAlgorithmNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesAlgorithmNumsAtLeast5Right.ToString() + "x";

                    lblB5WinningNumberPositionAlgorithmAnalysisResults.Text = "1st " + iWinningNumbersPos1Right.ToString() + "x, 2nd " + iWinningNumbersPos2Right.ToString() + "x, 3rd " + iWinningNumbersPos3Right.ToString() + "x, 4th " + iWinningNumbersPos4Right.ToString() + "x, 5th " + iWinningNumbersPos5Right.ToString() + "x";
                    lblB5OverdueNumbersPositionAlgorithmAnalysis.Text = "1st " + iOverdueNumbersPos1Right.ToString() + "x, 2nd " + iOverdueNumbersPos2Right.ToString() + "x, 3rd " + iOverdueNumbersPos3Right.ToString() + "x, 4th " + iOverdueNumbersPos4Right.ToString() + "x, 5th " + iOverdueNumbersPos5Right.ToString() + "x";
                    lblB5PastScoreNumbersPositionAlgorithmAnalysis.Text = "1st " + iScoredNumbersPos1Right.ToString() + "x, 2nd " + iScoredNumbersPos2Right.ToString() + "x, 3rd " + iScoredNumbersPos3Right.ToString() + "x, 4th " + iScoredNumbersPos4Right.ToString() + "x, 5th " + iScoredNumbersPos5Right.ToString() + "x";
                    lblB5AlgorithmicPositionAnalysis.Text = "1st " + iAlgorithmNumbersPos1Right.ToString() + "x, 2nd " + iAlgorithmNumbersPos2Right.ToString() + "x, 3rd " + iAlgorithmNumbersPos3Right.ToString() + "x, 4th " + iAlgorithmNumbersPos4Right.ToString() + "x, 5th " + iAlgorithmNumbersPos5Right.ToString() + "x";
                }
                else if (strGName == "MB")
                {
                    txtB5TotalPlays.Text = iB5TotalPlays.ToString();
                    lblB5WinningNumberAlgorithmAnalysisResults.Text = "0 " + iTotalTimesWinningNums0Right.ToString() + "x, 1 " + iTotalTimesWinningNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesWinningNumsAtLeast2Right.ToString() + "x, 3 " + iTotalTimesWinningNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesWinningNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesWinningNumsAtLeast5Right.ToString() + "x, 6 " + iTotalTimesWinningNumsAtLeast6Right.ToString() + "x";
                    lblB5OverdueNumbersAlgorithmAnalysis.Text = "0 " + iTotalTimesOverdueNums0Right.ToString() + "x, 1 " + iTotalTimesOverdueNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesOverdueNumsAtLeast2Right + "x, 3 " + iTotalTimesOverdueNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesOverdueNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesOverdueNumsAtLeast5Right.ToString() + "x, 6 " + iTotalTimesOverdueNumsAtLeast6Right.ToString() + "x";
                    lblB5PastScoreNumbersAlgorithmAnalysis.Text = "0 " + iTotalTimesScoredNums0Right.ToString() + "x, 1 " + iTotalTimesScoredNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesScoredNumsAtLeast2Right.ToString() + "x, 3 " + iTotalTimesScoredNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesScoredNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesScoredNumsAtLeast5Right.ToString() + "x, 6 " + iTotalTimesScoredNumsAtLeast6Right.ToString() + "x";
                    lblB5AlgorithmicAnalysis.Text = "0 " + iTotalTimesAlgorithmNums0Right.ToString() + "x, 1 " + iTotalTimesAlgorithmNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesAlgorithmNumsAtLeast2Right.ToString() + "x, 3 " + iTotalTimesAlgorithmNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesAlgorithmNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesAlgorithmNumsAtLeast5Right.ToString() + "x, 6 " + iTotalTimesAlgorithmNumsAtLeast6Right.ToString() + "x";

                    lblWeeklyB5WinningNumberAlgorithmAnalysisResults.Text = "0 " + iWeeklyTotalTimesWinningNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesWinningNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesWinningNumsAtLeast2Right.ToString() + "x, 3 " + iWeeklyTotalTimesWinningNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesWinningNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesWinningNumsAtLeast5Right.ToString() + "x, 6 " + iWeeklyTotalTimesWinningNumsAtLeast6Right.ToString() + "x";
                    lblWeeklyB5OverdueNumberAlgorithmAnalysisResults.Text = "0 " + iWeeklyTotalTimesOverdueNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesOverdueNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesOverdueNumsAtLeast2Right + "x, 3 " + iWeeklyTotalTimesOverdueNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesOverdueNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesOverdueNumsAtLeast5Right.ToString() + "x, 6 " + iWeeklyTotalTimesOverdueNumsAtLeast6Right.ToString() + "x";
                    lblWeeklyB5PastScoreNumbersAlgorithmAnalysis.Text = "0 " + iWeeklyTotalTimesScoredNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesScoredNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesScoredNumsAtLeast2Right.ToString() + "x, 3 " + iWeeklyTotalTimesScoredNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesScoredNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesScoredNumsAtLeast5Right.ToString() + "x, 6 " + iWeeklyTotalTimesScoredNumsAtLeast6Right.ToString() + "x";
                    lblWeeklyB5AlgorithmicAnalysis.Text = "0 " + iWeeklyTotalTimesAlgorithmNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesAlgorithmNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesAlgorithmNumsAtLeast2Right.ToString() + "x, 3 " + iWeeklyTotalTimesAlgorithmNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesAlgorithmNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesAlgorithmNumsAtLeast5Right.ToString() + "x, 6 " + iWeeklyTotalTimesAlgorithmNumsAtLeast6Right.ToString() + "x";

                    lblB5WinningNumberPositionAlgorithmAnalysisResults.Text = "1st " + iWinningNumbersPos1Right.ToString() + "x, 2nd " + iWinningNumbersPos2Right.ToString() + "x, 3rd " + iWinningNumbersPos3Right.ToString() + "x, 4th " + iWinningNumbersPos4Right.ToString() + "x, 5th " + iWinningNumbersPos5Right.ToString() + "x, 6th " + iWinningNumbersPos6Right.ToString() + "x";
                    lblB5OverdueNumbersPositionAlgorithmAnalysis.Text = "1st " + iOverdueNumbersPos1Right.ToString() + "x, 2nd " + iOverdueNumbersPos2Right.ToString() + "x, 3rd " + iOverdueNumbersPos3Right.ToString() + "x, 4th " + iOverdueNumbersPos4Right.ToString() + "x, 5th " + iOverdueNumbersPos5Right.ToString() + "x, 6th " + iOverdueNumbersPos6Right.ToString() + "x";
                    lblB5PastScoreNumbersPositionAlgorithmAnalysis.Text = "1st " + iScoredNumbersPos1Right.ToString() + "x, 2nd " + iScoredNumbersPos2Right.ToString() + "x, 3rd " + iScoredNumbersPos3Right.ToString() + "x, 4th " + iScoredNumbersPos4Right.ToString() + "x, 5th " + iScoredNumbersPos5Right.ToString() + "x, 6th " + iScoredNumbersPos6Right.ToString() + "x";
                    lblB5AlgorithmicPositionAnalysis.Text = "1st " + iAlgorithmNumbersPos1Right.ToString() + "x, 2nd " + iAlgorithmNumbersPos2Right.ToString() + "x, 3rd " + iAlgorithmNumbersPos3Right.ToString() + "x, 4th " + iAlgorithmNumbersPos4Right.ToString() + "x, 5th " + iAlgorithmNumbersPos5Right.ToString() + "x, 6th " + iAlgorithmNumbersPos6Right.ToString() + "x";
                }
                else //PB or MM
                {
                    txtPBTotalPlays.Text = iB5TotalPlays.ToString();
                    lblPBWinningNumberAlgorithmAnalysisResults.Text = "0 " + iTotalTimesWinningNums0Right.ToString() + "x, 1 " + iTotalTimesWinningNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesWinningNumsAtLeast2Right.ToString() + "x, 3 " + iTotalTimesWinningNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesWinningNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesWinningNumsAtLeast5Right.ToString() + "x";
                    lblPBOverdueNumbersAlgorithmAnalysis.Text = "0 " + iTotalTimesOverdueNums0Right.ToString() + "x, 1 " + iTotalTimesOverdueNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesOverdueNumsAtLeast2Right + "x, 3 " + iTotalTimesOverdueNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesOverdueNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesOverdueNumsAtLeast5Right.ToString() + "x";
                    lblPBPastScoreNumbersAlgorithmAnalysis.Text = "0 " + iTotalTimesScoredNums0Right.ToString() + "x, 1 " + iTotalTimesScoredNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesScoredNumsAtLeast2Right.ToString() + "x, 3 " + iTotalTimesScoredNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesScoredNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesScoredNumsAtLeast5Right.ToString() + "x";
                    lblPBAlgorithmicAnalysis.Text = "0 " + iTotalTimesAlgorithmNums0Right.ToString() + "x, 1 " + iTotalTimesAlgorithmNumsAtLeast1Right.ToString() + "x, 2 " + iTotalTimesAlgorithmNumsAtLeast2Right.ToString() + "x, 3 " + iTotalTimesAlgorithmNumsAtLeast3Right.ToString() + "x, 4 " + iTotalTimesAlgorithmNumsAtLeast4Right.ToString() + "x, 5 " + iTotalTimesAlgorithmNumsAtLeast5Right.ToString() + "x";

                    lblPBWeeklyWinningNumberAlgorithmAnalysisResults.Text = "0 " + iWeeklyTotalTimesWinningNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesWinningNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesWinningNumsAtLeast2Right.ToString() + "x, 3 " + iWeeklyTotalTimesWinningNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesWinningNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesWinningNumsAtLeast5Right.ToString() + "x";
                    lblPBWeeklyOverdueNumbersAlgorithmAnalysis.Text = "0 " + iWeeklyTotalTimesOverdueNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesOverdueNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesOverdueNumsAtLeast2Right + "x, 3 " + iWeeklyTotalTimesOverdueNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesOverdueNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesOverdueNumsAtLeast5Right.ToString() + "x";
                    lblPBWeeklyPastScoreNumbersAlgorithmAnalysis.Text = "0 " + iWeeklyTotalTimesScoredNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesScoredNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesScoredNumsAtLeast2Right.ToString() + "x, 3 " + iWeeklyTotalTimesScoredNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesScoredNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesScoredNumsAtLeast5Right.ToString() + "x";
                    lblPBWeeklyAlgorithmicAnalysis.Text = "0 " + iWeeklyTotalTimesAlgorithmNums0Right.ToString() + "x, 1 " + iWeeklyTotalTimesAlgorithmNumsAtLeast1Right.ToString() + "x, 2 " + iWeeklyTotalTimesAlgorithmNumsAtLeast2Right.ToString() + "x, 3 " + iWeeklyTotalTimesAlgorithmNumsAtLeast3Right.ToString() + "x, 4 " + iWeeklyTotalTimesAlgorithmNumsAtLeast4Right.ToString() + "x, 5 " + iWeeklyTotalTimesAlgorithmNumsAtLeast5Right.ToString() + "x";

                    if (strGName == "PB")
                    {
                        lblPBWinningNumberPositionAlgorithmAnalysisResults.Text = "1st " + iWinningNumbersPos1Right.ToString() + "x, 2nd " + iWinningNumbersPos2Right.ToString() + "x, 3rd " + iWinningNumbersPos3Right.ToString() + "x, 4th " + iWinningNumbersPos4Right.ToString() + "x, 5th " + iWinningNumbersPos5Right.ToString() + "x,  PB " + iWinningNumbersPBRight.ToString() + "x";
                        lblPBOverdueNumbersPositionAlgorithmAnalysis.Text = "1st " + iOverdueNumbersPos1Right.ToString() + "x, 2nd " + iOverdueNumbersPos2Right.ToString() + "x, 3rd " + iOverdueNumbersPos3Right.ToString() + "x, 4th " + iOverdueNumbersPos4Right.ToString() + "x, 5th " + iOverdueNumbersPos5Right.ToString() + "x,  PB " + iOverdueNumbersPBRight.ToString() + "x";
                        lblPBPastScoreNumbersPositionAlgorithmAnalysis.Text = "1st " + iScoredNumbersPos1Right.ToString() + "x, 2nd " + iScoredNumbersPos2Right.ToString() + "x, 3rd " + iScoredNumbersPos3Right.ToString() + "x, 4th " + iScoredNumbersPos4Right.ToString() + "x, 5th " + iScoredNumbersPos5Right.ToString() + "x,  PB " + iScoredNumbersPBRight.ToString() + "x";
                    }
                    else
                    {
                        lblPBWinningNumberPositionAlgorithmAnalysisResults.Text = "1st " + iWinningNumbersPos1Right.ToString() + "x, 2nd " + iWinningNumbersPos2Right.ToString() + "x, 3rd " + iWinningNumbersPos3Right.ToString() + "x, 4th " + iWinningNumbersPos4Right.ToString() + "x, 5th " + iWinningNumbersPos5Right.ToString() + "x,  MB " + iWinningNumbersPBRight.ToString() + "x";
                        lblPBOverdueNumbersPositionAlgorithmAnalysis.Text = "1st " + iOverdueNumbersPos1Right.ToString() + "x, 2nd " + iOverdueNumbersPos2Right.ToString() + "x, 3rd " + iOverdueNumbersPos3Right.ToString() + "x, 4th " + iOverdueNumbersPos4Right.ToString() + "x, 5th " + iOverdueNumbersPos5Right.ToString() + "x,  MB " + iOverdueNumbersPBRight.ToString() + "x";
                        lblPBPastScoreNumbersPositionAlgorithmAnalysis.Text = "1st " + iScoredNumbersPos1Right.ToString() + "x, 2nd " + iScoredNumbersPos2Right.ToString() + "x, 3rd " + iScoredNumbersPos3Right.ToString() + "x, 4th " + iScoredNumbersPos4Right.ToString() + "x, 5th " + iScoredNumbersPos5Right.ToString() + "x,  MB " + iScoredNumbersPBRight.ToString() + "x";
                        lblPBAlgorithmicPositionAnalysis.Text = "1st " + iAlgorithmNumbersPos1Right.ToString() + "x, 2nd " + iAlgorithmNumbersPos2Right.ToString() + "x, 3rd " + iAlgorithmNumbersPos3Right.ToString() + "x, 4th " + iAlgorithmNumbersPos4Right.ToString() + "x, 5th " + iAlgorithmNumbersPos5Right.ToString() + "x, MB " + iAlgorithmNumbersPBRight.ToString() + "x";
                    }
                }
            }
            catch (Exception ex)
            {
                this.Cursor = saved_cursor;
                MessageBox.Show("Error while querying database and currently on draw date " + dtpDrawDate.ToString() + ".  The message from the system was: " + Environment.NewLine + "     " + ex.Message);
            }

            this.Cursor = saved_cursor;
            if (m_strPopupMessage == string.Empty)
            {
                MessageBox.Show("DAMN...the computer never did pick 4 or 5 numbers right...algorithm clearly needs more work!");
            }
            else
            {
                MessageBox.Show(m_strPopupMessage);
            }
        }

        private void btnB5DetermineBestAlgorithm_Click(object sender, EventArgs e)
        {
            string strGname = "B5";
            if (radioMegabucks.Checked == true)
                strGname = "MB";

            AnalyzeAlgorithms(strGname, sender, e);
        }

        private void btnPBDetermineBestAlgorithm_Click(object sender, EventArgs e)
        {
            string strGname = "MM";
            if (radioPowerball.Checked == true)
                strGname = "PB";

            AnalyzeAlgorithms(strGname, sender, e);
        }

        private void radioBadger5_CheckedChanged(object sender, EventArgs e)
        {
            if (radioBadger5.Checked == true)
            {
                groupBoxBadger5.Text = "Badger 5";
                this.btnGenerateBadger5.Text = "Gen Badger5";
            }
            else
            {
                groupBoxBadger5.Text = "Megabucks";
                this.btnGenerateBadger5.Text = "Gen Megabucks";
            }
        }
    }
}
