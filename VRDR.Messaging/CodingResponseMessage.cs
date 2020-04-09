using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;

namespace VRDR
{
    /// <summary>Class <c>CodingResponseMessage</c> conveys the coded cause of death, race and ethnicity of a decedent.</summary>
    public class CodingResponseMessage : BaseMessage
    {
        private Parameters parameters;

        /// <summary>Constructor that creates a response for the specified message.</summary>
        /// <param name="sourceMessage">the message to create a response for.</param>
        /// <param name="source">the endpoint identifier that the message will be sent from.</param>
        public CodingResponseMessage(BaseMessage sourceMessage, string source = "http://nchs.cdc.gov/vrdr_submission") : this(sourceMessage.MessageSource, source)
        {
        }

        /// <summary>
        /// Construct a CodingResponseMessage from a FHIR Bundle.
        /// </summary>
        /// <param name="messageBundle">a FHIR Bundle that will be used to initialize the CodingResponseMessage</param>
        /// <returns></returns>
        public CodingResponseMessage(Bundle messageBundle) : base(messageBundle)
        {
            parameters = findEntry<Parameters>(ResourceType.Parameters);
        }

        /// <summary>Constructor that creates a response for the specified message.</summary>
        /// <param name="destination">the endpoint identifier that the response message will be sent to.</param>
        /// <param name="source">the endpoint identifier that the response message will be sent from.</param>
        public CodingResponseMessage(string destination, string source = "http://nchs.cdc.gov/vrdr_submission") : base("vrdr_coding")
        {
            Header.Source.Endpoint = source;
            this.MessageDestination = destination;
            this.parameters = new Parameters();
            this.parameters.Id = Guid.NewGuid().ToString();
            MessageBundle.AddResourceEntry(this.parameters, "urn:uuid:" + this.parameters.Id);
            Header.Focus.Add(new ResourceReference(this.parameters.Id));
        }

        /// <summary>Jurisdiction-assigned death certificate number</summary>
        public string CertificateNumber
        {
            get
            {
                return parameters.GetSingleValue<FhirString>("cert_no")?.Value;
            }
            set
            {
                parameters.Remove("cert_no");
                parameters.Add("cert_no", new FhirString(value));
            }
        }

        /// <summary>Jurisdiction-assigned id</summary>
        public string StateIdentifier
        {
            get
            {
                return parameters.GetSingleValue<FhirString>("state_id")?.Value;
            }
            set
            {
                parameters.Remove("state_id");
                parameters.Add("state_id", new FhirString(value));
            }
        }

        /// <summary>Ethnicity codes</summary>
        public enum HispanicOrigin
        {
            /// <summary>Edited Hispanic Origin Code</summary>
            DETHNICE,
            /// <summary>Hispanic Code for Literal</summary>
            DETHNIC5C
        }

        /// <summary>Decedent ethnicity coding</summary>
        public Dictionary<HispanicOrigin, string> Ethnicity
        {
            get
            {
                var ethnicity = new Dictionary<HispanicOrigin, string>();
                Parameters.ParameterComponent ethnicityEntry = parameters.GetSingle("ethnicity");
                if (ethnicityEntry != null)
                {
                    foreach (Parameters.ParameterComponent entry in ethnicityEntry.Part)
                    {
                        Enum.TryParse(entry.Name, out HispanicOrigin code);
                        var coding = (Coding)entry.Value;
                        ethnicity.Add(code, coding.Code);
                    }
                }
                return ethnicity;
            }
            set
            {
                parameters.Remove("ethnicity");
                var list = new List<Tuple<string, Base>>();
                foreach (KeyValuePair<HispanicOrigin, string> item in value)
                {
                    var part = Tuple.Create(item.Key.ToString(), 
                        (Base)(new Coding("https://www.cdc.gov/nchs/data/dvs/HispanicCodeTitles.pdf", item.Value)));
                    list.Add(part);
                }
                parameters.Add("ethnicity", list);
            }
        }

        /// <summary>Race codes</summary>
        public enum RaceCode
        {
            /// <summary>First Edited Code</summary>
            RACE1E,
            /// <summary>Second Edited Code</summary>
            RACE2E,
            /// <summary>Third Edited Code</summary>
            RACE3E,
            /// <summary>Fourth Edited Code</summary>
            RACE4E,
            /// <summary>Fifth Edited Code</summary>
            RACE5E,
            /// <summary>Sixth Edited Code</summary>
            RACE6E,
            /// <summary>Seventh Edited Code</summary>
            RACE7E,
            /// <summary>Eighth Edited Code</summary>
            RACE8E,
            /// <summary>First Am. Ind Code</summary>
            RACE16C,
            /// <summary>Second Am. Ind Code</summary>
            RACE17C,
            /// <summary>First Other Asian Code</summary>
            RACE18C,
            /// <summary>Second Other Asian Code</summary>
            RACE19C,
            /// <summary>First Other PI Code</summary>
            RACE20C,
            /// <summary>Second Other Pi Code</summary>
            RACE21C,
            /// <summary>First Other Code</summary>
            RACE22C,
            /// <summary>Second Other Code</summary>
            RACE23C,
            /// <summary>Bridged Multiple Race Code</summary>
            RACEBRG
        }

        /// <summary>Decedent race coding</summary>
        public Dictionary<RaceCode, string> Race
        {
            get
            {
                var race = new Dictionary<RaceCode, string>();
                Parameters.ParameterComponent raceEntry = parameters.GetSingle("race");
                if (raceEntry != null)
                {
                    foreach (Parameters.ParameterComponent entry in raceEntry.Part)
                    {
                        Enum.TryParse(entry.Name, out RaceCode code);
                        var coding = (Coding)entry.Value;
                        race.Add(code, coding.Code);
                    }
                }
                return race;
            }
            set
            {
                parameters.Remove("race");
                var list = new List<Tuple<string, Base>>();
                foreach (KeyValuePair<RaceCode, string> item in value)
                {
                    var part = Tuple.Create(item.Key.ToString(), 
                        (Base)(new Coding("https://www.cdc.gov/nchs/data/dvs/RaceCodeList.pdf", item.Value)));
                    list.Add(part);
                }
                parameters.Add("race", list);
            }
        }

        /// <summary>Underlying cause of death code (ICD-10)</summary>
        public string UnderlyingCauseOfDeath
        {
            get
            {
                return parameters.GetSingleValue<Coding>("underlying_cause_of_death")?.Code;
            }
            set
            {
                parameters.Remove("underlying_cause_of_death");
                parameters.Add("underlying_cause_of_death", new Coding("http://hl7.org/fhir/ValueSet/icd-10", value));
            }
        }

        /// <summary>Record axis coding of cause of death</summary>
        public List<string> CauseOfDeathRecordAxis
        {
            get
            {
                var codes = new List<string>();
                Parameters.ParameterComponent axisEntry = parameters.GetSingle("record_cause_of_death");
                if (axisEntry != null)
                {
                    foreach (Parameters.ParameterComponent entry in axisEntry.Part)
                    {
                        var coding = (Coding)entry.Value;
                        codes.Add(coding.Code);
                    }
                }
                return codes;
            }
            set
            {
                parameters.Remove("record_cause_of_death");
                var list = new List<Tuple<string, Base>>();
                foreach (string code in value)
                {
                    var part = Tuple.Create("coding", 
                        (Base)(new Coding("https://www.cdc.gov/nchs/data/dvs/RaceCodeList.pdf", code)));
                    list.Add(part);
                }
                parameters.Add("record_cause_of_death", list);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public List<CauseOfDeathEntityAxisEntry> CauseOfDeathEntityAxis
        {
            get
            {
                var entityAxisEntries = new List<CauseOfDeathEntityAxisEntry>();
                foreach (Parameters.ParameterComponent part in parameters.Get("entity_axis_code"))
                {
                    string text = "";
                    string conditionId = "";
                    var codes = new List<string>();
                    foreach (Parameters.ParameterComponent childPart in part.Part)
                    {
                        if (childPart.Name == "text")
                        {
                            text = ((FhirString)childPart.Value).Value;
                        }
                        else if (childPart.Name == "condition")
                        {
                            conditionId = ((Id)childPart.Value).Value;
                        }
                        else if (childPart.Name == "coding")
                        {
                            var coding = (Coding)childPart.Value;
                            codes.Add(coding.Code);
                        }
                    }
                    var entry = new CauseOfDeathEntityAxisEntry(text, conditionId, codes);
                    entityAxisEntries.Add(entry);
                }
                return entityAxisEntries;
            }
            set
            {
                parameters.Remove("entity_axis_code");
                foreach (CauseOfDeathEntityAxisEntry entry in value)
                {
                    var entityAxisEntry = new List<Tuple<string, Base>>();
                    var part = Tuple.Create("text", (Base)(new FhirString(entry.DeathCertificateText)));
                    entityAxisEntry.Add(part);
                    part = Tuple.Create("condition", (Base)(new Id(entry.CauseOfDeathConditionId)));
                    entityAxisEntry.Add(part);
                    foreach (string code in entry.AssignedCodes)
                    {
                        part = Tuple.Create("coding", (Base)(new Coding("http://hl7.org/fhir/ValueSet/icd-10", code)));
                        entityAxisEntry.Add(part);
                    }
                    parameters.Add("entity_axis_code", entityAxisEntry);
                }
            }
        }
    }

    /// <summary>Class for structuring a cause of death entity axis entry</summary>
    public class CauseOfDeathEntityAxisEntry
    {
        /// <summary>A line of the original cause of death on the death certificate</summary>
        public readonly string DeathCertificateText;

        /// <summary>The identifier of the corresponding cause of death condition entry in the VRDR FHIR document</summary>
        public readonly string CauseOfDeathConditionId;

        /// <summary>The codes assigned for the DeathCertificateText</summary>
        public readonly List<string> AssignedCodes;

        /// <summary>Create a CauseOfDeathEntityAxisEntry with the specified death certificate text and cause of death
        /// condition id</summary>
        public CauseOfDeathEntityAxisEntry(string deathCertificateText, string causeOfDeathConditionId)
        {
            this.AssignedCodes = new List<string>();
            this.DeathCertificateText = deathCertificateText;
            this.CauseOfDeathConditionId = causeOfDeathConditionId;
        }

        /// <summary>Create a CauseOfDeathEntityAxisEntry with the specified death certificate text and cause of death
        /// condition id</summary>
        public CauseOfDeathEntityAxisEntry(string deathCertificateText, string causeOfDeathConditionId, List<string> codes)
        {
            this.AssignedCodes = codes;
            this.DeathCertificateText = deathCertificateText;
            this.CauseOfDeathConditionId = causeOfDeathConditionId;
        }
    }

    /// <summary>Class <c>CodingUpdateMessage</c> conveys an updated coded cause of death, race and ethnicity of a decedent.</summary>
    public class CodingUpdateMessage : CodingResponseMessage
    {
        /// <summary>Constructor that creates an update for the specified message.</summary>
        /// <param name="sourceMessage">the message to create a response for.</param>
        /// <param name="source">the endpoint identifier that the message will be sent from.</param>
        public CodingUpdateMessage(BaseMessage sourceMessage, string source = "http://nchs.cdc.gov/vrdr_submission") : this(sourceMessage.MessageSource, source)
        {
        }

        /// <summary>
        /// Construct a CodingResponseMessage from a FHIR Bundle.
        /// </summary>
        /// <param name="messageBundle">a FHIR Bundle that will be used to initialize the CodingResponseMessage</param>
        /// <returns></returns>
        public CodingUpdateMessage(Bundle messageBundle) : base(messageBundle)
        {
        }

        /// <summary>Constructor that creates a response for the specified message.</summary>
        /// <param name="destination">the endpoint identifier that the response message will be sent to.</param>
        /// <param name="source">the endpoint identifier that the response message will be sent from.</param>
        public CodingUpdateMessage(string destination, string source = "http://nchs.cdc.gov/vrdr_submission") : base(destination, source)
        {
            MessageType = "vrdr_coding_update";
        }
    }
}
