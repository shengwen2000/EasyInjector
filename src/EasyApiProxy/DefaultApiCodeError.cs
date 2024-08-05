
namespace EasyApiProxys
{
    public class DefaultApiCodeError : System.Exception
    {
        public string Code { get; set; }

        public DefaultApiCodeError(string code, string message)
            : base(message)
        {
            Code = code;
        }

        public override string ToString()
        {
            return string.Format("{0}->{1}", Code, Message);
        }
    }
}
