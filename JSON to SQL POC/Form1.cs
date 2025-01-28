using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JSON_to_SQL_POC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //InitializeWebView();
            //LoadComboBoxData();

        }

        //private void LoadComboBoxData()
        //{
        //    var dataSource = new List<ComboBoxItem>
        //    {
        //        new ComboBoxItem { Id = 1, Name = "COMPANY_ID" },
        //        new ComboBoxItem { Id = 2, Name = "HPCODE" },
        //        new ComboBoxItem { Id = 3, Name = "OPT" }
        //    };

        //    // Bind the data source to the ComboBox
        //    extensionColumnsComboBox.DataSource = dataSource;
        //    extensionColumnsComboBox.DisplayMember = "Name";  // Property to display
        //    extensionColumnsComboBox.ValueMember = "Id";     // Property to store as value
        //}

        private async void InitializeWebView()
        {
            await webView.EnsureCoreWebView2Async();

            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            webView.CoreWebView2.WebMessageReceived += WebView_CoreWebView2_WebMessageReceived;
        }

        //private async void Form1_Load(object sender, EventArgs e)
        //{
        //    // Ensure WebView2 initialization
        //    await webView.EnsureCoreWebView2Async();
        //}

        private async void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                // Send a message to the React app after initialization
                await webView.CoreWebView2.ExecuteScriptAsync("window.receiveMessageFromWinForms('Hello from WinForms!')");
            }
            else
            {
                MessageBox.Show("WebView2 initialization failed.");
            }
        }

        private async void btnSendMessage_Click(object sender, EventArgs e)
        {
            await webView.CoreWebView2.ExecuteScriptAsync("window.receiveMessageFromWinForms('Button clicked in WinForms!')");
        }

        private void WebView_CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // Handle the message from React
            string message = e.WebMessageAsJson; // Use WebMessageAsJson if the message is JSON
            MessageBox.Show($"Message received from React: {message}", "Message Received");
        }

        private void generateButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                var jsonObj = JToken.Parse(jsonTextBox.Text);
                string sqlCase = ConvertJsonToSql(jsonObj);

                string output = GenerateDynamicCaseStatement(sqlCase);
                sqlQuery.Text = output;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ConvertJsonToSql(JToken token)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("CASE");
            ProcessIfStatement(token, sql, 1);
            sql.Append("END");
            return sql.ToString();
        }

        private void ProcessIfStatement(JToken token, StringBuilder sql, int depth)
        {
            if (token.Type != JTokenType.Object || !token["if"].HasValues)
                return;

            var condition = token["if"][0];
            var thenValue = token["if"][1];
            var elseValue = token["if"][2];

            string whenClause = ParseCondition(condition);
            sql.AppendLine($"  WHEN {whenClause} THEN {(ParseOperand(thenValue) == null ? "NULL" : $"{ParseOperand(thenValue)}")}");

            if (elseValue.Type == JTokenType.Object && elseValue["if"] != null)
            {
                ProcessIfStatement(elseValue, sql, depth + 1);
            }
            else
            {
                sql.AppendLine($"  ELSE {(ParseOperand(elseValue) == null ? "NULL" : $"{ParseOperand(elseValue)}")}");
            }
        }

        private string ParseCondition(JToken condition)
        {
            if (condition.Type == JTokenType.Object)
            {
                if (condition["=="] != null)
                {
                    var left = ParseOperand(condition["=="][0]);
                    var right = ParseOperand(condition["=="][1]);
                    return $"{left} {(right == null ? "IS NULL " : $"= {right}")}";
                }
                else if (condition["!="] != null)
                {
                    var left = ParseOperand(condition["!="][0]);
                    var right = ParseOperand(condition["!="][1]);
                    return $"{left} {(right == null ? "IS NOT NULL " : $"!= {right}")}";
                }
                else if (condition[">"] != null)
                {
                    var left = ParseOperand(condition[">"][0]);
                    var right = ParseOperand(condition[">"][1]);
                    return $"{left} > {right}";
                }
                else if (condition[">="] != null)
                {
                    var left = ParseOperand(condition[">="][0]);
                    var right = ParseOperand(condition[">="][1]);
                    return $"{left} >= {right}";
                }
                else if (condition["<"] != null)
                {
                    var left = ParseOperand(condition["<"][0]);
                    var right = ParseOperand(condition["<"][1]);
                    return $"{left} < {right}";
                }
                else if (condition["<="] != null)
                {
                    if (condition["<="].Count() == 3)
                    {
                        var left = ParseOperand(condition["<="][0]);
                        var middle = ParseOperand(condition["<="][1]);
                        var right = ParseOperand(condition["<="][2]);
                        return $"{middle} BETWEEN {left} AND {right}";
                    }
                    else if (condition["<="].Count() == 2)
                    {
                        var left = ParseOperand(condition["<="][0]);
                        var right = ParseOperand(condition["<="][1]);
                        return $"{left} <= {right}";
                    }
                }
                else if (condition["like"] != null)
                {
                    var searchValue = ParseOperand(condition["like"][0]);
                    var field = ParseOperand(condition["like"][1]);
                    return $"{field} LIKE '{searchValue.Replace("'", "")}'";
                }
                else if (condition["in"] != null)
                {
                    var field = ParseOperand(condition["in"][0]);
                    var searchValue = ParseOperand(condition["in"][1]);
                    return $"{field} IN ({searchValue})";
                }
                else if (condition["!"] != null)
                {
                    JToken con = condition["!"];
                    if (con["like"] != null)
                    {
                        var searchValue = ParseOperand(con["like"][0]);
                        var field = ParseOperand(con["like"][1]);
                        return $"{field} NOT LIKE '{searchValue.Replace("'", "")}'";
                    }
                    else if (con["in"] != null)
                    {
                        var field = ParseOperand(con["in"][0]);
                        var searchValue = ParseOperand(con["in"][1]);
                        return $"{field} NOT IN ({searchValue})";
                    }
                    else if (con["<="] != null && con["<="].Count() == 3)
                    {
                        var left = ParseOperand(con["<="][0]);
                        var middle = ParseOperand(con["<="][1]);
                        var right = ParseOperand(con["<="][2]);
                        return $"{middle} NOT BETWEEN {left} AND {right}";
                    }
                    var notCondition = ParseCondition(condition["!"]);
                    var operand = string.Empty;
                    if (notCondition == string.Empty)
                    {
                        operand = ParseOperand(condition["!"]);
                    }
                    return $" {(notCondition != string.Empty ? $"NOT({notCondition})" : $"{operand} = ''")} ";
                }
                else if (condition["!!"] != null)
                {
                    var notCondition = ParseOperand(condition["!!"]);
                    return $"{notCondition} != ''";
                }
                else if (condition["and"] != null)
                {
                    var conditions = condition["and"].Select(c => ParseCondition(c));
                    return string.Join(" AND ", conditions);
                }
                else if (condition["or"] != null)
                {
                    var conditions = condition["or"].Select(c => ParseCondition(c));
                    return string.Join(" OR ", conditions);
                }
            }
            return string.Empty;
        }

        private string ParseOperand(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                if (token["var"] != null)
                    return $"[{token["var"]}]";
                else if (token["toUpperCase"] != null)
                    return $"UPPER({ParseOperand(token["toUpperCase"][0])})";
                else if (token["toLowerCase"] != null)
                    return $"LOWER({ParseOperand(token["toLowerCase"][0])})";
                else if (token["toLen"] != null)
                    return $"LEN({ParseOperand(token["toLen"][0])})";
                else if (token["toLeft"] != null)
                {
                    var function = ParseOperand(token["toLeft"][0]);
                    var length = ParseOperand(token["toLeft"][1]);
                    return $"LEFT({function}, {length})";
                }
                else if (token["toRight"] != null)
                {
                    var function = ParseOperand(token["toRight"][0]);
                    var length = ParseOperand(token["toRight"][1]);
                    return $"RIGHT({function}, {length})";
                }
                else if (token["toSubstring"] != null)
                {
                    var function = ParseOperand(token["toSubstring"][0]);
                    var start = ParseOperand(token["toSubstring"][1]);
                    var length = ParseOperand(token["toSubstring"][2]);
                    return $"SUBSTRING({function}, {start}, {length})";
                }
                else if (token["toReplace"] != null)
                {
                    var function = ParseOperand(token["toReplace"][0]);
                    var searchString = ParseOperand(token["toReplace"][1]);
                    var replacementString = ParseOperand(token["toReplace"][2]);
                    return $"REPLACE({function}, {searchString}, {replacementString})";
                }
                else if (token["toIsNull"] != null)
                {
                    var function = ParseOperand(token["toIsNull"][0]);
                    var replacementString = ParseOperand(token["toIsNull"][1]);
                    return $"ISNULL({function}, {replacementString})";
                }
                else if (token["toConcat"] != null)
                {
                    string field1 = ParseOperand(token["toConcat"][0]);
                    string field2 = ParseOperand(token["toConcat"][1]);
                    string fieldsStr = $"{field1}, {field2}";
                    string field3 = string.Empty;
                    string field4 = string.Empty;
                    string field5 = string.Empty;

                    if (token["toConcat"].Count() >= 3 && token["toConcat"][2] != null)
                    {
                        field3 = ParseOperand(token["toConcat"][2]);
                        fieldsStr += $", {field3}";
                    }
                    if (token["toConcat"].Count() >= 4 && token["toConcat"][3] != null)
                    {
                        field4 = ParseOperand(token["toConcat"][3]);
                        fieldsStr += $", {field4}";
                    }
                    if (token["toConcat"].Count() >= 5 && token["toConcat"][4] != null)
                    {
                        field5 = ParseOperand(token["toConcat"][4]);
                        fieldsStr += $", {field5}";
                    }
                    return $"CONCAT({fieldsStr})";
                }
            }
            else if (token.Type == JTokenType.String)
            {
                return $"'{token}'";
            }
            else if (token.Type == JTokenType.Array)
            {
                var array = (JArray)token;
                return string.Join(", ", array.Select(item => ParseOperand(item)));
            }
            else if (token.Type == JTokenType.Null)
            {
                return null;
            }

            return token.ToString();
        }

        static string GenerateDynamicCaseStatement(string input)
        {
            var output = new StringBuilder();
            var lines = input.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("CASE") || trimmedLine.StartsWith("END") || trimmedLine.StartsWith("ELSE"))
                {
                    // Append CASE, END, and ELSE lines as-is
                    output.AppendLine(trimmedLine);
                }
                else if (trimmedLine.StartsWith("WHEN"))
                {
                    // Process WHEN ... THEN clauses in the same line
                    ProcessWhenThenClause(trimmedLine, output);
                }
            }

            return output.ToString();
        }

        static void ProcessWhenThenClause(string clause, StringBuilder output)
        {
            // Extract the WHEN and THEN parts
            var whenThenMatch = Regex.Match(clause, @"WHEN\s+(.*)\s+THEN\s+(.*)");
            if (!whenThenMatch.Success)
            {
                output.AppendLine(clause); // Append as-is if no WHEN ... THEN pattern is found
                return;
            }

            string whenPart = whenThenMatch.Groups[1].Value.Trim();
            string thenPart = whenThenMatch.Groups[2].Value.Trim();

            // Check if the WHEN part contains _CrosswalkTable
            string selectStatement = GenerateDynamicSelectStatement(clause);
            List<Dictionary<string, object>> crosswalkTableData = RetrieveCrosswalkTableDataFromDatabase(selectStatement);

            // Extract all conditions in the WHEN part using regex
            var conditionMatches = Regex.Matches(whenPart, @"\[(\w+_CrosswalkTable)\.(\w+)\]");
            if (conditionMatches.Count > 0)
            {
                foreach (var row in crosswalkTableData)
                {
                    string dynamicWhenPart = whenPart;

                    // Process each condition independently
                    foreach (Match condition in conditionMatches)
                    {
                        string crosswalkTable = condition.Groups[1].Value; // e.g., HPCODE_CrosswalkTable
                        string crosswalkColumn = condition.Groups[2].Value; // e.g., Provider_ID

                        // Replace the crosswalk column reference with its value if it exists in the current row
                        if (row.ContainsKey(crosswalkColumn))
                        {
                            dynamicWhenPart = dynamicWhenPart.Replace($"[{crosswalkTable}.{crosswalkColumn}]", $"'{row[crosswalkColumn]}'");
                        }
                    }

                    // Process the THEN part
                    string dynamicThenPart = thenPart;
                    var thenCrosswalkMatch = Regex.Match(thenPart, @"\[(\w+_CrosswalkTable)\.(\w+)\]");
                    if (thenCrosswalkMatch.Success)
                    {
                        string thenCrosswalkTable = thenCrosswalkMatch.Groups[1].Value;
                        string thenCrosswalkColumn = thenCrosswalkMatch.Groups[2].Value;

                        // Find the THEN column in the current row
                        if (row.ContainsKey(thenCrosswalkColumn))
                        {
                            dynamicThenPart = thenPart.Replace($"[{thenCrosswalkTable}.{thenCrosswalkColumn}]", $"'{row[thenCrosswalkColumn]}'");
                        }
                    }

                    // Append the dynamic WHEN ... THEN clause
                    output.AppendLine($"  WHEN {dynamicWhenPart} THEN {dynamicThenPart}");
                }
            }
            else
            {
                // If no _CrosswalkTable in WHEN part, append the clause as-is
                output.AppendLine($"  WHEN {whenPart} THEN {thenPart}");
            }
        }


        //static void ProcessWhenThenClause(string clause, StringBuilder output)
        //{
        //    // Extract the WHEN and THEN parts
        //    var whenThenMatch = Regex.Match(clause, @"WHEN\s+(.*)\s+THEN\s+(.*)");
        //    if (!whenThenMatch.Success)
        //    {
        //        output.AppendLine(clause); // Append as-is if no WHEN ... THEN pattern is found
        //        return;
        //    }

        //    string whenPart = whenThenMatch.Groups[1].Value.Trim();
        //    string thenPart = whenThenMatch.Groups[2].Value.Trim();

        //    // Check if the WHEN part contains _CrosswalkTable
        //    var whenCrosswalkMatch = Regex.Match(whenPart, @"\[(\w+_CrosswalkTable)\.(\w+)\]");
        //    if (whenCrosswalkMatch.Success)
        //    {
        //        string crosswalkTable = whenCrosswalkMatch.Groups[1].Value; // e.g., COMPANY_ID_CrosswalkTable
        //        string crosswalkColumn = whenCrosswalkMatch.Groups[2].Value; // e.g., Provider_ID

        //        string selectStatement = GenerateDynamicSelectStatement(clause);
        //        List<Dictionary<string, object>> crosswalkTableData = RetrieveCrosswalkTableDataFromDatabase(selectStatement);

        //        // Loop through crosswalkTableData to find matching rows
        //        foreach (var row in crosswalkTableData)
        //        {
        //            foreach (var kvp in row)
        //            {
        //                // Check if the key contains the crosswalk column and table
        //                if (kvp.Key.Contains(crosswalkColumn)) //&& kvp.Key.Contains(crosswalkTable.Replace("_CrosswalkTable", "")))
        //                {
        //                    var columnValue = kvp.Value;

        //                    // Replace the _CrosswalkTable column with its value in the WHEN part
        //                    string dynamicWhenPart = whenPart.Replace($"[{crosswalkTable}.{crosswalkColumn}]", $"'{columnValue}'");

        //                    // Process the THEN part if it contains _CrosswalkTable
        //                    string dynamicThenPart = thenPart;
        //                    var thenCrosswalkMatch = Regex.Match(thenPart, @"\[(\w+_CrosswalkTable)\.(\w+)\]");
        //                    if (thenCrosswalkMatch.Success)
        //                    {
        //                        string thenCrosswalkTable = thenCrosswalkMatch.Groups[1].Value;
        //                        string thenCrosswalkColumn = thenCrosswalkMatch.Groups[2].Value;

        //                        // Loop again to find THEN part match
        //                        foreach (var thenRow in crosswalkTableData)
        //                        {
        //                            // Find the THEN column in the same row as the WHEN column
        //                            if (thenRow.ContainsKey(crosswalkColumn) && thenRow[crosswalkColumn].Equals(columnValue))
        //                            {
        //                                if (thenRow.ContainsKey(thenCrosswalkColumn))
        //                                {
        //                                    dynamicThenPart = thenPart.Replace($"[{thenCrosswalkTable}.{thenCrosswalkColumn}]", $"'{thenRow[thenCrosswalkColumn]}'");
        //                                    break; // Exit the loop once the correct THEN value is found
        //                                }
        //                            }
        //                        }
        //                    }

        //                    // Append the dynamic WHEN ... THEN clause
        //                    output.AppendLine($"  WHEN {dynamicWhenPart} THEN {dynamicThenPart}");
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // If no _CrosswalkTable in WHEN part, append the clause as-is
        //        output.AppendLine($"  WHEN {whenPart} THEN {thenPart}");
        //    }
        //}

        static string GenerateDynamicSelectStatement(string whenThenPart)
        {
            // Extract column names from the WHEN ... THEN part
            var columnMatches = Regex.Matches(whenThenPart, @"\[(\w+_CrosswalkTable)\.(\w+)\]");

            if (columnMatches.Count == 0)
            {
                throw new ArgumentException("No _CrosswalkTable columns found in the WHEN ... THEN part.");
            }

            // Extract table name and columns
            string tableNameWithSuffix = columnMatches[0].Groups[1].Value; // e.g., COMPANY_ID_CrosswalkTable
            string tableName = tableNameWithSuffix.Replace("_CrosswalkTable", ""); // e.g., COMPANY_ID

            var columns = new StringBuilder();
            int inputColumnCounter = 1; // Counter for InputColumn1, InputColumn2, etc.

            // Split the WHEN ... THEN part into WHEN and THEN sections
            var whenThenSplit = Regex.Split(whenThenPart, @" THEN ");
            string whenPart = whenThenSplit[0].Trim(); // WHEN section
            string thenPart = whenThenSplit.Length > 1 ? whenThenSplit[1].Trim() : string.Empty; // THEN section

            // Process columns in the WHEN part
            foreach (Match match in columnMatches)
            {
                string columnName = match.Groups[2].Value; // e.g., Provider_ID or COMPANY_ID
                if (whenPart.Contains($"[{match.Groups[1].Value}.{columnName}]"))
                {
                    // For WHEN part columns, use InputColumn1, InputColumn2, etc.
                    string jsonPath = $"InputColumn{inputColumnCounter++}";
                    columns.AppendLine($"    JSON_VALUE(JCI.JsonColumn, '$.{jsonPath}') AS {columnName},");
                }
            }

            // Process columns in the THEN part
            foreach (Match match in columnMatches)
            {
                string columnName = match.Groups[2].Value; // e.g., Provider_ID or COMPANY_ID
                if (thenPart.Contains($"[{match.Groups[1].Value}.{columnName}]"))
                {
                    // For THEN part columns, use Output
                    columns.AppendLine($"    JSON_VALUE(JCI.JsonColumn, '$.Output') AS {columnName}");
                }
            }

            // Build the SELECT statement
            var selectStatement = new StringBuilder();
            selectStatement.AppendLine("SELECT");
            selectStatement.Append(columns);
            selectStatement.AppendLine("FROM JsonCompanyIdentifiers JCI");
            selectStatement.AppendLine("INNER JOIN CrosswalkMaster CM ON JCI.CrosswalkMasterID = CM.ID");
            selectStatement.AppendLine($"WHERE CM.ExtensionTableColumn = '{tableName}';");

            return selectStatement.ToString();
        }

        static List<Dictionary<string, object>> RetrieveCrosswalkTableDataFromDatabase(string selectStatement)
        {
            string connectionString = "Data Source=Xeon2\\dev01;Initial Catalog=MPM_EDL;Encrypt=True;TrustServerCertificate=True;User ID=KSUser;Password=KSUser";

            var results = new List<Dictionary<string, object>>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Open the database connection
                connection.Open();

                using (SqlCommand command = new SqlCommand(selectStatement, connection))
                {
                    // Execute the query
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Loop through the result set
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();

                            // Iterate through each column in the row
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.GetValue(i);
                            }

                            results.Add(row);
                        }
                    }
                }
            }

            return results;
        }
    }
}