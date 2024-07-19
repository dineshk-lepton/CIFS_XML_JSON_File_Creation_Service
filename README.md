# CIFS_XML_JSON_File_Creation_Service
This Will Check Creation of Json and XML file For Apple and Google Client.
---------------------------------------------------------------------------
his Service Will Check File Creation Status as Per Below Information:

From Links :        http://leptontransit.leptonsoftware.com/GOOGLE_XML 
                https://app.leptonsoftware.com:8080/api/CIFS/ 

For File_Name:        lepton-incidents.xml
                IncidentDetail

For Clients:        Google_XML
                Apple_JSON

 

This Will check LastModified Date of Above Files in Every 5 minutes for a Difference of 5 Minutes, Means if LastModified Time Difference is more than 5 minutes it will Sent an Alert as a Email.


Change LogFile Path in "Service_Running_Status_Check.dll.config" File

Service Deployed in Services.msc having name as "CIFS_JSON_XML_File_Creation_Check"
