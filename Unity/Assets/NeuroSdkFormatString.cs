#nullable enable

namespace NeuroSdk
{
    public sealed class NeuroSdkFormatString
    {
        private readonly string _str;

        private NeuroSdkFormatString(string str)
        {
            _str = str;
        }

        public string Format(params object[] args)
        {
            return string.Format(_str, args);
        }

        public static implicit operator NeuroSdkFormatString(string str)
        {
            return new NeuroSdkFormatString(str);
        }

        public static NeuroSdkFormatString operator +(NeuroSdkFormatString str1, NeuroSdkFormatString str2)
        {
            return new NeuroSdkFormatString(str1._str + str2._str);
        }
    }
}
