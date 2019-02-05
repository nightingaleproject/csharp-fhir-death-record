﻿using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ServiceProcess;
using FhirDeathRecord;

namespace FhirDeathRecord.HTTP
{
    public class Program
    {
        public FhirDeathRecordListener Listener;

        public Program()
        {
            Listener = new FhirDeathRecordListener(SendResponse, "http://*:8080/");
        }

        public void Start()
        {
            Listen();
            ManualResetEvent _quitEvent = new ManualResetEvent(false);
            _quitEvent.WaitOne();
            Stop();
        }

        public void Listen()
        {
            Listener.Run();
        }

        public void Stop()
        {
            Listener.Stop();
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();
        }

        public static string SendResponse(HttpListenerRequest request)
        {
            string requestBody = GetBodyContent(request);
            DeathRecord deathRecord = null;

            Console.WriteLine($"Request from: {request.UserHostAddress}, type: {request.ContentType}, url: {request.RawUrl}.");

            // Look at content type to determine input format; be permissive in what we accept as format specification
            switch (request.ContentType)
            {
                case string ijeType when new Regex(@"ije").IsMatch(ijeType): // application/ije
                    IJEMortality ije = new IJEMortality(requestBody);
                    deathRecord = ije.ToDeathRecord();
                    break;
                case string naaccrType when new Regex(@"naaccr").IsMatch(naaccrType): // application/naaccr
                    NAACCRRecord naaccr = new NAACCRRecord(requestBody);
                    deathRecord = naaccr.ToDeathRecord();
                    break;
                case string jsonType when new Regex(@"json").IsMatch(jsonType): // application/fhir+json
                case string xmlType when new Regex(@"xml").IsMatch(xmlType): // application/fhir+xml
                default:
                    deathRecord = new DeathRecord(requestBody);
                    break;
            }

            // Look at URL extension to determine output format; be permissive in what we accept as format specification
            string result = "";
            switch (request.RawUrl)
            {
                case string url when new Regex(@"(ije|mor)$").IsMatch(url): // .mor or .ije
                    IJEMortality ije = new IJEMortality(deathRecord);
                    result = ije.ToString();
                    break;
                case string url when new Regex(@"(naaccr)$").IsMatch(url): // .naaccr
                    NAACCRRecord naaccr = new NAACCRRecord(deathRecord);
                    naaccr.ConsultNLPService();
                    result = naaccr.ToString();
                    break;
                case string url when new Regex(@"json$").IsMatch(url): // .json
                    result = deathRecord.ToJSON();
                    break;
                case string url when new Regex(@"xml$").IsMatch(url): // .xml
                    result = deathRecord.ToXML();
                    break;
            }

            return result;
        }

        public static string GetBodyContent(HttpListenerRequest request)
        {
            using (System.IO.Stream body = request.InputStream)
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

    }
}
