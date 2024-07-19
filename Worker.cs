using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Service_Running_Status_Check.Controller;
using System.Configuration;

namespace Service_Running_Status_Check
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        string[] mailingaddresto = Convert.ToString(ConfigurationManager.AppSettings["MailToAddress"]).Split(',');
		FileCheck obj;

        public Worker(ILogger<Worker> logger, FileCheck obj1)
        {
            _logger = logger;
            this.obj = obj1;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
				

                int FileCheckInterval = int.Parse(ConfigurationManager.AppSettings["FileCheckInterval"].ToString());
				var strTime = ConfigurationManager.AppSettings["startTime"].ToString().Split(',');
                var endTime = ConfigurationManager.AppSettings["endTime"].ToString().Split(',');
                TimeSpan start = new TimeSpan(Convert.ToInt32(strTime[0]), Convert.ToInt32(strTime[1]), Convert.ToInt32(strTime[2])); //1 o'clock
                TimeSpan end = new TimeSpan(Convert.ToInt32(endTime[0]), Convert.ToInt32(endTime[1]), Convert.ToInt32(endTime[2])); //5 o'clock
                TimeSpan now = DateTime.Now.TimeOfDay;
                if ((now >= start) && (now <= end))
                    obj.StartProcess();
                //60000 millisec=1 min
                await Task.Delay(60000* FileCheckInterval, stoppingToken);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            obj.SendMailAsync("Service Started: Service_Running_Status_Check", mailingaddresto, null, "Service Start Alert");
            obj.WriteLog("Service_Running_Status_Check Service started", "File_Check_Service_Status");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            obj.SendMailAsync("Service Stopped: Service_Running_Status_Check", mailingaddresto, null, "Service Stopped Alert");
            obj.WriteLog("Service_Running_Status_Check Service Stopped", "File_Check_Service_Status");
            return base.StopAsync(cancellationToken);
        }
    }
}
