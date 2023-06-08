using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Models
{
    public class MailAttachment
    {
        public string Id { get; set; }

        public string ExternalId { get; set; }

        public string MessageId { get; set; }

        public string Name { get; set; }

        public string ContentType { get; set; }

        public uint? Size { get; set; }

        public bool IsInline { get; set; }

        public string ContentBytes { get; set; }

        public string BlobUrl { get; set; }
    }
}
