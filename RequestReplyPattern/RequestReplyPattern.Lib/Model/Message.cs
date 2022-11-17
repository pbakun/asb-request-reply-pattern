namespace RequestReplyPattern.Lib.Model
{
    public class Message
    {
        public string Content { get; set; }

        public override string ToString()
        {
            return this.Content.ToString();
        }
    }
}
