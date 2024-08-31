namespace Client.Extention
{
    public static class StringExtention
    {
        public static bool IsNullOrEmpty(this string value) => value == default || value.Length == 0;
    }
}