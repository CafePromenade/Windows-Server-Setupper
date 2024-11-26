using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace Exchange_Install_API.Controllers
{
    public class AutoInstallController : ApiController
    {
        public string MainDirectory = Directory.CreateDirectory(@"C:\Users\Administrator\Desktop\Sites\exchange-install\configs").FullName;
        public class AutoInstall
        {
            public Computer[] Computers { get; set; }
        }

        public class Computer
        {
            public string PCName{ get; set; }
            public string DomainName{ get; set; }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Post(Computer info)
        {
            File.WriteAllText(MainDirectory + "\\" + info.PCName, JsonConvert.SerializeObject(info));
            return Request.CreateResponse(HttpStatusCode.OK, info.DomainName);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Get()
        {
            AutoInstall hui = new AutoInstall();
            List<Computer> com = new List<Computer>();
            foreach (var s in Directory.GetFiles(MainDirectory))
            {
                Computer d = new Computer();
                d = JsonConvert.DeserializeObject<Computer>(File.ReadAllText(s));
                com.Add(d);
            }
            hui.Computers = com.OfType<Computer>().ToArray();

            string plainTextResponse = JsonConvert.SerializeObject(hui);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(plainTextResponse, System.Text.Encoding.UTF8, "text/plain")
            };

            return response;
        }

    }
}
