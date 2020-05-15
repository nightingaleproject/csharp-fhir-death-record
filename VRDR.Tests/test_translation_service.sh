#!/bin/bash

# Start translation service
cd VRDR.HTTP
dotnet run &
sleep 10

# Convert FHIR JSON => IJE
curl --data-binary "@../VRDR.Tests/fixtures/json/DeathRecord1.json" -H "Content-Type: application/fhir+json" -X POST http://localhost:8080/ije > 1.tmp

# Convert IJE => FHIR XML
curl --data-binary "@1.tmp" -H "Content-Type: application/ije" -X POST http://localhost:8080/xml > 2.tmp

# Convert FHIR XML => FHIR JSON
curl --data-binary "@2.tmp" -H "Content-Type: application/fhir+xml" -X POST http://localhost:8080/json > 3.tmp

# Convert FHIR JSON => IJE
curl --data-binary "@3.tmp" -H "Content-Type: application/fhir+json" -X POST http://localhost:8080/ije > 4.tmp

# Make all characters lower case for comparison (we don't really care if United States != UNITED STATES)
tr '[:upper:]' '[:lower:]' < 1.tmp > ije1.tmp
tr '[:upper:]' '[:lower:]' < 4.tmp > ije2.tmp

# Test that IJE records are the same
if diff ije1.tmp ije2.tmp; then
  echo "IJE matched! Roundtrip passed!"
else
  echo "IJE was different! Roundtrip failed!"
  exit 1
fi

# Convert FHIR JSON => Nightingale
curl --data-binary "@../VRDR.Tests/fixtures/json/DeathRecord1.json" -H "Content-Type: application/fhir+json" -X POST http://localhost:8080/nightingale > 1.nightingale

# Convert Nightingale => FHIR JSON
curl --data-binary "@1.nightingale" -H "Content-Type: application/nightingale" -X POST http://localhost:8080/json > 1.nightingale.json

# Convert FHIR JSON => Nightingale
curl --data-binary "@1.nightingale.json" -H "Content-Type: application/fhir+json" -X POST http://localhost:8080/nightingale > 2.nightingale

# Test that nightingale records are the same
if diff 1.nightingale 2.nightingale; then
  echo "Nightingale matched! Roundtrip passed!"
  exit 0
else
  echo "Nightingale was different! Roundtrip failed!"
  exit 1
fi
