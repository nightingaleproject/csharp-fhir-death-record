[![Build Status](https://travis-ci.org/nightingaleproject/vrdr-dotnet.svg?branch=master)](https://travis-ci.org/nightingaleproject/vrdr-dotnet)
[![Nuget](https://img.shields.io/nuget/v/VRDR.svg)](https://www.nuget.org/packages/VRDR)

# vrdr-dotnet
This repository includes .NET (C#) code for producing and consuming the Vital Records Death Reporting (VRDR) Health Level 7 (HL7) Fast Healthcare Interoperability Resources (FHIR) standard. [Click here to view the FHIR Implementation Guide](http://hl7.org/fhir/us/vrdr/2019May/). This code also includes support for converting VRDR FHIR records to and from the Inter-Jurisdictional Exchange (IJE) Mortality format, as well as companion microservice for performing conversions.

## Project Organization

### VRDR
This directory contains a FHIR Death Record library for consuming and producing VRDR FHIR. This library also includes support for converting to and from the Inter-Jurisdictional Exchange (IJE) Mortality format.

For API documentation, [click here](VRDR/DeathRecord.md).

#### Usage
This package is published on NuGet, so including it is as easy as:
```xml
<ItemGroup>
  ...
  <PackageReference Include="VRDR" Version="2.14.0" />
  ...
</ItemGroup>
```

You can also include the library by referencing `DeathRecord.csproj` in your project configuration, for example (taken from VRDR.CLI):
```xml
<Project Sdk="Microsoft.NET.Sdk">
  ...
  <ItemGroup>
    <ProjectReference Include="..\VRDR\DeathRecord.csproj" />
    ...
  </ItemGroup>
</Project>
```

#### Producing Example
The following snippet is a quick example of producing a from-scratch FHIR VRDR record using this library, and then printing it out as a JSON string. For a complete example, [click here](https://github.com/nightingaleproject/csharp-fhir-death-record/blob/master/VRDR.CLI/Program.cs#L23).
```cs
using VRDR;

DeathRecord deathRecord = new DeathRecord();

// Set Death Record ID
deathRecord.Identifier = "42";

// Add Decedent Given Names
string[] givenNames = { "First", "Middle" };
deathRecord.GivenNames = givenNames;

// Add Decedent Last Name
deathRecord.FamilyName = "Last";

// Cause of Death Part I, Line a
deathRecord.COD1A = "Rupture of myocardium";

// Cause of Death Part I Interval, Line a
deathRecord.INTERVAL1A = "Minutes";

// Cause of Death Part I Code, Line a
Dictionary<string, string> exampleCode = new Dictionary<string, string>();
code.Add("code", "I21.0");
code.Add("system", "ICD-10");
code.Add("display", "Acute transmural myocardial infarction of anterior wall");
deathRecord.CODE1A = exampleCode;

// Add PregnancyStatus
Dictionary<string, string> code = new Dictionary<string, string>();
code.Add("code", "PHC1260");
code.Add("system", "urn:oid:2.16.840.1.114222.4.5.274");
code.Add("display", "Not pregnant within past year");
deathRecord.PregnancyStatus = code;

// Add ExaminerContacted
deathRecord.ExaminerContacted = false;

// Add DateOfDeath
deathRecord.DateOfDeath = "2018-07-10T10:04:00+00:00";

// Print record as a JSON string
Console.WriteLine(deathRecord.ToJSON());
```

#### Consuming Example
An example of consuming a VRDR FHIR document (in XML format) using this library, and printing some details from it:
```cs
using VRDR;

// Read in FHIR Death Record XML file as a string
string xml = File.ReadAllText("./example_vrdr_fhir_record.xml");

// Construct a new DeathRecord object from the FHIR VRDR XML string
DeathRecord deathRecord = new DeathRecord(xml);

// Print out some details from the record
Console.WriteLine($"Decedent's Last Name: {deathRecord.FamilyName}");

Console.WriteLine($"Date/Time of Death: {deathRecord.DateOfDeath}");

Console.WriteLine($"Cause of Death Part I, Line a: {deathRecord.COD1A}");
Console.WriteLine($"Cause of Death Part I Interval, Line a: {deathRecord.INTERVAL1A}");
Console.WriteLine($"Cause of Death Part I Code, Line a: {String.Join(", ", deathRecord.CODE1A.Select(x => x.Key + "=" + x.Value).ToArray())}");
```

#### FHIR VRDR record to/from IJE Mortality format
An example of converting a VRDR FHIR Death Record to an IJE string:
```cs
using VRDR;

// Read in FHIR Death Record XML file as a string
string xml = File.ReadAllText("./example_vrdr_fhir_record.xml");

// Construct a new DeathRecord object from the string
DeathRecord deathRecord = new DeathRecord(xml);

// Create an IJEMortality instance from the DeathRecord
IJEMortality ije = new IJEMortality(deathRecord);

// Print out the corresponding IJE version of the DeathRecord
string ijeString = ije.ToString(); // Converts DeathRecord to IJE
Console.WriteLine(ijeString);
```

An example of converting an IJE string to a VRDR FHIR Death Record:
```cs
using VRDR;

// Construct a new IJEMortality instance from an IJE string
IJEMortality ije = new IJEMortality("..."); // This will convert the IJE string to a DeathRecord

// Grab the corresponding FHIR DeathRecord
DeathRecord deathRecord = ije.ToDeathRecord();

// Print out the converted FHIR DeathRecord as a JSON string
Console.WriteLine(deathRecord.ToJSON());
```

#### CauseCodes
This package also includes a class for handling the preliminary return message from NCHS containing coded causes.
```cs
using VRDR;

// Initialize a new CauseCodes; fill with ids and codes
CauseCodes causeCodes = new CauseCodes();
causeCodes.Identifier = "42";
causeCodes.BundleIdentifier = "MA000001";

List<string> codes = new List<string>();
codes.Add("I251");
codes.Add("I259");
codes.Add("I250");
causeCodes.Codes = codes.ToArray();

// Serialize to a JSON string
string json = causeCodes.ToJSON();

// Deserizlie back into a CauseCodes object
CauseCodes example = new CauseCodes(json);

// Print out as XML
Console.WriteLine(example.ToXML());
```

### VRDR.Messaging
This directory contains classes to create and parse FHIR messages used for Vital Records Death Reporting.

#### Creating a Death Record Submission

```cs
// Create a DeathRecord (see examples above)
DeathRecord record = ...;

// Create a submission message
DeathRecordSubmission message = new DeathRecordSubmission(record);

// Create a JSON representation of the message (XML is also supported via the ToXML method) 
string jsonMessage = message.ToJSON();

// Send the JSON message
...
```

The `DeathRecordSubmission` class supports several properties that enable customization of the message contents, E.g., the `MessageSource` property allows the sender of the message to be specified.

#### Consuming a Death Record Submission

```cs
// Get the DeathRecordSubmission message JSON or XML representation
StreamReader messageStream = ...;

// Parse the JSON
DeathRecordSubmission message = (DeathRecordSubmission)BaseMessage.Parse(messageStream);

// Get the DeathRecord
DeathRecord record = message.MessagePayload;

// Process the DeathRecord
...
```

#### Creating an Acknowledgement Message

```cs
// Get the DeathRecordSubmission message
DeathRecordSubmission message = ...;

// Create the acknowledgement message
AckMessage ack = new AckMessage(message);

// Create a JSON representation of the acknowledgement message
string jsonAck = ack.ToJSON();

// Send the JSON acknowledgement message
...
```

Note that the `AckMessage` constructor will automatically set the message header properties to identify the `DeathRecordSubmission` message that it acknowledges. It will also set the `MessageDestination` property to the value of the `DeathRecordSubmission.MessageSource` property.

#### Creating a Coding Response Message

```cs
// Create an empty coding response message
CodingResponseMessage message = new CodingResponseMessage("https://example.org/jurisdiction/endpoint");

// Assign the certificate number and satte identifier
message.CertificateNumber = "...";
message.StateIdentifier = "...";

// Create the ethnicity coding
var ethnicity = new Dictionary<CodingResponseMessage.HispanicOrigin, string>();
ethnicity.Add(CodingResponseMessage.HispanicOrigin.DETHNICE, "...");
ethnicity.Add(CodingResponseMessage.HispanicOrigin.DETHNIC5C, "...");
message.Ethnicity = ethnicity;

// Create the race coding
var race = new Dictionary<CodingResponseMessage.RaceCode, string>();
race.Add(CodingResponseMessage.RaceCode.RACE1E, "...");
race.Add(CodingResponseMessage.RaceCode.RACE17C, "...");
race.Add(CodingResponseMessage.RaceCode.RACEBRG, "...");
message.Race = race;

// Create the cause of death coding
message.UnderlyingCauseOfDeath = "...";

var recordAxisCodes = new List<string>();
recordAxisCodes.Add("...");
recordAxisCodes.Add("...");
recordAxisCodes.Add("...");
recordAxisCodes.Add("...");
message.CauseOfDeathRecordAxis = recordAxisCodes;

var entityAxisEntries = new List<CauseOfDeathEntityAxisEntry>();
var entry1 = new CauseOfDeathEntityAxisEntry("DEATH CERT LINE 1 TEXT", "ID of VRDR CauseOfDeathCondition");
entry1.AssignedCodes.Add("...");
entry1.AssignedCodes.Add("...");
entityAxisEntries.Add(entry1);
var entry2 = new CauseOfDeathEntityAxisEntry("DEATH CERT LINE 2 TEXT", "ID of VRDR CauseOfDeathCondition");
entry2.AssignedCodes.Add("...");
entityAxisEntries.Add(entry2);
message.CauseOfDeathEntityAxis = entityAxisEntries;

// Create a JSON representation of the coding response message
string jsonMesg = ack.ToJSON();

// Send the JSON coding response message
...
```

#### Creating a Coding Error Message

```cs
// Create the coding error message
var errMsg = CodingErrorMessage(certificateNumber, stateIdentifier, destinationEndpoint);

// Add the issues found during processing
var issues = new List<Issue>();
var issue = new Issue(OperationOutcome.IssueSeverity.Fatal, OperationOutcome.IssueType.Invalid, "Description of first issues");
issues.Add(issue);
issue = new Issue(OperationOutcome.IssueSeverity.Warning, OperationOutcome.IssueType.Expired, "Description of second issues");
issues.Add(issue);
err.Issues = issues;

// Create a JSON representation of the coding response message
string jsonMesg = ack.ToJSON();

// Send the JSON coding error response message
...
```

### VRDR.Tests
This directory contains unit and functional tests for the VRDR library.

#### Usage
The tests are automatically run by this repositories Travis CI config, but can be run locally by executing the following command in the root project directory:
```bash
dotnet test VRDR.Tests/DeathRecord.Tests.csproj
```

### VRDR.CLI
This directory contains a sample command line interface app that uses the VRDR library to do a few different things.

#### Example Usages
```bash
# Builds a fake death record and print out the record as FHIR XML and JSON
dotnet run

# Read in the FHIR XML or JSON death record and print out as IJE
dotnet run 2ije 1.xml

# Read in the IJE death record and print out as FHIR XML
dotnet run ije2xml 1.MOR

# Read in the IJE death record and print out as FHIR JSON
dotnet run ije2json 1.MOR

# Read in the FHIR XML death record and print out as FHIR JSON
dotnet run xml2json 1.xml

# Read in the FHIR JSON death record and print out as FHIR XML
dotnet run json2xml 1.json

# Read in the FHIR JSON death record, completely disassemble then reassemble, and print as FHIR JSON
dotnet run json2json 1.json

# Read in the FHIR XML death record, completely disassemble then reassemble, and print as FHIR XML
dotnet run xml2xml 1.xml

# Read in the given FHIR xml (being permissive) and print out the same; useful for doing validation diffs
dotnet run checkXml 1.xml

# Read in the given FHIR json (being permissive) and print out the same; useful for doing validation diffs
dotnet run checkJson 1.json

# Read in and parse an IJE death record and print out the values for every (supported) field
dotnet run ije 1.MOR
```

### VRDR.HTTP
This directory contains a deployable microservice that exposes endpoints for conversion of IJE flat files to DeathRecord JSON or XML, and vice versa.

The current available endpoints to `POST` to are:
```
http://<server>:8080/xml   <- For requesting a record as FHIR in XML format
http://<server>:8080/json  <- For requesting a record as FHIR in JSON format
http://<server>:8080/ije   <- For requesting a record as IJE
```

Include a `Content-Type` header indicating what format the record is represented as in the body of the message (e.g. `application/fhir+json`, `application/fhir+xml`, or `application/ije`.).

#### Running

To build a Dockerized version from scratch (from source), you can do so by running (inside the project root directory):

```
dotnet publish
docker build -t vrdr-microservice .
docker run -p 8080:8080 vrdr-microservice
```

If you prefer not to use Docker, you can run it from the root project directory using [.NET Core](https://dotnet.microsoft.com/download):

```
dotnet run
```

The service will be listening locally at `http://localhost:8080`.

## License

Copyright 2018, 2019, 2020 The MITRE Corporation

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at

```
http://www.apache.org/licenses/LICENSE-2.0
```

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

### Contact Information

For questions or comments about vrdr-dotnet, please send email to

    cdc-nvss-feedback-list@lists.mitre.org
