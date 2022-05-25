using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NHibernate;
using System.Collections;
using System.Data.SqlClient;

namespace RIFRegister
{
    public partial class HibConfig : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void btnLoadActtiveDir2_Click(object sender, EventArgs e)
        {
            //UtilNHib.UpdateSchema();
            using (NHib Cn = new NHib())
            {
                Cn.UpdateSchema();
            }
            lblStatus.Text = "Done! " + DateTime.Now.ToLongTimeString();
        }

        protected void btnLoadActtiveDir0_Click(object sender, EventArgs e)
        {
            //UtilNHib.CreateTables();
            using (NHib Cn = new NHib())
            {
                Cn.CreateTables();
            }
            lblStatus.Text = "Done! " + DateTime.Now.ToLongTimeString();
        }

        protected void btnLoadActtiveDir1_Click(object sender, EventArgs e)
        {
            //UtilNHib.DropTables();
            using (NHib Cn = new NHib())
            {
			    lblStatus.Text = "Not doing this until you explicitly uncomment the code";
                //Cn.DropTables();
            }
            //lblStatus.Text = "Done! " + DateTime.Now.ToLongTimeString();
        }

		protected void btnLoadActtiveDir3_Click(object sender, EventArgs e)
		{
			using (NHib Cn = new NHib())
			{
				ISession cn = Cn.cn;
				//Training t = new Training();
				//t.Description = "some descript";
				//t.Location = "some location";
				//t.DateTimeStart = DateTime.Now;
				//t.DateTimeEnd = DateTime.Now;
				//t.TrainingLevel = 1;
				//t.TrainingTitle = "some training";
				//cn.Save(t);

				//TrainingPersonInfo tpi = new TrainingPersonInfo();
				//tpi.TrainingID = 1;
				//tpi.PersonID = 1;
				//tpi.boolAttended = false;
				//tpi.strNotes = "some notes";
				//cn.Save(tpi);

				//Trainer tr = new Trainer();
				//tr.sAMAccountName = "Michael Lamuth";
				//cn.Save(tr);
				//tr = new Trainer();
				//tr.sAMAccountName = "Tony Tong";
				//cn.Save(tr);
				//tr = new Trainer();
				//tr.sAMAccountName = "Valerie Guzman";
				//cn.Save(tr);
				//tr = new Trainer();
				//tr.sAMAccountName = "Joshua Ralls";
				//cn.Save(tr);
				//tr = new Trainer();
				//tr.sAMAccountName = "Ian Elumba";
				//cn.Save(tr);
				//tr = new Trainer();
				//tr.sAMAccountName = "Richard Brown";
				//cn.Save(tr);

				lblStatus.Text = "Done! " + DateTime.Now.ToLongTimeString();
			}
		}

		protected void btnLoadActtiveDir4_Click(object sender, EventArgs e)
        {
            using (NHib Cn = new NHib())
            {
                SqlConnection cn = Cn.GetSqlConnection();
               // UtilDB.ExecuteNonQuery(cn, "truncate table [indreferral];");

                lblStatus.Text = "Done " + DateTime.Now.ToLongTimeString();
            }
        }

        protected void btnTest_Click(object sender, EventArgs e)
        {
            using (NHib cn = new NHib())
            {
                

                //Console.WriteLine(arr.Count + " " + arr2.Count + arr3.Count);
                Response.Write("Done");
            }
        }

    }
}

