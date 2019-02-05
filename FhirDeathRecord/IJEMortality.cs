using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Hl7.Fhir.Model;

namespace FhirDeathRecord
{
    /// <summary>Property attribute used to describe a field in the IJE Mortality format.</summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class IJEField : System.Attribute
    {
        /// <summary>Field number.</summary>
        public int Field;

        /// <summary>Beggining location.</summary>
        public int Location;

        /// <summary>Field length.</summary>
        public int Length;

        /// <summary>Description of what the field contains.</summary>
        public string Contents;

        /// <summary>Field name.</summary>
        public string Name;

        /// <summary>Priority - lower will be "GET" and "SET" earlier.</summary>
        public int Priority;

        /// <summary>Constructor.</summary>
        public IJEField(int field, int location, int length, string contents, string name, int priority)
        {
            this.Field = field;
            this.Location = location;
            this.Length = length;
            this.Contents = contents;
            this.Name = name;
            this.Priority = priority;
        }
    }

    /// <summary>A "wrapper" class to convert between a FHIR based <c>DeathRecord</c> and
    /// a record in IJE Mortality format. Each property of this class corresponds exactly
    /// with a field in the IJE Mortality format. The getters convert from the embedded
    /// FHIR based <c>DeathRecord</c> to the IJE format for a specific field, and
    /// the setters convert from IJE format for a specific field and set that value
    /// on the embedded FHIR based <c>DeathRecord</c>.</summary>
    public class IJEMortality
    {
        /// <summary>FHIR based death record.</summary>
        private DeathRecord record;

        /// <summary>IJE data lookup helper. Thread-safe singleton!</summary>
        private MortalityData dataLookup = MortalityData.Instance;

        /// <summary>Constructor that takes a <c>DeathRecord</c>.</summary>
        public IJEMortality(DeathRecord record)
        {
            this.record = record;
        }

        /// <summary>Constructor that takes an IJE string and builds a corresponding internal <c>DeathRecord</c>.</summary>
        public IJEMortality(string ije)
        {
            if (ije == null)
            {
                throw new ArgumentException("IJE string cannot be null.");
            }
            if (ije.Length < 5000)
            {
                ije = ije.PadRight(5000, ' ');
            }
            this.record = new DeathRecord();
            // Loop over every property (these are the fields); Order by priority
            List<PropertyInfo> properties = typeof(IJEMortality).GetProperties().ToList().OrderBy(p => ((IJEField)p.GetCustomAttributes().First()).Priority).ToList();
            foreach(PropertyInfo property in properties)
            {
                // Grab the field attributes
                IJEField info = (IJEField)property.GetCustomAttributes().First();
                // Grab the field value
                string field = ije.Substring(info.Location - 1, info.Length);
                // Set the value on this IJEMortality (and the embedded record)
                property.SetValue(this, field);
            }
        }

        /// <summary>Converts the internal <c>DeathRecord</c> into an IJE string.</summary>
        public override string ToString()
        {
            // Start with empty IJE Mortality record
            StringBuilder ije = new StringBuilder(new String(' ', 5000), 5000);
            // Loop over every property (these are the fields)
            foreach(PropertyInfo property in typeof(IJEMortality).GetProperties())
            {
                // Grab the field value
                string field = Convert.ToString(property.GetValue(this, null));
                // Grab the field attributes
                IJEField info = (IJEField)property.GetCustomAttributes().First();
                // Be mindful about lengths
                if (field.Length > info.Length)
                {
                    field = field.Substring(0, info.Length);
                }
                // Insert the field value into the record
                ije.Insert(info.Location - 1, field);
            }
            return ije.ToString();
        }

        /// <summary>Returns the corresponding <c>DeathRecord</c> for this IJE string.</summary>
        public DeathRecord ToDeathRecord()
        {
            return this.record;
        }


        /////////////////////////////////////////////////////////////////////////////////
        //
        // Class helper methods for getting and settings IJE fields.
        //
        /////////////////////////////////////////////////////////////////////////////////

        /// <summary>Truncates the given string to the given length.</summary>
        private static string Truncate(string value, int length)
        {
            if (String.IsNullOrWhiteSpace(value) || value.Length <= length)
            {
                return value;
            }
            else
            {
                return value.Substring(0, length);
            }
        }

        /// <summary>Grabs the IJEInfo for a specific IJE field name.</summary>
        private static IJEField FieldInfo(string ijeFieldName)
        {
            return (IJEField)typeof(IJEMortality).GetProperty(ijeFieldName).GetCustomAttributes().First();
        }

        /// <summary>Helps decompose a DateTime into individual parts (year, month, day, time).</summary>
        private string DateTimeStringHelper(IJEField info, string value, string type, DateTimeOffset date)
        {
            if (type == "yyyy")
            {
                if (value == null || value.Length < 4)
                {
                    return "";
                }
                int year;
                if (Int32.TryParse(Truncate(value, info.Length), out year))
                {
                    date = new DateTimeOffset(year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, TimeSpan.Zero);
                }
            }
            else if (type == "MM")
            {
                if (value == null || value.Length < 2)
                {
                    return "";
                }
                int month;
                if (Int32.TryParse(Truncate(value, info.Length), out month))
                {
                    date = new DateTimeOffset(date.Year, month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, TimeSpan.Zero);
                }
            }
            else if (type == "dd")
            {
                if (value == null || value.Length < 2)
                {
                    return "";
                }
                int day;
                if (Int32.TryParse(Truncate(value, info.Length), out day))
                {
                    date = new DateTimeOffset(date.Year, date.Month, day, date.Hour, date.Minute, date.Second, date.Millisecond, TimeSpan.Zero);
                }
            }
            else if (type == "HHmm")
            {
                if (value == null || value.Length < 4)
                {
                    return "";
                }
                int hour;
                if (Int32.TryParse(Truncate(value, info.Length).Substring(0, 2), out hour))
                {
                    // can't have '99' hour
                    if (hour != 99)
                    {
                        date = new DateTimeOffset(date.Year, date.Month, date.Day, hour, date.Minute, date.Second, date.Millisecond, TimeSpan.Zero);
                    }
                }
                int minute;
                if (Int32.TryParse(Truncate(value, info.Length).Substring(2, 2), out minute))
                {
                    // can't have a '99' minute
                    if (minute != 99)
                    {
                        date = new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, minute, date.Second, date.Millisecond, TimeSpan.Zero);
                    }
                }
            }
            else if (type == "MMddyyyy")
            {
                if (value == null || value.Length < 8)
                {
                    return "";
                }
                int month;
                if (Int32.TryParse(Truncate(value, info.Length).Substring(0, 2), out month))
                {
                    // can't do '99' months
                    if (month != 99)
                    {
                        date = new DateTimeOffset(date.Year, month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, TimeSpan.Zero);
                    }
                }
                int day;
                if (Int32.TryParse(Truncate(value, info.Length).Substring(2, 2), out day))
                {
                    // can't do '99' months
                    if (day != 99)
                    {
                        date = new DateTimeOffset(date.Year, date.Month, day, date.Hour, date.Minute, date.Second, date.Millisecond, TimeSpan.Zero);
                    }
                }
                int year;
                if (Int32.TryParse(Truncate(value, info.Length).Substring(4, 4), out year))
                {
                    // can't do 9999 year
                    if (year != 9999)
                    {
                        date = new DateTimeOffset(year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, TimeSpan.Zero);
                    }
                }
            }
            return date.ToString("o");
        }

        /// <summary>Get a value on the DeathRecord whose type is some part of a DateTime.</summary>
        private string DateTime_Get(string ijeFieldName, string dateTimeType, string fhirFieldName)
        {
            IJEField info = FieldInfo(ijeFieldName);
            DateTimeOffset date;
            string current = Convert.ToString(typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record));
            if (DateTimeOffset.TryParse(current, out date))
            {
                date = date.ToUniversalTime();
                date = new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, TimeSpan.Zero);
                return Truncate(date.ToString(dateTimeType), info.Length);
            }
            else
            {
                return new String(' ', info.Length);
            }
        }

        /// <summary>Set a value on the DeathRecord whose type is some part of a DateTime.</summary>
        private void DateTime_Set(string ijeFieldName, string dateTimeType, string fhirFieldName, string value)
        {
            IJEField info = FieldInfo(ijeFieldName);
            string current = Convert.ToString(typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record));
            DateTimeOffset date;
            if (current != null && DateTimeOffset.TryParse(current, out date))
            {
                date = date.ToUniversalTime();
                date = new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, TimeSpan.Zero);
                typeof(DeathRecord).GetProperty(fhirFieldName).SetValue(this.record, DateTimeStringHelper(info, value, dateTimeType, date));
            }
            else
            {
                date = new DateTimeOffset(1, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
                typeof(DeathRecord).GetProperty(fhirFieldName).SetValue(this.record, DateTimeStringHelper(info, value, dateTimeType, date));
            }
        }

        /// <summary>Get a value on the DeathRecord whose type is some part of a DateTime.</summary>
        private string DateTime_Dictionary_Get(string ijeFieldName, string dateTimeType, string fhirFieldName, string key)
        {
            IJEField info = FieldInfo(ijeFieldName);
            DateTimeOffset date;
            Dictionary<string, string> currentDict = (Dictionary<string, string>)typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record);
            string current = currentDict[key];
            if (DateTimeOffset.TryParse(current, out date))
            {
                date = date.ToUniversalTime();
                date = new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, TimeSpan.Zero);
                return Truncate(date.ToString(dateTimeType), info.Length);
            }
            else
            {
                return new String(' ', info.Length);
            }
        }

        /// <summary>Set a value on the DeathRecord whose type is some part of a DateTime.</summary>
        private void DateTime_Dictionary_Set(string ijeFieldName, string dateTimeType, string fhirFieldName, string key, string value)
        {
            IJEField info = FieldInfo(ijeFieldName);
            Dictionary<string, string> currentDict = (Dictionary<string, string>)typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record);
            string current = currentDict[key];
            DateTimeOffset date;
            if (current != null && DateTimeOffset.TryParse(current, out date))
            {
                date = date.ToUniversalTime();
                date = new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond, TimeSpan.Zero);
                string updated = DateTimeStringHelper(info, value, dateTimeType, date);
                currentDict[key] = updated;
                typeof(DeathRecord).GetProperty(fhirFieldName).SetValue(this.record, currentDict);
            }
            else
            {
                date = new DateTimeOffset(1, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
                
                // If "99" comes in, it's not a valid date. Change back to blank.
                value = "";

                string updated = DateTimeStringHelper(info, value, dateTimeType, date);
                currentDict[key] = updated;
                typeof(DeathRecord).GetProperty(fhirFieldName).SetValue(this.record, currentDict);
            }
        }

        /// <summary>Get a value on the DeathRecord whose IJE type is a left justified string.</summary>
        private string RightJustifiedZeroed_Get(string ijeFieldName, string fhirFieldName)
        {
            IJEField info = FieldInfo(ijeFieldName);
            string current = Convert.ToString(typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record));
            if (current != null)
            {
                return Truncate(current, info.Length).PadLeft(info.Length, '0');
            }
            else
            {
                return new String('0', info.Length);
            }
        }

        /// <summary>Set a value on the DeathRecord whose IJE type is a right justified, zeroed filled string.</summary>
        private void RightJustifiedZeroed_Set(string ijeFieldName, string fhirFieldName, string value)
        {
            IJEField info = FieldInfo(ijeFieldName);
            typeof(DeathRecord).GetProperty(fhirFieldName).SetValue(this.record, value.TrimStart('0'));
        }

        /// <summary>Get a value on the DeathRecord whose IJE type is a left justified string.</summary>
        private string LeftJustified_Get(string ijeFieldName, string fhirFieldName)
        {
            IJEField info = FieldInfo(ijeFieldName);
            string current = Convert.ToString(typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record));
            if (current != null)
            {
                return Truncate(current, info.Length).PadRight(info.Length, ' ');
            }
            else
            {
                return new String(' ', info.Length);
            }
        }

        /// <summary>Set a value on the DeathRecord whose IJE type is a left justified string.</summary>
        private void LeftJustified_Set(string ijeFieldName, string fhirFieldName, string value)
        {
            IJEField info = FieldInfo(ijeFieldName);
            typeof(DeathRecord).GetProperty(fhirFieldName).SetValue(this.record, value.Trim());
        }

        /// <summary>Get a value on the DeathRecord whose property is a Dictionary type.</summary>
        private string Dictionary_Get(string ijeFieldName, string fhirFieldName, string key)
        {
            IJEField info = FieldInfo(ijeFieldName);
            Dictionary<string, string> dictionary = (Dictionary<string, string>)typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record);
            string current = Convert.ToString(dictionary[key]);
            if (current != null)
            {
                return Truncate(current, info.Length).PadRight(info.Length, ' ');
            }
            else
            {
                return new String(' ', info.Length);
            }
        }

        /// <summary>Get a value on the DeathRecord whose property is a Dictionary type, with NO truncating.</summary>
        private string Dictionary_Get_Full(string ijeFieldName, string fhirFieldName, string key)
        {
            IJEField info = FieldInfo(ijeFieldName);
            Dictionary<string, string> dictionary = (Dictionary<string, string>)typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record);
            string current = Convert.ToString(dictionary[key]);
            if (current != null)
            {
                return current;
            }
            else
            {
                return "";
            }
        }

        /// <summary>Set a value on the DeathRecord whose property is a Dictionary type.</summary>
        private void Dictionary_Set(string ijeFieldName, string fhirFieldName, string key, string value)
        {
            IJEField info = FieldInfo(ijeFieldName);
            Dictionary<string, string> dictionary = (Dictionary<string, string>)typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record);
            if (dictionary != null && (!dictionary.ContainsKey(key) || String.IsNullOrWhiteSpace(dictionary[key])))
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    dictionary[key] = value.Trim();
                }
            }
            else
            {
                dictionary = new Dictionary<string, string>();
                if (!String.IsNullOrWhiteSpace(value))
                {
                    dictionary[key] = value.Trim();
                }
            }
            typeof(DeathRecord).GetProperty(fhirFieldName).SetValue(this.record, dictionary);
        }

        /// <summary>Get a value on the DeathRecord whose property is a geographic type (and is contained in a dictionary).</summary>
        private string Dictionary_Geo_Get(string ijeFieldName, string fhirFieldName, string keyPrefix, string geoType, bool isCoded)
        {
            IJEField info = FieldInfo(ijeFieldName);
            Dictionary<string, string> dictionary = (Dictionary<string, string>)typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record);
            string key = keyPrefix + char.ToUpper(geoType[0]) + geoType.Substring(1);
            string current = Convert.ToString(dictionary[key]);
            if (isCoded)
            {
                if (geoType == "place"|| geoType == "city")
                {
                    string state = null;
                    string county = null;
                    dictionary.TryGetValue(keyPrefix + "State", out state);
                    dictionary.TryGetValue(keyPrefix + "County", out county);
                    if (state != null && county != null)
                    {
                        current = dataLookup.StateNameAndCountyNameAndPlaceNameToPlaceCode(state, county, current);
                    }
                }
                else if (geoType == "county")
                {
                    string state = null;
                    dictionary.TryGetValue(keyPrefix + "State", out state);
                    if (state != null)
                    {
                        current = dataLookup.StateNameAndCountyNameToCountyCode(state, current);
                    }
                }
                else if (geoType == "state")
                {
                    current = dataLookup.StateNameToStateCode(current);
                }
                else if (geoType == "country")
                {
                    current = dataLookup.CountryNameToCountryCode(current);
                }
                else if (geoType == "insideCityLimits")
                {
                    if (String.IsNullOrWhiteSpace(current))
                    {
                        current = "U";
                    }
                    else if (current == "true" || current == "True")
                    {
                        current = "Y";
                    }
                    else if (current == "false" || current == "False")
                    {
                        current = "N";
                    }
                }
            }
            if (current != null)
            {
                return Truncate(current.Replace("-", string.Empty), info.Length).PadRight(info.Length, ' '); // Remove "-" for zip
            }
            else
            {
                return new String(' ', info.Length);
            }
        }

        /// <summary>Set a value on the DeathRecord whose property is a geographic type (and is contained in a dictionary).</summary>
        private void Dictionary_Geo_Set(string ijeFieldName, string fhirFieldName, string keyPrefix, string geoType, bool isCoded, string value)
        {
            IJEField info = FieldInfo(ijeFieldName);
            Dictionary<string, string> dictionary = (Dictionary<string, string>)typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record);
            string key = keyPrefix + char.ToUpper(geoType[0]) + geoType.Substring(1);
            if (dictionary != null && (!dictionary.ContainsKey(key) || String.IsNullOrWhiteSpace(dictionary[key])))
            {
                if (isCoded)
                {
                    if (geoType == "place" || geoType == "city") // This is a tricky case, we need to know about county and state!
                    {
                        string state = null;
                        string county = null;
                        dictionary.TryGetValue(keyPrefix + "State", out state);
                        dictionary.TryGetValue(keyPrefix + "County", out county);
                        if (!String.IsNullOrWhiteSpace(state) && !String.IsNullOrWhiteSpace(county))
                        {
                            string city = dataLookup.StateNameAndCountyNameAndPlaceCodeToPlaceName(state, county, value);
                            if (!String.IsNullOrWhiteSpace(city))
                            {
                                dictionary[key] = city;
                            }
                        }
                    }
                    else if (geoType == "county") // This is a tricky case, we need to know about state!
                    {
                        string state = null;
                        dictionary.TryGetValue(keyPrefix + "State", out state);
                        if (!String.IsNullOrWhiteSpace(state))
                        {
                            string county = dataLookup.StateNameAndCountyCodeToCountyName(state, value);
                            if (!String.IsNullOrWhiteSpace(county))
                            {
                                dictionary[key] = county;
                            }
                        }
                    }
                    else if (geoType == "state")
                    {
                        string state = dataLookup.StateCodeToStateName(value);
                        if (!String.IsNullOrWhiteSpace(state))
                        {
                            dictionary[key] = state;
                        }
                    }
                    else if (geoType == "country")
                    {
                        string country = dataLookup.CountryCodeToCountryName(value);
                        if (!String.IsNullOrWhiteSpace(country))
                        {
                            dictionary[key] = country;
                        }
                    }
                    else if (geoType == "insideCityLimits")
                    {
                        if (!String.IsNullOrWhiteSpace(value) && value == "N")
                        {
                            dictionary[key] = "False";
                        }
                    }
                }
                else
                {
                    dictionary[key] = value.Trim();
                }
            }
            else
            {
                dictionary = new Dictionary<string, string>();
                dictionary[key] = value.Trim();
            }
            typeof(DeathRecord).GetProperty(fhirFieldName).SetValue(this.record, dictionary);
        }

        /// <summary>If the decedent was of hispanic origin, returns a list of ethnicities.</summary>
        private string[] HispanicOrigin()
        {
            Tuple<string, string>[] ethnicityStatus = record.Ethnicity;
            List<string> ethnicities = new List<string>();
            // Check if hispanic origin
            if (Array.Exists(ethnicityStatus, element => element.Item1 == "Hispanic or Latino" || element.Item2 == "2135-2"))
            {
                foreach(Tuple<string, string> tuple in ethnicityStatus)
                {
                    ethnicities.Add(tuple.Item1);
                    ethnicities.Add(tuple.Item2);
                }
            }
            return ethnicities.ToArray();
        }

        /// <summary>If the decedent was of hispanic origin, returns a list of OTHER ethnicities (not Mexican, Cuban, or Puerto Rican).</summary>
        private Tuple<string, string>[] HispanicOriginOther()
        {
            Tuple<string, string>[] ethnicityStatus = record.Ethnicity;
            List<Tuple<string, string>> ethnicities = new List<Tuple<string, string>>();
            // Check if hispanic origin
            if (ethnicityStatus != null && Array.Exists(ethnicityStatus, element => element.Item1 == "Hispanic or Latino" || element.Item2 == "2135-2"))
            {
                foreach(Tuple<string, string> tuple in ethnicityStatus)
                {
                    if (tuple.Item1.ToUpper() != "Hispanic or Latino".ToUpper() &&
                        tuple.Item1.ToUpper() != "Mexican".ToUpper() &&
                        tuple.Item1.ToUpper() != "Puerto Rican".ToUpper() &&
                        tuple.Item1.ToUpper() != "Cuban".ToUpper())
                    {
                        if (tuple.Item2.ToUpper() != "2135-2".ToUpper() &&
                            tuple.Item2.ToUpper() != "2148-5".ToUpper() &&
                            tuple.Item2.ToUpper() != "2180-8".ToUpper() &&
                            tuple.Item2.ToUpper() != "2182-4".ToUpper())
                        {
                            ethnicities.Add(tuple);
                        }
                    }
                }
            }
            return ethnicities.ToArray();
        }

        /// <summary>Checks if the given race exists in the record.</summary>
        private bool Get_Race(string code, string display)
        {
            return Array.Exists(record.Race, element => element.Item1 == display || element.Item2 == code);
        }

        /// <summary>Retrieves American Indian or Alaska Native Race literals on the record.</summary>
        private string[] Get_Race_AIAN_Literals()
        {
            Tuple<string, string>[] literals = record.Race.Select(race => Tuple.Create(race.Item2, race.Item1)).Intersect(dataLookup.CDCRaceAIANCodes).ToArray();
            string[] filterCodes = { "1002-5" };
            return literals.Where(race => !filterCodes.Contains(race.Item1)).Select(race => race.Item2).ToArray();
        }

        /// <summary>Retrieves Asian Race literals (not including ones captured by distinct fields).</summary>
        private string[] Get_Race_A_Literals()
        {
            Tuple<string, string>[] literals = record.Race.Select(race => Tuple.Create(race.Item2, race.Item1)).Intersect(dataLookup.CDCRaceACodes).ToArray();
            string[] filterCodes = { "2028-9", "2039-6", "2040-4", "2047-9", "2036-2", "2034-7", "2029-7" };
            return literals.Where(race => !filterCodes.Contains(race.Item1)).Select(race => race.Item2).ToArray();
        }

        /// <summary>Retrieves Black or African American Race literals on the record.</summary>
        private string[] Get_Race_BAA_Literals()
        {
            Tuple<string, string>[] literals = record.Race.Select(race => Tuple.Create(race.Item2, race.Item1)).Intersect(dataLookup.CDCRaceBAACodes).ToArray();
            string[] filterCodes = { "2054-5" };
            return literals.Where(race => !filterCodes.Contains(race.Item1)).Select(race => race.Item2).ToArray();
        }

        /// <summary>Retrieves Native Hawaiian or Other Pacific Islander Race literals on the record.</summary>
        private string[] Get_Race_NHOPI_Literals()
        {
            Tuple<string, string>[] literals = record.Race.Select(race => Tuple.Create(race.Item2, race.Item1)).Intersect(dataLookup.CDCRaceNHOPICodes).ToArray();
            string[] filterCodes = { "2076-8", "2086-7", "2080-0", "2079-2" };
            return literals.Where(race => !filterCodes.Contains(race.Item1)).Select(race => race.Item2).ToArray();
        }

        /// <summary>Retrieves White Race literals on the record.</summary>
        private string[] Get_Race_W_Literals()
        {
            Tuple<string, string>[] literals = record.Race.Select(race => Tuple.Create(race.Item2, race.Item1)).Intersect(dataLookup.CDCRaceWCodes).ToArray();
            string[] filterCodes = { "2106-3" };
            return literals.Where(race => !filterCodes.Contains(race.Item1)).Select(race => race.Item2).ToArray();
        }

        /// <summary>Retrieves OTHER Race literals on the record.</summary>
        private string[] Get_Race_OTHER_Literals()
        {
            return Get_Race_W_Literals().ToList().Concat(Get_Race_BAA_Literals().ToList()).ToArray();
        }

        /// <summary>Adds the given race to the record.</summary>
        private void Set_Race(string code, string display)
        {
            List<Tuple<string, string>> raceStatus = record.Race.ToList();
            raceStatus.Add(Tuple.Create(display, code));
            record.Race = raceStatus.Distinct().ToList().ToArray();
        }

        /// <summary>Gets a "Yes", "No", or "Unkown" value.</summary>
        private string Get_YNU(string fhirFieldName)
        {
            object status = typeof(DeathRecord).GetProperty(fhirFieldName).GetValue(this.record);
            if (status == null)
            {
                return "U";
            }
            else
            {
                return ((bool)status) ? "Y" : "N";
            }
        }

        /// <summary>Sets a "Yes", "No", or "Unkown" value.</summary>
        private void Set_YNU(string fhirFieldName, string value)
        {
            if (value != "U" && value == "Y")
            {
                typeof(DeathRecord).GetProperty(fhirFieldName).SetValue(this.record, true);
            }
            else if (value != "U" && value == "N")
            {
                typeof(DeathRecord).GetProperty(fhirFieldName).SetValue(this.record, false);
            }
        }


        /////////////////////////////////////////////////////////////////////////////////
        //
        // Class Properties that provide getters and setters for each of the IJE
        // Mortality fields.
        //
        // Getters look at the embedded DeathRecord and convert values to IJE style.
        // Setters convert and store IJE style values to the embedded DeathRecord.
        //
        /////////////////////////////////////////////////////////////////////////////////

        /// <summary>Date of Death--Year</summary>
        [IJEField(1, 1, 4, "Date of Death--Year", "DOD_YR", 1)]
        public string DOD_YR
        {
            get
            {
                return DateTime_Get("DOD_YR", "yyyy", "DateOfDeath") ?? DateTime_Get("DOD_YR", "yyyy", "ActualOrPresumedDateOfDeath");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("DOD_YR", "yyyy", "DateOfDeath", value);
                    DateTime_Set("DOD_YR", "yyyy", "ActualOrPresumedDateOfDeath", value);
                }
            }
        }

        /// <summary>State, U.S. Territory or Canadian Province of Death - code</summary>
        [IJEField(2, 5, 2, "State, U.S. Territory or Canadian Province of Death - code", "DSTATE", 1)]
        public string DSTATE
        {
            get
            {
                return Dictionary_Geo_Get("DSTATE", "PlaceOfDeath", "placeOfDeath", "state", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("DSTATE", "PlaceOfDeath", "placeOfDeath", "state", true, value);
                }
            }
        }

        /// <summary>Certificate Number</summary>
        [IJEField(3, 7, 6, "Certificate Number", "FILENO", 1)]
        public string FILENO
        {
            get
            {
                if (String.IsNullOrWhiteSpace(record.Id))
                {
                    return "".PadLeft(6, '0');
                }
                string id_str = record.Id;
                if (id_str.Length > 6)
                {
                    id_str = id_str.Substring(id_str.Length - 6);
                }
                return id_str.PadLeft(6, '0');
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    RightJustifiedZeroed_Set("FILENO", "Id", value);
                }
            }
        }

        /// <summary>Void flag</summary>
        [IJEField(4, 13, 1, "Void flag", "VOID", 1)]
        public string VOID
        {
            get
            {
                return "0";
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Auxiliary State file number</summary>
        [IJEField(5, 14, 12, "Auxiliary State file number", "AUXNO", 1)]
        public string AUXNO
        {
            get
            {
                return RightJustifiedZeroed_Get("AUXNO", "Id");
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Source flag: paper/electronic</summary>
        [IJEField(6, 26, 1, "Source flag: paper/electronic", "MFILED", 1)]
        public string MFILED
        {
            get
            {
                return "0";
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Decedent's Legal Name--Given</summary>
        [IJEField(7, 27, 50, "Decedent's Legal Name--Given", "GNAME", 1)]
        public string GNAME
        {
            get
            {
                string[] names = record.GivenNames;
                if (names.Length > 0)
                {
                    return names[0];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    record.GivenNames = new string[] { value.Trim() };
                }
            }
        }

        /// <summary>Decedent's Legal Name--Middle</summary>
        [IJEField(8, 77, 1, "Decedent's Legal Name--Middle", "MNAME", 3)]
        public string MNAME
        {
            get
            {
                string[] names = record.GivenNames;
                if (names.Length > 1)
                {
                    return names[1];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (String.IsNullOrWhiteSpace(DMIDDLE))
                    {
                        if (record.GivenNames != null)
                        {
                            List<string> names = record.GivenNames.ToList();
                            names.Add(value.Trim());
                            record.GivenNames = names.ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>Decedent's Legal Name--Last</summary>
        [IJEField(9, 78, 50, "Decedent's Legal Name--Last", "LNAME", 1)]
        public string LNAME
        {
            get
            {
                return LeftJustified_Get("LNAME", "FamilyName");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    LeftJustified_Set("LNAME", "FamilyName", value.Trim());
                }
            }
        }

        /// <summary>Decedent's Legal Name--Suffix</summary>
        [IJEField(10, 128, 10, "Decedent's Legal Name--Suffix", "SUFF", 1)]
        public string SUFF
        {
            get
            {
                return LeftJustified_Get("SUFF", "Suffix");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    LeftJustified_Set("SUFF", "Suffix", value.Trim());
                }
            }
        }

        /// <summary>Decedent's Legal Name--Alias</summary>
        [IJEField(11, 138, 1, "Decedent's Legal Name--Alias", "ALIAS", 1)]
        public string ALIAS
        {
            get
            {
                return "0";
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Father's Surname</summary>
        [IJEField(12, 139, 50, "Father's Surname", "FLNAME", 1)]
        public string FLNAME
        {
            get
            {
                return LeftJustified_Get("FLNAME", "FatherFamilyName");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    LeftJustified_Set("FLNAME", "FatherFamilyName", value.Trim());
                }
            }
        }

        /// <summary>Sex</summary>
        [IJEField(13, 189, 1, "Sex", "SEX", 1)]
        public string SEX
        {
            get
            {
                string gender = LeftJustified_Get("SEX", "Gender");
                if (!String.IsNullOrWhiteSpace(gender))
                {
                    switch(gender.Trim())
                    {
                        case "male":
                        case "Male":
                        case "m":
                        case "M":
                            return "M";
                        case "female":
                        case "Female":
                        case "f":
                        case "F":
                            return "F";
                        case "other":
                        case "Other":
                        case "o":
                        case "O":
                        case "unknown":
                        case "Unknown":
                        case "u":
                        case "U":
                            return "U";
                    }
                }
                return Dictionary_Get("SEX", "BirthSex", "code");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    string code = value == "U" ? "UNK" : value;
                    string display = "";
                    if (code == "M")
                    {
                        display = "Male";
                    }
                    else if (code == "F")
                    {
                        display = "Female";
                    }
                    else if (code == "UNK")
                    {
                        display = "Unknown";
                    }
                    Dictionary_Set("SEX", "BirthSex", "code", code);
                    Dictionary_Set("SEX", "BirthSex", "display", display);
                    Dictionary_Set("SEX", "BirthSex", "system", "http://hl7.org/fhir/us/core/ValueSet/us-core-birthsex");
                    LeftJustified_Set("SEX", "Gender", display);
                }
            }
        }

        /// <summary>Sex--Edit Flag</summary>
        [IJEField(14, 190, 1, "Sex--Edit Flag", "SEX_BYPASS", 1)]
        public string SEX_BYPASS
        {
            get
            {
                return ""; // Blank
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Social Security Number</summary>
        [IJEField(15, 191, 9, "Social Security Number", "SSN", 1)]
        public string SSN
        {
            get
            {
                string ssn = record.SSN;
                if (!String.IsNullOrWhiteSpace(ssn))
                {
                    return ssn.Replace("-", string.Empty);
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    LeftJustified_Set("SSN", "SSN", value);
                }
            }
        }

        /// <summary>Decedent's Age--Type</summary>
        [IJEField(16, 200, 1, "Decedent's Age--Type", "AGETYPE", 1)]
        public string AGETYPE
        {
            get
            {
                if (record.Age != null)
                {
                    return "1"; // Years
                }
                return "";
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Decedent's Age--Units</summary>
        [IJEField(17, 201, 3, "Decedent's Age--Units", "AGE", 1)]
        public string AGE
        {
            get
            {
                if (record.Age != null)
                {
                    return RightJustifiedZeroed_Get("AGE", "Age");
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    RightJustifiedZeroed_Set("AGE", "Age", value);
                }
            }
        }

        /// <summary>Decedent's Age--Edit Flag</summary>
        [IJEField(18, 204, 1, "Decedent's Age--Edit Flag", "AGE_BYPASS", 1)]
        public string AGE_BYPASS
        {
            get
            {
                return ""; // Blank
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Date of Birth--Year</summary>
        [IJEField(19, 205, 4, "Date of Birth--Year", "DOB_YR", 1)]
        public string DOB_YR
        {
            get
            {
                return DateTime_Get("DOB_YR", "yyyy", "DateOfBirth");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("DOB_YR", "yyyy", "DateOfBirth", value);
                }
            }
        }

        /// <summary>Date of Birth--Month</summary>
        [IJEField(20, 209, 2, "Date of Birth--Month", "DOB_MO", 1)]
        public string DOB_MO
        {
            get
            {
                return DateTime_Get("DOB_MO", "MM", "DateOfBirth");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("DOB_MO", "MM", "DateOfBirth", value);
                }
            }
        }

        /// <summary>Date of Birth--Day</summary>
        [IJEField(21, 211, 2, "Date of Birth--Day", "DOB_DY", 1)]
        public string DOB_DY
        {
            get
            {
                return DateTime_Get("DOB_DY", "dd", "DateOfBirth");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("DOB_DY", "dd", "DateOfBirth", value);
                }
            }
        }

        /// <summary>Birthplace--Country</summary>
        [IJEField(22, 213, 2, "Birthplace--Country", "BPLACE_CNT", 1)]
        public string BPLACE_CNT
        {
            get
            {
                return Dictionary_Geo_Get("BPLACE_CNT", "PlaceOfBirth", "placeOfBirth", "country", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("BPLACE_CNT", "PlaceOfBirth", "placeOfBirth", "country", true, value);
                }
            }
        }

        /// <summary>State, U.S. Territory or Canadian Province of Birth - code</summary>
        [IJEField(23, 215, 2, "State, U.S. Territory or Canadian Province of Birth - code", "BPLACE_ST", 1)]
        public string BPLACE_ST
        {
            get
            {
                return Dictionary_Geo_Get("BPLACE_ST", "PlaceOfBirth", "placeOfBirth", "state", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("BPLACE_ST", "PlaceOfBirth", "placeOfBirth", "state", true, value);
                }
            }
        }

        /// <summary>Decedent's Residence--City</summary>
        [IJEField(24, 217, 5, "Decedent's Residence--City", "CITYC", 3)]
        public string CITYC
        {
            get
            {
                return Dictionary_Geo_Get("CITYC", "Residence", "residence", "city", true);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Decedent's Residence--County</summary>
        [IJEField(25, 222, 3, "Decedent's Residence--County", "COUNTYC", 2)]
        public string COUNTYC
        {
            get
            {
                return Dictionary_Geo_Get("COUNTYC", "Residence", "residence", "county", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("COUNTYC", "Residence", "residence", "county", true, value);
                }
            }
        }

        /// <summary>State, U.S. Territory or Canadian Province of Decedent's residence - code</summary>
        [IJEField(26, 225, 2, "State, U.S. Territory or Canadian Province of Decedent's residence - code", "STATEC", 1)]
        public string STATEC
        {
            get
            {
                return Dictionary_Geo_Get("STATEC", "Residence", "residence", "state", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("STATEC", "Residence", "residence", "state", true, value);
                }
            }
        }

        /// <summary>Decedent's Residence--Country</summary>
        [IJEField(27, 227, 2, "Decedent's Residence--Country", "COUNTRYC", 1)]
        public string COUNTRYC
        {
            get
            {
                return Dictionary_Geo_Get("COUNTRYC", "Residence", "residence", "country", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("COUNTRYC", "Residence", "residence", "country", true, value);
                }
            }
        }

        /// <summary>Decedent's Residence--Inside City Limits</summary>
        [IJEField(28, 229, 1, "Decedent's Residence--Inside City Limits", "LIMITS", 1)]
        public string LIMITS
        {
            get
            {
                return Dictionary_Geo_Get("LIMITS", "Residence", "residence", "insideCityLimits", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("LIMITS", "Residence", "residence", "insideCityLimits", true, value);
                }
            }
        }

        /// <summary>Marital Status</summary>
        [IJEField(29, 230, 1, "Marital Status", "MARITAL", 1)]
        public string MARITAL
        {
            get
            {
                string code = Dictionary_Get("MARITAL", "MaritalStatus", "code");
                switch (code)
                {
                    case "M":
                    case "A":
                    case "W":
                    case "D":
                    case "S":
                        return code;
                    case "I":
                    case "L":
                    case "P":
                    case "T":
                    case "U":
                    case "UNK":
                        return "U";
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    switch (value)
                    {
                        case "M":
                            Dictionary_Set("MARITAL", "MaritalStatus", "code", value);
                            Dictionary_Set("MARITAL", "MaritalStatus", "system", "http://hl7.org/fhir/v3/MaritalStatus");
                            Dictionary_Set("MARITAL", "MaritalStatus", "display", "Married");
                            break;
                        case "A":
                            Dictionary_Set("MARITAL", "MaritalStatus", "code", value);
                            Dictionary_Set("MARITAL", "MaritalStatus", "system", "http://hl7.org/fhir/v3/MaritalStatus");
                            Dictionary_Set("MARITAL", "MaritalStatus", "display", "Annulled");
                            break;
                        case "W":
                            Dictionary_Set("MARITAL", "MaritalStatus", "code", value);
                            Dictionary_Set("MARITAL", "MaritalStatus", "system", "http://hl7.org/fhir/v3/MaritalStatus");
                            Dictionary_Set("MARITAL", "MaritalStatus", "display", "Widowed");
                            break;
                        case "D":
                            Dictionary_Set("MARITAL", "MaritalStatus", "code", value);
                            Dictionary_Set("MARITAL", "MaritalStatus", "system", "http://hl7.org/fhir/v3/MaritalStatus");
                            Dictionary_Set("MARITAL", "MaritalStatus", "display", "Divorced");
                            break;
                        case "S":
                            Dictionary_Set("MARITAL", "MaritalStatus", "code", value);
                            Dictionary_Set("MARITAL", "MaritalStatus", "system", "http://hl7.org/fhir/v3/MaritalStatus");
                            Dictionary_Set("MARITAL", "MaritalStatus", "display", "Never Married");
                            break;
                        case "U":
                            Dictionary_Set("MARITAL", "MaritalStatus", "code", "UNK");
                            Dictionary_Set("MARITAL", "MaritalStatus", "system", "http://hl7.org/fhir/v3/NullFlavor");
                            Dictionary_Set("MARITAL", "MaritalStatus", "display", "unknown");
                            break;
                    }
                }
            }
        }

        /// <summary>Marital Status--Edit Flag</summary>
        [IJEField(30, 231, 1, "Marital Status--Edit Flag", "MARITAL_BYPASS", 1)]
        public string MARITAL_BYPASS
        {
            get
            {
                return ""; // Blank
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Place of Death</summary>
        [IJEField(31, 232, 1, "Place of Death", "DPLACE", 1)]
        public string DPLACE
        {
            get
            {
                string code = Dictionary_Get_Full("DPLACE", "PlaceOfDeath", "placeOfDeathTypeCode");
                switch (code)
                {
                    case "63238001": // Dead on arrival at hospital
                        return "3";
                    case "440081000124100": // Death in home
                        return "4";
                    case "440071000124103": // Death in hospice
                        return "5";
                    case "16983000": // Death in hospital
                        return "1";
                    case "450391000124102": // Death in hospital-based emergency department or outpatient department
                        return "2";
                    case "450381000124100": // Death in nursing home or long term care facility
                        return "6";
                    case "UNK": // Unknown
                        return "9";
                    case "OTH": // Other
                        return "7";
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    switch (value)
                    {
                        case "1":
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeCode", "16983000");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeSystem", "http://snomed.info/sct");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeDisplay", "Death in hospital");
                            break;
                        case "2":
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeCode", "450391000124102");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeSystem", "http://snomed.info/sct");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeDisplay", "Death in hospital-based emergency department or outpatient department");
                            break;
                        case "3":
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeCode", "63238001");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeSystem", "http://snomed.info/sct");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeDisplay", "Dead on arrival at hospital");
                            break;
                        case "4":
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeCode", "440081000124100");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeSystem", "http://snomed.info/sct");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeDisplay", "Death in home");
                            break;
                        case "5":
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeCode", "440071000124103");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeSystem", "http://snomed.info/sct");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeDisplay", "Death in hospice");
                            break;
                        case "6":
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeCode", "450381000124100");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeSystem", "http://snomed.info/sct");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeDisplay", "Death in nursing home or long term care facility");
                            break;
                        case "7":
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeCode", "UNK");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeSystem", "http://hl7.org/fhir/v3/NullFlavor");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeDisplay", "Unknown");
                            break;
                        case "9":
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeCode", "OTH");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "system", "http://hl7.org/fhir/v3/NullFlavor");
                            Dictionary_Set("DPLACE", "PlaceOfDeath", "placeOfDeathTypeDisplay", "Other");
                            break;
                    }
                }
            }
        }

        /// <summary>County of Death Occurrence</summary>
        [IJEField(32, 233, 3, "County of Death Occurrence", "COD", 2)]
        public string COD
        {
            get
            {
                return Dictionary_Geo_Get("COD", "PlaceOfDeath", "placeOfDeath", "county", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("COD", "PlaceOfDeath", "placeOfDeath", "county", true, value);
                }
            }
        }

        /// <summary>Method of Disposition</summary>
        [IJEField(33, 236, 1, "Method of Disposition", "DISP", 1)]
        public string DISP
        {
            get
            {
                string code = Dictionary_Get_Full("DISP", "Disposition", "dispositionTypeCode");
                switch (code)
                {
                    case "449951000124101": // Donation
                        return "D";
                    case "449971000124106": // Burial
                        return "B";
                    case "449961000124104": // Cremation
                        return "C";
                    case "449931000124108": // Entombment
                        return "E";
                    case "449941000124103": // Removal from state
                        return "R";
                    case "UNK": // Unknown
                        return "U";
                    case "455401000124109": // Hospital Disposition
                    case "OTH": // Other
                        return "O";
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    switch (value)
                    {
                        case "D":
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeCode", "449951000124101");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeSystem", "http://snomed.info/sct");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeDisplay", "Donation");
                            break;
                        case "B":
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeCode", "449971000124106");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeSystem", "http://snomed.info/sct");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeDisplay", "Burial");
                            break;
                        case "C":
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeCode", "449961000124104");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeSystem", "http://snomed.info/sct");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeDisplay", "Cremation");
                            break;
                        case "E":
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeCode", "449931000124108");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeSystem", "http://snomed.info/sct");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeDisplay", "Entombment");
                            break;
                        case "R":
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeCode", "449941000124103");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeSystem", "http://snomed.info/sct");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeDisplay", "Removal from state");
                            break;
                        case "U":
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeCode", "UNK");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeSystem", "http://hl7.org/fhir/v3/NullFlavor");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeDisplay", "Unknown");
                            break;
                        case "O":
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeCode", "OTH");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeSystem", "http://hl7.org/fhir/v3/NullFlavor");
                            Dictionary_Set("DISP", "Disposition", "dispositionTypeDisplay", "Other");
                            break;
                    }
                }
            }
        }

        /// <summary>Date of Death--Month</summary>
        [IJEField(34, 237, 2, "Date of Death--Month", "DOD_MO", 1)]
        public string DOD_MO
        {
            get
            {
                return DateTime_Get("DOD_MO", "MM", "DateOfDeath") ?? DateTime_Get("DOD_MO", "MM", "ActualOrPresumedDateOfDeath");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("DOD_MO", "MM", "DateOfDeath", value);
                    DateTime_Set("DOD_MO", "MM", "ActualOrPresumedDateOfDeath", value);
                }
            }
        }

        /// <summary>Date of Death--Day</summary>
        [IJEField(35, 239, 2, "Date of Death--Day", "DOD_DY", 1)]
        public string DOD_DY
        {
            get
            {
                return DateTime_Get("DOD_DY", "dd", "DateOfDeath") ?? DateTime_Get("DOD_DY", "dd", "ActualOrPresumedDateOfDeath");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("DOD_DY", "dd", "DateOfDeath", value);
                    DateTime_Set("DOD_DY", "dd", "ActualOrPresumedDateOfDeath", value);
                }
            }
        }

        /// <summary>Time of Death</summary>
        [IJEField(36, 241, 4, "Time of Death", "TOD", 1)]
        public string TOD
        {
            get
            {
                return DateTime_Get("TOD", "HHmm", "ActualOrPresumedDateOfDeath") ?? DateTime_Get("TOD", "HHmm", "DateOfDeath");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("TOD", "HHmm", "DateOfDeath", value);
                    DateTime_Set("TOD", "HHmm", "ActualOrPresumedDateOfDeath", value);
                }
            }
        }

        /// <summary>Decedent's Education</summary>
        [IJEField(37, 245, 1, "Decedent's Education", "DEDUC", 1)]
        public string DEDUC
        {
            get
            {
                string code = Dictionary_Get_Full("DEDUC", "Education", "code");
                switch (code)
                {
                    case "PHC1448": // 8th grade or less
                        return "1";
                    case "PHC1449": // 9th through 12th grade; no diploma
                        return "2";
                    case "PHC1450": // High School Graduate or GED Completed
                        return "3";
                    case "PHC1451": // Some college credit, but no degree
                        return "4";
                    case "PHC1452": // Associate Degree
                        return "5";
                    case "PHC1453": // Bachelor's Degree
                        return "6";
                    case "PHC1454": // Master's Degree
                        return "7";
                    case "PHC1455": // Doctorate Degree or Professional Degree
                        return "8";
                    case "UNK": // Unknown
                        return "9";
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    switch (value)
                    {
                        case "1":
                            Dictionary_Set("DEDUC", "Education", "code", "PHC1448");
                            Dictionary_Set("DEDUC", "Education", "system", "http://github.com/nightingaleproject/fhirDeathRecord/sdr/decedent/cs/EducationCS");
                            Dictionary_Set("DEDUC", "Education", "display", "8th grade or less");
                            break;
                        case "2":
                            Dictionary_Set("DEDUC", "Education", "code", "PHC1449");
                            Dictionary_Set("DEDUC", "Education", "system", "http://github.com/nightingaleproject/fhirDeathRecord/sdr/decedent/cs/EducationCS");
                            Dictionary_Set("DEDUC", "Education", "display", "9th through 12th grade; no diploma");
                            break;
                        case "3":
                            Dictionary_Set("DEDUC", "Education", "code", "PHC1450");
                            Dictionary_Set("DEDUC", "Education", "system", "http://github.com/nightingaleproject/fhirDeathRecord/sdr/decedent/cs/EducationCS");
                            Dictionary_Set("DEDUC", "Education", "display", "High School Graduate or GED Completed");
                            break;
                        case "4":
                            Dictionary_Set("DEDUC", "Education", "code", "PHC1451");
                            Dictionary_Set("DEDUC", "Education", "system", "http://github.com/nightingaleproject/fhirDeathRecord/sdr/decedent/cs/EducationCS");
                            Dictionary_Set("DEDUC", "Education", "display", "Some college credit, but no degree");
                            break;
                        case "5":
                            Dictionary_Set("DEDUC", "Education", "code", "PHC1452");
                            Dictionary_Set("DEDUC", "Education", "system", "http://github.com/nightingaleproject/fhirDeathRecord/sdr/decedent/cs/EducationCS");
                            Dictionary_Set("DEDUC", "Education", "display", "Associate Degree");
                            break;
                        case "6":
                            Dictionary_Set("DEDUC", "Education", "code", "PHC1453");
                            Dictionary_Set("DEDUC", "Education", "system", "http://github.com/nightingaleproject/fhirDeathRecord/sdr/decedent/cs/EducationCS");
                            Dictionary_Set("DEDUC", "Education", "display", "Bachelor's Degree");
                            break;
                        case "7":
                            Dictionary_Set("DEDUC", "Education", "code", "PHC1454");
                            Dictionary_Set("DEDUC", "Education", "system", "http://github.com/nightingaleproject/fhirDeathRecord/sdr/decedent/cs/EducationCS");
                            Dictionary_Set("DEDUC", "Education", "display", "Master's Degree");
                            break;
                        case "8":
                            Dictionary_Set("DEDUC", "Education", "code", "PHC1455");
                            Dictionary_Set("DEDUC", "Education", "system", "http://github.com/nightingaleproject/fhirDeathRecord/sdr/decedent/cs/EducationCS");
                            Dictionary_Set("DEDUC", "Education", "display", "Doctorate Degree or Professional Degree");
                            break;
                        case "9":
                            Dictionary_Set("DEDUC", "Education", "code", "UNK");
                            Dictionary_Set("DEDUC", "Education", "system", "http://hl7.org/fhir/v3/NullFlavor");
                            Dictionary_Set("DEDUC", "Education", "display", "Unknown");
                            break;
                    }
                }
            }
        }

        /// <summary>Decedent's Education--Edit Flag</summary>
        [IJEField(38, 246, 1, "Decedent's Education--Edit Flag", "DEDUC_BYPASS", 1)]
        public string DEDUC_BYPASS
        {
            get
            {
                return ""; // Blank
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Decedent of Hispanic Origin?--Mexican</summary>
        [IJEField(39, 247, 1, "Decedent of Hispanic Origin?--Mexican", "DETHNIC1", 1)]
        public string DETHNIC1
        {
            get
            {
                string[] ethnicities = HispanicOrigin();
                if (ethnicities.Length == 0)
                {
                    return "U";
                }
                if (Array.Exists(ethnicities, element => element == "Mexican" || element == "2148-5"))
                {
                    return "H";
                }
                return "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string>> ethnicities = record.Ethnicity.ToList();
                    if (value == "H")
                    {
                        ethnicities.Add(Tuple.Create("Mexican", "2148-5"));
                        ethnicities.Add(Tuple.Create("Hispanic or Latino", "2135-2"));
                        ethnicities.RemoveAll(x => x.Item1 == "Non Hispanic or Latino" || x.Item2 == "2186-5");
                        record.Ethnicity = ethnicities.Distinct().ToList().ToArray();
                    }
                    else if (ethnicities.Count == 0)
                    {
                        ethnicities.Add(Tuple.Create("Non Hispanic or Latino", "2186-5"));
                        record.Ethnicity = ethnicities.Distinct().ToList().ToArray();
                    }
                }
            }
        }

        /// <summary>Decedent of Hispanic Origin?--Puerto Rican</summary>
        [IJEField(40, 248, 1, "Decedent of Hispanic Origin?--Puerto Rican", "DETHNIC2", 1)]
        public string DETHNIC2
        {
            get
            {
                string[] ethnicities = HispanicOrigin();
                if (ethnicities.Length == 0)
                {
                    return "U";
                }
                if (Array.Exists(ethnicities, element => element == "Puerto Rican" || element == "2180-8"))
                {
                    return "H";
                }
                return "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string>> ethnicities = record.Ethnicity.ToList();
                    if (value == "H")
                    {
                        ethnicities.Add(Tuple.Create("Puerto Rican", "2180-8"));
                        ethnicities.Add(Tuple.Create("Hispanic or Latino", "2135-2"));
                        ethnicities.RemoveAll(x => x.Item1 == "Non Hispanic or Latino" || x.Item2 == "2186-5");
                        record.Ethnicity = ethnicities.Distinct().ToList().ToArray();
                    }
                    else if (ethnicities.Count == 0)
                    {
                        ethnicities.Add(Tuple.Create("Non Hispanic or Latino", "2186-5"));
                        record.Ethnicity = ethnicities.Distinct().ToList().ToArray();
                    }
                }
            }
        }

        /// <summary>Decedent of Hispanic Origin?--Cuban</summary>
        [IJEField(41, 249, 1, "Decedent of Hispanic Origin?--Cuban", "DETHNIC3", 1)]
        public string DETHNIC3
        {
            get
            {
                string[] ethnicities = HispanicOrigin();
                if (ethnicities.Length == 0)
                {
                    return "U";
                }
                if (Array.Exists(ethnicities, element => element == "Cuban" || element == "2182-4"))
                {
                    return "H";
                }
                return "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string>> ethnicities = record.Ethnicity.ToList();
                    if (value == "H")
                    {
                        ethnicities.Add(Tuple.Create("Cuban", "2182-4"));
                        ethnicities.Add(Tuple.Create("Hispanic or Latino", "2135-2"));
                        ethnicities.RemoveAll(x => x.Item1 == "Non Hispanic or Latino" || x.Item2 == "2186-5");
                        record.Ethnicity = ethnicities.Distinct().ToList().ToArray();
                    }
                    else if (ethnicities.Count == 0)
                    {
                        ethnicities.Add(Tuple.Create("Non Hispanic or Latino", "2186-5"));
                        record.Ethnicity = ethnicities.Distinct().ToList().ToArray();
                    }
                }
            }
        }

        /// <summary>Decedent of Hispanic Origin?--Other</summary>
        [IJEField(42, 250, 1, "Decedent of Hispanic Origin?--Other", "DETHNIC4", 1)]
        public string DETHNIC4
        {
            get
            {
                string[] hispanicOrigin = HispanicOrigin();
                if (hispanicOrigin != null && hispanicOrigin.Length == 0)
                {
                    return "U";
                }
                // We need to handle cases where hispanic origin is other with or without write-in
                else if (hispanicOrigin != null && hispanicOrigin.Length > 0 && DETHNIC1 == "N" && DETHNIC2 == "N" && DETHNIC3 == "N")
                {
                    return "H";
                }
                else
                {
                    return "N";
                }
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string>> ethnicities = record.Ethnicity.ToList();
                    if (value == "H")
                    {
                        ethnicities.Add(Tuple.Create("Hispanic or Latino", "2135-2"));
                        ethnicities.RemoveAll(x => x.Item1 == "Non Hispanic or Latino" || x.Item2 == "2186-5");
                        record.Ethnicity = ethnicities.Distinct().ToList().ToArray();
                    }
                    else if (ethnicities.Count == 0)
                    {
                        ethnicities.Add(Tuple.Create("Non Hispanic or Latino", "2186-5"));
                        record.Ethnicity = ethnicities.Distinct().ToList().ToArray();
                    }
                }
            }
        }

        /// <summary>Decedent of Hispanic Origin?--Other, Literal</summary>
        [IJEField(43, 251, 20, "Decedent of Hispanic Origin?--Other, Literal", "DETHNIC5", 1)]
        public string DETHNIC5
        {
            get
            {
                Tuple<string, string> other = HispanicOriginOther().FirstOrDefault();
                if (other != null)
                {
                    return other.Item1;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string>> ethnicities = record.Ethnicity.ToList();
                    // Try to find a matching code for the literal
                    string codeMatch = dataLookup.EthnicityNameToEthnicityCode(value.Trim());
                    ethnicities.Add(Tuple.Create(value.Trim(), codeMatch));
                    ethnicities.Add(Tuple.Create("Hispanic or Latino", "2135-2"));
                    ethnicities.RemoveAll(x => x.Item1 == "Non Hispanic or Latino" || x.Item2 == "2186-5");
                    record.Ethnicity = ethnicities.Distinct().ToList().ToArray();
                }
            }
        }

        /// <summary>Decedent's Race--White</summary>
        [IJEField(44, 271, 1, "Decedent's Race--White", "RACE1", 1)]
        public string RACE1
        {
            get
            {
                return Get_Race("2106-3", "White") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("2106-3", "White");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Black or African American</summary>
        [IJEField(45, 272, 1, "Decedent's Race--Black or African American", "RACE2", 1)]
        public string RACE2
        {
            get
            {
                return Get_Race("2054-5", "Black or African American") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("2054-5", "Black or African American");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--American Indian or Alaska Native</summary>
        [IJEField(46, 273, 1, "Decedent's Race--American Indian or Alaska Native", "RACE3", 1)]
        public string RACE3
        {
            get
            {
                return Get_Race("1002-5", "American Indian or Alaska Native") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("1002-5", "American Indian or Alaska Native");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Asian Indian</summary>
        [IJEField(47, 274, 1, "Decedent's Race--Asian Indian", "RACE4", 1)]
        public string RACE4
        {
            get
            {
                return Get_Race("2029-7", "Asian Indian") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("2029-7", "Asian Indian");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Chinese</summary>
        [IJEField(48, 275, 1, "Decedent's Race--Chinese", "RACE5", 1)]
        public string RACE5
        {
            get
            {
                return Get_Race("2034-7", "Chinese") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("2034-7", "Chinese");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Filipino</summary>
        [IJEField(49, 276, 1, "Decedent's Race--Filipino", "RACE6", 1)]
        public string RACE6
        {
            get
            {
                return Get_Race("2036-2", "Filipino") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("2036-2", "Filipino");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Japanese</summary>
        [IJEField(50, 277, 1, "Decedent's Race--Japanese", "RACE7", 1)]
        public string RACE7
        {
            get
            {
                return Get_Race("2039-6", "Japanese") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("2039-6", "Japanese");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Korean</summary>
        [IJEField(51, 278, 1, "Decedent's Race--Korean", "RACE8", 1)]
        public string RACE8
        {
            get
            {
                return Get_Race("2040-4", "Korean") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("2040-4", "Korean");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Vietnamese</summary>
        [IJEField(52, 279, 1, "Decedent's Race--Vietnamese", "RACE9", 1)]
        public string RACE9
        {
            get
            {
                return Get_Race("2047-9", "Vietnamese") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("2047-9", "Vietnamese");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Other Asian</summary>
        [IJEField(53, 280, 1, "Decedent's Race--Other Asian", "RACE10", 1)]
        public string RACE10
        {
            get
            {
                return Get_Race_A_Literals().Length > 0 ? "Y" : "N";
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Decedent's Race--Native Hawaiian</summary>
        [IJEField(54, 281, 1, "Decedent's Race--Native Hawaiian", "RACE11", 1)]
        public string RACE11
        {
            get
            {
                return Get_Race("2079-2", "Native Hawaiian") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("2079-2", "Native Hawaiian");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Guamanian or Chamorro</summary>
        [IJEField(55, 282, 1, "Decedent's Race--Guamanian or Chamorro", "RACE12", 1)]
        public string RACE12
        {
            get
            {
                return Get_Race("2086-7", "Guamanian or Chamorro") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("2086-7", "Guamanian or Chamorro");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Samoan</summary>
        [IJEField(56, 283, 1, "Decedent's Race--Samoan", "RACE13", 1)]
        public string RACE13
        {
            get
            {
                return Get_Race("2080-0", "Samoan") ? "Y" : "N";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (value == "Y")
                    {
                        Set_Race("2080-0", "Samoan");
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Other Pacific Islander</summary>
        [IJEField(57, 284, 1, "Decedent's Race--Other Pacific Islander", "RACE14", 1)]
        public string RACE14
        {
            get
            {
                return Get_Race_NHOPI_Literals().Length > 0 ? "Y" : "N";
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Decedent's Race--Other</summary>
        [IJEField(58, 285, 1, "Decedent's Race--Other", "RACE15", 1)]
        public string RACE15
        {
            get
            {
                return Get_Race_OTHER_Literals().Length > 0 ? "Y" : "N";
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Decedent's Race--First American Indian or Alaska Native Literal</summary>
        [IJEField(59, 286, 30, "Decedent's Race--First American Indian or Alaska Native Literal", "RACE16", 1)]
        public string RACE16
        {
            get
            {
                string[] literals = Get_Race_AIAN_Literals();
                if (literals.Length > 0)
                {
                    return literals[0];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    string name = value.Trim();
                    string code = dataLookup.AIANRaceNameToRaceCode(name);
                    if (!String.IsNullOrWhiteSpace(code) && !String.IsNullOrWhiteSpace(name))
                    {
                        Set_Race(code, name);
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Second American Indian or Alaska Native Literal</summary>
        [IJEField(60, 316, 30, "Decedent's Race--Second American Indian or Alaska Native Literal", "RACE17", 1)]
        public string RACE17
        {
            get
            {
                string[] literals = Get_Race_AIAN_Literals();
                if (literals.Length > 1)
                {
                    return literals[1];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    string name = value.Trim();
                    string code = dataLookup.AIANRaceNameToRaceCode(name);
                    if (!String.IsNullOrWhiteSpace(code) && !String.IsNullOrWhiteSpace(name))
                    {
                        Set_Race(code, name);
                    }
                }
            }
        }

        /// <summary>Decedent's Race--First Other Asian Literal</summary>
        [IJEField(61, 346, 30, "Decedent's Race--First Other Asian Literal", "RACE18", 1)]
        public string RACE18
        {
            get
            {
                string[] literals = Get_Race_A_Literals();
                if (literals.Length > 0)
                {
                    return literals[0];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    string name = value.Trim();
                    string code = dataLookup.ARaceNameToRaceCode(name);
                    if (!String.IsNullOrWhiteSpace(code) && !String.IsNullOrWhiteSpace(name))
                    {
                        Set_Race(code, name);
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Second Other Asian Literal</summary>
        [IJEField(62, 376, 30, "Decedent's Race--Second Other Asian Literal", "RACE19", 1)]
        public string RACE19
        {
            get
            {
                string[] literals = Get_Race_A_Literals();
                if (literals.Length > 1)
                {
                    return literals[1];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    string name = value.Trim();
                    string code = dataLookup.ARaceNameToRaceCode(name);
                    if (!String.IsNullOrWhiteSpace(code) && !String.IsNullOrWhiteSpace(name))
                    {
                        Set_Race(code, name);
                    }
                }
            }
        }

        /// <summary>Decedent's Race--First Other Pacific Islander Literal</summary>
        [IJEField(63, 406, 30, "Decedent's Race--First Other Pacific Islander Literal", "RACE20", 1)]
        public string RACE20
        {
            get
            {
                string[] literals = Get_Race_NHOPI_Literals();
                if (literals.Length > 0)
                {
                    return literals[0];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    string name = value.Trim();
                    string code = dataLookup.NHOPIRaceNameToRaceCode(name);
                    if (!String.IsNullOrWhiteSpace(code) && !String.IsNullOrWhiteSpace(name))
                    {
                        Set_Race(code, name);
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Second Other Pacific Islander Literalr</summary>
        [IJEField(64, 436, 30, "Decedent's Race--Second Other Pacific Islander Literal", "RACE21", 1)]
        public string RACE21
        {
            get
            {
                string[] literals = Get_Race_NHOPI_Literals();
                if (literals.Length > 1)
                {
                    return literals[1];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    string name = value.Trim();
                    string code = dataLookup.NHOPIRaceNameToRaceCode(name);
                    if (!String.IsNullOrWhiteSpace(code) && !String.IsNullOrWhiteSpace(name))
                    {
                        Set_Race(code, name);
                    }
                }
            }
        }

        /// <summary>Decedent's Race--First Other Literal</summary>
        [IJEField(65, 466, 30, "Decedent's Race--First Other Literal", "RACE22", 1)]
        public string RACE22
        {
            get
            {
                string[] literals = Get_Race_OTHER_Literals();
                if (literals.Length > 0)
                {
                    return literals[0];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    string name = value.Trim();
                    string code = dataLookup.WRaceNameToRaceCode(name);
                    if (String.IsNullOrWhiteSpace(code))
                    {
                        code = dataLookup.BAARaceNameToRaceCode(name);
                    }
                    if (!String.IsNullOrWhiteSpace(code) && !String.IsNullOrWhiteSpace(name))
                    {
                        Set_Race(code, name);
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Second Other Literal</summary>
        [IJEField(66, 496, 30, "Decedent's Race--Second Other Literal", "RACE23", 1)]
        public string RACE23
        {
            get
            {
                string[] literals = Get_Race_OTHER_Literals();
                if (literals.Length > 1)
                {
                    return literals[1];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    string name = value.Trim();
                    string code = dataLookup.WRaceNameToRaceCode(name);
                    if (String.IsNullOrWhiteSpace(code))
                    {
                        code = dataLookup.BAARaceNameToRaceCode(name);
                    }
                    if (!String.IsNullOrWhiteSpace(code) && !String.IsNullOrWhiteSpace(name))
                    {
                        Set_Race(code, name);
                    }
                }
            }
        }

        /// <summary>Decedent's Race--Missing</summary>
        [IJEField(83, 574, 1, "Decedent's Race--Missing", "RACE_MVR", 1)]
        public string RACE_MVR
        {
            get
            {
                return ""; // Blank
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Occupation -- Literal (OPTIONAL)</summary>
        [IJEField(84, 575, 40, "Occupation -- Literal (OPTIONAL)", "OCCUP", 1)]
        public string OCCUP
        {
            get
            {
                return Dictionary_Get("OCCUP", "Occupation", "jobDescription");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("OCCUP", "Occupation", "jobDescription", value);
                }
            }
        }

        /// <summary>Industry -- Literal (OPTIONAL)</summary>
        [IJEField(86, 618, 40, "Industry -- Literal (OPTIONAL)", "INDUST", 1)]
        public string INDUST
        {
            get
            {
                return Dictionary_Get("INDUST", "Occupation", "industryDescription");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("INDUST", "Occupation", "industryDescription", value);
                }
            }
        }

        /// <summary>Infant Death/Birth Linking - birth certificate number</summary>
        [IJEField(88, 661, 6, "Infant Death/Birth Linking - birth certificate number", "BCNO", 1)]
        public string BCNO
        {
            get
            {
                return "";
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Infant Death/Birth Linking - year of birth</summary>
        [IJEField(89, 667, 4, "Infant Death/Birth Linking - year of birth", "IDOB_YR", 1)]
        public string IDOB_YR
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(BCNO))
                {
                    string dob = DateTime_Get("IDOB_YR", "yyyy", "DateOfBirth");
                    if (String.IsNullOrWhiteSpace(dob))
                    {
                        return "9999"; // Unknown
                    }
                    else
                    {
                        return dob;
                    }
                }
                return ""; // Blank
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Infant Death/Birth Linking - year of birth</summary>
        [IJEField(90, 671, 2, "Infant Death/Birth Linking - State, U.S. Territory or Canadian Province of Birth - code", "BSTATE", 1)]
        public string BSTATE
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(BCNO))
                {
                    return Dictionary_Geo_Get("BSTATE", "PlaceOfBirth", "placeOfBirth", "state", true);
                }
                return ""; // Blank
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Date of Registration--Year</summary>
        [IJEField(95, 689, 4, "Date of Registration--Year", "DOR_YR", 1)]
        public string DOR_YR
        {
            get
            {
                return DateTime_Get("DOR_YR", "yyyy", "DateOfRegistration");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("DOR_YR", "yyyy", "DateOfRegistration", value);
                }
            }
        }

        /// <summary>Date of Registration--Month</summary>
        [IJEField(96, 693, 2, "Date of Registration--Month", "DOR_MO", 1)]
        public string DOR_MO
        {
            get
            {
                return DateTime_Get("DOR_MO", "MM", "DateOfRegistration");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("DOR_MO", "MM", "DateOfRegistration", value);
                }
            }
        }

        /// <summary>Date of Registration--Day</summary>
        [IJEField(97, 695, 2, "Date of Registration--Day", "DOR_DY", 1)]
        public string DOR_DY
        {
            get
            {
                return DateTime_Get("DOR_DY", "dd", "DateOfRegistration");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("DOR_DY", "dd", "DateOfRegistration", value);
                }
            }
        }

        /// <summary>Manner of Death</summary>
        [IJEField(99, 701, 1, "Manner of Death", "MANNER", 1)]
        public string MANNER
        {
            get
            {
                string code = Dictionary_Get_Full("MANNER", "MannerOfDeath", "code");
                switch (code)
                {
                    case "38605008": // Natural
                        return "N";
                    case "7878000": // Accident
                        return "A";
                    case "44301001": // Suicide
                        return "S";
                    case "27935005": // Homicide
                        return "H";
                    case "185973002": // Pending Investigation
                        return "P";
                    case "65037004": // Could not be determined
                        return "C";
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    switch (value)
                    {
                        case "N":
                            Dictionary_Set("MANNER", "MannerOfDeath", "code", "38605008");
                            Dictionary_Set("MANNER", "MannerOfDeath", "system", "http://snomed.info/sct");
                            Dictionary_Set("MANNER", "MannerOfDeath", "display", "Natural");
                            break;
                        case "A":
                            Dictionary_Set("MANNER", "MannerOfDeath", "code", "7878000");
                            Dictionary_Set("MANNER", "MannerOfDeath", "system", "http://snomed.info/sct");
                            Dictionary_Set("MANNER", "MannerOfDeath", "display", "Accident");
                            break;
                        case "S":
                            Dictionary_Set("MANNER", "MannerOfDeath", "code", "44301001");
                            Dictionary_Set("MANNER", "MannerOfDeath", "system", "http://snomed.info/sct");
                            Dictionary_Set("MANNER", "MannerOfDeath", "display", "Suicide");
                            break;
                        case "H":
                            Dictionary_Set("MANNER", "MannerOfDeath", "code", "27935005");
                            Dictionary_Set("MANNER", "MannerOfDeath", "system", "http://snomed.info/sct");
                            Dictionary_Set("MANNER", "MannerOfDeath", "display", "Homicide");
                            break;
                        case "P":
                            Dictionary_Set("MANNER", "MannerOfDeath", "code", "185973002");
                            Dictionary_Set("MANNER", "MannerOfDeath", "system", "http://snomed.info/sct");
                            Dictionary_Set("MANNER", "MannerOfDeath", "display", "Pending Investigation");
                            break;
                        case "C":
                            Dictionary_Set("MANNER", "MannerOfDeath", "code", "65037004");
                            Dictionary_Set("MANNER", "MannerOfDeath", "system", "http://snomed.info/sct");
                            Dictionary_Set("MANNER", "MannerOfDeath", "display", "Could not be determined");
                            break;
                    }
                }
            }
        }

        /// <summary>Place of Injury (computer generated)</summary>
        [IJEField(102, 704, 1, "Place of Injury (computer generated)", "INJPL", 1)]
        public string INJPL
        {
            get
            {
                // IJE options below, default to Blank.
                // A Home
                // B Farm
                // C Residential Institution
                // D Military Residence
                // E Hospital
                // F School, Other Institutions, Administrative Area
                // G Industrial and Construction
                // H Garage/Warehouse
                // I Trade and Service Area
                // J Mine/Quarry
                // K Street/Highway
                // L Public Recreation Area
                // M Institutional Recreation Area
                // N Sports and Recreation Area
                // O Other building
                // P Other specified Place
                // Q Unspecified Place
                // Blank
                return "";
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Was Autopsy performed</summary>
        [IJEField(108, 976, 1, "Was Autopsy performed", "AUTOP", 1)]
        public string AUTOP
        {
            get
            {
                return Get_YNU("AutopsyPerformed");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Set_YNU("AutopsyPerformed", value);
                }
            }
        }

        /// <summary>Were Autopsy Findings Available to Complete the Cause of Death?</summary>
        [IJEField(109, 977, 1, "Were Autopsy Findings Available to Complete the Cause of Death?", "AUTOPF", 1)]
        public string AUTOPF
        {
            get
            {
                return Get_YNU("AutopsyResultsAvailable");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Set_YNU("AutopsyResultsAvailable", value);
                }
            }
        }

        /// <summary>Did Tobacco Use Contribute to Death?</summary>
        [IJEField(110, 978, 1, "Did Tobacco Use Contribute to Death?", "TOBAC", 1)]
        public string TOBAC
        {
            get
            {
                string code = Dictionary_Get_Full("TOBAC", "TobaccoUseContributedToDeath", "code");
                switch (code)
                {
                    case "373066001": // Yes
                        return "Y";
                    case "373067005": // No
                        return "N";
                    case "2931005": // Probably
                        return "P";
                    case "UNK": // Unknown
                        return "U";
                    case "NASK": // Pending Investigation
                        return "C";
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    switch (value)
                    {
                        case "Y":
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "code", "373066001");
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "system", "http://snomed.info/sct");
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "display", "Yes");
                            break;
                        case "N":
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "code", "373067005");
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "system", "http://snomed.info/sct");
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "display", "No");
                            break;
                        case "P":
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "code", "2931005");
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "system", "http://snomed.info/sct");
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "display", "Probably");
                            break;
                        case "U":
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "code", "UNK");
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "system", "http://hl7.org/fhir/v3/NullFlavor");
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "display", "Unknown");
                            break;
                        case "C":
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "code", "NASK");
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "system", "http://hl7.org/fhir/v3/NullFlavor");
                            Dictionary_Set("TOBAC", "TobaccoUseContributedToDeath", "display", "Not asked");
                            break;
                    }
                }
            }
        }

        /// <summary>Pregnancy</summary>
        [IJEField(111, 979, 1, "Pregnancy", "PREG", 1)]
        public string PREG
        {
            get
            {
                string code = Dictionary_Get_Full("PREG", "TimingOfRecentPregnancyInRelationToDeath", "code");
                switch (code)
                {
                    case "PHC1260": // Not pregnant within past year
                        return "1";
                    case "PHC1261": // Pregnant at time of death
                        return "2";
                    case "PHC1262": // Not pregnant, but pregnant within 42 days of death
                        return "3";
                    case "PHC1263": // Not pregnant, but pregnant 43 days to 1 year before death
                        return "4";
                    case "PHC1264": // Unknown if pregnant within the past year
                        return "9";
                    case "NA": // Not applicable
                        return "8";
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    switch (value)
                    {
                        case "1":
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "code", "PHC1260");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "system", "urn:oid:2.16.840.1.114222.4.11.6003");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "display", "Not pregnant within past year");
                            break;
                        case "2":
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "code", "PHC1261");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "system", "urn:oid:2.16.840.1.114222.4.11.6003");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "display", "Pregnant at time of death");
                            break;
                        case "3":
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "code", "PHC1262");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "system", "urn:oid:2.16.840.1.114222.4.11.6003");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "display", "Not pregnant, but pregnant within 42 days of death");
                            break;
                        case "4":
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "code", "PHC1263");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "system", "urn:oid:2.16.840.1.114222.4.11.6003");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "display", "Not pregnant, but pregnant 43 days to 1 year before death");
                            break;
                        case "9":
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "code", "PHC1264");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "system", "urn:oid:2.16.840.1.114222.4.11.6003");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "display", "Unknown if pregnant within the past year");
                            break;
                        case "8":
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "code", "NA");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "system", "http://hl7.org/fhir/v3/NullFlavor");
                            Dictionary_Set("PREG", "TimingOfRecentPregnancyInRelationToDeath", "display", "Not applicable");
                            break;
                    }
                }
            }
        }

        /// <summary>If Female--Edit Flag: From EDR only</summary>
        [IJEField(112, 980, 1, "If Female--Edit Flag: From EDR only", "PREG_BYPASS", 1)]
        public string PREG_BYPASS
        {
            get
            {
                return ""; // Blank
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Date of injury--month</summary>
        [IJEField(113, 981, 2, "Date of injury--month", "DOI_MO", 1)]
        public string DOI_MO
        {
            get
            {
                return DateTime_Dictionary_Get("DOI_MO", "MM", "DetailsOfInjury", "detailsOfInjuryEffectiveDateTime");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Dictionary_Set("DOI_MO", "MM", "DetailsOfInjury", "detailsOfInjuryEffectiveDateTime", value.Trim());
                }
            }
        }

        /// <summary>Date of injury--day</summary>
        [IJEField(114, 983, 2, "Date of injury--day", "DOI_DY", 1)]
        public string DOI_DY
        {
            get
            {
                return DateTime_Dictionary_Get("DOI_DY", "dd", "DetailsOfInjury", "detailsOfInjuryEffectiveDateTime");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Dictionary_Set("DOI_DY", "dd", "DetailsOfInjury", "detailsOfInjuryEffectiveDateTime", value.Trim());
                }
            }
        }

        /// <summary>Date of injury--year</summary>
        [IJEField(115, 985, 4, "Date of injury--year", "DOI_YR", 1)]
        public string DOI_YR
        {
            get
            {
                return DateTime_Dictionary_Get("DOI_YR", "yyyy", "DetailsOfInjury", "detailsOfInjuryEffectiveDateTime");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Dictionary_Set("DOI_YR", "yyyy", "DetailsOfInjury", "detailsOfInjuryEffectiveDateTime", value.Trim());
                }
            }
        }

        /// <summary>Time of injury</summary>
        [IJEField(116, 989, 4, "Time of injury", "TOI_HR", 1)]
        public string TOI_HR
        {
            get
            {
                return DateTime_Dictionary_Get("TOI_HR", "HHmm", "DetailsOfInjury", "detailsOfInjuryEffectiveDateTime");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Dictionary_Set("TOI_HR", "HHmm", "DetailsOfInjury", "detailsOfInjuryEffectiveDateTime", value.Trim());
                }
            }
        }

        /// <summary>Injury at work</summary>
        [IJEField(117, 993, 1, "Injury at work", "WORKINJ", 1)]
        public string WORKINJ
        {
            get
            {
                return Get_YNU("DeathFromWorkInjury");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Set_YNU("DeathFromWorkInjury", value);
                }
            }
        }

        /// <summary>Title of Certifier</summary>
        [IJEField(118, 994, 30, "Title of Certifier", "CERTL", 1)]
        public string CERTL
        {
            get
            {
                string code = Dictionary_Get_Full("CERTL", "CertifierType", "code");
                switch (code)
                {
                    case "434641000124105": // Physician (Certifier)
                        return "D";
                    case "434651000124107": // Physician (Pronouncer and Certifier)
                        return "P";
                    case "310193003": // Coroner
                        return "M";
                    case "440051000124108": // Medical Examiner
                        return "M";
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    switch (value.Trim())
                    {
                        case "D":
                            Dictionary_Set("CERTL", "CertifierType", "code", "434641000124105");
                            Dictionary_Set("CERTL", "CertifierType", "system", "http://snomed.info/sct");
                            Dictionary_Set("CERTL", "CertifierType", "display", "Physician (Certifier)");
                            break;
                        case "P":
                            Dictionary_Set("CERTL", "CertifierType", "code", "434651000124107");
                            Dictionary_Set("CERTL", "CertifierType", "system", "http://snomed.info/sct");
                            Dictionary_Set("CERTL", "CertifierType", "display", "Physician (Pronouncer and Certifier)");
                            break;
                        case "M":
                            Dictionary_Set("CERTL", "CertifierType", "code", "440051000124108");
                            Dictionary_Set("CERTL", "CertifierType", "system", "http://snomed.info/sct");
                            Dictionary_Set("CERTL", "CertifierType", "display", "Medical Examiner");
                            break;
                    }
                }
            }
        }

        /// <summary>Activity at time of death (computer generated)</summary>
        [IJEField(119, 1024, 1, "Activity at time of death (computer generated)", "INACT", 1)]
        public string INACT
        {
            get
            {
                // IJE options below, default to "9"
                // 0 While engaged in sports activity
                // 1 While engaged in leisure activities
                // 2 While working for income
                // 3 While engaged in other types of work
                // 4 While resting, sleeping, eating, or engaging in other vital activities
                // 8 While engaged in other specified activities
                // 9 During unspecified activity
                return "9";
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Time of Injury Unit</summary>
        [IJEField(125, 1075, 1, "Time of Injury Unit", "TOI_UNIT", 1)]
        public string TOI_UNIT
        {
            get
            {
                return "M"; // Military time
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Decedent ever served in Armed Forces?</summary>
        [IJEField(127, 1081, 1, "Decedent ever served in Armed Forces?", "ARMEDF", 1)]
        public string ARMEDF
        {
            get
            {
                return Get_YNU("ServedInArmedForces");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Set_YNU("ServedInArmedForces", value);
                }
            }
        }

        /// <summary>Death Institution name</summary>
        [IJEField(128, 1082, 30, "Death Institution name", "DINSTI", 1)]
        public string DINSTI
        {
            get
            {
                return Dictionary_Get("DINSTI", "Disposition", "dispositionPlaceName");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("DINSTI", "Disposition", "dispositionPlaceName", value);
                }
            }
        }

        /// <summary>Long String address for place of death</summary>
        [IJEField(129, 1112, 50, "Long String address for place of death", "ADDRESS_D", 1)]
        public string ADDRESS_D
        {
            get
            {
                return Dictionary_Get("ADDRESS_D", "PlaceOfDeath", "placeOfDeathLine1");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("ADDRESS_D", "PlaceOfDeath", "placeOfDeathLine1", value);
                }
            }
        }

        /// <summary>Place of death. City or Town name</summary>
        [IJEField(135, 1252, 28, "Place of death. City or Town name", "CITYTEXT_D", 1)]
        public string CITYTEXT_D
        {
            get
            {
                return Dictionary_Geo_Get("CITYTEXT_D", "PlaceOfDeath", "placeOfDeath", "city", false);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("CITYTEXT_D", "PlaceOfDeath", "placeOfDeath", "city", false, value);
                }
            }
        }

        /// <summary>Place of death. State name literal</summary>
        [IJEField(136, 1280, 28, "Place of death. State name literal", "STATETEXT_D", 1)]
        public string STATETEXT_D
        {
            get
            {
                return Dictionary_Geo_Get("STATETEXT_D", "PlaceOfDeath", "placeOfDeath", "state", false);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Place of death. Zip code</summary>
        [IJEField(137, 1308, 9, "Place of death. Zip code", "ZIP9_D", 1)]
        public string ZIP9_D
        {
            get
            {
                return Dictionary_Get("ZIP9_D", "PlaceOfDeath", "placeOfDeathZip");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("ZIP9_D", "PlaceOfDeath", "placeOfDeathZip", value);
                }
            }
        }

        /// <summary>Place of death. County of Death</summary>
        [IJEField(138, 1317, 28, "Place of death. County of Death", "COUNTYTEXT_D", 2)]
        public string COUNTYTEXT_D
        {
            get
            {
                return Dictionary_Geo_Get("COUNTYTEXT_D", "PlaceOfDeath", "placeOfDeath", "county", false);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Place of death. City FIPS code</summary>
        [IJEField(139, 1345, 5, "Place of death. City FIPS code", "CITYCODE_D", 1)]
        public string CITYCODE_D
        {
            get
            {
                return Dictionary_Geo_Get("CITYCODE_D", "PlaceOfDeath", "placeOfDeath", "city", true);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Decedent's Residence - City or Town name</summary>
        [IJEField(151, 1560, 28, "Decedent's Residence - City or Town name", "CITYTEXT_R", 3)]
        public string CITYTEXT_R
        {
            get
            {
                return Dictionary_Geo_Get("CITYTEXT_R", "Residence", "residence", "city", false);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("CITYTEXT_R", "Residence", "residence", "city", false, value);
                }
            }
        }

        /// <summary>Decedent's Residence - ZIP code</summary>
        [IJEField(152, 1588, 9, "Decedent's Residence - ZIP code", "ZIP9_R", 1)]
        public string ZIP9_R
        {
            get
            {
                return Dictionary_Geo_Get("ZIP9_R", "Residence", "residence", "zip", false);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("ZIP9_R", "Residence", "residence", "zip", false, value);
                }
            }
        }

        /// <summary>Decedent's Residence - County</summary>
        [IJEField(153, 1597, 28, "Decedent's Residence - County", "COUNTYTEXT_R", 1)]
        public string COUNTYTEXT_R
        {
            get
            {
                return Dictionary_Geo_Get("COUNTYTEXT_R", "Residence", "residence", "county", false);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Decedent's Residence - State name</summary>
        [IJEField(154, 1625, 28, "Decedent's Residence - State name", "STATETEXT_R", 1)]
        public string STATETEXT_R
        {
            get
            {
                return Dictionary_Geo_Get("STATETEXT_R", "Residence", "residence", "state", false);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Decedent's Residence - COUNTRY name</summary>
        [IJEField(155, 1653, 28, "Decedent's Residence - COUNTRY name", "COUNTRYTEXT_R", 1)]
        public string COUNTRYTEXT_R
        {
            get
            {
                return Dictionary_Geo_Get("COUNTRYTEXT_R", "Residence", "residence", "country", false);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Long string address for decedent's place of residence same as above but allows states to choose the way they capture information.</summary>
        [IJEField(156, 1681, 50, "Long string address for decedent's place of residence same as above but allows states to choose the way they capture information.", "ADDRESS_R", 1)]
        public string ADDRESS_R
        {
            get
            {
                return Dictionary_Get("ADDRESS_R", "Residence", "residenceLine1");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("ADDRESS_R", "Residence", "residenceLine1", value);
                }
            }
        }

        /// <summary>Middle Name of Decedent</summary>
        [IJEField(165, 1808, 50, "Middle Name of Decedent", "DMIDDLE", 2)]
        public string DMIDDLE
        {
            get
            {
                string[] names = record.GivenNames;
                if (names.Length > 1)
                {
                    return names[1];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (record.GivenNames != null)
                    {
                        List<string> names = record.GivenNames.ToList();
                        names.Add(value.Trim());
                        record.GivenNames = names.ToArray();
                    }
                }
            }
        }

        /// <summary>Was case Referred to Medical Examiner/Coroner?</summary>
        [IJEField(171, 2108, 1, "Was case Referred to Medical Examiner/Coroner?", "REFERRED", 1)]
        public string REFERRED
        {
            get
            {
                return Get_YNU("MedicalExaminerContacted");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Set_YNU("MedicalExaminerContacted", value);
                }
            }
        }

        /// <summary>Place of Injury- literal</summary>
        [IJEField(172, 2109, 50, "Place of Injury- literal", "POILITRL", 1)]
        public string POILITRL
        {
            get
            {
                return Dictionary_Get("POILITRL", "DetailsOfInjury", "detailsOfInjuryPlaceDescription");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("POILITRL", "DetailsOfInjury", "detailsOfInjuryPlaceDescription", value);
                }
            }
        }

        /// <summary>Describe How Injury Occurred</summary>
        [IJEField(173, 2159, 250, "Describe How Injury Occurred", "HOWINJ", 1)]
        public string HOWINJ
        {
            get
            {
                return Dictionary_Get("HOWINJ", "DetailsOfInjury", "detailsOfInjuryDescription");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("HOWINJ", "DetailsOfInjury", "detailsOfInjuryDescription", value);
                }
            }
        }

        /// <summary>If Transportation Accident, Specify</summary>
        [IJEField(174, 2409, 30, "If Transportation Accident, Specify", "TRANSPRT", 1)]
        public string TRANSPRT
        {
            get
            {
                string code = Dictionary_Get_Full("TRANSPRT", "DeathFromTransportInjury", "code");
                switch (code)
                {
                    case "236320001": // Vehicle driver
                        return "DR";
                    case "257500003": // Passenger
                        return "PA";
                    case "257518000": // Pedestrian
                        return "PE";
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    switch (value.Trim())
                    {
                        case "DR":
                            Dictionary_Set("TRANSPRT", "DeathFromTransportInjury", "code", "236320001");
                            Dictionary_Set("TRANSPRT", "DeathFromTransportInjury", "system", "http://snomed.info/sct");
                            Dictionary_Set("TRANSPRT", "DeathFromTransportInjury", "display", "Vehicle driver");
                            break;
                        case "PA":
                            Dictionary_Set("TRANSPRT", "DeathFromTransportInjury", "code", "257500003");
                            Dictionary_Set("TRANSPRT", "DeathFromTransportInjury", "system", "http://snomed.info/sct");
                            Dictionary_Set("TRANSPRT", "DeathFromTransportInjury", "display", "Passenger");
                            break;
                        case "PE":
                            Dictionary_Set("TRANSPRT", "DeathFromTransportInjury", "code", "257518000");
                            Dictionary_Set("TRANSPRT", "DeathFromTransportInjury", "system", "http://snomed.info/sct");
                            Dictionary_Set("TRANSPRT", "DeathFromTransportInjury", "display", "Pedestrian");
                            break;
                    }
                }
            }
        }

        /// <summary>County of Injury - literal</summary>
        [IJEField(175, 2439, 28, "County of Injury - literal", "COUNTYTEXT_I", 1)]
        public string COUNTYTEXT_I
        {
            get
            {
                return Dictionary_Geo_Get("COUNTYTEXT_I", "DetailsOfInjury", "detailsOfInjury", "county", false);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>County of Injury code</summary>
        [IJEField(176, 2467, 3, "County of Injury code", "COUNTYCODE_I", 2)]
        public string COUNTYCODE_I
        {
            get
            {
                return Dictionary_Geo_Get("COUNTYCODE_I", "DetailsOfInjury", "detailsOfInjury", "county", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("COUNTYCODE_I", "DetailsOfInjury", "detailsOfInjury", "county", true, value);
                }
            }
        }

        /// <summary>Town/city of Injury - literal</summary>
        [IJEField(177, 2470, 28, "Town/city of Injury - literal", "CITYTEXT_I", 3)]
        public string CITYTEXT_I
        {
            get
            {
                return Dictionary_Geo_Get("CITYTEXT_I", "DetailsOfInjury", "detailsOfInjury", "city", false);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("CITYTEXT_I", "DetailsOfInjury", "detailsOfInjury", "city", false, value);
                }
            }
        }

        /// <summary>Town/city of Injury code</summary>
        [IJEField(178, 2498, 5, "Town/city of Injury code", "CITYCODE_I", 3)]
        public string CITYCODE_I
        {
            get
            {
                return Dictionary_Geo_Get("CITYCODE_I", "DetailsOfInjury", "detailsOfInjury", "city", true);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>State, U.S. Territory or Canadian Province of Injury - code</summary>
        [IJEField(179, 2503, 2, "State, U.S. Territory or Canadian Province of Injury - code", "STATECODE_I", 1)]
        public string STATECODE_I
        {
            get
            {
                return Dictionary_Geo_Get("STATECODE_I", "DetailsOfInjury", "detailsOfInjury", "state", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("STATECODE_I", "DetailsOfInjury", "detailsOfInjury", "state", true, value);
                }
            }
        }

        /// <summary>Cause of Death Part I Line a</summary>
        [IJEField(184, 2542, 120, "Cause of Death Part I Line a", "COD1A", 1)]
        public string COD1A
        {
            get
            {
                if (record.CausesOfDeath.Count() >= 1) {
                    return record.CausesOfDeath[0].Item1.Trim();
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string, Dictionary<string, string>>> results = record.CausesOfDeath.ToList();
                    results.Add(Tuple.Create(value.Trim(), "", new Dictionary<string, string>()));
                    record.CausesOfDeath = results.ToArray();
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line a</summary>
        [IJEField(185, 2662, 20, "Cause of Death Part I Interval, Line a", "INTERVAL1A", 2)]
        public string INTERVAL1A
        {
            get
            {
                if (record.CausesOfDeath.Count() >= 1) {
                    return record.CausesOfDeath[0].Item2.Trim();
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string, Dictionary<string, string>>> results = record.CausesOfDeath.ToList();
                    Tuple<string, string, Dictionary<string, string>> last = results.Last();
                    Tuple<string, string, Dictionary<string, string>> updated = Tuple.Create(last.Item1, value.Trim(), last.Item3);
                    Tuple<string, string, Dictionary<string, string>>[] updatedCauses = results.ToArray();
                    updatedCauses[0] = updated;
                    record.CausesOfDeath = updatedCauses;
                }
            }
        }

        /// <summary>Cause of Death Part I Line b</summary>
        [IJEField(186, 2682, 120, "Cause of Death Part I Line b", "COD1B", 3)]
        public string COD1B
        {
            get
            {
                if (record.CausesOfDeath.Count() >= 2) {
                    return record.CausesOfDeath[1].Item1.Trim();
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string, Dictionary<string, string>>> results = record.CausesOfDeath.ToList();
                    results.Add(Tuple.Create(value.Trim(), "", new Dictionary<string, string>()));
                    record.CausesOfDeath = results.ToArray();
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line b</summary>
        [IJEField(187, 2802, 20, "Cause of Death Part I Interval, Line b", "INTERVAL1B", 4)]
        public string INTERVAL1B
        {
            get
            {
                if (record.CausesOfDeath.Count() >= 2) {
                    return record.CausesOfDeath[1].Item2.Trim();
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string, Dictionary<string, string>>> results = record.CausesOfDeath.ToList();
                    Tuple<string, string, Dictionary<string, string>> last = results.Last();
                    Tuple<string, string, Dictionary<string, string>> updated = Tuple.Create(last.Item1, value.Trim(), last.Item3);
                    Tuple<string, string, Dictionary<string, string>>[] updatedCauses = results.ToArray();
                    updatedCauses[1] = updated;
                    record.CausesOfDeath = updatedCauses;
                }
            }
        }

        /// <summary>Cause of Death Part I Line c</summary>
        [IJEField(188, 2822, 120, "Cause of Death Part I Line c", "COD1C", 5)]
        public string COD1C
        {
            get
            {
                if (record.CausesOfDeath.Count() >= 3) {
                    return record.CausesOfDeath[2].Item1.Trim();
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string, Dictionary<string, string>>> results = record.CausesOfDeath.ToList();
                    results.Add(Tuple.Create(value.Trim(), "", new Dictionary<string, string>()));
                    record.CausesOfDeath = results.ToArray();
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line c</summary>
        [IJEField(189, 2942, 20, "Cause of Death Part I Interval, Line c", "INTERVAL1C", 6)]
        public string INTERVAL1C
        {
            get
            {
                if (record.CausesOfDeath.Count() >= 3) {
                    return record.CausesOfDeath[2].Item2.Trim();
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string, Dictionary<string, string>>> results = record.CausesOfDeath.ToList();
                    Tuple<string, string, Dictionary<string, string>> last = results.Last();
                    Tuple<string, string, Dictionary<string, string>> updated = Tuple.Create(last.Item1, value.Trim(), last.Item3);
                    Tuple<string, string, Dictionary<string, string>>[] updatedCauses = results.ToArray();
                    updatedCauses[2] = updated;
                    record.CausesOfDeath = updatedCauses;
                }
            }
        }

        /// <summary>Cause of Death Part I Line d</summary>
        [IJEField(190, 2962, 120, "Cause of Death Part I Line d", "COD1D", 7)]
        public string COD1D
        {
            get
            {
                if (record.CausesOfDeath.Count() >= 4) {
                    return record.CausesOfDeath[3].Item1.Trim();
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string, Dictionary<string, string>>> results = record.CausesOfDeath.ToList();
                    results.Add(Tuple.Create(value.Trim(), "", new Dictionary<string, string>()));
                    record.CausesOfDeath = results.ToArray();
                }
            }
        }

        /// <summary>Cause of Death Part I Interval, Line d</summary>
        [IJEField(191, 3082, 20, "Cause of Death Part I Interval, Line d", "INTERVAL1D", 8)]
        public string INTERVAL1D
        {
            get
            {
                if (record.CausesOfDeath.Count() >= 4) {
                    return record.CausesOfDeath[3].Item2.Trim();
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    List<Tuple<string, string, Dictionary<string, string>>> results = record.CausesOfDeath.ToList();
                    Tuple<string, string, Dictionary<string, string>> last = results.Last();
                    Tuple<string, string, Dictionary<string, string>> updated = Tuple.Create(last.Item1, value.Trim(), last.Item3);
                    Tuple<string, string, Dictionary<string, string>>[] updatedCauses = results.ToArray();
                    updatedCauses[3] = updated;
                    record.CausesOfDeath = updatedCauses;
                }
            }
        }

        /// <summary>Cause of Death Part II</summary>
        [IJEField(192, 3102, 240, "Cause of Death Part II", "OTHERCONDITION", 1)]
        public string OTHERCONDITION
        {
            get
            {
                if (record.ContributingConditions != null)
                {
                    return record.ContributingConditions.Trim();
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    record.ContributingConditions = value.Trim();
                }
            }
        }

        /// <summary>Decedent's Maiden Name</summary>
        [IJEField(193, 3342, 50, "Decedent's Maiden Name", "DMAIDEN", 1)]
        public string DMAIDEN
        {
            get
            {
                return LeftJustified_Get("DMAIDEN", "MaidenName");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    LeftJustified_Set("DMAIDEN", "MaidenName", value);
                }
            }
        }

        /// <summary>Decedent's Birth Place City - Literal</summary>
        [IJEField(195, 3397, 28, "Decedent's Birth Place City - Literal", "DBPLACECITY", 3)]
        public string DBPLACECITY
        {
            get
            {
                return Dictionary_Geo_Get("DBPLACECITY", "PlaceOfBirth", "placeOfBirth", "city", false);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("DBPLACECITY", "PlaceOfBirth", "placeOfBirth", "city", false, value);
                    // We've got city, so we probably also have state now - so attempt to find county while we're at it (IJE does NOT include this).
                    string state = Dictionary_Geo_Get("BSTATE", "PlaceOfBirth", "placeOfBirth", "state", false);
                    string county = dataLookup.StateCodeAndCityNameToCountyName(state, value);
                    if (!String.IsNullOrWhiteSpace(county))
                    {
                        Dictionary_Geo_Set("DBPLACECITY", "PlaceOfBirth", "placeOfBirth", "county", false, county);
                    }
                }
            }
        }

        /// <summary>State, U.S. Territory or Canadian Province of Disposition - code</summary>
        [IJEField(201, 3535, 2, "State, U.S. Territory or Canadian Province of Disposition - code", "DISPSTATECD", 1)]
        public string DISPSTATECD
        {
            get
            {
                return Dictionary_Geo_Get("DISPSTATECD", "Disposition", "dispositionPlace", "state", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("DISPSTATECD", "Disposition", "dispositionPlace", "state", true, value);
                }
            }
        }

        /// <summary>Disposition State or Territory - Literal</summary>
        [IJEField(202, 3537, 28, "Disposition State or Territory - Literal", "DISPSTATE", 1)]
        public string DISPSTATE
        {
            get
            {
                return Dictionary_Geo_Get("DISPSTATE", "Disposition", "dispositionPlace", "state", false);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Disposition City - Literal</summary>
        [IJEField(204, 3570, 28, "Disposition City - Literal", "DISPCITY", 3)]
        public string DISPCITY
        {
            get
            {
                return Dictionary_Geo_Get("DISPCITY", "Disposition", "dispositionPlace", "city", false);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("DISPCITY", "Disposition", "dispositionPlace", "city", false, value);
                    // We've got city, so we probably also have state now - so attempt to find county while we're at it (IJE does NOT include this).
                    string state = Dictionary_Geo_Get("DISPSTATECD", "Disposition", "dispositionPlace", "state", false);
                    string county = dataLookup.StateCodeAndCityNameToCountyName(state, value);
                    if (!String.IsNullOrWhiteSpace(county))
                    {
                        Dictionary_Geo_Set("DISPCITY", "Disposition", "dispositionPlace", "county", false, county);
                        // If we found a county, we know the country.
                        Dictionary_Geo_Set("DISPCITY", "Disposition", "dispositionPlace", "country", false, "United States");
                    }
                }
            }
        }

        /// <summary>Funeral Facility Name</summary>
        [IJEField(205, 3598, 100, "Funeral Facility Name", "FUNFACNAME", 1)]
        public string FUNFACNAME
        {
            get
            {
                return Dictionary_Get("FUNFACNAME", "Disposition", "funeralFacilityName");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("FUNFACNAME", "Disposition", "funeralFacilityName", value);
                }
            }
        }

        /// <summary>Long string address for Funeral Facility same as above but allows states to choose the way they capture information.</summary>
        [IJEField(212, 3773, 50, "Long string address for Funeral Facility same as above but allows states to choose the way they capture information.", "FUNFACADDRESS", 1)]
        public string FUNFACADDRESS
        {
            get
            {
                return Dictionary_Get("FUNFACADDRESS", "Disposition", "funeralFacilityLine1");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("FUNFACADDRESS", "Disposition", "funeralFacilityLine1", value);
                }
            }
        }

        /// <summary>Funeral Facility - City or Town name</summary>
        [IJEField(213, 3823, 28, "Funeral Facility - City or Town name", "FUNCITYTEXT", 3)]
        public string FUNCITYTEXT
        {
            get
            {
                return Dictionary_Get("FUNCITYTEXT", "Disposition", "funeralFacilityCity");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("FUNCITYTEXT", "Disposition", "funeralFacilityCity", value);
                    // We've got city, so we probably also have state now - so attempt to find county while we're at it (IJE does NOT include this).
                    string state = dataLookup.StateNameToStateCode(Dictionary_Get("FUNSTATE", "Disposition", "funeralFacilityState"));
                    string county = dataLookup.StateCodeAndCityNameToCountyName(state, value);
                    if (!String.IsNullOrWhiteSpace(county))
                    {
                        Dictionary_Set("FUNCITYTEXT", "Disposition", "funeralFacilityCounty", county);
                        // If we found a county, we know the country.
                        Dictionary_Set("FUNCITYTEXT", "Disposition", "funeralFacilityCountry", "United States");
                    }
                }
            }
        }

        /// <summary>State, U.S. Territory or Canadian Province of Funeral Facility - code</summary>
        [IJEField(214, 3851, 2, "State, U.S. Territory or Canadian Province of Funeral Facility - code", "FUNSTATECD", 1)]
        public string FUNSTATECD
        {
            get
            {
                return dataLookup.StateNameToStateCode(Dictionary_Get_Full("FUNSTATECD", "Disposition", "funeralFacilityState"));
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("FUNSTATECD", "Disposition", "funeralFacilityState", dataLookup.StateCodeToStateName(value));
                }
            }
        }

        /// <summary>State, U.S. Territory or Canadian Province of Funeral Facility - literal</summary>
        [IJEField(215, 3853, 28, "State, U.S. Territory or Canadian Province of Funeral Facility - literal", "FUNSTATE", 1)]
        public string FUNSTATE
        {
            get
            {
                return Dictionary_Get("FUNSTATE", "Disposition", "funeralFacilityState");
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Funeral Facility - ZIP</summary>
        [IJEField(216, 3881, 9, "Funeral Facility - ZIP", "FUNZIP", 1)]
        public string FUNZIP
        {
            get
            {
                return Dictionary_Get("FUNZIP", "Disposition", "funeralFacilityZip");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("FUNZIP", "Disposition", "funeralFacilityZip", value);
                }
            }
        }

        /// <summary>Person Pronouncing Date Signed</summary>
        [IJEField(217, 3890, 8, "Person Pronouncing Date Signed", "PPDATESIGNED", 1)]
        public string PPDATESIGNED
        {
            get
            {
                return DateTime_Get("PPDATESIGNED", "MMddyyyy", "DatePronouncedDead");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("PPDATESIGNED", "MMddyyyy", "DatePronouncedDead", value);
                }
            }
        }

        /// <summary>Person Pronouncing Time Pronounced</summary>
        [IJEField(218, 3898, 4, "Person Pronouncing Time Pronounced", "PPTIME", 1)]
        public string PPTIME
        {
            get
            {
                return DateTime_Get("PPTIME", "HHmm", "DatePronouncedDead");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    DateTime_Set("PPTIME", "HHmm", "DatePronouncedDead", value);
                }
            }
        }

        /// <summary>Certifier's First Name</summary>
        [IJEField(219, 3902, 50, "Certifier's First Name", "CERTFIRST", 1)]
        public string CERTFIRST
        {
            get
            {
                string[] names = record.CertifierGivenNames;
                if (names.Length > 0)
                {
                    return names[0];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    record.CertifierGivenNames = new string[] { value.Trim() };
                }
            }
        }

        /// <summary>Certifier's Middle Name</summary>
        [IJEField(220, 3952, 50, "Certifier's Middle Name", "CERTMIDDLE", 2)]
        public string CERTMIDDLE
        {
            get
            {
                string[] names = record.CertifierGivenNames;
                if (names.Length > 1)
                {
                    return names[1];
                }
                return "";
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (record.GivenNames != null)
                    {
                        List<string> names = record.CertifierGivenNames.ToList();
                        names.Add(value.Trim());
                        record.CertifierGivenNames = names.ToArray();
                    }
                }
            }
        }

        /// <summary>Certifier's Last Name</summary>
        [IJEField(221, 4002, 50, "Certifier's Last Name", "CERTLAST", 3)]
        public string CERTLAST
        {
            get
            {
                return LeftJustified_Get("CERTLAST", "CertifierFamilyName");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    LeftJustified_Set("CERTLAST", "CertifierFamilyName", value.Trim());
                }
            }
        }

        /// <summary>Certifier's Suffix Name</summary>
        [IJEField(222, 4052, 10, "Certifier's Suffix Name", "CERTSUFFIX", 4)]
        public string CERTSUFFIX
        {
            get
            {
                return LeftJustified_Get("CERTSUFFIX", "CertifierSuffix");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    LeftJustified_Set("CERTSUFFIX", "CertifierSuffix", value.Trim());
                }
            }
        }

        /// <summary>Long string address for Certifier same as above but allows states to choose the way they capture information.</summary>
        [IJEField(229, 4137, 50, "Long string address for Certifier same as above but allows states to choose the way they capture information.", "CERTADDRESS", 1)]
        public string CERTADDRESS
        {
            get
            {
                return Dictionary_Get("CERTADDRESS", "CertifierAddress", "certifierAddressLine1");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("CERTADDRESS", "CertifierAddress", "certifierAddressLine1", value);
                }
            }
        }

        /// <summary>Certifier - City or Town name</summary>
        [IJEField(230, 4187, 28, "Certifier - City or Town name", "CERTCITYTEXT", 1)]
        public string CERTCITYTEXT
        {
            get
            {
                return Dictionary_Get("CERTCITYTEXT", "CertifierAddress", "certifierAddressCity");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("CERTCITYTEXT", "CertifierAddress", "certifierAddressCity", value);
                }
            }
        }

        /// <summary>State, U.S. Territory or Canadian Province of Certifier - code</summary>
        [IJEField(231, 4215, 2, "State, U.S. Territory or Canadian Province of Certifier - code", "CERTSTATECD", 2)]
        public string CERTSTATECD
        {
            get
            {
                return dataLookup.StateNameToStateCode(Dictionary_Get_Full("CERTSTATECD", "CertifierAddress", "certifierAddressState"));
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("CERTSTATECD", "CertifierAddress", "certifierAddressState", dataLookup.StateCodeToStateName(value));
                }
            }
        }

        /// <summary>State, U.S. Territory or Canadian Province of Certifier - literal</summary>
        [IJEField(232, 4217, 28, "State, U.S. Territory or Canadian Province of Certifier - literal", "CERTSTATE", 1)]
        public string CERTSTATE
        {
            get
            {
                return Dictionary_Get("CERTSTATE", "CertifierAddress", "certifierAddressState");
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Certifier - Zip</summary>
        [IJEField(233, 4245, 9, "Certifier - Zip", "CERTZIP", 1)]
        public string CERTZIP
        {
            get
            {
                return Dictionary_Get("CERTZIP", "CertifierAddress", "certifierAddressZip");
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Set("CERTZIP", "CertifierAddress", "certifierAddressZip", value);
                }
            }
        }

        /// <summary>State, U.S. Territory or Canadian Province of Injury - literal</summary>
        [IJEField(236, 4270, 28, "State, U.S. Territory or Canadian Province of Injury - literal", "STINJURY", 1)]
        public string STINJURY
        {
            get
            {
                return Dictionary_Geo_Get("STINJURY", "DetailsOfInjury", "detailsOfInjury", "state", false);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>State, U.S. Territory or Canadian Province of Birth - literal</summary>
        [IJEField(237, 4298, 28, "State, U.S. Territory or Canadian Province of Birth - literal", "STATEBTH", 1)]
        public string STATEBTH
        {
            get
            {
                return Dictionary_Geo_Get("STATEBTH", "PlaceOfBirth", "placeOfBirth", "state", false);
            }
            set
            {
                // NOOP
            }
        }

        /// <summary>Country of Death - Code</summary>
        [IJEField(238, 4326, 2, "Country of Death - Code", "DTHCOUNTRYCD", 1)]
        public string DTHCOUNTRYCD
        {
            get
            {
                return Dictionary_Geo_Get("DTHCOUNTRYCD", "PlaceOfDeath", "placeOfDeath", "country", true);
            }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    Dictionary_Geo_Set("DTHCOUNTRYCD", "PlaceOfDeath", "placeOfDeath", "country", true, value);
                }
            }
        }

        /// <summary>Country of Death - Literal</summary>
        [IJEField(239, 4328, 28, "Country of Death - Literal", "DTHCOUNTRY", 1)]
        public string DTHCOUNTRY
        {
            get
            {
                return Dictionary_Geo_Get("DTHCOUNTRY", "PlaceOfDeath", "placeOfDeath", "country", false);
            }
            set
            {
                // NOOP
            }
        }
    }
}
