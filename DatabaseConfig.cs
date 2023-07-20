using System;
using System.Data.SQLite;
using System.Globalization;
using System.Data;
using System.Text.RegularExpressions;

namespace TaskBookConsoleApp
{
    internal class DatabaseConfig
    {
        SQLiteConnection myConnection;

        public SQLiteConnection MyDBConnection()
        {
            myConnection = new SQLiteConnection("Data Source=MyTaskbookDB.sqlite;Version=3;");
            myConnection.Open();
            return myConnection;
        }

        #region TableCreation
        public void TaskbookTable()
        {
            string createQuery = "create table if not exists mytaskbook(" +
                "Id integer primary key autoincrement," +
                "NameofTask varchar(50) not null," +
                "AssignedBy varchar(30) not null," +
                "AssignedTo varchar(30) not null," +
                "Date varchar(10) not null," +
                "Duration decimal(10,1) not null," +
                "Status varchar(15) not null)";
            SQLiteCommand command = new SQLiteCommand(createQuery, MyDBConnection());
            command.ExecuteNonQuery();
        }
        #endregion

        public void UserInput()
        {
            #region ReadingUserInput
            Console.Write("Enter Task Name : ");
            string name = Console.ReadLine();
            if (!IsAlphabet(name))
            {
                Console.WriteLine("Invalid Input. Please enter only alphabets and spaces.");
                return;
            }
            Console.Write("Assigned By : ");
            string assignedBy = Console.ReadLine();
            if (!IsAlphabet(assignedBy))
            {
                Console.WriteLine("Invalid Input. Please enter only alphabets and spaces.");
                return;
            }
            Console.Write("Assigned to : ");
            string assignedTo = Console.ReadLine();
            Console.Write("Enter the date (YYYY-MM-DD) : ");
            string dateInput = Console.ReadLine();
            DateTime date;
            if (!(DateTime.TryParseExact(dateInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)))
            {
                Console.WriteLine("Invalid date format. Please enter the date in the format 'YYYY-MM-DD'.");
                return;
            }
            string dateAsString = date.ToString("yyyy-MM-dd");
            decimal taskDuration;
            bool isValidDuration = false;
            do
            {
                Console.WriteLine("Enter the Task Duration : ");
                string duration = Console.ReadLine();
                isValidDuration = decimal.TryParse(duration, out taskDuration);
                if (!isValidDuration)
                {
                    Console.WriteLine("Invalid Input , Enter a Valid Input !");
                }
            } while (!isValidDuration);

            Console.WriteLine("Select the Task Status : ");
            foreach (TaskStatus Status in Enum.GetValues(typeof(TaskStatus)))
            {
                Console.WriteLine($"{(int)Status}. {Status}");
            }
            Console.Write("Choose the Status: ");
            string statusInput = Console.ReadLine();
            if (!int.TryParse(statusInput, out int statusNumber) || !Enum.IsDefined(typeof(TaskStatus), statusNumber))
            {
                Console.WriteLine("Invalid selection. Please enter a valid status number.");
                return;
            }

            TaskStatus selectedStatus = (TaskStatus)statusNumber;
            string selectedValue = Enum.GetName(typeof(TaskStatus), selectedStatus);
            #endregion

            #region WriteToTable
            string queryString = "INSERT INTO mytaskbook (NameofTask,AssignedBy,AssignedTo,Date,Duration,Status) " +
   "VALUES (@TaskName, @AssignedBy, @AssignedTo, @TaskDate, @TaskDuration, @TaskStatus)";

            SQLiteCommand command = new SQLiteCommand(queryString, MyDBConnection());
            command.Parameters.AddWithValue("@TaskName", name);
            command.Parameters.AddWithValue("@AssignedBy", assignedBy);
            command.Parameters.AddWithValue("@AssignedTo", assignedTo);
            command.Parameters.AddWithValue("@TaskDate", dateAsString);
            command.Parameters.AddWithValue("@TaskDuration", taskDuration);
            command.Parameters.AddWithValue("@TaskStatus", selectedValue);

            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine("Data inserted successfully.");
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Error occurred while inserting data: " + ex.Message);
            }
            #endregion
        }

        #region ShowData

        public bool IsAlphabet(string value)
        {
            return Regex.IsMatch(value, "^[a-zA-Z ]+$");
        }
        public void ViewData()
        {
            string query = "select * from mytaskbook";
            SQLiteCommand command = new SQLiteCommand(query, myConnection);
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int taskId = reader.GetInt32(0);
                    string taskName = reader.GetString(1);
                    string assignedBy = reader.GetString(2);
                    string assignedTo = reader.GetString(3);
                    string taskDate = reader.GetString(4);
                    Decimal taskDuration = reader.GetDecimal(5);
                    string taskStatus = reader.GetString(6);

                    Console.WriteLine($"Task ID: {taskId}");
                    Console.WriteLine($"Task Name: {taskName}");
                    Console.WriteLine($"Assigned By: {assignedBy}");
                    Console.WriteLine($"Assigned To: {assignedTo}");
                    Console.WriteLine($"Task Date: {taskDate}");
                    Console.WriteLine($"Task Duration: {taskDuration}");
                    Console.WriteLine($"Task Status: {taskStatus}");
                    Console.WriteLine();
                }
            }
        }
        #endregion
        #region TaskStatus
        public enum TaskStatus
        {
            Ongoing = 1,
            Completed,
            OnHold
        };
        #endregion

        public void DeleteData()
        {
            Console.WriteLine("Enter the Task ID to delete:");
            string input = Console.ReadLine();

            int taskId;
            if (!int.TryParse(input, out taskId))
            {
                Console.WriteLine("Invalid Task ID. Please enter a valid numeric ID.");
                return;
            }

            bool idExists = IfIdExists(taskId);
            if (!idExists)
            {
                Console.WriteLine("The provided Task ID does not exist.");
                return;
            }
            string query = $"delete from mytaskbook where Id={taskId}";
            SQLiteCommand command = new SQLiteCommand(query, MyDBConnection());
            command.ExecuteNonQuery();
            Console.WriteLine($"Data at ID {taskId} Deleted Successfully.");
        }
        #region CheckIdExist
        public bool IfIdExists(int id)
        {
            string query = "SELECT 1 FROM mytaskbook WHERE Id = @TaskId";
            using (SQLiteCommand command = new SQLiteCommand(query, MyDBConnection()))
            {
                command.Parameters.AddWithValue("@TaskId", id);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
        #endregion
        public void ModifyData()
        {
            Console.WriteLine("Enter the ID of the task to modify:");
            string input = Console.ReadLine();

            int taskId;
            if (!int.TryParse(input, out taskId))
            {
                Console.WriteLine("Invalid Task ID. Please enter a valid numeric ID.");
                return;
            }

            bool idExists = IfIdExists(taskId);
            if (!idExists)
            {
                Console.WriteLine("The provided Task ID does not exist.");
                return;
            }
            Console.Write("Enter Task Name : ");
            string name = Console.ReadLine();
            if (!IsAlphabet(name))
            {
                Console.WriteLine("Invalid Input. Please enter only alphabets and spaces.");
                return;
            }
            Console.Write("Assigned By : ");
            string assignedBy = Console.ReadLine();
            if (!IsAlphabet(assignedBy))
            {
                Console.WriteLine("Invalid Input. Please enter only alphabets and spaces.");
                return;
            }
            Console.Write("Assigned to : ");
            string assignedTo = Console.ReadLine();
            if (!IsAlphabet(assignedTo))
            {
                Console.WriteLine("Invalid Input. Please enter only alphabets and spaces.");
                return;
            }
            Console.Write("Enter the date (YYYY-MM-DD) : ");
            string dateInput = Console.ReadLine();
            string format = "yyyy-MM-dd";
            DateTime date;
            if (!(DateTime.TryParseExact(dateInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)))
            {
                Console.WriteLine("Invalid date format. Please enter the date in the format 'YYYY-MM-DD'.");
                return;
            }
            string dateAsString = date.ToString("yyyy-MM-dd");
            decimal taskDuration;
            bool isValidDuration = false;
            do
            {
                Console.WriteLine("Enter the Task Duration : ");
                string duration = Console.ReadLine();
                isValidDuration = decimal.TryParse(duration, out taskDuration);
                if (!isValidDuration)
                {
                    Console.WriteLine("Invalid input, enter a valid input!");
                }
            } while (!isValidDuration);

            Console.WriteLine("Select the Task Status :");
            foreach (TaskStatus Status in Enum.GetValues(typeof(TaskStatus)))
            {
                Console.WriteLine($"{(int)Status}.{Status}");
            }
            Console.Write("Choose the Status: ");
            string statusInput = Console.ReadLine();
            if (!int.TryParse(statusInput, out int statusNumber) || !Enum.IsDefined(typeof(TaskStatus), statusNumber))
            {
                Console.WriteLine("Invalid selection. Please enter a valid status number.");
                return;
            }

            TaskStatus selectedStatus = (TaskStatus)statusNumber;
            string selectedValue = Enum.GetName(typeof(TaskStatus), selectedStatus);
            string queryString = "UPDATE mytaskbook SET NameofTask = @TaskName, AssignedBy = @AssignedBy, AssignedTo = @AssignedTo, " +
                "Date = @TaskDate, Duration = @TaskDuration, Status = @TaskStatus WHERE Id = @TaskId";

            SQLiteCommand command = new SQLiteCommand(queryString, MyDBConnection());
            command.Parameters.AddWithValue("@TaskId", taskId);
            command.Parameters.AddWithValue("@TaskName", name);
            command.Parameters.AddWithValue("@AssignedBy", assignedBy);
            command.Parameters.AddWithValue("@AssignedTo", assignedTo);
            command.Parameters.AddWithValue("@TaskDate", dateAsString);
            command.Parameters.AddWithValue("@TaskDuration", taskDuration);
            command.Parameters.AddWithValue("@TaskStatus", selectedValue);

            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine("Data modified successfully.");
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine("Error occurred while modifying data: " + ex.Message);
            }
        }



        public DataRow GetTaskDataById(int taskId)
        {
            string query = "SELECT NameofTask, AssignedBy, AssignedTo, Date, Duration, Status FROM mytaskbook WHERE Id = @TaskId";
            using (SQLiteCommand command = new SQLiteCommand(query, MyDBConnection()))
            {
                command.Parameters.AddWithValue("@TaskId", taskId);

                DataTable dataTable = new DataTable();
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }

                if (dataTable.Rows.Count > 0)
                {
                    return dataTable.Rows[0];
                }
            }

            return null;
        }



    }
}
