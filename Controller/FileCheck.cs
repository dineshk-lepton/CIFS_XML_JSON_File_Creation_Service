using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Text;
using System.Threading;
using cloudscribe.HtmlAgilityPack;
using System.Linq;
using System.Globalization;
using System.Security.Policy;
using System.Xml.Linq;
using System.Drawing;
using System.Linq.Expressions;

namespace Service_Running_Status_Check.Controller
{
    public class FileCheck
    {
        bool sendMail; string fromPassword, fromEmail, computerDetails = string.Empty;
        string Mail = ConfigurationManager.AppSettings["Mail"];
        string[] mailingaddresto = Convert.ToString(ConfigurationManager.AppSettings["MailToAddress"]).Split(',');


		int timeDifference = int.Parse(ConfigurationManager.AppSettings["Time_Difference"].ToString());

		string[] fileUrl = Convert.ToString(ConfigurationManager.AppSettings["File_Url"]).Split(',');
		string[] fileName = Convert.ToString(ConfigurationManager.AppSettings["File_Name"]).Split(',');
		string[] fileNameFor = Convert.ToString(ConfigurationManager.AppSettings["File_Name_For"]).Split(',');
		string LogPath = ConfigurationManager.AppSettings["LogPath"].ToString();

		string logPath = string.Empty;
        DateTime hittime = DateTime.Now;
        DateTime fxnExeTime;
        string[] ar = { };

        public FileCheck()
        {
            try
            {
                string[] mailDetails = Mail.Split('/');
                if (mailDetails.Length.Equals(2))
                {
                    fromEmail = mailDetails[0];
                    fromPassword = mailDetails[1];
                    sendMail = true;
                }
                else
                {
                    sendMail = false;
                }
                string hostName = Dns.GetHostName();
                string myIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                computerDetails = hostName + " [" + myIP + "]";
            }
            catch (Exception ex) { }
        }

        public void StartProcess()
        {
            hittime = DateTime.Now;
            WriteLog("First URL hit time Service_Running_Status_Check " + DateTime.Now.ToString() + "", "Url_Hit_time");
			FileCheck obj1 = new FileCheck();
            obj1.FileCreatedCheck(fileUrl, fileName, fileNameFor);
            WriteLog("Last URL hit time Service_Running_Status_Check " + DateTime.Now.ToString() + "", "Url_Hit_time");
        }

        //this method checks pb file of Delhi, Kolkata and Mysore from the URL present in the app config.
        public void FileCreatedCheck(string[] FileUrl, string[] FileName, string[] FileNameFor)
        {
            int i = 0;
			foreach (string fileUrl in FileUrl) 
            {
				WriteLog("Start Processing for URL: "+ fileUrl+ "\n For File: \t\t" + FileName[i] +"\n For Service/File: \t "+ FileNameFor[i] + " At: " + DateTime.Now.ToString() + "", "Url_Hit_time");

				string fileCompletePath = fileUrl + "/"+ FileName[i];
				//Uri myUri = new Uri(URL);
				Uri myUri = new Uri(fileCompletePath);
				sendMail = true;
				try
				{
					HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(myUri);
					HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
					if (myHttpWebResponse.StatusCode == HttpStatusCode.OK)
					{
                        //time in UTS
                        //DateTime today = Convert.ToDateTime(DateTime.Now.ToString("U"));
                        //DateTime serverdatetime =  Convert.ToDateTime(DateTime.Parse(myHttpWebResponse.Headers["Last-Modified"]).ToString("U"));

                        //local time format

                        DateTime serverdatetime = DateTime.Parse(myHttpWebResponse.LastModified.ToString());

						//DateTime.Parse(myHttpWebResponse.Headers["Last-Modified"]);
						/*
						switch (FileName[i].ToUpper().Trim())
						{
                            case "LEPTON-INCIDENTS.XML":
								serverdatetime = DateTime.Parse(myHttpWebResponse.Headers["Last-Modified"]);
								break;
							case "INCIDENTDETAIL":
								serverdatetime = DateTime.Parse(myHttpWebResponse.Headers["Date"]);
								break;							
							default:
								serverdatetime = DateTime.Parse(myHttpWebResponse.Headers["Last-Modified"]);
								break;
						}*/

						//DateTime serverdatetime = DateTime.Parse(myHttpWebResponse.Headers["Last-Modified"]);
						DateTime today = DateTime.Now;
						var diff = Convert.ToInt32((today - serverdatetime).TotalMinutes);
						if (diff > timeDifference)
						{
							SendMailAsync("File: "+FileName[i] +" for "+ FileNameFor[i]+" not generated since " + serverdatetime.ToString() + ". Please check the following path: " + fileUrl, mailingaddresto, FileName[i], "WARNING");
							WriteLog("File: "+FileName[i] +" for "+ FileNameFor[i]+" not generated.Please check the following path: " + fileUrl + "\t" + serverdatetime.ToString() + "\t" + hittime.ToString() + "", "Error_Log");
							sendMail = false;
						}
						WriteLog("Ending Processing for URL: " + fileUrl , "Url_Hit_time");
						WriteLog("=============================================================================================", "Url_Hit_time");

						//WriteLog("Ending Processing for URL: " + fileUrl + "\n For File: " + FileName[i] + "\n For Service/File : " + FileNameFor[i] + " At: " + DateTime.Now.ToString() + "", "Url_Hit_time");

					}
					else
					{
						SendMailAsync("Unable to Process the URL :" +fileUrl+" for File Name: " + FileName[i] + "Please check the following Path: " + fileUrl, mailingaddresto, FileName[i], "WARNING");
						WriteLog("Unable to process the URL :" +fileUrl+" for " + FileName[i] + "\t" + hittime.ToString() + "", "Error_Log");
						sendMail = false;
					}

				}
				catch (Exception ex)
				{
					SendMailAsync("Process failed for " + FileName[i] + " for URL :" +fileUrl+" with this error \"" + ex.Message + "\"", mailingaddresto, FileName[i], "PROCESS FAILURE");
					WriteLog("Process failed for " + FileName[i] + " for URL : "+fileUrl+" with this error \t \"" + ex.Message + "\" \t" + hittime.ToString() + "", "Process_Failure");
					sendMail = false;
				}
                i++;
			}


            

        }

        public void SendMailAsync(string body, string[] emailIDs, string? SubjectHeaderName, string mailType = "")
        {
            try
            {
                if (!sendMail)
                    return;
                body = body + "<p />Thanks,<br />Lepton - File_Create_Check Service <br />-<br /><i> <u>Sent from " + computerDetails + "</u></i>";
                string Subject = "File_Create_Check_Service!....." + SubjectHeaderName + "....." + mailType;
                foreach (string emailID in emailIDs)
                {
                    var fromAddress = new MailAddress(fromEmail);
                    var toAddress = new MailAddress(emailID);
                    SmtpClient smtp = new SmtpClient
                    {
                        Host = "smtp.verio.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                    };
                    var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = Subject,
                        Body = body,
                        IsBodyHtml = true,
                    };
                    try { smtp.Send(message); }
                    catch (Exception ex)
                    {
                        WriteLog("Mail Error\"" + ex.Message + "\"  " + DateTime.Now.ToString() + "", "Email_Failure");
                    }

                }
            }
            catch (Exception ex)
            {

            }
        }
        public void WriteLog(string logMessage, string logFileName)
        {

            try
            {  
                logPath = LogPath + "\\" + logFileName+"_"+ DateTime.Now.ToString("dd_MM_yyyy") + ".txt";
                if (!File.Exists(logPath))
                {
                    FileStream fileStream = File.Create(logPath);
                    fileStream.Close();

                }
                using (StreamWriter txtWriter = File.AppendText(logPath))
                {
                    txtWriter.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\t" + logMessage);
                    if (!logMessage.Contains("Processing"))
                        txtWriter.WriteLine("-------------------------------------------------------------------------");
                }
            }

            catch (Exception ex)
            {

            }
        }


        public void mailTriggr(string URL, string Name)
        {
            HashSet<string> lst = new HashSet<string>();
            var lst3 = new List<Tuple<DateTime, string>>();
            var lst2 = new List<Tuple<DateTime,string>>();
            string filepath = URL;
            Uri myUri = new Uri(filepath);
            sendMail = true;

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(myUri);
            HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
            try
            {
                if (myHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (WebClient client = new WebClient())
                    {
                        string html = client.DownloadString(URL);
                        HtmlDocument htmlDocument = new HtmlDocument();
                        htmlDocument.LoadHtml(html);



                        foreach (var el in htmlDocument.DocumentNode.ChildNodes[0].ChildNodes[1].ChildNodes[3].ChildNodes)
                        {
                            if (el.Name == "#text")
                            {
                                lst.Add(el.InnerText.Split("        ", StringSplitOptions.None)[0].Trim());
                                try
                                {
                                    lst2.Add(Tuple.Create(DateTime.ParseExact(el.InnerText.Split("       ", StringSplitOptions.None)[0].Trim().Replace("  ", " "), "M/dd/yyyy h:mm tt", CultureInfo.InvariantCulture), el.NextSibling.InnerHtml.Split('_')[0]));
                                }
                                catch (Exception ex)
                                {
                                    lst2.Add(Tuple.Create(DateTime.ParseExact(el.InnerText.Split("       ", StringSplitOptions.None)[0].Trim().Replace("  ", " "), "M/d/yyyy h:mm tt", CultureInfo.InvariantCulture), el.NextSibling.InnerHtml.Split('_')[0]));
                                }
                            }
                        }
                        string[] ar = { "CancelledTrains", "DivertedTrains", "PartCancelledTrains", "RescheduledTrains" };
                        foreach (string s in ar)
                        {
                            var x = from a in lst2
                                    where a.Item2 == s
                                    select a;
                            lst3.Add(x.LastOrDefault());
                        }
                        string y = "";
                        foreach (var x in lst3)
                        {
                            if ((DateTime.Now - x.Item1).TotalMinutes > 120)
                            {
                                y += x.Item2 + ", ";

                            }
                        }

                        if (y != "")
                        {
                            SendMailAsync("Unable to process the URL for " + y + "Please check the following path: " + URL, mailingaddresto, Name, "WARNING");
                            WriteLog("Unable to process the URL for " + y + "\t" + hittime.ToString() + "", "Error_Log");
                        }


                    }

                }
            }catch (Exception ex)
            {
                WriteLog("Railway mail trigger"+"\t"+ex.Message+"\t"+hittime.ToString(), "Error_log");
            }
            

        }

       
    }
}
