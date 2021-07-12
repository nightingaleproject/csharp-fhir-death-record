using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.FhirPath;
using Newtonsoft.Json;

namespace VRDR
{
    /// <summary>Class <c>DeathRecord</c> models a FHIR Vital Records Death Reporting (VRDR) Death
    /// Record. This class was designed to help consume and produce death records that follow the
    /// HL7 FHIR Vital Records Death Reporting Implementation Guide, as described at:
    /// http://hl7.org/fhir/us/vrdr and https://github.com/hl7/vrdr.
    /// </summary>
    public class DeathRecord
    {
        /// <summary>Mortality data for code translations.</summary>
        private MortalityData MortalityData = MortalityData.Instance;

        /// <summary>Useful for navigating around the FHIR Bundle using FHIRPaths.</summary>
        private ITypedElement Navigator;

        /// <summary>Bundle that contains the death record.</summary>
        private Bundle Bundle;

        /// <summary>Composition that described what the Bundle is, as well as keeping references to its contents.</summary>
        private Composition Composition;

        /// <summary>DocumentReference that is used to specify state local death record identifier.</summary>
        private DocumentReference StateDocumentReference;

        /// <summary>The Decedent.</summary>
        private Patient Decedent;

        /// <summary>The Pronouncer of death.</summary>
        private Practitioner Pronouncer;

        /// <summary>The Certifier.</summary>
        private Practitioner Certifier;

        /// <summary>The Mortician.</summary>
        private Practitioner Mortician;

        /// <summary>The Certification.</summary>
        private Procedure DeathCertification;

        /// <summary>The Interested Party.</summary>
        private Organization InterestedParty;

        /// <summary>The Manner of Death Observation.</summary>
        private Observation MannerOfDeath;

        /// <summary>Condition Contributing to Death.</summary>
        private Condition ConditionContributingToDeath;

        /// <summary>Cause Of Death Condition Line A (#1).</summary>
        private Condition CauseOfDeathConditionA;

        /// <summary>Cause Of Death Condition Line B (#2).</summary>
        private Condition CauseOfDeathConditionB;

        /// <summary>Cause Of Death Condition Line C (#3).</summary>
        private Condition CauseOfDeathConditionC;

        /// <summary>Cause Of Death Condition Line D (#4).</summary>
        private Condition CauseOfDeathConditionD;

        /// <summary>Cause Of Death Condition Line E (#5).</summary>
        private Condition CauseOfDeathConditionE;

        /// <summary>Cause Of Death Condition Line F (#6).</summary>
        private Condition CauseOfDeathConditionF;

        /// <summary>Cause Of Death Condition Line G (#7).</summary>
        private Condition CauseOfDeathConditionG;

        /// <summary>Cause Of Death Condition Line H (#8).</summary>
        private Condition CauseOfDeathConditionH;

        /// <summary>Cause Of Death Condition Line I (#9).</summary>
        private Condition CauseOfDeathConditionI;

        /// <summary>Cause Of Death Condition Line J (#10).</summary>
        private Condition CauseOfDeathConditionJ;

        /// <summary>Cause Of Death Condition Pathway.</summary>
        private List CauseOfDeathConditionPathway;

        /// <summary>Decedent's Father.</summary>
        private RelatedPerson Father;

        /// <summary>Decedent's Mother.</summary>
        private RelatedPerson Mother;

        /// <summary>Decedent's Spouse.</summary>
        private RelatedPerson Spouse;

        /// <summary>Decedent Education Level.</summary>
        private Observation DecedentEducationLevel;

        /// <summary>Birth Record Identifier.</summary>
        private Observation BirthRecordIdentifier;

        /// <summary>Usual Work.</summary>
        private Observation UsualWork;

        /// <summary>Whether the decedent served in the military</summary>
        private Observation MilitaryServiceObs;

        /// <summary>The Funeral Home.</summary>
        private Organization FuneralHome;

        /// <summary>The Funeral Home Director.</summary>
        private PractitionerRole FuneralHomeDirector;

        /// <summary>Disposition Location.</summary>
        private Location DispositionLocation;

        /// <summary>Disposition Method.</summary>
        private Observation DispositionMethod;

        /// <summary>Autopsy Performed.</summary>
        private Observation AutopsyPerformed;

        /// <summary>Age At Death.</summary>
        private Observation AgeAtDeathObs;

        /// <summary>Decedent Pregnancy Status.</summary>
        private Observation PregnancyObs;

        /// <summary>Examiner Contacted.</summary>
        private Observation ExaminerContactedObs;

        /// <summary>Tobacco Use Contributed To Death.</summary>
        private Observation TobaccoUseObs;

        /// <summary>Transportation Role.</summary>
        private Observation TransportationRoleObs;

        /// <summary>Injury Location.</summary>
        private Location InjuryLocationLoc;

        /// <summary>Injury Incident.</summary>
        private Observation InjuryIncidentObs;

        /// <summary>Death Location.</summary>
        private Location DeathLocationLoc;

        /// <summary>Date Of Death.</summary>
        private Observation DeathDateObs;
        private const string  locationJurisdictionExtPath = "jurisdictionID"; // in 1.1 will be"http://hl7.org/fhir/us/vrdr/StructureDefinition/Location-Jurisdiction-Id"


        
        /// <summary>Default constructor that creates a new, empty DeathRecord.</summary>
        public DeathRecord()
        {
            // Start with an empty Bundle.
            Bundle = new Bundle();
            Bundle.Id = Guid.NewGuid().ToString();
            Bundle.Type = Bundle.BundleType.Document; // By default, Bundle type is "document".
            Bundle.Meta = new Meta();
            string[] bundle_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Certificate-Document" };
            Bundle.Timestamp = DateTime.Now;
            Bundle.Meta.Profile = bundle_profile;

            // Start with an empty decedent.
            Decedent = new Patient();
            Decedent.Id = Guid.NewGuid().ToString();
            Decedent.Meta = new Meta();
            string[] decedent_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent" };
            Decedent.Meta.Profile = decedent_profile;

            // Start with an empty certifier.
            Certifier = new Practitioner();
            Certifier.Id = Guid.NewGuid().ToString();
            Certifier.Meta = new Meta();
            string[] certifier_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Certifier" };
            Certifier.Meta.Profile = certifier_profile;

            // Start with an empty pronouncer.
            Pronouncer = new Practitioner();
            Pronouncer.Id = Guid.NewGuid().ToString();
            Pronouncer.Meta = new Meta();
            string[] pronouncer_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Pronouncement-Performer" };
            Pronouncer.Meta.Profile = pronouncer_profile;

            // Start with an empty mortician.
            InitializeMorticianIfNull();

            // Start with an empty certification.
            DeathCertification = new Procedure();
            DeathCertification.Id = Guid.NewGuid().ToString();
            DeathCertification.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
            DeathCertification.Meta = new Meta();
            string[] deathcertification_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Certification" };
            DeathCertification.Meta.Profile = deathcertification_profile;
            DeathCertification.Status = EventStatus.Completed;
            DeathCertification.Category = new CodeableConcept("http://snomed.info/sct", "103693007", "Diagnostic procedure", null);
            DeathCertification.Code = new CodeableConcept("http://snomed.info/sct", "308646001", "Death certification", null);

            // Start with an empty interested party.
            InterestedParty = new Organization();
            InterestedParty.Id = Guid.NewGuid().ToString();
            InterestedParty.Meta = new Meta();
            string[] interestedparty_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Interested-Party" };
            InterestedParty.Meta.Profile = interestedparty_profile;
            InterestedParty.Active = true;

            // Start with an empty funeral home.
            FuneralHome = new Organization();
            FuneralHome.Id = Guid.NewGuid().ToString();
            FuneralHome.Meta = new Meta();
            string[] funeralhome_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Funeral-Home" };
            FuneralHome.Meta.Profile = funeralhome_profile;
            FuneralHome.Active = true;
            FuneralHome.Type.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/organization-type", "bus", "Non-Healthcare Business or Corporation", null));

            // FuneralHomeLicensee Points to Mortician and FuneralHome
            FuneralHomeDirector = new PractitionerRole();
            FuneralHomeDirector.Id = Guid.NewGuid().ToString();
            FuneralHomeDirector.Meta = new Meta();
            string[] funeralhomedirector_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Funeral-Service-Licensee" };
            FuneralHomeDirector.Meta.Profile = funeralhomedirector_profile;
            FuneralHomeDirector.Practitioner = new ResourceReference("urn:uuid:" + Mortician.Id);
            FuneralHomeDirector.Organization = new ResourceReference("urn:uuid:" + FuneralHome.Id);

            // Location of Disposition
            DispositionLocation = new Location();
            DispositionLocation.Id = Guid.NewGuid().ToString();
            DispositionLocation.Meta = new Meta();
            string[] dispositionlocation_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Disposition-Location" };
            DispositionLocation.Meta.Profile = dispositionlocation_profile;
            Coding pt = new Coding("http://terminology.hl7.org/CodeSystem/location-physical-type", "si", "Site");
            DispositionLocation.PhysicalType = new CodeableConcept();
            DispositionLocation.PhysicalType.Coding.Add(pt);

            // Add Composition to bundle. As the record is filled out, new entries will be added to this element.
            Composition = new Composition();
            Composition.Id = Guid.NewGuid().ToString();
            Composition.Status = CompositionStatus.Final;
            Composition.Meta = new Meta();
            string[] composition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Certificate" };
            Composition.Meta.Profile = composition_profile;
            Composition.Type = new CodeableConcept("http://loinc.org", "64297-5", "Death certificate", null);
            Composition.Section.Add(new Composition.SectionComponent());
            Composition.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
            Composition.Author.Add(new ResourceReference("urn:uuid:" + Certifier.Id));
            Composition.Title = "Death Certificate";
            Composition.Attester.Add(new Composition.AttesterComponent());
            Composition.Attester.First().Party = new ResourceReference("urn:uuid:" + Certifier.Id);
            Composition.Attester.First().ModeElement = new Code<Hl7.Fhir.Model.Composition.CompositionAttestationMode>(Hl7.Fhir.Model.Composition.CompositionAttestationMode.Legal);
            Hl7.Fhir.Model.Composition.EventComponent eventComponent = new Hl7.Fhir.Model.Composition.EventComponent();
            eventComponent.Code.Add(new CodeableConcept("http://snomed.info/sct", "103693007", "Diagnostic procedure (procedure)", null));
            eventComponent.Detail.Add(new ResourceReference("urn:uuid:" + DeathCertification.Id));
            Composition.Event.Add(eventComponent);
            Bundle.AddResourceEntry(Composition, "urn:uuid:" + Composition.Id);

            // Start with an empty Cause of Death Pathway
            CauseOfDeathConditionPathway = new List();
            CauseOfDeathConditionPathway.Id = Guid.NewGuid().ToString();
            CauseOfDeathConditionPathway.Meta = new Meta();
            string[] causeofdeathconditionpathway_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-of-Death-Pathway" };
            CauseOfDeathConditionPathway.Meta.Profile = causeofdeathconditionpathway_profile;
            CauseOfDeathConditionPathway.Status = List.ListStatus.Current;
            CauseOfDeathConditionPathway.Mode = Hl7.Fhir.Model.ListMode.Snapshot;
            CauseOfDeathConditionPathway.Source = new ResourceReference("urn:uuid:" + Certifier.Id);
            CauseOfDeathConditionPathway.OrderedBy = new CodeableConcept("http://terminology.hl7.org/CodeSystem/list-order", "priority", "Sorted by Priority", null);

            // Add references back to the Decedent, Certifier, Certification, etc.
            AddReferenceToComposition(Decedent.Id);
            AddReferenceToComposition(Certifier.Id);
            AddReferenceToComposition(Pronouncer.Id);
            AddReferenceToComposition(Mortician.Id);
            AddReferenceToComposition(DeathCertification.Id);
            AddReferenceToComposition(InterestedParty.Id);
            AddReferenceToComposition(FuneralHome.Id);
            AddReferenceToComposition(FuneralHomeDirector.Id);
            AddReferenceToComposition(CauseOfDeathConditionPathway.Id);
            AddReferenceToComposition(DispositionLocation.Id);
            Bundle.AddResourceEntry(Decedent, "urn:uuid:" + Decedent.Id);
            Bundle.AddResourceEntry(Certifier, "urn:uuid:" + Certifier.Id);
            Bundle.AddResourceEntry(Pronouncer, "urn:uuid:" + Pronouncer.Id);
            Bundle.AddResourceEntry(Mortician, "urn:uuid:" + Mortician.Id);
            Bundle.AddResourceEntry(DeathCertification, "urn:uuid:" + DeathCertification.Id);
            Bundle.AddResourceEntry(InterestedParty, "urn:uuid:" + InterestedParty.Id);
            Bundle.AddResourceEntry(FuneralHome, "urn:uuid:" + FuneralHome.Id);
            Bundle.AddResourceEntry(FuneralHomeDirector, "urn:uuid:" + FuneralHomeDirector.Id);
            Bundle.AddResourceEntry(CauseOfDeathConditionPathway, "urn:uuid:" + CauseOfDeathConditionPathway.Id);
            Bundle.AddResourceEntry(DispositionLocation, "urn:uuid:" + DispositionLocation.Id);

            // Create a Navigator for this new death record.
            Navigator = Bundle.ToTypedElement();

            UpdateBundleIdentifier();
        }

        /// <summary>Constructor that takes a string that represents a FHIR Death Record in either XML or JSON format.</summary>
        /// <param name="record">represents a FHIR Death Record in either XML or JSON format.</param>
        /// <param name="permissive">if the parser should be permissive when parsing the given string</param>
        /// <exception cref="ArgumentException">Record is neither valid XML nor JSON.</exception>
        public DeathRecord(string record, bool permissive = false)
        {
            ParserSettings parserSettings = new ParserSettings { AcceptUnknownMembers = permissive,
                                                                 AllowUnrecognizedEnums = permissive,
                                                                 PermissiveParsing = permissive };
            // XML?
            Boolean maybeXML = record.TrimStart().StartsWith("<");
            Boolean maybeJSON = record.TrimStart().StartsWith("{");
            if (!String.IsNullOrEmpty(record) && (maybeXML || maybeJSON))
            {
                // Grab all errors found by visiting all nodes and report if not permissive
                if (!permissive)
                {
                    List<string> entries = new List<string>();
                    ISourceNode node = null;
                    if (maybeXML) {
                        node = FhirXmlNode.Parse(record, new FhirXmlParsingSettings { PermissiveParsing = permissive });
                    }
                    else {
                        node = FhirJsonNode.Parse(record, "Bundle", new FhirJsonParsingSettings { PermissiveParsing = permissive });
                    }
                    foreach (Hl7.Fhir.Utility.ExceptionNotification problem in node.VisitAndCatch())
                    {
                        entries.Add(problem.Message);
                    }
                    if (entries.Count > 0)
                    {
                        throw new System.ArgumentException(String.Join("; ", entries).TrimEnd());
                    }
                }
                // Try Parse
                try
                {
                    if(maybeXML) {
                        FhirXmlParser parser = new FhirXmlParser(parserSettings);
                        Bundle = parser.Parse<Bundle>(record);
                    }
                    else {
                        FhirJsonParser parser = new FhirJsonParser(parserSettings);
                        Bundle = parser.Parse<Bundle>(record);
                    }
                    Navigator = Bundle.ToTypedElement();
                }
                catch (Exception e)
                {
                    throw new System.ArgumentException(e.Message);
                }
            }
            // Fill out class instance references
            if (Navigator != null)
            {
                RestoreReferences();
            }
            else
            {
                throw new System.ArgumentException("The given input does not appear to be a valid XML or JSON FHIR record.");
            }
        }

        /// <summary>Constructor that takes a FHIR Bundle that represents a FHIR Death Record.</summary>
        /// <param name="bundle">represents a FHIR Bundle.</param>
        /// <exception cref="ArgumentException">Record is invalid.</exception>
        public DeathRecord(Bundle bundle)
        {
            Bundle = bundle;
            Navigator = Bundle.ToTypedElement();
            RestoreReferences();
        }

        /// <summary>Helper method to return a XML string representation of this Death Record.</summary>
        /// <returns>a string representation of this Death Record in XML format</returns>
        public string ToXML()
        {
            return Bundle.ToXml();
        }

        /// <summary>Helper method to return a XML string representation of this Death Record.</summary>
        /// <returns>a string representation of this Death Record in XML format</returns>
        public string ToXml()
        {
            return Bundle.ToXml();
        }

        /// <summary>Helper method to return a JSON string representation of this Death Record.</summary>
        /// <returns>a string representation of this Death Record in JSON format</returns>
        public string ToJSON()
        {
            return Bundle.ToJson();
        }

        /// <summary>Helper method to return a JSON string representation of this Death Record.</summary>
        /// <returns>a string representation of this Death Record in JSON format</returns>
        public string ToJson()
        {
            return Bundle.ToJson();
        }

        /// <summary>Helper method to return an ITypedElement of the record bundle.</summary>
        /// <returns>an ITypedElement of the record bundle</returns>
        public ITypedElement GetITypedElement()
        {
            return Navigator;
        }

        /// <summary>Helper method to return a the bundle.</summary>
        /// <returns>a FHIR Bundle</returns>
        public Bundle GetBundle()
        {
            return Bundle;
        }

        /////////////////////////////////////////////////////////////////////////////////
        //
        // Record Properties: Death Certification
        //
        /////////////////////////////////////////////////////////////////////////////////

        /// <summary>Death Record Identifier, Death Certificate Number.</summary>
        /// <value>a record identification string.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.Identifier = "42";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Death Certificate Number: {ExampleDeathRecord.Identifier}");</para>
        /// </example>
        [Property("Identifier", Property.Types.String, "Death Certification", "Death Certificate Number.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Certification.html", true, 3)]
        [FHIRPath("Bundle.entry.resource.where($this is Procedure).where(code.coding.code='308646001')", "identifier")]
        public string Identifier
        {
            get
            {
                Identifier id = DeathCertification?.Identifier?.FirstOrDefault(i => i.Value != null && i.Value.Length > 0);
                return id == null ? null : id.Value;
            }
            set
            {
                DeathCertification.Identifier.Clear();
                Identifier identifier = new Identifier();
                identifier.Value = value;
                DeathCertification.Identifier.Add(identifier);
                UpdateBundleIdentifier();
            }
        }

        /// <summary>Update the bundle identifier from the component fields.</summary>
        private void UpdateBundleIdentifier()
        {
            uint certificateNumber = 0;
            UInt32.TryParse(this.Identifier, out certificateNumber);
            uint deathYear = 0;
            if (this.DateOfDeath != null)
            {
                if (this.DateOfDeath.Length >= 4)
                {
                    UInt32.TryParse(this.DateOfDeath.Substring(0,4), out deathYear);
                }
            }
            var jurisdictionId = this.DeathLocationAddress?["addressState"];
            if (jurisdictionId == null || jurisdictionId.Trim().Length < 2)
            {
                jurisdictionId = "XX";
            }
            else
            {
                jurisdictionId = jurisdictionId.Trim().Substring(0, 2).ToUpper();
            }
            this.BundleIdentifier = $"{deathYear.ToString("D4")}{jurisdictionId}{certificateNumber.ToString("D6")}";
        }

        /// <summary>Death Record Bundle Identifier, NCHS identifier.</summary>
        /// <value>a record bundle identification string.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.BundleIdentifier = "42";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"NCHS identifier: {ExampleDeathRecord.BundleIdentifier}");</para>
        /// </example>
        [Property("Bundle Identifier", Property.Types.String, "Death Certification", "NCHS identifier.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Certificate-Document.html", true, 4)]
        [FHIRPath("Bundle", "identifier")]
        public string BundleIdentifier
        {
            get
            {
                if (Bundle != null && Bundle.Identifier != null)
                {
                    return Bundle.Identifier.Value;
                }
                return null;
            }
            private set
            {
                Identifier identifier = new Identifier();
                identifier.Value = value;
                identifier.System = "http://nchs.cdc.gov/vrdr_id";
                Bundle.Identifier = identifier;
            }
        }

        /// <summary>State Local Identifier.</summary>
        /// <value>a state local identification string.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.StateLocalIdentifier = "4242123";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"State local identifier: {ExampleDeathRecord.StateLocalIdentifier}");</para>
        /// </example>
        [Property("State Local Identifier", Property.Types.String, "Decedent Demographics", "State Local Identifier.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Certificate-Reference.html", true, 4)]
        [FHIRPath("Bundle.entry.resource.where($this is DocumentReference).where(type.coding.code='64297-5')", "")]
        public string StateLocalIdentifier
        {
            get
            {
                // return first non-null/empty identifier value or null if none found
                Identifier identifier = StateDocumentReference?.Identifier?.FirstOrDefault(i => i.Value != null && i.Value.Length > 0);
                return identifier==null ? null : identifier.Value;
            }
            set
            {
                if (StateDocumentReference == null)
                {
                    StateDocumentReference = new DocumentReference();
                    StateDocumentReference.Id = Guid.NewGuid().ToString();
                    StateDocumentReference.Meta = new Meta();
                    string[] profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Certificate-Reference" };
                    StateDocumentReference.Meta.Profile = profile;
                    StateDocumentReference.Status = DocumentReferenceStatus.Current;
                    StateDocumentReference.Type = new CodeableConcept("http://loinc.org", "64297-5", "Death certificate", null);
                    Identifier identifier = new Identifier();
                    identifier.Value = value;
                    StateDocumentReference.Identifier.Add(identifier);
                    if (InterestedParty != null)
                    {
                        StateDocumentReference.Author.Add(new ResourceReference("urn:uuid:" + InterestedParty.Id));
                    }
                    StateDocumentReference.Date = new DateTimeOffset(DateTime.Now);
                    Attachment attachment = new Attachment();
                    attachment.Url = "urn:uuid:" + Bundle.Id;
                    DocumentReference.ContentComponent content = new DocumentReference.ContentComponent();
                    content.Attachment = attachment;
                    StateDocumentReference.Content.Add(content);
                    AddReferenceToComposition(StateDocumentReference.Id);
                    Bundle.AddResourceEntry(StateDocumentReference, "urn:uuid:" + StateDocumentReference.Id);
                }
                else
                {
                    StateDocumentReference.Identifier.Clear();
                    Identifier identifier = new Identifier();
                    identifier.Value = value;
                    StateDocumentReference.Identifier.Add(identifier);
                }
            }
        }


        /// <summary>Certified time.</summary>
        /// <value>time when the record was certified.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.CertifiedTime = "2019-01-29T16:48:06-05:00";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Certified at: {ExampleDeathRecord.CertifiedTime}");</para>
        /// </example>
        [Property("Certified Time", Property.Types.StringDateTime, "Death Certification", "Certified time (i.e. certifier date signed).", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Certification.html", false, 12)]
        [FHIRPath("Bundle.entry.resource.where($this is Procedure).where(code.coding.code='308646001')", "")]
        public string CertifiedTime
        {
            get
            {
                if (DeathCertification != null && DeathCertification.Performed != null)
                {
                    return Convert.ToString(DeathCertification.Performed);
                } else if (Composition.Attester != null && Composition.Attester.FirstOrDefault() != null && Composition.Attester.First().Time != null)
                {
                    return Composition.Attester.First().Time;
                }
                return null;
            }
            set
            {
                if (DeathCertification == null)
                {
                    DeathCertification = new Procedure();
                    DeathCertification.Id = Guid.NewGuid().ToString();
                    DeathCertification.Meta = new Meta();
                    string[] deathcertification_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Certification" };
                    DeathCertification.Meta.Profile = deathcertification_profile;
                    DeathCertification.Status = EventStatus.Completed;
                    DeathCertification.Category = new CodeableConcept("http://snomed.info/sct", "103693007", "Diagnostic procedure", null);
                    DeathCertification.Code = new CodeableConcept("http://snomed.info/sct", "308646001", "Death certification", null);
                    AddReferenceToComposition(DeathCertification.Id);
                    Bundle.AddResourceEntry(DeathCertification, "urn:uuid:" + DeathCertification.Id);
                    Composition.Attester.First().Time = value;
                    DeathCertification.Performed = new FhirDateTime(value);
                }
                else
                {
                    Composition.Attester.First().Time = value;
                    DeathCertification.Performed = new FhirDateTime(value);
                }
            }
        }

        /// <summary>Registered time.</summary>
        /// <value>time when the record was registered.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.RegisteredTime = "2019-01-29T16:48:06-05:00";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Registered at: {ExampleDeathRecord.RegisteredTime}");</para>
        /// </example>
        [Property("Registered Date/Time", Property.Types.StringDateTime, "Death Certification", "Date/Time of record registration.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Certificate.html", true, 13)]
        [FHIRPath("Bundle.entry.resource.where($this is Composition)", "date")]
        public string RegisteredTime
        {
            get
            {
                return Composition.Date;
            }
            set
            {
                Composition.Date = value;
            }
        }

        /// <summary>Certification Role.</summary>
        /// <value>the role/qualification of the person who certified the death. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; role = new Dictionary&lt;string, string&gt;();</para>
        /// <para>role.Add("code", "76899008");</para>
        /// <para>role.Add("system", "http://snomed.info/sct");</para>
        /// <para>role.Add("display", "Infectious diseases physician");</para>
        /// <para>ExampleDeathRecord.CertificationRole = role;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Certification Role: {ExampleDeathRecord.CertificationRole['display']}");</para>
        /// </example>
        [Property("Certification Role", Property.Types.Dictionary, "Death Certification", "Certification Role.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Certification.html", true, 4)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Procedure).where(code.coding.code='308646001')", "performer")]
        public Dictionary<string, string> CertificationRole
        {
            get
            {
                if (DeathCertification == null)
                {
                    return EmptyCodeDict();
                }
                Hl7.Fhir.Model.Procedure.PerformerComponent performer = DeathCertification.Performer.FirstOrDefault();
                if (performer != null && performer.Function != null)
                {
                    return CodeableConceptToDict(performer.Function);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (DeathCertification == null)
                {
                    DeathCertification = new Procedure();
                    DeathCertification.Id = Guid.NewGuid().ToString();
                    DeathCertification.Meta = new Meta();
                    string[] deathcertification_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Certification" };
                    DeathCertification.Meta.Profile = deathcertification_profile;
                    DeathCertification.Status = EventStatus.Completed;
                    DeathCertification.Category = new CodeableConcept("http://snomed.info/sct", "103693007", "Diagnostic procedure", null);
                    DeathCertification.Code = new CodeableConcept("http://snomed.info/sct", "308646001", "Death certification", null);
                    AddReferenceToComposition(DeathCertification.Id);
                    Bundle.AddResourceEntry(DeathCertification, "urn:uuid:" + DeathCertification.Id);
                    Hl7.Fhir.Model.Procedure.PerformerComponent performer = new Hl7.Fhir.Model.Procedure.PerformerComponent();
                    performer.Function = DictToCodeableConcept(value);
                    performer.Actor = new ResourceReference("urn:uuid:" + Certifier.Id);
                    DeathCertification.Performer.Clear();
                    DeathCertification.Performer.Add(performer);
                }
                else
                {
                    Hl7.Fhir.Model.Procedure.PerformerComponent performer = new Hl7.Fhir.Model.Procedure.PerformerComponent();
                    performer.Function = DictToCodeableConcept(value);
                    performer.Actor = new ResourceReference("urn:uuid:" + Certifier.Id);
                    DeathCertification.Performer.Clear();
                    DeathCertification.Performer.Add(performer);
                }
            }
        }

        /// <summary>Interested Party Identifier.</summary>
        /// <value>the interested party identification. A Dictionary representing a system (e.g. NPI) and a value, containing the following key/value pairs:
        /// <para>"system" - the identifier system, e.g. US NPI</para>
        /// <para>"value" - the identifier value, e.g. US NPI number</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; identifier = new Dictionary&lt;string, string&gt;();</para>
        /// <para>identifier.Add("system", "http://hl7.org/fhir/sid/us-npi");</para>
        /// <para>identifier.Add("value", "1234567890");</para>
        /// <para>ExampleDeathRecord.InterestedPartyIdentifier = identifier;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tPronouncer Identifier: {ExampleDeathRecord.InterestedPartyIdentifier['value']}");</para>
        /// </example>
        [Property("Interested Party Identifier", Property.Types.Dictionary, "Death Certification", "Interested Party Identifier.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Interested-Party.html", false, 101)]
        [PropertyParam("system", "The identifier system.")]
        [PropertyParam("value", "The identifier value.")]
        [FHIRPath("Bundle.entry.resource.where($this is Organization).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Interested-Party')", "identifier")]
        public Dictionary<string, string> InterestedPartyIdentifier
        {
            get
            {
                Identifier identifier = InterestedParty?.Identifier?.FirstOrDefault();
                var result = new Dictionary<string, string>();
                if (identifier != null)
                {
                    result["system"] = identifier.System;
                    result["value"] = identifier.Value;
                }
                return result;
            }
            set
            {
                if (InterestedParty == null)
                {
                    InterestedParty = new Organization();
                    InterestedParty.Id = Guid.NewGuid().ToString();
                    InterestedParty.Meta = new Meta();
                    string[] interestedparty_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Interested-Party" };
                    InterestedParty.Meta.Profile = interestedparty_profile;
                    InterestedParty.Active = true;
                    AddReferenceToComposition(InterestedParty.Id);
                    Bundle.AddResourceEntry(InterestedParty, "urn:uuid:" + InterestedParty.Id);
                }
                if (InterestedParty.Identifier.Count > 0)
                {
                    InterestedParty.Identifier.Clear();
                }

                Identifier identifier = new Identifier();
                identifier.System = value["system"];
                identifier.Value = value["value"];
                InterestedParty.Identifier.Add(identifier);
            }
        }

        /// <summary>Interested Party Name.</summary>
        /// <value>an interested party name string.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.InterestedPartyName = "Fourty Two";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interested Party name: {ExampleDeathRecord.InterestedPartyName}");</para>
        /// </example>
        [Property("Interested Party Name", Property.Types.String, "Death Certification", "Interested Party Name.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Interested-Party.html", false, 101)]
        [FHIRPath("Bundle.entry.resource.where($this is Organization).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Interested-Party')", "name")]
        public string InterestedPartyName
        {
            get
            {
                if (InterestedParty != null)
                {
                    return InterestedParty.Name;
                }
                return null;
            }
            set
            {
                if (InterestedParty == null)
                {
                    InterestedParty = new Organization();
                    InterestedParty.Id = Guid.NewGuid().ToString();
                    InterestedParty.Meta = new Meta();
                    string[] interestedparty_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Interested-Party" };
                    InterestedParty.Meta.Profile = interestedparty_profile;
                    InterestedParty.Active = true;
                    InterestedParty.Name = value;
                    AddReferenceToComposition(InterestedParty.Id);
                    Bundle.AddResourceEntry(InterestedParty, "urn:uuid:" + InterestedParty.Id);
                }
                else
                {
                    InterestedParty.Name = value;
                }
            }
        }

        /// <summary>Interested Party's address.</summary>
        /// <value>Interested Party's address. A Dictionary representing an address, containing the following key/value pairs:
        /// <para>"addressLine1" - address, line one</para>
        /// <para>"addressLine2" - address, line two</para>
        /// <para>"addressCity" - address, city</para>
        /// <para>"addressCounty" - address, county</para>
        /// <para>"addressState" - address, state</para>
        /// <para>"addressZip" - address, zip</para>
        /// <para>"addressCountry" - address, country</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; address = new Dictionary&lt;string, string&gt;();</para>
        /// <para>address.Add("addressLine1", "9 Example Street");</para>
        /// <para>address.Add("addressLine2", "Line 2");</para>
        /// <para>address.Add("addressCity", "Bedford");</para>
        /// <para>address.Add("addressCounty", "Middlesex");</para>
        /// <para>address.Add("addressState", "MA");</para>
        /// <para>address.Add("addressZip", "12345");</para>
        /// <para>address.Add("addressCountry", "United States");</para>
        /// <para>ExampleDeathRecord.InterestedPartyAddress = address;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interested Party state: {ExampleDeathRecord.InterestedPartyAddress["addressState"]}");</para>
        /// </example>
        [Property("Interested Party Address", Property.Types.Dictionary, "Death Certification", "Interested Party's address.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Interested-Party.html", false, 101)]
        [PropertyParam("addressLine1", "address, line one")]
        [PropertyParam("addressLine2", "address, line two")]
        [PropertyParam("addressCity", "address, city")]
        [PropertyParam("addressCounty", "address, county")]
        [PropertyParam("addressState", "address, state")]
        [PropertyParam("addressZip", "address, zip")]
        [PropertyParam("addressCountry", "address, country")]
        [FHIRPath("Bundle.entry.resource.where($this is Organization).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Interested-Party')", "address")]
        public Dictionary<string, string> InterestedPartyAddress
        {
            get
            {
                if (InterestedParty != null && InterestedParty.Address != null && InterestedParty.Address.Count() > 0)
                {
                    return AddressToDict(InterestedParty.Address.First());
                }
                return EmptyAddrDict();
            }
            set
            {
                InterestedParty.Address.Clear();
                InterestedParty.Address.Add(DictToAddress(value));
            }
        }

        /// <summary>Interested Party Type.</summary>
        /// <value>the interested party type. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; type = new Dictionary&lt;string, string&gt;();</para>
        /// <para>type.Add("code", "prov");</para>
        /// <para>type.Add("system", "http://terminology.hl7.org/CodeSystem/organization-type");</para>
        /// <para>type.Add("display", "Healthcare Provider");</para>
        /// <para>ExampleDeathRecord.InterestedPartyType = type;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interested Party Type: {ExampleDeathRecord.InterestedPartyType['display']}");</para>
        /// </example>
        [Property("Interested Party Type", Property.Types.Dictionary, "Death Certification", "Interested Party Type.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Interested-Party.html", false, 101)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Organization).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Interested-Party')", "type")]
        public Dictionary<string, string> InterestedPartyType
        {
            get
            {
                if (InterestedParty != null && InterestedParty.Type != null && InterestedParty.Type.Count() > 0)
                {
                    return CodeableConceptToDict(InterestedParty.Type.First());
                }
                return EmptyCodeDict();
            }
            set
            {
                InterestedParty.Type.Clear();
                InterestedParty.Type.Add(DictToCodeableConcept(value));
            }
        }

        /// <summary>Manner of Death Type.</summary>
        /// <value>the manner of death type. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; manner = new Dictionary&lt;string, string&gt;();</para>
        /// <para>manner.Add("code", "7878000");</para>
        /// <para>manner.Add("system", "");</para>
        /// <para>manner.Add("display", "Accidental death");</para>
        /// <para>ExampleDeathRecord.MannerOfDeathType = manner;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Manner Of Death Type: {ExampleDeathRecord.MannerOfDeathType['display']}");</para>
        /// </example>
        [Property("Manner Of Death Type", Property.Types.Dictionary, "Death Certification", "Manner of Death Type.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Manner-of-Death.html", true, 49)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='69449-7')", "")]
        public Dictionary<string, string> MannerOfDeathType
        {
            get
            {
                if (MannerOfDeath != null && MannerOfDeath.Value != null && MannerOfDeath.Value as CodeableConcept != null)
                {
                    return CodeableConceptToDict((CodeableConcept)MannerOfDeath.Value);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (MannerOfDeath == null)
                {
                    MannerOfDeath = new Observation();
                    MannerOfDeath.Id = Guid.NewGuid().ToString();
                    MannerOfDeath.Meta = new Meta();
                    string[] mannerofdeath_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Manner-of-Death" };
                    MannerOfDeath.Meta.Profile = mannerofdeath_profile;
                    MannerOfDeath.Status = ObservationStatus.Final;
                    MannerOfDeath.Code = new CodeableConcept("http://loinc.org", "69449-7", "Manner of death", null);
                    MannerOfDeath.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    MannerOfDeath.Performer.Add(new ResourceReference("urn:uuid:" + Certifier.Id));
                    MannerOfDeath.Value = DictToCodeableConcept(value);
                    AddReferenceToComposition(MannerOfDeath.Id);
                    Bundle.AddResourceEntry(MannerOfDeath, "urn:uuid:" + MannerOfDeath.Id);
                }
                else
                {
                    MannerOfDeath.Value = DictToCodeableConcept(value);
                }
            }
        }

        /// <summary>Given name(s) of certifier.</summary>
        /// <value>the certifier's name (first, middle, etc.)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>string[] names = { "Doctor", "Middle" };</para>
        /// <para>ExampleDeathRecord.CertifierGivenNames = names;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Certifier Given Name(s): {string.Join(", ", ExampleDeathRecord.CertifierGivenNames)}");</para>
        /// </example>
        [Property("Certifier Given Names", Property.Types.StringArr, "Death Certification", "Given name(s) of certifier.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Certifier.html", true, 5)]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Certifier')", "name")]
        public string[] CertifierGivenNames
        {
            get
            {
                if (Certifier.Name.Count() > 0)
                {
                    return Certifier.Name.First().Given.ToArray();
                }
                return new string[0];
            }
            set
            {
                HumanName name = Certifier.Name.SingleOrDefault(n => n.Use == HumanName.NameUse.Official);
                if (name != null)
                {
                    name.Given = value;
                }
                else
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    name.Given = value;
                    Certifier.Name.Add(name);
                }
            }
        }

        /// <summary>Family name of certifier.</summary>
        /// <value>the certifier's family name (i.e. last name)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.CertifierFamilyName = "Last";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Certifier's Last Name: {ExampleDeathRecord.CertifierFamilyName}");</para>
        /// </example>
        [Property("Certifier Family Name", Property.Types.String, "Death Certification", "Family name of certifier.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Certifier.html", true, 6)]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Certifier')", "name")]
        public string CertifierFamilyName
        {
            get
            {
                if (Certifier.Name.Count() > 0)
                {
                    return Certifier.Name.First().Family;
                }
                return null;
            }
            set
            {
                HumanName name = Certifier.Name.FirstOrDefault();
                if (name != null && !String.IsNullOrEmpty(value))
                {
                    name.Family = value;
                }
                else if (!String.IsNullOrEmpty(value))
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    name.Family = value;
                    Certifier.Name.Add(name);
                }
            }
        }

        /// <summary>Certifier's Suffix.</summary>
        /// <value>the certifier's suffix</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.CertifierSuffix = "Jr.";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Certifier Suffix: {ExampleDeathRecord.CertifierSuffix}");</para>
        /// </example>
        [Property("Certifier Suffix", Property.Types.String, "Death Certification", "Certifier's Suffix.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Certifier.html", true, 7)]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Certifier')", "name")]
        public string CertifierSuffix
        {
            get
            {
                if (Certifier.Name.Count() > 0 && Certifier.Name.First().Suffix.Count() > 0)
                {
                    return Certifier.Name.First().Suffix.First();
                }
                return null;
            }
            set
            {
                HumanName name = Certifier.Name.FirstOrDefault();
                if (name != null && !String.IsNullOrEmpty(value))
                {
                    string[] suffix = { value };
                    name.Suffix = suffix;
                }
                else if (!String.IsNullOrEmpty(value))
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    string[] suffix = { value };
                    name.Suffix = suffix;
                    Certifier.Name.Add(name);
                }
            }
        }

        /// <summary>Certifier's Address.</summary>
        /// <value>the certifier's address. A Dictionary representing an address, containing the following key/value pairs:
        /// <para>"addressLine1" - address, line one</para>
        /// <para>"addressLine2" - address, line two</para>
        /// <para>"addressCity" - address, city</para>
        /// <para>"addressCounty" - address, county</para>
        /// <para>"addressState" - address, state</para>
        /// <para>"addressZip" - address, zip</para>
        /// <para>"addressCountry" - address, country</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; address = new Dictionary&lt;string, string&gt;();</para>
        /// <para>address.Add("addressLine1", "123 Test Street");</para>
        /// <para>address.Add("addressLine2", "Unit 3");</para>
        /// <para>address.Add("addressCity", "Boston");</para>
        /// <para>address.Add("addressCounty", "Suffolk");</para>
        /// <para>address.Add("addressState", "MA");</para>
        /// <para>address.Add("addressZip", "12345");</para>
        /// <para>address.Add("addressCountry", "United States");</para>
        /// <para>ExampleDeathRecord.CertifierAddress = address;</para>
        /// <para>// Getter:</para>
        /// <para>foreach(var pair in ExampleDeathRecord.CertifierAddress)</para>
        /// <para>{</para>
        /// <para>  Console.WriteLine($"\tCertifierAddress key: {pair.Key}: value: {pair.Value}");</para>
        /// <para>};</para>
        /// </example>
        [Property("Certifier Address", Property.Types.Dictionary, "Death Certification", "Certifier's Address.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Certifier.html", true, 8)]
        [PropertyParam("addressLine1", "address, line one")]
        [PropertyParam("addressLine2", "address, line two")]
        [PropertyParam("addressCity", "address, city")]
        [PropertyParam("addressCounty", "address, county")]
        [PropertyParam("addressState", "address, state")]
        [PropertyParam("addressZip", "address, zip")]
        [PropertyParam("addressCountry", "address, country")]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Certifier')", "address")]
        public Dictionary<string, string> CertifierAddress
        {
            get
            {
                return AddressToDict(Certifier.Address.FirstOrDefault());
            }
            set
            {
                Certifier.Address.Clear();
                Certifier.Address.Add(DictToAddress(value));
            }
        }

        /// <summary>Certifier Qualification.</summary>
        /// <value>the certifier qualification. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; qualification = new Dictionary&lt;string, string&gt;();</para>
        /// <para>qualification.Add("code", "434641000124105");</para>
        /// <para>qualification.Add("system", "http://snomed.info/sct");</para>
        /// <para>qualification.Add("display", "Physician certified and pronounced death certificate");</para>
        /// <para>ExampleDeathRecord.CertifierQualification = qualification;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCertifier Qualification: {ExampleDeathRecord.CertifierQualification['display']}");</para>
        /// </example>
        [Property("Certifier Qualification", Property.Types.Dictionary, "Death Certification", "Certifier Qualification.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Certifier.html", false, 9)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Certifier')", "qualification")]
        public Dictionary<string, string> CertifierQualification
        {
            get
            {
                Practitioner.QualificationComponent qualification = Certifier.Qualification.FirstOrDefault();
                if (qualification != null && qualification.Code != null && qualification.Code.Coding.FirstOrDefault() != null)
                {
                    return CodeableConceptToDict(qualification.Code);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (Certifier.Qualification.FirstOrDefault() == null)
                {
                    Practitioner.QualificationComponent qualification = new Practitioner.QualificationComponent();
                    qualification.Code = DictToCodeableConcept(value);
                    Certifier.Qualification.Add(qualification);
                }
                else
                {
                    Certifier.Qualification.First().Code = DictToCodeableConcept(value);
                }
            }
        }

        /// <summary>Certifier Identifier.</summary>
        /// <value>the certifier identification. A Dictionary representing a system (e.g. NPI) and a value, containing the following key/value pairs:
        /// <para>"system" - the identifier system, e.g. US NPI</para>
        /// <para>"value" - the idetifier value, e.g. US NPI number</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; identifier = new Dictionary&lt;string, string&gt;();</para>
        /// <para>identifier.Add("system", "http://hl7.org/fhir/sid/us-npi");</para>
        /// <para>identifier.Add("value", "1234567890");</para>
        /// <para>ExampleDeathRecord.CertifierIdentifier = identifier;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCertifier Identifier: {ExampleDeathRecord.CertifierIdentifier['value']}");</para>
        /// </example>
        [Property("Certifier Identifier", Property.Types.Dictionary, "Death Certification", "Certifier Identifier.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Certifier.html", false, 10)]
        [PropertyParam("system", "The identifier system.")]
        [PropertyParam("value", "The identifier value.")]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Certifier')", "identifier")]
        public Dictionary<string, string> CertifierIdentifier
        {
            get
            {
                Identifier identifier = Certifier.Identifier.FirstOrDefault();
                var result = new Dictionary<string, string>();
                if (identifier != null)
                {
                    result["system"] = identifier.System;
                    result["value"] = identifier.Value;
                }
                return result;
            }
            set
            {
                if (Certifier.Identifier.Count > 0)
                {
                    Certifier.Identifier.Clear();
                }
                Identifier identifier = new Identifier();
                identifier.System = value["system"];
                identifier.Value = value["value"];
                Certifier.Identifier.Add(identifier);
            }
        }

        /// <summary>Certifier License Number.</summary>
        /// <value>A string containing the certifier license number.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.CertifierQualification = qualification;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCertifier Qualification: {ExampleDeathRecord.CertifierQualification['display']}");</para>
        /// </example>
        [Property("Certifier License Number", Property.Types.String, "Death Certification", "Certifier License Number.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Certifier.html", false, 11)]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Certifier')", "qualification")]
        public string CertifierLicenseNumber
        {
            get
            {
                Practitioner.QualificationComponent qualification = Certifier.Qualification.FirstOrDefault();
                if (qualification != null && qualification.Identifier.FirstOrDefault() != null)
                {
                    if (!String.IsNullOrWhiteSpace(qualification.Identifier.First().Value))
                    {
                        return qualification.Identifier.First().Value;
                    }
                    return null;
                }
                return null;
            }
            set
            {
                if (Certifier.Qualification.FirstOrDefault() == null)
                {
                    Practitioner.QualificationComponent qualification = new Practitioner.QualificationComponent();
                    Identifier identifier = new Identifier();
                    identifier.Value = value;
                    qualification.Identifier.Add(identifier);
                    Certifier.Qualification.Add(qualification);
                }
                else
                {
                    Certifier.Qualification.First().Identifier.Clear();
                    Identifier identifier = new Identifier();
                    identifier.Value = value;
                    Certifier.Qualification.First().Identifier.Add(identifier);
                }
            }
        }

        /// <summary>Significant conditions that contributed to death but did not result in the underlying cause.
        /// Corresponds to part 2 of item 32 of the U.S. Standard Certificate of Death.</summary>
        /// <value>A string containing the significant conditions that contributed to death but did not result in
        /// the underlying cause captured by a CauseOfDeathCondition.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.ContributingConditions = "Example Contributing Condition";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Cause: {ExampleDeathRecord.ContributingConditions}");</para>
        /// </example>
        [Property("Contributing Conditions", Property.Types.String, "Death Certification", "Significant conditions that contributed to death but did not result in the underlying cause.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Condition-Contributing-To-Death.html", true, 100)]
        [FHIRPath("Bundle.entry.resource.where($this is Condition).where(onset.empty())", "")]
        public string ContributingConditions
        {
            get
            {
                if (ConditionContributingToDeath != null && ConditionContributingToDeath.Code != null && ConditionContributingToDeath.Code.Text != null)
                {
                    return ConditionContributingToDeath.Code.Text;
                }
                return null;
            }
            set
            {
                if (ConditionContributingToDeath != null)
                {
                    ConditionContributingToDeath.Code.Text = value;
                }
                else
                {
                    ConditionContributingToDeath = new Condition();
                    ConditionContributingToDeath.Id = Guid.NewGuid().ToString();
                    ConditionContributingToDeath.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    ConditionContributingToDeath.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    ConditionContributingToDeath.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Condition-Contributing-To-Death" };
                    ConditionContributingToDeath.Meta.Profile = condition_profile;
                    ConditionContributingToDeath.Code = new CodeableConcept();
                    ConditionContributingToDeath.Code.Text = value;
                    AddReferenceToComposition(ConditionContributingToDeath.Id);
                    Bundle.AddResourceEntry(ConditionContributingToDeath, "urn:uuid:" + ConditionContributingToDeath.Id);
                }
            }
        }

        /// <summary>Conditions that resulted in the cause of death. Corresponds to part 1 of item 32 of the U.S.
        /// Standard Certificate of Death.</summary>
        /// <value>Conditions that resulted in the underlying cause of death. An array of tuples (in the order they would
        /// appear on a death certificate, from top to bottom), each containing the cause of death literal (Tuple "Item1") the
        /// approximate onset to death (Tuple "Item2"), and an (optional) Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code describing this finding</para>
        /// <para>"system" - the system the given code belongs to</para>
        /// <para>"display" - the human readable display text that corresponds to the given code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Tuple&lt;string, string, Dictionary&lt;string, string&gt;&gt;[] causes =</para>
        /// <para>{</para>
        /// <para>    Tuple.Create("Example Immediate COD", "minutes", new Dictionary&lt;string, string&gt;(){ {"code", "1234"}, {"system", "example"} }),</para>
        /// <para>    Tuple.Create("Example Underlying COD 1", "2 hours", new Dictionary&lt;string, string&gt;()),</para>
        /// <para>    Tuple.Create("Example Underlying COD 2", "6 months", new Dictionary&lt;string, string&gt;()),</para>
        /// <para>    Tuple.Create("Example Underlying COD 3", "15 years", new Dictionary&lt;string, string&gt;())</para>
        /// <para>};</para>
        /// <para>ExampleDeathRecord.CausesOfDeath = causes;</para>
        /// <para>// Getter:</para>
        /// <para>Tuple&lt;string, string&gt;[] causes = ExampleDeathRecord.CausesOfDeath;</para>
        /// <para>foreach (var cause in causes)</para>
        /// <para>{</para>
        /// <para>    Console.WriteLine($"Cause: {cause.Item1}, Onset: {cause.Item2}, Code: {cause.Item3}");</para>
        /// <para>}</para>
        /// </example>
        [Property("Causes Of Death", Property.Types.TupleCOD, "Death Certification", "Conditions that resulted in the cause of death.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-of-Death-Pathway.html", true, 50)]
        [FHIRPath("Bundle.entry.resource.where($this is Condition).where(onset.empty().not())", "")]
        public Tuple<string, string, Dictionary<string, string>>[] CausesOfDeath
        {
            get
            {
                List<Tuple<string, string, Dictionary<string, string>>> results = new List<Tuple<string, string, Dictionary<string, string>>>();
                if (!String.IsNullOrEmpty(COD1A) || !String.IsNullOrEmpty(INTERVAL1A) || (CODE1A != null && !String.IsNullOrEmpty(CODE1A["code"])))
                {
                    results.Add(Tuple.Create(COD1A, INTERVAL1A, CODE1A));
                }
                if (!String.IsNullOrEmpty(COD1B) || !String.IsNullOrEmpty(INTERVAL1B) || (CODE1B != null && !String.IsNullOrEmpty(CODE1B["code"])))
                {
                    results.Add(Tuple.Create(COD1B, INTERVAL1B, CODE1B));
                }
                if (!String.IsNullOrEmpty(COD1C) || !String.IsNullOrEmpty(INTERVAL1C) || (CODE1C != null && !String.IsNullOrEmpty(CODE1C["code"])))
                {
                    results.Add(Tuple.Create(COD1C, INTERVAL1C, CODE1C));
                }
                if (!String.IsNullOrEmpty(COD1D) || !String.IsNullOrEmpty(INTERVAL1D) || (CODE1D != null && !String.IsNullOrEmpty(CODE1D["code"])))
                {
                    results.Add(Tuple.Create(COD1D, INTERVAL1D, CODE1D));
                }
                if (!String.IsNullOrEmpty(COD1E) || !String.IsNullOrEmpty(INTERVAL1E) || (CODE1E != null && !String.IsNullOrEmpty(CODE1E["code"])))
                {
                    results.Add(Tuple.Create(COD1E, INTERVAL1E, CODE1E));
                }
                if (!String.IsNullOrEmpty(COD1F) || !String.IsNullOrEmpty(INTERVAL1F) || (CODE1F != null && !String.IsNullOrEmpty(CODE1F["code"])))
                {
                    results.Add(Tuple.Create(COD1F, INTERVAL1F, CODE1F));
                }
                if (!String.IsNullOrEmpty(COD1G) || !String.IsNullOrEmpty(INTERVAL1G) || (CODE1G != null && !String.IsNullOrEmpty(CODE1G["code"])))
                {
                    results.Add(Tuple.Create(COD1G, INTERVAL1G, CODE1G));
                }
                if (!String.IsNullOrEmpty(COD1H) || !String.IsNullOrEmpty(INTERVAL1H) || (CODE1H != null && !String.IsNullOrEmpty(CODE1H["code"])))
                {
                    results.Add(Tuple.Create(COD1H, INTERVAL1H, CODE1H));
                }
                if (!String.IsNullOrEmpty(COD1I) || !String.IsNullOrEmpty(INTERVAL1I) || (CODE1I != null && !String.IsNullOrEmpty(CODE1I["code"])))
                {
                    results.Add(Tuple.Create(COD1I, INTERVAL1I, CODE1I));
                }
                if (!String.IsNullOrEmpty(COD1J) || !String.IsNullOrEmpty(INTERVAL1J) || (CODE1J != null && !String.IsNullOrEmpty(CODE1J["code"])))
                {
                    results.Add(Tuple.Create(COD1J, INTERVAL1J, CODE1J));
                }
                return results.ToArray();
            }
            set
            {
                if (value != null)
                {
                    if (value.Length > 0)
                    {
                        COD1A = value[0].Item1;
                        INTERVAL1A = value[0].Item2;
                        CODE1A = value[0].Item3;
                    }
                    if (value.Length > 1)
                    {
                        COD1B = value[1].Item1;
                        INTERVAL1B = value[1].Item2;
                        CODE1B = value[1].Item3;
                    }
                    if (value.Length > 2)
                    {
                        COD1C = value[2].Item1;
                        INTERVAL1C = value[2].Item2;
                        CODE1C = value[2].Item3;
                    }
                    if (value.Length > 3)
                    {
                        COD1D = value[3].Item1;
                        INTERVAL1D = value[3].Item2;
                        CODE1D = value[3].Item3;
                    }
                    if (value.Length > 4)
                    {
                        COD1E = value[4].Item1;
                        INTERVAL1E = value[4].Item2;
                        CODE1E = value[4].Item3;
                    }
                    if (value.Length > 5)
                    {
                        COD1F = value[5].Item1;
                        INTERVAL1F = value[5].Item2;
                        CODE1F = value[5].Item3;
                    }
                    if (value.Length > 6)
                    {
                        COD1G = value[6].Item1;
                        INTERVAL1G = value[6].Item2;
                        CODE1G = value[6].Item3;
                    }
                    if (value.Length > 7)
                    {
                        COD1H = value[7].Item1;
                        INTERVAL1H = value[7].Item2;
                        CODE1H = value[7].Item3;
                    }
                    if (value.Length > 8)
                    {
                        COD1I = value[8].Item1;
                        INTERVAL1I = value[8].Item2;
                        CODE1I = value[8].Item3;
                    }
                    if (value.Length > 9)
                    {
                        COD1J = value[9].Item1;
                        INTERVAL1J = value[9].Item2;
                        CODE1J = value[9].Item3;
                    }
                }
            }
        }

        /// <summary>Cause of Death Part I, Line a.</summary>
        /// <value>the immediate cause of death literal.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.COD1A = "Rupture of myocardium";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Cause: {ExampleDeathRecord.COD1A}");</para>
        /// </example>
        [Property("COD1A", Property.Types.String, "Death Certification", "Cause of Death Part I, Line a.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string COD1A
        {
            get
            {
                if (CauseOfDeathConditionA != null && CauseOfDeathConditionA.Code != null)
                {
                    return CauseOfDeathConditionA.Code.Text;
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionA != null && CauseOfDeathConditionA.Code != null)
                {
                    CauseOfDeathConditionA.Code.Text = value;
                }
                else if (CauseOfDeathConditionA != null)
                {
                    CauseOfDeathConditionA.Code = new CodeableConcept();
                    CauseOfDeathConditionA.Code.Text = value;
                }
                else
                {
                    CauseOfDeathConditionA = new Condition();
                    CauseOfDeathConditionA.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionA.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionA.Meta.Profile = condition_profile;
                    CauseOfDeathConditionA.Code = new CodeableConcept();
                    CauseOfDeathConditionA.Code.Text = value;
                    CauseOfDeathConditionA.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionA.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionA.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionA, "urn:uuid:" + CauseOfDeathConditionA.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionA.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[0] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line a.</summary>
        /// <value>the immediate cause of death approximate interval: onset to death.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.INTERVAL1A = "Minutes";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interval: {ExampleDeathRecord.INTERVAL1A}");</para>
        /// </example>
        [Property("INTERVAL1A", Property.Types.String, "Death Certification", "Cause of Death Part I Interval, Line a.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string INTERVAL1A
        {
            get
            {
                if (CauseOfDeathConditionA != null && CauseOfDeathConditionA.Onset != null)
                {
                    return CauseOfDeathConditionA.Onset.ToString();
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionA != null)
                {
                    CauseOfDeathConditionA.Onset = new FhirString(value);
                }
                else
                {
                    CauseOfDeathConditionA = new Condition();
                    CauseOfDeathConditionA.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionA.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionA.Meta.Profile = condition_profile;
                    CauseOfDeathConditionA.Onset = new FhirString(value);
                    CauseOfDeathConditionA.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionA.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionA.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionA, "urn:uuid:" + CauseOfDeathConditionA.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionA.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[0] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Code, Line a.</summary>
        /// <value>the immediate cause of death coding. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "I21.0");</para>
        /// <para>code.Add("system", "http://hl7.org/fhir/sid/icd-10");</para>
        /// <para>code.Add("display", "Acute transmural myocardial infarction of anterior wall");</para>
        /// <para>ExampleDeathRecord.CODE1A = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCause of Death: {ExampleDeathRecord.CODE1A['display']}");</para>
        /// </example>
        [Property("CODE1A", Property.Types.Dictionary, "Death Certification", "Cause of Death Part I Code, Line a.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        public Dictionary<string, string> CODE1A
        {
            get
            {
                if (CauseOfDeathConditionA != null && CauseOfDeathConditionA.Code != null)
                {
                    return CodeableConceptToDict(CauseOfDeathConditionA.Code);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (CauseOfDeathConditionA != null && CauseOfDeathConditionA.Code != null)
                {
                    CodeableConcept code = DictToCodeableConcept(value);
                    code.Text = CauseOfDeathConditionA.Code.Text;
                    CauseOfDeathConditionA.Code = code;
                }
                else if (CauseOfDeathConditionA != null)
                {
                    CauseOfDeathConditionA.Code = DictToCodeableConcept(value);
                }
                else
                {
                    CauseOfDeathConditionA = new Condition();
                    CauseOfDeathConditionA.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionA.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionA.Meta.Profile = condition_profile;
                    CauseOfDeathConditionA.Code = DictToCodeableConcept(value);
                    CauseOfDeathConditionA.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionA.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionA.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionA, "urn:uuid:" + CauseOfDeathConditionA.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionA.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[0] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I, Line b.</summary>
        /// <value>the first underlying cause of death literal.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.COD1B = "Acute myocardial infarction";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Cause: {ExampleDeathRecord.COD1B}");</para>
        /// </example>
        [Property("COD1B", Property.Types.String, "Death Certification", "Cause of Death Part I, Line b.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string COD1B
        {
            get
            {
                if (CauseOfDeathConditionB != null && CauseOfDeathConditionB.Code != null)
                {
                    return CauseOfDeathConditionB.Code.Text;
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionB != null && CauseOfDeathConditionB.Code != null)
                {
                    CauseOfDeathConditionB.Code.Text = value;
                }
                else if (CauseOfDeathConditionB != null)
                {
                    CauseOfDeathConditionB.Code = new CodeableConcept();
                    CauseOfDeathConditionB.Code.Text = value;
                }
                else
                {
                    CauseOfDeathConditionB = new Condition();
                    CauseOfDeathConditionB.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionB.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionB.Meta.Profile = condition_profile;
                    CauseOfDeathConditionB.Code = new CodeableConcept();
                    CauseOfDeathConditionB.Code.Text = value;
                    CauseOfDeathConditionB.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionB.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionB.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionB, "urn:uuid:" + CauseOfDeathConditionB.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionB.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[1] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line b.</summary>
        /// <value>the first underlying cause of death approximate interval: onset to death.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.INTERVAL1B = "6 days";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interval: {ExampleDeathRecord.INTERVAL1B}");</para>
        /// </example>
        [Property("INTERVAL1B", Property.Types.String, "Death Certification", "Cause of Death Part I Interval, Line b.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string INTERVAL1B
        {
            get
            {
                if (CauseOfDeathConditionB != null && CauseOfDeathConditionB.Onset != null)
                {
                    return CauseOfDeathConditionB.Onset.ToString();
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionB != null)
                {
                    CauseOfDeathConditionB.Onset = new FhirString(value);
                }
                else
                {
                    CauseOfDeathConditionB = new Condition();
                    CauseOfDeathConditionB.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionB.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionB.Meta.Profile = condition_profile;
                    CauseOfDeathConditionB.Onset = new FhirString(value);
                    CauseOfDeathConditionB.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionB.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionB.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionB, "urn:uuid:" + CauseOfDeathConditionB.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionB.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[1] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Code, Line b.</summary>
        /// <value>the first underlying cause of death coding. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "I21.9");</para>
        /// <para>code.Add("system", "http://hl7.org/fhir/sid/icd-10");</para>
        /// <para>code.Add("display", "Acute myocardial infarction, unspecified");</para>
        /// <para>ExampleDeathRecord.CODE1B = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCause of Death: {ExampleDeathRecord.CODE1B['display']}");</para>
        /// </example>
        [Property("CODE1B", Property.Types.Dictionary, "Death Certification", "Cause of Death Part I Code, Line b.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        public Dictionary<string, string> CODE1B
        {
            get
            {
                if (CauseOfDeathConditionB != null && CauseOfDeathConditionB.Code != null)
                {
                    return CodeableConceptToDict(CauseOfDeathConditionB.Code);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (CauseOfDeathConditionB != null && CauseOfDeathConditionB.Code != null)
                {
                    CodeableConcept code = DictToCodeableConcept(value);
                    code.Text = CauseOfDeathConditionB.Code.Text;
                    CauseOfDeathConditionB.Code = code;
                }
                else if (CauseOfDeathConditionB != null)
                {
                    CauseOfDeathConditionB.Code = DictToCodeableConcept(value);
                }
                else
                {
                    CauseOfDeathConditionB = new Condition();
                    CauseOfDeathConditionB.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionB.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionB.Meta.Profile = condition_profile;
                    CauseOfDeathConditionB.Code = DictToCodeableConcept(value);
                    CauseOfDeathConditionB.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionB.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionB.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionB, "urn:uuid:" + CauseOfDeathConditionB.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionB.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[1] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I, Line c.</summary>
        /// <value>the second underlying cause of death literal.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.COD1C = "Coronary artery thrombosis";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Cause: {ExampleDeathRecord.COD1C}");</para>
        /// </example>
        [Property("COD1C", Property.Types.String, "Death Certification", "Cause of Death Part I, Line c.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string COD1C
        {
            get
            {
                if (CauseOfDeathConditionC != null && CauseOfDeathConditionC.Code != null)
                {
                    return CauseOfDeathConditionC.Code.Text;
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionC != null && CauseOfDeathConditionC.Code != null)
                {
                    CauseOfDeathConditionC.Code.Text = value;
                }
                else if (CauseOfDeathConditionC != null)
                {
                    CauseOfDeathConditionC.Code = new CodeableConcept();
                    CauseOfDeathConditionC.Code.Text = value;
                }
                else
                {
                    CauseOfDeathConditionC = new Condition();
                    CauseOfDeathConditionC.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionC.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionC.Meta.Profile = condition_profile;
                    CauseOfDeathConditionC.Code = new CodeableConcept();
                    CauseOfDeathConditionC.Code.Text = value;
                    CauseOfDeathConditionC.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionC.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionC.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionC, "urn:uuid:" + CauseOfDeathConditionC.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionC.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[2] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line c.</summary>
        /// <value>the second underlying cause of death approximate interval: onset to death.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.INTERVAL1C = "5 years";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interval: {ExampleDeathRecord.INTERVAL1C}");</para>
        /// </example>
        [Property("INTERVAL1C", Property.Types.String, "Death Certification", "Cause of Death Part I Interval, Line c.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string INTERVAL1C
        {
            get
            {
                if (CauseOfDeathConditionC != null && CauseOfDeathConditionC.Onset != null)
                {
                    return CauseOfDeathConditionC.Onset.ToString();
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionC != null)
                {
                    CauseOfDeathConditionC.Onset = new FhirString(value);
                }
                else
                {
                    CauseOfDeathConditionC = new Condition();
                    CauseOfDeathConditionC.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionC.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionC.Meta.Profile = condition_profile;
                    CauseOfDeathConditionC.Onset = new FhirString(value);
                    CauseOfDeathConditionC.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionC.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionC.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionC, "urn:uuid:" + CauseOfDeathConditionC.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionC.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[2] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Code, Line c.</summary>
        /// <value>the second underlying cause of death coding. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "I21.9");</para>
        /// <para>code.Add("system", "http://hl7.org/fhir/sid/icd-10");</para>
        /// <para>code.Add("display", "Acute myocardial infarction, unspecified");</para>
        /// <para>ExampleDeathRecord.CODE1C = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCause of Death: {ExampleDeathRecord.CODE1C['display']}");</para>
        /// </example>
        [Property("CODE1C", Property.Types.Dictionary, "Death Certification", "Cause of Death Part I Code, Line c.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        public Dictionary<string, string> CODE1C
        {
            get
            {
                if (CauseOfDeathConditionC != null && CauseOfDeathConditionC.Code != null)
                {
                    return CodeableConceptToDict(CauseOfDeathConditionC.Code);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (CauseOfDeathConditionC != null && CauseOfDeathConditionC.Code != null)
                {
                    CodeableConcept code = DictToCodeableConcept(value);
                    code.Text = CauseOfDeathConditionC.Code.Text;
                    CauseOfDeathConditionC.Code = code;
                }
                else if (CauseOfDeathConditionC != null)
                {
                    CauseOfDeathConditionC.Code = DictToCodeableConcept(value);
                }
                else
                {
                    CauseOfDeathConditionC = new Condition();
                    CauseOfDeathConditionC.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionC.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionC.Meta.Profile = condition_profile;
                    CauseOfDeathConditionC.Code = DictToCodeableConcept(value);
                    CauseOfDeathConditionC.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionC.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionC.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionC, "urn:uuid:" + CauseOfDeathConditionC.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionC.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[2] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I, Line d.</summary>
        /// <value>the third underlying cause of death literal.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.COD1D = "Atherosclerotic coronary artery disease";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Cause: {ExampleDeathRecord.COD1D}");</para>
        /// </example>
        [Property("COD1D", Property.Types.String, "Death Certification", "Cause of Death Part I, Line d.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string COD1D
        {
            get
            {
                if (CauseOfDeathConditionD != null && CauseOfDeathConditionD.Code != null)
                {
                    return CauseOfDeathConditionD.Code.Text;
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionD != null && CauseOfDeathConditionD.Code != null)
                {
                    CauseOfDeathConditionD.Code.Text = value;
                }
                else if (CauseOfDeathConditionD != null)
                {
                    CauseOfDeathConditionD.Code = new CodeableConcept();
                    CauseOfDeathConditionD.Code.Text = value;
                }
                else
                {
                    CauseOfDeathConditionD = new Condition();
                    CauseOfDeathConditionD.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionD.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionD.Meta.Profile = condition_profile;
                    CauseOfDeathConditionD.Code = new CodeableConcept();
                    CauseOfDeathConditionD.Code.Text = value;
                    CauseOfDeathConditionD.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionD.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionD.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionD, "urn:uuid:" + CauseOfDeathConditionD.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionD.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[3] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line d.</summary>
        /// <value>the third underlying cause of death approximate interval: onset to death.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.INTERVAL1D = "7 years";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interval: {ExampleDeathRecord.INTERVAL1D}");</para>
        /// </example>
        [Property("INTERVAL1D", Property.Types.String, "Death Certification", "Cause of Death Part I Interval, Line d.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string INTERVAL1D
        {
            get
            {
                if (CauseOfDeathConditionD != null && CauseOfDeathConditionD.Onset != null)
                {
                    return CauseOfDeathConditionD.Onset.ToString();
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionD != null)
                {
                    CauseOfDeathConditionD.Onset = new FhirString(value);
                }
                else
                {
                    CauseOfDeathConditionD = new Condition();
                    CauseOfDeathConditionD.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionD.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionD.Meta.Profile = condition_profile;
                    CauseOfDeathConditionD.Onset = new FhirString(value);
                    CauseOfDeathConditionD.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionD.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionD.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionD, "urn:uuid:" + CauseOfDeathConditionD.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionD.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[3] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Code, Line d.</summary>
        /// <value>the third underlying cause of death coding. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "I21.9");</para>
        /// <para>code.Add("system", "http://hl7.org/fhir/sid/icd-10");</para>
        /// <para>code.Add("display", "Acute myocardial infarction, unspecified");</para>
        /// <para>ExampleDeathRecord.CODE1D = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCause of Death: {ExampleDeathRecord.CODE1D['display']}");</para>
        /// </example>
        [Property("CODE1D", Property.Types.Dictionary, "Death Certification", "Cause of Death Part I Code, Line d.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        public Dictionary<string, string> CODE1D
        {
            get
            {
                if (CauseOfDeathConditionD != null && CauseOfDeathConditionD.Code != null)
                {
                    return CodeableConceptToDict(CauseOfDeathConditionD.Code);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (CauseOfDeathConditionD != null && CauseOfDeathConditionD.Code != null)
                {
                    CodeableConcept code = DictToCodeableConcept(value);
                    code.Text = CauseOfDeathConditionD.Code.Text;
                    CauseOfDeathConditionD.Code = code;
                }
                else if (CauseOfDeathConditionD != null)
                {
                    CauseOfDeathConditionD.Code = DictToCodeableConcept(value);
                }
                else
                {
                    CauseOfDeathConditionD = new Condition();
                    CauseOfDeathConditionD.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionD.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionD.Meta.Profile = condition_profile;
                    CauseOfDeathConditionD.Code = DictToCodeableConcept(value);
                    CauseOfDeathConditionD.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionD.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionD.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionD, "urn:uuid:" + CauseOfDeathConditionD.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionD.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[3] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I, Line e.</summary>
        /// <value>the fourth underlying cause of death literal.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.COD1E = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Cause: {ExampleDeathRecord.COD1E}");</para>
        /// </example>
        [Property("COD1E", Property.Types.String, "Death Certification", "Cause of Death Part I, Line e.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string COD1E
        {
            get
            {
                if (CauseOfDeathConditionE != null && CauseOfDeathConditionE.Code != null)
                {
                    return CauseOfDeathConditionE.Code.Text;
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionE != null && CauseOfDeathConditionE.Code != null)
                {
                    CauseOfDeathConditionE.Code.Text = value;
                }
                else if (CauseOfDeathConditionE != null)
                {
                    CauseOfDeathConditionE.Code = new CodeableConcept();
                    CauseOfDeathConditionE.Code.Text = value;
                }
                else
                {
                    CauseOfDeathConditionE = new Condition();
                    CauseOfDeathConditionE.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionE.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionE.Meta.Profile = condition_profile;
                    CauseOfDeathConditionE.Code = new CodeableConcept();
                    CauseOfDeathConditionE.Code.Text = value;
                    CauseOfDeathConditionE.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionE.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionE.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionE, "urn:uuid:" + CauseOfDeathConditionE.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionE.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[4] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line e.</summary>
        /// <value>the fourth underlying cause of death approximate interval: onset to death.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.INTERVAL1E = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interval: {ExampleDeathRecord.INTERVAL1E}");</para>
        /// </example>
        [Property("INTERVAL1E", Property.Types.String, "Death Certification", "Cause of Death Part I Interval, Line e.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string INTERVAL1E
        {
            get
            {
                if (CauseOfDeathConditionE != null && CauseOfDeathConditionE.Onset != null)
                {
                    return CauseOfDeathConditionE.Onset.ToString();
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionE != null)
                {
                    CauseOfDeathConditionE.Onset = new FhirString(value);
                }
                else
                {
                    CauseOfDeathConditionE = new Condition();
                    CauseOfDeathConditionE.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionE.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionE.Meta.Profile = condition_profile;
                    CauseOfDeathConditionE.Onset = new FhirString(value);
                    CauseOfDeathConditionE.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionE.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionE.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionE, "urn:uuid:" + CauseOfDeathConditionE.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionE.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[4] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Code, Line e.</summary>
        /// <value>the fourth underlying cause of death coding. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "example");</para>
        /// <para>code.Add("system", "example");</para>
        /// <para>code.Add("display", "example");</para>
        /// <para>ExampleDeathRecord.CODE1E = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCause of Death: {ExampleDeathRecord.CODE1E['display']}");</para>
        /// </example>
        [Property("CODE1E", Property.Types.Dictionary, "Death Certification", "Cause of Death Part I Code, Line e.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [PropertyParam("text", "Additional descriptive text.")]
        public Dictionary<string, string> CODE1E
        {
            get
            {
                if (CauseOfDeathConditionE != null && CauseOfDeathConditionE.Code != null)
                {
                    return CodeableConceptToDict(CauseOfDeathConditionE.Code);
                }
                return EmptyCodeableDict();
            }
            set
            {
                if (CauseOfDeathConditionE != null && CauseOfDeathConditionE.Code != null)
                {
                    CodeableConcept code = DictToCodeableConcept(value);
                    code.Text = CauseOfDeathConditionE.Code.Text;
                    CauseOfDeathConditionE.Code = code;
                }
                else if (CauseOfDeathConditionE != null)
                {
                    CauseOfDeathConditionE.Code = DictToCodeableConcept(value);
                }
                else
                {
                    CauseOfDeathConditionE = new Condition();
                    CauseOfDeathConditionE.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionE.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionE.Meta.Profile = condition_profile;
                    CauseOfDeathConditionE.Code = DictToCodeableConcept(value);
                    CauseOfDeathConditionE.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionE.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionE.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionE, "urn:uuid:" + CauseOfDeathConditionE.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionE.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[4] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I, Line f.</summary>
        /// <value>the fifth underlying cause of death literal.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.COD1F = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Cause: {ExampleDeathRecord.COD1F}");</para>
        /// </example>
        [Property("COD1F", Property.Types.String, "Death Certification", "Cause of Death Part I, Line f.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string COD1F
        {
            get
            {
                if (CauseOfDeathConditionF != null && CauseOfDeathConditionF.Code != null)
                {
                    return CauseOfDeathConditionF.Code.Text;
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionF != null && CauseOfDeathConditionF.Code != null)
                {
                    CauseOfDeathConditionF.Code.Text = value;
                }
                else if (CauseOfDeathConditionF != null)
                {
                    CauseOfDeathConditionF.Code = new CodeableConcept();
                    CauseOfDeathConditionF.Code.Text = value;
                }
                else
                {
                    CauseOfDeathConditionF = new Condition();
                    CauseOfDeathConditionF.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionF.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionF.Meta.Profile = condition_profile;
                    CauseOfDeathConditionF.Code = new CodeableConcept();
                    CauseOfDeathConditionF.Code.Text = value;
                    CauseOfDeathConditionF.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionF.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionF.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionF, "urn:uuid:" + CauseOfDeathConditionF.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionF.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[5] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line f.</summary>
        /// <value>the fifth underlying cause of death approximate interval: onset to death.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.INTERVAL1F = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interval: {ExampleDeathRecord.INTERVAL1F}");</para>
        /// </example>
        [Property("INTERVAL1F", Property.Types.String, "Death Certification", "Cause of Death Part I Interval, Line f.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string INTERVAL1F
        {
            get
            {
                if (CauseOfDeathConditionF != null && CauseOfDeathConditionF.Onset != null)
                {
                    return CauseOfDeathConditionF.Onset.ToString();
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionF != null)
                {
                    CauseOfDeathConditionF.Onset = new FhirString(value);
                }
                else
                {
                    CauseOfDeathConditionF = new Condition();
                    CauseOfDeathConditionF.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionF.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionF.Meta.Profile = condition_profile;
                    CauseOfDeathConditionF.Onset = new FhirString(value);
                    CauseOfDeathConditionF.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionF.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionF.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionF, "urn:uuid:" + CauseOfDeathConditionF.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionF.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[5] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Code, Line f.</summary>
        /// <value>the fifth underlying cause of death coding. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "example");</para>
        /// <para>code.Add("system", "example");</para>
        /// <para>code.Add("display", "example");</para>
        /// <para>ExampleDeathRecord.CODE1F = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCause of Death: {ExampleDeathRecord.CODE1F['display']}");</para>
        /// </example>
        [Property("CODE1F", Property.Types.Dictionary, "Death Certification", "Cause of Death Part I Code, Line f.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [PropertyParam("text", "Additional descriptive text.")]
        public Dictionary<string, string> CODE1F
        {
            get
            {
                if (CauseOfDeathConditionF != null && CauseOfDeathConditionF.Code != null)
                {
                    return CodeableConceptToDict(CauseOfDeathConditionF.Code);
                }
                return EmptyCodeableDict();
            }
            set
            {
                if (CauseOfDeathConditionF != null && CauseOfDeathConditionF.Code != null)
                {
                    CodeableConcept code = DictToCodeableConcept(value);
                    code.Text = CauseOfDeathConditionF.Code.Text;
                    CauseOfDeathConditionF.Code = code;
                }
                else if (CauseOfDeathConditionF != null)
                {
                    CauseOfDeathConditionF.Code = DictToCodeableConcept(value);
                }
                else
                {
                    CauseOfDeathConditionF = new Condition();
                    CauseOfDeathConditionF.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionF.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionF.Meta.Profile = condition_profile;
                    CauseOfDeathConditionF.Code = DictToCodeableConcept(value);
                    CauseOfDeathConditionF.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionF.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionF.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionF, "urn:uuid:" + CauseOfDeathConditionF.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionF.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[5] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I, Line g.</summary>
        /// <value>the sixth underlying cause of death literal.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.COD1G = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Cause: {ExampleDeathRecord.COD1G}");</para>
        /// </example>
        [Property("COD1G", Property.Types.String, "Death Certification", "Cause of Death Part I, Line g.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string COD1G
        {
            get
            {
                if (CauseOfDeathConditionG != null && CauseOfDeathConditionG.Code != null)
                {
                    return CauseOfDeathConditionG.Code.Text;
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionG != null && CauseOfDeathConditionG.Code != null)
                {
                    CauseOfDeathConditionG.Code.Text = value;
                }
                else if (CauseOfDeathConditionG != null)
                {
                    CauseOfDeathConditionG.Code = new CodeableConcept();
                    CauseOfDeathConditionG.Code.Text = value;
                }
                else
                {
                    CauseOfDeathConditionG = new Condition();
                    CauseOfDeathConditionG.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionG.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionG.Meta.Profile = condition_profile;
                    CauseOfDeathConditionG.Code = new CodeableConcept();
                    CauseOfDeathConditionG.Code.Text = value;
                    CauseOfDeathConditionG.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionG.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionG.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionG, "urn:uuid:" + CauseOfDeathConditionG.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionG.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[6] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line g.</summary>
        /// <value>the sixth underlying cause of death approximate interval: onset to death.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.INTERVAL1G = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interval: {ExampleDeathRecord.INTERVAL1G}");</para>
        /// </example>
        [Property("INTERVAL1G", Property.Types.String, "Death Certification", "Cause of Death Part I Interval, Line g.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string INTERVAL1G
        {
            get
            {
                if (CauseOfDeathConditionG != null && CauseOfDeathConditionG.Onset != null)
                {
                    return CauseOfDeathConditionG.Onset.ToString();
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionG != null)
                {
                    CauseOfDeathConditionG.Onset = new FhirString(value);
                }
                else
                {
                    CauseOfDeathConditionG = new Condition();
                    CauseOfDeathConditionG.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionG.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionG.Meta.Profile = condition_profile;
                    CauseOfDeathConditionG.Onset = new FhirString(value);
                    CauseOfDeathConditionG.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionG.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionG.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionG, "urn:uuid:" + CauseOfDeathConditionG.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionG.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[6] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Code, Line g.</summary>
        /// <value>the sixth underlying cause of death coding. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "example");</para>
        /// <para>code.Add("system", "example");</para>
        /// <para>code.Add("display", "example");</para>
        /// <para>ExampleDeathRecord.CODE1G = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCause of Death: {ExampleDeathRecord.CODE1G['display']}");</para>
        /// </example>
        [Property("CODE1G", Property.Types.Dictionary, "Death Certification", "Cause of Death Part I Code, Line g.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [PropertyParam("text", "Additional descriptive text.")]
        public Dictionary<string, string> CODE1G
        {
            get
            {
                if (CauseOfDeathConditionG != null && CauseOfDeathConditionG.Code != null)
                {
                    return CodeableConceptToDict(CauseOfDeathConditionG.Code);
                }
                return EmptyCodeableDict();
            }
            set
            {
                if (CauseOfDeathConditionG != null && CauseOfDeathConditionG.Code != null)
                {
                    CodeableConcept code = DictToCodeableConcept(value);
                    code.Text = CauseOfDeathConditionG.Code.Text;
                    CauseOfDeathConditionG.Code = code;
                }
                else if (CauseOfDeathConditionG != null)
                {
                    CauseOfDeathConditionG.Code = DictToCodeableConcept(value);
                }
                else
                {
                    CauseOfDeathConditionG = new Condition();
                    CauseOfDeathConditionG.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionG.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionG.Meta.Profile = condition_profile;
                    CauseOfDeathConditionG.Code = DictToCodeableConcept(value);
                    CauseOfDeathConditionG.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionG.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionG.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionG, "urn:uuid:" + CauseOfDeathConditionG.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionG.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[6] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I, Line h.</summary>
        /// <value>the seventh underlying cause of death literal.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.COD1H = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Cause: {ExampleDeathRecord.COD1H}");</para>
        /// </example>
        [Property("COD1H", Property.Types.String, "Death Certification", "Cause of Death Part I, Line h.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string COD1H
        {
            get
            {
                if (CauseOfDeathConditionH != null && CauseOfDeathConditionH.Code != null)
                {
                    return CauseOfDeathConditionH.Code.Text;
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionH != null && CauseOfDeathConditionH.Code != null)
                {
                    CauseOfDeathConditionH.Code.Text = value;
                }
                else if (CauseOfDeathConditionH != null)
                {
                    CauseOfDeathConditionH.Code = new CodeableConcept();
                    CauseOfDeathConditionH.Code.Text = value;
                }
                else
                {
                    CauseOfDeathConditionH = new Condition();
                    CauseOfDeathConditionH.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionH.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionH.Meta.Profile = condition_profile;
                    CauseOfDeathConditionH.Code = new CodeableConcept();
                    CauseOfDeathConditionH.Code.Text = value;
                    CauseOfDeathConditionH.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionH.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionH.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionH, "urn:uuid:" + CauseOfDeathConditionH.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionH.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[7] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line h.</summary>
        /// <value>the seventh underlying cause of death approximate interval: onset to death.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.INTERVAL1H = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interval: {ExampleDeathRecord.INTERVAL1H}");</para>
        /// </example>
        [Property("INTERVAL1H", Property.Types.String, "Death Certification", "Cause of Death Part I Interval, Line h.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string INTERVAL1H
        {
            get
            {
                if (CauseOfDeathConditionH != null && CauseOfDeathConditionH.Onset != null)
                {
                    return CauseOfDeathConditionH.Onset.ToString();
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionH != null)
                {
                    CauseOfDeathConditionH.Onset = new FhirString(value);
                }
                else
                {
                    CauseOfDeathConditionH = new Condition();
                    CauseOfDeathConditionH.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionH.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionH.Meta.Profile = condition_profile;
                    CauseOfDeathConditionH.Onset = new FhirString(value);
                    CauseOfDeathConditionH.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionH.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionH.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionH, "urn:uuid:" + CauseOfDeathConditionH.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionH.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[7] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Code, Line h.</summary>
        /// <value>the seventh underlying cause of death coding. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "example");</para>
        /// <para>code.Add("system", "example");</para>
        /// <para>code.Add("display", "example");</para>
        /// <para>ExampleDeathRecord.CODE1H = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCause of Death: {ExampleDeathRecord.CODE1H['display']}");</para>
        /// </example>
        [Property("CODE1H", Property.Types.Dictionary, "Death Certification", "Cause of Death Part I Code, Line h.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [PropertyParam("text", "Additional descriptive text.")]
        public Dictionary<string, string> CODE1H
        {
            get
            {
                if (CauseOfDeathConditionH != null && CauseOfDeathConditionH.Code != null)
                {
                    return CodeableConceptToDict(CauseOfDeathConditionH.Code);
                }
                return EmptyCodeableDict();
            }
            set
            {
                if (CauseOfDeathConditionH != null && CauseOfDeathConditionH.Code != null)
                {
                    CodeableConcept code = DictToCodeableConcept(value);
                    code.Text = CauseOfDeathConditionH.Code.Text;
                    CauseOfDeathConditionH.Code = code;
                }
                else if (CauseOfDeathConditionH != null)
                {
                    CauseOfDeathConditionH.Code = DictToCodeableConcept(value);
                }
                else
                {
                    CauseOfDeathConditionH = new Condition();
                    CauseOfDeathConditionH.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionH.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionH.Meta.Profile = condition_profile;
                    CauseOfDeathConditionH.Code = DictToCodeableConcept(value);
                    CauseOfDeathConditionH.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionH.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionH.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionH, "urn:uuid:" + CauseOfDeathConditionH.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionH.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[7] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I, Line i.</summary>
        /// <value>the eighth underlying cause of death literal.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.COD1I = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Cause: {ExampleDeathRecord.COD1I}");</para>
        /// </example>
        [Property("COD1I", Property.Types.String, "Death Certification", "Cause of Death Part I, Line i.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string COD1I
        {
            get
            {
                if (CauseOfDeathConditionI != null && CauseOfDeathConditionI.Code != null)
                {
                    return CauseOfDeathConditionI.Code.Text;
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionI != null && CauseOfDeathConditionI.Code != null)
                {
                    CauseOfDeathConditionI.Code.Text = value;
                }
                else if (CauseOfDeathConditionI != null)
                {
                    CauseOfDeathConditionI.Code = new CodeableConcept();
                    CauseOfDeathConditionI.Code.Text = value;
                }
                else
                {
                    CauseOfDeathConditionI = new Condition();
                    CauseOfDeathConditionI.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionI.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionI.Meta.Profile = condition_profile;
                    CauseOfDeathConditionI.Code = new CodeableConcept();
                    CauseOfDeathConditionI.Code.Text = value;
                    CauseOfDeathConditionI.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionI.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionI.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionI, "urn:uuid:" + CauseOfDeathConditionI.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionI.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[8] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line i.</summary>
        /// <value>the eighth underlying cause of death approximate interval: onset to death.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.INTERVAL1I = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interval: {ExampleDeathRecord.INTERVAL1I}");</para>
        /// </example>
        [Property("INTERVAL1I", Property.Types.String, "Death Certification", "Cause of Death Part I Interval, Line i.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string INTERVAL1I
        {
            get
            {
                if (CauseOfDeathConditionI != null && CauseOfDeathConditionI.Onset != null)
                {
                    return CauseOfDeathConditionI.Onset.ToString();
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionI != null)
                {
                    CauseOfDeathConditionI.Onset = new FhirString(value);
                }
                else
                {
                    CauseOfDeathConditionI = new Condition();
                    CauseOfDeathConditionI.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionI.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionI.Meta.Profile = condition_profile;
                    CauseOfDeathConditionI.Onset = new FhirString(value);
                    CauseOfDeathConditionI.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionI.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionI.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionI, "urn:uuid:" + CauseOfDeathConditionI.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionI.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[8] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Code, Line i.</summary>
        /// <value>the eighth underlying cause of death coding. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "example");</para>
        /// <para>code.Add("system", "example");</para>
        /// <para>code.Add("display", "example");</para>
        /// <para>ExampleDeathRecord.CODE1I = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCause of Death: {ExampleDeathRecord.CODE1I['display']}");</para>
        /// </example>
        [Property("CODE1I", Property.Types.Dictionary, "Death Certification", "Cause of Death Part I Code, Line i.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [PropertyParam("text", "Additional descriptive text.")]
        public Dictionary<string, string> CODE1I
        {
            get
            {
                if (CauseOfDeathConditionI != null && CauseOfDeathConditionI.Code != null)
                {
                    return CodeableConceptToDict(CauseOfDeathConditionI.Code);
                }
                return EmptyCodeableDict();
            }
            set
            {
                if (CauseOfDeathConditionI != null && CauseOfDeathConditionI.Code != null)
                {
                    CodeableConcept code = DictToCodeableConcept(value);
                    code.Text = CauseOfDeathConditionI.Code.Text;
                    CauseOfDeathConditionI.Code = code;
                }
                else if (CauseOfDeathConditionI != null)
                {
                    CauseOfDeathConditionI.Code = DictToCodeableConcept(value);
                }
                else
                {
                    CauseOfDeathConditionI = new Condition();
                    CauseOfDeathConditionI.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionI.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionI.Meta.Profile = condition_profile;
                    CauseOfDeathConditionI.Code = DictToCodeableConcept(value);
                    CauseOfDeathConditionI.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionI.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionI.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionI, "urn:uuid:" + CauseOfDeathConditionI.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionI.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[8] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I, Line j.</summary>
        /// <value>the ninth underlying cause of death literal.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.COD1J = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Cause: {ExampleDeathRecord.COD1J}");</para>
        /// </example>
        [Property("COD1J", Property.Types.String, "Death Certification", "Cause of Death Part I, Line j.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string COD1J
        {
            get
            {
                if (CauseOfDeathConditionJ != null && CauseOfDeathConditionJ.Code != null)
                {
                    return CauseOfDeathConditionJ.Code.Text;
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionJ != null && CauseOfDeathConditionJ.Code != null)
                {
                    CauseOfDeathConditionJ.Code.Text = value;
                }
                else if (CauseOfDeathConditionJ != null)
                {
                    CauseOfDeathConditionJ.Code = new CodeableConcept();
                    CauseOfDeathConditionJ.Code.Text = value;
                }
                else
                {
                    CauseOfDeathConditionJ = new Condition();
                    CauseOfDeathConditionJ.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionJ.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionJ.Meta.Profile = condition_profile;
                    CauseOfDeathConditionJ.Code = new CodeableConcept();
                    CauseOfDeathConditionJ.Code.Text = value;
                    CauseOfDeathConditionJ.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionJ.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionJ.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionJ, "urn:uuid:" + CauseOfDeathConditionJ.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionJ.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[9] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line j.</summary>
        /// <value>the ninth underlying cause of death approximate interval: onset to death.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.INTERVAL1J = "example";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Interval: {ExampleDeathRecord.INTERVAL1J}");</para>
        /// </example>
        [Property("INTERVAL1J", Property.Types.String, "Death Certification", "Cause of Death Part I Interval, Line j.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        public string INTERVAL1J
        {
            get
            {
                if (CauseOfDeathConditionJ != null && CauseOfDeathConditionJ.Onset != null)
                {
                    return CauseOfDeathConditionJ.Onset.ToString();
                }
                return null;
            }
            set
            {
                if (CauseOfDeathConditionJ != null)
                {
                    CauseOfDeathConditionJ.Onset = new FhirString(value);
                }
                else
                {
                    CauseOfDeathConditionJ = new Condition();
                    CauseOfDeathConditionJ.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionJ.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionJ.Meta.Profile = condition_profile;
                    CauseOfDeathConditionJ.Onset = new FhirString(value);
                    CauseOfDeathConditionJ.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionJ.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionJ.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionJ, "urn:uuid:" + CauseOfDeathConditionJ.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionJ.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[9] = entry;
                }
            }
        }

        /// <summary>Cause of Death Part I Code, Line j.</summary>
        /// <value>the ninth underlying cause of death coding. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "example");</para>
        /// <para>code.Add("system", "example");</para>
        /// <para>code.Add("display", "example");</para>
        /// <para>ExampleDeathRecord.CODE1J = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tCause of Death: {ExampleDeathRecord.CODE1J['display']}");</para>
        /// </example>
        [Property("CODE1J", Property.Types.Dictionary, "Death Certification", "Cause of Death Part I Code, Line j.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Cause-Of-Death-Condition.html", false, 100)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [PropertyParam("text", "Additional descriptive text.")]
        public Dictionary<string, string> CODE1J
        {
            get
            {
                if (CauseOfDeathConditionJ != null && CauseOfDeathConditionJ.Code != null)
                {
                    return CodeableConceptToDict(CauseOfDeathConditionJ.Code);
                }
                return EmptyCodeableDict();
            }
            set
            {
                if (CauseOfDeathConditionJ != null && CauseOfDeathConditionJ.Code != null)
                {
                    CodeableConcept code = DictToCodeableConcept(value);
                    code.Text = CauseOfDeathConditionJ.Code.Text;
                    CauseOfDeathConditionJ.Code = code;
                }
                else if (CauseOfDeathConditionJ != null)
                {
                    CauseOfDeathConditionJ.Code = DictToCodeableConcept(value);
                }
                else
                {
                    CauseOfDeathConditionJ = new Condition();
                    CauseOfDeathConditionJ.Id = Guid.NewGuid().ToString();
                    CauseOfDeathConditionJ.Meta = new Meta();
                    string[] condition_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Cause-Of-Death-Condition" };
                    CauseOfDeathConditionJ.Meta.Profile = condition_profile;
                    CauseOfDeathConditionJ.Code = DictToCodeableConcept(value);
                    CauseOfDeathConditionJ.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    CauseOfDeathConditionJ.Asserter = new ResourceReference("urn:uuid:" + Certifier.Id);
                    AddReferenceToComposition(CauseOfDeathConditionJ.Id);
                    Bundle.AddResourceEntry(CauseOfDeathConditionJ, "urn:uuid:" + CauseOfDeathConditionJ.Id);
                    List.EntryComponent entry = new List.EntryComponent();
                    entry.Item = new ResourceReference("urn:uuid:" + CauseOfDeathConditionJ.Id);
                    if (CauseOfDeathConditionPathway.Entry.Count() != 10)
                    {
                        foreach (var i in Enumerable.Range(0, 10)) { CauseOfDeathConditionPathway.Entry.Add(null); }
                    }
                    CauseOfDeathConditionPathway.Entry[9] = entry;
                }
            }
        }


        /////////////////////////////////////////////////////////////////////////////////
        //
        // Record Properties: Decedent Demographics
        //
        /////////////////////////////////////////////////////////////////////////////////

        /// <summary>Decedent's Given Name(s). Middle name should be the last entry.</summary>
        /// <value>the decedent's name (first, etc., middle)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>string[] names = { "Example", "Something", "Middle" };</para>
        /// <para>ExampleDeathRecord.GivenNames = names;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent Given Name(s): {string.Join(", ", ExampleDeathRecord.GivenNames)}");</para>
        /// </example>
        [Property("Given Names", Property.Types.StringArr, "Decedent Demographics", "Decedent's Given Name(s).", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 0)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient)", "name")]
        public string[] GivenNames
        {
            get
            {
                string[] names = GetAllString("Bundle.entry.resource.where($this is Patient).name.where(use='official').given");
                return names != null ? names : new string[0];
            }
            set
            {
                HumanName name = Decedent.Name.SingleOrDefault(n => n.Use == HumanName.NameUse.Official);
                if (name != null)
                {
                    name.Given = value;
                }
                else
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    name.Given = value;
                    Decedent.Name.Add(name);
                }
            }
        }

        /// <summary>Decedent's Family Name.</summary>
        /// <value>the decedent's family name (i.e. last name)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.FamilyName = "Last";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent's Last Name: {ExampleDeathRecord.FamilyName}");</para>
        /// </example>
        [Property("Family Name", Property.Types.String, "Decedent Demographics", "Decedent's Family Name.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 5)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient)", "name")]
        public string FamilyName
        {
            get
            {
                return GetFirstString("Bundle.entry.resource.where($this is Patient).name.where(use='official').family");
            }
            set
            {
                HumanName name = Decedent.Name.SingleOrDefault(n => n.Use == HumanName.NameUse.Official);
                if (name != null && !String.IsNullOrEmpty(value))
                {
                    name.Family = value;
                }
                else if (!String.IsNullOrEmpty(value))
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    name.Family = value;
                    Decedent.Name.Add(name);
                }
            }
        }

        /// <summary>Decedent's Suffix.</summary>
        /// <value>the decedent's suffix</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.Suffix = "Jr.";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent Suffix: {ExampleDeathRecord.Suffix}");</para>
        /// </example>
        [Property("Suffix", Property.Types.String, "Decedent Demographics", "Decedent's Suffix.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 6)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient)", "name")]
        public string Suffix
        {
            get
            {
                return GetFirstString("Bundle.entry.resource.where($this is Patient).name.where(use='official').suffix");
            }
            set
            {
                HumanName name = Decedent.Name.SingleOrDefault(n => n.Use == HumanName.NameUse.Official);
                if (name != null && !String.IsNullOrEmpty(value))
                {
                    string[] suffix = { value };
                    name.Suffix = suffix;
                }
                else if (!String.IsNullOrEmpty(value))
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    string[] suffix = { value };
                    name.Suffix = suffix;
                    Decedent.Name.Add(name);
                }
            }
        }

        /// <summary>Decedent's Alias Name(s). Middle name should be the last entry.</summary>
        /// <value>the decedent's alias name (first, etc., middle)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>string[] names = { "FirstAlias", "MiddleAlias" };</para>
        /// <para>ExampleDeathRecord.AliasGivenNames = names;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent Alias Given Name(s): {string.Join(", ", ExampleDeathRecord.AliasGivenNames)}");</para>
        /// </example>
        [Property("Alias Given Names", Property.Types.StringArr, "Decedent Demographics", "Decedent's Alias Given Name(s).", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", false, 7)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient).name.where(use='nickname')", "first")]
        public string[] AliasGivenNames
        {
            get
            {
                string[] names = GetAllString("Bundle.entry.resource.where($this is Patient).name.where(use='nickname').given");
                return names != null ? names : new string[0];
            }
            set
            {
                HumanName name = Decedent.Name.SingleOrDefault(n => n.Use == HumanName.NameUse.Nickname);
                if (name != null)
                {
                    name.Given = value;
                }
                else
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Nickname;
                    name.Given = value;
                    Decedent.Name.Add(name);
                }
            }
        }

        /// <summary>Decedent's Alias Family Name.</summary>
        /// <value>the decedent's alias family name (i.e. last name)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.AliasFamilyName = "AliasLast";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent's Alias Last Name: {ExampleDeathRecord.AliasFamilyName}");</para>
        /// </example>
        [Property("Alias Family Name", Property.Types.String, "Decedent Demographics", "Decedent's Alias Family Name.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", false, 8)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient).name.where(use='nickname')", "family")]
        public string AliasFamilyName
        {
            get
            {
                return GetFirstString("Bundle.entry.resource.where($this is Patient).name.where(use='nickname').family");
            }
            set
            {
                HumanName name = Decedent.Name.SingleOrDefault(n => n.Use == HumanName.NameUse.Nickname);
                if (name != null && !String.IsNullOrEmpty(value))
                {
                    name.Family = value;
                }
                else if (!String.IsNullOrEmpty(value))
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Nickname;
                    name.Family = value;
                    Decedent.Name.Add(name);
                }
            }
        }

        /// <summary>Decedent's Alias Suffix.</summary>
        /// <value>the decedent's alias suffix</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.AliasSuffix = "Jr.";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent Alias Suffix: {ExampleDeathRecord.AliasSuffix}");</para>
        /// </example>
        [Property("Alias Suffix", Property.Types.String, "Decedent Demographics", "Decedent's Alias Suffix.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", false, 9)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient).name.where(use='nickname')", "suffix")]
        public string AliasSuffix
        {
            get
            {
                return GetFirstString("Bundle.entry.resource.where($this is Patient).name.where(use='nickname').suffix");
            }
            set
            {
                HumanName name = Decedent.Name.SingleOrDefault(n => n.Use == HumanName.NameUse.Nickname);
                if (name != null && !String.IsNullOrEmpty(value))
                {
                    string[] suffix = { value };
                    name.Suffix = suffix;
                }
                else if (!String.IsNullOrEmpty(value))
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Nickname;
                    string[] suffix = { value };
                    name.Suffix = suffix;
                    Decedent.Name.Add(name);
                }
            }
        }

        /// <summary>Decedent's Maiden Name.</summary>
        /// <value>the decedent's maiden name (i.e. last name before marriage)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.MaidenName = "Last";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent's Maiden Name: {ExampleDeathRecord.MaidenName}");</para>
        /// </example>
        [Property("Maiden Name", Property.Types.String, "Decedent Demographics", "Decedent's Maiden Name.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 10)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient).name.where(use='maiden')", "family")]
        public string MaidenName
        {
            get
            {
                return GetFirstString("Bundle.entry.resource.where($this is Patient).name.where(use='maiden').family");
            }
            set
            {
                HumanName name = Decedent.Name.SingleOrDefault(n => n.Use == HumanName.NameUse.Maiden);
                if (name != null && !String.IsNullOrEmpty(value))
                {
                    name.Family = value;
                }
                else if (!String.IsNullOrEmpty(value))
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Maiden;
                    name.Family = value;
                    Decedent.Name.Add(name);
                }
            }
        }

        /// <summary>Decedent's Gender.</summary>
        /// <value>the decedent's gender</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.Gender = "female";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Gender: {ExampleDeathRecord.Gender}");</para>
        /// </example>
        [Property("Gender", Property.Types.String, "Decedent Demographics", "Decedent's Gender.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 11)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient)", "gender")]
        public string Gender
        {
            get
            {
                return GetFirstString("Bundle.entry.resource.where($this is Patient).gender");
            }
            set
            {
                switch(value)
                {
                    case "male":
                    case "Male":
                    case "m":
                    case "M":
                        Decedent.Gender = AdministrativeGender.Male;
                        break;
                    case "female":
                    case "Female":
                    case "f":
                    case "F":
                        Decedent.Gender = AdministrativeGender.Female;
                        break;
                    case "other":
                    case "Other":
                    case "o":
                    case "O":
                        Decedent.Gender = AdministrativeGender.Other;
                        break;
                    case "unknown":
                    case "Unknown":
                    case "u":
                    case "U":
                        Decedent.Gender = AdministrativeGender.Unknown;
                        break;
                }
            }
        }

        /// <summary>Decedent's Birth Sex.</summary>
        /// <value>the decedent's birth sex</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.BirthSex = "F";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Birth Sex: {ExampleDeathRecord.BirthSex}");</para>
        /// </example>
        [Property("Birth Sex", Property.Types.String, "Decedent Demographics", "Decedent's Birth Sex.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 12)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient).extension.where(url='http://hl7.org/fhir/us/core/StructureDefinition/us-core-birthsex')", "")]
        public string BirthSex
        {
            get
            {
                Extension birthsex = Decedent.Extension.Find(ext => ext.Url == "http://hl7.org/fhir/us/core/StructureDefinition/us-core-birthsex");
                if (birthsex != null && birthsex.Value != null && birthsex.Value.GetType() == typeof(Code))
                {
                    return ((Code)birthsex.Value).Value;
                }
                return null;
            }
            set
            {
                Decedent.Extension.RemoveAll(ext => ext.Url == "http://hl7.org/fhir/us/core/StructureDefinition/us-core-birthsex");
                Extension birthsex = new Extension();
                birthsex.Url = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-birthsex";
                birthsex.Value = new Code(value);
                Decedent.Extension.Add(birthsex);
            }
        }

        /// <summary>Decedent's Date of Birth.</summary>
        /// <value>the decedent's date of birth</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.DateOfBirth = "1940-02-19";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent Date of Birth: {ExampleDeathRecord.DateOfBirth}");</para>
        /// </example>
        [Property("Date Of Birth", Property.Types.String, "Decedent Demographics", "Decedent's Date of Birth.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 14)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient)", "birthDate")]
        public string DateOfBirth
        {
            get
            {
                return GetFirstString("Bundle.entry.resource.where($this is Patient).birthDate");
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    return;
                }
                Decedent.BirthDate = value.Trim();
            }
        }

        /// <summary>Decedent's residence.</summary>
        /// <value>Decedent's residence. A Dictionary representing residence address, containing the following key/value pairs:
        /// <para>"addressLine1" - address, line one</para>
        /// <para>"addressLine2" - address, line two</para>
        /// <para>"addressCity" - address, city</para>
        /// <para>"addressCounty" - address, county</para>
        /// <para>"addressState" - address, state</para>
        /// <para>"addressZip" - address, zip</para>
        /// <para>"addressCountry" - address, country</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; address = new Dictionary&lt;string, string&gt;();</para>
        /// <para>address.Add("addressLine1", "123 Test Street");</para>
        /// <para>address.Add("addressLine2", "Unit 3");</para>
        /// <para>address.Add("addressCity", "Boston");</para>
        /// <para>address.Add("addressCounty", "Suffolk");</para>
        /// <para>address.Add("addressState", "MA");</para>
        /// <para>address.Add("addressZip", "12345");</para>
        /// <para>address.Add("addressCountry", "United States");</para>
        /// <para>SetterDeathRecord.Residence = address;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"State of residence: {ExampleDeathRecord.Residence["addressState"]}");</para>
        /// </example>
        [Property("Residence", Property.Types.Dictionary, "Decedent Demographics", "Decedent's residence.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 19)]
        [PropertyParam("addressLine1", "address, line one")]
        [PropertyParam("addressLine2", "address, line two")]
        [PropertyParam("addressCity", "address, city")]
        [PropertyParam("addressCounty", "address, county")]
        [PropertyParam("addressState", "address, state")]
        [PropertyParam("addressZip", "address, zip")]
        [PropertyParam("addressCountry", "address, country")]
        [FHIRPath("Bundle.entry.resource.where($this is Patient)", "address")]
        public Dictionary<string, string> Residence
        {
            get
            {
                if (Decedent != null && Decedent.Address != null && Decedent.Address.Count() > 0)
                {
                    return AddressToDict(Decedent.Address.First());
                }
                return EmptyAddrDict();
            }
            set
            {
                if (ResidenceWithinCityLimitsBoolean == false)
                {
                    Decedent.Address.Clear();
                    Decedent.Address.Add(DictToAddress(value));
                    ResidenceWithinCityLimitsBoolean = false;
                }
                if (ResidenceWithinCityLimitsBoolean == true)
                {
                    Decedent.Address.Clear();
                    Decedent.Address.Add(DictToAddress(value));
                    ResidenceWithinCityLimitsBoolean = true;
                }
                else
                {
                    Decedent.Address.Clear();
                    Decedent.Address.Add(DictToAddress(value));
                }
            }
        }

        /// <summary>Decedent's residence is/is not within city limits.</summary>
        /// <value>Decedent's residence is/is not within city limits. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para></value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; within = new Dictionary&lt;string, string&gt;();</para>
        /// <para>within.Add("code", "Y");</para>
        /// <para>within.Add("system", "http://terminology.hl7.org/CodeSystem/v2-0136");</para>
        /// <para>within.Add("display", "Yes");</para>
        /// <para>SetterDeathRecord.ResidenceWithinCityLimits = within;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Residence within city limits: {ExampleDeathRecord.ResidenceWithinCityLimits['display']}");</para>
        /// </example>
        [Property("Residence Within City Limits", Property.Types.Dictionary, "Decedent Demographics", "Decedent's residence is/is not within city limits.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 20)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Patient)", "address")]
        public Dictionary<string, string> ResidenceWithinCityLimits
        {
            get
            {
                if (Decedent != null && Decedent.Address.FirstOrDefault() != null)
                {
                    Extension cityLimits = Decedent.Address.FirstOrDefault().Extension.Find(ext => ext.Url == "http://hl7.org/fhir/us/vrdr/StructureDefinition/Within-City-Limits-Indicator");
                    if (cityLimits != null && cityLimits.Value != null && cityLimits.Value.GetType() == typeof(Coding))
                    {
                        return CodingToDict((Coding)cityLimits.Value);
                    }
                }
                return EmptyCodeDict();
            }
            set
            {
                if (Decedent != null)
                {
                    if (Decedent.Address.FirstOrDefault() == null)
                    {
                        Decedent.Address.Add(new Address());
                    }
                    Decedent.Address.FirstOrDefault().Extension.RemoveAll(ext => ext.Url == "http://hl7.org/fhir/us/vrdr/StructureDefinition/Within-City-Limits-Indicator");
                    Extension withinCityLimits = new Extension();
                    withinCityLimits.Url = "http://hl7.org/fhir/us/vrdr/StructureDefinition/Within-City-Limits-Indicator";
                    withinCityLimits.Value = DictToCoding(value);
                    Decedent.Address.FirstOrDefault().Extension.Add(withinCityLimits);
                }
            }
        }

        /// <summary>Decedent's residence is/is not within city limits. This is a convenience method, to access the coded value use ResidenceWithinCityLimits.</summary>
        /// <value>Decedent's residence is/is not within city limits. A null value means "not applicable".</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>SetterDeathRecord.ResidenceWithinCityLimitsBoolean = true;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Residence within city limits: {ExampleDeathRecord.ResidenceWithinCityLimitsBoolean}");</para>
        /// </example>
        [Property("Residence Within City Limits Boolean", Property.Types.Bool, "Decedent Demographics", "Decedent's residence is/is not within city limits.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", false, 21)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient)", "address")]
        public bool? ResidenceWithinCityLimitsBoolean
        {
            get
            {
                var code = this.ResidenceWithinCityLimits;
                switch (code["code"])
                {
                    case "Y": // Yes
                        return true;
                    case "N": // No
                        return false;
                    default: // Not applicable
                        return null;
                }
            }
            set
            {
                var code = EmptyCodeDict();
                switch(value)
                {
                    case true:
                        code["code"] = "Y";
                        code["display"] = "Yes";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    case false:
                        code["code"] = "N";
                        code["display"] = "No";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    default:
                        code["code"] = "NA";
                        code["display"] = "not applicable";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v3-NullFlavor";
                        break;
                }
                this.ResidenceWithinCityLimits = code;
            }
        }

        /// <summary>Decedent's Social Security Number.</summary>
        /// <value>the decedent's social security number, without dashes.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.SSN = "12345678";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent Suffix: {ExampleDeathRecord.SSN}");</para>
        /// </example>
        [Property("SSN", Property.Types.String, "Decedent Demographics", "Decedent's Social Security Number.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 13)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient).identifier.where(system='http://hl7.org/fhir/sid/us-ssn')", "")]
        public string SSN
        {
            get
            {
                return GetFirstString("Bundle.entry.resource.where($this is Patient).identifier.where(system = 'http://hl7.org/fhir/sid/us-ssn').value");
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    return;
                }
                Decedent.Identifier.RemoveAll(iden => iden.System == "http://hl7.org/fhir/sid/us-ssn");
                Identifier ssn = new Identifier();
                ssn.Type = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "SB", "Social Beneficiary Identifier", null);
                ssn.System = "http://hl7.org/fhir/sid/us-ssn";
                ssn.Value = value.Replace("-", string.Empty);
                Decedent.Identifier.Add(ssn);
            }
        }

        /// <summary>Decedent's Ethnicity.</summary>
        /// <value>the decedent's ethnicity as a text string. Use the Ethnicity property to set coded ethnicity.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.EthnicityText = "Hispanic or Latino";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent Ethnicity Text: {ExampleDeathRecord.EthnicityText}");</para>
        /// </example>
        [Property("EthnicityText", Property.Types.String, "Decedent Demographics", "Decedent's Ethnicity.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 35)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient).extension.where(url='http://hl7.org/fhir/us/core/StructureDefinition/us-core-ethnicity')", "")]
        public string EthnicityText
        {
            get
            {
                Extension ethnicity = Decedent.Extension.Where(ext => ext.Url == "http://hl7.org/fhir/us/core/StructureDefinition/us-core-ethnicity").FirstOrDefault();
                if (ethnicity != null)
                {
                    Extension ethnicityText = ethnicity.Extension.Where(ext => ext.Url == "text").FirstOrDefault();
                    if (ethnicityText != null && ethnicityText.Value.GetType() == typeof(FhirString))
                    {
                        return ethnicityText.Value.ToString();
                    }
                }
                return null;
            }
            set
            {
                Extension ethnicity = Decedent.Extension.Where(ext => ext.Url == "http://hl7.org/fhir/us/core/StructureDefinition/us-core-ethnicity").FirstOrDefault();
                if (ethnicity == null)
                {
                    ethnicity = new Extension();
                    ethnicity.Url = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-ethnicity";
                    Decedent.Extension.Add(ethnicity);
                }
                Extension ethnicityText = ethnicity.Extension.Where(ext => ext.Url == "text").FirstOrDefault();
                if (ethnicityText == null)
                {
                    ethnicityText = new Extension();
                    ethnicityText.Url = "text";
                    ethnicity.Extension.Add(ethnicityText);
                }
                ethnicityText.Value = new FhirString(value);
            }
        }

        /// <summary>Decedent's Ethnicity.</summary>
        /// <value>the decedent's ethnicity. An array of tuples, where the first value of each tuple is the display value, and the second is
        /// the code. Use the EthnicityText property to set this value as a simple string, setting the value of this property will create a default value for the
        /// EthnicityText property by concatenating the supplied code display texts.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Tuple&lt;string, string&gt;[] ethnicity = { Tuple.Create("Non Hispanic or Latino", "2186-5"), Tuple.Create("Salvadoran", "2161-8") };</para>
        /// <para>ExampleDeathRecord.Ethnicity = ethnicity;</para>
        /// <para>// Getter:</para>
        /// <para>foreach(var pair in deathRecord.Ethnicity)</para>
        /// <para>{</para>
        /// <para>  Console.WriteLine($"\tEthnicity text: {pair.Key}: code: {pair.Value}");</para>
        /// <para>};</para>
        /// </example>
        [Property("Ethnicity", Property.Types.TupleArr, "Decedent Demographics", "Decedent's Ethnicity.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 36)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient).extension.where(url='http://hl7.org/fhir/us/core/StructureDefinition/us-core-ethnicity')", "")]
        public Tuple<string, string>[] Ethnicity
        {
            get
            {
                string[] ombCodes = new string[] { };
                string[] detailedCodes = new string[] { };
                string[] ombDisplays = new string[] { };
                string[] detailedDisplays = new string[] { };
                ombCodes = GetAllString("Bundle.entry.resource.where($this is Patient).extension.where(url = 'http://hl7.org/fhir/us/core/StructureDefinition/us-core-ethnicity').extension.where(url = 'ombCategory').value.code");
                detailedCodes = GetAllString("Bundle.entry.resource.where($this is Patient).extension.where(url = 'http://hl7.org/fhir/us/core/StructureDefinition/us-core-ethnicity').extension.where(url = 'detailed').value.code");
                ombDisplays = GetAllString("Bundle.entry.resource.where($this is Patient).extension.where(url = 'http://hl7.org/fhir/us/core/StructureDefinition/us-core-ethnicity').extension.where(url = 'ombCategory').value.display");
                detailedDisplays = GetAllString("Bundle.entry.resource.where($this is Patient).extension.where(url = 'http://hl7.org/fhir/us/core/StructureDefinition/us-core-ethnicity').extension.where(url = 'detailed').value.display");
                Tuple<string, string>[] ethnicityList = new Tuple<string, string>[ombCodes.Length + detailedCodes.Length];
                for (int i = 0; i < ombCodes.Length; i++)
                {
                    if (!String.IsNullOrWhiteSpace(MortalityData.EthnicityCodeToEthnicityName(ombCodes[i])))
                    {
                        ethnicityList[i] = (Tuple.Create(MortalityData.EthnicityCodeToEthnicityName(ombCodes[i]), String.IsNullOrWhiteSpace(ombCodes[i]) ? null : ombCodes[i]));
                    }
                    else
                    {
                        ethnicityList[i] = (Tuple.Create(ombDisplays[i], String.IsNullOrWhiteSpace(ombCodes[i]) ? null : ombCodes[i]));
                    }
                }
                for (int i = 0; i < detailedCodes.Length; i++)
                {
                    if (!String.IsNullOrWhiteSpace(MortalityData.EthnicityCodeToEthnicityName(detailedCodes[i])))
                    {
                        ethnicityList[ombCodes.Length + i] = (Tuple.Create(MortalityData.EthnicityCodeToEthnicityName(detailedCodes[i]), String.IsNullOrWhiteSpace(detailedCodes[i]) ? null : detailedCodes[i]));
                    }
                    else
                    {
                        ethnicityList[ombCodes.Length + i] = (Tuple.Create(detailedDisplays[i], String.IsNullOrWhiteSpace(detailedCodes[i]) ? null : detailedCodes[i]));
                    }
                }
                return ethnicityList;
            }
            set
            {
                Decedent.Extension.RemoveAll(ext => ext.Url == "http://hl7.org/fhir/us/core/StructureDefinition/us-core-ethnicity");
                Extension ethnicities = new Extension();
                ethnicities.Url = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-ethnicity";
                List<string> textEthStrs = new List<string>();
                foreach (Tuple<string, string> element in value)
                {
                    string display = element.Item1;
                    string code = element.Item2;
                    textEthStrs.Add(display);
                    Extension codeEthnicity = new Extension();
                    if (code == "2135-2" || code == "2186-5")
                    {
                        codeEthnicity.Url = "ombCategory";
                    }
                    else if (display.Equals("Hispanic or Latino", StringComparison.InvariantCultureIgnoreCase) ||
                             display.Equals("Not Hispanic or Latino", StringComparison.InvariantCultureIgnoreCase))
                    {
                        codeEthnicity.Url = "ombCategory";
                    }
                    else
                    {
                        codeEthnicity.Url = "detailed";
                    }
                    Coding coding = new Coding();
                    coding.System = "urn:oid:2.16.840.1.113883.6.238";
                    if (!String.IsNullOrWhiteSpace(code))
                    {
                        coding.Code = code;
                    }
                    if (!String.IsNullOrWhiteSpace(display))
                    {
                        coding.Display = display;
                    }
                    codeEthnicity.Value = coding;
                    ethnicities.Extension.Add(codeEthnicity);
                }
                Extension textEth = new Extension();
                textEth.Url = "text";
                textEth.Value = new FhirString(String.Join(", ", textEthStrs));
                ethnicities.Extension.Add(textEth);
                Decedent.Extension.Add(ethnicities);
            }
        }

        /// <summary>Decedent's Race.</summary>
        /// <value>the decedent's race as a text string. Use the Race property to set coded race.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.RaceText = "White";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent Race Text: {ExampleDeathRecord.RaceText}");</para>
        /// </example>
        [Property("RaceText", Property.Types.String, "Decedent Demographics", "Decedent's Race.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 37)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient).extension.where(url='http://hl7.org/fhir/us/core/StructureDefinition/us-core-race')", "")]
        public string RaceText
        {
            get
            {
                Extension race = Decedent.Extension.Where(ext => ext.Url == "http://hl7.org/fhir/us/core/StructureDefinition/us-core-race").FirstOrDefault();
                if (race != null)
                {
                    Extension raceText = race.Extension.Where(ext => ext.Url == "text").FirstOrDefault();
                    if (raceText != null && raceText.Value.GetType() == typeof(FhirString))
                    {
                        return raceText.Value.ToString();
                    }
                }
                return null;
            }
            set
            {
                Extension race = Decedent.Extension.Where(ext => ext.Url == "http://hl7.org/fhir/us/core/StructureDefinition/us-core-race").FirstOrDefault();
                if (race == null)
                {
                    race = new Extension();
                    race.Url = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-race";
                    Decedent.Extension.Add(race);
                }
                Extension raceText = race.Extension.Where(ext => ext.Url == "text").FirstOrDefault();
                if (raceText == null)
                {
                    raceText = new Extension();
                    raceText.Url = "text";
                    race.Extension.Add(raceText);
                }
                raceText.Value = new FhirString(value);
            }
        }

        /// <summary>Decedent's Race.</summary>
        /// <value>the decedent's race. An array of tuples, where the first value of each tuple is the display value, and the second is
        /// the code. Use the RaceText property to set this value as a simple string, setting the value of this property will create a default value for the
        /// RaceText property by concatenating the supplied code display texts.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Tuple&lt;string, string&gt;[] race = { Tuple.Create("Non Hispanic or Latino", "2186-5"), Tuple.Create("Salvadoran", "2161-8") };</para>
        /// <para>ExampleDeathRecord.Race = race;</para>
        /// <para>// Getter:</para>
        /// <para>foreach(var pair in ExampleDeathRecord.race)</para>
        /// <para>{</para>
        /// <para>   Console.WriteLine($"\Race text: {pair.Key}: code: {pair.Value}");</para>
        /// <para>};</para>
        /// </example>
        [Property("Race", Property.Types.TupleArr, "Decedent Demographics", "Decedent's Race.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 38)]
        [FHIRPath("Bundle.entry.resource.where($this is Patient).extension.where(url='http://hl7.org/fhir/us/core/StructureDefinition/us-core-race')", "")]
        public Tuple<string, string>[] Race
        {
            get
            {
                string[] ombCodes = new string[] { };
                string[] detailedCodes = new string[] { };
                string[] ombDisplays = new string[] { };
                string[] detailedDisplays = new string[] { };
                ombCodes = GetAllString("Bundle.entry.resource.where($this is Patient).extension.where(url = 'http://hl7.org/fhir/us/core/StructureDefinition/us-core-race').extension.where(url = 'ombCategory').value.code");
                detailedCodes = GetAllString("Bundle.entry.resource.where($this is Patient).extension.where(url = 'http://hl7.org/fhir/us/core/StructureDefinition/us-core-race').extension.where(url = 'detailed').value.code");
                ombDisplays = GetAllString("Bundle.entry.resource.where($this is Patient).extension.where(url = 'http://hl7.org/fhir/us/core/StructureDefinition/us-core-race').extension.where(url = 'ombCategory').value.display");
                detailedDisplays = GetAllString("Bundle.entry.resource.where($this is Patient).extension.where(url = 'http://hl7.org/fhir/us/core/StructureDefinition/us-core-race').extension.where(url = 'detailed').value.display");
                Tuple<string, string>[] raceList = new Tuple<string, string>[ombCodes.Length + detailedCodes.Length];
                for (int i = 0; i < ombCodes.Length; i++)
                {
                    if (!String.IsNullOrWhiteSpace(MortalityData.RaceCodeToRaceName(ombCodes[i])))
                    {
                        raceList[i] = (Tuple.Create(MortalityData.RaceCodeToRaceName(ombCodes[i]), String.IsNullOrWhiteSpace(ombCodes[i]) ? null : ombCodes[i]));
                    }
                    else
                    {
                        raceList[i] = (Tuple.Create(ombDisplays[i], String.IsNullOrWhiteSpace(ombCodes[i]) ? null : ombCodes[i]));
                    }
                }
                for (int i = 0; i < detailedCodes.Length; i++)
                {
                    if (!String.IsNullOrWhiteSpace(MortalityData.RaceCodeToRaceName(detailedCodes[i])))
                    {
                        raceList[ombCodes.Length + i] = (Tuple.Create(MortalityData.RaceCodeToRaceName(detailedCodes[i]), String.IsNullOrWhiteSpace(detailedCodes[i]) ? null : detailedCodes[i]));
                    }
                    else
                    {
                        raceList[ombCodes.Length + i] = (Tuple.Create(detailedDisplays[i], String.IsNullOrWhiteSpace(detailedCodes[i]) ? null : detailedCodes[i]));
                    }
                }
                return raceList;
            }
            set
            {
                Decedent.Extension.RemoveAll(ext => ext.Url == "http://hl7.org/fhir/us/core/StructureDefinition/us-core-race");
                Extension races = new Extension();
                races.Url = "http://hl7.org/fhir/us/core/StructureDefinition/us-core-race";
                List<string> textRaceStrs = new List<string>();
                foreach(Tuple<string,string> element in value)
                {
                    string display = element.Item1;
                    string code = element.Item2;
                    textRaceStrs.Add(display);
                    Extension codeRace = new Extension();
                    if (code == "1002-5" || code == "2028-9" || code == "2054-5" || code == "2076-8" || code == "2106-3")
                    {
                        codeRace.Url = "ombCategory";
                    }
                    else if (display.Equals("American Indian or Alaska Native", StringComparison.InvariantCultureIgnoreCase) ||
                             display.Equals("Asian", StringComparison.InvariantCultureIgnoreCase) ||
                             display.Equals("Black or African American", StringComparison.InvariantCultureIgnoreCase) ||
                             display.Equals("Native Hawaiian or Other Pacific Islander", StringComparison.InvariantCultureIgnoreCase) ||
                             display.Equals("White", StringComparison.InvariantCultureIgnoreCase))
                    {
                        codeRace.Url = "ombCategory";
                    }
                    else
                    {
                        codeRace.Url = "detailed";
                    }
                    Coding coding = new Coding();
                    coding.System = "urn:oid:2.16.840.1.113883.6.238";
                    if (!String.IsNullOrWhiteSpace(code))
                    {
                        coding.Code = code;
                    }
                    if (!String.IsNullOrWhiteSpace(display))
                    {
                        coding.Display = display;
                    }
                    codeRace.Value = coding;
                    races.Extension.Add(codeRace);
                }
                Extension textRace = new Extension();
                textRace.Url = "text";
                textRace.Value = new FhirString(String.Join(", ", textRaceStrs));
                races.Extension.Add(textRace);
                Decedent.Extension.Add(races);
            }
        }

        /// <summary>Decedent's Place Of Birth.</summary>
        /// <value>decedent's Place Of Birth. A Dictionary representing residence address, containing the following key/value pairs:
        /// <para>"addressLine1" - address, line one</para>
        /// <para>"addressLine2" - address, line two</para>
        /// <para>"addressCity" - address, city</para>
        /// <para>"addressCounty" - address, county</para>
        /// <para>"addressState" - address, state</para>
        /// <para>"addressZip" - address, zip</para>
        /// <para>"addressCountry" - address, country</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; address = new Dictionary&lt;string, string&gt;();</para>
        /// <para>address.Add("addressLine1", "123 Test Street");</para>
        /// <para>address.Add("addressLine2", "Unit 3");</para>
        /// <para>address.Add("addressCity", "Boston");</para>
        /// <para>address.Add("addressCounty", "Suffolk");</para>
        /// <para>address.Add("addressState", "MA");</para>
        /// <para>address.Add("addressZip", "12345");</para>
        /// <para>address.Add("addressCountry", "United States");</para>
        /// <para>SetterDeathRecord.PlaceOfBirth = address;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"State where decedent was born: {ExampleDeathRecord.PlaceOfBirth["placeOfBirthState"]}");</para>
        /// </example>
        [Property("Place Of Birth", Property.Types.Dictionary, "Decedent Demographics", "Decedent's Place Of Birth.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 15)]
        [PropertyParam("addressLine1", "address, line one")]
        [PropertyParam("addressLine2", "address, line two")]
        [PropertyParam("addressCity", "address, city")]
        [PropertyParam("addressCounty", "address, county")]
        [PropertyParam("addressState", "address, state")]
        [PropertyParam("addressZip", "address, zip")]
        [PropertyParam("addressCountry", "address, country")]
        [FHIRPath("Bundle.entry.resource.where($this is Patient).extension.where(url='http://hl7.org/fhir/StructureDefinition/patient-birthPlace')", "")]
        public Dictionary<string, string> PlaceOfBirth
        {
            get
            {
                Extension addressExt = Decedent.Extension.FirstOrDefault( extension => extension.Url == "http://hl7.org/fhir/StructureDefinition/patient-birthPlace" );
                if (addressExt != null)
                {
                    Address address = (Address)addressExt.Value;
                    if (address != null)
                    {
                        return AddressToDict((Address)address);
                    }
                    return EmptyAddrDict();
                }
                return EmptyAddrDict();
            }
            set
            {
                Decedent.Extension.RemoveAll(ext => ext.Url == "http://hl7.org/fhir/StructureDefinition/patient-birthPlace");
                Extension placeOfBirthExt = new Extension();
                placeOfBirthExt.Url = "http://hl7.org/fhir/StructureDefinition/patient-birthPlace";
                placeOfBirthExt.Value = DictToAddress(value);
                Decedent.Extension.Add(placeOfBirthExt);
            }
        }

        /// <summary>The marital status of the decedent at the time of death.</summary>
        /// <value>the marital status of the decedent at the time of death. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code describing this finding</para>
        /// <para>"system" - the system the given code belongs to</para>
        /// <para>"display" - the human readable display text that corresponds to the given code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "S");</para>
        /// <para>code.Add("system", "http://terminology.hl7.org/CodeSystem/v3-MaritalStatus");</para>
        /// <para>code.Add("display", "Never Married");</para>
        /// <para>ExampleDeathRecord.MaritalStatus = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Marital status: {ExampleDeathRecord.MaritalStatus["display"]}");</para>
        /// </example>
        [Property("Marital Status", Property.Types.Dictionary, "Decedent Demographics", "The marital status of the decedent at the time of death.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent.html", true, 24)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Patient)", "maritalStatus")]
        public Dictionary<string, string> MaritalStatus
        {
            get
            {
                if (Decedent != null && Decedent.MaritalStatus != null)
                {
                    return CodeableConceptToDict(Decedent.MaritalStatus);
                }
                return EmptyCodeDict();
            }
            set
            {
                Decedent.MaritalStatus = DictToCodeableConcept(value);
            }
        }

        /// <summary>Given name(s) of decedent's father.</summary>
        /// <value>the decedent's father's name (first, middle, etc.)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>string[] names = { "Dad", "Middle" };</para>
        /// <para>ExampleDeathRecord.FatherGivenNames = names;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Father Given Name(s): {string.Join(", ", ExampleDeathRecord.FatherGivenNames)}");</para>
        /// </example>
        [Property("Father Given Names", Property.Types.StringArr, "Decedent Demographics", "Given name(s) of decedent's father.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Father.html", false, 28)]
        [FHIRPath("Bundle.entry.resource.where($this is RelatedPerson).where(relationship.coding.code='FTH')", "name")]
        public string[] FatherGivenNames
        {
            get
            {
                if (Father != null && Father.Name != null && Father.Name.Count() > 0 && Father.Name.First().Given != null) {
                    return Father.Name.First().Given.ToArray();
                }
                return new string[0];
            }
            set
            {
                if (Father == null)
                {
                    Father = new RelatedPerson();
                    Father.Id = Guid.NewGuid().ToString();
                    Father.Meta = new Meta();
                    string[] father_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Father" };
                    Father.Meta.Profile = father_profile;
                    Father.Patient = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Father.Relationship.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-RoleCode", "FTH", "father", null));
                    HumanName name = new HumanName();
                    name.Given = value;
                    Father.Name.Add(name);
                    AddReferenceToComposition(Father.Id);
                    Bundle.AddResourceEntry(Father, "urn:uuid:" + Father.Id);
                }
                else
                {
                    Father.Name.First().Given = value;
                }

            }
        }

        /// <summary>Family name of decedent's father.</summary>
        /// <value>the decedent's father's family name (i.e. last name)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.FatherFamilyName = "Last";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Father's Last Name: {ExampleDeathRecord.FatherFamilyName}");</para>
        /// </example>
        [Property("Father Family Name", Property.Types.String, "Decedent Demographics", "Family name of decedent's father.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Father.html", false, 29)]
        [FHIRPath("Bundle.entry.resource.where($this is RelatedPerson).where(relationship.coding.code='FTH')", "name")]
        public string FatherFamilyName
        {
            get
            {
                if (Father != null && Father.Name != null && Father.Name.Count() > 0 && Father.Name.First().Family != null) {
                    return Father.Name.First().Family;
                }
                return null;
            }
            set
            {
                if (Father == null)
                {
                    Father = new RelatedPerson();
                    Father.Id = Guid.NewGuid().ToString();
                    Father.Meta = new Meta();
                    string[] father_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Father" };
                    Father.Meta.Profile = father_profile;
                    Father.Patient = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Father.Relationship.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-RoleCode", "FTH", "father", null));
                    HumanName name = new HumanName();
                    name.Family = value;
                    Father.Name.Add(name);
                    AddReferenceToComposition(Father.Id);
                    Bundle.AddResourceEntry(Father, "urn:uuid:" + Father.Id);
                }
                else
                {
                    Father.Name.First().Family = value;
                }

            }
        }

        /// <summary>Father's Suffix.</summary>
        /// <value>the decedent's father's suffix</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.FatherSuffix = "Jr.";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Father Suffix: {ExampleDeathRecord.FatherSuffix}");</para>
        /// </example>
        [Property("Father Suffix", Property.Types.String, "Decedent Demographics", "Father's Suffix.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Father.html", false, 30)]
        [FHIRPath("Bundle.entry.resource.where($this is RelatedPerson).where(relationship.coding.code='FTH')", "name")]
        public string FatherSuffix
        {
            get
            {
                if (Father != null && Father.Name != null && Father.Name.Count() > 0 && Father.Name.First().Suffix != null) {

                    return Father.Name.First().Suffix.FirstOrDefault();
                }
                return null;
            }
            set
            {
                if (Father == null)
                {
                    Father = new RelatedPerson();
                    Father.Id = Guid.NewGuid().ToString();
                    Father.Meta = new Meta();
                    string[] father_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Father" };
                    Father.Meta.Profile = father_profile;
                    Father.Patient = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Father.Relationship.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-RoleCode", "FTH", "father", null));
                    HumanName name = new HumanName();
                    string[] suffix = { value };
                    name.Suffix = suffix;
                    Father.Name.Add(name);
                    AddReferenceToComposition(Father.Id);
                    Bundle.AddResourceEntry(Father, "urn:uuid:" + Father.Id);
                }
                else
                {
                    string[] suffix = { value };
                    Father.Name.First().Suffix = suffix;
                }

            }
        }

        /// <summary>Given name(s) of decedent's mother.</summary>
        /// <value>the decedent's mother's name (first, middle, etc.)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>string[] names = { "Mom", "Middle" };</para>
        /// <para>ExampleDeathRecord.MotherGivenNames = names;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Mother Given Name(s): {string.Join(", ", ExampleDeathRecord.MotherGivenNames)}");</para>
        /// </example>
        [Property("Mother Given Names", Property.Types.StringArr, "Decedent Demographics", "Given name(s) of decedent's mother.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Mother.html", false, 31)]
        [FHIRPath("Bundle.entry.resource.where($this is RelatedPerson).where(relationship.coding.code='MTH')", "name")]
        public string[] MotherGivenNames
        {
            get
            {
                if (Mother != null && Mother.Name != null && Mother.Name.Count() > 0 && Mother.Name.First().Given != null) {
                    return Mother.Name.First().Given.ToArray();
                }
                return new string[0];
            }
            set
            {
                if (Mother == null)
                {
                    Mother = new RelatedPerson();
                    Mother.Id = Guid.NewGuid().ToString();
                    Mother.Meta = new Meta();
                    string[] mother_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Mother" };
                    Mother.Meta.Profile = mother_profile;
                    Mother.Patient = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Mother.Relationship.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-RoleCode", "MTH", "mother", null));
                    HumanName name = new HumanName();
                    name.Given = value;
                    Mother.Name.Add(name);
                    AddReferenceToComposition(Mother.Id);
                    Bundle.AddResourceEntry(Mother, "urn:uuid:" + Mother.Id);
                }
                else
                {
                    Mother.Name.First().Given = value;
                }

            }
        }

        /// <summary>Maiden name of decedent's mother.</summary>
        /// <value>the decedent's mother's maiden name (i.e. last name before marriage)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.MotherMaidenName = "Last";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Mother's Maiden Name: {ExampleDeathRecord.MotherMaidenName}");</para>
        /// </example>
        [Property("Mother Maiden Name", Property.Types.String, "Decedent Demographics", "Maiden name of decedent's mother.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Mother.html", false, 32)]
        [FHIRPath("Bundle.entry.resource.where($this is RelatedPerson).where(relationship.coding.code='MTH')", "name")]
        public string MotherMaidenName
        {
            get
            {
                if (Mother != null && Mother.Name != null && Mother.Name.Count() > 0 && Mother.Name.First().Family != null) {
                    return Mother.Name.First().Family;
                }
                return null;
            }
            set
            {
                if (Mother == null)
                {
                    Mother = new RelatedPerson();
                    Mother.Id = Guid.NewGuid().ToString();
                    Mother.Meta = new Meta();
                    string[] mother_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Mother" };
                    Mother.Meta.Profile = mother_profile;
                    Mother.Patient = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Mother.Relationship.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-RoleCode", "MTH", "mother", null));
                    HumanName name = new HumanName();
                    name.Family = value;
                    Mother.Name.Add(name);
                    AddReferenceToComposition(Mother.Id);
                    Bundle.AddResourceEntry(Mother, "urn:uuid:" + Mother.Id);
                }
                else
                {
                    Mother.Name.First().Family = value;
                }

            }
        }

        /// <summary>Mother's Suffix.</summary>
        /// <value>the decedent's mother's suffix</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.MotherSuffix = "Jr.";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Mother Suffix: {ExampleDeathRecord.MotherSuffix}");</para>
        /// </example>
        [Property("Mother Suffix", Property.Types.String, "Decedent Demographics", "Mother's Suffix.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Mother.html", false, 33)]
        [FHIRPath("Bundle.entry.resource.where($this is RelatedPerson).where(relationship.coding.code='MTH')", "name")]
        public string MotherSuffix
        {
            get
            {
                if (Mother != null && Mother.Name != null && Mother.Name.Count() > 0 && Mother.Name.First().Suffix != null) {

                    return Mother.Name.First().Suffix.FirstOrDefault();
                }
                return null;
            }
            set
            {
                if (Mother == null)
                {
                    Mother = new RelatedPerson();
                    Mother.Id = Guid.NewGuid().ToString();
                    Mother.Meta = new Meta();
                    string[] mother_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Mother" };
                    Mother.Meta.Profile = mother_profile;
                    Mother.Patient = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Mother.Relationship.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-RoleCode", "MTH", "mother", null));
                    HumanName name = new HumanName();
                    string[] suffix = { value };
                    name.Suffix = suffix;
                    Mother.Name.Add(name);
                    AddReferenceToComposition(Mother.Id);
                    Bundle.AddResourceEntry(Mother, "urn:uuid:" + Mother.Id);
                }
                else
                {
                    string[] suffix = { value };
                    Mother.Name.First().Suffix = suffix;
                }

            }
        }

        /// <summary>Given name(s) of decedent's spouse.</summary>
        /// <value>the decedent's spouse's name (first, middle, etc.)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>string[] names = { "Spouse", "Middle" };</para>
        /// <para>ExampleDeathRecord.SpouseGivenNames = names;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Spouse Given Name(s): {string.Join(", ", ExampleDeathRecord.SpouseGivenNames)}");</para>
        /// </example>
        [Property("Spouse Given Names", Property.Types.StringArr, "Decedent Demographics", "Given name(s) of decedent's spouse.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Spouse.html", false, 25)]
        [FHIRPath("Bundle.entry.resource.where($this is RelatedPerson).where(relationship.coding.code='SPS')", "name")]
        public string[] SpouseGivenNames
        {
            get
            {
                if (Spouse != null && Spouse.Name != null && Spouse.Name.Count() > 0 && Spouse.Name.First().Given != null) {
                    return Spouse.Name.First().Given.ToArray();
                }
                return new string[0];
            }
            set
            {
                if (Spouse == null)
                {
                    Spouse = new RelatedPerson();
                    Spouse.Id = Guid.NewGuid().ToString();
                    Spouse.Meta = new Meta();
                    string[] spouse_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Spouse" };
                    Spouse.Meta.Profile = spouse_profile;
                    Spouse.Patient = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Spouse.Relationship.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-RoleCode", "SPS", "spouse", null));
                    HumanName name = new HumanName();
                    name.Given = value;
                    Spouse.Name.Add(name);
                    AddReferenceToComposition(Spouse.Id);
                    Bundle.AddResourceEntry(Spouse, "urn:uuid:" + Spouse.Id);
                }
                else
                {
                    Spouse.Name.First().Given = value;
                }

            }
        }

        /// <summary>Family name of decedent's spouse.</summary>
        /// <value>the decedent's spouse's family name (i.e. last name)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.SpouseFamilyName = "Last";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Spouse's Last Name: {ExampleDeathRecord.SpouseFamilyName}");</para>
        /// </example>
        [Property("Spouse Family Name", Property.Types.String, "Decedent Demographics", "Family name of decedent's spouse.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Spouse.html", false, 26)]
        [FHIRPath("Bundle.entry.resource.where($this is RelatedPerson).where(relationship.coding.code='SPS')", "name")]
        public string SpouseFamilyName
        {
            get
            {
                if (Spouse != null && Spouse.Name != null && Spouse.Name.Count() > 0 && Spouse.Name.First().Family != null) {
                    return Spouse.Name.First().Family;
                }
                return null;
            }
            set
            {
                if (Spouse == null)
                {
                    Spouse = new RelatedPerson();
                    Spouse.Id = Guid.NewGuid().ToString();
                    Spouse.Meta = new Meta();
                    string[] spouse_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Spouse" };
                    Spouse.Meta.Profile = spouse_profile;
                    Spouse.Patient = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Spouse.Relationship.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-RoleCode", "SPS", "spouse", null));
                    HumanName name = new HumanName();
                    name.Family = value;
                    Spouse.Name.Add(name);
                    AddReferenceToComposition(Spouse.Id);
                    Bundle.AddResourceEntry(Spouse, "urn:uuid:" + Spouse.Id);
                }
                else
                {
                    Spouse.Name.First().Family = value;
                }

            }
        }

        /// <summary>Spouse's Suffix.</summary>
        /// <value>the decedent's spouse's suffix</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.SpouseSuffix = "Jr.";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Spouse Suffix: {ExampleDeathRecord.SpouseSuffix}");</para>
        /// </example>
        [Property("Spouse Suffix", Property.Types.String, "Decedent Demographics", "Spouse's Suffix.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Spouse.html", false, 27)]
        [FHIRPath("Bundle.entry.resource.where($this is RelatedPerson).where(relationship.coding.code='SPS')", "name")]
        public string SpouseSuffix
        {
            get
            {
                if (Spouse != null && Spouse.Name != null && Spouse.Name.Count() > 0 && Spouse.Name.First().Suffix != null) {

                    return Spouse.Name.First().Suffix.FirstOrDefault();
                }
                return null;
            }
            set
            {
                if (Spouse == null)
                {
                    Spouse = new RelatedPerson();
                    Spouse.Id = Guid.NewGuid().ToString();
                    Spouse.Meta = new Meta();
                    string[] spouse_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Spouse" };
                    Spouse.Meta.Profile = spouse_profile;
                    Spouse.Patient = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Spouse.Relationship.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-RoleCode", "SPS", "spouse", null));
                    HumanName name = new HumanName();
                    string[] suffix = { value };
                    name.Suffix = suffix;
                    Spouse.Name.Add(name);
                    AddReferenceToComposition(Spouse.Id);
                    Bundle.AddResourceEntry(Spouse, "urn:uuid:" + Spouse.Id);
                }
                else
                {
                    string[] suffix = { value };
                    Spouse.Name.First().Suffix = suffix;
                }

            }
        }

        /// <summary>Decedent's Education Level.</summary>
        /// <value>the decedent's education level. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; elevel = new Dictionary&lt;string, string&gt;();</para>
        /// <para>elevel.Add("code", "BD");</para>
        /// <para>elevel.Add("system", "http://terminology.hl7.org/CodeSystem/v3-EducationLevel");</para>
        /// <para>elevel.Add("display", "College or baccalaureate degree complete");</para>
        /// <para>ExampleDeathRecord.EducationLevel = elevel;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Education Level: {ExampleDeathRecord.EducationLevel['display']}");</para>
        /// </example>
        [Property("Education Level", Property.Types.Dictionary, "Decedent Demographics", "Decedent's Education Level.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Education-Level.html", false, 34)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='80913-7')", "")]
        public Dictionary<string, string> EducationLevel
        {
            get
            {
                if (DecedentEducationLevel != null && DecedentEducationLevel.Value != null && DecedentEducationLevel.Value as CodeableConcept != null)
                {
                    return CodeableConceptToDict((CodeableConcept)DecedentEducationLevel.Value);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (DecedentEducationLevel == null)
                {
                    DecedentEducationLevel = new Observation();
                    DecedentEducationLevel.Id = Guid.NewGuid().ToString();
                    DecedentEducationLevel.Meta = new Meta();
                    string[] educationlevel_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Education-Level" };
                    DecedentEducationLevel.Meta.Profile = educationlevel_profile;
                    DecedentEducationLevel.Status = ObservationStatus.Final;
                    DecedentEducationLevel.Code = new CodeableConcept("http://loinc.org", "80913-7", "Highest level of education [US Standard Certificate of Death]", null);
                    DecedentEducationLevel.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    DecedentEducationLevel.Value = DictToCodeableConcept(value);
                    AddReferenceToComposition(DecedentEducationLevel.Id);
                    Bundle.AddResourceEntry(DecedentEducationLevel, "urn:uuid:" + DecedentEducationLevel.Id);
                }
                else
                {
                    DecedentEducationLevel.Value = DictToCodeableConcept(value);
                }
            }
        }

        /// <summary>Birth Record Identifier.</summary>
        /// <value>a birth record identification string.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.BirthRecordId = "4242123";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Birth Record identification: {ExampleDeathRecord.BirthRecordId}");</para>
        /// </example>
        [Property("Birth Record Id", Property.Types.String, "Decedent Demographics", "Birth Record Identifier (i.e. Certificate Number).", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-BirthRecordIdentifier.html", true, 16)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='BR')", "")]
        public string BirthRecordId
        {
            get
            {
                if (BirthRecordIdentifier != null && BirthRecordIdentifier.Value != null)
                {
                    return Convert.ToString(BirthRecordIdentifier.Value);
                }
                return null;
            }
            set
            {
                if (BirthRecordIdentifier == null)
                {
                    BirthRecordIdentifier = new Observation();
                    BirthRecordIdentifier.Id = Guid.NewGuid().ToString();
                    BirthRecordIdentifier.Meta = new Meta();
                    string[] br_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-BirthRecordIdentifier" };
                    BirthRecordIdentifier.Meta.Profile = br_profile;
                    BirthRecordIdentifier.Status = ObservationStatus.Final;
                    BirthRecordIdentifier.Code = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "BR", "Birth registry number", null);
                    BirthRecordIdentifier.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    BirthRecordIdentifier.Value = new FhirString(value);
                    AddReferenceToComposition(BirthRecordIdentifier.Id);
                    Bundle.AddResourceEntry(BirthRecordIdentifier, "urn:uuid:" + BirthRecordIdentifier.Id);
                }
                else
                {
                    BirthRecordIdentifier.Value = new FhirString(value);
                }
            }
        }

        /// <summary>Birth Record State.</summary>
        /// <value>the state of the decedent's birth certificate. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; brs = new Dictionary&lt;string, string&gt;();</para>
        /// <para>brs.Add("code", "MA");</para>
        /// <para>brs.Add("system", "urn:iso:std:iso:3166:-2");</para>
        /// <para>brs.Add("display", "Massachusetts");</para>
        /// <para>ExampleDeathRecord.BirthRecordState = brs;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Birth Record identification: {ExampleDeathRecord.BirthRecordState}");</para>
        /// </example>
        [Property("Birth Record State", Property.Types.Dictionary, "Decedent Demographics", "Birth Record State.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-BirthRecordIdentifier.html", true, 17)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='BR')", "")]
        public Dictionary<string, string> BirthRecordState
        {
            get
            {
                if (BirthRecordIdentifier != null && BirthRecordIdentifier.Component.Count > 0)
                {
                    // Find correct component
                    var stateComp = BirthRecordIdentifier.Component.FirstOrDefault( entry => ((Observation.ComponentComponent)entry).Code != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault() != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault().Code == "21842-0" );
                    if (stateComp != null && stateComp.Value != null && stateComp.Value as CodeableConcept != null)
                    {
                        return CodeableConceptToDict((CodeableConcept)stateComp.Value);
                    }
                }
                return EmptyCodeDict();
            }
            set
            {
                if (BirthRecordIdentifier == null)
                {
                    BirthRecordIdentifier = new Observation();
                    BirthRecordIdentifier.Id = Guid.NewGuid().ToString();
                    BirthRecordIdentifier.Meta = new Meta();
                    string[] br_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-BirthRecordIdentifier" };
                    BirthRecordIdentifier.Meta.Profile = br_profile;
                    BirthRecordIdentifier.Status = ObservationStatus.Final;
                    BirthRecordIdentifier.Code = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "BR", "Birth registry number", null);
                    BirthRecordIdentifier.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Observation.ComponentComponent component = new Observation.ComponentComponent();
                    component.Code = new CodeableConcept("http://loinc.org", "21842-0", "Birthplace", null);
                    component.Value = DictToCodeableConcept(value);
                    BirthRecordIdentifier.Component.Add(component);
                    AddReferenceToComposition(BirthRecordIdentifier.Id);
                    Bundle.AddResourceEntry(BirthRecordIdentifier, "urn:uuid:" + BirthRecordIdentifier.Id);
                }
                else
                {
                    // Find correct component; if doesn't exist add another
                    var stateComp = BirthRecordIdentifier.Component.FirstOrDefault( entry => ((Observation.ComponentComponent)entry).Code != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault() != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault().Code == "21842-0" );
                    if (stateComp != null)
                    {
                        ((Observation.ComponentComponent)stateComp).Value = DictToCodeableConcept(value);
                    }
                    else
                    {
                        Observation.ComponentComponent component = new Observation.ComponentComponent();
                        component.Code = new CodeableConcept("http://loinc.org", "21842-0", "Birthplace", null);
                        component.Value = DictToCodeableConcept(value);
                        BirthRecordIdentifier.Component.Add(component);
                    }
                }
            }
        }

        /// <summary>Birth Record Year.</summary>
        /// <value>the year found on the decedent's birth certificate.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.BirthRecordYear = "1940";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Birth Record year: {ExampleDeathRecord.BirthRecordYear}");</para>
        /// </example>
        [Property("Birth Record Year", Property.Types.String, "Decedent Demographics", "Birth Record Year.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-BirthRecordIdentifier.html", true, 18)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='BR')", "")]
        public string BirthRecordYear
        {
            get
            {
                if (BirthRecordIdentifier != null && BirthRecordIdentifier.Component.Count > 0)
                {
                    // Find correct component
                    var stateComp = BirthRecordIdentifier.Component.FirstOrDefault( entry => ((Observation.ComponentComponent)entry).Code != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault() != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault().Code == "80904-6" );
                    if (stateComp != null)
                    {
                        return Convert.ToString(stateComp.Value);
                    }
                }
                return null;
            }
            set
            {
                if (BirthRecordIdentifier == null)
                {
                    BirthRecordIdentifier = new Observation();
                    BirthRecordIdentifier.Id = Guid.NewGuid().ToString();
                    BirthRecordIdentifier.Meta = new Meta();
                    string[] br_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-BirthRecordIdentifier" };
                    BirthRecordIdentifier.Meta.Profile = br_profile;
                    BirthRecordIdentifier.Status = ObservationStatus.Final;
                    BirthRecordIdentifier.Code = new CodeableConcept("http://terminology.hl7.org/CodeSystem/v2-0203", "BR", "Birth registry number", null);
                    BirthRecordIdentifier.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Observation.ComponentComponent component = new Observation.ComponentComponent();
                    component.Code = new CodeableConcept("http://loinc.org", "80904-6", "Birth year", null);
                    component.Value = new FhirDateTime(value);
                    BirthRecordIdentifier.Component.Add(component);
                    AddReferenceToComposition(BirthRecordIdentifier.Id);
                    Bundle.AddResourceEntry(BirthRecordIdentifier, "urn:uuid:" + BirthRecordIdentifier.Id);
                }
                else
                {
                    // Find correct component; if doesn't exist add another
                    var stateComp = BirthRecordIdentifier.Component.FirstOrDefault( entry => ((Observation.ComponentComponent)entry).Code != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault() != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault().Code == "80904-6" );
                    if (stateComp != null)
                    {
                        ((Observation.ComponentComponent)stateComp).Value = new FhirDateTime(value);
                    }
                    else
                    {
                        Observation.ComponentComponent component = new Observation.ComponentComponent();
                        component.Code = new CodeableConcept("http://loinc.org", "80904-6", "Birth year", null);
                        component.Value = new FhirDateTime(value);
                        BirthRecordIdentifier.Component.Add(component);
                    }
                }
            }
        }

        /// <summary>Decedent's Usual Occupation (Code).</summary>
        /// <value>the decedent's usual occupation. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; uocc = new Dictionary&lt;string, string&gt;();</para>
        /// <para>uocc.Add("code", "1340");</para>
        /// <para>uocc.Add("system", "urn:oid:2.16.840.1.114222.4.11.7186");</para>
        /// <para>uocc.Add("display", "Biomedical engineers");</para>
        /// <para>ExampleDeathRecord.UsualOccupationCode = uocc;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Usual Occupation: {ExampleDeathRecord.UsualOccupationCode['display']}");</para>
        /// </example>
        [Property("Usual Occupation (Code)", Property.Types.Dictionary, "Decedent Demographics", "Decedent's Usual Occupation.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Usual-Work.html", false, 39)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [PropertyParam("text", "Additional descriptive text.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='21843-8')", "")]
        public Dictionary<string, string> UsualOccupationCode
        {
            get
            {
                if (UsualWork != null && UsualWork.Value != null && UsualWork.Value as CodeableConcept != null)
                {
                    return CodeableConceptToDict((CodeableConcept)UsualWork.Value);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (UsualWork == null)
                {
                    UsualWork = new Observation();
                    UsualWork.Id = Guid.NewGuid().ToString();
                    UsualWork.Meta = new Meta();
                    string[] usualwork_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Usual-Work" };
                    UsualWork.Meta.Profile = usualwork_profile;
                    UsualWork.Status = ObservationStatus.Final;
                    UsualWork.Code = new CodeableConcept("http://loinc.org", "21843-8", "History of Usual occupation", null);
                    UsualWork.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    UsualWork.Effective = new Period();
                    UsualWork.Value = DictToCodeableConcept(value);
                    AddReferenceToComposition(UsualWork.Id);
                    Bundle.AddResourceEntry(UsualWork, "urn:uuid:" + UsualWork.Id);
                }
                else
                {
                    UsualWork.Value = DictToCodeableConcept(value);
                }
            }
        }

        /// <summary>Decedent's Usual Occupation.</summary>
        /// <value>the decedent's usual occupation.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.UsualOccupation = "Biomedical engineering";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Usual Occupation: {ExampleDeathRecord.UsualOccupation}");</para>
        /// </example>
        [Property("Usual Occupation (Text)", Property.Types.String, "Decedent Demographics", "Decedent's Usual Occupation.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Usual-Work.html", true, 40)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='21843-8')", "")]
        public string UsualOccupation
        {
            get
            {
                var usualOccupationCode = UsualOccupationCode;
                if (usualOccupationCode.ContainsKey("text"))
                {
                    return UsualOccupationCode["text"];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                var uocc = new Dictionary<string, string>();
                uocc["text"] = value;
                UsualOccupationCode = uocc;
            }
        }

        /// <summary>Start Date of Usual Occupation.</summary>
        /// <value>the date usual occupation started</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.UsualOccupationStart = "2018-02-19";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Start of Usual Occupation: {ExampleDeathRecord.UsualOccupationStart}");</para>
        /// </example>
        [Property("Usual Occupation Start Date", Property.Types.StringDateTime, "Decedent Demographics", "Decedent's Usual Occupation.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Usual-Work.html", true, 41)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='21843-8')", "")]
        public string UsualOccupationStart
        {
            get
            {
                if (UsualWork?.Effective != null && UsualWork.Effective as Period != null && ((Period)UsualWork.Effective).Start != null)
                {
                    return Convert.ToString(((Period)UsualWork.Effective).Start);
                }
                return null;
            }
            set
            {
                if (UsualWork == null)
                {
                    UsualWork = new Observation();
                    UsualWork.Id = Guid.NewGuid().ToString();
                    UsualWork.Meta = new Meta();
                    string[] usualwork_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Usual-Work" };
                    UsualWork.Meta.Profile = usualwork_profile;
                    UsualWork.Status = ObservationStatus.Final;
                    UsualWork.Code = new CodeableConcept("http://loinc.org", "21843-8", "History of Usual occupation", null);
                    UsualWork.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    UsualWork.Effective = new Period(new FhirDateTime(value), null);
                    AddReferenceToComposition(UsualWork.Id);
                    Bundle.AddResourceEntry(UsualWork, "urn:uuid:" + UsualWork.Id);
                }
                else if (UsualWork.Effective as Period != null)
                {
                    ((Period)UsualWork.Effective).Start = value;
                }
                else
                {
                    UsualWork.Effective = new Period(new FhirDateTime(value), null);
                }
            }
        }

        /// <summary>End Date of Usual Occupation.</summary>
        /// <value>the date usual occupation ended</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.UsualOccupationEnd = "2018-02-19";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"End of Usual Occupation: {ExampleDeathRecord.UsualOccupationEnd}");</para>
        /// </example>
        [Property("Usual Occupation End Date", Property.Types.StringDateTime, "Decedent Demographics", "Decedent's Usual Occupation.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Usual-Work.html", true, 42)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='21843-8')", "")]
        public string UsualOccupationEnd
        {
            get
            {
                if (UsualWork?.Effective != null && UsualWork.Effective as Period != null && ((Period)UsualWork.Effective).End != null)
                {
                    return Convert.ToString(((Period)UsualWork.Effective).End);
                }
                return null;
            }
            set
            {
                if (UsualWork == null)
                {
                    UsualWork = new Observation();
                    UsualWork.Id = Guid.NewGuid().ToString();
                    UsualWork.Meta = new Meta();
                    string[] usualwork_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Usual-Work" };
                    UsualWork.Meta.Profile = usualwork_profile;
                    UsualWork.Status = ObservationStatus.Final;
                    UsualWork.Code = new CodeableConcept("http://loinc.org", "21843-8", "History of Usual occupation", null);
                    UsualWork.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    UsualWork.Effective = new Period(null, new FhirDateTime(value));
                    AddReferenceToComposition(UsualWork.Id);
                    Bundle.AddResourceEntry(UsualWork, "urn:uuid:" + UsualWork.Id);
                }
                else if (UsualWork.Effective as Period != null)
                {
                    ((Period)UsualWork.Effective).End = value;
                }
                else
                {
                    UsualWork.Effective = new Period(null, new FhirDateTime(value));
                }
            }
        }

        /// <summary>Decedent's Usual Industry (Code).</summary>
        /// <value>the decedent's usual industry. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; uind = new Dictionary&lt;string, string&gt;();</para>
        /// <para>uind.Add("code", "7280");</para>
        /// <para>uind.Add("system", "urn:oid:2.16.840.1.114222.4.11.7187");</para>
        /// <para>uind.Add("display", "Accounting, tax preparation, bookkeeping, and payroll services");</para>
        /// <para>ExampleDeathRecord.UsualIndustryCode = uind;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Usual Industry: {ExampleDeathRecord.UsualIndustryCode['display']}");</para>
        /// </example>
        [Property("Usual Industry (Code)", Property.Types.Dictionary, "Decedent Demographics", "Decedent's Usual Industry.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Usual-Work.html", false, 43)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [PropertyParam("text", "Additional descriptive text.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='21843-8')", "")]
        public Dictionary<string, string> UsualIndustryCode
        {
            get
            {
                if (UsualWork != null)
                {
                    Observation.ComponentComponent component = UsualWork.Component.FirstOrDefault( cmp => cmp.Code!= null && cmp.Code.Coding != null && cmp.Code.Coding.Count() > 0 && cmp.Code.Coding.First().Code == "21844-6" );
                    if (component != null && component.Value != null && component.Value as CodeableConcept != null)
                    {
                        return CodeableConceptToDict((CodeableConcept)component.Value);
                    }
                    return EmptyCodeableDict();
                }
                return EmptyCodeableDict();
            }
            set
            {
                if (UsualWork == null)
                {
                    UsualWork = new Observation();
                    UsualWork.Id = Guid.NewGuid().ToString();
                    UsualWork.Meta = new Meta();
                    string[] employmenthistory_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Employment-History" };
                    UsualWork.Meta.Profile = employmenthistory_profile;
                    UsualWork.Status = ObservationStatus.Final;
                    UsualWork.Code = new CodeableConcept("http://loinc.org", "21843-8", "History of Usual occupation", null);
                    UsualWork.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    UsualWork.Effective = new Period();
                    Observation.ComponentComponent component = new Observation.ComponentComponent();
                    component.Code = new CodeableConcept("http://loinc.org", "21844-6", "History of Usual industry", null);
                    component.Value = DictToCodeableConcept(value);
                    UsualWork.Component.Add(component);
                    AddReferenceToComposition(UsualWork.Id);
                    Bundle.AddResourceEntry(UsualWork, "urn:uuid:" + UsualWork.Id);
                }
                else
                {
                    UsualWork.Component.RemoveAll( cmp => cmp.Code!= null && cmp.Code.Coding != null && cmp.Code.Coding.Count() > 0 && cmp.Code.Coding.First().Code == "21844-6" );
                    Observation.ComponentComponent component = new Observation.ComponentComponent();
                    component.Code = new CodeableConcept("http://loinc.org", "21844-6", "History of Usual industry", null);
                    component.Value = DictToCodeableConcept(value);
                    UsualWork.Component.Add(component);
                }
            }
        }

        /// <summary>Decedent's Usual Industry (Text).</summary>
        /// <value>the decedent's usual industry.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.UsualIndustry = "Accounting";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Usual Industry: {ExampleDeathRecord.UsualIndustry}");</para>
        /// </example>
        [Property("Usual Industry (Text)", Property.Types.String, "Decedent Demographics", "Decedent's Usual Industry.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Usual-Work.html", true, 44)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='21843-8')", "")]
        public string UsualIndustry
        {
            get
            {
                var usualIndustryCode = UsualIndustryCode;
                if (usualIndustryCode.ContainsKey("text"))
                {
                    return UsualIndustryCode["text"];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                var uicc = new Dictionary<string, string>();
                uicc["text"] = value;
                UsualIndustryCode = uicc;
            }
        }

        /// <summary>Decedent's Military Service.</summary>
        /// <value>the decedent's military service. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; mserv = new Dictionary&lt;string, string&gt;();</para>
        /// <para>mserv.Add("code", "Y");</para>
        /// <para>mserv.Add("system", "http://terminology.hl7.org/CodeSystem/v2-0136");</para>
        /// <para>mserv.Add("display", "Yes");</para>
        /// <para>ExampleDeathRecord.MilitaryService = uind;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Military Service: {ExampleDeathRecord.MilitaryService['display']}");</para>
        /// </example>
        [Property("Military Service", Property.Types.Dictionary, "Decedent Demographics", "Decedent's Military Service.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Military-Service.html", false, 22)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='55280-2')", "")]
        public Dictionary<string, string> MilitaryService
        {
            get
            {
                if (MilitaryServiceObs != null && MilitaryServiceObs.Value != null && MilitaryServiceObs.Value as CodeableConcept != null)
                {
                    return CodeableConceptToDict((CodeableConcept)MilitaryServiceObs.Value);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (MilitaryServiceObs == null)
                {
                    MilitaryServiceObs = new Observation();
                    MilitaryServiceObs.Id = Guid.NewGuid().ToString();
                    MilitaryServiceObs.Meta = new Meta();
                    string[] militaryhistory_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Military-Service" };
                    MilitaryServiceObs.Meta.Profile = militaryhistory_profile;
                    MilitaryServiceObs.Status = ObservationStatus.Final;
                    MilitaryServiceObs.Code = new CodeableConcept("http://loinc.org", "55280-2", "Military service Narrative", null);
                    MilitaryServiceObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    MilitaryServiceObs.Value = DictToCodeableConcept(value);
                    AddReferenceToComposition(MilitaryServiceObs.Id);
                    Bundle.AddResourceEntry(MilitaryServiceObs, "urn:uuid:" + MilitaryServiceObs.Id);
                }
                else
                {
                    MilitaryServiceObs.Value = DictToCodeableConcept(value);
                }
            }
        }

        /// <summary>Decedent's Military Service. This is a convenience method, to obtain the code use the MilitaryService property instead.</summary>
        /// <value>the decedent's military service. Whether the decedent served in the military, a null value means "unknown".</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.MilitaryServiceBoolean = true;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Military Service: {ExampleDeathRecord.MilitaryServiceBoolean}");</para>
        /// </example>
       [Property("Military Service Boolean", Property.Types.Bool, "Decedent Demographics", "Decedent's Military Service.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Military-Service.html", false, 23)]
       [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='55280-2')", "")]
        public bool? MilitaryServiceBoolean
        {
            get
            {
                var code = this.MilitaryService;
                switch (code["code"])
                {
                    case "Y": // Yes
                        return true;
                    case "N": // No
                        return false;
                    default: // Unknown
                        return null;
                }
            }
            set
            {
                var code = EmptyCodeDict();
                switch(value)
                {
                    case true:
                        code["code"] = "Y";
                        code["display"] = "Yes";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    case false:
                        code["code"] = "N";
                        code["display"] = "No";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    default:
                        code["code"] = "UNK";
                        code["display"] = "unknown";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v3-NullFlavor";
                        break;
                }
                this.MilitaryService = code;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        //
        // Record Properties: Decedent Disposition
        //
        /////////////////////////////////////////////////////////////////////////////////

        /// <summary>Given name(s) of mortician.</summary>
        /// <value>the mortician's name (first, middle, etc.)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>string[] names = { "FD", "Middle" };</para>
        /// <para>ExampleDeathRecord.MorticianGivenNames = names;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Mortician Given Name(s): {string.Join(", ", ExampleDeathRecord.MorticianGivenNames)}");</para>
        /// </example>
        [Property("Mortician Given Names", Property.Types.StringArr, "Decedent Disposition", "Given name(s) of mortician.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Mortician.html", false, 96)]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Mortician')", "name")]
        public string[] MorticianGivenNames
        {
            get
            {
                if (Mortician != null && Mortician.Name.Count() > 0)
                {
                    return Mortician.Name.First().Given.ToArray();
                }
                return new string[0];
            }
            set
            {
                InitializeMorticianIfNull();
                HumanName name = Mortician.Name.SingleOrDefault(n => n.Use == HumanName.NameUse.Official);
                if (name != null)
                {
                    name.Given = value;
                }
                else
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    name.Given = value;
                    Mortician.Name.Add(name);
                }
            }
        }

        /// <summary>Family name of mortician.</summary>
        /// <value>the mortician's family name (i.e. last name)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.MorticianFamilyName = "Last";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Mortician's Last Name: {ExampleDeathRecord.MorticianFamilyName}");</para>
        /// </example>
        [Property("Mortician Family Name", Property.Types.String, "Decedent Disposition", "Family name of mortician.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Mortician.html", false, 97)]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Mortician')", "name")]
        public string MorticianFamilyName
        {
            get
            {
                if (Mortician != null && Mortician.Name.Count() > 0)
                {
                    return Mortician.Name.First().Family;
                }
                return null;
            }
            set
            {
                InitializeMorticianIfNull();
                HumanName name = Mortician.Name.FirstOrDefault();
                if (name != null && !String.IsNullOrEmpty(value))
                {
                    name.Family = value;
                }
                else if (!String.IsNullOrEmpty(value))
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    name.Family = value;
                    Mortician.Name.Add(name);
                }
            }
        }

        /// <summary>Mortician's Suffix.</summary>
        /// <value>the mortician's suffix</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.MorticianSuffix = "Jr.";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Mortician Suffix: {ExampleDeathRecord.MorticianSuffix}");</para>
        /// </example>
        [Property("Mortician Suffix", Property.Types.String, "Decedent Disposition", "Mortician's Suffix.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Mortician.html", false, 98)]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Mortician')", "suffix")]
        public string MorticianSuffix
        {
            get
            {
                if (Mortician != null && Mortician.Name.Count() > 0 && Mortician.Name.First().Suffix.Count() > 0)
                {
                    return Mortician.Name.First().Suffix.First();
                }
                return null;
            }
            set
            {
                InitializeMorticianIfNull();
                HumanName name = Mortician.Name.FirstOrDefault();
                if (name != null && !String.IsNullOrEmpty(value))
                {
                    string[] suffix = { value };
                    name.Suffix = suffix;
                }
                else if (!String.IsNullOrEmpty(value))
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    string[] suffix = { value };
                    name.Suffix = suffix;
                    Mortician.Name.Add(name);
                }
            }
        }

        /// <summary>Mortician Identifier.</summary>
        /// <value>the mortician identification. A Dictionary representing a system (e.g. NPI) and a value, containing the following key/value pairs:
        /// <para>"system" - the identifier system, e.g. US NPI</para>
        /// <para>"value" - the idetifier value, e.g. US NPI number</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; identifier = new Dictionary&lt;string, string&gt;();</para>
        /// <para>identifier.Add("system", "http://hl7.org/fhir/sid/us-npi");</para>
        /// <para>identifier.Add("value", "1234567890");</para>
        /// <para>ExampleDeathRecord.MorticianIdentifier = identifier;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tMortician Identifier: {ExampleDeathRecord.MorticianIdentifier['value']}");</para>
        /// </example>
        [Property("Mortician Identifier", Property.Types.Dictionary, "Decedent Disposition", "Mortician Identifier.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Mortician.html", false, 99)]
        [PropertyParam("system", "The identifier system.")]
        [PropertyParam("value", "The identifier value.")]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Mortician')", "identifier")]
        public Dictionary<string, string> MorticianIdentifier
        {
            get
            {
                Identifier identifier = Mortician?.Identifier?.FirstOrDefault();
                var result = new Dictionary<string, string>();
                if (identifier != null)
                {
                    result["system"] = identifier.System;
                    result["value"] = identifier.Value;
                }
                return result;
            }
            set
            {
                InitializeMorticianIfNull();
                if (Mortician.Identifier.Count > 0)
                {
                    Mortician.Identifier.Clear();
                }
                Identifier identifier = new Identifier();
                identifier.System = value["system"];
                identifier.Value = value["value"];
                Mortician.Identifier.Add(identifier);
            }
        }

        private void InitializeMorticianIfNull()
        {
            if (Mortician == null)
            {
                Mortician = new Practitioner();
                Mortician.Id = Guid.NewGuid().ToString();
                Mortician.Meta = new Meta();
                string[] mortician_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Mortician" };
                Mortician.Meta.Profile = mortician_profile;
            }
        }

        /// <summary>Funeral Home Address.</summary>
        /// <value>the funeral home address. A Dictionary representing an address, containing the following key/value pairs:
        /// <para>"addressLine1" - address, line one</para>
        /// <para>"addressLine2" - address, line two</para>
        /// <para>"addressCity" - address, city</para>
        /// <para>"addressCounty" - address, county</para>
        /// <para>"addressState" - address, state</para>
        /// <para>"addressZip" - address, zip</para>
        /// <para>"addressCountry" - address, country</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; address = new Dictionary&lt;string, string&gt;();</para>
        /// <para>address.Add("addressLine1", "1234 Test Street");</para>
        /// <para>address.Add("addressLine2", "Unit 3");</para>
        /// <para>address.Add("addressCity", "Boston");</para>
        /// <para>address.Add("addressCounty", "Suffolk");</para>
        /// <para>address.Add("addressState", "MA");</para>
        /// <para>address.Add("addressZip", "12345");</para>
        /// <para>address.Add("addressCountry", "United States");</para>
        /// <para>ExampleDeathRecord.FuneralHomeAddress = address;</para>
        /// <para>// Getter:</para>
        /// <para>foreach(var pair in ExampleDeathRecord.FuneralHomeAddress)</para>
        /// <para>{</para>
        /// <para>  Console.WriteLine($"\FuneralHomeAddress key: {pair.Key}: value: {pair.Value}");</para>
        /// <para>};</para>
        /// </example>
        [Property("Funeral Home Address", Property.Types.Dictionary, "Decedent Disposition", "Funeral Home Address.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Funeral-Home.html", false, 93)]
        [PropertyParam("addressLine1", "address, line one")]
        [PropertyParam("addressLine2", "address, line two")]
        [PropertyParam("addressCity", "address, city")]
        [PropertyParam("addressCounty", "address, county")]
        [PropertyParam("addressState", "address, state")]
        [PropertyParam("addressZip", "address, zip")]
        [PropertyParam("addressCountry", "address, country")]
        [FHIRPath("Bundle.entry.resource.where($this is Organization).where(type.coding.code='bus')", "address")]
        public Dictionary<string, string> FuneralHomeAddress
        {
            get
            {
                if (FuneralHome != null)
                {
                    return AddressToDict(FuneralHome.Address.FirstOrDefault());
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (FuneralHome == null)
                {
                    FuneralHome = new Organization();
                    FuneralHome.Id = Guid.NewGuid().ToString();
                    FuneralHome.Meta = new Meta();
                    string[] funeralhome_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Funeral-Home" };
                    FuneralHome.Meta.Profile = funeralhome_profile;
                    FuneralHome.Type.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/organization-type", "bus", "Non-Healthcare Business or Corporation", null));
                    FuneralHome.Active = true;
                    FuneralHome.Address.Add(DictToAddress(value));
                }
                else
                {
                    FuneralHome.Address.Clear();
                    FuneralHome.Address.Add(DictToAddress(value));
                }
            }
        }

        /// <summary>Name of Funeral Home.</summary>
        /// <value>the funeral home name.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.FuneralHomeName = "Smith Funeral Home";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Funeral Home Name: {ExampleDeathRecord.FuneralHomeName}");</para>
        /// </example>
        [Property("Funeral Home Name", Property.Types.String, "Decedent Disposition", "Name of Funeral Home.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Funeral-Home.html", false, 94)]
        [FHIRPath("Bundle.entry.resource.where($this is Organization).where(type.coding.code='bus')", "name")]
        public string FuneralHomeName
        {
            get
            {
                if (FuneralHome != null)
                {
                    return FuneralHome.Name;
                }
                return null;
            }
            set
            {
                if (FuneralHome == null)
                {
                    FuneralHome = new Organization();
                    FuneralHome.Id = Guid.NewGuid().ToString();
                    FuneralHome.Meta = new Meta();
                    string[] funeralhome_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Funeral-Home" };
                    FuneralHome.Meta.Profile = funeralhome_profile;
                    FuneralHome.Type.Add(new CodeableConcept("http://terminology.hl7.org/CodeSystem/organization-type", "bus", "Non-Healthcare Business or Corporation", null));
                    FuneralHome.Active = true;
                    FuneralHome.Name = value;
                }
                else
                {
                    FuneralHome.Name = value;
                }
            }
        }

        /// <summary>Funeral Director Phone.</summary>
        /// <value>the funeral director phone number.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.FuneralDirectorPhone = "000-000-0000";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Funeral Director Phone: {ExampleDeathRecord.FuneralDirectorPhone}");</para>
        /// </example>
        [Property("Funeral Director Phone", Property.Types.String, "Decedent Disposition", "Funeral Director Phone.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Funeral-Service-Licensee.html", false, 95)]
        [FHIRPath("Bundle.entry.resource.where($this is PractitionerRole)", "telecom")]
        public string FuneralDirectorPhone
        {
            get
            {
                string value = null;
                if (FuneralHomeDirector != null)
                {
                    ContactPoint cp = FuneralHomeDirector.Telecom.FirstOrDefault(entry => entry.System == ContactPoint.ContactPointSystem.Phone);
                    if (cp != null)
                    {
                        value = cp.Value;
                    }
                }
                return value;
            }
            set
            {
                if (FuneralHomeDirector == null)
                {
                    FuneralHomeDirector = new PractitionerRole();
                    FuneralHomeDirector.Id = Guid.NewGuid().ToString();
                    FuneralHomeDirector.Meta = new Meta();
                    string[] funeralhomedirector_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Funeral-Service-Licensee" };
                    FuneralHomeDirector.Meta.Profile = funeralhomedirector_profile;
                    AddReferenceToComposition(FuneralHomeDirector.Id);
                    Bundle.AddResourceEntry(FuneralHomeDirector, "urn:uuid:" + FuneralHomeDirector.Id);
                }
                ContactPoint cp = FuneralHomeDirector.Telecom.FirstOrDefault(entry => entry.System == ContactPoint.ContactPointSystem.Phone);
                if (cp != null)
                {
                    cp.Value = value;
                }
                else
                {
                    cp = new ContactPoint();
                    cp.System = ContactPoint.ContactPointSystem.Phone;
                    cp.Value = value;
                    FuneralHomeDirector.Telecom.Add(cp);
                }
            }
        }

        /// <summary>Disposition Location Address.</summary>
        /// <value>the disposition location address. A Dictionary representing an address, containing the following key/value pairs:
        /// <para>"addressLine1" - address, line one</para>
        /// <para>"addressLine2" - address, line two</para>
        /// <para>"addressCity" - address, city</para>
        /// <para>"addressCounty" - address, county</para>
        /// <para>"addressState" - address, state</para>
        /// <para>"addressZip" - address, zip</para>
        /// <para>"addressCountry" - address, country</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; address = new Dictionary&lt;string, string&gt;();</para>
        /// <para>address.Add("addressLine1", "1234 Test Street");</para>
        /// <para>address.Add("addressLine2", "Unit 3");</para>
        /// <para>address.Add("addressCity", "Boston");</para>
        /// <para>address.Add("addressCounty", "Suffolk");</para>
        /// <para>address.Add("addressState", "MA");</para>
        /// <para>address.Add("addressZip", "12345");</para>
        /// <para>address.Add("addressCountry", "United States");</para>
        /// <para>ExampleDeathRecord.DispositionLocationAddress = address;</para>
        /// <para>// Getter:</para>
        /// <para>foreach(var pair in ExampleDeathRecord.DispositionLocationAddress)</para>
        /// <para>{</para>
        /// <para>  Console.WriteLine($"\DispositionLocationAddress key: {pair.Key}: value: {pair.Value}");</para>
        /// <para>};</para>
        /// </example>
        [Property("Disposition Location Address", Property.Types.Dictionary, "Decedent Disposition", "Disposition Location Address.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Disposition-Location.html", true, 91)]
        [PropertyParam("addressLine1", "address, line one")]
        [PropertyParam("addressLine2", "address, line two")]
        [PropertyParam("addressCity", "address, city")]
        [PropertyParam("addressCounty", "address, county")]
        [PropertyParam("addressState", "address, state")]
        [PropertyParam("addressZip", "address, zip")]
        [PropertyParam("addressCountry", "address, country")]
        [FHIRPath("Bundle.entry.resource.where($this is Location).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Disposition-Location')", "address")]
        public Dictionary<string, string> DispositionLocationAddress
        {
            get
            {
                if (DispositionLocation != null)
                {
                    return AddressToDict(DispositionLocation.Address);
                }
                return EmptyAddrDict();
            }
            set
            {
                if (DispositionLocation == null)
                {
                    DispositionLocation = new Location();
                    DispositionLocation.Id = Guid.NewGuid().ToString();
                    DispositionLocation.Meta = new Meta();
                    string[] dispositionlocation_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Disposition-Location" };
                    DispositionLocation.Meta.Profile = dispositionlocation_profile;
                    DispositionLocation.Address = DictToAddress(value);
                    Coding pt = new Coding("http://terminology.hl7.org/CodeSystem/location-physical-type", "si", "Site");
                    DispositionLocation.PhysicalType = new CodeableConcept();
                    DispositionLocation.PhysicalType.Coding.Add(pt);
                    LinkObservationToLocation(DispositionMethod, DispositionLocation);
                }
                else
                {
                    DispositionLocation.Address = DictToAddress(value);
                }
            }
        }

        /// <summary>Name of Disposition Location.</summary>
        /// <value>the displosition location name.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.DispositionLocationName = "Bedford Cemetery";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Disposition Location Name: {ExampleDeathRecord.DispositionLocationName}");</para>
        /// </example>
        [Property("Disposition Location Name", Property.Types.String, "Decedent Disposition", "Name of Disposition Location.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Disposition-Location.html", false, 92)]
        [FHIRPath("Bundle.entry.resource.where($this is Location).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Disposition-Location')", "name")]
        public string DispositionLocationName
        {
            get
            {
                if (DispositionLocation != null)
                {
                    return DispositionLocation.Name;
                }
                return null;
            }
            set
            {
                if (DispositionLocation == null)
                {
                    DispositionLocation = new Location();
                    DispositionLocation.Id = Guid.NewGuid().ToString();
                    DispositionLocation.Meta = new Meta();
                    string[] dispositionlocation_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Disposition-Location" };
                    DispositionLocation.Meta.Profile = dispositionlocation_profile;
                    Coding pt = new Coding("http://terminology.hl7.org/CodeSystem/location-physical-type", "si", "Site");
                    DispositionLocation.PhysicalType = new CodeableConcept();
                    DispositionLocation.PhysicalType.Coding.Add(pt);
                    DispositionLocation.Name = value;
                    LinkObservationToLocation(DispositionMethod, DispositionLocation);
                }
                else
                {
                    DispositionLocation.Name = value;
                }
            }
        }

        /// <summary>Decedent's Disposition Method.</summary>
        /// <value>the decedent's disposition method. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; dmethod = new Dictionary&lt;string, string&gt;();</para>
        /// <para>dmethod.Add("code", "449971000124106");</para>
        /// <para>dmethod.Add("system", "http://snomed.info/sct");</para>
        /// <para>dmethod.Add("display", "Burial");</para>
        /// <para>ExampleDeathRecord.DecedentDispositionMethod = dmethod;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent Disposition Method: {ExampleDeathRecord.DecedentDispositionMethod['display']}");</para>
        /// </example>
        [Property("Decedent Disposition Method", Property.Types.Dictionary, "Decedent Disposition", "Decedent's Disposition Method.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Disposition-Location.html", true, 1)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='80905-3')", "")]
        public Dictionary<string, string> DecedentDispositionMethod
        {
            get
            {
                if (DispositionMethod != null && DispositionMethod.Value != null && DispositionMethod.Value as CodeableConcept != null)
                {
                    return CodeableConceptToDict((CodeableConcept)DispositionMethod.Value);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (DispositionMethod == null)
                {
                    DispositionMethod = new Observation();
                    DispositionMethod.Id = Guid.NewGuid().ToString();
                    DispositionMethod.Meta = new Meta();
                    string[] dispositionmethod_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Disposition-Method" };
                    DispositionMethod.Meta.Profile = dispositionmethod_profile;
                    DispositionMethod.Status = ObservationStatus.Final;
                    DispositionMethod.Code = new CodeableConcept("http://loinc.org", "80905-3", "Body disposition method", null);
                    DispositionMethod.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    DispositionMethod.Performer.Add(new ResourceReference("urn:uuid:" + Mortician.Id));
                    DispositionMethod.Value = DictToCodeableConcept(value);
                    LinkObservationToLocation(DispositionMethod, DispositionLocation);
                    AddReferenceToComposition(DispositionMethod.Id);
                    Bundle.AddResourceEntry(DispositionMethod, "urn:uuid:" + DispositionMethod.Id);
                }
                else
                {
                    DispositionMethod.Value = DictToCodeableConcept(value);
                }
            }
        }

        private void LinkObservationToLocation(Observation observation, Location location)
        {
            if (observation == null || location == null)
            {
                return;
            }

            Extension extension = null;
            foreach (Extension ext in observation.Extension)
            {
                if (ext.Url == "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Observation-Location")
                {
                    extension = ext;
                    break;
                }
            }
            if (extension == null)
            {
                extension = new Extension();
                extension.Url = "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Observation-Location";
                observation.Extension.Add(extension);
            }
            extension.Value = new ResourceReference("urn:uuid:" + location.Id);
        }

        /////////////////////////////////////////////////////////////////////////////////
        //
        // Record Properties: Death Investigation
        //
        /////////////////////////////////////////////////////////////////////////////////

        /// <summary>Autopsy Performed Indicator.</summary>
        /// <value>autopsy performed indicator. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "Y");</para>
        /// <para>code.Add("system", "http://terminology.hl7.org/CodeSystem/v2-0136");</para>
        /// <para>code.Add("display", "Yes");</para>
        /// <para>ExampleDeathRecord.AutopsyPerformedIndicator = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Autopsy Performed Indicator: {ExampleDeathRecord.AutopsyPerformedIndicator['display']}");</para>
        /// </example>
        [Property("Autopsy Performed Indicator", Property.Types.Dictionary, "Death Investigation", "Autopsy Performed Indicator.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Autopsy-Performed-Indicator.html", true, 28)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='85699-7')", "")]
        public Dictionary<string, string> AutopsyPerformedIndicator
        {
            get
            {
                if (AutopsyPerformed != null && AutopsyPerformed.Value != null && AutopsyPerformed.Value as CodeableConcept != null)
                {
                    return CodeableConceptToDict((CodeableConcept)AutopsyPerformed.Value);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (AutopsyPerformed == null)
                {
                    AutopsyPerformed = new Observation();
                    AutopsyPerformed.Id = Guid.NewGuid().ToString();
                    AutopsyPerformed.Meta = new Meta();
                    string[] autopsyperformed_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Autopsy-Performed-Indicator" };
                    AutopsyPerformed.Meta.Profile = autopsyperformed_profile;
                    AutopsyPerformed.Status = ObservationStatus.Final;
                    AutopsyPerformed.Code = new CodeableConcept("http://loinc.org", "85699-7", "Autopsy was performed", null);
                    AutopsyPerformed.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    AutopsyPerformed.Value = DictToCodeableConcept(value);
                    AddReferenceToComposition(AutopsyPerformed.Id);
                    Bundle.AddResourceEntry(AutopsyPerformed, "urn:uuid:" + AutopsyPerformed.Id);
                }
                else
                {
                    AutopsyPerformed.Value = DictToCodeableConcept(value);
                }
            }
        }

        /// <summary>Autopsy Performed Indicator Boolean. This is a convenience method, to access the code use the AutopsyPerformedIndicator property.</summary>
        /// <value>autopsy performed indicator. A null value indicates "not applicable".</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.AutopsyPerformedIndicatorBoolean = true;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Autopsy Performed Indicator: {ExampleDeathRecord.AutopsyPerformedIndicatorBoolean}");</para>
        /// </example>
        [Property("Autopsy Performed Indicator Boolean", Property.Types.Bool, "Death Investigation", "Autopsy Performed Indicator.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Autopsy-Performed-Indicator.html", false, 29)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='85699-7')", "")]
        public bool? AutopsyPerformedIndicatorBoolean
        {
            get
            {
                var code = this.AutopsyPerformedIndicator;
                switch (code["code"])
                {
                    case "Y": // Yes
                        return true;
                    case "N": // No
                        return false;
                    default: // Not applicable
                        return null;
                }
            }
            set
            {
                var code = EmptyCodeDict();
                switch(value)
                {
                    case true:
                        code["code"] = "Y";
                        code["display"] = "Yes";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    case false:
                        code["code"] = "N";
                        code["display"] = "No";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    default:
                        code["code"] = "NA";
                        code["display"] = "not applicable";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v3-NullFlavor";
                        break;
                }
                this.AutopsyPerformedIndicator = code;
            }
        }

        /// <summary>Given name(s) of Pronouncer.</summary>
        /// <value>the Pronouncer's name (first, middle, etc.)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>string[] names = { "FD", "Middle" };</para>
        /// <para>ExampleDeathRecord.PronouncerGivenNames = names;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Pronouncer Given Name(s): {string.Join(", ", ExampleDeathRecord.PronouncerGivenNames)}");</para>
        /// </example>
        [Property("Pronouncer Given Names", Property.Types.StringArr, "Death Investigation", "Given name(s) of Pronouncer.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Pronouncement-Performer.html", false, 21)]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Pronouncement-Performer')", "name")]
        public string[] PronouncerGivenNames
        {
            get
            {
                if (Pronouncer != null && Pronouncer.Name.Count() > 0)
                {
                    return Pronouncer.Name.First().Given.ToArray();
                }
                return new string[0];
            }
            set
            {
                HumanName name = Pronouncer.Name.SingleOrDefault(n => n.Use == HumanName.NameUse.Official);
                if (name != null)
                {
                    name.Given = value;
                }
                else
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    name.Given = value;
                    Pronouncer.Name.Add(name);
                }
            }
        }

        /// <summary>Family name of Pronouncer.</summary>
        /// <value>the Pronouncer's family name (i.e. last name)</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.PronouncerFamilyName = "Last";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Pronouncer's Last Name: {ExampleDeathRecord.PronouncerFamilyName}");</para>
        /// </example>
        [Property("Pronouncer Family Name", Property.Types.String, "Death Investigation", "Family name of Pronouncer.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Pronouncement-Performer.html", false, 22)]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Pronouncement-Performer')", "name")]
        public string PronouncerFamilyName
        {
            get
            {
                if (Pronouncer != null && Pronouncer.Name.Count() > 0)
                {
                    return Pronouncer.Name.First().Family;
                }
                return null;
            }
            set
            {
                HumanName name = Pronouncer.Name.FirstOrDefault();
                if (name != null && !String.IsNullOrEmpty(value))
                {
                    name.Family = value;
                }
                else if (!String.IsNullOrEmpty(value))
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    name.Family = value;
                    Pronouncer.Name.Add(name);
                }
            }
        }

        /// <summary>Pronouncer's Suffix.</summary>
        /// <value>the Pronouncer's suffix</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.PronouncerSuffix = "Jr.";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Pronouncer Suffix: {ExampleDeathRecord.PronouncerSuffix}");</para>
        /// </example>
        [Property("Pronouncer Suffix", Property.Types.String, "Death Investigation", "Pronouncer's Suffix.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Pronouncement-Performer.html", false, 23)]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Pronouncement-Performer')", "suffix")]
        public string PronouncerSuffix
        {
            get
            {
                if (Pronouncer != null && Pronouncer.Name.Count() > 0 && Pronouncer.Name.First().Suffix.Count() > 0)
                {
                    return Pronouncer.Name.First().Suffix.First();
                }
                return null;
            }
            set
            {
                HumanName name = Pronouncer.Name.FirstOrDefault();
                if (name != null && !String.IsNullOrEmpty(value))
                {
                    string[] suffix = { value };
                    name.Suffix = suffix;
                }
                else if (!String.IsNullOrEmpty(value))
                {
                    name = new HumanName();
                    name.Use = HumanName.NameUse.Official;
                    string[] suffix = { value };
                    name.Suffix = suffix;
                    Pronouncer.Name.Add(name);
                }
            }
        }

        /// <summary>Pronouncer Identifier.</summary>
        /// <value>the Pronouncer identification. A Dictionary representing a system (e.g. NPI) and a value, containing the following key/value pairs:
        /// <para>"system" - the identifier system, e.g. US NPI</para>
        /// <para>"value" - the identifier value, e.g. US NPI number</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; identifier = new Dictionary&lt;string, string&gt;();</para>
        /// <para>identifier.Add("system", "http://hl7.org/fhir/sid/us-npi");</para>
        /// <para>identifier.Add("value", "1234567890");</para>
        /// <para>ExampleDeathRecord.PronouncerIdentifier = identifier;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"\tPronouncer Identifier: {ExampleDeathRecord.PronouncerIdentifier['value']}");</para>
        /// </example>
        [Property("Pronouncer Identifier", Property.Types.Dictionary, "Death Investigation", "Pronouncer Identifier.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Pronouncement-Performer.html", false, 24)]
        [PropertyParam("system", "The identifier system.")]
        [PropertyParam("value", "The identifier value.")]
        [FHIRPath("Bundle.entry.resource.where($this is Practitioner).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Pronouncement-Performer')", "identifier")]
        public Dictionary<string, string> PronouncerIdentifier
        {
            get
            {
                Identifier identifier = Pronouncer?.Identifier?.FirstOrDefault();
                var result = new Dictionary<string, string>();
                if (identifier != null)
                {
                    result["system"] = identifier.System;
                    result["value"] = identifier.Value;
                }
                return result;
            }
            set
            {
                if (Pronouncer.Identifier.Count > 0)
                {
                    Pronouncer.Identifier.Clear();
                }
                Identifier identifier = new Identifier();
                identifier.System = value["system"];
                identifier.Value = value["value"];
                Pronouncer.Identifier.Add(identifier);
            }
        }


        /// <summary>Decedent's Date/Time of Death.</summary>
        /// <value>the decedent's date and time of death</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.DateOfDeath = "2018-02-19T16:48:06-05:00";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent Date of Death: {ExampleDeathRecord.DateOfDeath}");</para>
        /// </example>
        [Property("Date/Time Of Death", Property.Types.StringDateTime, "Death Investigation", "Decedent's Date and Time of Death.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Date.html", true, 25)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='81956-5')", "")]
        public string DateOfDeath
        {
            get
            {
                if (DeathDateObs != null)
                {
                    return Convert.ToString(DeathDateObs.Value);
                }
                return null;
            }
            set
            {
                if (DeathDateObs == null)
                {
                    DeathDateObs = new Observation();
                    DeathDateObs.Id = Guid.NewGuid().ToString();
                    DeathDateObs.Meta = new Meta();
                    string[] deathdate_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Date" };
                    DeathDateObs.Meta.Profile = deathdate_profile;
                    DeathDateObs.Status = ObservationStatus.Final;
                    DeathDateObs.Code = new CodeableConcept("http://loinc.org", "81956-5", "Date+time of death", null);
                    DeathDateObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    if (Pronouncer != null)
                    {
                        DeathDateObs.Performer.Add(new ResourceReference("urn:uuid:" + Pronouncer.Id));
                    }
                    DeathDateObs.Value = DeathDateObs.Effective = new FhirDateTime(value);
                    LinkObservationToLocation(DeathDateObs, DeathLocationLoc);
                    AddReferenceToComposition(DeathDateObs.Id);
                    Bundle.AddResourceEntry(DeathDateObs, "urn:uuid:" + DeathDateObs.Id);
                }
                else
                {
                    DeathDateObs.Value = DeathDateObs.Effective = new FhirDateTime(value);
                }
                UpdateBundleIdentifier();
            }
        }

        /// <summary>Decedent's Date/Time of Death Pronouncement.</summary>
        /// <value>the decedent's date and time of death pronouncement</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.DateOfDeathPronouncement = "2018-02-20T16:48:06-05:00";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Decedent Date of Death Pronouncement: {ExampleDeathRecord.DateOfDeathPronouncement}");</para>
        /// </example>
        [Property("Date/Time Of Death Pronouncement", Property.Types.StringDateTime, "Death Investigation", "Decedent's Date/Time of Death Pronouncement.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Date.html", false, 20)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='80616-6')", "")]
        public string DateOfDeathPronouncement
        {
            get
            {
                if (DeathDateObs != null)
                {
                    Observation.ComponentComponent component = DeathDateObs.Component.FirstOrDefault();
                    if (component != null)
                    {
                        return Convert.ToString(component.Value);
                    }
                }
                return null;
            }
            set
            {
                if (DeathDateObs == null)
                {
                    DeathDateObs = new Observation();
                    DeathDateObs.Id = Guid.NewGuid().ToString();
                    DeathDateObs.Meta = new Meta();
                    string[] deathdate_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Date" };
                    DeathDateObs.Meta.Profile = deathdate_profile;
                    DeathDateObs.Status = ObservationStatus.Final;
                    DeathDateObs.Code = new CodeableConcept("http://loinc.org", "81956-5", "Date+time of death", null);
                    DeathDateObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    DeathDateObs.Performer.Add(new ResourceReference("urn:uuid:" + Certifier.Id));
                    Observation.ComponentComponent component = new Observation.ComponentComponent();
                    component.Code = new CodeableConcept("http://loinc.org", "80616-6", "Date and time pronounced dead [US Standard Certificate of Death]", null);
                    component.Value = new FhirDateTime(value);
                    DeathDateObs.Component.Add(component);
                    AddReferenceToComposition(DeathDateObs.Id);
                    Bundle.AddResourceEntry(DeathDateObs, "urn:uuid:" + DeathDateObs.Id);
                }
                else
                {
                    DeathDateObs.Component.Clear();
                    Observation.ComponentComponent component = new Observation.ComponentComponent();
                    component.Code = new CodeableConcept("http://loinc.org", "80616-6", "Date and time pronounced dead [US Standard Certificate of Death]", null);
                    component.Value = new FhirDateTime(value);
                    DeathDateObs.Component.Add(component);
                }
            }
        }

        /// <summary>Autopsy Results Available.</summary>
        /// <value>autopsy results available. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "Y");</para>
        /// <para>code.Add("system", "http://terminology.hl7.org/CodeSystem/v2-0136");</para>
        /// <para>code.Add("display", "Yes");</para>
        /// <para>ExampleDeathRecord.AutopsyResultsAvailable = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Autopsy Results Available: {ExampleDeathRecord.AutopsyResultsAvailable['display']}");</para>
        /// </example>
        [Property("Autopsy Results Available", Property.Types.Dictionary, "Death Investigation", "Autopsy results available, used to complete cause of death.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Autopsy-Performed-Indicator.html", true, 30)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='85699-7')", "")]
        public Dictionary<string, string> AutopsyResultsAvailable
        {
            get
            {
                if (AutopsyPerformed != null && AutopsyPerformed.Component.Count() > 0 && AutopsyPerformed.Component.First().Value as CodeableConcept != null)
                {
                    return CodeableConceptToDict((CodeableConcept)AutopsyPerformed.Component.First().Value);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (AutopsyPerformed == null)
                {
                    AutopsyPerformed = new Observation();
                    AutopsyPerformed.Id = Guid.NewGuid().ToString();
                    AutopsyPerformed.Meta = new Meta();
                    string[] autopsyperformed_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Autopsy-Performed-Indicator" };
                    AutopsyPerformed.Meta.Profile = autopsyperformed_profile;
                    AutopsyPerformed.Status = ObservationStatus.Final;
                    AutopsyPerformed.Code = new CodeableConcept("http://loinc.org", "85699-7", "Autopsy was performed", null);
                    AutopsyPerformed.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Observation.ComponentComponent component = new Observation.ComponentComponent();
                    component.Code = new CodeableConcept("http://loinc.org", "69436-4", "Autopsy results available", null);
                    component.Value = DictToCodeableConcept(value);
                    AutopsyPerformed.Component.Clear();
                    AutopsyPerformed.Component.Add(component);
                    AddReferenceToComposition(AutopsyPerformed.Id);
                    Bundle.AddResourceEntry(AutopsyPerformed, "urn:uuid:" + AutopsyPerformed.Id);
                }
                else
                {
                    Observation.ComponentComponent component = new Observation.ComponentComponent();
                    component.Code = new CodeableConcept("http://loinc.org", "69436-4", "Autopsy results available", null);
                    component.Value = DictToCodeableConcept(value);
                    AutopsyPerformed.Component.Clear();
                    AutopsyPerformed.Component.Add(component);
                }
            }
        }

        /// <summary>Autopsy Results Available Boolean. This is a convenience method, to access the coded value use AutopsyResultsAvailable.</summary>
        /// <value>autopsy results available. A null value indicates "not applicable".</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.AutopsyResultsAvailableBoolean = false;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Autopsy Results Available: {ExampleDeathRecord.AutopsyResultsAvailableBoolean}");</para>
        /// </example>
        [Property("Autopsy Results Available Boolean", Property.Types.Bool, "Death Investigation", "Autopsy results available, used to complete cause of death.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Autopsy-Performed-Indicator.html", false, 31)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='85699-7')", "")]
        public bool? AutopsyResultsAvailableBoolean
        {
            get
            {
                var code = this.AutopsyResultsAvailable;
                switch (code["code"])
                {
                    case "Y": // Yes
                        return true;
                    case "N": // No
                        return false;
                    default: // Not applicable
                        return null;
                }
            }
            set
            {
                var code = EmptyCodeDict();
                switch(value)
                {
                    case true:
                        code["code"] = "Y";
                        code["display"] = "Yes";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    case false:
                        code["code"] = "N";
                        code["display"] = "No";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    default:
                        code["code"] = "NA";
                        code["display"] = "not applicable";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v3-NullFlavor";
                        break;
                }
                this.AutopsyResultsAvailable = code;
            }
        }

        /// <summary>Location of Death.</summary>
        /// <value>location of death. A Dictionary representing an address, containing the following key/value pairs:
        /// <para>"addressLine1" - address, line one</para>
        /// <para>"addressLine2" - address, line two</para>
        /// <para>"addressCity" - address, city</para>
        /// <para>"addressCounty" - address, county</para>
        /// <para>"addressState" - address, state</para>
        /// <para>"addressZip" - address, zip</para>
        /// <para>"addressCountry" - address, country</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; address = new Dictionary&lt;string, string&gt;();</para>
        /// <para>address.Add("addressLine1", "123456789 Test Street");</para>
        /// <para>address.Add("addressLine2", "Unit 3");</para>
        /// <para>address.Add("addressCity", "Boston");</para>
        /// <para>address.Add("addressCounty", "Suffolk");</para>
        /// <para>address.Add("addressState", "MA");</para>
        /// <para>address.Add("addressZip", "12345");</para>
        /// <para>address.Add("addressCountry", "United States");</para>
        /// <para>ExampleDeathRecord.DeathLocationAddress = address;</para>
        /// <para>// Getter:</para>
        /// <para>foreach(var pair in ExampleDeathRecord.DeathLocationAddress)</para>
        /// <para>{</para>
        /// <para>  Console.WriteLine($"\DeathLocationAddress key: {pair.Key}: value: {pair.Value}");</para>
        /// <para>};</para>
        /// </example>
        [Property("Death Location Address", Property.Types.Dictionary, "Death Investigation", "Location of Death.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Location.html", true, 15)]
        [PropertyParam("addressLine1", "address, line one")]
        [PropertyParam("addressLine2", "address, line two")]
        [PropertyParam("addressCity", "address, city")]
        [PropertyParam("addressCounty", "address, county")]
        [PropertyParam("addressState", "address, state")]
        [PropertyParam("addressZip", "address, zip")]
        [PropertyParam("addressCountry", "address, country")]
        [FHIRPath("Bundle.entry.resource.where($this is Location).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Location')", "address")]
        public Dictionary<string, string> DeathLocationAddress
        {
            get
            {
            if (DeathLocationLoc != null)
                {
                    return AddressToDict(DeathLocationLoc.Address);
                }
                return EmptyAddrDict();
            }
            set
            {
                if (DeathLocationLoc == null)
                {
                    DeathLocationLoc = new Location();
                    DeathLocationLoc.Id = Guid.NewGuid().ToString();
                    DeathLocationLoc.Meta = new Meta();
                    string[] deathlocation_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Location" };
                    DeathLocationLoc.Meta.Profile = deathlocation_profile;
                    DeathLocationLoc.Address = DictToAddress(value);
                    LinkObservationToLocation(DeathDateObs, DeathLocationLoc);
                    AddReferenceToComposition(DeathLocationLoc.Id);
                    Bundle.AddResourceEntry(DeathLocationLoc, "urn:uuid:" + DeathLocationLoc.Id);
                }
                else
                {
                    DeathLocationLoc.Address = DictToAddress(value);
                }
                UpdateBundleIdentifier();
            }
        }

        /// <summary>Death Location Jurisdiction.</summary>
        /// <value>the vital record jurisdiction identifier.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.DeathLocationJurisdiction = "MA";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Death Location Jurisdiction: {ExampleDeathRecord.DeathLocationJurisdiction}");</para>
        /// </example>
        [Property("Death Location Jurisdiction", Property.Types.String, "Death Investigation", "Vital Records Jurisdiction of Death Location.", true, locationJurisdictionExtPath, false, 16)]
        [FHIRPath("Bundle.entry.resource.where($this is Location).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Location')", "name")]
        public string DeathLocationJurisdiction
        {
            get
            {
                if (DeathLocationLoc != null)
                {
                    Extension jurisdiction = DeathLocationLoc.Extension.Find(ext => ext.Url == locationJurisdictionExtPath);
                    if (jurisdiction != null && jurisdiction.Value != null &&  jurisdiction.Value.GetType() == typeof(CodeableConcept))
                    {
                        CodeableConcept cc = (CodeableConcept)jurisdiction.Value;
                        return MortalityData.JurisdictionCodeToJurisdictionName(cc.Coding[0].Code);
                    }
                }
                return null;
            }
            set
            {
                if (DeathLocationLoc == null)
                {
                    DeathLocationLoc = new Location();
                    DeathLocationLoc.Id = Guid.NewGuid().ToString();
                    DeathLocationLoc.Meta = new Meta();
                    string[] deathlocation_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Location" };
                    DeathLocationLoc.Meta.Profile = deathlocation_profile;
                    LinkObservationToLocation(DeathDateObs, DeathLocationLoc);
                    AddReferenceToComposition(DeathLocationLoc.Id);
                    Bundle.AddResourceEntry(DeathLocationLoc, "urn:uuid:" + DeathLocationLoc.Id);
                }
                else
                {
                    DeathLocationLoc.Extension.RemoveAll(ext => ext.Url == locationJurisdictionExtPath);
                }
                if (!String.IsNullOrWhiteSpace(value)) // If a jurisdiction is provided, create and add the extension
                {
                    CodeableConcept cc = new CodeableConcept();
                    string code = MortalityData.JurisdictionNameToJurisdictionCode(value);
                    string  system;
                    string  display = value;

                    if (value == "YC")
                    {
                        system = "urn:oid:2.16.840.1.113883.6.245" ;  // YC is the only code U.S. Board on Geographic Names (USGS - GNIS)
                    }
                    else
                    {
                        system =   "urn:oid:2.16.840.1.113883.6.92" ; // All other codes are from FIPS_5-2
                    }
                    cc = new CodeableConcept(system, code, display, display);
                    Extension extension = new Extension();
                    extension.Url = locationJurisdictionExtPath;
                    extension.Value = cc;
                    DeathLocationLoc.Extension.Add(extension);
                }
            }
        }

        /// <summary>Name of Death Location.</summary>
        /// <value>the death location name.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.DeathLocationName = "Example Death Location Name";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Death Location Name: {ExampleDeathRecord.DeathLocationName}");</para>
        /// </example>
        [Property("Death Location Name", Property.Types.String, "Death Investigation", "Name of Death Location.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Location.html", false, 17)]
        [FHIRPath("Bundle.entry.resource.where($this is Location).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Location')", "name")]
        public string DeathLocationName
        {
            get
            {
                if (DeathLocationLoc != null)
                {
                    return DeathLocationLoc.Name;
                }
                return null;
            }
            set
            {
                if (DeathLocationLoc == null)
                {
                    DeathLocationLoc = new Location();
                    DeathLocationLoc.Id = Guid.NewGuid().ToString();
                    DeathLocationLoc.Meta = new Meta();
                    string[] deathlocation_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Location" };
                    DeathLocationLoc.Meta.Profile = deathlocation_profile;
                    DeathLocationLoc.Name = value;
                    LinkObservationToLocation(DeathDateObs, DeathLocationLoc);
                    AddReferenceToComposition(DeathLocationLoc.Id);
                    Bundle.AddResourceEntry(DeathLocationLoc, "urn:uuid:" + DeathLocationLoc.Id);
                }
                else
                {
                    DeathLocationLoc.Name = value;
                }
            }
        }

        /// <summary>Description of Death Location.</summary>
        /// <value>the death location description.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.DeathLocationDescription = "Bedford Cemetery";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Death Location Description: {ExampleDeathRecord.DeathLocationDescription}");</para>
        /// </example>
        [Property("Death Location Description", Property.Types.String, "Death Investigation", "Description of Death Location.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Location.html", false, 18)]
        [FHIRPath("Bundle.entry.resource.where($this is Location).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Location')", "description")]
        public string DeathLocationDescription
        {
            get
            {
                if (DeathLocationLoc != null)
                {
                    return DeathLocationLoc.Description;
                }
                return null;
            }
            set
            {
                if (DeathLocationLoc == null)
                {
                    DeathLocationLoc = new Location();
                    DeathLocationLoc.Id = Guid.NewGuid().ToString();
                    DeathLocationLoc.Meta = new Meta();
                    string[] deathlocation_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Location" };
                    DeathLocationLoc.Meta.Profile = deathlocation_profile;
                    DeathLocationLoc.Description = value;
                    LinkObservationToLocation(DeathDateObs, DeathLocationLoc);
                    AddReferenceToComposition(DeathLocationLoc.Id);
                    Bundle.AddResourceEntry(DeathLocationLoc, "urn:uuid:" + DeathLocationLoc.Id);
                }
                else
                {
                    DeathLocationLoc.Description = value;
                }
            }
        }

        /// <summary>Type of Death Location</summary>
        /// <value>type of death location. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "16983000");</para>
        /// <para>code.Add("system", "http://snomed.info/sct");</para>
        /// <para>code.Add("display", "Death in hospital");</para>
        /// <para>ExampleDeathRecord.DeathLocationType = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Death Location Type: {ExampleDeathRecord.DeathLocationType['display']}");</para>
        /// </example>
        [Property("Death Location Type", Property.Types.Dictionary, "Death Investigation", "Type of Death Location.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Death-Location.html", false, 19)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [PropertyParam("text", "Additional descriptive text.")]
        [FHIRPath("Bundle.entry.resource.where($this is Location).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Location')", "type")]
        public Dictionary<string, string> DeathLocationType
        {
            get
            {
                if (DeathLocationLoc != null && DeathLocationLoc.Type != null && DeathLocationLoc.Type.Count > 0)
                {
                    return CodeableConceptToDict(DeathLocationLoc.Type.First());
                }
                return EmptyCodeableDict();
            }
            set
            {
                if (DeathLocationLoc == null)
                {
                    DeathLocationLoc = new Location();
                    DeathLocationLoc.Id = Guid.NewGuid().ToString();
                    DeathLocationLoc.Meta = new Meta();
                    string[] deathlocation_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Death-Location" };
                    DeathLocationLoc.Meta.Profile = deathlocation_profile;
                    DeathLocationLoc.Type.Add(DictToCodeableConcept(value));
                    LinkObservationToLocation(DeathDateObs, DeathLocationLoc);
                    AddReferenceToComposition(DeathLocationLoc.Id);
                    Bundle.AddResourceEntry(DeathLocationLoc, "urn:uuid:" + DeathLocationLoc.Id);
                }
                else
                {
                    DeathLocationLoc.Type.Clear();
                    DeathLocationLoc.Type.Add(DictToCodeableConcept(value));
                }
            }
        }

        /// <summary>Age At Death.</summary>
        /// <value>decedent's age at time of death. A Dictionary representing a length of time, containing the following key/value pairs:
        /// <para>"value" - the quantity value</para>
        /// <para>"system" - the quantity unit</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; age = new Dictionary&lt;string, string&gt;();</para>
        /// <para>age.Add("value", "100");</para>
        /// <para>age.Add("unit", "a"); // USE: http://hl7.org/fhir/stu3/valueset-age-units.html</para>
        /// <para>ExampleDeathRecord.AgeAtDeath = age;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Age At Death: {ExampleDeathRecord.AgeAtDeath['unit']} years");</para>
        /// </example>
        [Property("Age At Death", Property.Types.Dictionary, "Death Investigation", "Age At Death.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Age.html", true, 2)]
        [PropertyParam("value", "The quantity value.")]
        [PropertyParam("unit", "The quantity unit.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='30525-0')", "")]
        public Dictionary<string, string> AgeAtDeath
        {
            get
            {
                if (AgeAtDeathObs != null && AgeAtDeathObs.Value != null)
                {
                    Dictionary<string, string> age = new Dictionary<string, string>();
                    age.Add("value", Convert.ToString(((Quantity)AgeAtDeathObs.Value).Value));
                    age.Add("unit", ((Quantity)AgeAtDeathObs.Value).Unit);
                    return age;
                }
                return new Dictionary<string, string>() { { "value", "" }, { "unit", "" } };
            }
            set
            {
                if (AgeAtDeathObs == null)
                {
                    AgeAtDeathObs = new Observation();
                    AgeAtDeathObs.Id = Guid.NewGuid().ToString();
                    AgeAtDeathObs.Meta = new Meta();
                    string[] age_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Age" };
                    AgeAtDeathObs.Meta.Profile = age_profile;
                    AgeAtDeathObs.Status = ObservationStatus.Final;
                    AgeAtDeathObs.Code = new CodeableConcept("http://loinc.org", "30525-0", "Age", null);
                    AgeAtDeathObs.Effective = DeathDateObs?.Value;
                    AgeAtDeathObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Quantity quant = new Quantity();
                    if (!String.IsNullOrWhiteSpace(GetValue(value, "value")))
                    {
                        quant.Value = Convert.ToDecimal(GetValue(value, "value"));
                    }
                    if (!String.IsNullOrWhiteSpace(GetValue(value, "unit")))
                    {
                        quant.Unit = GetValue(value, "unit");
                    }
                    AgeAtDeathObs.Value = quant;
                    AddReferenceToComposition(AgeAtDeathObs.Id);
                    Bundle.AddResourceEntry(AgeAtDeathObs, "urn:uuid:" + AgeAtDeathObs.Id);
                }
                else
                {
                    Quantity quant = new Quantity();
                    if (!String.IsNullOrWhiteSpace(GetValue(value, "value")))
                    {
                        quant.Value = Convert.ToDecimal(GetValue(value, "value"));
                    }
                    if (!String.IsNullOrWhiteSpace(GetValue(value, "unit")))
                    {
                        quant.Unit = GetValue(value, "unit");
                    }
                    AgeAtDeathObs.Effective = DeathDateObs?.Value;
                    AgeAtDeathObs.Value = quant;
                }
            }
        }

        /// <summary>Pregnancy Status At Death.</summary>
        /// <value>pregnancy status at death. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "PHC1260");</para>
        /// <para>code.Add("system", "urn:oid:2.16.840.1.114222.4.5.274");</para>
        /// <para>code.Add("display", "Not pregnant within past year");</para>
        /// <para>ExampleDeathRecord.PregnancyStatus = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Pregnancy Status: {ExampleDeathRecord.PregnancyStatus['display']}");</para>
        /// </example>
        [Property("Pregnancy Status", Property.Types.Dictionary, "Death Investigation", "Pregnancy Status At Death.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Pregnancy.html", true, 33)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='69442-2')", "")]
        public Dictionary<string, string> PregnancyStatus
        {
            get
            {
                if (PregnancyObs != null && PregnancyObs.Value != null && PregnancyObs.Value as CodeableConcept != null)
                {
                    return CodeableConceptToDict((CodeableConcept)PregnancyObs.Value);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (PregnancyObs == null)
                {
                    PregnancyObs = new Observation();
                    PregnancyObs.Id = Guid.NewGuid().ToString();
                    PregnancyObs.Meta = new Meta();
                    string[] p_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Pregnancy" };
                    PregnancyObs.Meta.Profile = p_profile;
                    PregnancyObs.Status = ObservationStatus.Final;
                    PregnancyObs.Code = new CodeableConcept("http://loinc.org", "69442-2", "Timing of recent pregnancy in relation to death", null);
                    PregnancyObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    PregnancyObs.Value = DictToCodeableConcept(value);
                    AddReferenceToComposition(PregnancyObs.Id);
                    Bundle.AddResourceEntry(PregnancyObs, "urn:uuid:" + PregnancyObs.Id);
                }
                else
                {
                    PregnancyObs.Value = DictToCodeableConcept(value);
                }
            }
        }

        /// <summary>Examiner Contacted.</summary>
        /// <value>if a medical examiner was contacted.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; ec = new Dictionary&lt;string, string&gt;();</para>
        /// <para>within.Add("code", "Y");</para>
        /// <para>within.Add("system", "http://terminology.hl7.org/CodeSystem/v2-0136");</para>
        /// <para>within.Add("display", "Yes");</para>
        /// <para>ExampleDeathRecord.ExaminerContacted = ec;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Examiner Contacted: {ExampleDeathRecord.ExaminerContacted['display']}");</para>
        /// </example>
        [Property("Examiner Contacted", Property.Types.Dictionary, "Death Investigation", "Examiner Contacted.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Examiner-Contacted.html", true, 26)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='74497-9')", "")]
        public Dictionary<string, string> ExaminerContacted
        {
            get
            {
                if (ExaminerContactedObs != null && ExaminerContactedObs.Value != null && ExaminerContactedObs.Value as CodeableConcept != null)
                {
                    CodeableConcept cc = (CodeableConcept)ExaminerContactedObs.Value;
                    return CodeableConceptToDict(cc);
                }
                return EmptyCodeDict();
            }
            set
            {
                var contactedCoding = DictToCodeableConcept(value);
                if (ExaminerContactedObs == null)
                {
                    ExaminerContactedObs = new Observation();
                    ExaminerContactedObs.Id = Guid.NewGuid().ToString();
                    ExaminerContactedObs.Meta = new Meta();
                    string[] ec_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Examiner-Contacted" };
                    ExaminerContactedObs.Meta.Profile = ec_profile;
                    ExaminerContactedObs.Status = ObservationStatus.Final;
                    ExaminerContactedObs.Code = new CodeableConcept("http://loinc.org", "74497-9", "Medical examiner or coroner was contacted [US Standard Certificate of Death]", null);
                    ExaminerContactedObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    ExaminerContactedObs.Value = contactedCoding;
                    AddReferenceToComposition(ExaminerContactedObs.Id);
                    Bundle.AddResourceEntry(ExaminerContactedObs, "urn:uuid:" + ExaminerContactedObs.Id);
                }
                else
                {
                    ExaminerContactedObs.Value = contactedCoding;
                }
            }
        }

        /// <summary>Examiner Contacted Boolean. This is a convenience method, to access the code use ExaminerContacted instead.</summary>
        /// <value>if a medical examiner was contacted. A null value indicates "unknown".</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.ExaminerContacted = false;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Examiner Contacted: {ExampleDeathRecord.ExaminerContacted}");</para>
        /// </example>
        [Property("Examiner Contacted Boolean", Property.Types.Bool, "Death Investigation", "Examiner Contacted.", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Examiner-Contacted.html", false, 27)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='74497-9')", "")]
        public bool? ExaminerContactedBoolean
        {
            get
            {
                var code = this.ExaminerContacted;
                switch (code["code"])
                {
                    case "Y": // Yes
                        return true;
                    case "N": // No
                        return false;
                    default: // Unknown
                        return null;
                }
            }
            set
            {
                var code = EmptyCodeDict();
                switch(value)
                {
                    case true:
                        code["code"] = "Y";
                        code["display"] = "Yes";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    case false:
                        code["code"] = "N";
                        code["display"] = "No";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    default:
                        code["code"] = "UNK";
                        code["display"] = "unknown";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v3-NullFlavor";
                        break;
                }
                this.ExaminerContacted = code;
            }
        }

        /// <summary>Location of Injury.</summary>
        /// <value>location of injury. A Dictionary representing an address, containing the following key/value pairs:
        /// <para>"addressLine1" - address, line one</para>
        /// <para>"addressLine2" - address, line two</para>
        /// <para>"addressCity" - address, city</para>
        /// <para>"addressCounty" - address, county</para>
        /// <para>"addressState" - address, state</para>
        /// <para>"addressZip" - address, zip</para>
        /// <para>"addressCountry" - address, country</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; address = new Dictionary&lt;string, string&gt;();</para>
        /// <para>address.Add("addressLine1", "123456 Test Street");</para>
        /// <para>address.Add("addressLine2", "Unit 3");</para>
        /// <para>address.Add("addressCity", "Boston");</para>
        /// <para>address.Add("addressCounty", "Suffolk");</para>
        /// <para>address.Add("addressState", "MA");</para>
        /// <para>address.Add("addressZip", "12345");</para>
        /// <para>address.Add("addressCountry", "United States");</para>
        /// <para>ExampleDeathRecord.InjuryLocationAddress = address;</para>
        /// <para>// Getter:</para>
        /// <para>foreach(var pair in ExampleDeathRecord.InjuryLocationAddress)</para>
        /// <para>{</para>
        /// <para>  Console.WriteLine($"\InjuryLocationAddress key: {pair.Key}: value: {pair.Value}");</para>
        /// <para>};</para>
        /// </example>
        [Property("Injury Location Address", Property.Types.Dictionary, "Death Investigation", "Location of Injury.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Injury-Location.html", true, 34)]
        [PropertyParam("addressLine1", "address, line one")]
        [PropertyParam("addressLine2", "address, line two")]
        [PropertyParam("addressCity", "address, city")]
        [PropertyParam("addressCounty", "address, county")]
        [PropertyParam("addressState", "address, state")]
        [PropertyParam("addressZip", "address, zip")]
        [PropertyParam("addressCountry", "address, country")]
        [FHIRPath("Bundle.entry.resource.where($this is Location).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Injury-Location')", "address")]
        public Dictionary<string, string> InjuryLocationAddress
        {
            get
            {
                if (InjuryLocationLoc != null)
                {
                    return AddressToDict(InjuryLocationLoc.Address);
                }
                return EmptyAddrDict();
            }
            set
            {
                if (InjuryLocationLoc == null)
                {
                    InjuryLocationLoc = new Location();
                    InjuryLocationLoc.Id = Guid.NewGuid().ToString();
                    InjuryLocationLoc.Meta = new Meta();
                    string[] injurylocation_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Injury-Location" };
                    InjuryLocationLoc.Meta.Profile = injurylocation_profile;
                    InjuryLocationLoc.Address = DictToAddress(value);
                    LinkObservationToLocation(InjuryIncidentObs, InjuryLocationLoc);
                    AddReferenceToComposition(InjuryLocationLoc.Id);
                    Bundle.AddResourceEntry(InjuryLocationLoc, "urn:uuid:" + InjuryLocationLoc.Id);
                }
                else
                {
                    InjuryLocationLoc.Address = DictToAddress(value);
                }
            }
        }

        /// <summary>Name of Injury Location.</summary>
        /// <value>the injury location name.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.InjuryLocationName = "Bedford Cemetery";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Injury Location Name: {ExampleDeathRecord.InjuryLocationName}");</para>
        /// </example>
        [Property("Injury Location Name", Property.Types.String, "Death Investigation", "Name of Injury Location.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Injury-Location.html", true, 35)]
        [FHIRPath("Bundle.entry.resource.where($this is Location).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Injury-Location')", "name")]
        public string InjuryLocationName
        {
            get
            {
                if (InjuryLocationLoc != null)
                {
                    return InjuryLocationLoc.Name;
                }
                return null;
            }
            set
            {
                if (InjuryLocationLoc == null)
                {
                    InjuryLocationLoc = new Location();
                    InjuryLocationLoc.Id = Guid.NewGuid().ToString();
                    InjuryLocationLoc.Meta = new Meta();
                    string[] injurylocation_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Injury-Location" };
                    InjuryLocationLoc.Meta.Profile = injurylocation_profile;
                    InjuryLocationLoc.Name = value;
                    LinkObservationToLocation(InjuryIncidentObs, InjuryLocationLoc);
                    AddReferenceToComposition(InjuryLocationLoc.Id);
                    Bundle.AddResourceEntry(InjuryLocationLoc, "urn:uuid:" + InjuryLocationLoc.Id);
                }
                else
                {
                    InjuryLocationLoc.Name = value;
                }
            }
        }

        /// <summary>Description of Injury Location.</summary>
        /// <value>the injury location description.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.InjuryLocationDescription = "Bedford Cemetery";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Injury Location Description: {ExampleDeathRecord.InjuryLocationDescription}");</para>
        /// </example>
        [Property("Injury Location Description", Property.Types.String, "Death Investigation", "Description of Injury Location.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Injury-Location.html", true, 36)]
        [FHIRPath("Bundle.entry.resource.where($this is Location).where(meta.profile='http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Injury-Location')", "description")]
        public string InjuryLocationDescription
        {
            get
            {
                if (InjuryLocationLoc != null)
                {
                    return InjuryLocationLoc.Description;
                }
                return null;
            }
            set
            {
                if (InjuryLocationLoc == null)
                {
                    InjuryLocationLoc = new Location();
                    InjuryLocationLoc.Id = Guid.NewGuid().ToString();
                    InjuryLocationLoc.Meta = new Meta();
                    string[] injurylocation_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Injury-Location" };
                    InjuryLocationLoc.Meta.Profile = injurylocation_profile;
                    InjuryLocationLoc.Description = value;
                    LinkObservationToLocation(InjuryIncidentObs, InjuryLocationLoc);
                    AddReferenceToComposition(InjuryLocationLoc.Id);
                    Bundle.AddResourceEntry(InjuryLocationLoc, "urn:uuid:" + InjuryLocationLoc.Id);
                }
                else
                {
                    InjuryLocationLoc.Description = value;
                }
            }
        }

        /// <summary>Date/Time of Injury.</summary>
        /// <value>the date and time of injury</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.InjuryDate = "2018-02-19T16:48:06-05:00";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Date of Injury: {ExampleDeathRecord.InjuryDate}");</para>
        /// </example>
        [Property("Injury Date/Time", Property.Types.StringDateTime, "Death Investigation", "Date/Time of Injury.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-InjuryIncident.html", true, 37)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='11374-6')", "")]
        public string InjuryDate
        {
            get
            {
                if (InjuryIncidentObs?.Effective != null)
                {
                    return Convert.ToString(InjuryIncidentObs.Effective);
                }
                return null;
            }
            set
            {
                if (InjuryIncidentObs == null)
                {
                    InjuryIncidentObs = new Observation();
                    InjuryIncidentObs.Id = Guid.NewGuid().ToString();
                    InjuryIncidentObs.Meta = new Meta();
                    string[] iio_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-InjuryIncident" };
                    InjuryIncidentObs.Meta.Profile = iio_profile;
                    InjuryIncidentObs.Status = ObservationStatus.Final;
                    InjuryIncidentObs.Code = new CodeableConcept("http://loinc.org", "11374-6", "Injury incident description Narrative", null);
                    InjuryIncidentObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    InjuryIncidentObs.Effective = new FhirDateTime(value);
                    LinkObservationToLocation(InjuryIncidentObs, InjuryLocationLoc);
                    AddReferenceToComposition(InjuryIncidentObs.Id);
                    Bundle.AddResourceEntry(InjuryIncidentObs, "urn:uuid:" + InjuryIncidentObs.Id);
                }
                else
                {
                    InjuryIncidentObs.Effective = new FhirDateTime(value);
                }
            }
        }

        /// <summary>Description of Injury.</summary>
        /// <value>the description of the injury</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.InjuryDescription = "drug toxicity";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Injury Description: {ExampleDeathRecord.InjuryDescription}");</para>
        /// </example>
        [Property("Injury Description", Property.Types.String, "Death Investigation", "Description of Injury.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-InjuryIncident.html", true, 38)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='11374-6')", "")]
        public string InjuryDescription
        {
            get
            {
                if (InjuryIncidentObs?.Value != null)
                {
                    return Convert.ToString(InjuryIncidentObs.Value);
                }
                return null;
            }
            set
            {
                if (InjuryIncidentObs == null)
                {
                    InjuryIncidentObs = new Observation();
                    InjuryIncidentObs.Id = Guid.NewGuid().ToString();
                    InjuryIncidentObs.Meta = new Meta();
                    string[] iio_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-InjuryIncident" };
                    InjuryIncidentObs.Meta.Profile = iio_profile;
                    InjuryIncidentObs.Status = ObservationStatus.Final;
                    InjuryIncidentObs.Code = new CodeableConcept("http://loinc.org", "11374-6", "Injury incident description Narrative", null);
                    InjuryIncidentObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    InjuryIncidentObs.Value = new FhirString(value);
                    LinkObservationToLocation(InjuryIncidentObs, InjuryLocationLoc);
                    AddReferenceToComposition(InjuryIncidentObs.Id);
                    Bundle.AddResourceEntry(InjuryIncidentObs, "urn:uuid:" + InjuryIncidentObs.Id);
                }
                else
                {
                    InjuryIncidentObs.Value = new FhirString(value);
                }
            }
        }

        /// <summary>Place of Injury.</summary>
        /// <value>the place of injury. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "0");</para>
        /// <para>code.Add("system", "urn:oid:2.16.840.1.114222.4.5.320");</para>
        /// <para>code.Add("display", "Home");</para>
        /// <para>ExampleDeathRecord.InjuryPlace = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Place of Injury: {ExampleDeathRecord.InjuryPlace['display']}");</para>
        /// </example>
        [Property("Injury Place", Property.Types.Dictionary, "Death Investigation", "Place of Injury.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-InjuryIncident.html", true, 39)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [PropertyParam("text", "Additional descriptive text.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='11374-6')", "")]
        public Dictionary<string, string> InjuryPlace
        {
            get
            {
                if (InjuryIncidentObs != null && InjuryIncidentObs.Component.Count > 0)
                {
                    // Find correct component
                    var placeComp = InjuryIncidentObs.Component.FirstOrDefault( entry => ((Observation.ComponentComponent)entry).Code != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault() != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault().Code == "69450-5" );
                    if (placeComp?.Value != null && placeComp.Value as CodeableConcept != null)
                    {
                        return CodeableConceptToDict((CodeableConcept)placeComp.Value);
                    }
                }
                return EmptyCodeDict();
            }
            set
            {
                if (InjuryIncidentObs == null)
                {
                    InjuryIncidentObs = new Observation();
                    InjuryIncidentObs.Id = Guid.NewGuid().ToString();
                    InjuryIncidentObs.Meta = new Meta();
                    string[] iio_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-InjuryIncident" };
                    InjuryIncidentObs.Meta.Profile = iio_profile;
                    InjuryIncidentObs.Status = ObservationStatus.Final;
                    InjuryIncidentObs.Code = new CodeableConcept("http://loinc.org", "11374-6", "Injury incident description Narrative", null);
                    InjuryIncidentObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Observation.ComponentComponent component = new Observation.ComponentComponent();
                    component.Code = new CodeableConcept("http://loinc.org", "69450-5", "Place of injury Facility", null);
                    component.Value = DictToCodeableConcept(value);
                    InjuryIncidentObs.Component.Add(component);
                    LinkObservationToLocation(InjuryIncidentObs, InjuryLocationLoc);
                    AddReferenceToComposition(InjuryIncidentObs.Id);
                    Bundle.AddResourceEntry(InjuryIncidentObs, "urn:uuid:" + InjuryIncidentObs.Id);
                }
                else
                {
                    // Find correct component; if doesn't exist add another
                    var placeComp = InjuryIncidentObs.Component.FirstOrDefault( entry => ((Observation.ComponentComponent)entry).Code != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault() != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault().Code == "69450-5" );
                    if (placeComp != null)
                    {
                        ((Observation.ComponentComponent)placeComp).Value = DictToCodeableConcept(value);
                    }
                    else
                    {
                        Observation.ComponentComponent component = new Observation.ComponentComponent();
                        component.Code = new CodeableConcept("http://loinc.org", "69450-5", "Place of injury Facility", null);
                        component.Value = DictToCodeableConcept(value);
                        InjuryIncidentObs.Component.Add(component);
                    }
                }
            }
        }

        /// <summary>Place of Injury Description.</summary>
        /// <value>the place of injury.</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.InjuryPlaceDescription = "At home, in the kitchen";</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Place of Injury Description: {ExampleDeathRecord.InjuryPlaceDescription}");</para>
        /// </example>
        [Property("Injury Place Description", Property.Types.String, "Death Investigation", "Place of Injury.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-InjuryIncident.html", true, 40)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='11374-6')", "")]
        public string InjuryPlaceDescription
        {
            get
            {
                var injuryPlace = this.InjuryPlace;
                if (injuryPlace.ContainsKey("text") && injuryPlace["text"] != null)
                {
                    return injuryPlace["text"];
                }
                else
                {
                    return "";
                }
            }
            set
            {
                var injuryPlace = this.InjuryPlace;
                injuryPlace["text"] = value;
                this.InjuryPlace = injuryPlace;
            }
        }

        /// <summary>Injury At Work?</summary>
        /// <value>did the injury occur at work? A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "N");</para>
        /// <para>code.Add("system", "http://terminology.hl7.org/CodeSystem/v2-0136");</para>
        /// <para>code.Add("display", "No");</para>
        /// <para>ExampleDeathRecord.InjuryAtWork = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Injury At Work?: {ExampleDeathRecord.InjuryAtWork['display']}");</para>
        /// </example>
        [Property("Injury At Work?", Property.Types.Dictionary, "Death Investigation", "Did the injury occur at work?", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-InjuryIncident.html", true, 41)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='11374-6')", "")]
        public Dictionary<string, string> InjuryAtWork
        {
            get
            {
                if (InjuryIncidentObs != null && InjuryIncidentObs.Component.Count > 0)
                {
                    // Find correct component
                    var placeComp = InjuryIncidentObs.Component.FirstOrDefault( entry => ((Observation.ComponentComponent)entry).Code != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault() != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault().Code == "69444-8" );
                    if (placeComp != null && placeComp.Value != null && placeComp.Value as CodeableConcept != null)
                    {
                        return CodeableConceptToDict((CodeableConcept)placeComp.Value);
                    }
                }
                return EmptyCodeDict();
            }
            set
            {
                if (InjuryIncidentObs == null)
                {
                    InjuryIncidentObs = new Observation();
                    InjuryIncidentObs.Id = Guid.NewGuid().ToString();
                    InjuryIncidentObs.Meta = new Meta();
                    string[] iio_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-InjuryIncident" };
                    InjuryIncidentObs.Meta.Profile = iio_profile;
                    InjuryIncidentObs.Status = ObservationStatus.Final;
                    InjuryIncidentObs.Code = new CodeableConcept("http://loinc.org", "11374-6", "Injury incident description Narrative", null);
                    InjuryIncidentObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Observation.ComponentComponent component = new Observation.ComponentComponent();
                    component.Code = new CodeableConcept("http://loinc.org", "69444-8", "Did death result from injury at work", null);
                    component.Value = DictToCodeableConcept(value);
                    InjuryIncidentObs.Component.Add(component);
                    LinkObservationToLocation(InjuryIncidentObs, InjuryLocationLoc);
                    AddReferenceToComposition(InjuryIncidentObs.Id);
                    Bundle.AddResourceEntry(InjuryIncidentObs, "urn:uuid:" + InjuryIncidentObs.Id);
                }
                else
                {
                    // Find correct component; if doesn't exist add another
                    var placeComp = InjuryIncidentObs.Component.FirstOrDefault( entry => ((Observation.ComponentComponent)entry).Code != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault() != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault().Code == "69444-8" );
                    if (placeComp != null)
                    {
                        ((Observation.ComponentComponent)placeComp).Value = DictToCodeableConcept(value);
                    }
                    else
                    {
                        Observation.ComponentComponent component = new Observation.ComponentComponent();
                        component.Code = new CodeableConcept("http://loinc.org", "69444-8", "Did death result from injury at work", null);
                        component.Value = DictToCodeableConcept(value);
                        InjuryIncidentObs.Component.Add(component);
                    }
                }
            }
        }

        /// <summary>Injury At Work? This is a convenience method, to access the code use the InjuryAtWork property instead.</summary>
        /// <value>did the injury occur at work? A null value indicates "not applicable".</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.InjuryAtWorkBoolean = true;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Injury At Work?: {ExampleDeathRecord.InjuryAtWorkBoolean}");</para>
        /// </example>
        [Property("Injury At Work?", Property.Types.Bool, "Death Investigation", "Did the injury occur at work?", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-InjuryIncident.html", false, 42)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='11374-6')", "")]
        public bool? InjuryAtWorkBoolean
        {
            get
            {
                var code = this.InjuryAtWork;
                switch (code["code"])
                {
                    case "Y": // Yes
                        return true;
                    case "N": // No
                        return false;
                    default: // Not applicable
                        return null;
                }
            }
            set
            {
                var code = EmptyCodeDict();
                switch(value)
                {
                    case true:
                        code["code"] = "Y";
                        code["display"] = "Yes";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    case false:
                        code["code"] = "N";
                        code["display"] = "No";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    default:
                        code["code"] = "NA";
                        code["display"] = "not applicable";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v3-NullFlavor";
                        break;
                }
                this.InjuryAtWork = code;
            }
        }

        /// <summary>Transportation Event?</summary>
        /// <value>was the injury associated with a transportation event? A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "N");</para>
        /// <para>code.Add("system", "http://terminology.hl7.org/CodeSystem/v2-0136");</para>
        /// <para>code.Add("display", "No");</para>
        /// <para>ExampleDeathRecord.TransportationEvent = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Transportation Event?: {ExampleDeathRecord.TransportationEvent['display']}");</para>
        /// </example>
        [Property("Transportation Event?", Property.Types.Dictionary, "Death Investigation", "Was the injury associated with a transportation event?", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-InjuryIncident.html", true, 43)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [PropertyParam("text", "Additional descriptive text.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='11374-6')", "")]
        public Dictionary<string, string> TransportationEvent
        {
            get
            {
                if (InjuryIncidentObs != null && InjuryIncidentObs.Component.Count > 0)
                {
                    // Find correct component
                    var transportComp = InjuryIncidentObs.Component.FirstOrDefault( entry => ((Observation.ComponentComponent)entry).Code != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault() != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault().Code == "69448-9" );
                    if (transportComp != null && transportComp.Value != null && transportComp.Value as CodeableConcept != null)
                    {
                        return CodeableConceptToDict((CodeableConcept)transportComp.Value);
                    }
                }
                return EmptyCodeableDict();
            }
            set
            {
                if (InjuryIncidentObs == null)
                {
                    InjuryIncidentObs = new Observation();
                    InjuryIncidentObs.Id = Guid.NewGuid().ToString();
                    InjuryIncidentObs.Meta = new Meta();
                    string[] iio_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-InjuryIncident" };
                    InjuryIncidentObs.Meta.Profile = iio_profile;
                    InjuryIncidentObs.Status = ObservationStatus.Final;
                    InjuryIncidentObs.Code = new CodeableConcept("http://loinc.org", "11374-6", "Injury incident description Narrative", null);
                    InjuryIncidentObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    Observation.ComponentComponent component = new Observation.ComponentComponent();
                    component.Code = new CodeableConcept("http://loinc.org", "69448-9", "Injury leading to death associated with transportation event", null);
                    component.Value = DictToCodeableConcept(value);
                    InjuryIncidentObs.Component.Add(component);
                    LinkObservationToLocation(InjuryIncidentObs, InjuryLocationLoc);
                    AddReferenceToComposition(InjuryIncidentObs.Id);
                    Bundle.AddResourceEntry(InjuryIncidentObs, "urn:uuid:" + InjuryIncidentObs.Id);
                }
                else
                {
                    // Find correct component; if doesn't exist add another
                    var transportComp = InjuryIncidentObs.Component.FirstOrDefault( entry => ((Observation.ComponentComponent)entry).Code != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault() != null && ((Observation.ComponentComponent)entry).Code.Coding.FirstOrDefault().Code == "69448-9" );
                    if (transportComp != null)
                    {
                        ((Observation.ComponentComponent)transportComp).Value = DictToCodeableConcept(value);
                    }
                    else
                    {
                        Observation.ComponentComponent component = new Observation.ComponentComponent();
                        component.Code = new CodeableConcept("http://loinc.org", "69448-9", "Injury leading to death associated with transportation event", null);
                        component.Value = DictToCodeableConcept(value);
                        InjuryIncidentObs.Component.Add(component);
                    }
                }
            }
        }

        /// <summary>Transportation Event Boolean? This is a convenience method, to access the code use the Transportation Event property instead.</summary>
        /// <value>was the injury associated with a transportation event? A null value indicates "unknown"</value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>ExampleDeathRecord.TransportationEventBoolean = true;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Transportation Event?: {ExampleDeathRecord.TransportationEventBoolean}");</para>
        /// </example>
        [Property("Transportation Event Boolean?", Property.Types.Bool, "Death Investigation", "Was the injury associated with a transportation event?", false, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-InjuryIncident.html", false, 44)]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='11374-6')", "")]
        public bool? TransportationEventBoolean
        {
            get
            {
                var code = this.TransportationEvent;
                switch (code["code"])
                {
                    case "Y": // Yes
                        return true;
                    case "N": // No
                        return false;
                    default: // Unknown
                        return null;
                }
            }
            set
            {
                var code = EmptyCodeDict();
                switch(value)
                {
                    case true:
                        code["code"] = "Y";
                        code["display"] = "Yes";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    case false:
                        code["code"] = "N";
                        code["display"] = "No";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v2-0136";
                        break;
                    default:
                        code["code"] = "UNK";
                        code["display"] = "unknown";
                        code["system"] = "http://terminology.hl7.org/CodeSystem/v3-NullFlavor";
                        break;
                }
                this.TransportationEvent = code;
            }
        }

        /// <summary>Transportation Role in death.</summary>
        /// <value>transportation role in death. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "257500003");</para>
        /// <para>code.Add("system", "http://snomed.info/sct");</para>
        /// <para>code.Add("display", "Passenger");</para>
        /// <para>ExampleDeathRecord.TransportationRole = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Transportation Role: {ExampleDeathRecord.TransportationRole['display']}");</para>
        /// </example>
        [Property("Transportation Role", Property.Types.Dictionary, "Death Investigation", "Transportation Role in death.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Decedent-Transportation-Role.html", true, 45)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='69451-3')", "")]
        public Dictionary<string, string> TransportationRole
        {
            get
            {
                if (TransportationRoleObs?.Value != null && TransportationRoleObs.Value as CodeableConcept != null)
                {
                    return CodeableConceptToDict((CodeableConcept)TransportationRoleObs.Value);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (TransportationRoleObs == null)
                {
                    TransportationRoleObs = new Observation();
                    TransportationRoleObs.Id = Guid.NewGuid().ToString();
                    TransportationRoleObs.Meta = new Meta();
                    string[] t_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Decedent-Transportation-Role" };
                    TransportationRoleObs.Meta.Profile = t_profile;
                    TransportationRoleObs.Status = ObservationStatus.Final;
                    TransportationRoleObs.Code = new CodeableConcept("http://loinc.org", "69451-3", "Transportation role of decedent ", null);
                    TransportationRoleObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    TransportationRoleObs.Value = DictToCodeableConcept(value);
                    AddReferenceToComposition(TransportationRoleObs.Id);
                    Bundle.AddResourceEntry(TransportationRoleObs, "urn:uuid:" + TransportationRoleObs.Id);
                }
                else
                {
                    TransportationRoleObs.Value = DictToCodeableConcept(value);
                }
            }
        }

        /// <summary>Tobacco Use Contributed To Death.</summary>
        /// <value>if tobacco use contributed to death. A Dictionary representing a code, containing the following key/value pairs:
        /// <para>"code" - the code</para>
        /// <para>"system" - the code system this code belongs to</para>
        /// <para>"display" - a human readable meaning of the code</para>
        /// </value>
        /// <example>
        /// <para>// Setter:</para>
        /// <para>Dictionary&lt;string, string&gt; code = new Dictionary&lt;string, string&gt;();</para>
        /// <para>code.Add("code", "373066001");</para>
        /// <para>code.Add("system", "http://snomed.info/sct");</para>
        /// <para>code.Add("display", "Yes");</para>
        /// <para>ExampleDeathRecord.TobaccoUse = code;</para>
        /// <para>// Getter:</para>
        /// <para>Console.WriteLine($"Tobacco Use: {ExampleDeathRecord.TobaccoUse['display']}");</para>
        /// </example>
        [Property("Tobacco Use", Property.Types.Dictionary, "Death Investigation", "If Tobacco Use Contributed To Death.", true, "http://build.fhir.org/ig/HL7/vrdr/StructureDefinition-VRDR-Tobacco-Use-Contributed-To-Death.html", true, 32)]
        [PropertyParam("code", "The code used to describe this concept.")]
        [PropertyParam("system", "The relevant code system.")]
        [PropertyParam("display", "The human readable version of this code.")]
        [FHIRPath("Bundle.entry.resource.where($this is Observation).where(code.coding.code='69443-0')", "")]
        public Dictionary<string, string> TobaccoUse
        {
            get
            {
                if (TobaccoUseObs != null && TobaccoUseObs.Value != null && TobaccoUseObs.Value as CodeableConcept != null)
                {
                    return CodeableConceptToDict((CodeableConcept)TobaccoUseObs.Value);
                }
                return EmptyCodeDict();
            }
            set
            {
                if (TobaccoUseObs == null)
                {
                    TobaccoUseObs = new Observation();
                    TobaccoUseObs.Id = Guid.NewGuid().ToString();
                    TobaccoUseObs.Meta = new Meta();
                    string[] tb_profile = { "http://hl7.org/fhir/us/vrdr/StructureDefinition/VRDR-Tobacco-Use-Contributed-To-Death" };
                    TobaccoUseObs.Meta.Profile = tb_profile;
                    TobaccoUseObs.Status = ObservationStatus.Final;
                    TobaccoUseObs.Code = new CodeableConcept("http://loinc.org", "69443-0", "Did tobacco use contribute to death", null);
                    TobaccoUseObs.Subject = new ResourceReference("urn:uuid:" + Decedent.Id);
                    TobaccoUseObs.Value = DictToCodeableConcept(value);
                    AddReferenceToComposition(TobaccoUseObs.Id);
                    Bundle.AddResourceEntry(TobaccoUseObs, "urn:uuid:" + TobaccoUseObs.Id);
                }
                else
                {
                    TobaccoUseObs.Value = DictToCodeableConcept(value);
                }
            }
        }


        /////////////////////////////////////////////////////////////////////////////////
        //
        // Class helper methods useful for building, searching through records.
        //
        /////////////////////////////////////////////////////////////////////////////////

        /// <summary>Add a reference to the Death Record Composition.</summary>
        /// <param name="reference">a reference.</param>
        private void AddReferenceToComposition(string reference)
        {
            Composition.Section.First().Entry.Add(new ResourceReference("urn:uuid:" + reference));
        }

        /// <summary>Remove a reference from the Death Record Composition.</summary>
        /// <param name="reference">a reference.</param>
        private bool RemoveReferenceFromComposition(string reference)
        {
            return Composition.Section.First().Entry.RemoveAll(entry => entry.Reference == reference) > 0;
        }

        /// <summary>Restores class references from a newly parsed record.</summary>
        private void RestoreReferences()
        {
            // Grab Composition
            var compositionEntry = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Composition );
            if (compositionEntry != null)
            {
                Composition = (Composition)compositionEntry.Resource;
            }
            else
            {
                throw new System.ArgumentException("Failed to find a Composition. The first entry in the FHIR Bundle should be a Composition.");
            }

            // Grab Patient
            if (Composition.Subject == null || String.IsNullOrWhiteSpace(Composition.Subject.Reference))
            {
                throw new System.ArgumentException("The Composition is missing a subject (a reference to the Decedent resource).");
            }
            var patientEntry = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Patient );
            if (patientEntry != null)
            {
                Decedent = (Patient)patientEntry.Resource;
            }
            else
            {
                throw new System.ArgumentException("Failed to find a Decedent (Patient). The second entry in the FHIR Bundle is usually the Decedent (Patient).");
            }

            // Grab Certifier
            if (Composition.Attester == null || Composition.Attester.FirstOrDefault() == null || Composition.Attester.First().Party == null || String.IsNullOrWhiteSpace(Composition.Attester.First().Party.Reference))
            {
                throw new System.ArgumentException("The Composition is missing an attestor (a reference to the Certifier/Practitioner resource).");
            }
            var practitionerEntry = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Practitioner && (entry.FullUrl == Composition.Attester.First().Party.Reference || (entry.Resource.Id != null && entry.Resource.Id == Composition.Attester.First().Party.Reference)));
            if (practitionerEntry != null)
            {
                Certifier = (Practitioner)practitionerEntry.Resource;
            }
            else
            {
                throw new System.ArgumentException("Failed to find a Certifier (Practitioner). The third entry in the FHIR Bundle is usually the Certifier (Practitioner). Either the Certifier is missing from the Bundle, or the attestor reference specified in the Composition is incorrect.");
            }

            // Grab Pronouncer
            // IMPROVEMENT: Move away from using meta profile to find this Practitioner.
            var pronouncerEntry = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Practitioner && entry.Resource.Meta.Profile.FirstOrDefault() != null && MatchesProfile("VRDR-Death-Pronouncement-Performer", entry.Resource.Meta.Profile.FirstOrDefault()));
            if (pronouncerEntry != null)
            {
                Pronouncer = (Practitioner)pronouncerEntry.Resource;
            }

            // Grab Mortician
            // IMPROVEMENT: Move away from using meta profile or id to find this Practitioner, use reference from disposition method performer instead or as well
            var morticianEntry = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Practitioner && entry.Resource.Meta.Profile.FirstOrDefault() != null && MatchesProfile("VRDR-Mortician", entry.Resource.Meta.Profile.FirstOrDefault()));
            if (morticianEntry == null)
            {
                morticianEntry = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Practitioner && ((Practitioner)entry.Resource).Id != Certifier.Id );
            }
            if (morticianEntry != null)
            {
                Mortician = (Practitioner)morticianEntry.Resource;
            }

            // Grab Death Certification
            var procedureEntry = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Procedure );
            if (procedureEntry != null)
            {
                DeathCertification = (Procedure)procedureEntry.Resource;
            }

            // Grab State Local Identifier
            var stateDocumentReferenceEntry = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.DocumentReference && ((DocumentReference)entry.Resource).Type.Coding.First().Code == "64297-5" );
            if (stateDocumentReferenceEntry != null)
            {
                StateDocumentReference = (DocumentReference)stateDocumentReferenceEntry.Resource;
            }

            // Grab Interested Party
            var interestedParty = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Organization && ((Organization)entry.Resource).Active != null );
            if (interestedParty != null)
            {
                InterestedParty = (Organization)interestedParty.Resource;
            }

            // Grab Funeral Home
            var funeralHome = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Organization && (InterestedParty == null || ((Organization)entry.Resource).Id != InterestedParty.Id) );
            if (funeralHome != null)
            {
                FuneralHome = (Organization)funeralHome.Resource;
            }

            // Grab Funeral Home Director
            var funeralHomeDirector = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.PractitionerRole );
            if (funeralHomeDirector != null)
            {
                FuneralHomeDirector = (PractitionerRole)funeralHomeDirector.Resource;
            }

            // Scan through all Observations to make sure they all have codes!
            foreach (var ob in Bundle.Entry.Where( entry => entry.Resource.ResourceType == ResourceType.Observation ))
            {
                Observation obs = (Observation)ob.Resource;
                if (obs.Code == null || obs.Code.Coding == null || obs.Code.Coding.FirstOrDefault() == null || obs.Code.Coding.First().Code == null)
                {
                    throw new System.ArgumentException("Found an Observation resource that did not contain a code. All Observations must include a code to specify what the Observation is referring to.");
                }
            }

            // Grab Manner of Death
            var mannerOfDeath = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "69449-7" );
            if (mannerOfDeath != null)
            {
                MannerOfDeath = (Observation)mannerOfDeath.Resource;
            }

            // Grab Transportation Role Observation
            var transportationRole = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "69451-3" );
            if (transportationRole != null)
            {
                TransportationRoleObs = (Observation)transportationRole.Resource;
            }

            // Grab Disposition Method
            var dispositionMethod = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "80905-3" );
            if (dispositionMethod != null)
            {
                DispositionMethod = (Observation)dispositionMethod.Resource;
            }

            // Grab Cause Of Death Condition Pathway
            var causeOfDeathConditionPathway = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.List );
            if (causeOfDeathConditionPathway != null)
            {
                CauseOfDeathConditionPathway = (List)causeOfDeathConditionPathway.Resource;
            }

            // Grab Causes of Death using CauseOfDeathConditionPathway
            List<Condition> causeConditions = new List<Condition>();
            if (CauseOfDeathConditionPathway != null)
            {
                foreach (List.EntryComponent condition in CauseOfDeathConditionPathway.Entry)
                {
                    if (condition != null && condition.Item != null && condition.Item.Reference != null)
                    {
                        var codCond = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Condition && (entry.FullUrl == condition.Item.Reference || (entry.Resource.Id != null && entry.Resource.Id == condition.Item.Reference)) );
                        if (codCond != null)
                        {
                            causeConditions.Add((Condition)codCond.Resource);
                        }
                    }
                }
                if (causeConditions.Count() > 0)
                {
                    CauseOfDeathConditionA = causeConditions[0];
                }
                if (causeConditions.Count() > 1)
                {
                    CauseOfDeathConditionB = causeConditions[1];
                }
                if (causeConditions.Count() > 2)
                {
                    CauseOfDeathConditionC = causeConditions[2];
                }
                if (causeConditions.Count() > 3)
                {
                    CauseOfDeathConditionD = causeConditions[3];
                }
                if (causeConditions.Count() > 4)
                {
                    CauseOfDeathConditionE = causeConditions[4];
                }
                if (causeConditions.Count() > 5)
                {
                    CauseOfDeathConditionF = causeConditions[5];
                }
                if (causeConditions.Count() > 6)
                {
                    CauseOfDeathConditionG = causeConditions[6];
                }
                if (causeConditions.Count() > 7)
                {
                    CauseOfDeathConditionH = causeConditions[7];
                }
                if (causeConditions.Count() > 8)
                {
                    CauseOfDeathConditionI = causeConditions[8];
                }
                if (causeConditions.Count() > 9)
                {
                    CauseOfDeathConditionJ = causeConditions[9];
                }
            }

            // Grab Condition Contributing To Death
            List<Condition> remainingConditions = new List<Condition>();
            foreach (var condition in Bundle.Entry.Where( entry => entry.Resource.ResourceType == ResourceType.Condition ))
            {
                if (condition != null)
                {
                    if (!causeConditions.Contains((Condition)condition.Resource))
                    {
                        remainingConditions.Add((Condition)condition.Resource);
                    }
                }
            }
            if (remainingConditions.Count() > 1)
            {
                throw new System.ArgumentException("There are multiple Condition Contributing to Death resources present. Condition Contributing to Death resources are identified by not being referenced in the Cause of Death Pathway resource, so please confirm that all Cause of Death Conditions are correctly referenced in the Cause of Death Pathway to ensure they are not mistaken for a Condition Contributing to Death resource.");
            }
            else if (remainingConditions.Count() == 1)
            {
                ConditionContributingToDeath = remainingConditions[0];
            }

            // Scan through all RelatedPerson to make sure they all have relationship codes!
            foreach (var rp in Bundle.Entry.Where( entry => entry.Resource.ResourceType == ResourceType.RelatedPerson ))
            {
                RelatedPerson rpn = (RelatedPerson)rp.Resource;
                if (rpn.Relationship == null || rpn.Relationship.FirstOrDefault() == null || rpn.Relationship.FirstOrDefault().Coding == null || rpn.Relationship.FirstOrDefault().Coding.FirstOrDefault() == null || rpn.Relationship.FirstOrDefault().Coding.First().Code == null)
                {
                    throw new System.ArgumentException("Found a RelatedPerson resource that did not contain a relationship code. All RelatedPersons must include a relationship code to specify how the RelatedPerson is related to the subject.");
                }
            }

            // Grab Father
            var father = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.RelatedPerson && ((RelatedPerson)entry.Resource).Relationship.First().Coding.First().Code == "FTH" );
            if (father != null)
            {
                Father = (RelatedPerson)father.Resource;
            }

            // Grab Mother
            var mother = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.RelatedPerson && ((RelatedPerson)entry.Resource).Relationship.First().Coding.First().Code == "MTH" );
            if (mother != null)
            {
                Mother = (RelatedPerson)mother.Resource;
            }

            // Grab Spouse
            var spouse = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.RelatedPerson && ((RelatedPerson)entry.Resource).Relationship.First().Coding.First().Code == "SPS" );
            if (spouse != null)
            {
                Spouse = (RelatedPerson)spouse.Resource;
            }

            // Grab Decedent Education Level
            var decedentEducationLevel = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "80913-7" );
            if (decedentEducationLevel != null)
            {
                DecedentEducationLevel = (Observation)decedentEducationLevel.Resource;
            }

            // Grab Birth Record Identifier
            var birthRecordIdentifier = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "BR" );
            if (birthRecordIdentifier != null)
            {
                BirthRecordIdentifier = (Observation)birthRecordIdentifier.Resource;
            }

            // Grab Employment History
            var employmentHistory = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "21843-8" );
            if (employmentHistory != null)
            {
                UsualWork = (Observation)employmentHistory.Resource;
            }

            // Grab Employment History
            var militaryServiceEntry = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "55280-2" );
            if (militaryServiceEntry != null)
            {
                MilitaryServiceObs = (Observation)militaryServiceEntry.Resource;
            }

            // Grab Disposition Location
            // IMPROVEMENT: Move away from using meta profile to find this exact Location.
            var dispositionLocation = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Location && ((Location)entry.Resource).Meta.Profile.FirstOrDefault() != null && MatchesProfile("VRDR-Disposition-Location", ((Location)entry.Resource).Meta.Profile.FirstOrDefault()));
            if (dispositionLocation != null)
            {
                DispositionLocation = (Location)dispositionLocation.Resource;
            }

            // Grab Injury Location
            // IMPROVEMENT: Move away from using meta profile to find this exact Location.
            var injuryLocation = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Location && ((Location)entry.Resource).Meta.Profile.FirstOrDefault() != null && MatchesProfile("VRDR-Injury-Location", ((Location)entry.Resource).Meta.Profile.FirstOrDefault()));
            if (injuryLocation != null)
            {
                InjuryLocationLoc = (Location)injuryLocation.Resource;
            }

            // Grab Death Location
            // IMPROVEMENT: Move away from using meta profile to find this exact Location.
            var deathLocation = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Location && ((Location)entry.Resource).Meta.Profile.FirstOrDefault() != null && MatchesProfile("VRDR-Death-Location", ((Location)entry.Resource).Meta.Profile.FirstOrDefault()));
            if (deathLocation != null)
            {
                DeathLocationLoc = (Location)deathLocation.Resource;
            }

            // Grab Autopsy Performed
            var autopsyPerformed = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "85699-7" );
            if (autopsyPerformed != null)
            {
                AutopsyPerformed = (Observation)autopsyPerformed.Resource;
            }

            // Grab Age At Death
            var ageAtDeath = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "30525-0" );
            if (ageAtDeath != null)
            {
                AgeAtDeathObs = (Observation)ageAtDeath.Resource;
            }

            // Grab Pregnancy
            var pregnancyStatus = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "69442-2" );
            if (pregnancyStatus != null)
            {
                PregnancyObs = (Observation)pregnancyStatus.Resource;
            }

            // Grab Examiner Contacted
            var examinerContacted = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "74497-9" );
            if (examinerContacted != null)
            {
                ExaminerContactedObs = (Observation)examinerContacted.Resource;
            }

            // Grab Tobacco Use
            var tobaccoUse = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "69443-0" );
            if (tobaccoUse != null)
            {
                TobaccoUseObs = (Observation)tobaccoUse.Resource;
            }

            // Grab Injury Incident
            var injuryIncident = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "11374-6" );
            if (injuryIncident != null)
            {
                InjuryIncidentObs = (Observation)injuryIncident.Resource;
            }

            // Grab Death Date
            var dateOfDeath = Bundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Observation && ((Observation)entry.Resource).Code.Coding.First().Code == "81956-5" );
            if (dateOfDeath != null)
            {
                DeathDateObs = (Observation)dateOfDeath.Resource;
            }
        }

        /// <summary>Convert a "code" dictionary to a FHIR Coding.</summary>
        /// <param name="dict">represents a code.</param>
        /// <returns>the corresponding Coding representation of the code.</returns>
        private Coding DictToCoding(Dictionary<string, string> dict)
        {
            Coding coding = new Coding();
            if (dict != null)
            {
                if (dict.ContainsKey("code") && !String.IsNullOrEmpty(dict["code"]))
                {
                    coding.Code = dict["code"];
                }
                if (dict.ContainsKey("system") && !String.IsNullOrEmpty(dict["system"]))
                {
                    coding.System = dict["system"];
                }
                if (dict.ContainsKey("display") && !String.IsNullOrEmpty(dict["display"]))
                {
                    coding.Display = dict["display"];
                }
            }
            return coding;
        }

        /// <summary>Convert a "code" dictionary to a FHIR CodableConcept.</summary>
        /// <param name="dict">represents a code.</param>
        /// <returns>the corresponding CodeableConcept representation of the code.</returns>
        private CodeableConcept DictToCodeableConcept(Dictionary<string, string> dict)
        {
            CodeableConcept codeableConcept = new CodeableConcept();
            Coding coding = DictToCoding(dict);
            codeableConcept.Coding.Add(coding);
            if (dict != null && dict.ContainsKey("text") && dict["text"] != null && dict["text"].Length > 0)
            {
                codeableConcept.Text = dict["text"];
            }
            return codeableConcept;
        }

        /// <summary>Convert a FHIR Coding to a "code" Dictionary</summary>
        /// <param name="coding">a FHIR Coding.</param>
        /// <returns>the corresponding Dictionary representation of the code.</returns>
        private Dictionary<string, string> CodingToDict(Coding coding)
        {
            Dictionary<string, string> dictionary = EmptyCodeDict();
            if (coding != null)
            {
                if (!String.IsNullOrEmpty(coding.Code))
                {
                    dictionary["code"] = coding.Code;
                }
                if (!String.IsNullOrEmpty(coding.System))
                {
                    dictionary["system"] = coding.System;
                }
                if (!String.IsNullOrEmpty(coding.Display))
                {
                    dictionary["display"] = coding.Display;
                }
            }
            return dictionary;
        }

        /// <summary>Convert a FHIR CodableConcept to a "code" Dictionary</summary>
        /// <param name="codeableConcept">a FHIR CodeableConcept.</param>
        /// <returns>the corresponding Dictionary representation of the code.</returns>
        private Dictionary<string, string> CodeableConceptToDict(CodeableConcept codeableConcept)
        {
            if (codeableConcept != null && codeableConcept.Coding != null)
            {
                Coding coding = codeableConcept.Coding.FirstOrDefault();
                var codeDict = CodingToDict(coding);
                if (codeableConcept != null && codeableConcept.Text != null && codeableConcept.Text.Length > 0)
                {
                    codeDict["text"] = codeableConcept.Text;
                }
                else
                {
                    codeDict["text"] = "";
                }
                return codeDict;
            }
            else
            {
                return EmptyCodeableDict();
            }
        }

        /// <summary>Convert an "address" dictionary to a FHIR Address.</summary>
        /// <param name="dict">represents an address.</param>
        /// <returns>the corresponding FHIR Address representation of the address.</returns>
        private Address DictToAddress(Dictionary<string, string> dict)
        {
            Address address = new Address();
            if (dict != null)
            {
                List<string> lines = new List<string>();
                if (dict.ContainsKey("addressLine1") && !String.IsNullOrEmpty(dict["addressLine1"]))
                {
                    lines.Add(dict["addressLine1"]);
                }
                if (dict.ContainsKey("addressLine2") && !String.IsNullOrEmpty(dict["addressLine2"]))
                {
                    lines.Add(dict["addressLine2"]);
                }
                if (lines.Count() > 0)
                {
                    address.Line = lines.ToArray();
                }
                if (dict.ContainsKey("addressCity") && !String.IsNullOrEmpty(dict["addressCity"]))
                {
                    address.City = dict["addressCity"];
                }
                if (dict.ContainsKey("addressCounty") && !String.IsNullOrEmpty(dict["addressCounty"]))
                {
                    address.District = dict["addressCounty"];
                }
                if (dict.ContainsKey("addressState") && !String.IsNullOrEmpty(dict["addressState"]))
                {
                    address.State = dict["addressState"];
                }
                if (dict.ContainsKey("addressZip") && !String.IsNullOrEmpty(dict["addressZip"]))
                {
                    address.PostalCode = dict["addressZip"];
                }
                if (dict.ContainsKey("addressCountry") && !String.IsNullOrEmpty(dict["addressCountry"]))
                {
                    address.Country = dict["addressCountry"];
                }
            }
            return address;
        }

        /// <summary>Convert a FHIR Address to an "address" Dictionary.</summary>
        /// <param name="addr">a FHIR Address.</param>
        /// <returns>the corresponding Dictionary representation of the FHIR Address.</returns>
        private Dictionary<string, string> AddressToDict(Address addr)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (addr != null)
            {
                if (addr.Line != null && addr.Line.Count() > 0)
                {
                    dictionary.Add("addressLine1", addr.Line.First());
                }
                else
                {
                    dictionary.Add("addressLine1", "");
                }
                if (addr.Line != null && addr.Line.Count() > 1)
                {
                    dictionary.Add("addressLine2", addr.Line.Last());
                }
                else
                {
                    dictionary.Add("addressLine2", "");
                }
                dictionary.Add("addressCity", addr.City);
                dictionary.Add("addressCounty", addr.District);
                dictionary.Add("addressState", addr.State);
                dictionary.Add("addressZip", addr.PostalCode);
                dictionary.Add("addressCountry", addr.Country);
            }
            else
            {
                dictionary.Add("addressLine1", "");
                dictionary.Add("addressLine2", "");
                dictionary.Add("addressCity", "");
                dictionary.Add("addressCounty", "");
                dictionary.Add("addressState", "");
                dictionary.Add("addressZip", "");
                dictionary.Add("addressCountry", "");
            }
            return dictionary;
        }

        /// <summary>Returns an empty "address" Dictionary.</summary>
        /// <returns>an empty "address" Dictionary.</returns>
        private Dictionary<string, string> EmptyAddrDict()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("addressLine1", "");
            dictionary.Add("addressLine2", "");
            dictionary.Add("addressCity", "");
            dictionary.Add("addressCounty", "");
            dictionary.Add("addressState", "");
            dictionary.Add("addressZip", "");
            dictionary.Add("addressCountry", "");
            return dictionary;
        }

        /// <summary>Returns an empty "code" Dictionary.</summary>
        /// <returns>an empty "code" Dictionary.</returns>
        private Dictionary<string, string> EmptyCodeDict()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("code", "");
            dictionary.Add("system", "");
            dictionary.Add("display", "");
            return dictionary;
        }

        /// <summary>Returns an empty "codeable" Dictionary.</summary>
        /// <returns>an empty "codeable" Dictionary.</returns>
        private Dictionary<string, string> EmptyCodeableDict()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("code", "");
            dictionary.Add("system", "");
            dictionary.Add("display", "");
            dictionary.Add("text", "");
            return dictionary;
        }

        /// <summary>Given a FHIR path, return the elements that match the given path;
        /// returns an empty array if no matches are found.</summary>
        /// <param name="path">represents a FHIR path.</param>
        /// <returns>all elements that match the given path, or an empty array if no matches are found.</returns>
        public object[] GetAll(string path)
        {
            var matches = Navigator.Select(path);
            ArrayList list = new ArrayList();
            foreach (var match in matches)
            {
                list.Add(match.Value);
            }
            return list.ToArray();
        }

        /// <summary>Given a FHIR path, return the first element that matches the given path.</summary>
        /// <param name="path">represents a FHIR path.</param>
        /// <returns>the first element that matches the given path, or null if no match is found.</returns>
        public object GetFirst(string path)
        {
            var matches = Navigator.Select(path);
            if (matches.Count() > 0)
            {
                return matches.First().Value;
            }
            else
            {
                return null; // Nothing found
            }
        }

        /// <summary>Given a FHIR path, return the last element that matches the given path.</summary>
        /// <param name="path">represents a FHIR path.</param>
        /// <returns>the last element that matches the given path, or null if no match is found.</returns>
        public object GetLast(string path)
        {
            var matches = Navigator.Select(path);
            if (matches.Count() > 0)
            {
                return matches.Last().Value;
            }
            else
            {
                return null; // Nothing found
            }
        }

        /// <summary>Given a FHIR path, return the elements that match the given path as a string;
        /// returns an empty array if no matches are found.</summary>
        /// <param name="path">represents a FHIR path.</param>
        /// <returns>all elements that match the given path as a string, or an empty array if no matches are found.</returns>
        private string[] GetAllString(string path)
        {
            ArrayList list = new ArrayList();
            foreach (var match in GetAll(path))
            {
                list.Add(Convert.ToString(match));
            }
            return list.ToArray(typeof(string)) as string[];
        }

        /// <summary>Given a FHIR path, return the first element that matches the given path as a string;
        /// returns an empty string if no match is found.</summary>
        /// <param name="path">represents a FHIR path.</param>
        /// <returns>the first element that matches the given path as a string, or null if no match is found.</returns>
        private string GetFirstString(string path)
        {
            var first = GetFirst(path);
            if (first != null)
            {
                return Convert.ToString(first);
            }
            else
            {
                return null; // Nothing found
            }
        }

        /// <summary>Given a FHIR path, return the last element that matches the given path as a string;
        /// returns an empty string if no match is found.</summary>
        /// <param name="path">represents a FHIR path.</param>
        /// <returns>the last element that matches the given path as a string, or null if no match is found.</returns>
        private string GetLastString(string path)
        {
            var last = GetLast(path);
            if (last != null)
            {
                return Convert.ToString(last);
            }
            else
            {
                return null; // Nothing found
            }
        }

        /// <summary>Get a value from a Dictionary, but return null if the key doesn't exist.</summary>
        private static string GetValue(Dictionary<string, string> dict, string key)
        {
            string value;
            dict.TryGetValue(key, out value);
            return value;
        }

        /// <summary>Check to make sure the given profile contains the given resource.</summary>
        private static bool MatchesProfile(string resource, string profile)
        {
            if (!String.IsNullOrWhiteSpace(profile) && profile.Contains(resource))
            {
                return true;
            }
            return false;
        }

        /// <summary>Combine the given dictionaries and return the combined result.</summary>
        private static Dictionary<string, string> UpdateDictionary(Dictionary<string, string> a, Dictionary<string, string> b)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach(KeyValuePair<string, string> entry in a)
            {
                dictionary[entry.Key] = entry.Value;
            }
            foreach(KeyValuePair<string, string> entry in b)
            {
                dictionary[entry.Key] = entry.Value;
            }
            return dictionary;
        }

        /// <summary>Returns a JSON encoded structure that maps to the various property
        /// annotations found in the DeathRecord class. This is useful for scenarios
        /// where you may want to display the data in user interfaces.</summary>
        /// <returns>a string representation of this Death Record in a descriptive format.</returns>
        public string ToDescription()
        {
            Dictionary<string, Dictionary<string, dynamic>> description = new Dictionary<string, Dictionary<string, dynamic>>();
            // the priority values should order the categories as: Decedent Demographics, Decedent Disposition, Death Investigation, Death Certification
            foreach(PropertyInfo property in typeof(DeathRecord).GetProperties().OrderBy(p => ((Property)p.GetCustomAttributes().First()).Priority))
            {
                // Grab property annotation for this property
                Property info = (Property)property.GetCustomAttributes().First();

                // Skip properties that shouldn't be serialized.
                if (!info.Serialize)
                {
                    continue;
                }

                // Add category if it doesn't yet exist
                if (!description.ContainsKey(info.Category))
                {
                    description.Add(info.Category, new Dictionary<string, dynamic>());
                }

                // Add the new property to the category
                Dictionary<string, dynamic> category = description[info.Category];
                category[property.Name] = new Dictionary<string, dynamic>();

                // Add the attributes of the property
                category[property.Name]["Name"] = info.Name;
                category[property.Name]["Type"] = info.Type.ToString();
                category[property.Name]["Description"] = info.Description;
                category[property.Name]["IGUrl"] = info.IGUrl;
                category[property.Name]["CapturedInIJE"] = info.CapturedInIJE;

                // Add snippets
                FHIRPath path = (FHIRPath)property.GetCustomAttributes().Last();
                var matches = Navigator.Select(path.Path);
                if (matches.Count() > 0)
                {
                    if (info.Type == Property.Types.TupleCOD)
                    {
                        // Make sure to grab all of the Conditions for COD
                        string xml = "";
                        string json = "";
                        foreach(var match in matches)
                        {
                            xml += match.ToXml();
                            json += match.ToJson() + ",";
                        }
                        category[property.Name]["SnippetXML"] = xml;
                        category[property.Name]["SnippetJSON"] = "[" + json + "]";
                    }
                    else if (!String.IsNullOrWhiteSpace(path.Element))
                    {
                        // Since there is an "Element" for this path, we need to be more
                        // specific about what is included in the snippets.
                        XElement root = XElement.Parse(matches.First().ToXml());
                        XElement node = root.DescendantsAndSelf("{http://hl7.org/fhir}" + path.Element).FirstOrDefault();
                        if (node != null)
                        {
                            node.Name = node.Name.LocalName;
                            category[property.Name]["SnippetXML"] = node.ToString();
                        }
                        else
                        {
                            category[property.Name]["SnippetXML"] = "";
                        }
                         Dictionary<string, dynamic> jsonRoot =
                            JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(matches.First().ToJson(),
                                new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });
                        if (jsonRoot != null && jsonRoot.Keys.Contains(path.Element))
                        {
                            category[property.Name]["SnippetJSON"] = "{" + $"\"{path.Element}\": \"{jsonRoot[path.Element]}\"" + "}";
                        }
                        else
                        {
                            category[property.Name]["SnippetJSON"] = "";
                        }
                    }
                    else
                    {
                        category[property.Name]["SnippetXML"] = matches.First().ToXml();
                        category[property.Name]["SnippetJSON"] = matches.First().ToJson();
                    }

                }
                else
                {
                    category[property.Name]["SnippetXML"] = "";
                    category[property.Name]["SnippetJSON"] = "";
                }

                // Add the current value of the property
                if (info.Type == Property.Types.Dictionary)
                {
                    // Special case for Dictionary; we want to be able to describe what each key means
                    Dictionary<string, string> value = (Dictionary<string, string>)property.GetValue(this);
                    if (value == null)
                    {
                        continue;
                    }
                    Dictionary<string, Dictionary<string, string>> moreInfo = new Dictionary<string, Dictionary<string, string>>();
                    foreach (PropertyParam propParameter in property.GetCustomAttributes().Reverse().Skip(1).Reverse().Skip(1))
                    {
                        moreInfo[propParameter.Key] = new Dictionary<string, string>();
                        moreInfo[propParameter.Key]["Description"] = propParameter.Description;
                        if (value.ContainsKey(propParameter.Key))
                        {
                            moreInfo[propParameter.Key]["Value"] = value[propParameter.Key];
                        }
                        else
                        {
                            moreInfo[propParameter.Key]["Value"] = null;
                        }
                    }
                    category[property.Name]["Value"] = moreInfo;
                }
                else
                {
                    category[property.Name]["Value"] = property.GetValue(this);
                }
            }
            return JsonConvert.SerializeObject(description);
        }

        /// <summary>Helper method to return a JSON string representation of this Death Record.</summary>
        /// <param name="contents">string that represents </param>
        /// <returns>a new DeathRecord that corresponds to the given descriptive format</returns>
        public static DeathRecord FromDescription(string contents)
        {
            DeathRecord record = new DeathRecord();
            Dictionary<string, Dictionary<string, dynamic>> description =
                JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, dynamic>>>(contents,
                    new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });
            // Loop over each category
            foreach(KeyValuePair<string, Dictionary<string, dynamic>> category in description)
            {
                // Loop over each property
                foreach(KeyValuePair<string, dynamic> property in category.Value)
                {
                    if (!property.Value.ContainsKey("Value") || property.Value["Value"] == null)
                    {
                        continue;
                    }
                    // Set the property on the new DeathRecord based on its type
                    string propertyName = property.Key;
                    Object value = null;
                    if (property.Value["Type"] == Property.Types.String || property.Value["Type"] == Property.Types.StringDateTime)
                    {
                        value = property.Value["Value"].ToString();
                        if (String.IsNullOrWhiteSpace((string)value))
                        {
                            value = null;
                        }
                    }
                    else if (property.Value["Type"] == Property.Types.StringArr)
                    {
                        value = property.Value["Value"].ToObject<String[]>();
                    }
                    else if (property.Value["Type"] == Property.Types.Bool)
                    {
                        value = property.Value["Value"].ToObject<bool>();
                    }
                    else if (property.Value["Type"] == Property.Types.TupleArr)
                    {
                        value = property.Value["Value"].ToObject<Tuple<string, string>[]>();
                    }
                    else if (property.Value["Type"] == Property.Types.TupleCOD)
                    {
                        value = property.Value["Value"].ToObject<Tuple<string, string, Dictionary<string, string>>[]>();
                    }
                    else if (property.Value["Type"] == Property.Types.Dictionary)
                    {
                        Dictionary<string, Dictionary<string, string>> moreInfo =
                            property.Value["Value"].ToObject<Dictionary<string, Dictionary<string, string>>>();
                        Dictionary<string, string> result = new Dictionary<string, string>();
                        foreach(KeyValuePair<string, Dictionary<string, string>> entry in moreInfo)
                        {
                            result[entry.Key] = entry.Value["Value"];
                        }
                        value = result;
                    }
                    if (value != null)
                    {
                        typeof(DeathRecord).GetProperty(propertyName).SetValue(record, value);
                    }
                }
            }
            return record;
        }
    }

    /// <summary>Property attribute used to describe a DeathRecord property.</summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class Property : System.Attribute
    {
        /// <summary>Enum for describing the property type.</summary>
        public enum Types
        {
            /// <summary>Parameter is a string.</summary>
            String,
            /// <summary>Parameter is an array of strings.</summary>
            StringArr,
            /// <summary>Parameter is like a string, but should be treated as a date and time.</summary>
            StringDateTime,
            /// <summary>Parameter is a bool.</summary>
            Bool,
            /// <summary>Parameter is a Dictionary.</summary>
            Dictionary,
            /// <summary>Parameter is an array of Tuples.</summary>
            TupleArr,
            /// <summary>Parameter is an array of Tuples, specifically for CausesOfDeath.</summary>
            TupleCOD
        };

        /// <summary>Name of this property.</summary>
        public string Name;

        /// <summary>The property type (e.g. string, bool, Dictionary).</summary>
        public Types Type;

        /// <summary>Category of this property.</summary>
        public string Category;

        /// <summary>Description of this property.</summary>
        public string Description;

        /// <summary>If this field should be kept when serialzing.</summary>
        public bool Serialize;

        /// <summary>URL that links to the IG description for this property.</summary>
        public string IGUrl;

        /// <summary>If this field has an equivalent in IJE.</summary>
        public bool CapturedInIJE;

        /// <summary>Priority that this should show up in generated lists. Lower numbers come first.</summary>
        public int Priority;

        /// <summary>Constructor.</summary>
        public Property(string name, Types type, string category, string description, bool serialize, string igurl, bool capturedInIJE, int priority = 4)
        {
            this.Name = name;
            this.Type = type;
            this.Category = category;
            this.Description = description;
            this.Serialize = serialize;
            this.IGUrl = igurl;
            this.CapturedInIJE = capturedInIJE;
            this.Priority = priority;
        }
    }

    /// <summary>Property attribute used to describe a DeathRecord property parameter,
    /// specifically if the property is a dictionary that has keys.</summary>
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = true)]
    public class PropertyParam : System.Attribute
    {
        /// <summary>If the related property is a Dictionary, the key name.</summary>
        public string Key;

        /// <summary>Description of this parameter.</summary>
        public string Description;

        /// <summary>Constructor.</summary>
        public PropertyParam(string key, string description)
        {
            this.Key = key;
            this.Description = description;
        }
    }

    /// <summary>Describes a FHIR path that can be used to get to the element.</summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class FHIRPath : System.Attribute
    {
        /// <summary>The relevant FHIR path.</summary>
        public string Path;

        /// <summary>The relevant element.</summary>
        public string Element;

        /// <summary>Constructor.</summary>
        public FHIRPath(string path, string element)
        {
            this.Path = path;
            this.Element = element;
        }
    }
}
