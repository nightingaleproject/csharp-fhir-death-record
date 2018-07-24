﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.ElementModel;
using Hl7.FhirPath;
using FhirDeathRecord;

namespace csharp_fhir_death_record
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var path in args)
            {
                ReadFile(path);
            }
        }

        private static void ReadFile(string path)
        {
            if (File.Exists(path))
            {
                Console.WriteLine($"Reading file '{path}'");
                string json = File.ReadAllText(path);
                DeathRecord deathRecord = new DeathRecord(json);

                // Decedent Information
                Console.WriteLine($"\tGiven Name: {deathRecord.GivenName}");
                Console.WriteLine($"\tLast Name: {deathRecord.LastName}");
                Console.WriteLine($"\tSSN: {deathRecord.SSN}");

                // Certifier Information
                Console.WriteLine($"\tCertifier Given Name: {deathRecord.CertifierGivenName}");
                Console.WriteLine($"\tCertifier Last Name: {deathRecord.CertifierLastName}");

                // Conditions
                Tuple<string, string>[] causes = deathRecord.CausesOfDeath;
                foreach (var cause in causes)
                {
                    Console.WriteLine($"\tCause: {cause.Item1}, Onset: {cause.Item2}");
                }
                Console.WriteLine($"\tContributing Conditions: {deathRecord.ContributingConditions}");

                // Observations
                Console.WriteLine($"\tAutopsy Performed: {deathRecord.AutopsyPerformed}");
                Console.WriteLine($"\tAutopsy Results Available: {deathRecord.AutopsyResultsAvailable}");
                Console.WriteLine($"\tManner of Death: {deathRecord.MannerOfDeath}");
                Console.WriteLine($"\tTobacco Use Contributed to Death: {deathRecord.TobaccoUseContributedToDeath}");
            }
            else
            {
                Console.WriteLine($"Error: File '{path}' does not exist");
            }
        }
    }
}
