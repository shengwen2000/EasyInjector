
namespace EasyApiProxys
{
    public class DefaultApiResult
    {
        public string Result { get; set; }

        public string Message { get; set; }

        protected object _data;

        public object Data {
            get { return _data; }
            set { _data = value; }
        }
    }

    public class DefaultApiResult<T> : DefaultApiResult
    {
        new public T Data
        {
            get { return (T)_data; }
            set { _data = value; }
        }
    }
}
