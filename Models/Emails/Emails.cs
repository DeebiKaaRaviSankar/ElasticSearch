using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEditor.Models
{
    public class Email : Editable
    {
        public Email()
        {
            this.To = new List<EmailContact>();
            this.Cc = new List<EmailContact>();
            this.Bcc = new List<EmailContact>();
            this.ReplyTo = new List<EmailContact>();
            this.MailAttachments = new List<MailAttachment>();
        }

        public string Id { get; set; }

        public string ExternalId { get; set; }

        public int ExternalToolId { get; set; }

        public string IntegratedEmailId { get; set; }

        public DateTimeOffset? ReceivedDateTime { get; set; }

        public DateTimeOffset? SentDateTime { get; set; }

        public bool? HasAttachments { get; set; }

        public string InternetMessageId { get; set; }

        public string Subject { get; set; }

        public string BodyPreview { get; set; }

        public string Importance { get; set; }

        public string ParentFolderId { get; set; }

        public string ConversationId { get; set; }

        public bool? IsRead { get; set; }

        public bool? IsDraft { get; set; }

        public string ContentType { get; set; }

        public string Content { get; set; }

        public EmailContact From { get; set; }

        public ICollection<EmailContact> To { get; set; }

        public ICollection<EmailContact> Cc { get; set; }

        public ICollection<EmailContact> Bcc { get; set; }

        public ICollection<EmailContact> ReplyTo { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public FolderType FolderType { get; set; }

        public bool IsDeleted { get; set; }

        public IEnumerable<MailAttachment> MailAttachments { get; set; }
    }
}
