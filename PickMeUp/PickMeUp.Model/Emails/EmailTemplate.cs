using System.Net;
using System.Text;

namespace PickMeUp.Model.Emails
{
    public static class EmailTemplate
    {
        private const string Brand = "#546190";
        private const string BrandDark = "#3E4867";
        private const string PageBackground = "#EEF0F6";
        private const string CardBackground = "#FFFFFF";
        private const string TextPrimary = "#1D1B20";
        private const string TextMuted = "#6B6880";
        private const string Border = "#E4E3ED";
        private const string FontStack =
            "Segoe UI, Roboto, Helvetica Neue, Helvetica, Arial, sans-serif";

        public static string RenderHtml(string subject, EmailContent content)
        {
            var html = new StringBuilder();

            html.Append("<!doctype html><html><head><meta charset=\"utf-8\">");
            html.Append("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
            html.Append($"<title>{Escape(subject)}</title></head>");
            html.Append($"<body style=\"margin:0;padding:0;background:{PageBackground};\">");

            html.Append($"<span style=\"display:none;max-height:0;overflow:hidden;opacity:0;\">{Escape(content.Intro)}</span>");

            html.Append($"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background:{PageBackground};\">");
            html.Append("<tr><td align=\"center\" style=\"padding:24px 12px;\">");
            html.Append("<table role=\"presentation\" width=\"600\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:100%;max-width:600px;\">");

            html.Append($"<tr><td style=\"background:{Brand};border-radius:10px 10px 0 0;padding:20px 28px;\">");
            html.Append($"<span style=\"font-family:{FontStack};font-size:20px;font-weight:700;color:#FFFFFF;letter-spacing:0.4px;\">PickMeUp</span>");
            html.Append($"<span style=\"font-family:{FontStack};font-size:13px;color:#D7DBEA;padding-left:10px;\">taxi, when you need it</span>");
            html.Append("</td></tr>");

            html.Append($"<tr><td style=\"background:{CardBackground};border-left:1px solid {Border};border-right:1px solid {Border};padding:28px;\">");
            html.Append($"<h1 style=\"margin:0 0 12px 0;font-family:{FontStack};font-size:22px;line-height:28px;font-weight:600;color:{TextPrimary};\">{Escape(content.Title)}</h1>");
            html.Append($"<p style=\"margin:0 0 18px 0;font-family:{FontStack};font-size:15px;line-height:23px;color:{TextPrimary};\">{Escape(content.Intro)}</p>");

            if (!string.IsNullOrWhiteSpace(content.Code))
            {
                html.Append($"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"margin:0 0 18px 0;\"><tr><td align=\"center\" style=\"background:{PageBackground};border:1px solid {Border};border-radius:8px;padding:20px 12px;\">");
                html.Append($"<div style=\"font-family:Consolas,Menlo,Courier New,monospace;font-size:32px;line-height:38px;font-weight:700;letter-spacing:8px;color:{BrandDark};\">{Escape(content.Code!)}</div>");

                if (!string.IsNullOrWhiteSpace(content.CodeCaption))
                {
                    html.Append($"<div style=\"font-family:{FontStack};font-size:13px;line-height:19px;color:{TextMuted};padding-top:8px;\">{Escape(content.CodeCaption!)}</div>");
                }

                html.Append("</td></tr></table>");
            }

            if (content.Facts.Count > 0)
            {
                html.Append($"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"margin:0 0 18px 0;border:1px solid {Border};border-radius:8px;\">");

                for (var i = 0; i < content.Facts.Count; i++)
                {
                    var fact = content.Facts[i];
                    var divider = i == content.Facts.Count - 1 ? "none" : $"1px solid {Border}";

                    html.Append($"<tr><td style=\"font-family:{FontStack};font-size:13px;line-height:19px;color:{TextMuted};padding:10px 16px;border-bottom:{divider};width:40%;\">{Escape(fact.Label)}</td>");
                    html.Append($"<td style=\"font-family:{FontStack};font-size:14px;line-height:20px;color:{TextPrimary};font-weight:600;padding:10px 16px;border-bottom:{divider};\">{Escape(fact.Value)}</td></tr>");
                }

                html.Append("</table>");
            }

            if (!string.IsNullOrWhiteSpace(content.Highlight))
            {
                html.Append($"<p style=\"margin:0 0 18px 0;font-family:{FontStack};font-size:15px;line-height:23px;color:{BrandDark};font-weight:600;\">{Escape(content.Highlight!)}</p>");
            }

            if (!string.IsNullOrWhiteSpace(content.SecurityNote))
            {
                html.Append($"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"background:{PageBackground};border-left:3px solid {Brand};border-radius:4px;padding:12px 14px;\">");
                html.Append($"<span style=\"font-family:{FontStack};font-size:13px;line-height:20px;color:{TextMuted};\">{Escape(content.SecurityNote!)}</span>");
                html.Append("</td></tr></table>");
            }

            html.Append("</td></tr>");

            html.Append($"<tr><td style=\"background:{CardBackground};border:1px solid {Border};border-top:none;border-radius:0 0 10px 10px;padding:16px 28px;\">");
            html.Append($"<span style=\"font-family:{FontStack};font-size:12px;line-height:18px;color:{TextMuted};\">PickMeUp &middot; automated message, do not reply &middot; {DateTime.UtcNow.Year}</span>");
            html.Append("</td></tr>");

            html.Append("</table></td></tr></table></body></html>");

            return html.ToString();
        }

        public static string RenderText(EmailContent content)
        {
            var text = new StringBuilder();

            text.AppendLine("PickMeUp");
            text.AppendLine(new string('=', 40));
            text.AppendLine();
            text.AppendLine(content.Title);
            text.AppendLine();
            text.AppendLine(content.Intro);

            if (!string.IsNullOrWhiteSpace(content.Code))
            {
                text.AppendLine();
                text.AppendLine($"    {content.Code}");

                if (!string.IsNullOrWhiteSpace(content.CodeCaption))
                {
                    text.AppendLine($"    {content.CodeCaption}");
                }
            }

            if (content.Facts.Count > 0)
            {
                text.AppendLine();

                foreach (var fact in content.Facts)
                {
                    text.AppendLine($"  {fact.Label}: {fact.Value}");
                }
            }

            if (!string.IsNullOrWhiteSpace(content.Highlight))
            {
                text.AppendLine();
                text.AppendLine(content.Highlight);
            }

            if (!string.IsNullOrWhiteSpace(content.SecurityNote))
            {
                text.AppendLine();
                text.AppendLine(content.SecurityNote);
            }

            text.AppendLine();
            text.AppendLine(new string('-', 40));
            text.AppendLine($"PickMeUp · automated message, do not reply · {DateTime.UtcNow.Year}");

            return text.ToString();
        }

        private static string Escape(string value) => WebUtility.HtmlEncode(value);
    }
}
