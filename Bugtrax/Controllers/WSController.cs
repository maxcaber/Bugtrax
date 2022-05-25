using Bugtrax.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Bugtrax.Controllers
{
	public class WSController : ApiController
	{
		[HttpGet]
		public List<Project> GetProjects()
		{
			using (NHib cn = new NHib())
			{
				return cn.GetAll<Project>();

			}
		}
		

	}
}