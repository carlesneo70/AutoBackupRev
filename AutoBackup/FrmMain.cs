using System;
using System.ComponentModel;
using System.Drawing;
using NEO.Helper;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Threading;

using AutoBackup.Class;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace AutoBackup
{
    public partial class FrmMain : Form
    {
        private string dbname;
        private string bkpath;
        private string connstring;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            try
            {
                this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - base.Width, Screen.PrimaryScreen.WorkingArea.Height - base.Height);
                this.progressBar1.Style = ProgressBarStyle.Marquee;

                string[] passedInArgs = Environment.GetCommandLineArgs();

                textBox1.Visible = false;
                textBox2.Visible = false;
                label1.Visible = false;
                label4.Visible = false;
                BtnCancel.Visible = false;
                BtnForceClose.Visible = true;

                if (passedInArgs != null)
                {
                    if (!Utils.IsRunningUnderIDE())
                    {
                        textBox1.Text = passedInArgs[1];
                        textBox2.Text = passedInArgs[2];
                        bkpath = textBox1.Text;
                        connstring = passedInArgs[2];
                        dbname = Function.Between(connstring, "database=", ";", 0).ToUpper();
                    }

                    BtnCancel.Visible = true;
                    BtnForceClose.Visible = false;

                    bool backupAllTables = Array.Exists(passedInArgs, arg => arg.ToLower() == "alltables");
                    if (backupAllTables)
                    {
                        List<string> allTableNames = GetAllTableNames();
                        BackupTables(allTableNames);
                    }
                    else if (passedInArgs.Length >= 4 && passedInArgs[3] == "tables")
                    {
                        List<string> tableNames = new List<string>();
                        for (int i = 4; i < passedInArgs.Length; i++)
                        {
                            tableNames.Add(passedInArgs[i]);
                        }

                        if (tableNames.Count > 0)
                        {
                            // Memulai background worker untuk backup tabel
                            backgroundWorker1.RunWorkerAsync(tableNames);
                            return;
                        }
                    }
                }

                if (!Utils.IsUnderDevelopment())
                {
                    // Memulai background worker untuk backup database
                    backgroundWorker1.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Backup DB Failure with message: " + ex.Message.ToString());
            }
        }


        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.progressBar1.Style = ProgressBarStyle.Blocks;
            base.Dispose();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(5000);
            BackgroundWorker worker = sender as BackgroundWorker;

            if (worker != null)
            {
                worker.ReportProgress(0); // Mulai tugas latar belakang

                if (e.Argument is List<string> tableNames)
                {
                    // Backup tabel
                    BackupTables(tableNames);
                }
                else if (!Array.Exists(Environment.GetCommandLineArgs(), arg => arg.ToLower() == "alltables"))
                {
                    // Backup database
                    Backup();
                }

                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                }
            }
        }


        public void Backup()
        {
            try
            {
                string DbFile = dbname + "_BACKUP_" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".sql";
                string dbfullpath = this.bkpath + DbFile;
                connstring += "charset=utf8;convertzerodatetime=true;";
                using (MySqlConnection conn = new MySqlConnection(connstring))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            mb.ExportToFile(dbfullpath);
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Class.Logger.LogError("Backup DB Failure with message: " + ex.Message.ToString());
            }
        }

        private List<string> GetAllTableNames()
        {
            List<string> tableNames = new List<string>();

            using (MySqlConnection conn = new MySqlConnection(connstring))
            {
                conn.Open();

                DataTable schemaTable = conn.GetSchema("Tables");
                foreach (DataRow row in schemaTable.Rows)
                {
                    string tableName = row["TABLE_NAME"].ToString();
                    tableNames.Add(tableName);
                }

                conn.Close();
            }

            return tableNames;
        }

        private void BackupTables(List<string> tableNames)
        {
            string ApplicationName = Program.appName;
            try
            {
                string backupFolderPath = Path.Combine(bkpath, dbname);
                if (!Directory.Exists(backupFolderPath))
                {
                    Directory.CreateDirectory(backupFolderPath);
                }

                connstring += "charset=utf8mb4;convertzerodatetime=true;";

                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(connstring);

                string targetServerType = "MySQL";  // Anda dapat mengganti dengan tipe server yang sesuai
                string targetServerVersion = GetServerVersion(builder.Server);
                string fileEncoding = Encoding.UTF8.WebName;  // Anda dapat mengganti dengan encoding yang sesuai

                string[] connParts = connstring.Split(';');
                string host = "";
                int port = 0;

                foreach (string part in connParts)
                {
                    string[] keyValue = part.Split('=');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim().ToLower();
                        string value = keyValue[1].Trim();

                        if (key == "server" || key == "data source" || key == "datasource" || key == "host")
                        {
                            host = value;
                        }
                        else if (key == "port")
                        {
                            int.TryParse(value, out port);
                        }
                    }
                }

                using (MySqlConnection conn = new MySqlConnection(connstring))
                {
                    conn.Open();

                    // Mendapatkan karakter set dari database
                    string characterSet = GetCharacterSet(conn);

                    foreach (string tableName in tableNames)
                    {
                        string backupFileName = $"{tableName}_backup_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.sql";
                        string backupFilePath = Path.Combine(backupFolderPath, backupFileName);

                        using (StreamWriter writer = new StreamWriter(backupFilePath, false, Encoding.UTF8))
                        {
                            // Tulis header dengan informasi database
                            writer.WriteLine("/*");
                            writer.WriteLine($" {ApplicationName} database using MySqlBackup.Net Library");
                            writer.WriteLine();
                            writer.WriteLine($" Source Server         : {host}_{port}");
                            writer.WriteLine(" Source Server Type    : MySQL");
                            writer.WriteLine($" Source Server Version : {conn.ServerVersion}");
                            writer.WriteLine($" Source Host           : {host}:{port}");
                            writer.WriteLine($" Source Schema         : {dbname}");
                            writer.WriteLine();
                            writer.WriteLine($" Target Server Type    : {targetServerType}");
                            writer.WriteLine($" Target Server Version : {targetServerVersion}");
                            writer.WriteLine($" File Encoding         : {fileEncoding}");
                            writer.WriteLine();
                            writer.WriteLine($" Date: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                            writer.WriteLine("*/");
                            writer.WriteLine();
                            writer.WriteLine($"SET NAMES {characterSet};");
                            writer.WriteLine("SET FOREIGN_KEY_CHECKS = 0;");
                            writer.WriteLine();

                            // Tulis definisi tabel
                            writer.WriteLine($"-- ----------------------------");
                            writer.WriteLine($"-- Table structure for {tableName}");
                            writer.WriteLine($"-- ----------------------------");
                            writer.WriteLine($"DROP TABLE IF EXISTS `{tableName}`;");
                            writer.WriteLine($"CREATE TABLE `{tableName}` (");

                            bool isFirstColumn = true;

                            using (MySqlCommand cmd = new MySqlCommand($"SHOW COLUMNS FROM {tableName}", conn))
                            {
                                using (MySqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string columnName = reader.GetString("Field");
                                        string columnType = reader.GetString("Type");

                                        if (isFirstColumn)
                                        {
                                            writer.WriteLine($"  `{columnName}` {columnType} NULL DEFAULT NULL");
                                            isFirstColumn = false;
                                        }
                                        else
                                        {
                                            writer.WriteLine($", `{columnName}` {columnType} NULL DEFAULT NULL");
                                        }
                                    }
                                }
                            }

                            writer.WriteLine(") ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;");
                            writer.WriteLine();

                            // Tulis data tabel
                            writer.WriteLine($"-- ----------------------------");
                            writer.WriteLine($"-- Records of {tableName}");
                            writer.WriteLine($"-- ----------------------------");

                            using (MySqlCommand cmd = new MySqlCommand($"SELECT * FROM {tableName}", conn))
                            {
                                using (MySqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        StringBuilder insertCommand = new StringBuilder();
                                        StringBuilder values = new StringBuilder();

                                        insertCommand.Append($"INSERT INTO `{tableName}` VALUES (");

                                        bool isFirstValue = true;

                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            if (!reader.IsDBNull(i))
                                            {
                                                string value = reader.GetValue(i).ToString();

                                                if (isFirstValue)
                                                {
                                                    values.Append($"'{value}'");
                                                    isFirstValue = false;
                                                }
                                                else
                                                {
                                                    values.Append($", '{value}'");
                                                }
                                            }
                                            else
                                            {
                                                if (isFirstValue)
                                                {
                                                    values.Append("NULL");
                                                    isFirstValue = false;
                                                }
                                                else
                                                {
                                                    values.Append(", NULL");
                                                }
                                            }
                                        }

                                        insertCommand.Append(values.ToString());
                                        insertCommand.Append(");");

                                        writer.WriteLine(insertCommand.ToString());
                                    }
                                }
                            }

                            writer.WriteLine();
                            writer.WriteLine("SET FOREIGN_KEY_CHECKS = 1;");
                        }
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Class.Logger.LogError("Backup DB Failure with message: " + ex.Message.ToString());
            }
        }

        private string GetServerVersion(string server)
        {
            using (MySqlConnection conn = new MySqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT VERSION()";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                return cmd.ExecuteScalar().ToString();
            }
        }

        private string GetCharacterSet(MySqlConnection conn)
        {
            string query = "SELECT @@character_set_database";
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                return cmd.ExecuteScalar().ToString();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            bool isBusy = this.backgroundWorker1.IsBusy;
            if (isBusy)
            {
                this.backgroundWorker1.CancelAsync();
                base.Dispose();
                Environment.Exit(1);
                Application.Exit();
            }
        }

        private void BtnForceClose_Click(object sender, EventArgs e)
        {
            base.Dispose();
            Environment.Exit(1);
            Application.Exit();
        }
    }
}
