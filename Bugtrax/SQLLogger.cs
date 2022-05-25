using System;
using System.Data.SqlClient;



public class SQLLogger
{
	public static void LogError(Exception e)
	{

		SqlConnection cn = new SqlConnection(NHib.Cn);
		cn.Open();
		string sql = "Insert Into AppLog (Message,StackTrace,Time) VALUES(@Message, @StackTrace,@Time)";
		SqlCommand cm = new SqlCommand(sql, cn);
		cm.Parameters.Add(new SqlParameter("@Message", e.Message));
		cm.Parameters.Add(new SqlParameter("@StackTrace", e.StackTrace));
		cm.Parameters.Add(new SqlParameter("@Time", DateTime.Now));
		cm.ExecuteNonQuery();
		cm.Dispose();
		cn.Close();

	}

	public static void LogMessage(string message)
	{
		SqlConnection cn = new SqlConnection(NHib.Cn);
		cn.Open();
		string sql = "Insert Into AppLog (Message,Time) VALUES(@Message,@Time)";
		SqlCommand cm = new SqlCommand(sql, cn);
		cm.Parameters.Add(new SqlParameter("@Message", message));
		cm.Parameters.Add(new SqlParameter("@Time", DateTime.Now));
		cm.ExecuteNonQuery();
		cm.Dispose();
		cn.Close();
	}


	public static void LogLogin(string samAccount)
	{
		SqlConnection cn = new SqlConnection(NHib.Cn);
		cn.Open();
		string sql = "Insert Into Logins (SAMAccount,Time) VALUES(@SAMAccount,@Time)";
		SqlCommand cm = new SqlCommand(sql, cn);
		cm.Parameters.Add(new SqlParameter("@SAMAccount", samAccount));
		cm.Parameters.Add(new SqlParameter("@Time", DateTime.Now));
		cm.ExecuteNonQuery();
		cm.Dispose();
		cn.Close();
	}

	//public static void LogAction(string staffOrOffice, String action)
	//{
	//	String sAMAccount = Util.GetCurrentUserName();
	//	SqlConnection cn = new SqlConnection(NHib.Cn);
	//	cn.Open();
	//	string sql = "Insert Into ActionLog (CurrentUser,StaffOrOffice,Time,Action) VALUES(@CurrentUser, @StaffOrOffice, @Time, @Action)";
	//	SqlCommand cm = new SqlCommand(sql, cn);
	//	cm.Parameters.Add(new SqlParameter("@CurrentUser", sAMAccount));
	//	cm.Parameters.Add(new SqlParameter("@StaffOrOffice", staffOrOffice));
	//	cm.Parameters.Add(new SqlParameter("@Time", DateTime.Now));
	//	cm.Parameters.Add(new SqlParameter("@Action", action));
	//	cm.ExecuteNonQuery();
	//	cm.Dispose();
	//	cn.Close();
	//}
}


/*
create table AppLog
(
Id int primary key identity(1,1)
,Message varchar(255)
,StackTrace varchar(max)
,Time datetime
)
create table Logins
(
Id int primary key identity(1,1)
,SAMAccount varchar(255)
,Time datetime
)
create table ActionLog
 (
 Id int primary key identity(1,1)
 ,CurrentUser varchar(255)
 ,ReportOwner_id int
 ,Time datetime
 ,Report_Id int
 ,Action varchar(255)
 )
*/
