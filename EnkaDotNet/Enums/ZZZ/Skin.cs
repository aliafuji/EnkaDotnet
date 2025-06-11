namespace EnkaDotNet.Enums.ZZZ
{
    public class Skin
    {
        private string _image;
        private string _circleIcon;

        public string Image
        {
            get => _image;
            set => _image = value?.Trim();
        }

        public string CircleIcon
        {
            get => _circleIcon;
            set => _circleIcon = value?.Trim();
        }
    }
}
