using System;
using Newtonsoft.Json;

namespace testapp
{
	public class Product
	{
		public Product()
		{
		}

		public string id { get; set; }
        public string name { get; set; }
        public string categoryId { get; set; }
        public double price { get; set; }
     

		public Product(string id, string name, string categoryId)
		{
			this.id = id;
			this.name = name;
			this.categoryId = categoryId;

		}

	}


}

