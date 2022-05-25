
using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using FluentNHibernate.Cfg;
using FluentNHibernate.Automapping;
using NHibernate.Criterion;
using NHibernate.Transform;
using System.Data;
using System.Threading;
using System.Data.SqlClient;

public class NHib : IDisposable
{
	static NHib()
	{
#if DEBUG
		//Cn = @"Data Source=cta-qa-db1;Initial Catalog=Bugtrax;User id=AppUser; password=xxxx";
		Cn = @"Server=localhost\SQLEXPRESS;Database=Bugtrax;Trusted_Connection=True;";



#else
		Cn = @"Data Source=cta-qa-db1;Initial Catalog=InvoiceApp;User id=AppUser; password=xxxx";
		

#endif
	}
	public string strNamespace { get; set; }
	private Type tInNamespaceIfInDiffAssembly = null; //a type of any class in another assembly (only used if strnamespace belongs to another assembly)
	public static string Cn { get; set; }
	private ISession _cn = null;


	private static Mutex _sessionMutex = new Mutex();

	public ISession cn
	{
		get
		{
			if (_cn != null)
				return _cn;

			_sessionMutex.WaitOne();
			try
			{
				ISessionFactory fact = CreateSessionFactory();
				_cn = fact.OpenSession();
				fact.Dispose();
				return _cn;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				_sessionMutex.ReleaseMutex();
			}
		}
	}


	public NHib()
	{

		strNamespace = "Bugtrax.Entity";
		tInNamespaceIfInDiffAssembly = typeof(Bugtrax.Entity.Project);
	}

	public NHib(string strConnectionString)
	{
		strNamespace = "Bugtrax.Entity";
		tInNamespaceIfInDiffAssembly = typeof(Bugtrax.Entity.Project);

		strNamespace = string.Empty;
		Cn = strConnectionString;
	}

	public NHib(string strConnectionString, string strNamespace)
	{
		this.strNamespace = strNamespace;
		Cn = strConnectionString;
	}

	public NHib(string strSQLServer, string strSQLCatalog, string strNamespace) :
		this(@"Data Source=" + strSQLServer + ";Initial Catalog=" + strSQLCatalog + ";Integrated Security=True", strNamespace)
	{ }

	public NHib(string strSQLServer, string strSQLCatalog, string strUser, string strPassword, string strNamespace)
		: this(@"Data Source=" + strSQLServer + ";Initial Catalog=" + strSQLCatalog + ";User Id=" + strUser + ";Password=" + strPassword, strNamespace)
	{ }

	public void Dispose()
	{
		if (cn.IsConnected)
		{
			cn.Flush();
			TransactionCommit();
		}
		if (cn.IsOpen)
		{
			cn.Close();
			cn.Dispose();
		}
		_cn = null;
	}

	public void CloseAndFlush()
	{
		cn.Flush();
		TransactionCommit();
		cn.Close();
		_cn = null;
	}

	/// <summary>
	/// obtain a sqlconnection object from an already open session
	/// </summary>
	/// <param name="cn"></param>
	/// <returns></returns>
	public SqlConnection GetSqlConnection()
	{
		return (SqlConnection)cn.Connection;
	}

	public T GetUnique<T>(string strColumn, object value, string strColumn2, object value2) where T : class
	{
		T tmp = cn.CreateCriteria<T>().
				Add(Restrictions.Eq(strColumn, value) && Restrictions.Eq(strColumn2, value2)).UniqueResult<T>();

		return tmp;
	}

	public T GetUnique<T>(string strColumn, object value, string strColumn2, object value2, string strColumn3, object value3) where T : class
	{
		T tmp = cn.CreateCriteria<T>().
				Add(Restrictions.Eq(strColumn, value) && Restrictions.Eq(strColumn2, value2) && Restrictions.Eq(strColumn3, value3)).UniqueResult<T>();

		return tmp;
	}

	public List<T> GetMany<T>(string strColumn, object value, string strColumn2, object value2) where T : class
	{
		List<T> tmp = cn.CreateCriteria<T>().
				Add(Restrictions.Eq(strColumn, value) && Restrictions.Eq(strColumn2, value2)).List<T>().ToList();

		return tmp;
	}

	public List<T> GetMany<T>(string strColumn, object value, string strColumn2, object value2, string strColumn3, object value3) where T : class
	{
		List<T> tmp = cn.CreateCriteria<T>().
				Add(Restrictions.Eq(strColumn, value) && Restrictions.Eq(strColumn2, value2) && Restrictions.Eq(strColumn3, value3)).List<T>().ToList();

		return tmp;
	}

	public int GetCount(string strSQL)
	{
		//var session = GetSession();

		//int ret = cn.CreateSQLQuery(strSQL).SetResultTransformer
		//          (new AliasToBeanResultTransformer(typeof(int))).UniqueResult<int>();

		DataTable dtbl = UtilDB.ExecuteQueryDataTable(GetSqlConnection(), strSQL);

		return Convert.ToInt32(dtbl.Rows[0][0]);


		//cn.CreateSQLQuery(strSQL)

		//var criteria = cn.CreateCriteria(typeof(T)).Add(Restrictions.Eq("Product", product))
		//  .SetProjection(Projections.CountDistinct("Price")); return (int)criteria.UniqueResult(); 
	}

	//public int GetCount(ICriterion expression)
	//{
	//    //T ret = cn.CreateSQLQuery(strSQL).SetResultTransformer
	//      //          (new AliasToBeanResultTransformer(typeof(T))).UniqueResult<T>();

	//    ICriteria crit = 

	//    var exp = expression.SetProjection(Projections.CountDistinct("*"));

	//    return (int)exp.UniqueResult(); 
	//}

	public T GetUnique<T>(string strColumn, object value) where T : class
	{
		T tmp = cn.CreateCriteria<T>().
				Add(Restrictions.Eq(strColumn, value)).UniqueResult<T>();

		return tmp;
	}

	public T GetUnique<T>(ICriterion expression) where T : class
	{
		T tmp = cn.CreateCriteria<T>().
				Add(expression).UniqueResult<T>();

		return tmp;
	}

	public List<T> GetMany<T>(string strColumn, object value) where T : class
	{
		List<T> tmp = cn.CreateCriteria<T>().
				Add(Restrictions.Eq(strColumn, value)).List<T>().ToList();

		return tmp;
	}

	public List<T> GetAll<T>() where T : class
	{
		List<T> arrTemp = cn.CreateCriteria<T>().List<T>().ToList();

		return arrTemp;
	}

	public List<T> GetMany<T>(ICriterion expression) where T : class
	{
		List<T> arrTemp = cn.CreateCriteria<T>().
				Add(expression).List<T>().ToList();

		return arrTemp;
	}

	public T GetUnique<T>(string strSQL) where T : class
	{
		T ret = cn.CreateSQLQuery(strSQL).SetResultTransformer
					(new AliasToBeanResultTransformer(typeof(T))).UniqueResult<T>();

		return ret;
	}

	public List<T> GetMany<T>(string strSQL) where T : class
	{
		List<T> arrRet = cn.CreateSQLQuery(strSQL).SetResultTransformer
					(new AliasToBeanResultTransformer(typeof(T))).List<T>().ToList();

		return arrRet;
	}

	public void TransactionBegin()
	{
		cn.Transaction.Begin();
	}

	public void TransactionRollBack()
	{
		if (cn.Transaction != null && cn.Transaction.IsActive && !cn.Transaction.WasRolledBack &&
				!cn.Transaction.WasCommitted)
		{
			cn.Transaction.Rollback();
		}
	}

	public void TransactionCommit()
	{
		if (cn.Transaction != null && cn.Transaction.IsActive && !cn.Transaction.WasRolledBack &&
				!cn.Transaction.WasCommitted)
		{
			cn.Transaction.Commit();
		}
	}

	public void DeleteAll(Type obj)
	{
		string strTable = UtilString.DelimFieldFromEnd(obj.ToString(), '.', 1);
		ISQLQuery qry = cn.CreateSQLQuery("delete from [" + strTable + "];");
		qry.ExecuteUpdate();
	}

	private ISessionFactory CreateSessionFactory()
	{
		return FluentConfig()
			.BuildSessionFactory();
	}

	private FluentConfiguration FluentConfig()
	{
		if (string.IsNullOrEmpty(strNamespace))
		{
			FluentConfiguration cfg = Fluently.Configure()
				.Database(FluentNHibernate.Cfg.Db.MsSqlConfiguration.MsSql2000.ConnectionString(Cn).DefaultSchema("dbo"));

			cfg.Mappings(m =>
			m.FluentMappings.AddFromAssembly(System.Reflection.Assembly.GetExecutingAssembly())); //add from default (this) assembly

			return cfg;
		}
		else
		{
			FluentConfiguration cfg = Fluently.Configure()
				.Database(FluentNHibernate.Cfg.Db.MsSqlConfiguration.MsSql2000.ConnectionString(Cn).DefaultSchema("dbo"));

			//THIS WORKS - NOTE: 'AUTO' WILL TRY TO ADD ALL CLASSES BUT SOME METHODS LATER (IE UPDATESCHEMA) WILL FAIL IF THEY'RE NOT
			// VALID VALID NHIB CLASSES.  THE extra mapping class is optional and only necessary if custom configuration is needed. 
			// it will assume the 'ID' property is the primary key
			AutoPersistenceModel mdl = null;
			if (tInNamespaceIfInDiffAssembly != null)
			{
				mdl = AutoMap.Assembly(System.Reflection.Assembly.GetAssembly(tInNamespaceIfInDiffAssembly));
				mdl.UseOverridesFromAssembly(System.Reflection.Assembly.GetAssembly(tInNamespaceIfInDiffAssembly));
			}
			else
			{
				mdl = AutoMap.Assembly(System.Reflection.Assembly.GetExecutingAssembly());
				mdl.UseOverridesFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
			}

			cfg.Mappings(m => m.AutoMappings.Add(mdl.Where(type =>
			type.Namespace != null && type.Namespace.ToLower() == strNamespace.ToLower())));

			return cfg;
		}
	}

	private void CreateTables(Configuration config)
	{
		// this NHibernate tool takes a configuration (with mapping info in)
		// and exports a database schema from it
		new SchemaExport(config)
			.Create(false, true);
	}

	private void UpdateSchema(Configuration config)
	{
		// this NHibernate tool takes a configuration (with mapping info in)
		// and exports a database schema from it
		new SchemaUpdate(config)
			.Execute(false, true);
	}

	private void DropTables(Configuration config)
	{
		// this NHibernate tool takes a configuration (with mapping info in)
		// and exports a database schema from it
		new SchemaExport(config)
			.Drop(false, true);
	}

	/// <summary>
	/// (optionally) creates the tables and updates the schemas to existing tables
	/// </summary>
	public void UpdateSchema()
	{
		FluentConfig().ExposeConfiguration(UpdateSchema)
			.BuildSessionFactory();
	}

	public void CreateTables()
	{
		FluentConfig().ExposeConfiguration(CreateTables)
			.BuildSessionFactory();
	}

	public void DropTables()
	{
		FluentConfig().ExposeConfiguration(DropTables)
			.BuildSessionFactory();
	}

}
