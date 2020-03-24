using System;
using System.Linq;
using System.Collections.Generic;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace VRDR
{
    /// <summary>Class <c>DeathRecordSubmission</c> supports the submission of VRDR records.</summary>
    public class DeathRecordSubmission : BaseMessage
    {
        /// <summary>Bundle that contains the message payload.</summary>
        private DeathRecord Payload;

        /// <summary>Default constructor that creates a new, empty DeathRecordSubmission.</summary>
        public DeathRecordSubmission() : base("vrdr_submission")
        {
        }

        /// <summary>Constructor that takes a VRDR.DeathRecord and wraps it in a DeathRecordSubmission.</summary>
        /// <param name="record">the VRDR.DeathRecord to create a DeathRecordSubmission for.</param>
        public DeathRecordSubmission(DeathRecord record) : this()
        {
            MessagePayload = record;
        }

        /// <summary>Constructor that takes a string that represents a DeathRecordSubmission message in either XML or JSON format.</summary>
        /// <param name="message">represents a DeathRecordSubmission message in either XML or JSON format.</param>
        /// <param name="permissive">if the parser should be permissive when parsing the given string</param>
        /// <exception cref="ArgumentException">Message is neither valid XML nor JSON.</exception>
        public DeathRecordSubmission(string message, bool permissive = false) : base(message, permissive)
        {
        }

        /// <summary>Message payload</summary>
        /// <value>the message payload as a FHIR Bundle.</value>
        public DeathRecord MessagePayload
        {
            get
            {
                return Payload;
            }
            set
            {
                Payload = value;
                MessageBundle.Entry.RemoveAll( entry => entry.Resource.ResourceType == ResourceType.Bundle );
                MessageBundle.AddResourceEntry(Payload.GetBundle(), "urn:uuid:" + Payload.GetBundle().Id);
                Header.Focus.Clear();
                Header.Focus.Add(new ResourceReference(Payload.GetBundle().Id));
            }
        }

        /// <summary>Restores class references from a newly parsed record.</summary>
        protected override void RestoreReferences()
        {
            base.RestoreReferences();

            // Grab Payload
            var payloadEntry = MessageBundle.Entry.FirstOrDefault( entry => entry.Resource.ResourceType == ResourceType.Bundle );
            if (payloadEntry != null)
            {
                Payload = new DeathRecord((Bundle)payloadEntry.Resource);
            }
        }
    }
}
