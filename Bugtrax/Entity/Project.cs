using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bugtrax.Entity
{
	public class Project
	{
		public virtual Guid ID { get; set; }

		public virtual string Title { get; set; }
	}
}