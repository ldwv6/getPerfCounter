using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.ServiceProcess;
using System.Timers;
using System.IO;
using System.Linq;

namespace EntPerfService
{
    public partial class Service1 : ServiceBase
    {
        /// <summary>
        /// 전역 필드
        /// </summary>
      
        private Timer timer;
        private string dataFolder;
        private string logFileName;
        BasePerfInfo oPerfInfo;
        bool objFlag = true;

        public Service1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 디버깅을 위한 메서드 
        /// </summary>

        public void Ondebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            // app.config 로 부터 얻어오는 값 
            dataFolder = ConfigurationManager.AppSettings["DataFolder"];
            int interval = Int32.Parse(ConfigurationManager.AppSettings["Interval"]);

            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            timer = new Timer();
            // app.config 로 부터 얻어오는 값 

            timer.Interval = interval;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(objFlag)
            {
                oPerfInfo = CreateObj();
                objFlag = false;
            }

            if(DateTime.Now.DayOfWeek == DayOfWeek.Monday)
            {
                Directory.GetFiles(dataFolder)
                    .Select(f => new FileInfo(f))
                    .Where(f => f.CreationTime < DateTime.Now.AddMonths(-1))
                    .ToList().ForEach(f => f.Delete());
            }

            oPerfInfo.GetValueFromCounter();
            var json = JsonConvert.SerializeObject(oPerfInfo);

            DateTime today = DateTime.Now;
            string date = today.ToString("yyyyMMdd");
            string logfilePath = dataFolder + "\\" + logFileName;

            logFileName = "";
            logFileName = "PerfLog_" + date + ".log";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(logfilePath, true))
            {
                file.WriteLine(json);
            }
        }

        protected override void OnStop()
        {
        }

        /// <summary>
        /// SQL 과 WEB 간 정적 다형성
        /// </summary>
        /// <returns></returns>

        private static BasePerfInfo CreateObj()
        {

            if (PerformanceCounterCategory.Exists("SQLServer:SQL Statistics"))
            {
                return new SqlPerfInfo();
            }

            else if (PerformanceCounterCategory.Exists("MSSQL$TOURDBINS:General Statistics"))
            {
                ServiceController sc = new ServiceController("MSSQL$TOURDBINS");


                if(sc.Status == ServiceControllerStatus.Running)
                {
                    return new TourSqlPerfInfo();
                }

                else
                {
                    return new AirSqlPerfInfo();
                }   
            }

            else
            {
                return new PerfInfo();
            }
        }
    }


    /// <summary>
    /// Perfmonce Counnter 클래스 모음 
    /// </summary>

    class BasePerfInfo
    {
        public virtual void GetValueFromCounter() { }
    }

    class PerfInfo : BasePerfInfo
    {
        PerformanceCounter oProcessor = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        PerformanceCounter oProcessorQueueLength = new PerformanceCounter("System", "Processor Queue Length");
        PerformanceCounter oMemory = new PerformanceCounter("Memory", "% Committed Bytes In Use");


        public string TimeStamp { get; set; }
        public string HostsName { get; set; }
        public float Processor { get; set; }
        public float ProcessorQueueLength { get; set; }
        public float Memory { get; set; }

        public override void GetValueFromCounter()
        {
            DateTime _timeStamp = DateTime.Now;
            this.TimeStamp = string.Format("{0:s}", _timeStamp);
            this.HostsName = Dns.GetHostName();
            this.Processor = oProcessor.NextValue();
            this.ProcessorQueueLength = oProcessorQueueLength.NextValue();
            this.Memory = oMemory.NextValue();
        }
    }

    class SqlPerfInfo : BasePerfInfo
    {

        PerformanceCounter oProcessor = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        PerformanceCounter oProcessorQueueLength = new PerformanceCounter("System", "Processor Queue Length");
        PerformanceCounter oMemory = new PerformanceCounter("Memory", "% Committed Bytes In Use");
        PerformanceCounter oProcessesBlocked = new PerformanceCounter("SQLServer:General Statistics", "Processes blocked");
        PerformanceCounter oBatchRequests = new PerformanceCounter("SQLServer:SQL Statistics", "Batch Requests/sec");
        PerformanceCounter oSqlCompliations = new PerformanceCounter("SQLServer:SQL Statistics", "SQL Compilations/sec");
        PerformanceCounter oSQLreComplisations = new PerformanceCounter("SQLServer:SQL Statistics", "SQL Re-Compilations/sec");
        PerformanceCounter oUserConnections = new PerformanceCounter("SQLServer:General Statistics", "User Connections");


        public string TimeStamp { get; set; }
        public string HostsName { get; set; }
        public float Processor { get; set; }
        public float ProcessorQueueLength { get; set; }
        public float Memory { get; set; }
        public float ProcessesBlocked { get; set; }
        public float BatchRequests { get; set; }
        public float SqlCompliations { get; set; }
        public float SQLreComplisations { get; set; }
        public float UserConnections { get; set; }

        public override void GetValueFromCounter()
        {
            DateTime _timeStamp = DateTime.Now;
            this.TimeStamp = string.Format("{0:s}", _timeStamp);
            this.HostsName = Dns.GetHostName();
            this.Processor = oProcessor.NextValue();
            this.ProcessorQueueLength = oProcessorQueueLength.NextValue();
            this.Memory = oMemory.NextValue();
            this.ProcessesBlocked = oProcessesBlocked.NextValue();
            this.BatchRequests = oBatchRequests.NextValue();
            this.SqlCompliations = oSqlCompliations.NextValue();
            this.SQLreComplisations = oSQLreComplisations.NextValue();
            this.UserConnections = oUserConnections.NextValue();
        }
    }


    class TourSqlPerfInfo : BasePerfInfo
    {

        PerformanceCounter oProcessor = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        PerformanceCounter oProcessorQueueLength = new PerformanceCounter("System", "Processor Queue Length");
        PerformanceCounter oMemory = new PerformanceCounter("Memory", "% Committed Bytes In Use");
        PerformanceCounter oProcessesBlocked = new PerformanceCounter("MSSQL$TOURDBINS:General Statistics", "Processes blocked");
        PerformanceCounter oBatchRequests = new PerformanceCounter("MSSQL$TOURDBINS:SQL Statistics", "Batch Requests/sec");
        PerformanceCounter oSqlCompliations = new PerformanceCounter("MSSQL$TOURDBINS:SQL Statistics", "SQL Compilations/sec");
        PerformanceCounter oSQLreComplisations = new PerformanceCounter("MSSQL$TOURDBINS:SQL Statistics", "SQL Re-Compilations/sec");
        PerformanceCounter oUserConnections = new PerformanceCounter("MSSQL$TOURDBINS:General Statistics", "User Connections");


        public string TimeStamp { get; set; }
        public string HostsName { get; set; }
        public float Processor { get; set; }
        public float ProcessorQueueLength { get; set; }
        public float Memory { get; set; }
        public float ProcessesBlocked { get; set; }
        public float BatchRequests { get; set; }
        public float SqlCompliations { get; set; }
        public float SQLreComplisations { get; set; }
        public float UserConnections { get; set; }

        public override void GetValueFromCounter()
        {
            DateTime _timeStamp = DateTime.Now;
            this.TimeStamp = string.Format("{0:s}", _timeStamp);
            this.HostsName = Dns.GetHostName();
            this.Processor = oProcessor.NextValue();
            this.ProcessorQueueLength = oProcessorQueueLength.NextValue();
            this.Memory = oMemory.NextValue();
            this.ProcessesBlocked = oProcessesBlocked.NextValue();
            this.BatchRequests = oBatchRequests.NextValue();
            this.SqlCompliations = oSqlCompliations.NextValue();
            this.SQLreComplisations = oSQLreComplisations.NextValue();
            this.UserConnections = oUserConnections.NextValue();
        }
    }

    class AirSqlPerfInfo : BasePerfInfo
    {

        PerformanceCounter oProcessor = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        PerformanceCounter oProcessorQueueLength = new PerformanceCounter("System", "Processor Queue Length");
        PerformanceCounter oMemory = new PerformanceCounter("Memory", "% Committed Bytes In Use");
        PerformanceCounter oProcessesBlocked = new PerformanceCounter("MSSQL$FLIGHTDBINS:General Statistics", "Processes blocked");
        PerformanceCounter oBatchRequests = new PerformanceCounter("MSSQL$FLIGHTDBINS:SQL Statistics", "Batch Requests/sec");
        PerformanceCounter oSqlCompliations = new PerformanceCounter("MSSQL$FLIGHTDBINS:SQL Statistics", "SQL Compilations/sec");
        PerformanceCounter oSQLreComplisations = new PerformanceCounter("MSSQL$FLIGHTDBINS:SQL Statistics", "SQL Re-Compilations/sec");
        PerformanceCounter oUserConnections = new PerformanceCounter("MSSQL$FLIGHTDBINS:General Statistics", "User Connections");


        public string TimeStamp { get; set; }
        public string HostsName { get; set; }
        public float Processor { get; set; }
        public float ProcessorQueueLength { get; set; }
        public float Memory { get; set; }
        public float ProcessesBlocked { get; set; }
        public float BatchRequests { get; set; }
        public float SqlCompliations { get; set; }
        public float SQLreComplisations { get; set; }
        public float UserConnections { get; set; }

        public override void GetValueFromCounter()
        {
            DateTime _timeStamp = DateTime.Now;
            this.TimeStamp = string.Format("{0:s}", _timeStamp);
            this.HostsName = Dns.GetHostName();
            this.Processor = oProcessor.NextValue();
            this.ProcessorQueueLength = oProcessorQueueLength.NextValue();
            this.Memory = oMemory.NextValue();
            this.ProcessesBlocked = oProcessesBlocked.NextValue();
            this.BatchRequests = oBatchRequests.NextValue();
            this.SqlCompliations = oSqlCompliations.NextValue();
            this.SQLreComplisations = oSQLreComplisations.NextValue();
            this.UserConnections = oUserConnections.NextValue();
        }
    }
}
