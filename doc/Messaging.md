# Using the VRDR.Messaging Library

This document describes how to use the VRDR.Messaging library to simplify implementation of VRDR message exchange flows.

## Death Record Submission

The diagram below illustrates the messages exchanged during submission of a death record and the subsequent return of coded causes of death and race and ethnicity information.

![Message Exchange Pattern for Death Record Submission and Coding Response](submission.png)

### Submit Death Record

A vital records jurisdiction can create a death record submission as follows:

```cs
// Create a DeathRecord
DeathRecord record = ...;

// Create a submission message
DeathRecordSubmission message = new DeathRecordSubmission(record);
message.MessageSource = "https://example.com/juristdiction/message/endpoint";

// Create a JSON representation of the message (XML is also supported via the ToXML method)
string jsonMessage = message.ToJSON();

// Send the JSON message
...
```

The `DeathRecordSubmission` constructor will extract information from the supplied `DeathRecord` to populate the corresponding message property values (`StateIdentifier`, `CertificateNumber`, `NCHSIdentifier`) automatically.

### Extract Death Record

On receipt of a message, NCHS can parse it, determine the type of the message, and extract a death record using the following steps:

```cs
// Get the message as a stream
StreamReader messageStream = ...;

// Parse the message
BaseMessage message = BaseMessage.Parse(messageStream);

// Switch to determine the type of message
switch(message)
{
    case DeathRecordSubmission submission:
        var record = submission.DeathRecord;
        var nchsId = submission.NCHSIDentifier;
        ProcessSubmission(record, nchsId);
        break;
    ...
}
```

### Acknowledge Death Record

If extraction was succesful, NCHS can generate an acknowledgement message and send it to the submitting jurisdiction. An `AckMessage` constructor is available that accepts a source message parameter (`submission` in the example below) and this is used to initialize `AckMessage` properties:

- `AckMessage.MessageDestination` will be assigned the value of `source.MessageSource`
- `AckMessage.AckedMessageId` will be assigned the value of `source.MessageId`
- `StateIdentifier` will be assigned the value of `source.StateIdentifier`
- `CertificateNumber` will be assigned the value of `source.CertificateNumber`
- `NCHSIdentifier` will be assigned the value of `source.NCHSIdentifier`

The constructor will also default the value of the `MessageSource` property to `http://nchs.cdc.gov/vrdr_submission` which is suitable for messaging originating from NCHS.

```cs
// Create the acknowledgement message
var ackMsg = new AckMessage(submission);

// Serialize the acknowledgement message
string ackMsgJson = ackMsg.ToJSON();

// Send the JSON message
...
```

On receipt of the `AckMessage`, the jurisdiction can parse it, determine the type of the message, and extract the required information using the following steps:

```cs
// Get the message as a stream
StreamReader messageStream = ...;

// Parse the message
BaseMessage message = BaseMessage.Parse(messageStream);

// Switch to determine the type of message
switch(message)
{
    case AckMessage ack:
        var ackedMsgId = ack.AckedMessageId;
        var nchsId = ack.NCHSIdentifier;
        var stateId = ack.StateIdentifier;
        ProcessAck(ackedMsgId, nchsId, stateId);
        break;
    ...
}
```

### Return Coding

NCHS codes both causes of death, and race and ethnicity of decedents. The VRDR.Messaging library supports returning these two types of information together or separately. Here we will assume the two are coded and sent together, if they were coded separately then the corresponding code blocks would simply be omitted..

Once NCHS have determined the causes of death they can create a `CodingResponseMessage` to return that information to the jurisdiction:

```cs
// Create an empty coding response message
CodingResponseMessage message = new CodingResponseMessage("https://example.org/jurisdiction/endpoint");

// Assign the business identifiers
message.CertificateNumber = "...";
message.StateIdentifier = "...";
message.NCHSIdentifier = "...";

// Create the ethnicity coding
var ethnicity = new Dictionary<CodingResponseMessage.HispanicOrigin, string>();
ethnicity.Add(CodingResponseMessage.HispanicOrigin.DETHNICE, <ethnicity code>);
ethnicity.Add(CodingResponseMessage.HispanicOrigin.DETHNIC5C, <ethnicity code>);
message.Ethnicity = ethnicity;

// Create the race coding
var race = new Dictionary<CodingResponseMessage.RaceCode, string>();
race.Add(CodingResponseMessage.RaceCode.RACE1E, <race code>);
race.Add(CodingResponseMessage.RaceCode.RACE17C, <race code>);
race.Add(CodingResponseMessage.RaceCode.RACEBRG, <race code>);
message.Race = race;

// Create the cause of death coding
message.UnderlyingCauseOfDeath = <icd code>;

// Assign the record axis codes
var recordAxisCodes = new List<string>();
recordAxisCodes.Add(<icd code>);
recordAxisCodes.Add(<icd code>);
recordAxisCodes.Add(<icd code>);
recordAxisCodes.Add(<icd code>);
message.CauseOfDeathRecordAxis = recordAxisCodes;

// Assign the entity axis codes
var builder = new CauseOfDeathEntityAxisBuilder();
// for each entity axis codes
...
builder.Add(<lineNumber>, <positionInLine>, <icd code>);
...
// end loop
message.CauseOfDeathEntityAxis = builder.ToCauseOfDeathEntityAxis();

// Create a JSON representation of the coding response message
string jsonMessage = message.ToJSON();

// Send the JSON message
...
```

On receipt of the `CodingResponseMessage`, the jurisdiction can parse it, determine the type of the message, and extract the required information using the following steps:

```cs
// Get the message as a stream
StreamReader messageStream = ...;

// Parse the message
BaseMessage message = BaseMessage.Parse(messageStream);

// Switch to determine the type of message
switch(message)
{
    case CodingResponseMessage coding:
        string nchsId = coding.NCHSIdentifier;
        string stateId = coding.StateIdentifier;
        string cod = coding.CauseOfDeathConditionId;
        Dictionary<CodingResponseMessage.HispanicOrigin, string> ethnicity = coding.Ethnicity;
        Dictionary<CodingResponseMessage.RaceCode, string> race = coding.Race;
        List<string> recordAxis = coding.CauseOfDeathRecordAxis;
        List<CauseOfDeathEntityAxisEntry> entityAxis = coding.CauseOfDeathEntityAxis;
        ProcessCoding(nchsId, stateId, ethnicity, race, cod, recordAxis, entityAxis, );
        break;
    ...
}
```

### Acknowledge Coding

If extraction of the coding information was succesful, the jurisdiction can generate an acknowledgement message and send it to NCHS. As described earlier, the `AckMessage` constructor initializes properties based on the source coding message.

The constructor will default the value of the `MessageSource` property to `http://nchs.cdc.gov/vrdr_submission` which is not suitable for messages originating from jurisdictions so this needs to be overriden.

```cs
// Create the acknowledgement message
var ackMsg = new AckMessage(coding);
ackMsg.MessageSource = "https://example.com/juristdiction/message/endpoint";

// Serialize the acknowledgement message
string ackMsgJson = ackMsg.ToJSON();

// Send the JSON message
...
```

On receipt of the `AckMessage`, NCHS can parse it, determine the type of the message, and extract the required information using the following steps:

```cs
// Get the message as a stream
StreamReader messageStream = ...;

// Parse the message
BaseMessage message = BaseMessage.Parse(messageStream);

// Switch to determine the type of message
switch(message)
{
    case AckMessage ack:
        var ackedMsgId = ack.AckedMessageId;
        var nchsId = ack.NCHSIdentifier;
        var stateId = ack.StateIdentifier;
        ProcessAck(ackedMsgId, nchsId, stateId);
        break;
    ...
}
```